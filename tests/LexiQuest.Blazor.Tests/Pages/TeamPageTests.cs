using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Pages;
using LexiQuest.Blazor.Services;
using LexiQuest.Shared.DTOs.Teams;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Tempo.Blazor.Localization;
using Xunit;

namespace LexiQuest.Blazor.Tests.Pages;

public class TeamPageTests : BunitContext
{
    private readonly ITeamService _teamService;
    private readonly IStringLocalizer<Team> _localizer;
    private readonly ITmLocalizer _tmLocalizer;

    public TeamPageTests()
    {
        _teamService = Substitute.For<ITeamService>();
        _localizer = Substitute.For<IStringLocalizer<Team>>();
        _tmLocalizer = Substitute.For<ITmLocalizer>();

        _localizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
        _tmLocalizer[Arg.Any<string>()].Returns(ci => ci.Arg<string>());

        // Default mocks to prevent NullReferenceException
        _teamService.GetTeamMembersAsync(Arg.Any<Guid>()).Returns(Task.FromResult(new List<TeamMemberDto>()));
        _teamService.GetMyInvitesAsync().Returns(Task.FromResult(new List<TeamInviteDto>()));
        _teamService.GetJoinRequestsAsync(Arg.Any<Guid>()).Returns(Task.FromResult(new List<TeamJoinRequestDto>()));
        _teamService.GetRankingAsync().Returns(Task.FromResult(new List<TeamRankingDto>()));

        Services.AddSingleton(_teamService);
        Services.AddSingleton(_localizer);
        Services.AddSingleton(_tmLocalizer);
        Services.AddSingleton<NavigationManager>(new TestNavigationManager());
    }

    [Fact]
    public void TeamPage_NoTeam_ShowsCreateOrJoin()
    {
        // Arrange
        _teamService.GetMyTeamAsync().Returns(Task.FromResult<TeamDto?>(null));
        _teamService.GetMyInvitesAsync().Returns(Task.FromResult(new List<TeamInviteDto>()));
        _teamService.GetRankingAsync().Returns(Task.FromResult(new List<TeamRankingDto>()));

        // Act
        var cut = Render<Team>();

        // Assert
        cut.WaitForState(() => cut.Find(".empty-state") != null);
        cut.Find(".empty-state").Should().NotBeNull();
        var buttons = cut.FindAll("button");
        buttons.Any(b => b.TextContent.Contains("Button_CreateTeam")).Should().BeTrue();
        buttons.Any(b => b.TextContent.Contains("Button_SearchTeam")).Should().BeTrue();
    }

    [Fact]
    public void TeamPage_HasTeam_ShowsDashboard()
    {
        // Arrange
        var team = CreateTeamDto();
        _teamService.GetMyTeamAsync().Returns(Task.FromResult<TeamDto?>(team));
        _teamService.GetRankingAsync().Returns(Task.FromResult(new List<TeamRankingDto>()));
        _teamService.GetJoinRequestsAsync(Arg.Any<Guid>()).Returns(Task.FromResult(new List<TeamJoinRequestDto>()));

        // Act
        var cut = Render<Team>();

        // Assert
        cut.WaitForState(() => cut.Find(".team-dashboard") != null);
        cut.Find(".team-dashboard").Should().NotBeNull();
        cut.Find(".team-header").TextContent.Should().Contain("TestTeam");
        cut.Find(".team-tag").TextContent.Should().Contain("TEST");
    }

    [Fact]
    public void TeamPage_Leader_ShowsManagementOptions()
    {
        // Arrange
        var leaderId = Guid.NewGuid();
        var team = CreateTeamDto(leaderId: leaderId, isCurrentUserLeader: true);
        _teamService.GetMyTeamAsync().Returns(Task.FromResult<TeamDto?>(team));
        _teamService.GetRankingAsync().Returns(Task.FromResult(new List<TeamRankingDto>()));
        _teamService.GetJoinRequestsAsync(Arg.Any<Guid>()).Returns(Task.FromResult(new List<TeamJoinRequestDto>()));

        // Act
        var cut = Render<Team>();

        // Assert
        cut.WaitForState(() => cut.Find(".team-dashboard") != null);
        var buttons = cut.FindAll("button");
        buttons.Any(b => b.TextContent.Contains("Button_Invite")).Should().BeTrue();
        buttons.Any(b => b.TextContent.Contains("Button_DisbandTeam")).Should().BeTrue();
    }

    private static TeamDto CreateTeamDto(Guid? leaderId = null, bool isCurrentUserLeader = false)
    {
        var leader = leaderId ?? Guid.NewGuid();
        return new TeamDto(
            Id: Guid.NewGuid(),
            Name: "TestTeam",
            Tag: "TEST",
            Description: "A test team",
            LogoUrl: null,
            LeaderId: leader,
            LeaderUsername: "Leader",
            CreatedAt: DateTime.UtcNow.AddDays(-7),
            MemberCount: 5,
            Stats: new TeamStatsDto(
                WeeklyXP: 5000,
                AllTimeXP: 25000,
                Rank: 3,
                TotalWins: 42,
                MatchesPlayed: 100,
                WinRatePercentage: 42
            )
        );
    }
}
