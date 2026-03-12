using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;

namespace LexiQuest.Core.Tests.Domain.Entities;

public class SubscriptionTests
{
    [Fact]
    public void Subscription_Create_SetsDefaultValues()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var plan = SubscriptionPlan.Monthly;
        var stripeSubscriptionId = "sub_123456";
        var startedAt = DateTime.UtcNow;
        var expiresAt = startedAt.AddMonths(1);

        // Act
        var subscription = Subscription.Create(userId, plan, stripeSubscriptionId, startedAt, expiresAt);

        // Assert
        subscription.UserId.Should().Be(userId);
        subscription.Plan.Should().Be(plan);
        subscription.StripeSubscriptionId.Should().Be(stripeSubscriptionId);
        subscription.StartedAt.Should().Be(startedAt);
        subscription.ExpiresAt.Should().Be(expiresAt);
        subscription.Status.Should().Be(SubscriptionStatus.Active);
        subscription.CancelledAt.Should().BeNull();
        subscription.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Subscription_IsActive_ReturnsTrueBeforeExpiry()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var plan = SubscriptionPlan.Monthly;
        var stripeSubscriptionId = "sub_123456";
        var startedAt = DateTime.UtcNow.AddDays(-10);
        var expiresAt = DateTime.UtcNow.AddDays(20);

        var subscription = Subscription.Create(userId, plan, stripeSubscriptionId, startedAt, expiresAt);

        // Act
        var isActive = subscription.IsActive;

        // Assert
        isActive.Should().BeTrue();
    }

    [Fact]
    public void Subscription_IsActive_ReturnsFalseAfterExpiry()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var plan = SubscriptionPlan.Monthly;
        var stripeSubscriptionId = "sub_123456";
        var startedAt = DateTime.UtcNow.AddDays(-40);
        var expiresAt = DateTime.UtcNow.AddDays(-10);

        var subscription = Subscription.Create(userId, plan, stripeSubscriptionId, startedAt, expiresAt);

        // Act
        var isActive = subscription.IsActive;

        // Assert
        isActive.Should().BeFalse();
    }

    [Fact]
    public void Subscription_IsActive_ReturnsFalseWhenCancelled()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var plan = SubscriptionPlan.Monthly;
        var stripeSubscriptionId = "sub_123456";
        var startedAt = DateTime.UtcNow.AddDays(-10);
        var expiresAt = DateTime.UtcNow.AddDays(20);

        var subscription = Subscription.Create(userId, plan, stripeSubscriptionId, startedAt, expiresAt);
        subscription.Cancel(DateTime.UtcNow);

        // Act
        var isActive = subscription.IsActive;

        // Assert
        isActive.Should().BeFalse();
    }

    [Fact]
    public void Subscription_Cancel_SetsCancelledAt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var plan = SubscriptionPlan.Monthly;
        var stripeSubscriptionId = "sub_123456";
        var startedAt = DateTime.UtcNow;
        var expiresAt = startedAt.AddMonths(1);
        var cancelledAt = DateTime.UtcNow;

        var subscription = Subscription.Create(userId, plan, stripeSubscriptionId, startedAt, expiresAt);

        // Act
        subscription.Cancel(cancelledAt);

        // Assert
        subscription.CancelledAt.Should().Be(cancelledAt);
        subscription.Status.Should().Be(SubscriptionStatus.Cancelled);
    }

    [Fact]
    public void Subscription_Extend_UpdatesExpiresAt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var plan = SubscriptionPlan.Monthly;
        var stripeSubscriptionId = "sub_123456";
        var startedAt = DateTime.UtcNow.AddDays(-10);
        var expiresAt = DateTime.UtcNow.AddDays(20);
        var newExpiry = DateTime.UtcNow.AddMonths(1);

        var subscription = Subscription.Create(userId, plan, stripeSubscriptionId, startedAt, expiresAt);

        // Act
        subscription.Extend(newExpiry);

        // Assert
        subscription.ExpiresAt.Should().Be(newExpiry);
    }

    [Fact]
    public void Subscription_MarkAsExpired_SetsExpiredStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var plan = SubscriptionPlan.Monthly;
        var stripeSubscriptionId = "sub_123456";
        var startedAt = DateTime.UtcNow.AddDays(-40);
        var expiresAt = DateTime.UtcNow.AddDays(-10);

        var subscription = Subscription.Create(userId, plan, stripeSubscriptionId, startedAt, expiresAt);

        // Act
        subscription.MarkAsExpired();

        // Assert
        subscription.Status.Should().Be(SubscriptionStatus.Expired);
    }

    [Fact]
    public void Subscription_MarkAsPastDue_SetsPastDueStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var plan = SubscriptionPlan.Monthly;
        var stripeSubscriptionId = "sub_123456";
        var startedAt = DateTime.UtcNow.AddDays(-10);
        var expiresAt = DateTime.UtcNow.AddDays(20);

        var subscription = Subscription.Create(userId, plan, stripeSubscriptionId, startedAt, expiresAt);

        // Act
        subscription.MarkAsPastDue();

        // Assert
        subscription.Status.Should().Be(SubscriptionStatus.PastDue);
    }
}
