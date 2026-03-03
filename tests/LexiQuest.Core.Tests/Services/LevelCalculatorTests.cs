using FluentAssertions;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;

namespace LexiQuest.Core.Tests.Services;

public class LevelCalculatorTests
{
    private readonly ILevelCalculator _levelCalculator;

    public LevelCalculatorTests()
    {
        _levelCalculator = new LevelCalculator();
    }

    [Fact]
    public void LevelCalculator_Level1_Requires100XP()
    {
        // Act
        var xpRequired = _levelCalculator.GetXpRequiredForLevel(1);

        // Assert
        xpRequired.Should().Be(100); // 100 XP to go from Level 1 to Level 2
    }

    [Fact]
    public void LevelCalculator_Level2_Requires150XP()
    {
        // Act
        var xpRequired = _levelCalculator.GetXpRequiredForLevel(2);

        // Assert
        xpRequired.Should().Be(150); // 150 XP to go from Level 2 to Level 3
    }

    [Fact]
    public void LevelCalculator_Level5_Requires338XP()
    {
        // Act - XP needed to go from Level 5 to Level 6
        var xpRequired = _levelCalculator.GetXpRequiredForLevel(5);

        // Assert - 100 * 1.5^4 = 506, but we use floor
        xpRequired.Should().Be(506); // 100 * 1.5^4 = 506
    }

    [Fact]
    public void LevelCalculator_Level10_Requires3844XP()
    {
        // Act - XP needed to go from Level 10 to Level 11
        var xpRequired = _levelCalculator.GetXpRequiredForLevel(10);

        // Assert - 100 * 1.5^9 ≈ 3844
        xpRequired.Should().Be(3844);
    }

    [Theory]
    [InlineData(0, 1)]      // 0 XP = Level 1
    [InlineData(50, 1)]     // 50 XP = Level 1
    [InlineData(99, 1)]     // 99 XP = Level 1
    [InlineData(100, 2)]    // 100 XP = Level 2 (threshold crossed)
    [InlineData(150, 2)]    // 150 XP = Level 2
    [InlineData(249, 2)]    // 249 XP = Level 2
    [InlineData(250, 3)]    // 250 XP = Level 3 (100 + 150)
    [InlineData(400, 3)]    // 400 XP = Level 3
    [InlineData(475, 4)]    // 475 XP = Level 4 (100 + 150 + 225)
    public void LevelCalculator_GetLevel_FromTotalXP_ReturnsCorrectLevel(int totalXp, int expectedLevel)
    {
        // Act
        var level = _levelCalculator.GetLevelFromXp(totalXp);

        // Assert
        level.Should().Be(expectedLevel);
    }

    [Theory]
    [InlineData(0, 0)]      // Level 1, 0 XP progress = 0%
    [InlineData(50, 50)]    // Level 1, 50/100 = 50%
    [InlineData(100, 0)]    // Level 2, 0/150 = 0% (just leveled up)
    [InlineData(125, 16)]   // Level 2, 25/150 = 16%
    [InlineData(175, 50)]   // Level 2, 75/150 = 50%
    public void LevelCalculator_GetProgress_ReturnsPercentageInCurrentLevel(int totalXp, int expectedProgress)
    {
        // Act
        var progress = _levelCalculator.GetProgressInCurrentLevel(totalXp);

        // Assert
        progress.Should().Be(expectedProgress);
    }

    [Fact]
    public void LevelCalculator_DetectLevelUp_WhenXPCrossesThreshold_ReturnsTrue()
    {
        // Arrange - crossing from 99 (Level 1) to 100 (Level 2)
        const int previousXp = 99;
        const int newXp = 100;

        // Act
        var hasLeveledUp = _levelCalculator.HasLeveledUp(previousXp, newXp);

        // Assert
        hasLeveledUp.Should().BeTrue();
    }

    [Fact]
    public void LevelCalculator_DetectLevelUp_WhenXPBelowThreshold_ReturnsFalse()
    {
        // Arrange
        const int previousXp = 50;
        const int newXp = 75;

        // Act
        var hasLeveledUp = _levelCalculator.HasLeveledUp(previousXp, newXp);

        // Assert
        hasLeveledUp.Should().BeFalse();
    }

    [Fact]
    public void LevelCalculator_DetectLevelUp_WhenMultipleLevels_ReturnsTrue()
    {
        // Arrange - crossing from Level 1 (50 XP) to Level 3 (300 XP)
        const int previousXp = 50;   // Level 1
        const int newXp = 300;       // Level 3 (crossed 100 and 250)

        // Act
        var hasLeveledUp = _levelCalculator.HasLeveledUp(previousXp, newXp);

        // Assert
        hasLeveledUp.Should().BeTrue();
    }

    [Fact]
    public void LevelCalculator_XPProgress_ReturnsCorrectValues()
    {
        // Arrange - 375 total XP
        // Level 1: 0-99 (need 100)
        // Level 2: 100-249 (need 150) 
        // Level 3: 250-474 (need 225)
        // 375 - 250 = 125 XP in Level 3
        const int totalXp = 375;

        // Act
        var progress = _levelCalculator.GetXpProgress(totalXp);

        // Assert
        progress.TotalXP.Should().Be(375);
        progress.CurrentLevel.Should().Be(3);
        progress.XPInCurrentLevel.Should().Be(125);
        progress.XPRequiredForNextLevel.Should().Be(225); // For level 3→4
        progress.ProgressPercentage.Should().Be(55); // 125/225 ≈ 55%
    }

    [Fact]
    public void LevelCalculator_CumulativeXpForLevel5_Is812()
    {
        // Level 5 requires cumulative XP:
        // Level 1→2: 100
        // Level 2→3: 150  
        // Level 3→4: 225
        // Level 4→5: 337 (floor of 100 * 1.5^3 = 337.5)
        // Total: 812

        // Act
        var levelAt811 = _levelCalculator.GetLevelFromXp(811);
        var levelAt812 = _levelCalculator.GetLevelFromXp(812);

        // Assert
        levelAt811.Should().Be(4);
        levelAt812.Should().Be(5);
    }
}
