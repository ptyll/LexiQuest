using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Pages;
using LexiQuest.Blazor.Services;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using LexiQuest.Blazor.Tests.Helpers;

namespace LexiQuest.Blazor.Tests.Pages;

public class GamePageTests : BunitContext
{
    private readonly IStringLocalizer<Game> _localizer;
    private readonly IGameService _gameService;
    private readonly IToastService _toastService;

    public GamePageTests()
    {
        _localizer = Substitute.For<IStringLocalizer<Game>>();
        _localizer["Loading"].Returns(new LocalizedString("Loading", "Loading..."));
        _localizer["Welcome"].Returns(new LocalizedString("Welcome", "Welcome!"));
        _localizer["SelectMode"].Returns(new LocalizedString("SelectMode", "Select game mode"));
        _localizer["Mode_Training"].Returns(new LocalizedString("Mode_Training", "Training"));
        _localizer["Mode_TimeAttack"].Returns(new LocalizedString("Mode_TimeAttack", "Time Attack"));
        _localizer["Retry"].Returns(new LocalizedString("Retry", "Try Again"));
        _localizer["Error_StartingGame"].Returns(new LocalizedString("Error_StartingGame", "Failed to start game"));
        _localizer["Error_LoadingGame"].Returns(new LocalizedString("Error_LoadingGame", "Failed to load game"));
        _localizer["OfflineTraining"].Returns(new LocalizedString("OfflineTraining", "Offline training"));
        _localizer["AchievementUnlocked.Toast"].Returns(new LocalizedString("AchievementUnlocked.Toast", "Úspěch odemčen: {0}"));

        _gameService = Substitute.For<IGameService>();
        _toastService = Substitute.For<IToastService>();

        // Mock localizers for child components
        var gameArenaLocalizer = Substitute.For<IStringLocalizer<LexiQuest.Blazor.Components.Game.GameArena>>();
        gameArenaLocalizer["GameArena.Aria"].Returns(new LocalizedString("GameArena.Aria", "Game arena"));
        gameArenaLocalizer["Button.Back"].Returns(new LocalizedString("Button.Back", "Back"));
        gameArenaLocalizer["Button.Submit"].Returns(new LocalizedString("Button.Submit", "Submit"));
        gameArenaLocalizer["Button.Submitting"].Returns(new LocalizedString("Button.Submitting", "Submitting..."));
        gameArenaLocalizer["Button.Skip"].Returns(new LocalizedString("Button.Skip", "Skip"));
        gameArenaLocalizer["Button.Continue"].Returns(new LocalizedString("Button.Continue", "Continue"));
        gameArenaLocalizer["Answer.Placeholder"].Returns(new LocalizedString("Answer.Placeholder", "Enter answer"));
        gameArenaLocalizer["Level.Name"].Returns(new LocalizedString("Level.Name", "Round {0}"));
        gameArenaLocalizer["Level.Progress"].Returns(new LocalizedString("Level.Progress", "{0} / {1}"));
        gameArenaLocalizer["Combo.Multiplier"].Returns(new LocalizedString("Combo.Multiplier", "x{0} Combo"));
        gameArenaLocalizer["Feedback.Correct"].Returns(new LocalizedString("Feedback.Correct", "Correct! +{0} XP"));
        gameArenaLocalizer["Feedback.Wrong"].Returns(new LocalizedString("Feedback.Wrong", "Wrong answer!"));
        gameArenaLocalizer["SpeedBonus.Label"].Returns(new LocalizedString("SpeedBonus.Label", "Speed Bonus"));
        gameArenaLocalizer["CorrectAnswer.Was"].Returns(new LocalizedString("CorrectAnswer.Was", "Correct answer was: {0}"));
        gameArenaLocalizer["LevelComplete.Title"].Returns(new LocalizedString("LevelComplete.Title", "Level Complete!"));
        gameArenaLocalizer["LevelComplete.XP"].Returns(new LocalizedString("LevelComplete.XP", "Total XP earned: {0}"));
        gameArenaLocalizer["GameOver.Title"].Returns(new LocalizedString("GameOver.Title", "Game over"));
        gameArenaLocalizer["GameOver.Description"].Returns(new LocalizedString("GameOver.Description", "No lives remaining."));

        var gameTimerLocalizer = Substitute.For<IStringLocalizer<LexiQuest.Blazor.Components.Game.GameTimer>>();
        gameTimerLocalizer["TimeRemaining"].Returns(new LocalizedString("TimeRemaining", "Time: {0}"));

        var livesIndicatorLocalizer = Substitute.For<IStringLocalizer<LexiQuest.Blazor.Components.Game.LivesIndicator>>();
        livesIndicatorLocalizer["Label"].Returns(new LocalizedString("Label", "Lives"));
        livesIndicatorLocalizer["Regeneration"].Returns(new LocalizedString("Regeneration", "Next life in: {0}"));
        livesIndicatorLocalizer["NoLives"].Returns(new LocalizedString("NoLives", "No lives"));
        livesIndicatorLocalizer["LowWarning"].Returns(new LocalizedString("LowWarning", "Last life"));

        Services.AddSingleton(_localizer);
        Services.AddSingleton(gameArenaLocalizer);
        Services.AddSingleton(gameTimerLocalizer);
        Services.AddSingleton(livesIndicatorLocalizer);
        Services.AddSingleton(_gameService);
        Services.AddSingleton(_toastService);
        Services.AddSingleton<NotificationRefreshService>();
        TempoTestHelper.RegisterTempoServices(Services);
    }

