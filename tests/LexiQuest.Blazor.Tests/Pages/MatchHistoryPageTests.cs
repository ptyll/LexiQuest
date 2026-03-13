using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components;
using LexiQuest.Blazor.Pages;
using LexiQuest.Shared.DTOs.Multiplayer;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using LexiQuest.Blazor.Tests.Helpers;
using Xunit;

namespace LexiQuest.Blazor.Tests.Pages;

public class MatchHistoryPageTests : BunitContext
{
    private readonly IStringLocalizer<LexiQuest.Blazor.Pages.Multiplayer> _localizer;
    private readonly NavigationManager _navigationManager;

    public MatchHistoryPageTests()
    {
        _localizer = Substitute.For<IStringLocalizer<LexiQuest.Blazor.Pages.Multiplayer>>();
        SetupLocalizer();
        
        _navigationManager = new TestNavigationManager();
        Services.AddSingleton(_localizer);
        Services.AddSingleton(_navigationManager);
        Services.AddSingleton(Substitute.For<LexiQuest.Blazor.Services.IMatchHistoryClient>());
        TempoTestHelper.RegisterTempoServices(Services);
    }

    private void SetupLocalizer()
    {
        _localizer["MatchHistory_Title"].Returns(new LocalizedString("MatchHistory_Title", "Historie zápasů"));
        _localizer["MatchHistory_Empty"].Returns(new LocalizedString("MatchHistory_Empty", "Zatím žádné zápasy"));
        _localizer["MatchHistory_Empty_Description"].Returns(new LocalizedString("MatchHistory_Empty_Description", "Zahraj si svůj první multiplayer zápas!"));
        _localizer["MatchHistory_Tab_All"].Returns(new LocalizedString("MatchHistory_Tab_All", "Vše"));
        _localizer["MatchHistory_Tab_QuickMatch"].Returns(new LocalizedString("MatchHistory_Tab_QuickMatch", "⚔️ Quick Match"));
        _localizer["MatchHistory_Tab_PrivateRoom"].Returns(new LocalizedString("MatchHistory_Tab_PrivateRoom", "🏠 Private Room"));
        _localizer["MatchHistory_Stats_Played"].Returns(new LocalizedString("MatchHistory_Stats_Played", "Odehráno"));
        _localizer["MatchHistory_Stats_Wins"].Returns(new LocalizedString("MatchHistory_Stats_Wins", "Výhry"));
        _localizer["MatchHistory_Stats_Losses"].Returns(new LocalizedString("MatchHistory_Stats_Losses", "Prohry"));
        _localizer["MatchHistory_Stats_WinRate"].Returns(new LocalizedString("MatchHistory_Stats_WinRate", "Win Rate"));
        _localizer["MatchHistory_Result_Win"].Returns(new LocalizedString("MatchHistory_Result_Win", "Výhra"));
        _localizer["MatchHistory_Result_Loss"].Returns(new LocalizedString("MatchHistory_Result_Loss", "Prohra"));
        _localizer["MatchHistory_Result_Draw"].Returns(new LocalizedString("MatchHistory_Result_Draw", "Remíza"));
        _localizer["MatchHistory_Today"].Returns(new LocalizedString("MatchHistory_Today", "Dnes"));
        _localizer["MatchHistory_Yesterday"].Returns(new LocalizedString("MatchHistory_Yesterday", "Včera"));
        _localizer["MatchHistory_ThisWeek"].Returns(new LocalizedString("MatchHistory_ThisWeek", "Tento týden"));
        _localizer["MatchHistory_Older"].Returns(new LocalizedString("MatchHistory_Older", "Starší"));
        _localizer["MatchHistory_Type_Quick"].Returns(new LocalizedString("MatchHistory_Type_Quick", "⚔️ Quick"));
        _localizer["MatchHistory_Type_Private"].Returns(new LocalizedString("MatchHistory_Type_Private", "🏠 Private"));
        _localizer["MatchHistory_Duration"].Returns(new LocalizedString("MatchHistory_Duration", "Délka"));
        _localizer["MatchHistory_XP"].Returns(new LocalizedString("MatchHistory_XP", "XP"));
        _localizer["MatchHistory_Back"].Returns(new LocalizedString("MatchHistory_Back", "Zpět"));
        _localizer["MatchHistory_Link"].Returns(new LocalizedString("MatchHistory_Link", "Historie zápasů"));
        _localizer["PrivateRoom_NoLeagueXP"].Returns(new LocalizedString("PrivateRoom_NoLeagueXP", "Bez liga XP"));
        _localizer["QuickMatch_LeagueXP"].Returns(new LocalizedString("QuickMatch_LeagueXP", "Liga XP ✓"));
        _localizer["Room_Series_Score"].Returns(new LocalizedString("Room_Series_Score", "Série: {0}:{1}"));
    }

