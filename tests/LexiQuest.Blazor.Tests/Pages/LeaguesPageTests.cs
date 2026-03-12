using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Pages;
using LexiQuest.Blazor.Services;
using LexiQuest.Shared.DTOs.Leagues;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Tempo.Blazor.Localization;
using Xunit;

namespace LexiQuest.Blazor.Tests.Pages;

public class LeaguesPageTests : BunitContext
{
    private readonly ILeagueService _leagueService;
    private readonly IStringLocalizer<Leagues> _localizer;
    private readonly ITmLocalizer _tmLocalizer;

    public LeaguesPageTests()
    {
        _leagueService = Substitute.For<ILeagueService>();
        _localizer = Substitute.For<IStringLocalizer<Leagues>>();
        _tmLocalizer = Substitute.For<ITmLocalizer>();
        
        _localizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
        _tmLocalizer[Arg.Any<string>()].Returns(ci => ci.Arg<string>());
        
        Services.AddSingleton(_leagueService);
        Services.AddSingleton(_localizer);
        Services.AddSingleton(_tmLocalizer);
    }

    [Fact]
    public void LeaguesPage_Renders_LeagueHeaderWithTier()
    {
        // Arrange
        var leagueInfo = CreateLeagueInfo(LeagueTier.Gold, 5, 1000);
        _leagueService.GetCurrentLeagueAsync().Returns(Task.FromResult<LeagueInfoDto?>(leagueInfo));
        _leagueService.GetLeaderboardAsync().Returns(Task.FromResult(new List<LeagueParticipantDto>()));

        // Act
        var cut = Render<Leagues>();

        // Assert
        cut.WaitForState(() => cut.Find(".league-header") != null);
        cut.Find(".tier-info").TextContent.Should().Contain("Gold");
    }

    [Fact]
    public void LeaguesPage_Renders_UserPositionCard()
    {
        // Arrange
        var leagueInfo = CreateLeagueInfo(LeagueTier.Silver, 3, 2500);
        _leagueService.GetCurrentLeagueAsync().Returns(Task.FromResult<LeagueInfoDto?>(leagueInfo));
        _leagueService.GetLeaderboardAsync().Returns(Task.FromResult(new List<LeagueParticipantDto>()));

        // Act
        var cut = Render<Leagues>();

        // Assert
        cut.WaitForState(() => cut.Find(".user-position-card") != null);
        var userCard = cut.Find(".user-position-card");
        userCard.Should().NotBeNull();
        userCard.TextContent.Should().Contain("3"); // Rank
        userCard.TextContent.Should().Contain("2500"); // XP
    }

    [Fact]
    public void LeaguesPage_Renders_Leaderboard()
    {
        // Arrange
        var leagueInfo = CreateLeagueInfo(LeagueTier.Bronze, 1, 5000);
        var leaderboard = CreateLeaderboard(10);
        
        _leagueService.GetCurrentLeagueAsync().Returns(Task.FromResult<LeagueInfoDto?>(leagueInfo));
        _leagueService.GetLeaderboardAsync().Returns(Task.FromResult(leaderboard));

        // Act
        var cut = Render<Leagues>();

        // Assert
        cut.WaitForState(() => cut.Find(".leaderboard") != null);
        var rows = cut.FindAll(".leaderboard-row");
        rows.Count.Should().Be(10);
    }

    [Fact]
    public void LeaguesPage_Leaderboard_HighlightsCurrentUser()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var leagueInfo = CreateLeagueInfo(LeagueTier.Diamond, 5, 3000);
        var leaderboard = new List<LeagueParticipantDto>
        {
            new(Guid.NewGuid(), "User1", null, 1, 5000, false, true, false),
            new(Guid.NewGuid(), "User2", null, 2, 4500, false, true, false),
            new(currentUserId, "CurrentUser", null, 5, 3000, true, false, false),
            new(Guid.NewGuid(), "User4", null, 10, 1000, false, false, true)
        };

        _leagueService.GetCurrentLeagueAsync().Returns(Task.FromResult<LeagueInfoDto?>(leagueInfo));
        _leagueService.GetLeaderboardAsync().Returns(Task.FromResult(leaderboard));

