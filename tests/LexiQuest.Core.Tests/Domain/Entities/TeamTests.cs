using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using Xunit;

namespace LexiQuest.Core.Tests.Domain.Entities;

public class TeamTests
{
    [Fact]
    public void Team_Create_SetsDefaults()
    {
        // Arrange
        var leaderId = Guid.NewGuid();
        var name = "Test Team";
        var tag = "TT";

        // Act
        var team = Team.Create(name, tag, leaderId);

        // Assert
        team.Name.Should().Be(name);
        team.Tag.Should().Be(tag);
        team.LeaderId.Should().Be(leaderId);
        team.Description.Should().BeNull();
        team.LogoUrl.Should().BeNull();
        team.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        team.Members.Should().HaveCount(1);
        team.Members.First().UserId.Should().Be(leaderId);
        team.Members.First().Role.Should().Be(TeamRole.Leader);
    }

    [Fact]
    public void Team_AddMember_IncreasesMemberCount()
    {
        // Arrange
        var leaderId = Guid.NewGuid();
        var team = Team.Create("Test Team", "TT", leaderId);
        var newMemberId = Guid.NewGuid();

        // Act
        team.AddMember(newMemberId, TeamRole.Member);

        // Assert
        team.Members.Should().HaveCount(2);
        team.Members.Should().Contain(m => m.UserId == newMemberId && m.Role == TeamRole.Member);
    }

    [Fact]
    public void Team_AddMember_Max20_Throws()
    {
        // Arrange
        var leaderId = Guid.NewGuid();
        var team = Team.Create("Test Team", "TT", leaderId);

        // Add 19 more members to reach limit
        for (int i = 0; i < 19; i++)
        {
            team.AddMember(Guid.NewGuid(), TeamRole.Member);
        }

        // Act & Assert
        var action = () => team.AddMember(Guid.NewGuid(), TeamRole.Member);
        action.Should().Throw<InvalidOperationException>().WithMessage("*maximum member limit*");
    }

    [Fact]
    public void Team_AddMember_DuplicateUser_Throws()
    {
        // Arrange
        var leaderId = Guid.NewGuid();
        var team = Team.Create("Test Team", "TT", leaderId);
        var memberId = Guid.NewGuid();
        team.AddMember(memberId, TeamRole.Member);

        // Act & Assert
        var action = () => team.AddMember(memberId, TeamRole.Member);
        action.Should().Throw<InvalidOperationException>().WithMessage("*already a member*");
    }

    [Fact]
    public void Team_Name_3to30Chars()
    {
        // Arrange & Act & Assert
        Team.Create("AB", "TT", Guid.NewGuid()).Should().BeNull(); // Too short
        Team.Create("ABC", "TT", Guid.NewGuid()).Should().NotBeNull(); // Min length
        Team.Create(new string('A', 30), "TT", Guid.NewGuid()).Should().NotBeNull(); // Max length
        Team.Create(new string('A', 31), "TT", Guid.NewGuid()).Should().BeNull(); // Too long
    }

    [Fact]
    public void Team_Tag_2to4Chars()
    {
        // Arrange & Act & Assert
        Team.Create("Test", "T", Guid.NewGuid()).Should().BeNull(); // Too short
        Team.Create("Test", "TT", Guid.NewGuid()).Should().NotBeNull(); // Min length
        Team.Create("Test", "TTTT", Guid.NewGuid()).Should().NotBeNull(); // Max length
        Team.Create("Test", "TTTTT", Guid.NewGuid()).Should().BeNull(); // Too long
    }

    [Fact]
    public void Team_Tag_MustBeUppercaseAlphanumeric()
    {
        // Arrange & Act & Assert
        Team.Create("Test", "TT", Guid.NewGuid()).Should().NotBeNull(); // Valid
        Team.Create("Test", "tt", Guid.NewGuid()).Should().BeNull(); // Lowercase
        Team.Create("Test", "T1", Guid.NewGuid()).Should().NotBeNull(); // Alphanumeric valid
        Team.Create("Test", "T-", Guid.NewGuid()).Should().BeNull(); // Special char
    }

    [Fact]
    public void Team_RemoveMember_RemovesFromList()
    {
        // Arrange
        var leaderId = Guid.NewGuid();
        var team = Team.Create("Test Team", "TT", leaderId);
        var memberId = Guid.NewGuid();
        team.AddMember(memberId, TeamRole.Member);

        // Act
        team.RemoveMember(memberId);

        // Assert
        team.Members.Should().HaveCount(1);
        team.Members.Should().NotContain(m => m.UserId == memberId);
    }

