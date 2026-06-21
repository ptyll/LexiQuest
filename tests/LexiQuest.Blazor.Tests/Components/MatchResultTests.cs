using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components;
using LexiQuest.Shared.DTOs.Multiplayer;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using LexiQuest.Blazor.Tests.Helpers;
using Tempo.Blazor.Localization;
using Xunit;

namespace LexiQuest.Blazor.Tests.Components;

public class MatchResultTests : BunitContext
{
    private readonly IStringLocalizer<MatchResult> _localizer;

    public MatchResultTests()
    {
        _localizer = Substitute.For<IStringLocalizer<MatchResult>>();
        var tmLocalizer = Substitute.For<ITmLocalizer>();
        tmLocalizer[Arg.Any<string>()].Returns(ci => ci.Arg<string>());
        Services.AddSingleton(tmLocalizer);
        
        // Setup localizer s českými texty
        _localizer["Result_Victory_Title"].Returns(new LocalizedString("Result_Victory_Title", "🎉 VÍTĚZSTVÍ!"));
        _localizer["Result_Defeat_Title"].Returns(new LocalizedString("Result_Defeat_Title", "😔 PROHRA"));
        _localizer["Result_Draw_Title"].Returns(new LocalizedString("Result_Draw_Title", "🤝 REMÍZA!"));
        _localizer["Result_Draw_Message"].Returns(new LocalizedString("Result_Draw_Message", "Oba hráči skončili se stejným výsledkem."));
        _localizer["Result_XP_Won"].Returns(new LocalizedString("Result_XP_Won", "⭐ +{0} XP"));
        _localizer["Result_XP_League"].Returns(new LocalizedString("Result_XP_League", "📈 Liga: +{0} XP"));
        _localizer["Result_Motivation"].Returns(new LocalizedString("Result_Motivation", "💪 Příště to dáš!"));
        _localizer["Result_Speed_Tiebreaker"].Returns(new LocalizedString("Result_Speed_Tiebreaker", "Rychlejší vyhrává!"));
        _localizer["Result_Winner_Badge"].Returns(new LocalizedString("Result_Winner_Badge", "🏆"));
        _localizer["Button_NextMatch"].Returns(new LocalizedString("Button_NextMatch", "Další zápas"));
        _localizer["Button_Home"].Returns(new LocalizedString("Button_Home", "Domů"));
        _localizer["Button_Rematch"].Returns(new LocalizedString("Button_Rematch", "Hrát znovu"));
        _localizer["Button_Revenge"].Returns(new LocalizedString("Button_Revenge", "Odveta"));
        _localizer["Button_AcceptRematch"].Returns(new LocalizedString("Button_AcceptRematch", "Přijmout"));
        _localizer["Button_DeclineRematch"].Returns(new LocalizedString("Button_DeclineRematch", "Odmítnout"));
        _localizer["Rematch_Pending"].Returns(new LocalizedString("Rematch_Pending", "Čeká se na soupeře..."));
        _localizer["Rematch_Request"].Returns(new LocalizedString("Rematch_Request", "Soupeř chce odvetu."));
        _localizer["Rematch_Declined"].Returns(new LocalizedString("Rematch_Declined", "Soupeř odvetu odmítl."));
        _localizer["Player_You"].Returns(new LocalizedString("Player_You", "Vy"));
        _localizer["Player_Opponent"].Returns(new LocalizedString("Player_Opponent", "Soupeř"));
        _localizer["Score_Correct"].Returns(new LocalizedString("Score_Correct", "Správně"));
        _localizer["PrivateRoom_NoLeagueXP"].Returns(new LocalizedString("PrivateRoom_NoLeagueXP", "Soukromé místnosti nepřidávají ligové XP"));
        _localizer["Series_Score"].Returns(new LocalizedString("Series_Score", "Série: {0}:{1}"));
        
        Services.AddSingleton(_localizer);
        Services.AddSingleton<NavigationManager>(new TestNavigationManager());
        TempoTestHelper.RegisterTempoServices(Services);
    }

