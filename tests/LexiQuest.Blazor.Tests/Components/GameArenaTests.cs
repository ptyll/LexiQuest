using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components.Game;
using LexiQuest.Shared.DTOs.Game;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using LexiQuest.Blazor.Tests.Helpers;
using NSubstitute;

namespace LexiQuest.Blazor.Tests.Components;

public class GameArenaTests : BunitContext
{
    private readonly IStringLocalizer<GameArena> _localizer;
    private readonly IStringLocalizer<LivesIndicator> _livesLocalizer;
    private readonly IStringLocalizer<LetterTileInput> _letterTileLocalizer;

    public GameArenaTests()
    {
        _localizer = Substitute.For<IStringLocalizer<GameArena>>();
        _livesLocalizer = Substitute.For<IStringLocalizer<LivesIndicator>>();
        _letterTileLocalizer = Substitute.For<IStringLocalizer<LetterTileInput>>();
        SetupLocalizer();
        SetupLivesLocalizer();
        SetupLetterTileLocalizer();

        Services.AddSingleton(_localizer);
        Services.AddSingleton(_livesLocalizer);
        Services.AddSingleton(_letterTileLocalizer);
        TempoTestHelper.RegisterTempoServices(Services);
    }

    private void SetupLocalizer()
    {
        _localizer["GameArena.Aria"].Returns(new LocalizedString("GameArena.Aria", "Game arena"));
        _localizer["Button.Back"].Returns(new LocalizedString("Button.Back", "Back"));
        _localizer["Button.Submit"].Returns(new LocalizedString("Button.Submit", "Submit"));
        _localizer["Button.Submitting"].Returns(new LocalizedString("Button.Submitting", "Submitting..."));
        _localizer["Button.Skip"].Returns(new LocalizedString("Button.Skip", "Skip"));
        _localizer["Button.Continue"].Returns(new LocalizedString("Button.Continue", "Continue"));
        _localizer["Answer.Placeholder"].Returns(new LocalizedString("Answer.Placeholder", "Enter your answer"));
        _localizer["Level.Name"].Returns(new LocalizedString("Level.Name", "Round {0}"));
        _localizer["Level.Progress"].Returns(new LocalizedString("Level.Progress", "{0} / {1}"));
        _localizer["Combo.Multiplier"].Returns(new LocalizedString("Combo.Multiplier", "x{0} Combo"));
        _localizer["Feedback.Correct"].Returns(new LocalizedString("Feedback.Correct", "Correct! +{0} XP"));
        _localizer["Feedback.Wrong"].Returns(new LocalizedString("Feedback.Wrong", "Wrong answer!"));
        _localizer["SpeedBonus.Label"].Returns(new LocalizedString("SpeedBonus.Label", "Speed Bonus"));
        _localizer["CorrectAnswer.Was"].Returns(new LocalizedString("CorrectAnswer.Was", "Correct answer was: {0}"));
        _localizer["LevelComplete.Title"].Returns(new LocalizedString("LevelComplete.Title", "Level Complete!"));
        _localizer["LevelComplete.XP"].Returns(new LocalizedString("LevelComplete.XP", "Total XP earned: {0}"));
        _localizer["GameOver.Title"].Returns(new LocalizedString("GameOver.Title", "Game over"));
        _localizer["GameOver.Description"].Returns(new LocalizedString("GameOver.Description", "No lives remaining."));
        _localizer["TimeUp.Title"].Returns(new LocalizedString("TimeUp.Title", "Time is up"));
        _localizer["TimeUp.Description"].Returns(new LocalizedString("TimeUp.Description", "Training ended."));
    }

    private void SetupLivesLocalizer()
    {
        _livesLocalizer["Label"].Returns(new LocalizedString("Label", "Lives"));
        _livesLocalizer["Regeneration"].Returns(new LocalizedString("Regeneration", "Next life in: {0}"));
        _livesLocalizer["NoLives"].Returns(new LocalizedString("NoLives", "No lives"));
        _livesLocalizer["LowWarning"].Returns(new LocalizedString("LowWarning", "Last life"));
    }