    [Fact]
    public void MatchHistory_Renders_TitleAndTabs()
    {
        // Arrange & Act
        var cut = Render<MatchHistory>();

        // Assert
        cut.Find("h1").TextContent.Should().Contain("Historie");
        cut.FindAll("button").Any(b => b.TextContent.Contains("Vše")).Should().BeTrue();
        cut.FindAll("button").Any(b => b.TextContent.Contains("Quick Match")).Should().BeTrue();
        cut.FindAll("button").Any(b => b.TextContent.Contains("Private Room")).Should().BeTrue();
    }

    [Fact]
    public void MatchHistory_Renders_StatsCards()
    {
        // Arrange & Act
        var cut = Render<MatchHistory>();

        // Assert
        cut.FindAll(".stat-card").Count.Should().Be(4);
        cut.FindAll(".stat-label").Any(l => l.TextContent.Contains("Odehráno")).Should().BeTrue();
        cut.FindAll(".stat-label").Any(l => l.TextContent.Contains("Výhry")).Should().BeTrue();
        cut.FindAll(".stat-label").Any(l => l.TextContent.Contains("Prohry")).Should().BeTrue();
        cut.FindAll(".stat-label").Any(l => l.TextContent.Contains("Win Rate")).Should().BeTrue();
    }

    [Fact]
    public void MatchHistory_WithMatches_RendersMatchList()
    {
        // Arrange
        var cut = Render<MatchHistory>();
        var matches = CreateSampleMatches();
        cut.Instance.LoadTestData(matches, CreateSampleStats());

        // Act - rerender
        cut.Render();

        // Assert
        cut.FindAll(".match-entry").Count.Should().Be(3);
    }

    [Fact]
    public void MatchHistory_EmptyState_ShowsEmptyMessage()
    {
        // Arrange
        var cut = Render<MatchHistory>();
        cut.Instance.LoadTestData(new List<MatchHistoryEntryDto>(), CreateEmptyStats());

        // Act
        cut.Render();

        // Assert
        cut.Find(".empty-state").TextContent.Should().Contain("Zatím žádné zápasy");
    }

    [Fact]
    public void MatchHistory_TabFilter_Click_ChangesFilter()
    {
        // Arrange
        var cut = Render<MatchHistory>();
        var quickMatchTab = cut.FindAll("button").First(b => b.TextContent.Contains("Quick Match"));

        // Act
        quickMatchTab.Click();

        // Assert
        cut.Instance.CurrentFilter.Should().Be(MatchHistoryFilter.QuickMatch);
    }

    [Fact]
    public void MatchHistory_MatchEntry_ShowsCorrectResultBadge()
    {
        // Arrange
        var cut = Render<MatchHistory>();
        var matches = new List<MatchHistoryEntryDto>
        {
            new MatchHistoryEntryDto(
                MatchId: Guid.NewGuid(),
                OpponentUsername: "Opponent1",
                OpponentAvatar: null,
                YourScore: 10,
                OpponentScore: 5,
                Result: MatchResultType.Win,
                XPEarned: 100,
                Duration: TimeSpan.FromMinutes(3),
                PlayedAt: DateTime.UtcNow,
                Type: LexiQuest.Shared.DTOs.Multiplayer.MatchType.QuickMatch,
                RoomCode: null,
                SeriesScoreYou: null,
                SeriesScoreOpponent: null
            )
        };
        cut.Instance.LoadTestData(matches, CreateSampleStats());
        cut.Render();

        // Assert
        cut.Find(".result-badge").TextContent.Should().Contain("Výhra");
        cut.Find(".match-entry").ClassList.Should().Contain("win");
    }

