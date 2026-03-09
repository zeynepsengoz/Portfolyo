using Microsoft.EntityFrameworkCore;
using Portfolyo.Data;
using PortfolyoDbContext;
using DotNetEnv;
using Portfolyo.Options;
using Portfolyo.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Text;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

if (File.Exists(".env"))
{
    Env.Load();
}

SetConfigIfNotEmpty(builder.Configuration, "AdminAuth:Key", "ADMIN_KEY");
SetConfigIfNotEmpty(builder.Configuration, "Jwt:Issuer", "JWT_ISSUER");
SetConfigIfNotEmpty(builder.Configuration, "Jwt:Audience", "JWT_AUDIENCE");
SetConfigIfNotEmpty(builder.Configuration, "Jwt:Secret", "JWT_SECRET");
SetConfigIfNotEmpty(builder.Configuration, "Jwt:ExpiresMinutes", "JWT_EXPIRES_MINUTES");
SetConfigIfNotEmpty(builder.Configuration, "Cloudinary:CloudName", "CLOUDINARY_CLOUD_NAME");
SetConfigIfNotEmpty(builder.Configuration, "Cloudinary:ApiKey", "CLOUDINARY_API_KEY");
SetConfigIfNotEmpty(builder.Configuration, "Cloudinary:ApiSecret", "CLOUDINARY_API_SECRET");

var useSqlServer = string.Equals(
    Environment.GetEnvironmentVariable("USE_SQLSERVER"),
    "true",
    StringComparison.OrdinalIgnoreCase);

var connectionString = useSqlServer
    ? ResolveSqlServerConnectionString(builder.Configuration)
    : ResolvePostgresConnectionString(builder.Configuration);

var legacySqlServerConnection = Environment.GetEnvironmentVariable("LEGACY_SQLSERVER_CONNECTION");
var shouldMigrateData =
    string.Equals(Environment.GetEnvironmentVariable("MIGRATE_LOCAL_TO_POSTGRES"), "true", StringComparison.OrdinalIgnoreCase);

builder.Services.AddControllersWithViews();

builder.Services.Configure<AdminAuthOptions>(
    builder.Configuration.GetSection("AdminAuth"));

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<CloudinaryOptions>(
    builder.Configuration.GetSection("Cloudinary"));

builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<IProjectImageStorageService, ProjectImageStorageService>();

var dataProtectionKeysPath = Environment.GetEnvironmentVariable("DATA_PROTECTION_KEYS_PATH");
var dataProtection = builder.Services
    .AddDataProtection()
    .SetApplicationName("Portfolyo");

if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    Directory.CreateDirectory(dataProtectionKeysPath);
    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
}

builder.Services.AddDbContext<portfolyodbContext>(options =>
{
    if (useSqlServer)
        options.UseSqlServer(connectionString);
    else
        options.UseNpgsql(connectionString);
});

builder.Services.AddDbContext<AdminAuthDbContext>(options =>
{
    if (useSqlServer)
        options.UseSqlServer(connectionString);
    else
        options.UseNpgsql(connectionString);
});

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwt.Secret))
{
    throw new InvalidOperationException("JWT secret is missing. Set JWT_SECRET environment variable.");
}
if (string.IsNullOrWhiteSpace(jwt.Issuer) || string.IsNullOrWhiteSpace(jwt.Audience))
{
    throw new InvalidOperationException("JWT issuer or audience is missing. Set JWT_ISSUER and JWT_AUDIENCE.");
}

var keyBytes = Encoding.UTF8.GetBytes(jwt.Secret);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,

            ValidateAudience = true,
            ValidAudience = jwt.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue("admin_token", out var token))
                    context.Token = token;

                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.Redirect("/admin/auth/login");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "role" && c.Value == "admin")
        ));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("StartupDatabase");
    var portfolioDb = scope.ServiceProvider.GetRequiredService<portfolyodbContext>();
    var adminAuthDb = scope.ServiceProvider.GetRequiredService<AdminAuthDbContext>();

    if (useSqlServer)
    {
        logger.LogInformation("USE_SQLSERVER=true; application is running with local SQL Server.");
        await EnsurePortfolioSchemaAsync(portfolioDb);
    }
    else
    {
        await portfolioDb.Database.EnsureCreatedAsync();
        await EnsureAdminAuthSchemaAsync(adminAuthDb);
        await EnsurePortfolioSchemaAsync(portfolioDb);

        if (shouldMigrateData)
        {
            await LegacyDataMigrationService.MigrateFromSqlServerAsync(
                legacySqlServerConnection,
                portfolioDb,
                adminAuthDb,
                logger);
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Default}/{action=Index}/{id?}");

app.Run();

static void SetConfigIfNotEmpty(ConfigurationManager config, string key, string envName)
{
    var value = Environment.GetEnvironmentVariable(envName);
    if (!string.IsNullOrWhiteSpace(value))
    {
        config[key] = value;
    }
}

static string ResolvePostgresConnectionString(ConfigurationManager config)
{
    var raw =
        Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? Environment.GetEnvironmentVariable("DefaultConnection")
        ?? config.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(raw))
    {
        throw new InvalidOperationException("PostgreSQL connection string not found. Set DATABASE_URL or DefaultConnection.");
    }

    if (!raw.Contains("://", StringComparison.Ordinal))
    {
        return raw;
    }

    var uri = new Uri(raw);
    if (!uri.Scheme.Equals("postgres", StringComparison.OrdinalIgnoreCase)
        && !uri.Scheme.Equals("postgresql", StringComparison.OrdinalIgnoreCase))
    {
        return raw;
    }

    var userInfo = uri.UserInfo.Split(':', 2);
    var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty;
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;

    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Username = username,
        Password = password,
        Database = uri.AbsolutePath.TrimStart('/'),
        SslMode = SslMode.Require,
        TrustServerCertificate = true
    };

    return builder.ConnectionString;
}

