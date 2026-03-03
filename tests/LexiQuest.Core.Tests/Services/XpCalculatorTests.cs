using FluentAssertions;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;

namespace LexiQuest.Core.Tests.Services;

public class XpCalculatorTests
{
    private readonly IXpCalculator _calculator = new XpCalculator();

    [Fact]
    public void XpCalculator_CorrectAnswer_Returns10BaseXP()
    {
        // Arrange
        var timeSpentMs = 15000; // 15 seconds - slow, no speed bonus
        var comboCount = 0;
        var correctStreak = 0;

        // Act
        var result = _calculator.CalculateCorrectAnswer(timeSpentMs, comboCount, correctStreak);

        // Assert
        result.BaseXP.Should().Be(10);
        result.TotalXP.Should().Be(10); // No bonuses
    }

    [Fact]
    public void XpCalculator_FastAnswer_Under3s_Returns5SpeedBonus()
    {
        // Arrange
        var timeSpentMs = 2500; // 2.5 seconds
        var comboCount = 0;
        var correctStreak = 0;

        // Act
        var result = _calculator.CalculateCorrectAnswer(timeSpentMs, comboCount, correctStreak);

        // Assert
        result.SpeedBonus.Should().Be(5);
        result.BaseXP.Should().Be(10);
        result.TotalXP.Should().Be(15); // 10 + 5 = 15
    }

    [Fact]
    public void XpCalculator_FastAnswer_Under5s_Returns3SpeedBonus()
    {
        // Arrange
        var timeSpentMs = 4500; // 4.5 seconds
        var comboCount = 0;
        var correctStreak = 0;

        // Act
        var result = _calculator.CalculateCorrectAnswer(timeSpentMs, comboCount, correctStreak);

        // Assert
        result.SpeedBonus.Should().Be(3);
        result.BaseXP.Should().Be(10);
        result.TotalXP.Should().Be(13); // 10 + 3 = 13
    }

    [Fact]
    public void XpCalculator_FastAnswer_Under10s_Returns1SpeedBonus()
    {
        // Arrange
        var timeSpentMs = 8500; // 8.5 seconds
        var comboCount = 0;
        var correctStreak = 0;

        // Act
        var result = _calculator.CalculateCorrectAnswer(timeSpentMs, comboCount, correctStreak);

        // Assert
        result.SpeedBonus.Should().Be(1);
        result.BaseXP.Should().Be(10);
        result.TotalXP.Should().Be(11); // 10 + 1 = 11
    }

    [Fact]
    public void XpCalculator_SlowAnswer_NoSpeedBonus()
    {
        // Arrange
        var timeSpentMs = 15000; // 15 seconds - slow
        var comboCount = 0;
        var correctStreak = 0;

        // Act
        var result = _calculator.CalculateCorrectAnswer(timeSpentMs, comboCount, correctStreak);

        // Assert
        result.SpeedBonus.Should().Be(0);
        result.TotalXP.Should().Be(10);
    }

    [Fact]
    public void XpCalculator_Combo3Plus_Returns1Point2Multiplier()
    {
        // Arrange - combo only, no streak bonus (streak < 5)
        var timeSpentMs = 15000;
        var comboCount = 3; // Combo of 3
        var correctStreak = 3; // Less than 5, no streak bonus

        // Act
        var result = _calculator.CalculateCorrectAnswer(timeSpentMs, comboCount, correctStreak);

        // Assert
        result.ComboMultiplier.Should().Be(1.2);
        result.StreakBonus.Should().Be(0); // No streak bonus
        // Floor((10 + 0) * 1.2) + 0 = Floor(12) = 12
        result.TotalXP.Should().Be(12);
    }

    [Fact]
    public void XpCalculator_Combo5Plus_Returns1Point5Multiplier()
    {
        // Arrange - combo only, no streak bonus (streak < 5)
        var timeSpentMs = 15000;
        var comboCount = 5; // Combo of 5
        var correctStreak = 0; // No streak to isolate combo test

        // Act
        var result = _calculator.CalculateCorrectAnswer(timeSpentMs, comboCount, correctStreak);

        // Assert
        result.ComboMultiplier.Should().Be(1.5);
        result.StreakBonus.Should().Be(0); // No streak bonus
        // Floor((10 + 0) * 1.5) + 0 = Floor(15) = 15
        result.TotalXP.Should().Be(15);
    }

    [Fact]
    public void XpCalculator_Combo10Plus_Returns2xMultiplier()
    {
        // Arrange - combo only, no streak bonus (streak < 5)
        var timeSpentMs = 15000;
        var comboCount = 10; // Combo of 10
        var correctStreak = 0; // No streak to isolate combo test

        // Act
        var result = _calculator.CalculateCorrectAnswer(timeSpentMs, comboCount, correctStreak);

        // Assert
        result.ComboMultiplier.Should().Be(2.0);
        result.StreakBonus.Should().Be(0); // No streak bonus
        // Floor((10 + 0) * 2.0) + 0 = Floor(20) = 20
        result.TotalXP.Should().Be(20);
    }

    [Fact]
    public void XpCalculator_WrongAnswer_Returns0XP()
    {
        // Act
        var result = _calculator.CalculateWrongAnswer();

        // Assert
        result.TotalXP.Should().Be(0);
        result.BaseXP.Should().Be(0);
        result.SpeedBonus.Should().Be(0);
        result.ComboMultiplier.Should().Be(1.0);
        result.StreakBonus.Should().Be(0);
    }

    [Fact]
    public void XpCalculator_StreakBonus_5PlusCorrect_Returns2ExtraXP()
    {
        // Arrange
        var timeSpentMs = 15000;
        var comboCount = 0;
        var correctStreak = 5; // 5+ correct answers streak

        // Act
        var result = _calculator.CalculateCorrectAnswer(timeSpentMs, comboCount, correctStreak);

        // Assert
        result.StreakBonus.Should().Be(2);
        result.TotalXP.Should().Be(12); // 10 + 0 + 2 = 12
    }

    [Fact]
    public void XpCalculator_FullCalculation_WithAllBonuses()
    {
        // Arrange - Fast answer, high combo, streak bonus
        var timeSpentMs = 2500; // Fast: +5
        var comboCount = 10;    // 2x multiplier
        var correctStreak = 8;  // Streak bonus: +2

        // Act
        var result = _calculator.CalculateCorrectAnswer(timeSpentMs, comboCount, correctStreak);

        // Assert
        // Floor((10 + 5) * 2.0) + 2 = Floor(30) + 2 = 32
        result.BaseXP.Should().Be(10);
        result.SpeedBonus.Should().Be(5);
        result.ComboMultiplier.Should().Be(2.0);
        result.StreakBonus.Should().Be(2);
        result.TotalXP.Should().Be(32);
    }

    [Theory]
    [InlineData(0, 0)]    // No streak
    [InlineData(1, 0)]    // 1 correct
    [InlineData(2, 0)]    // 2 correct
    [InlineData(3, 0)]    // 3 correct
    [InlineData(4, 0)]    // 4 correct
    [InlineData(5, 2)]    // 5 correct - streak bonus kicks in
    [InlineData(6, 2)]    // 6 correct
    [InlineData(10, 2)]   // 10 correct
    public void XpCalculator_StreakBonus_VariousStreaks(int streak, int expectedBonus)
    {
        // Act
        var result = _calculator.CalculateCorrectAnswer(15000, 0, streak);

        // Assert
        result.StreakBonus.Should().Be(expectedBonus);
    }
}
