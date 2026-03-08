using Microsoft.EntityFrameworkCore;
using Portfolyo.Data;
using PortfolyoDbContext;

namespace Portfolyo.Services;

public static class LegacyDataMigrationService
{
    public static async Task MigrateFromSqlServerAsync(
        string? legacySqlServerConnection,
        portfolyodbContext targetPortfolioDb,
        AdminAuthDbContext targetAdminDb,
        ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(legacySqlServerConnection))
        {
            logger.LogWarning("MIGRATE_LOCAL_TO_POSTGRES=true but LEGACY_SQLSERVER_CONNECTION is not set. Skipping data migration.");
            return;
        }

        logger.LogInformation("Starting one-time data migration from SQL Server to PostgreSQL...");

        var sourcePortfolioOptions = new DbContextOptionsBuilder<portfolyodbContext>()
            .UseSqlServer(legacySqlServerConnection)
            .Options;

        var sourceAdminOptions = new DbContextOptionsBuilder<AdminAuthDbContext>()
            .UseSqlServer(legacySqlServerConnection)
            .Options;

        await using var sourcePortfolioDb = new portfolyodbContext(sourcePortfolioOptions);
        await using var sourceAdminDb = new AdminAuthDbContext(sourceAdminOptions);

        await TruncateTargetsAsync(targetPortfolioDb, targetAdminDb, logger);

        await CopyAllAsync(sourcePortfolioDb.CategoryTables, targetPortfolioDb.CategoryTables, "CategoryTable", logger);
        await CopyAllAsync(sourcePortfolioDb.AboutMeTables, targetPortfolioDb.AboutMeTables, "AboutMeTable", logger);
        await targetPortfolioDb.SaveChangesAsync();

        await CopyAboutMe2WithNormalizedKeysAsync(sourcePortfolioDb, targetPortfolioDb, logger);
        await targetPortfolioDb.SaveChangesAsync();

        await CopyAllAsync(sourcePortfolioDb.HomePages, targetPortfolioDb.HomePages, "HomePage", logger);
        await CopyAllAsync(sourcePortfolioDb.ProjectsTables, targetPortfolioDb.ProjectsTables, "ProjectsTable", logger);
        await CopyAllAsync(sourcePortfolioDb.ServicesTables, targetPortfolioDb.ServicesTables, "Services Table", logger);
        await CopyAllAsync(sourcePortfolioDb.SkillTables, targetPortfolioDb.SkillTables, "SkillTable", logger);
        await CopyAllAsync(sourcePortfolioDb.TestimonialTables, targetPortfolioDb.TestimonialTables, "TestimonialTable", logger);
        await CopyAllAsync(sourcePortfolioDb.MessageTables, targetPortfolioDb.MessageTables, "MessagesTable", logger);
        await CopyAllAsync(sourcePortfolioDb.EducationTables, targetPortfolioDb.EducationTables, "Educations", logger);
        await CopyAllAsync(sourcePortfolioDb.AboutInfoTables, targetPortfolioDb.AboutInfoTables, "AboutInfoTable", logger);
        await CopyAllAsync(sourceAdminDb.AdminUsers, targetAdminDb.AdminUsers, "AdminUsers", logger);

        await targetPortfolioDb.SaveChangesAsync();
        await targetAdminDb.SaveChangesAsync();

        await ResetIdentitySequenceAsync(targetPortfolioDb, "CategoryTable", "CategoryID");
        await ResetIdentitySequenceAsync(targetPortfolioDb, "AboutMeTable", "AboutID");
        await ResetIdentitySequenceAsync(targetPortfolioDb, "AboutMe2Table", "DetailID");
        await ResetIdentitySequenceAsync(targetPortfolioDb, "HomePage", "homeID");
        await ResetIdentitySequenceAsync(targetPortfolioDb, "ProjectsTable", "ProjectID");
        await ResetIdentitySequenceAsync(targetPortfolioDb, "Services Table", "ExperinceID");
        await ResetIdentitySequenceAsync(targetPortfolioDb, "SkillTable", "SkilID");
        await ResetIdentitySequenceAsync(targetPortfolioDb, "MessagesTable", "MessageId");
        await ResetIdentitySequenceAsync(targetPortfolioDb, "Educations", "EducationId");
        await ResetIdentitySequenceAsync(targetPortfolioDb, "AboutInfoTable", "AboutInfoId");
        await ResetIdentitySequenceAsync(targetAdminDb, "AdminUsers", "Id");

