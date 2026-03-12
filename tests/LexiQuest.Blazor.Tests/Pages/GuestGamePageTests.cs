using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Pages;
using LexiQuest.Blazor.Services;
using LexiQuest.Shared.DTOs.Game;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Tempo.Blazor.Localization;
using Xunit;

namespace LexiQuest.Blazor.Tests.Pages;

/// <summary>
/// Tests for GuestGame page (T-302.1).
/// </summary>
public class GuestGamePageTests : BunitContext
{
    private readonly IGuestGameService _guestGameService;
    private readonly IStringLocalizer<GuestGame> _localizer;

    public GuestGamePageTests()
    {
        _guestGameService = Substitute.For<IGuestGameService>();
        _localizer = Substitute.For<IStringLocalizer<GuestGame>>();

        // Setup localizer
        _localizer[Arg.Any<string>()].Returns(x => new LocalizedString(x.Arg<string>(), x.Arg<string>()));
        _localizer["Title"].Returns(new LocalizedString("Title", "Hraj jako host"));
        _localizer["Loading"].Returns(new LocalizedString("Loading", "Načítání..."));
        _localizer["Error_RateLimit"].Returns(new LocalizedString("Error_RateLimit", "Denní limit dosažen"));
        _localizer["Error_Generic"].Returns(new LocalizedString("Error_Generic", "Něco se pokazilo"));
        _localizer["StartGame"].Returns(new LocalizedString("StartGame", "Začít hrát"));
        _localizer["Welcome"].Returns(new LocalizedString("Welcome", "Vítejte v LexiQuest"));
        _localizer["GuestDescription"].Returns(new LocalizedString("GuestDescription", "Hrajte bez registrace"));
        _localizer["BackToHome"].Returns(new LocalizedString("BackToHome", "Zpět"));
        _localizer["AlreadyHaveAccount"].Returns(new LocalizedString("AlreadyHaveAccount", "Už máte účet?"));
        _localizer["Login"].Returns(new LocalizedString("Login", "Přihlásit se"));
        _localizer["WordProgress"].Returns(new LocalizedString("WordProgress", "Slovo {0} z {1}"));
        _localizer["RemainingGames"].Returns(new LocalizedString("RemainingGames", "Zbývá her: {0}"));
        _localizer["SessionXp"].Returns(new LocalizedString("SessionXp", "XP: {0}"));
        _localizer["EnterAnswer"].Returns(new LocalizedString("EnterAnswer", "Zadejte odpověď..."));
        _localizer["Submit"].Returns(new LocalizedString("Submit", "Odeslat"));
        _localizer["Correct"].Returns(new LocalizedString("Correct", "Správně!"));
        _localizer["Wrong"].Returns(new LocalizedString("Wrong", "Špatně"));

        Services.AddSingleton(_guestGameService);
        Services.AddSingleton(_localizer);
        Services.AddSingleton(Substitute.For<ITmLocalizer>());
        Services.AddSingleton(Substitute.For<NavigationManager>());
    }

    [Fact]
    public void GuestGamePage_Renders_WelcomeScreen_WhenNoSession()
    {
        // Act
        var cut = Render<GuestGame>();

        // Assert
        cut.Find("[data-testid='guest-welcome']").Should().NotBeNull();
        cut.Find("[data-testid='btn-start-guest']").Should().NotBeNull();
    }

    [Fact]
    public async Task GuestGamePage_StartGame_CallsService_AndShowsGameArena()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var response = new GuestStartResponse(
            SessionId: sessionId,
            ScrambledWords: new List<GuestScrambledWordDto>
            {
                new(Guid.NewGuid(), "sep", 3),
                new(Guid.NewGuid(), "ktaa", 4),
                new(Guid.NewGuid(), "omcd", 4),
                new(Guid.NewGuid(), "lpsoe", 5),
                new(Guid.NewGuid(), "iahtsl", 6)
            },
            RemainingGames: 4,
            Message: "Hra začala"
        );

        _guestGameService.StartGameAsync().Returns(response);

        var cut = Render<GuestGame>();

        // Act
        cut.Find("[data-testid='btn-start-guest']").Click();
        await Task.Delay(100); // Wait for async operation
        cut.Render();