    private void SetupLetterTileLocalizer()
    {
        _letterTileLocalizer[Arg.Any<string>()].Returns(x => new LocalizedString(x.Arg<string>(), x.Arg<string>()));
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
    public void GameArena_RendersAnswerWorkbench()
    {
        // Act
        var cut = Render<GameArena>(parameters => parameters
            .Add(p => p.ScrambledWord, "PES"));

        // Assert
        cut.Find(".answer-workbench").Should().NotBeNull();
        cut.Find(".answer-workbench .letter-tile-input").Should().NotBeNull();
        cut.Find(".answer-workbench .answer-input").Should().NotBeNull();
    }

    [Fact]
    public void GameArena_LetterTileInput_RendersComposerAndTileBank()
    {
        // Act
        var cut = Render<GameArena>(parameters => parameters
            .Add(p => p.ScrambledWord, "PES"));

        // Assert
        cut.Find(".answer-composer").Should().NotBeNull();
        cut.Find(".tile-bank").Should().NotBeNull();
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
    public async Task GameArena_ShowResult_WhileSubmitCallbackIsRunning_ResetsSubmittingState()
    {
        // Arrange
        var result = new GameRoundResult(
            IsCorrect: true,
            CorrectAnswer: "JABLKO",
            XPEarned: 15,
            SpeedBonus: 0,
            ComboCount: 1,
            IsLevelComplete: false,
            LivesRemaining: 5,
            null, null, false);
        var resultShown = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseSubmit = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        IRenderedComponent<GameArena>? cut = null;

        cut = Render<GameArena>(parameters => parameters
            .Add(p => p.OnSubmitAnswer, async _ =>
            {
                await cut!.Instance.ShowResult(result, 15);
                resultShown.SetResult();
                await releaseSubmit.Task;
            }));

        cut.Find(".answer-input").Input("JABLKO");

        // Act
        var clickTask = cut.InvokeAsync(() => cut.Find(".btn-submit").Click());
        await resultShown.Task.WaitAsync(TimeSpan.FromSeconds(1));

        // Assert
        cut.Find(".feedback-success").Should().NotBeNull();
        cut.Find(".btn-submit").TextContent.Should().Contain("Submit");
        cut.Find(".btn-submit").TextContent.Should().NotContain("Submitting");

        releaseSubmit.SetResult();
        await clickTask;
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
    public async Task GameArena_ShowResult_WithNextRound_DisablesControlsDuringFeedback()
    {
        // Arrange
        var cut = Render<GameArena>();
        cut.Find(".answer-input").Input("WRONG");

        var result = new GameRoundResult(
            IsCorrect: false,
            CorrectAnswer: "JABLKO",
            XPEarned: 0,
            SpeedBonus: 0,
            ComboCount: 0,
            IsLevelComplete: false,
            LivesRemaining: 4,
            NextScrambledWord: "BANÁN",
            NextRoundNumber: 2,
            IsGameOver: false);

        // Act
        await cut.Instance.ShowResult(result, 0);

        // Assert
        cut.Find(".answer-input").HasAttribute("disabled").Should().BeTrue();
        cut.Find(".btn-submit").HasAttribute("disabled").Should().BeTrue();
        cut.Find(".btn-skip").HasAttribute("disabled").Should().BeTrue();
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
    public async Task GameArena_GameOver_ShowsOverlayAndDisablesInput()
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
            LivesRemaining: 0,
            null, null, true);

        // Act
        await cut.Instance.ShowResult(result, 0);

        // Assert
        cut.Find("[data-testid='game-over']").TextContent.Should().Contain("Game over");
        cut.Find(".answer-input").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public async Task GameArena_ShowTimeExpired_ShowsOverlayAndDisablesControls()
    {
        // Arrange
        var cut = Render<GameArena>(parameters => parameters
            .Add(p => p.ScrambledWord, "PES"));
        cut.Find(".answer-input").Input("PES");

        // Act
        await cut.Instance.ShowTimeExpired();

        // Assert
        cut.Find("[data-testid='game-time-expired']").TextContent.Should().Contain("Time is up");
        cut.Find(".answer-input").HasAttribute("disabled").Should().BeTrue();
        cut.Find(".btn-submit").HasAttribute("disabled").Should().BeTrue();
        cut.Find(".btn-skip").HasAttribute("disabled").Should().BeTrue();
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
    public async Task GameArena_EnterKey_SubmitsCurrentAnswer()
    {
        // Arrange
        string? submittedAnswer = null;
        var cut = Render<GameArena>(parameters => parameters
            .Add(p => p.OnSubmitAnswer, answer =>
            {
                submittedAnswer = answer;
                return Task.CompletedTask;
            }));
        var input = cut.Find(".answer-input");
        input.Input("JABLKO");

        // Act
        input.KeyDown(new KeyboardEventArgs { Key = "Enter" });
        await Task.Delay(50);

        // Assert
        submittedAnswer.Should().Be("JABLKO");
    }

    [Fact]
    public void GameArena_LetterTiles_BuildAnswerAndEnableSubmit()
    {
        // Arrange
        var cut = Render<GameArena>(parameters => parameters
            .Add(p => p.ScrambledWord, "PES"));

        // Act
        ClickTile(cut, "P");
        ClickTile(cut, "E");
        ClickTile(cut, "S");

        // Assert
        cut.Find(".answer-input").GetAttribute("value").Should().Be("PES");
        cut.Find(".btn-submit").HasAttribute("disabled").Should().BeFalse();
        cut.FindAll("[data-testid='game-letter-input-tile'][disabled]").Should().HaveCount(3);
    }

    [Fact]
    public void GameArena_LetterTileBackspaceAndClear_UpdateAnswer()
    {
        // Arrange
        var cut = Render<GameArena>(parameters => parameters
            .Add(p => p.ScrambledWord, "PES"));
        ClickTile(cut, "P");
        ClickTile(cut, "E");

        // Act
        cut.Find("[data-testid='game-letter-input-backspace']").Click();

        // Assert
        cut.Find(".answer-input").GetAttribute("value").Should().Be("P");

        // Act
        cut.Find("[data-testid='game-letter-input-clear']").Click();

        // Assert
        cut.Find(".answer-input").GetAttribute("value").Should().BeNullOrEmpty();
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

    private static void ClickTile(IRenderedComponent<GameArena> cut, string letter)
    {
        cut.FindAll("[data-testid='game-letter-input-tile']")
            .First(tile => tile.TextContent.Trim() == letter)
            .Click();
    }
}
