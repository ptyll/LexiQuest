using FluentAssertions;
using LexiQuest.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Tests.Persistence;

public class DatabaseSeederTests
{
    [Fact]
    public async Task SeedAsync_EmptyDatabase_SeedsCoreCatalogData()
    {
        await using var dbContext = CreateDbContext();

        await DatabaseSeeder.SeedAsync(dbContext);

        dbContext.Words.Should().NotBeEmpty();
        dbContext.LearningPaths.Should().HaveCount(4);
        dbContext.PathLevels.Should().NotBeEmpty();
        dbContext.Achievements.Should().NotBeEmpty();
        dbContext.ShopItems.Should().NotBeEmpty();
        dbContext.DailyChallenges.Should().ContainSingle();
        dbContext.Leagues.Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public async Task SeedAsync_AlreadySeededDatabase_DoesNotDuplicateCoreCatalogData()
    {
        await using var dbContext = CreateDbContext();
        await DatabaseSeeder.SeedAsync(dbContext);

        var wordCount = await dbContext.Words.CountAsync();
        var pathCount = await dbContext.LearningPaths.CountAsync();
        var achievementCount = await dbContext.Achievements.CountAsync();
        var shopItemCount = await dbContext.ShopItems.CountAsync();
        var dailyChallengeCount = await dbContext.DailyChallenges.CountAsync();
        var leagueCount = await dbContext.Leagues.CountAsync();

        await DatabaseSeeder.SeedAsync(dbContext);

        (await dbContext.Words.CountAsync()).Should().Be(wordCount);
        (await dbContext.LearningPaths.CountAsync()).Should().Be(pathCount);
        (await dbContext.Achievements.CountAsync()).Should().Be(achievementCount);
        (await dbContext.ShopItems.CountAsync()).Should().Be(shopItemCount);
        (await dbContext.DailyChallenges.CountAsync()).Should().Be(dailyChallengeCount);
        (await dbContext.Leagues.CountAsync()).Should().Be(leagueCount);
    }

    private static LexiQuestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<LexiQuestDbContext>()
            .UseInMemoryDatabase($"DatabaseSeederTests_{Guid.NewGuid()}")
            .Options;

        return new LexiQuestDbContext(options);
    }
}
