using Microsoft.EntityFrameworkCore;
using Portfolyo.Data;
using PortfolyoDbContext;
using DotNetEnv;
using Portfolyo.Options;
using Portfolyo.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Configuration["AdminAuth:Key"] =
    Environment.GetEnvironmentVariable("ADMIN_KEY");

builder.Configuration["Jwt:Issuer"] =
    Environment.GetEnvironmentVariable("JWT_ISSUER");

builder.Configuration["Jwt:Audience"] =
    Environment.GetEnvironmentVariable("JWT_AUDIENCE");

builder.Configuration["Jwt:Secret"] =
    Environment.GetEnvironmentVariable("JWT_SECRET");

builder.Configuration["Jwt:ExpiresMinutes"] =
    Environment.GetEnvironmentVariable("JWT_EXPIRES_MINUTES");

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.Configure<AdminAuthOptions>(
    builder.Configuration.GetSection("AdminAuth"));

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.AddScoped<JwtTokenService>();





builder.Services.AddDbContext<portfolyodbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Her program açýldýðýnda DbContext sýnýfýný kullanrak veri tabanýna baðlanýr

builder.Services.AddDbContext<AdminAuthDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
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









// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

//Proram cs --> projemizin ilk açýldýðýnda çalýþacak kodlar
//Projem ilk açýldýðýnda veri tabanýna baðlanmalý
//Bunun için veri tabanýmý temsil eden sýnýf(DbContext) tanýmlamam lazým   