    [Fact]
    public void MatchHistory_PrivateRoom_ShowsSeriesScore()
    {
        // Arrange
        var cut = Render<MatchHistory>();
        var matches = new List<MatchHistoryEntryDto>
        {
            new MatchHistoryEntryDto(
                MatchId: Guid.NewGuid(),
                OpponentUsername: "Opponent1",
                OpponentAvatar: null,
                YourScore: 10,
                OpponentScore: 8,
                Result: MatchResultType.Win,
                XPEarned: 100,
                Duration: TimeSpan.FromMinutes(2),
                PlayedAt: DateTime.UtcNow,
                Type: global::LexiQuest.Shared.DTOs.Multiplayer.MatchType.PrivateRoom,
                RoomCode: "LEXIQ-ABCD",
                SeriesScoreYou: 2,
                SeriesScoreOpponent: 1
            )
        };
        cut.Instance.LoadTestData(matches, CreateSampleStats());
        cut.Render();

        // Assert
        cut.Find(".series-score").TextContent.Should().Contain("Série: 2:1");
    }

    [Fact]
    public void MatchHistory_BackButton_NavigatesToMultiplayer()
    {
        // Arrange
        var cut = Render<MatchHistory>();
        var backButton = cut.FindAll("button").First(b => b.TextContent.Contains("Zpět") || b.TextContent.Contains("Back"));

        // Act
        backButton.Click();

        // Assert
        _navigationManager.Uri.Should().Contain("/multiplayer");
    }

    private static List<MatchHistoryEntryDto> CreateSampleMatches()
    {
        return new List<MatchHistoryEntryDto>
        {
            new MatchHistoryEntryDto(
                MatchId: Guid.NewGuid(),
                OpponentUsername: "Opponent1",
                OpponentAvatar: null,
                YourScore: 10,
                OpponentScore: 5,
                Result: MatchResultType.Win,
                XPEarned: 100,
                Duration: TimeSpan.FromMinutes(3),
                PlayedAt: DateTime.UtcNow,
                Type: LexiQuest.Shared.DTOs.Multiplayer.MatchType.QuickMatch,
                RoomCode: null,
                SeriesScoreYou: null,
                SeriesScoreOpponent: null
            ),
            new MatchHistoryEntryDto(
                MatchId: Guid.NewGuid(),
                OpponentUsername: "Opponent2",
                OpponentAvatar: null,
                YourScore: 5,
                OpponentScore: 10,
                Result: MatchResultType.Loss,
                XPEarned: 30,
                Duration: TimeSpan.FromMinutes(3),
                PlayedAt: DateTime.UtcNow.AddDays(-1),
                Type: LexiQuest.Shared.DTOs.Multiplayer.MatchType.QuickMatch,
                RoomCode: null,
                SeriesScoreYou: null,
                SeriesScoreOpponent: null
            ),
            new MatchHistoryEntryDto(
                MatchId: Guid.NewGuid(),
                OpponentUsername: "Opponent3",
                OpponentAvatar: null,
                YourScore: 8,
                OpponentScore: 8,
                Result: MatchResultType.Draw,
                XPEarned: 50,
                Duration: TimeSpan.FromMinutes(2),
                PlayedAt: DateTime.UtcNow.AddDays(-2),
                Type: global::LexiQuest.Shared.DTOs.Multiplayer.MatchType.PrivateRoom,
                RoomCode: "LEXIQ-TEST",
                SeriesScoreYou: 1,
                SeriesScoreOpponent: 1
            )
        };
    }

    private static MultiplayerStatsDto CreateSampleStats()
    {
        return new MultiplayerStatsDto(
            TotalMatchesPlayed: 10,
            Wins: 6,
            Losses: 3,
            Draws: 1,
            WinRatePercentage: 60.0,
            TotalXPEarned: 750,
            QuickMatchStats: new MatchTypeStats(7, 4, 2, 1, 57.1),
            PrivateRoomStats: new MatchTypeStats(3, 2, 1, 0, 66.7)
        );
    }

    private static MultiplayerStatsDto CreateEmptyStats()
    {
        return new MultiplayerStatsDto(
            TotalMatchesPlayed: 0,
            Wins: 0,
            Losses: 0,
            Draws: 0,
            WinRatePercentage: 0,
            TotalXPEarned: 0,
            QuickMatchStats: new MatchTypeStats(0, 0, 0, 0, 0),
            PrivateRoomStats: new MatchTypeStats(0, 0, 0, 0, 0)
        );
    }
}
