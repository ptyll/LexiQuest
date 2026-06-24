using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        LexiQuestDbContext dbContext,
        TimeProvider? timeProvider = null,
        CancellationToken cancellationToken = default)
    {
        if (!await dbContext.Words.AnyAsync(cancellationToken))
        {
            dbContext.Words.AddRange(SeedData.GetWords());
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (!await dbContext.LearningPaths.AnyAsync(cancellationToken))
        {
            dbContext.LearningPaths.AddRange(SeedData.GetLearningPaths());
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (!await dbContext.Achievements.AnyAsync(cancellationToken))
        {
            dbContext.Achievements.AddRange(SeedData.GetAchievements());
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (!await dbContext.ShopItems.AnyAsync(cancellationToken))
        {
            dbContext.ShopItems.AddRange(SeedData.GetShopItems());
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await SeedDailyChallengeAsync(dbContext, timeProvider ?? TimeProvider.System, cancellationToken);
        await SeedActiveLeaguesAsync(dbContext, timeProvider ?? TimeProvider.System, cancellationToken);
    }

    private static async Task SeedDailyChallengeAsync(
        LexiQuestDbContext dbContext,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var today = timeProvider.GetUtcNow().UtcDateTime.Date;
        if (await dbContext.DailyChallenges.AnyAsync(challenge => challenge.Date == today, cancellationToken))
        {
            return;
        }

        var word = await dbContext.Words
            .AsNoTracking()
            .Where(item => item.Difficulty == DifficultyLevel.Beginner)
            .OrderBy(item => item.Original)
            .FirstAsync(cancellationToken);

        dbContext.DailyChallenges.Add(DailyChallenge.Create(
            today,
            word.Id,
            GetDailyModifierForDay(today.DayOfWeek)));

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedActiveLeaguesAsync(
        LexiQuestDbContext dbContext,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var weekStart = GetCurrentUtcWeekStart(timeProvider.GetUtcNow().UtcDateTime.Date);
        var weekEnd = weekStart.AddDays(7);

        foreach (var tier in Enum.GetValues<LeagueTier>())
        {
            var exists = await dbContext.Leagues.AnyAsync(
                league => league.Tier == tier
                    && league.IsActive
                    && league.WeekStart == weekStart,
                cancellationToken);

            if (!exists)
            {
                dbContext.Leagues.Add(League.Create(tier, weekStart, weekEnd));
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static DailyModifier GetDailyModifierForDay(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => DailyModifier.Category,
        DayOfWeek.Tuesday => DailyModifier.Speed,
        DayOfWeek.Wednesday => DailyModifier.NoHints,
        DayOfWeek.Thursday => DailyModifier.DoubleLetters,
        DayOfWeek.Friday => DailyModifier.Team,
        DayOfWeek.Saturday => DailyModifier.Hard,
        DayOfWeek.Sunday => DailyModifier.Easy,
        _ => DailyModifier.Easy
    };

    private static DateTime GetCurrentUtcWeekStart(DateTime today)
    {
        var daysSinceMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return today.AddDays(-daysSinceMonday);
    }
}
