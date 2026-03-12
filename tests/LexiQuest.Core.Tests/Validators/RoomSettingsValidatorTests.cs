using FluentAssertions;
using FluentValidation.TestHelper;
using LexiQuest.Core.Validators;
using LexiQuest.Shared.DTOs.Multiplayer;
using LexiQuest.Shared.Enums;
using Xunit;

namespace LexiQuest.Core.Tests.Validators;

/// <summary>
/// Unit tests for RoomSettingsValidator - T-503.2
/// </summary>
public class RoomSettingsValidatorTests
{
    private readonly RoomSettingsValidator _validator = new();

    [Theory]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    public void RoomSettingsValidator_WordCount_ValidValues_Pass(int wordCount)
    {
        // Arrange
        var settings = new RoomSettingsDto(
            WordCount: wordCount,
            TimeLimitMinutes: 3,
            Difficulty: DifficultyLevel.Intermediate,
            BestOf: 3
        );

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(7)]
    [InlineData(5)]
    [InlineData(12)]
    [InlineData(25)]
    public void RoomSettingsValidator_WordCount_InvalidValues_Fail(int wordCount)
    {
        // Arrange
        var settings = new RoomSettingsDto(
            WordCount: wordCount,
            TimeLimitMinutes: 3,
            Difficulty: DifficultyLevel.Intermediate,
            BestOf: 3
        );

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WordCount);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public void RoomSettingsValidator_TimeLimit_ValidValues_Pass(int timeLimit)
    {
        // Arrange
        var settings = new RoomSettingsDto(
            WordCount: 15,
            TimeLimitMinutes: timeLimit,
            Difficulty: DifficultyLevel.Intermediate,
            BestOf: 3
        );

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(10)]
    public void RoomSettingsValidator_TimeLimit_InvalidValues_Fail(int timeLimit)
    {
        // Arrange
        var settings = new RoomSettingsDto(
            WordCount: 15,
            TimeLimitMinutes: timeLimit,
            Difficulty: DifficultyLevel.Intermediate,
            BestOf: 3
        );

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TimeLimitMinutes);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void RoomSettingsValidator_BestOf_ValidValues_Pass(int bestOf)
    {
        // Arrange
        var settings = new RoomSettingsDto(
            WordCount: 15,
            TimeLimitMinutes: 3,
            Difficulty: DifficultyLevel.Intermediate,
            BestOf: bestOf
        );

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(7)]
    public void RoomSettingsValidator_BestOf_InvalidValues_Fail(int bestOf)
    {
        // Arrange
        var settings = new RoomSettingsDto(
            WordCount: 15,
            TimeLimitMinutes: 3,
            Difficulty: DifficultyLevel.Intermediate,
            BestOf: bestOf
        );

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BestOf);
    }

    [Theory]
    [InlineData(DifficultyLevel.Beginner)]
    [InlineData(DifficultyLevel.Intermediate)]
    [InlineData(DifficultyLevel.Advanced)]
    [InlineData(DifficultyLevel.Expert)]
    public void RoomSettingsValidator_Difficulty_AnyValue_Pass(DifficultyLevel difficulty)
    {
        // Arrange
        var settings = new RoomSettingsDto(
            WordCount: 15,
            TimeLimitMinutes: 3,
            Difficulty: difficulty,
            BestOf: 3
        );

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Difficulty);
    }

    [Fact]
    public void RoomSettingsValidator_AllValid_Passes()
    {
        // Arrange
        var settings = new RoomSettingsDto(
            WordCount: 20,
            TimeLimitMinutes: 5,
            Difficulty: DifficultyLevel.Expert,
            BestOf: 5
        );

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RoomSettingsValidator_DefaultSettings_Passes()
    {
        // Arrange - default settings from UI
        var settings = new RoomSettingsDto(
            WordCount: 15,
            TimeLimitMinutes: 3,
            Difficulty: DifficultyLevel.Intermediate,
            BestOf: 3
        );

        // Act
        var result = _validator.TestValidate(settings);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