        // Assert
        await _guestGameService.Received(1).StartGameAsync();
        cut.Find("[data-testid='game-arena']").Should().NotBeNull();
    }

    [Fact]
    public async Task GuestGamePage_StartGame_RateLimit_ShowsLimitReached()
    {
        // Arrange
        _guestGameService.StartGameAsync().Returns((GuestStartResponse?)null);

        var cut = Render<GuestGame>();

        // Act
        cut.Find("[data-testid='btn-start-guest']").Click();
        await Task.Delay(100);
        cut.Render();

        // Assert
        cut.Find("[data-testid='guest-limit-reached']").Should().NotBeNull();
    }

    [Fact]
    public void GuestGamePage_LoadsExistingSession_ShowsGameArena()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var response = new GuestStartResponse(
            SessionId: sessionId,
            ScrambledWords: new List<GuestScrambledWordDto>
            {
                new(Guid.NewGuid(), "sep", 3)
            },
            RemainingGames: 4,
            Message: "Hra pokračuje"
        );

        _guestGameService.GetSessionAsync(sessionId).Returns(response);

        // Act
        var cut = Render<GuestGame>(parameters => parameters
            .Add(p => p.SessionId, sessionId));

        // Assert
        cut.Find("[data-testid='game-arena']").Should().NotBeNull();
    }

    [Fact]
    public async Task GuestGamePage_SubmitCorrectAnswer_ShowsCTAModal()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var wordId = Guid.NewGuid();
        var startResponse = new GuestStartResponse(
            SessionId: sessionId,
            ScrambledWords: new List<GuestScrambledWordDto>
            {
                new(wordId, "sep", 3)
            },
            RemainingGames: 4,
            Message: "Hra začala"
        );

        var answerResponse = new GuestAnswerResponse(
            IsCorrect: true,
            XpEarned: 10,
            CorrectAnswer: "pes",
            UserAnswer: null,
            TotalSessionXp: 10,
            WordsSolved: 1,
            WordsRemaining: 4,
            IsGameComplete: false
        );

        _guestGameService.StartGameAsync().Returns(startResponse);
        _guestGameService.SubmitAnswerAsync(sessionId, wordId, "pes").Returns(answerResponse);

        var cut = Render<GuestGame>();
        cut.Find("[data-testid='btn-start-guest']").Click();
        await Task.Delay(100);
        cut.Render();

        // Act
        var input = cut.Find("[data-testid='answer-input']");
        input.Input("pes");
        cut.Find("[data-testid='btn-submit']").Click();
        await Task.Delay(100);
        cut.Render();

        // Assert
        cut.Find("[data-testid='guest-cta-modal']").Should().NotBeNull();
    }

    [Fact]
    public async Task GuestGamePage_GameComplete_ShowsConvertModal()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var wordId = Guid.NewGuid();
        var startResponse = new GuestStartResponse(
            SessionId: sessionId,
            ScrambledWords: new List<GuestScrambledWordDto>
            {
                new(wordId, "sep", 3)
            },
            RemainingGames: 4,
            Message: "Hra začala"
        );

        var answerResponse = new GuestAnswerResponse(
            IsCorrect: true,
            XpEarned: 10,
            CorrectAnswer: "pes",
            UserAnswer: null,
            TotalSessionXp: 10,
            WordsSolved: 5,
            WordsRemaining: 0,
            IsGameComplete: true
        );

        _guestGameService.StartGameAsync().Returns(startResponse);
        _guestGameService.SubmitAnswerAsync(sessionId, wordId, "pes").Returns(answerResponse);

        var cut = Render<GuestGame>();
        cut.Find("[data-testid='btn-start-guest']").Click();
        await Task.Delay(100);
        cut.Render();

        // Act
        var input = cut.Find("[data-testid='answer-input']");
        input.Input("pes");
        cut.Find("[data-testid='btn-submit']").Click();
        await Task.Delay(100);
        cut.Render();

        // Assert
        cut.Find("[data-testid='guest-convert-modal']").Should().NotBeNull();
    }

    [Fact]
    public void GuestGamePage_BackButton_NavigatesToHome()
    {
        // Arrange
        var navigationManager = Substitute.For<NavigationManager>();
        Services.AddSingleton(navigationManager);

        var cut = Render<GuestGame>();

        // Act
        cut.Find("[data-testid='btn-back']").Click();

        // Assert
        navigationManager.Received(1).NavigateTo("/");
    }

    [Fact]
    public async Task GuestGamePage_DisplaysRemainingGames()
    {
        // Arrange
        var response = new GuestStartResponse(
            SessionId: Guid.NewGuid(),
            ScrambledWords: new List<GuestScrambledWordDto>(),
            RemainingGames: 3,
            Message: "Hra začala"
        );

        _guestGameService.StartGameAsync().Returns(response);

        var cut = Render<GuestGame>();

        // Act
        cut.Find("[data-testid='btn-start-guest']").Click();
        await Task.Delay(100);
        cut.Render();

        // Assert
        var remainingText = cut.Find("[data-testid='remaining-games']").TextContent;
        remainingText.Should().Contain("3");
    }
}