    [Fact]
    public void Team_RemoveMember_LeaderCannotBeRemoved()
    {
        // Arrange
        var leaderId = Guid.NewGuid();
        var team = Team.Create("Test Team", "TT", leaderId);

        // Act & Assert
        var action = () => team.RemoveMember(leaderId);
        action.Should().Throw<InvalidOperationException>().WithMessage("*leader cannot be removed*");
    }

    [Fact]
    public void Team_TransferLeadership_ChangesLeader()
    {
        // Arrange
        var leaderId = Guid.NewGuid();
        var team = Team.Create("Test Team", "TT", leaderId);
        var newLeaderId = Guid.NewGuid();
        team.AddMember(newLeaderId, TeamRole.Officer);

        // Act
        team.TransferLeadership(newLeaderId);

        // Assert
        team.LeaderId.Should().Be(newLeaderId);
        team.Members.First(m => m.UserId == newLeaderId).Role.Should().Be(TeamRole.Leader);
        team.Members.First(m => m.UserId == leaderId).Role.Should().Be(TeamRole.Officer);
    }

    [Fact]
    public void Team_TransferLeadership_ToNonMember_Throws()
    {
        // Arrange
        var leaderId = Guid.NewGuid();
        var team = Team.Create("Test Team", "TT", leaderId);

        // Act & Assert
        var action = () => team.TransferLeadership(Guid.NewGuid());
        action.Should().Throw<InvalidOperationException>().WithMessage("*member of the team*");
    }

    [Fact]
    public void Team_UpdateDetails_UpdatesProperties()
    {
        // Arrange
        var team = Team.Create("Test Team", "TT", Guid.NewGuid());

        // Act
        team.UpdateDetails("New Name", "New Description", "https://logo.png");

        // Assert
        team.Name.Should().Be("New Name");
        team.Description.Should().Be("New Description");
        team.LogoUrl.Should().Be("https://logo.png");
    }

    [Fact]
    public void Team_GetMemberRole_ReturnsCorrectRole()
    {
        // Arrange
        var leaderId = Guid.NewGuid();
        var team = Team.Create("Test Team", "TT", leaderId);
        var officerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        team.AddMember(officerId, TeamRole.Officer);
        team.AddMember(memberId, TeamRole.Member);

        // Act & Assert
        team.GetMemberRole(leaderId).Should().Be(TeamRole.Leader);
        team.GetMemberRole(officerId).Should().Be(TeamRole.Officer);
        team.GetMemberRole(memberId).Should().Be(TeamRole.Member);
        team.GetMemberRole(Guid.NewGuid()).Should().BeNull();
    }

    [Fact]
    public void Team_CanManageMembers_LeaderAndOfficer_ReturnsTrue()
    {
        // Arrange
        var leaderId = Guid.NewGuid();
        var team = Team.Create("Test Team", "TT", leaderId);
        var officerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        team.AddMember(officerId, TeamRole.Officer);
        team.AddMember(memberId, TeamRole.Member);

        // Act & Assert
        team.CanManageMembers(leaderId).Should().BeTrue();
        team.CanManageMembers(officerId).Should().BeTrue();
        team.CanManageMembers(memberId).Should().BeFalse();
    }

    [Fact]
    public void Team_IsLeader_OnlyLeader_ReturnsTrue()
    {
        // Arrange
        var leaderId = Guid.NewGuid();
        var team = Team.Create("Test Team", "TT", leaderId);
        var officerId = Guid.NewGuid();
        team.AddMember(officerId, TeamRole.Officer);

        // Act & Assert
        team.IsLeader(leaderId).Should().BeTrue();
        team.IsLeader(officerId).Should().BeFalse();
    }

    [Fact]
    public void Team_HasMember_ReturnsCorrectValue()
    {
        // Arrange
        var leaderId = Guid.NewGuid();
        var team = Team.Create("Test Team", "TT", leaderId);
        var memberId = Guid.NewGuid();
        team.AddMember(memberId, TeamRole.Member);

        // Act & Assert
        team.HasMember(leaderId).Should().BeTrue();
        team.HasMember(memberId).Should().BeTrue();
        team.HasMember(Guid.NewGuid()).Should().BeFalse();
    }
}
