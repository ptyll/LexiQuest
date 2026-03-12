using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using Xunit;

namespace LexiQuest.Core.Tests.Domain.Entities;

public class TeamJoinRequestTests
{
    [Fact]
    public void TeamJoinRequest_Create_SetsProperties()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var message = "Please let me join your team!";

        // Act
        var request = TeamJoinRequest.Create(teamId, userId, message);

        // Assert
        request.TeamId.Should().Be(teamId);
        request.UserId.Should().Be(userId);
        request.Message.Should().Be(message);
        request.Status.Should().Be(JoinRequestStatus.Pending);
        request.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void TeamJoinRequest_Create_WithNullMessage_SetsNull()
    {
        // Arrange & Act
        var request = TeamJoinRequest.Create(Guid.NewGuid(), Guid.NewGuid(), null);

        // Assert
        request.Message.Should().BeNull();
    }

    [Fact]
    public void TeamJoinRequest_Approve_ChangesStatusToApproved()
    {
        // Arrange
        var request = TeamJoinRequest.Create(Guid.NewGuid(), Guid.NewGuid(), "Hello");

        // Act
        request.Approve();

        // Assert
        request.Status.Should().Be(JoinRequestStatus.Approved);
    }

    [Fact]
    public void TeamJoinRequest_Reject_ChangesStatusToRejected()
    {
        // Arrange
        var request = TeamJoinRequest.Create(Guid.NewGuid(), Guid.NewGuid(), "Hello");

        // Act
        request.Reject();

        // Assert
        request.Status.Should().Be(JoinRequestStatus.Rejected);
    }

    [Fact]
    public void TeamJoinRequest_Approve_WhenNotPending_Throws()
    {
        // Arrange
        var request = TeamJoinRequest.Create(Guid.NewGuid(), Guid.NewGuid(), "Hello");
        request.Approve();

        // Act & Assert
        var action = () => request.Approve();
        action.Should().Throw<InvalidOperationException>().WithMessage("*Only pending*");
    }

    [Fact]
    public void TeamJoinRequest_Reject_WhenNotPending_Throws()
    {
        // Arrange
        var request = TeamJoinRequest.Create(Guid.NewGuid(), Guid.NewGuid(), "Hello");
        request.Reject();

        // Act & Assert
        var action = () => request.Reject();
        action.Should().Throw<InvalidOperationException>().WithMessage("*Only pending*");
    }

    [Fact]
    public void TeamJoinRequest_IsPending_WhenPending_ReturnsTrue()
    {
        // Arrange
        var request = TeamJoinRequest.Create(Guid.NewGuid(), Guid.NewGuid(), "Hello");

        // Act & Assert
        request.IsPending.Should().BeTrue();
    }

    [Fact]
    public void TeamJoinRequest_IsPending_WhenApproved_ReturnsFalse()
    {
        // Arrange
        var request = TeamJoinRequest.Create(Guid.NewGuid(), Guid.NewGuid(), "Hello");
        request.Approve();

        // Act & Assert
        request.IsPending.Should().BeFalse();
    }
}
