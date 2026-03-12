using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using Xunit;

namespace LexiQuest.Core.Tests.Domain.Entities;

public class TeamMemberTests
{
    [Fact]
    public void TeamMember_Create_DefaultRoleMember()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var teamId = Guid.NewGuid();

        // Act
        var member = TeamMember.Create(userId, teamId);

        // Assert
        member.UserId.Should().Be(userId);
        member.TeamId.Should().Be(teamId);
        member.Role.Should().Be(TeamRole.Member);
        member.JoinedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void TeamMember_Create_WithSpecifiedRole()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var teamId = Guid.NewGuid();

        // Act
        var leader = TeamMember.Create(userId, teamId, TeamRole.Leader);
        var officer = TeamMember.Create(userId, teamId, TeamRole.Officer);
        var member = TeamMember.Create(userId, teamId, TeamRole.Member);

        // Assert
        leader.Role.Should().Be(TeamRole.Leader);
        officer.Role.Should().Be(TeamRole.Officer);
        member.Role.Should().Be(TeamRole.Member);
    }

    [Fact]
    public void TeamMember_UpdateRole_ChangesRole()
    {
        // Arrange
        var member = TeamMember.Create(Guid.NewGuid(), Guid.NewGuid(), TeamRole.Member);

        // Act
        member.UpdateRole(TeamRole.Officer);

        // Assert
        member.Role.Should().Be(TeamRole.Officer);
    }

    [Fact]
    public void TeamMember_UpdateWeeklyXP_UpdatesValue()
    {
        // Arrange
        var member = TeamMember.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        member.AddWeeklyXP(100);

        // Assert
        member.WeeklyXP.Should().Be(100);
    }

    [Fact]
    public void TeamMember_ResetWeeklyXP_ResetsToZero()
    {
        // Arrange
        var member = TeamMember.Create(Guid.NewGuid(), Guid.NewGuid());
        member.AddWeeklyXP(500);

        // Act
        member.ResetWeeklyXP();

        // Assert
        member.WeeklyXP.Should().Be(0);
    }

    [Fact]
    public void TeamMember_AddToAllTimeXP_Accumulates()
    {
        // Arrange
        var member = TeamMember.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        member.AddToAllTimeXP(100);
        member.AddToAllTimeXP(50);

        // Assert
        member.AllTimeXP.Should().Be(150);
    }
}
