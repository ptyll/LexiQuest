namespace LexiQuest.Core.Interfaces.Services;

/// <summary>
/// Calculates XP earned for answering a word puzzle.
/// Formula: Floor((Base + SpeedBonus) * ComboMultiplier) + StreakBonus
/// </summary>
public interface IXpCalculator
{
    /// <summary>
    /// Calculates XP for a correct answer.
    /// </summary>
    /// <param name="timeSpentMs">Time spent answering in milliseconds</param>
    /// <param name="comboCount">Current combo count (consecutive correct answers)</param>
    /// <param name="correctStreak">Total correct answers in current session</param>
    /// <returns>XP breakdown with total and components</returns>
    XpCalculationResult CalculateCorrectAnswer(int timeSpentMs, int comboCount, int correctStreak);

    /// <summary>
    /// Returns 0 XP for wrong answer.
    /// </summary>
    XpCalculationResult CalculateWrongAnswer();
}

/// <summary>
/// Result of XP calculation with breakdown.
/// </summary>
public record XpCalculationResult(
    int TotalXP,
    int BaseXP,
    int SpeedBonus,
    double ComboMultiplier,
    int StreakBonus,
    string BreakdownDescription
);
