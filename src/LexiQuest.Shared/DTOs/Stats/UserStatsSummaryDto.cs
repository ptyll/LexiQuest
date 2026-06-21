using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.DTOs.Streak;

namespace LexiQuest.Shared.DTOs.Stats;

public record UserStatsSummaryDto
{
    public int TotalXP { get; init; }
    public int CurrentLevel { get; init; }
    public int CurrentStreak { get; init; }
    public int LongestStreak { get; init; }
    public double Accuracy { get; init; }
    public string AverageTime { get; init; } = string.Empty;
    public int TotalWordsSolved { get; init; }
    public XPProgress? XpProgress { get; init; }
    public StreakStatus? StreakStatus { get; init; }
    public StreakProtectionDto? StreakProtection { get; init; }
    public bool IsPremium { get; init; }
}
