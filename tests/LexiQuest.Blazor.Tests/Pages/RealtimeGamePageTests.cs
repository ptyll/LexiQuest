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

public class RealtimeGamePageTests : TestContext
{
    private readonly IMatchHubClient _matchHubClient;
    private readonly IStringLocalizer<RealtimeGame> _localizer;

    public RealtimeGamePageTests()
    {
        _matchHubClient = Substitute.For<IMatchHubClient>();
        _localizer = Substitute.For<IStringLocalizer<RealtimeGame>>();
        
        // Setup localizer
        _localizer["Game_Header_Word"].Returns(new LocalizedString("Game_Header_Word", "Slovo"));
        _localizer["Game_Player_You"].Returns(new LocalizedString("Game_Player_You", "Vy"));
        _localizer["Game_Player_Opponent"].Returns(new LocalizedString("Game_Player_Opponent", "Soupeř"));
        _localizer["Game_Input_Placeholder"].Returns(new LocalizedString("Game_Input_Placeholder", "Zadej odpověď..."));
        _localizer["Game_Button_Submit"].Returns(new LocalizedString("Game_Button_Submit", "Odeslat"));
        _localizer["Game_Combo_Label"].Returns(new LocalizedString("Game_Combo_Label", "🔥 x{0}"));
        _localizer["Game_Timer_Format"].Returns(new LocalizedString("Game_Timer_Format", "{0}:{1:D2}"));
        
        Services.AddSingleton(_matchHubClient);
        Services.AddSingleton(_localizer);
        Services.AddSingleton<NavigationManager>(new TestNavigationManager());
        TempoTestHelper.RegisterTempoServices(Services);
    }

    [Fact]
    public void RealtimeGame_Renders_BothPlayerCards()
    {
        // Act
        var cut = Render<RealtimeGame>(parameters => parameters
            .Add(p => p.MatchId, Guid.NewGuid()));
        
        // Assert
        cut.Find(".player-card.you").Should().NotBeNull();
        cut.Find(".player-card.opponent").Should().NotBeNull();
        cut.FindComponent<Tempo.Blazor.Components.Avatars.TmAvatar>().Should().NotBeNull();
        cut.FindComponent<Tempo.Blazor.Components.DataDisplay.TmCard>().Should().NotBeNull();
    }

    [Fact]
    public void RealtimeGame_SubmitAnswer_UpdatesScore()
    {
        // Arrange
        var cut = Render<RealtimeGame>(parameters => parameters
            .Add(p => p.MatchId, Guid.NewGuid()));
        
        var input = cut.Find("input");
        
        // Act
        input.Input("TEST");
        cut.Find("button[type=submit]").Click();
        
        // Assert
        cut.Find(".score-you").TextContent.Should().Contain("0");
    }

    [Fact]
    public void RealtimeGame_OpponentAnswered_UpdatesOpponentCard()
    {
        // Arrange
        var cut = Render<RealtimeGame>(parameters => parameters
            .Add(p => p.MatchId, Guid.NewGuid()));
        
        // Act - simulate opponent progress
        _matchHubClient.OnOpponentProgress += Raise.Event<EventHandler<LexiQuest.Shared.DTOs.Multiplayer.OpponentProgressDto>>(
            this, new LexiQuest.Shared.DTOs.Multiplayer.OpponentProgressDto(5, 5, 2, 1));
        
        // Assert
        cut.Find(".score-opponent").TextContent.Should().Contain("5");
    }
}