static string ResolveSqlServerConnectionString(ConfigurationManager config)
{
    var raw =
        Environment.GetEnvironmentVariable("SQLSERVER_CONNECTION")
        ?? Environment.GetEnvironmentVariable("LEGACY_SQLSERVER_CONNECTION")
        ?? config.GetConnectionString("SqlServerConnection")
        ?? config.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(raw))
    {
        throw new InvalidOperationException("SQL Server connection string not found. Set SQLSERVER_CONNECTION or LEGACY_SQLSERVER_CONNECTION.");
    }

    return raw;
}

static async Task EnsureAdminAuthSchemaAsync(AdminAuthDbContext adminAuthDb)
{
    await adminAuthDb.Database.ExecuteSqlRawAsync(
        @"CREATE TABLE IF NOT EXISTS ""AdminUsers"" (
            ""Id"" integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
            ""Username"" character varying(100) NOT NULL,
            ""PasswordHash"" text NOT NULL,
            ""CreatedAtUtc"" timestamp with time zone NOT NULL
        );");

    await adminAuthDb.Database.ExecuteSqlRawAsync(
        @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_AdminUsers_Username""
        ON ""AdminUsers"" (""Username"");");
}

static async Task EnsurePortfolioSchemaAsync(portfolyodbContext portfolioDb)
{
    var providerName = portfolioDb.Database.ProviderName ?? string.Empty;
    var isPostgres = providerName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase);

    if (isPostgres)
    {
        await portfolioDb.Database.ExecuteSqlRawAsync(
            @"ALTER TABLE ""ProjectsTable""
              ADD COLUMN IF NOT EXISTS ""DisplayOrder"" integer NOT NULL DEFAULT 0;");

        await portfolioDb.Database.ExecuteSqlRawAsync(
            @"CREATE TABLE IF NOT EXISTS ""ProjectImages"" (
                  ""ProjectImageId"" integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                  ""ProjectId"" integer NOT NULL,
                  ""ImagePath"" character varying(500) NOT NULL,
                  ""SortOrder"" integer NOT NULL DEFAULT 0,
                  CONSTRAINT ""FK_ProjectImages_ProjectsTable_ProjectId""
                      FOREIGN KEY (""ProjectId"")
                      REFERENCES ""ProjectsTable"" (""ProjectID"")
                      ON DELETE CASCADE
              );");

        await portfolioDb.Database.ExecuteSqlRawAsync(
            @"CREATE INDEX IF NOT EXISTS ""IX_ProjectImages_ProjectId""
              ON ""ProjectImages"" (""ProjectId"");");

        return;
    }

    await portfolioDb.Database.ExecuteSqlRawAsync(
        @"IF COL_LENGTH('ProjectsTable', 'DisplayOrder') IS NULL
          BEGIN
              ALTER TABLE [ProjectsTable]
              ADD [DisplayOrder] int NOT NULL
              CONSTRAINT [DF_ProjectsTable_DisplayOrder] DEFAULT(0);
          END");

    await portfolioDb.Database.ExecuteSqlRawAsync(
        @"IF OBJECT_ID('ProjectImages', 'U') IS NULL
          BEGIN
              CREATE TABLE [ProjectImages](
                  [ProjectImageId] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                  [ProjectId] int NOT NULL,
                  [ImagePath] nvarchar(500) NOT NULL,
                  [SortOrder] int NOT NULL CONSTRAINT [DF_ProjectImages_SortOrder] DEFAULT(0),
                  CONSTRAINT [FK_ProjectImages_ProjectsTable_ProjectId]
                      FOREIGN KEY ([ProjectId]) REFERENCES [ProjectsTable]([ProjectID]) ON DELETE CASCADE
              );
              CREATE INDEX [IX_ProjectImages_ProjectId] ON [ProjectImages]([ProjectId]);
          END");
}
