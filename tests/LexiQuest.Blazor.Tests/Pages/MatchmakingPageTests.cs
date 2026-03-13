using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Pages;
using LexiQuest.Blazor.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using LexiQuest.Blazor.Tests.Helpers;
using Xunit;

namespace LexiQuest.Blazor.Tests.Pages;

public class MatchmakingPageTests : TestContext
{
    private readonly IMatchHubClient _matchHubClient;
    private readonly IStringLocalizer<QuickMatch> _localizer;

    public MatchmakingPageTests()
    {
        _matchHubClient = Substitute.For<IMatchHubClient>();
        _localizer = Substitute.For<IStringLocalizer<QuickMatch>>();
        
        // Setup localizer
        _localizer["Matchmaking_Title"].Returns(new LocalizedString("Matchmaking_Title", "⚔️ 1v1 SOUBOJ ⚔️"));
        _localizer["Matchmaking_Searching"].Returns(new LocalizedString("Matchmaking_Searching", "Hledání soupeře..."));
        _localizer["Matchmaking_Cancel"].Returns(new LocalizedString("Matchmaking_Cancel", "Zrušit hledání"));
        _localizer["Matchmaking_Rules_Title"].Returns(new LocalizedString("Matchmaking_Rules_Title", "Pravidla"));
        _localizer["Matchmaking_Rules_1"].Returns(new LocalizedString("Matchmaking_Rules_1", "15 slov"));
        _localizer["Matchmaking_Rules_2"].Returns(new LocalizedString("Matchmaking_Rules_2", "3 minuty"));
        _localizer["Matchmaking_Rules_3"].Returns(new LocalizedString("Matchmaking_Rules_3", "Nejrychlejší vyhrává"));
        _localizer["Matchmaking_MatchFound_Title"].Returns(new LocalizedString("Matchmaking_MatchFound_Title", "⚔️ SOUPEŘ NALEZEN! ⚔️"));
        _localizer["Matchmaking_Countdown"].Returns(new LocalizedString("Matchmaking_Countdown", "Začínáme za: {0}"));
        _localizer["Matchmaking_GetReady"].Returns(new LocalizedString("Matchmaking_GetReady", "Připrav se!"));
        _localizer["Matchmaking_Timeout_Title"].Returns(new LocalizedString("Matchmaking_Timeout_Title", "Soupeř nenalezen"));
        _localizer["Matchmaking_Retry"].Returns(new LocalizedString("Matchmaking_Retry", "Zkusit znovu"));
        _localizer["Matchmaking_PlayVsAI"].Returns(new LocalizedString("Matchmaking_PlayVsAI", "Hrát proti AI"));
        _localizer["Matchmaking_Back"].Returns(new LocalizedString("Matchmaking_Back", "Zpět"));
        _localizer["Matchmaking_Vs"].Returns(new LocalizedString("Matchmaking_Vs", "VS"));
        
        Services.AddSingleton(_matchHubClient);
        Services.AddSingleton(_localizer);
        Services.AddSingleton<NavigationManager>(new TestNavigationManager());
        Services.AddSingleton(Substitute.For<LexiQuest.Blazor.Services.IAuthService>());
        TempoTestHelper.RegisterTempoServices(Services);
    }

    [Fact]
    public void MatchmakingScreen_Renders_SearchingState()
    {
        // Act
        var cut = Render<QuickMatch>();
        
        // Assert
        cut.Find(".matchmaking-title").TextContent.Should().Contain("1v1");
        cut.Find(".matchmaking-status").TextContent.Should().Contain("Hledání soupeře");
        cut.FindComponent<Tempo.Blazor.Components.Feedback.TmSpinner>().Should().NotBeNull();
        cut.Find(".matchmaking-timer").Should().NotBeNull();
        cut.FindComponent<Tempo.Blazor.Components.Feedback.TmProgressBar>().Should().NotBeNull();
    }

    [Fact]
    public void MatchmakingScreen_MatchFound_ShowsOpponent()
    {
        // Arrange
        var cut = Render<QuickMatch>();
        
        // Act - simulate match found
        var matchFoundEvent = new LexiQuest.Shared.DTOs.Multiplayer.MatchFoundEvent(
            Guid.NewGuid(), "OpponentName", 10, null, DateTime.UtcNow.AddSeconds(3), false);
        _matchHubClient.OnMatchFound += Raise.Event<EventHandler<LexiQuest.Shared.DTOs.Multiplayer.MatchFoundEvent>>(
            this, matchFoundEvent);
        
        // Assert
        cut.Find(".match-found-title").TextContent.Should().Contain("SOUPEŘ NALEZEN");
        cut.FindComponent<Tempo.Blazor.Components.Avatars.TmAvatar>().Should().NotBeNull();
    }

    [Fact]
    public void MatchmakingScreen_Timeout_ShowsOptions()
    {
        // Arrange
        var cut = Render<QuickMatch>();
        
        // Act - simulate timeout
        _matchHubClient.OnMatchmakingTimeout += Raise.Event<EventHandler>(this, EventArgs.Empty);
        
        // Assert
        cut.Find(".timeout-title").TextContent.Should().Contain("Soupeř nenalezen");
        cut.Find(".btn-retry").Should().NotBeNull();
        cut.Find(".btn-play-ai").Should().NotBeNull();
        cut.Find(".btn-back").Should().NotBeNull();
    }
}


