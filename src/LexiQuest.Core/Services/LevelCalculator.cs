using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Game;

namespace LexiQuest.Core.Services;

/// <summary>
/// Calculates level progression using exponential curve.
/// Formula: XP_needed_for_next_level = 100 * 1.5^(current_level-1)
/// </summary>
public class LevelCalculator : ILevelCalculator
{
    private const int BaseXp = 100;
    private const double GrowthRate = 1.5;

    /// <summary>
    /// Gets the XP required to advance FROM the given level TO the next level.
    /// Level 1 requires 100 XP to reach level 2.
    /// Level 2 requires 150 XP to reach level 3.
    /// </summary>
    public int GetXpRequiredForLevel(int level)
    {
        if (level < 1)
            return 0;

        // XP needed to advance from this level to the next
        // Formula: 100 * 1.5^(level-1)
        return (int)Math.Floor(BaseXp * Math.Pow(GrowthRate, level - 1));
    }

    /// <summary>
    /// Gets the cumulative XP required to REACH the given level.
    /// </summary>
    public int GetCumulativeXpForLevel(int targetLevel)
    {
        if (targetLevel <= 1)
            return 0;

        int cumulativeXp = 0;
        for (int level = 1; level < targetLevel; level++)
        {
            cumulativeXp += GetXpRequiredForLevel(level);
        }
        return cumulativeXp;
    }

    public int GetLevelFromXp(int totalXp)
    {
        if (totalXp < BaseXp)
            return 1;

        int level = 1;
        while (true)
        {
            int cumulativeXpForNextLevel = GetCumulativeXpForLevel(level + 1);
            
            if (totalXp < cumulativeXpForNextLevel)
                break;

            level++;
        }

        return level;
    }

    public int GetProgressInCurrentLevel(int totalXp)
    {
        var currentLevel = GetLevelFromXp(totalXp);
        int xpForPreviousLevels = GetCumulativeXpForLevel(currentLevel);
        int xpInCurrentLevel = totalXp - xpForPreviousLevels;
        int xpRequiredForNextLevel = GetXpRequiredForLevel(currentLevel);

        if (xpRequiredForNextLevel == 0)
            return 0;

        return Math.Min(100, (int)((double)xpInCurrentLevel / xpRequiredForNextLevel * 100));
    }

    public XPProgress GetXpProgress(int totalXp)
    {
        var currentLevel = GetLevelFromXp(totalXp);
        int xpForPreviousLevels = GetCumulativeXpForLevel(currentLevel);
        int xpInCurrentLevel = totalXp - xpForPreviousLevels;
        int xpRequiredForNextLevel = GetXpRequiredForLevel(currentLevel);

        int progressPercentage = xpRequiredForNextLevel > 0
            ? (int)((double)xpInCurrentLevel / xpRequiredForNextLevel * 100)
            : 0;

        return new XPProgress(
            TotalXP: totalXp,
            CurrentLevel: currentLevel,
            XPInCurrentLevel: xpInCurrentLevel,
            XPRequiredForNextLevel: xpRequiredForNextLevel,
            ProgressPercentage: Math.Min(100, progressPercentage)
        );
    }

    public bool HasLeveledUp(int previousXp, int newXp)
    {
        var previousLevel = GetLevelFromXp(previousXp);
        var newLevel = GetLevelFromXp(newXp);

        return newLevel > previousLevel;
    }
}
