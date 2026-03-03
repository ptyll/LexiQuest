using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components.Game;
using LexiQuest.Shared.DTOs.Game;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace LexiQuest.Blazor.Tests.Components;

public class GameArenaTests : BunitContext
{
    private readonly IStringLocalizer<GameArena> _localizer;

    public GameArenaTests()
    {
        _localizer = Substitute.For<IStringLocalizer<GameArena>>();
        SetupLocalizer();

        Services.AddSingleton(_localizer);
    }

    private void SetupLocalizer()
    {
        _localizer["Button_Back"].Returns(new LocalizedString("Button_Back", "Back"));
        _localizer["Button_Submit"].Returns(new LocalizedString("Button_Submit", "Submit"));
        _localizer["Button_Submitting"].Returns(new LocalizedString("Button_Submitting", "Submitting..."));
        _localizer["Button_Skip"].Returns(new LocalizedString("Button_Skip", "Skip"));
        _localizer["Button_Continue"].Returns(new LocalizedString("Button_Continue", "Continue"));
        _localizer["Answer_Placeholder"].Returns(new LocalizedString("Answer_Placeholder", "Enter your answer"));
        _localizer["Level_Name"].Returns(new LocalizedString("Level_Name", "Round {0}"));
        _localizer["Level_Progress"].Returns(new LocalizedString("Level_Progress", "{0} / {1}"));
        _localizer["Combo_Multiplier"].Returns(new LocalizedString("Combo_Multiplier", "x{0} Combo"));
        _localizer["Feedback_Correct"].Returns(new LocalizedString("Feedback_Correct", "Correct! +{0} XP"));
        _localizer["Feedback_Wrong"].Returns(new LocalizedString("Feedback_Wrong", "Wrong answer!"));
        _localizer["SpeedBonus_Label"].Returns(new LocalizedString("SpeedBonus_Label", "Speed Bonus"));
        _localizer["Correct_Answer_Was"].Returns(new LocalizedString("Correct_Answer_Was", "Correct answer was: {0}"));
        _localizer["LevelComplete_Title"].Returns(new LocalizedString("LevelComplete_Title", "Level Complete!"));
        _localizer["LevelComplete_XP"].Returns(new LocalizedString("LevelComplete_XP", "Total XP earned: {0}"));
    }

    [Fact]
    public void GameArena_Renders_ScrambledWordLetters()
    {
        // Arrange & Act
        var cut = Render<GameArena>(parameters => parameters
            .Add(p => p.ScrambledWord, "LKBOJA")
            .Add(p => p.CurrentRound, 1)
            .Add(p => p.TotalRounds, 10));

        // Assert
        var letters = cut.FindAll(".letter-card");
        letters.Count.Should().Be(6);
        letters[0].TextContent.Should().Be("L");
        letters[1].TextContent.Should().Be("K");
        letters[5].TextContent.Should().Be("A");
    }

    [Fact]
    public void GameArena_Renders_AnswerInput()
    {
        // Act
        var cut = Render<GameArena>();

        // Assert
        var input = cut.Find(".answer-input");
        input.Should().NotBeNull();
        input.GetAttribute("placeholder").Should().Be("Enter your answer");
    }

    [Fact]
    public void GameArena_Renders_SubmitButton()
    {
        // Act
        var cut = Render<GameArena>();

        // Assert
        var button = cut.Find(".btn-submit");
        button.TextContent.Should().Contain("Submit");
    }

    [Fact]
    public void GameArena_Renders_SkipButton()
    {
        // Act
        var cut = Render<GameArena>();

        // Assert
        var button = cut.Find(".btn-skip");
        button.TextContent.Should().Contain("Skip");
    }

    [Fact]
    public void GameArena_Renders_LevelProgress()
    {
        // Arrange & Act
        var cut = Render<GameArena>(parameters => parameters
            .Add(p => p.CurrentRound, 3)
            .Add(p => p.TotalRounds, 10));

        // Assert
        cut.Find(".level-progress").TextContent.Should().Contain("3");
        cut.Find(".level-progress").TextContent.Should().Contain("10");
    }

    [Fact]
    public void GameArena_WithCombo_ShowsComboBadge()
    {
        // Arrange & Act
        var cut = Render<GameArena>(parameters => parameters
            .Add(p => p.ComboCount, 5));

        // Assert
        cut.Find(".combo-badge").TextContent.Should().Contain("x5");
    }

    [Fact]
    public void GameArena_WithoutCombo_HidesComboBadge()
    {
        // Act
        var cut = Render<GameArena>(parameters => parameters
            .Add(p => p.ComboCount, 0));

        // Assert
        cut.FindAll(".combo-badge").Count.Should().Be(0);
    }

    [Fact]
    public async Task GameArena_ShowResult_Correct_DisplaysSuccessFeedback()
    {
        // Arrange
        var cut = Render<GameArena>();
        var result = new GameRoundResult(
            IsCorrect: true,
            CorrectAnswer: "JABLKO",
            XPEarned: 15,
            SpeedBonus: 3,
            ComboCount: 1,
            IsLevelComplete: false,
            LivesRemaining: 5,
            null, null, false);

        // Act
        await cut.Instance.ShowResult(result, 15);

        // Assert
        cut.Find(".feedback-success").Should().NotBeNull();
        cut.Find(".feedback").TextContent.Should().Contain("15");
    }

