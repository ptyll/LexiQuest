using LexiQuest.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.E2E.Tests.Infrastructure;

internal static class E2ETestDataSeeder
{
    public static async Task EnsureDatabaseAsync(string connectionString, CancellationToken cancellationToken)
    {
        var options = new DbContextOptionsBuilder<LexiQuestDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var dbContext = new LexiQuestDbContext(options);

        await dbContext.Database.MigrateAsync(cancellationToken);

        await DatabaseSeeder.SeedAsync(dbContext, cancellationToken: cancellationToken);
    }

    public static async Task SeedAsync(string connectionString, CancellationToken cancellationToken)
    {
        var options = new DbContextOptionsBuilder<LexiQuestDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var dbContext = new LexiQuestDbContext(options);
        await DatabaseSeeder.SeedAsync(dbContext, cancellationToken: cancellationToken);
    }
}