        // Act - would need to mock current user in practice
        var cut = Render<Leagues>();

        // Assert
        cut.WaitForState(() => cut.Find(".leaderboard") != null);
        cut.FindAll(".leaderboard-row.is-current-user").Should().HaveCount(1);
    }

    [Fact]
    public void LeaguesPage_Leaderboard_ShowsPromotionZone()
    {
        // Arrange
        var leagueInfo = CreateLeagueInfo(LeagueTier.Bronze, 7, 1500);
        var leaderboard = Enumerable.Range(1, 10)
            .Select(i => new LeagueParticipantDto(
                Guid.NewGuid(), $"User{i}", null, i, (11 - i) * 100, false, i <= 5, i > 5))
            .ToList();

        _leagueService.GetCurrentLeagueAsync().Returns(Task.FromResult<LeagueInfoDto?>(leagueInfo));
        _leagueService.GetLeaderboardAsync().Returns(Task.FromResult(leaderboard));

        // Act
        var cut = Render<Leagues>();

        // Assert
        cut.WaitForState(() => cut.Find(".leaderboard") != null);
        var promoRows = cut.FindAll(".leaderboard-row.promotion-zone");
        promoRows.Count.Should().Be(5);
    }

    [Fact]
    public void LeaguesPage_Leaderboard_ShowsDemotionZone()
    {
        // Arrange
        var leagueInfo = CreateLeagueInfo(LeagueTier.Gold, 3, 3500);
        var leaderboard = Enumerable.Range(1, 10)
            .Select(i => new LeagueParticipantDto(
                Guid.NewGuid(), $"User{i}", null, i, (11 - i) * 100, false, i <= 3, i > 5))
            .ToList();

        _leagueService.GetCurrentLeagueAsync().Returns(Task.FromResult<LeagueInfoDto?>(leagueInfo));
        _leagueService.GetLeaderboardAsync().Returns(Task.FromResult(leaderboard));

        // Act
        var cut = Render<Leagues>();

        // Assert
        cut.WaitForState(() => cut.Find(".leaderboard") != null);
        var demoRows = cut.FindAll(".leaderboard-row.demotion-zone");
        demoRows.Count.Should().Be(5);
    }

    [Fact]
    public void LeaguesPage_NotInLeague_ShowsEmptyState()
    {
        // Arrange
        _leagueService.GetCurrentLeagueAsync().Returns(Task.FromResult<LeagueInfoDto?>(null));
        _leagueService.GetLeaderboardAsync().Returns(Task.FromResult(new List<LeagueParticipantDto>()));

        // Act
        var cut = Render<Leagues>();

        // Assert
        cut.WaitForState(() => cut.Find(".empty-state") != null);
        cut.Find(".empty-state").Should().NotBeNull();
    }

    private static LeagueInfoDto CreateLeagueInfo(LeagueTier tier, int rank, int xp)
    {
        var (promoThreshold, demoThreshold) = tier == LeagueTier.Legend 
            ? (3, 11) 
            : (5, 26);

        return new LeagueInfoDto(
            LeagueId: Guid.NewGuid(),
            Tier: tier,
            WeekStart: DateTime.UtcNow.AddDays(-3),
            WeekEnd: DateTime.UtcNow.AddDays(4),
            CurrentRank: rank,
            TotalParticipants: 30,
            UserXP: xp,
            PromotionThreshold: promoThreshold,
            DemotionThreshold: demoThreshold,
            XPReward: GetXPReward(tier)
        );
    }

    private static List<LeagueParticipantDto> CreateLeaderboard(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => new LeagueParticipantDto(
                Guid.NewGuid(),
                $"User{i}",
                null,
                i,
                (count - i + 1) * 100,
                false,
                i <= 5,
                i > count - 5
            ))
            .ToList();
    }

    private static int GetXPReward(LeagueTier tier) => tier switch
    {
        LeagueTier.Bronze => 50,
        LeagueTier.Silver => 100,
        LeagueTier.Gold => 200,
        LeagueTier.Diamond => 500,
        LeagueTier.Legend => 1000,
        _ => 0
    };
}
