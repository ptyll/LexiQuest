namespace LexiQuest.Shared.DTOs.Streak;

public record StreakProtectionDto(
    int ShieldsRemaining,
    bool HasActiveShield,
    bool FreezeUsedThisWeek,
    DateTime? NextShieldAvailableAt,
    bool CanActivateFreeShield
);