        logger.LogInformation("Data migration completed.");
    }

    private static async Task CopyAllAsync<TEntity>(
        DbSet<TEntity> source,
        DbSet<TEntity> target,
        string tableName,
        ILogger logger)
        where TEntity : class
    {
        var rows = await source.AsNoTracking().ToListAsync();
        if (rows.Count == 0)
        {
            logger.LogInformation("Skipping {TableName}: source is empty.", tableName);
            return;
        }

        await target.AddRangeAsync(rows);
        logger.LogInformation("Copied {Count} row(s) into {TableName}.", rows.Count, tableName);
    }

    private static async Task CopyAboutMe2WithNormalizedKeysAsync(
        portfolyodbContext sourcePortfolioDb,
        portfolyodbContext targetPortfolioDb,
        ILogger logger)
    {
        var sourceRows = await sourcePortfolioDb.AboutMe2Tables.AsNoTracking().ToListAsync();
        if (sourceRows.Count == 0)
        {
            logger.LogInformation("Skipping AboutMe2Table: source is empty.");
            return;
        }

        var validAboutIds = (await targetPortfolioDb.AboutMeTables
            .Select(x => x.AboutId)
            .ToListAsync())
            .ToHashSet();

        var mappedRows = new List<AboutMe2Table>();
        var usedDetailIds = new HashSet<int>();
        foreach (var row in sourceRows)
        {
            var baseId = row.AboutId ?? row.DetailId;
            if (!validAboutIds.Contains(baseId))
            {
                logger.LogWarning("Skipping AboutMe2 row because parent AboutMeTable record is missing. Source DetailId={DetailId}, AboutId={AboutId}", row.DetailId, row.AboutId);
                continue;
            }

            if (!usedDetailIds.Add(baseId))
            {
                logger.LogWarning("Skipping duplicate AboutMe2 row after key normalization. Source DetailId={DetailId}, AboutId={AboutId}, NormalizedDetailId={NormalizedDetailId}", row.DetailId, row.AboutId, baseId);
                continue;
            }

            mappedRows.Add(new AboutMe2Table
            {
                DetailId = baseId,
                AboutId = baseId,
                DetailType = row.DetailType,
                Title = row.Title,
                Description = row.Description
            });
        }

        if (mappedRows.Count == 0)
        {
            logger.LogWarning("No valid AboutMe2Table rows to copy after key normalization.");
            return;
        }

        await targetPortfolioDb.AboutMe2Tables.AddRangeAsync(mappedRows);
        logger.LogInformation("Copied {Count} row(s) into AboutMe2Table with key normalization.", mappedRows.Count);
    }

    private static async Task TruncateTargetsAsync(
        portfolyodbContext targetPortfolioDb,
        AdminAuthDbContext targetAdminDb,
        ILogger logger)
    {
        logger.LogInformation("Clearing PostgreSQL target tables before copy...");

        // Child tables first, then parent tables.
        await targetPortfolioDb.Database.ExecuteSqlRawAsync(@"TRUNCATE TABLE ""AboutMe2Table"" RESTART IDENTITY CASCADE;");
        await targetPortfolioDb.Database.ExecuteSqlRawAsync(@"TRUNCATE TABLE ""ProjectsTable"" RESTART IDENTITY CASCADE;");
        await targetPortfolioDb.Database.ExecuteSqlRawAsync(@"TRUNCATE TABLE ""MessagesTable"" RESTART IDENTITY CASCADE;");
        await targetPortfolioDb.Database.ExecuteSqlRawAsync(@"TRUNCATE TABLE ""Educations"" RESTART IDENTITY CASCADE;");
        await targetPortfolioDb.Database.ExecuteSqlRawAsync(@"TRUNCATE TABLE ""AboutInfoTable"" RESTART IDENTITY CASCADE;");
        await targetPortfolioDb.Database.ExecuteSqlRawAsync(@"TRUNCATE TABLE ""TestimonialTable"" RESTART IDENTITY CASCADE;");
        await targetPortfolioDb.Database.ExecuteSqlRawAsync(@"TRUNCATE TABLE ""SkillTable"" RESTART IDENTITY CASCADE;");
        await targetPortfolioDb.Database.ExecuteSqlRawAsync(@"TRUNCATE TABLE ""Services Table"" RESTART IDENTITY CASCADE;");
        await targetPortfolioDb.Database.ExecuteSqlRawAsync(@"TRUNCATE TABLE ""HomePage"" RESTART IDENTITY CASCADE;");
        await targetPortfolioDb.Database.ExecuteSqlRawAsync(@"TRUNCATE TABLE ""AboutMeTable"" RESTART IDENTITY CASCADE;");
        await targetPortfolioDb.Database.ExecuteSqlRawAsync(@"TRUNCATE TABLE ""CategoryTable"" RESTART IDENTITY CASCADE;");
        await targetAdminDb.Database.ExecuteSqlRawAsync(@"TRUNCATE TABLE ""AdminUsers"" RESTART IDENTITY CASCADE;");
    }

    private static async Task ResetIdentitySequenceAsync(DbContext dbContext, string tableName, string columnName)
    {
        var sql =
            $"SELECT setval(pg_get_serial_sequence('\"{tableName}\"', '{columnName}'), " +
            $"COALESCE(MAX(\"{columnName}\"), 1), true) FROM \"{tableName}\";";

        try
        {
            await dbContext.Database.ExecuteSqlRawAsync(sql);
        }
        catch
        {
            // Sequence reset is best-effort; some tables may not use identity.
        }
    }
}
