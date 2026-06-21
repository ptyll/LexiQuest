using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Pages;
using LexiQuest.Blazor.Services;
using LexiQuest.Shared.DTOs.Teams;
using LexiQuest.Shared.DTOs.Users;
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
    private readonly IUserService _userService;
    private readonly IPremiumService _premiumService;
    private readonly IShopService _shopService;
    private readonly IStringLocalizer<Team> _localizer;
    private readonly ITmLocalizer _tmLocalizer;

    public TeamPageTests()
    {
        _teamService = Substitute.For<ITeamService>();
        _userService = Substitute.For<IUserService>();
        _premiumService = Substitute.For<IPremiumService>();
        _shopService = Substitute.For<IShopService>();
        _localizer = Substitute.For<IStringLocalizer<Team>>();
        _tmLocalizer = Substitute.For<ITmLocalizer>();

        _localizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
        _tmLocalizer[Arg.Any<string>()].Returns(ci => ci.Arg<string>());

        // Default mocks to prevent NullReferenceException
        _teamService.GetTeamMembersAsync(Arg.Any<Guid>()).Returns(Task.FromResult(new List<TeamMemberDto>()));
        _teamService.GetMyInvitesAsync().Returns(Task.FromResult(new List<TeamInviteDto>()));
        _teamService.GetJoinRequestsAsync(Arg.Any<Guid>()).Returns(Task.FromResult(new List<TeamJoinRequestDto>()));
        _teamService.GetRankingAsync().Returns(Task.FromResult(new List<TeamRankingDto>()));
        _teamService.CanCreateTeamAsync().Returns(Task.FromResult(true));
        _userService.GetProfileAsync().Returns(Task.FromResult<UserProfileDto?>(new UserProfileDto
        {
            Id = Guid.NewGuid(),
            Username = "CurrentUser",
            Email = "current@example.test"
        }));
        _premiumService.IsPremiumAsync().Returns(Task.FromResult(false));
        _shopService.GetUserCoinsAsync().Returns(Task.FromResult(0));

        Services.AddSingleton(_teamService);
        Services.AddSingleton(_userService);
        Services.AddSingleton(_premiumService);
        Services.AddSingleton(_shopService);
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
        cut.Find("[data-testid='team-page']").Should().NotBeNull();
        cut.Find("[data-testid='team-empty-state']").Should().NotBeNull();
        cut.Find("[data-testid='team-empty-title']").TextContent.Should().Contain("NoTeam_Title");
        cut.Find("[data-testid='team-empty-description']").TextContent.Should().Contain("NoTeam_Description");
        cut.Find("[data-testid='team-create']").Should().NotBeNull();
        cut.Find("[data-testid='team-search']").Should().NotBeNull();

        var buttons = cut.FindAll("button");
        buttons.Any(b => b.TextContent.Contains("Button_CreateTeam")).Should().BeTrue();
        buttons.Any(b => b.TextContent.Contains("Button_SearchTeam")).Should().BeTrue();
    }

    [Fact]
    public void TeamPage_CreateTeamClick_ShowsCreateModalWithPremiumCost()
    {
        // Arrange
        _teamService.GetMyTeamAsync().Returns(Task.FromResult<TeamDto?>(null));
        _premiumService.IsPremiumAsync().Returns(Task.FromResult(true));

        // Act
        var cut = Render<Team>();
        cut.WaitForState(() => cut.Find("[data-testid='team-empty-state']") != null);
        cut.Find("[data-testid='team-create']").Click();

        // Assert
        cut.Find("[data-testid='team-create-modal']").Should().NotBeNull();
        cut.Find("[data-testid='team-create-cost']").TextContent.Should().Contain("Create_Cost_Premium");
        cut.Find("[data-testid='team-create-name']").Should().NotBeNull();
        cut.Find("[data-testid='team-create-tag']").Should().NotBeNull();
        cut.Find("[data-testid='team-create-submit']").Should().NotBeNull();
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
        cut.Find("[data-testid='team-dashboard']").Should().NotBeNull();
        cut.Find(".team-header").TextContent.Should().Contain("TestTeam");
        cut.Find(".team-tag").TextContent.Should().Contain("TEST");
        cut.Find("[data-testid='team-dashboard-description']").TextContent.Should().Contain("A test team");
        cut.Find("[data-testid='team-stats-weekly-xp']").TextContent.Should().Contain("5000");
        cut.Find("[data-testid='team-stats-alltime-xp']").TextContent.Should().Contain("25000");
        cut.Find("[data-testid='team-stats-rank']").TextContent.Should().Contain("#3");
        cut.Find("[data-testid='team-stats-wins']").TextContent.Should().Contain("42");
    }

    [Fact]
    public void TeamPage_Leader_ShowsManagementOptions()
    {
        // Arrange
        var leaderId = Guid.NewGuid();
        var team = CreateTeamDto(leaderId: leaderId, isCurrentUserLeader: true);
        _userService.GetProfileAsync().Returns(Task.FromResult<UserProfileDto?>(new UserProfileDto
        {
            Id = leaderId,
            Username = "Leader",
            Email = "leader@example.test"
        }));
        _teamService.GetMyTeamAsync().Returns(Task.FromResult<TeamDto?>(team));
        _teamService.GetTeamMembersAsync(team.Id).Returns(Task.FromResult(new List<TeamMemberDto>
        {
            new(leaderId, "Leader", null, TeamRoleDto.Leader, DateTime.UtcNow, 100, 500)
        }));
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

    [Fact]
    public void TeamPage_Officer_ShowsKickOnlyForRegularMembers()
    {
        // Arrange
        var leaderId = Guid.NewGuid();
        var officerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var team = CreateTeamDto(leaderId: leaderId);
        _userService.GetProfileAsync().Returns(Task.FromResult<UserProfileDto?>(new UserProfileDto
        {
            Id = officerId,
            Username = "Officer",
            Email = "officer@example.test"
        }));
        _teamService.GetMyTeamAsync().Returns(Task.FromResult<TeamDto?>(team));
        _teamService.GetTeamMembersAsync(team.Id).Returns(Task.FromResult(new List<TeamMemberDto>
        {
            new(leaderId, "Leader", null, TeamRoleDto.Leader, DateTime.UtcNow, 100, 500),
            new(officerId, "Officer", null, TeamRoleDto.Officer, DateTime.UtcNow, 80, 300),
            new(memberId, "Member", null, TeamRoleDto.Member, DateTime.UtcNow, 30, 120)
        }));
        _teamService.GetRankingAsync().Returns(Task.FromResult(new List<TeamRankingDto>()));
        _teamService.GetJoinRequestsAsync(Arg.Any<Guid>()).Returns(Task.FromResult(new List<TeamJoinRequestDto>()));

        // Act
        var cut = Render<Team>();

        // Assert
        cut.WaitForState(() => cut.Find("[data-testid='team-dashboard']") != null);
        var rows = cut.FindAll("[data-testid='team-member-row']");
        rows.Single(row => row.TextContent.Contains("Leader"))
            .QuerySelectorAll("[data-testid='team-member-kick']")
            .Should().BeEmpty();
        rows.Single(row => row.TextContent.Contains("Officer"))
            .QuerySelectorAll("[data-testid='team-member-kick']")
            .Should().BeEmpty();
        rows.Single(row => row.TextContent.Contains("Member"))
            .QuerySelectorAll("[data-testid='team-member-kick']")
            .Should().ContainSingle();
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