    [Fact]
    public void GamePage_InitialState_ShowsStartScreen()
    {
        // Act
        var cut = Render<Game>();

        // Assert
        cut.Find(".start-screen").Should().NotBeNull();
        cut.Find("h1").TextContent.Should().Contain("Welcome");
    }

    [Fact]
    public void GamePage_HasModeButtons()
    {
        // Act
        var cut = Render<Game>();

        // Assert
        var buttons = cut.FindAll(".btn-mode");
        buttons.Count.Should().Be(2);
        buttons[0].TextContent.Should().Contain("Training");
        buttons[1].TextContent.Should().Contain("Time Attack");
    }

    [Fact]
    public void GamePage_ClickTraining_StartsGame()
    {
        // Arrange
        var expectedResponse = new ScrambledWordDto(
            Guid.NewGuid(), 1, "LKBOJA", 6, 
            DifficultyLevel.Beginner, 30, 10, 5);

        _gameService.StartGameAsync(Arg.Any<StartGameRequest>())
            .Returns(Task.FromResult<ScrambledWordDto?>(expectedResponse));

        var cut = Render<Game>();

        // Act
        var button = cut.FindAll(".btn-mode")[0];
        button.Click();

        // Assert
        _gameService.Received(1).StartGameAsync(
            Arg.Is<StartGameRequest>(r => r.Mode == GameMode.Training));
    }

    [Fact]
    public void GamePage_WhileLoading_ShowsSpinner()
    {
        // Arrange
        var tcs = new TaskCompletionSource<ScrambledWordDto?>();
        _gameService.StartGameAsync(Arg.Any<StartGameRequest>()).Returns(tcs.Task);

        var cut = Render<Game>();

        // Act
        var button = cut.FindAll(".btn-mode")[0];
        button.Click();

        // Assert
        cut.Find(".loading-state").Should().NotBeNull();
        cut.Find(".spinner").Should().NotBeNull();
    }

    [Fact]
    public void GamePage_WithSessionId_LoadsGameState()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var expectedState = new ScrambledWordDto(
            sessionId, 3, "BANÁN", 5,
            DifficultyLevel.Beginner, 30, 10, 4);

        _gameService.GetGameStateAsync(sessionId)
            .Returns(Task.FromResult<ScrambledWordDto?>(expectedState));

        // Act
        var cut = Render<Game>(parameters => parameters
            .Add(p => p.SessionId, sessionId));

        // Assert
        _gameService.Received(1).GetGameStateAsync(sessionId);
    }
}
