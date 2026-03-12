using LexiQuest.Shared.Enums;

namespace LexiQuest.Shared.DTOs.Achievements;

public record AchievementDto(
    Guid Id,
    string Key,
    string Name,
    string Description,
    AchievementCategory Category,
    int XPReward,
    int RequiredValue,
    int CurrentProgress,
    int ProgressPercentage,
    bool IsUnlocked,
    DateTime? UnlockedAt,
    string? IconName
);

public record AchievementProgressDto(
    Guid AchievementId,
    string Name,
    int CurrentProgress,
    int RequiredValue,
    int Percentage
);

public record AchievementUnlockDto(
    Guid AchievementId,
    string Name,
    string Description,
    int XPReward,
    string? IconName
);
