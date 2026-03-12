using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using Xunit;

namespace LexiQuest.Core.Tests.Domain.Entities;

public class TeamInviteTests
{
    [Fact]
    public void TeamInvite_Create_SetsProperties()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var invitedUserId = Guid.NewGuid();
        var invitedByUserId = Guid.NewGuid();

        // Act
        var invite = TeamInvite.Create(teamId, invitedUserId, invitedByUserId);

        // Assert
        invite.TeamId.Should().Be(teamId);
        invite.InvitedUserId.Should().Be(invitedUserId);
        invite.InvitedByUserId.Should().Be(invitedByUserId);
        invite.Status.Should().Be(InviteStatus.Pending);
        invite.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(1));
        invite.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void TeamInvite_Accept_ChangesStatusToAccepted()
    {
        // Arrange
        var invite = TeamInvite.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Act
        invite.Accept();

        // Assert
        invite.Status.Should().Be(InviteStatus.Accepted);
    }

    [Fact]
    public void TeamInvite_Reject_ChangesStatusToRejected()
    {
        // Arrange
        var invite = TeamInvite.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Act
        invite.Reject();

        // Assert
        invite.Status.Should().Be(InviteStatus.Rejected);
    }

    [Fact]
    public void TeamInvite_Cancel_ChangesStatusToCancelled()
    {
        // Arrange
        var invite = TeamInvite.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Act
        invite.Cancel();

        // Assert
        invite.Status.Should().Be(InviteStatus.Cancelled);
    }

    [Fact]
    public void TeamInvite_Accept_WhenNotPending_Throws()
    {
        // Arrange
        var invite = TeamInvite.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        invite.Accept();

        // Act & Assert
        var action = () => invite.Accept();
        action.Should().Throw<InvalidOperationException>().WithMessage("*Only pending*");
    }

    [Fact]
    public void TeamInvite_IsExpired_WhenExpiresAtPassed_ReturnsTrue()
    {
        // Arrange
        var invite = TeamInvite.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        
        // Use reflection to set expires at to past
        var expiresAtProperty = typeof(TeamInvite).GetProperty("ExpiresAt");
        expiresAtProperty?.SetValue(invite, DateTime.UtcNow.AddDays(-1));

        // Act & Assert
        invite.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void TeamInvite_IsExpired_WhenNotPassed_ReturnsFalse()
    {
        // Arrange
        var invite = TeamInvite.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        invite.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void TeamInvite_IsPending_WhenPendingAndNotExpired_ReturnsTrue()
    {
        // Arrange
        var invite = TeamInvite.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        invite.IsPending.Should().BeTrue();
    }

    [Fact]
    public void TeamInvite_IsPending_WhenExpired_ReturnsFalse()
    {
        // Arrange
        var invite = TeamInvite.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var expiresAtProperty = typeof(TeamInvite).GetProperty("ExpiresAt");
        expiresAtProperty?.SetValue(invite, DateTime.UtcNow.AddDays(-1));

        // Act & Assert
        invite.IsPending.Should().BeFalse();
    }
}
