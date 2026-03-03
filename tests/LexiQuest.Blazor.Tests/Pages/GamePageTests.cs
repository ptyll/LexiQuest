using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Pages;
using LexiQuest.Blazor.Services;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace LexiQuest.Blazor.Tests.Pages;

public class GamePageTests : BunitContext
{
    private readonly IStringLocalizer<Game> _localizer;
    private readonly IGameService _gameService;

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

        _gameService = Substitute.For<IGameService>();

        // Mock localizers for child components
        var gameArenaLocalizer = Substitute.For<IStringLocalizer<LexiQuest.Blazor.Components.Game.GameArena>>();
        gameArenaLocalizer["Button_Back"].Returns(new LocalizedString("Button_Back", "Back"));
        gameArenaLocalizer["Button_Submit"].Returns(new LocalizedString("Button_Submit", "Submit"));
        gameArenaLocalizer["Button_Submitting"].Returns(new LocalizedString("Button_Submitting", "Submitting..."));
        gameArenaLocalizer["Button_Skip"].Returns(new LocalizedString("Button_Skip", "Skip"));
        gameArenaLocalizer["Button_Continue"].Returns(new LocalizedString("Button_Continue", "Continue"));
        gameArenaLocalizer["Answer_Placeholder"].Returns(new LocalizedString("Answer_Placeholder", "Enter answer"));
        gameArenaLocalizer["Level_Name"].Returns(new LocalizedString("Level_Name", "Round {0}"));
        gameArenaLocalizer["Level_Progress"].Returns(new LocalizedString("Level_Progress", "{0} / {1}"));
        gameArenaLocalizer["Combo_Multiplier"].Returns(new LocalizedString("Combo_Multiplier", "x{0} Combo"));
        gameArenaLocalizer["Feedback_Correct"].Returns(new LocalizedString("Feedback_Correct", "Correct! +{0} XP"));
        gameArenaLocalizer["Feedback_Wrong"].Returns(new LocalizedString("Feedback_Wrong", "Wrong answer!"));
        gameArenaLocalizer["SpeedBonus_Label"].Returns(new LocalizedString("SpeedBonus_Label", "Speed Bonus"));
        gameArenaLocalizer["Correct_Answer_Was"].Returns(new LocalizedString("Correct_Answer_Was", "Correct answer was: {0}"));
        gameArenaLocalizer["LevelComplete_Title"].Returns(new LocalizedString("LevelComplete_Title", "Level Complete!"));
        gameArenaLocalizer["LevelComplete_XP"].Returns(new LocalizedString("LevelComplete_XP", "Total XP earned: {0}"));

        var gameTimerLocalizer = Substitute.For<IStringLocalizer<LexiQuest.Blazor.Components.Game.GameTimer>>();
        gameTimerLocalizer["TimeRemaining"].Returns(new LocalizedString("TimeRemaining", "Time: {0}"));

        Services.AddSingleton(_localizer);
        Services.AddSingleton(gameArenaLocalizer);
        Services.AddSingleton(gameTimerLocalizer);
        Services.AddSingleton(_gameService);
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
