namespace LexiQuest.Shared.DTOs.Game;

/// <summary>
/// Represents user's streak status.
/// </summary>
public record StreakStatus(
    int CurrentDays,
    int LongestDays,
    string FireLevel,
    DateTime? NextResetAt,
    TimeSpan? TimeRemaining,
    bool IsAtRisk
);
