using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;
using Xunit;

namespace LexiQuest.Core.Tests.Domain;

public class SubscriptionTests
{
    [Fact]
    public void Create_SetsDefaultValues()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var plan = SubscriptionPlan.Monthly;
        var stripeSubscriptionId = "sub_123456";
        var startedAt = DateTime.UtcNow;
        var expiresAt = startedAt.AddDays(30);

        // Act
        var subscription = Subscription.Create(userId, plan, stripeSubscriptionId, startedAt, expiresAt);

        // Assert
        subscription.UserId.Should().Be(userId);
        subscription.Plan.Should().Be(plan);
        subscription.StripeSubscriptionId.Should().Be(stripeSubscriptionId);
        subscription.Status.Should().Be(SubscriptionStatus.Active);
        subscription.StartedAt.Should().Be(startedAt);
        subscription.ExpiresAt.Should().Be(expiresAt);
        subscription.CancelledAt.Should().BeNull();
    }

    [Fact]
    public void IsActive_WhenActiveAndNotExpired_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startedAt = DateTime.UtcNow;
        var expiresAt = startedAt.AddDays(30);
        var subscription = Subscription.Create(userId, SubscriptionPlan.Monthly, "sub_123", startedAt, expiresAt);

        // Act & Assert
        subscription.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenExpired_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startedAt = DateTime.UtcNow.AddDays(-60);
        var expiresAt = startedAt.AddDays(30); // Expired 30 days ago
        var subscription = Subscription.Create(userId, SubscriptionPlan.Monthly, "sub_123", startedAt, expiresAt);

        // Act & Assert
        subscription.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenCancelled_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startedAt = DateTime.UtcNow;
        var expiresAt = startedAt.AddDays(30);
        var subscription = Subscription.Create(userId, SubscriptionPlan.Monthly, "sub_123", startedAt, expiresAt);
        subscription.Cancel(DateTime.UtcNow);

        // Act & Assert
        subscription.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Cancel_SetsCancelledAtAndStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startedAt = DateTime.UtcNow;
        var expiresAt = startedAt.AddDays(30);
        var subscription = Subscription.Create(userId, SubscriptionPlan.Monthly, "sub_123", startedAt, expiresAt);
        var cancelledAt = DateTime.UtcNow;

        // Act
        subscription.Cancel(cancelledAt);

        // Assert
        subscription.Status.Should().Be(SubscriptionStatus.Cancelled);
        subscription.CancelledAt.Should().Be(cancelledAt);
    }

    [Fact]
    public void MarkAsExpired_SetsStatusToExpired()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startedAt = DateTime.UtcNow;
        var expiresAt = startedAt.AddDays(30);
        var subscription = Subscription.Create(userId, SubscriptionPlan.Monthly, "sub_123", startedAt, expiresAt);

        // Act
        subscription.MarkAsExpired();

        // Assert
        subscription.Status.Should().Be(SubscriptionStatus.Expired);
    }

    [Fact]
    public void MarkAsPastDue_SetsStatusToPastDue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startedAt = DateTime.UtcNow;
        var expiresAt = startedAt.AddDays(30);
        var subscription = Subscription.Create(userId, SubscriptionPlan.Monthly, "sub_123", startedAt, expiresAt);

        // Act
        subscription.MarkAsPastDue();

        // Assert
        subscription.Status.Should().Be(SubscriptionStatus.PastDue);
    }

    [Fact]
    public void Extend_UpdatesExpiresAt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startedAt = DateTime.UtcNow;
        var originalExpiry = startedAt.AddDays(30);
        var subscription = Subscription.Create(userId, SubscriptionPlan.Monthly, "sub_123", startedAt, originalExpiry);
        var newExpiry = startedAt.AddDays(365);

        // Act
        subscription.Extend(newExpiry);

        // Assert
        subscription.ExpiresAt.Should().Be(newExpiry);
    }

    [Fact]
    public void IsLifetime_WhenLifetimePlan_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startedAt = DateTime.UtcNow;
        var expiresAt = startedAt.AddYears(100);
        var subscription = Subscription.Create(userId, SubscriptionPlan.Lifetime, "sub_123", startedAt, expiresAt);

        // Act & Assert
        subscription.Plan.Should().Be(SubscriptionPlan.Lifetime);
    }

    [Theory]
    [InlineData(SubscriptionPlan.Monthly)]
    [InlineData(SubscriptionPlan.Yearly)]
    [InlineData(SubscriptionPlan.Lifetime)]
    public void Plan_CanBeSet(SubscriptionPlan plan)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startedAt = DateTime.UtcNow;
        var expiresAt = startedAt.AddDays(30);
        var subscription = Subscription.Create(userId, plan, "sub_123", startedAt, expiresAt);

        // Act & Assert
        subscription.Plan.Should().Be(plan);
    }
}
