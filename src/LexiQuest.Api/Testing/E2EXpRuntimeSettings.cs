using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;

namespace LexiQuest.Api.Testing;

public sealed class E2EXpRuntimeSettings
{
    private int _fixedCorrectAnswerXp;

    public int? FixedCorrectAnswerXp
    {
        get
        {
            var value = Volatile.Read(ref _fixedCorrectAnswerXp);
            return value > 0 ? value : null;
        }
    }

    public void SetFixedCorrectAnswerXp(int amount)
    {
        Volatile.Write(ref _fixedCorrectAnswerXp, Math.Clamp(amount, 1, 10_000));
    }

    public void Reset()
    {
        Volatile.Write(ref _fixedCorrectAnswerXp, 0);
    }
}

public sealed class E2EXpCalculator : IXpCalculator
{
    private readonly E2EXpRuntimeSettings _settings;
    private readonly XpCalculator _defaultCalculator = new();

    public E2EXpCalculator(E2EXpRuntimeSettings settings)
    {
        _settings = settings;
    }

    public XpCalculationResult CalculateCorrectAnswer(int timeSpentMs, int comboCount, int correctStreak)
    {
        var fixedXp = _settings.FixedCorrectAnswerXp;
        if (fixedXp.HasValue)
        {
            return new XpCalculationResult(
                TotalXP: fixedXp.Value,
                BaseXP: fixedXp.Value,
                SpeedBonus: 0,
                ComboMultiplier: 1,
                StreakBonus: 0,
                BreakdownDescription: $"E2E fixed correct answer XP: {fixedXp.Value}");
        }

        return _defaultCalculator.CalculateCorrectAnswer(timeSpentMs, comboCount, correctStreak);
    }

    public XpCalculationResult CalculateWrongAnswer() => _defaultCalculator.CalculateWrongAnswer();
}

public sealed record E2EFixedCorrectAnswerXpRequest(int Amount);
