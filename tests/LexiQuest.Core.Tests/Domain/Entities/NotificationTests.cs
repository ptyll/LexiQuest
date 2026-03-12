using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.Enums;
using Xunit;

namespace LexiQuest.Core.Tests.Domain.Entities;

public class NotificationTests
{
    [Fact]
    public void Notification_Create_SetsDefaultUnread()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var notification = Notification.Create(
            userId,
            NotificationType.StreakWarning,
            "Streak Warning",
            "Your streak is at risk!",
            NotificationSeverity.Warning);

        // Assert
        notification.Id.Should().NotBe(Guid.Empty);
        notification.UserId.Should().Be(userId);
        notification.Type.Should().Be(NotificationType.StreakWarning);
        notification.Title.Should().Be("Streak Warning");
        notification.Message.Should().Be("Your streak is at risk!");
        notification.Severity.Should().Be(NotificationSeverity.Warning);
        notification.IsRead.Should().BeFalse();
        notification.ReadAt.Should().BeNull();
        notification.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        notification.ActionUrl.Should().BeNull();
    }

    [Fact]
    public void Notification_Create_WithActionUrl_SetsActionUrl()
    {
        // Arrange & Act
        var notification = Notification.Create(
            Guid.NewGuid(),
            NotificationType.AchievementUnlocked,
            "Achievement!",
            "You unlocked a new achievement",
            NotificationSeverity.Success,
            "/achievements");

        // Assert
        notification.ActionUrl.Should().Be("/achievements");
    }

    [Fact]
    public void Notification_MarkRead_SetsReadAt()
    {
        // Arrange
        var notification = Notification.Create(
            Guid.NewGuid(),
            NotificationType.DailyChallenge,
            "Daily Challenge",
            "New daily challenge available!",
            NotificationSeverity.Info);

        // Act
        notification.MarkRead();

        // Assert
        notification.IsRead.Should().BeTrue();
        notification.ReadAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Notification_MarkRead_AlreadyRead_DoesNotChangeReadAt()
    {
        // Arrange
        var notification = Notification.Create(
            Guid.NewGuid(),
            NotificationType.LeagueUpdate,
            "League",
            "Position changed",
            NotificationSeverity.Info);
        notification.MarkRead();
        var originalReadAt = notification.ReadAt;

        // Act
        notification.MarkRead();

        // Assert
        notification.ReadAt.Should().Be(originalReadAt);
    }
}

public class NotificationPreferenceTests
{
    [Fact]
    public void NotificationPreference_Default_AllEnabled()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var preference = NotificationPreference.CreateDefault(userId);

        // Assert
        preference.Id.Should().NotBe(Guid.Empty);
        preference.UserId.Should().Be(userId);
        preference.PushEnabled.Should().BeTrue();
        preference.EmailEnabled.Should().BeTrue();
        preference.StreakReminder.Should().BeTrue();
        preference.StreakReminderTime.Should().Be(TimeSpan.FromHours(21));
        preference.LeagueUpdates.Should().BeTrue();
        preference.AchievementNotifications.Should().BeTrue();
        preference.DailyChallengeReminder.Should().BeTrue();
    }

    [Fact]
    public void NotificationPreference_Update_ChangesValues()
    {
        // Arrange
        var preference = NotificationPreference.CreateDefault(Guid.NewGuid());

        // Act
        preference.Update(
            pushEnabled: false,
            emailEnabled: false,
            streakReminder: true,
            streakReminderTime: TimeSpan.FromHours(20),
            leagueUpdates: false,
            achievementNotifications: true,
            dailyChallengeReminder: false);

        // Assert
        preference.PushEnabled.Should().BeFalse();
        preference.EmailEnabled.Should().BeFalse();
        preference.StreakReminder.Should().BeTrue();
        preference.StreakReminderTime.Should().Be(TimeSpan.FromHours(20));
        preference.LeagueUpdates.Should().BeFalse();
        preference.AchievementNotifications.Should().BeTrue();
        preference.DailyChallengeReminder.Should().BeFalse();
    }
}
