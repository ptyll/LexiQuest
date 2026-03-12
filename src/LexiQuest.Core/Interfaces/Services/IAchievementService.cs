using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.DTOs.Achievements;

namespace LexiQuest.Core.Interfaces.Services;

public interface IAchievementService
{
    Task<List<AchievementUnlockResult>> CheckWordSolvedAsync(Guid userId, int totalWordsSolved, CancellationToken cancellationToken = default);
    Task<List<AchievementUnlockResult>> CheckStreakAsync(Guid userId, int currentStreak, CancellationToken cancellationToken = default);
    Task<List<AchievementUnlockResult>> CheckPathCompletedAsync(Guid userId, Guid pathId, CancellationToken cancellationToken = default);
    Task<List<AchievementUnlockResult>> CheckBossDefeatedAsync(Guid userId, bool perfectRun, CancellationToken cancellationToken = default);
    Task<int> GetProgressAsync(Guid userId, Guid achievementId, CancellationToken cancellationToken = default);
    Task<List<AchievementDto>> GetUserAchievementsAsync(Guid userId, CancellationToken cancellationToken = default);
}

public record AchievementUnlockResult(Guid AchievementId, string AchievementKey, string Name, int XPEarned);