    [Fact]
    public void MatchResult_Victory_ShowsConfetti()
    {
        // Arrange
        var result = CreateVictoryResult(isPrivateRoom: false);
        
        // Act
        var cut = Render<MatchResult>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Result, result));
        
        // Assert
        cut.Find(".confetti-container").Should().NotBeNull();
        cut.Find(".result-title").TextContent.Should().Contain("VÍTĚZSTVÍ");
    }

    [Fact]
    public void MatchResult_Defeat_ShowsMotivation()
    {
        // Arrange
        var result = CreateDefeatResult(isPrivateRoom: false);
        
        // Act
        var cut = Render<MatchResult>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Result, result));
        
        // Assert
        cut.Find(".result-title").TextContent.Should().Contain("PROHRA");
        cut.Find(".motivation-text").TextContent.Should().Contain("Příště to dáš");
    }

    [Fact]
    public void MatchResult_Draw_ShowsDrawMessage()
    {
        // Arrange
        var result = CreateDrawResult();
        
        // Act
        var cut = Render<MatchResult>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Result, result));
        
        // Assert
        cut.Find(".result-title").TextContent.Should().Contain("REMÍZA");
        cut.Find(".draw-text").TextContent.Should().Contain("stejným výsledkem");
        cut.Markup.Should().NotContain("Rychlejší vyhrává");
    }

    [Fact]
    public void MatchResult_Victory_QuickMatch_ShowsLeagueXP()
    {
        // Arrange
        var result = CreateVictoryResult(isPrivateRoom: false);
        
        // Act
        var cut = Render<MatchResult>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Result, result));
        
        // Assert - Quick Match má zobrazit i liga XP
        cut.Find(".xp-league").Should().NotBeNull();
        cut.Find(".xp-league").TextContent.Should().Contain("+50 XP");
    }

    [Fact]
    public void MatchResult_Victory_PrivateRoom_HidesLeagueXP()
    {
        // Arrange
        var result = CreateVictoryResult(isPrivateRoom: true);
        
        // Act
        var cut = Render<MatchResult>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Result, result));
        
        // Assert - Private Room nemá zobrazit liga XP
        var leagueBadges = cut.FindAll(".xp-league");
        leagueBadges.Count.Should().Be(0);

        var noLeagueInfo = cut.Find("[data-testid='multiplayer-result-no-league-info']");
        noLeagueInfo.TextContent.Should().Contain("Soukromé místnosti nepřidávají ligové XP");
    }

    [Fact]
    public void MatchResult_Defeat_QuickMatch_ShowsRevengeButton()
    {
        // Arrange
        var result = CreateDefeatResult(isPrivateRoom: false);
        
        // Act
        var cut = Render<MatchResult>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Result, result));
        
        // Assert - Quick Match poražený má "Odveta" tlačítko
        var buttons = cut.FindAll("button");
        buttons.Any(b => b.TextContent.Contains("Odveta")).Should().BeTrue();
    }

    [Fact]
    public void MatchResult_Defeat_PrivateRoom_ShowsRematchButton()
    {
        // Arrange
        var result = CreateDefeatResult(isPrivateRoom: true);
        
        // Act
        var cut = Render<MatchResult>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Result, result));
        
        // Assert - Private Room poražený má tlačítko pro další zápas
        var buttons = cut.FindAll("button");
        buttons.Any(b => b.TextContent.Contains("Hrát znovu")).Should().BeTrue();
    }

    [Fact]
    public void MatchResult_WinnerBadge_ShownOnWinnerCard()
    {
        // Arrange
        var result = CreateVictoryResult(isPrivateRoom: false);
        
        // Act
        var cut = Render<MatchResult>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Result, result));
        
        // Assert
        var winnerBadge = cut.Find(".winner-badge");
        winnerBadge.Should().NotBeNull();
        winnerBadge.TextContent.Should().Contain("🏆");
    }

    [Fact]
    public void MatchResult_OnNextMatchClicked_InvokesCallback()
    {
        // Arrange
        var result = CreateVictoryResult(isPrivateRoom: false);
        var nextMatchClicked = false;
        
        var cut = Render<MatchResult>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Result, result)
            .Add(p => p.OnNextMatch, () => { nextMatchClicked = true; }));
        
        // Act
        var nextButton = cut.FindAll("button").First(b => b.TextContent.Contains("Další zápas"));
        nextButton.Click();
        
        // Assert
        nextMatchClicked.Should().BeTrue();
    }

    [Fact]
    public void MatchResult_OnHomeClicked_InvokesCallback()
    {
        // Arrange
        var result = CreateVictoryResult(isPrivateRoom: false);
        var homeClicked = false;
        
        var cut = Render<MatchResult>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Result, result)
            .Add(p => p.OnHome, () => { homeClicked = true; }));
        
        // Act
        var homeButton = cut.FindAll("button").First(b => b.TextContent.Contains("Domů"));
        homeButton.Click();
        
        // Assert
        homeClicked.Should().BeTrue();
    }

    [Fact]
    public void MatchResult_PrivateRoom_RematchPending_ShowsWaitingAlert()
    {
        // Arrange
        var result = CreateVictoryResult(isPrivateRoom: true);

        // Act
        var cut = Render<MatchResult>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Result, result)
            .Add(p => p.RematchRequestSent, true));

        // Assert
        cut.Find("[data-testid='multiplayer-result-rematch-pending']")
            .TextContent.Should().Contain("Čeká se na soupeře");
    }

    [Fact]
    public void MatchResult_PrivateRoom_RematchRequest_AcceptAndDeclineInvokeCallbacks()
    {
        // Arrange
        var result = CreateVictoryResult(isPrivateRoom: true);
        var accepted = false;
        var declined = false;

        // Act
        var cut = Render<MatchResult>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Result, result)
            .Add(p => p.RematchRequestedByOpponent, true)
            .Add(p => p.OnAcceptRematch, () => { accepted = true; })
            .Add(p => p.OnDeclineRematch, () => { declined = true; }));

        cut.Find("[data-testid='multiplayer-result-rematch-request']")
            .TextContent.Should().Contain("Soupeř chce odvetu");

        cut.Find("[data-testid='multiplayer-result-rematch-accept'] button").Click();
        cut.Find("[data-testid='multiplayer-result-rematch-decline'] button").Click();

        // Assert
        accepted.Should().BeTrue();
        declined.Should().BeTrue();
    }

    [Fact]
    public void MatchResult_PrivateRoom_RematchRequest_HidesDuplicateNextAction()
    {
        // Arrange
        var result = CreateDefeatResult(isPrivateRoom: true);

        // Act
        var cut = Render<MatchResult>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Result, result)
            .Add(p => p.RematchRequestedByOpponent, true));

        // Assert
        cut.FindAll("[data-testid='multiplayer-result-next']").Should().BeEmpty();
        cut.Find("[data-testid='multiplayer-result-rematch-accept']").Should().NotBeNull();
        cut.Find("[data-testid='multiplayer-result-rematch-decline']").Should().NotBeNull();
    }

    [Fact]
    public void MatchResult_PrivateRoom_RematchDeclined_ShowsDeclinedAlert()
    {
        // Arrange
        var result = CreateVictoryResult(isPrivateRoom: true);

        // Act
        var cut = Render<MatchResult>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Result, result)
            .Add(p => p.RematchDeclined, true));

        // Assert
        cut.Find("[data-testid='multiplayer-result-rematch-declined']")
            .TextContent.Should().Contain("odvetu odmítl");
    }

    [Fact]
    public void MatchResult_PrivateRoom_BestOf_ShowsSeriesScore()
    {
        // Arrange
        var result = CreateVictoryResult(isPrivateRoom: true);

        // Act
        var cut = Render<MatchResult>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Result, result)
            .Add(p => p.SeriesPlayer1Wins, 2)
            .Add(p => p.SeriesPlayer2Wins, 1)
            .Add(p => p.BestOf, 3));

        // Assert - Should show series score for Best of 3
        var seriesScore = cut.Find(".series-score");
        seriesScore.Should().NotBeNull();
        seriesScore.TextContent.Should().Contain("2");
        seriesScore.TextContent.Should().Contain("1");
    }

    private static MatchResultDto CreateVictoryResult(bool isPrivateRoom)
    {
        var winnerId = Guid.NewGuid();
        return new MatchResultDto(
            WinnerId: winnerId,
            YourScore: 150,
            OpponentScore: 100,
            YourTime: TimeSpan.FromSeconds(120),
            OpponentTime: TimeSpan.FromSeconds(150),
            XPEarned: 100,
            LeagueXPEarned: isPrivateRoom ? 0 : 50,
            IsDraw: false,
            IsPrivateRoom: isPrivateRoom,
            RoomCode: isPrivateRoom ? "LEXIQ-ABCD" : null,
            YourResult: new PlayerMatchResult(
                Username: "Vy",
                Avatar: null,
                CorrectCount: 12,
                TotalTime: TimeSpan.FromSeconds(120),
                ComboMax: 5,
                XPEarned: 100
            ),
            OpponentResult: new PlayerMatchResult(
                Username: "Soupeř",
                Avatar: null,
                CorrectCount: 8,
                TotalTime: TimeSpan.FromSeconds(150),
                ComboMax: 3,
                XPEarned: 30
            )
        );
    }

    private static MatchResultDto CreateDefeatResult(bool isPrivateRoom)
    {
        var opponentId = Guid.NewGuid();
        return new MatchResultDto(
            WinnerId: opponentId,
            YourScore: 80,
            OpponentScore: 140,
            YourTime: TimeSpan.FromSeconds(180),
            OpponentTime: TimeSpan.FromSeconds(130),
            XPEarned: 30,
            LeagueXPEarned: isPrivateRoom ? 0 : 15,
            IsDraw: false,
            IsPrivateRoom: isPrivateRoom,
            RoomCode: isPrivateRoom ? "LEXIQ-ABCD" : null,
            YourResult: new PlayerMatchResult(
                Username: "Vy",
                Avatar: null,
                CorrectCount: 5,
                TotalTime: TimeSpan.FromSeconds(180),
                ComboMax: 2,
                XPEarned: 30
            ),
            OpponentResult: new PlayerMatchResult(
                Username: "Soupeř",
                Avatar: null,
                CorrectCount: 10,
                TotalTime: TimeSpan.FromSeconds(130),
                ComboMax: 4,
                XPEarned: 100
            )
        );
    }

    private static MatchResultDto CreateDrawResult()
    {
        return new MatchResultDto(
            WinnerId: null,
            YourScore: 100,
            OpponentScore: 100,
            YourTime: TimeSpan.FromSeconds(100),
            OpponentTime: TimeSpan.FromSeconds(120),
            XPEarned: 50,
            LeagueXPEarned: 25,
            IsDraw: true,
            IsPrivateRoom: false,
            RoomCode: null,
            YourResult: new PlayerMatchResult(
                Username: "Vy",
                Avatar: null,
                CorrectCount: 8,
                TotalTime: TimeSpan.FromSeconds(100),
                ComboMax: 3,
                XPEarned: 50
            ),
            OpponentResult: new PlayerMatchResult(
                Username: "Soupeř",
                Avatar: null,
                CorrectCount: 8,
                TotalTime: TimeSpan.FromSeconds(120),
                ComboMax: 3,
                XPEarned: 50
            )
        );
    }
}
