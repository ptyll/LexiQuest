using LexiQuest.Core.Interfaces.Services;

namespace LexiQuest.Core.Services;

/// <summary>
/// Calculates XP earned for answering word puzzles.
/// Formula: Floor((Base + SpeedBonus) * ComboMultiplier) + StreakBonus
/// </summary>
public class XpCalculator : IXpCalculator
{
    // Base XP for correct answer
    private const int BaseXPAward = 10;

    // Speed bonus thresholds (in milliseconds)
    private const int SpeedThreshold3s = 3000;   // Under 3s: +5 XP
    private const int SpeedThreshold5s = 5000;   // Under 5s: +3 XP
    private const int SpeedThreshold10s = 10000; // Under 10s: +1 XP

    // Combo multipliers
    private const double ComboMultiplierBase = 1.0;
    private const double ComboMultiplier3Plus = 1.2;  // 3+ combo
    private const double ComboMultiplier5Plus = 1.5;  // 5+ combo
    private const double ComboMultiplier10Plus = 2.0; // 10+ combo

    // Streak bonus
    private const int StreakBonusThreshold = 5; // 5+ correct answers
    private const int StreakBonusAmount = 2;

    /// <inheritdoc />
    public XpCalculationResult CalculateCorrectAnswer(int timeSpentMs, int comboCount, int correctStreak)
    {
        var speedBonus = CalculateSpeedBonus(timeSpentMs);
        var comboMultiplier = CalculateComboMultiplier(comboCount);
        var streakBonus = CalculateStreakBonus(correctStreak);

        // Formula: Floor((Base + SpeedBonus) * ComboMultiplier) + StreakBonus
        var totalXP = (int)Math.Floor((BaseXPAward + speedBonus) * comboMultiplier) + streakBonus;

        var breakdown = $"Base: {BaseXPAward}, Speed: +{speedBonus}, Combo: x{comboMultiplier:F1}, Streak: +{streakBonus}";

        return new XpCalculationResult(
            TotalXP: totalXP,
            BaseXP: BaseXPAward,
            SpeedBonus: speedBonus,
            ComboMultiplier: comboMultiplier,
            StreakBonus: streakBonus,
            BreakdownDescription: breakdown
        );
    }

    /// <inheritdoc />
    public XpCalculationResult CalculateWrongAnswer()
    {
        return new XpCalculationResult(
            TotalXP: 0,
            BaseXP: 0,
            SpeedBonus: 0,
            ComboMultiplier: ComboMultiplierBase,
            StreakBonus: 0,
            BreakdownDescription: "Wrong answer: 0 XP"
        );
    }

    private static int CalculateSpeedBonus(int timeSpentMs)
    {
        return timeSpentMs switch
        {
            < SpeedThreshold3s => 5,  // Under 3s: +5 XP
            < SpeedThreshold5s => 3,  // Under 5s: +3 XP
            < SpeedThreshold10s => 1, // Under 10s: +1 XP
            _ => 0                    // 10s+: no bonus
        };
    }

    private static double CalculateComboMultiplier(int comboCount)
    {
        return comboCount switch
        {
            >= 10 => ComboMultiplier10Plus, // 10+ combo: 2x
            >= 5 => ComboMultiplier5Plus,   // 5+ combo: 1.5x
            >= 3 => ComboMultiplier3Plus,   // 3+ combo: 1.2x
            _ => ComboMultiplierBase        // No combo: 1x
        };
    }

    private static int CalculateStreakBonus(int correctStreak)
    {
        return correctStreak >= StreakBonusThreshold ? StreakBonusAmount : 0;
    }
}
