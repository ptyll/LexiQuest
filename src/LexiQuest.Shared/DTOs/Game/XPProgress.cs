namespace LexiQuest.Shared.DTOs.Game;

/// <summary>
/// Represents XP progress information for a player.
/// </summary>
public record XPProgress(
    int TotalXP,
    int CurrentLevel,
    int XPInCurrentLevel,
    int XPRequiredForNextLevel,
    int ProgressPercentage
);