    [Fact]
    public async Task GameArena_ShowResult_Wrong_DisplaysErrorFeedback()
    {
        // Arrange
        var cut = Render<GameArena>();
        var result = new GameRoundResult(
            IsCorrect: false,
            CorrectAnswer: "JABLKO",
            XPEarned: 0,
            SpeedBonus: 0,
            ComboCount: 0,
            IsLevelComplete: false,
            LivesRemaining: 4,
            null, null, false);

        // Act
        await cut.Instance.ShowResult(result, 0);

        // Assert
        cut.Find(".feedback-error").Should().NotBeNull();
        cut.Find(".feedback").TextContent.Should().Contain("JABLKO");
    }

    [Fact]
    public async Task GameArena_LevelComplete_ShowsOverlay()
    {
        // Arrange
        var cut = Render<GameArena>();
        var result = new GameRoundResult(
            IsCorrect: true,
            CorrectAnswer: "JABLKO",
            XPEarned: 10,
            SpeedBonus: 0,
            ComboCount: 1,
            IsLevelComplete: true,
            LivesRemaining: 5,
            null, null, false);

        // Act
        await cut.Instance.ShowResult(result, 150);

        // Assert
        cut.Find(".level-complete-overlay").Should().NotBeNull();
        cut.Find(".level-complete-content").TextContent.Should().Contain("Level Complete");
        cut.Find(".level-complete-content").TextContent.Should().Contain("150");
    }

    [Fact]
    public async Task GameArena_UpdateScrambledWord_UpdatesDisplay()
    {
        // Arrange
        var cut = Render<GameArena>(parameters => parameters
            .Add(p => p.ScrambledWord, "LKBOJA"));

        // Act
        await cut.Instance.UpdateScrambledWord("BANÁN", 2);

        // Assert
        var letters = cut.FindAll(".letter-card");
        letters.Count.Should().Be(5);
        letters[0].TextContent.Should().Be("B");
    }

    [Fact]
    public void GameArena_EmptyAnswer_DisablesSubmitButton()
    {
        // Arrange
        var cut = Render<GameArena>();

        // Act - enter empty text
        var input = cut.Find(".answer-input");
        input.Input("");

        // Assert
        var button = cut.Find(".btn-submit");
        button.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void GameArena_EnteredAnswer_EnablesSubmitButton()
    {
        // Arrange
        var cut = Render<GameArena>();

        // Act - enter text
        var input = cut.Find(".answer-input");
        input.Input("JABLKO");

        // Assert
        var button = cut.Find(".btn-submit");
        button.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void GameArena_EmptyScrambledWord_HidesLetters()
    {
        // Act
        var cut = Render<GameArena>(parameters => parameters
            .Add(p => p.ScrambledWord, ""));

        // Assert
        cut.FindAll(".letter-card").Count.Should().Be(0);
    }

    [Fact]
    public void GameArena_CorrectAnswerAfterShowResult_ClearsInput()
    {
        // Arrange
        var cut = Render<GameArena>();
        var input = cut.Find(".answer-input");
        input.Input("JABLKO");

        var result = new GameRoundResult(
            IsCorrect: true,
            CorrectAnswer: "JABLKO",
            XPEarned: 15,
            SpeedBonus: 0,
            ComboCount: 1,
            IsLevelComplete: false,
            LivesRemaining: 5,
            null, null, false);

        // Act
        cut.Instance.ShowResult(result, 15);

        // Assert
        cut.Find(".answer-input").GetAttribute("value").Should().BeNullOrEmpty();
    }

    [Fact]
    public void GameArena_WrongAnswerAfterShowResult_KeepsInput()
    {
        // Arrange
        var cut = Render<GameArena>();
        var input = cut.Find(".answer-input");
        input.Input("WRONG");

        var result = new GameRoundResult(
            IsCorrect: false,
            CorrectAnswer: "JABLKO",
            XPEarned: 0,
            SpeedBonus: 0,
            ComboCount: 0,
            IsLevelComplete: false,
            LivesRemaining: 4,
            null, null, false);

        // Act
        cut.Instance.ShowResult(result, 0);

        // Assert - input should still have the wrong answer for user to see
        cut.Find(".answer-input").GetAttribute("value").Should().Be("WRONG");
    }

    [Fact]
    public void GameArena_InputWithWhitespace_TrimmedWhenValidated()
    {
        // Arrange
        var cut = Render<GameArena>();

        // Act - enter text with whitespace
        var input = cut.Find(".answer-input");
        input.Input("  JABLKO  ");

        // Assert - button should be enabled (whitespace is trimmed in validation)
        var button = cut.Find(".btn-submit");
        button.HasAttribute("disabled").Should().BeFalse();
    }
}
