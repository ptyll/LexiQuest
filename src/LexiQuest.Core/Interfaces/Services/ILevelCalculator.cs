using LexiQuest.Shared.DTOs.Game;

namespace LexiQuest.Core.Interfaces.Services;

/// <summary>
/// Service for calculating level progression based on XP.
/// Formula: XP_needed = 100 * 1.5^(level-1)
/// </summary>
public interface ILevelCalculator
{
    /// <summary>
    /// Gets the XP required to reach a specific level.
    /// </summary>
    int GetXpRequiredForLevel(int level);

    /// <summary>
    /// Gets the current level based on total XP.
    /// </summary>
    int GetLevelFromXp(int totalXp);

    /// <summary>
    /// Gets the progress percentage (0-100) in the current level.
    /// </summary>
    int GetProgressInCurrentLevel(int totalXp);

    /// <summary>
    /// Gets detailed XP progress information.
    /// </summary>
    XPProgress GetXpProgress(int totalXp);

    /// <summary>
    /// Determines if the user has leveled up between two XP values.
    /// </summary>
    bool HasLeveledUp(int previousXp, int newXp);
}
