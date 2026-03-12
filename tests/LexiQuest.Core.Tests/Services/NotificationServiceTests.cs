using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using LexiQuest.Shared.DTOs.Notifications;
using LexiQuest.Shared.Enums;
using NSubstitute;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

public class NotificationServiceTests
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPreferenceRepository _preferenceRepository;
    private readonly IPushService _pushService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _notificationRepository = Substitute.For<INotificationRepository>();
        _preferenceRepository = Substitute.For<INotificationPreferenceRepository>();
        _pushService = Substitute.For<IPushService>();
        _emailService = Substitute.For<IEmailService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new NotificationService(
            _notificationRepository,
            _preferenceRepository,
            _pushService,
            _emailService,
            _unitOfWork);
    }

    [Fact]
    public async Task NotificationService_Send_StreakWarning_CreatesNotification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new SendNotificationRequest(
            userId,
            NotificationType.StreakWarning,
            "Streak Warning",
            "Your streak is at risk!",
            NotificationSeverity.Warning);

        var preference = NotificationPreference.CreateDefault(userId);
        _preferenceRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<NotificationPreference?>(preference));
        _notificationRepository.GetRecentCountByTypeAsync(userId, NotificationType.StreakWarning, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(0));

        // Act
        await _sut.SendAsync(request);

        // Assert
        await _notificationRepository.Received(1).AddAsync(
            Arg.Is<Notification>(n =>
                n.UserId == userId &&
                n.Type == NotificationType.StreakWarning &&
                n.Title == "Streak Warning" &&
                n.IsRead == false),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NotificationService_Send_RespectsPreferences_PushDisabled_SkipsPush()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new SendNotificationRequest(
            userId,
            NotificationType.StreakWarning,
            "Streak Warning",
            "Your streak is at risk!",
            NotificationSeverity.Warning);

        var preference = NotificationPreference.CreateDefault(userId);
        preference.Update(
            pushEnabled: false,
            emailEnabled: true,
            streakReminder: true,
            streakReminderTime: TimeSpan.FromHours(21),
            leagueUpdates: true,
            achievementNotifications: true,
            dailyChallengeReminder: true);

        _preferenceRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<NotificationPreference?>(preference));
        _notificationRepository.GetRecentCountByTypeAsync(userId, NotificationType.StreakWarning, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(0));

        // Act
        await _sut.SendAsync(request);

        // Assert
        await _pushService.DidNotReceive().SendPushAsync(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NotificationService_Send_RespectsPreferences_EmailDisabled_SkipsEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new SendNotificationRequest(
            userId,
            NotificationType.StreakLost,
            "Streak Lost",
            "Your streak has been reset!",
            NotificationSeverity.Error);

        var preference = NotificationPreference.CreateDefault(userId);
        preference.Update(
            pushEnabled: true,
            emailEnabled: false,
            streakReminder: true,
            streakReminderTime: TimeSpan.FromHours(21),
            leagueUpdates: true,
            achievementNotifications: true,
            dailyChallengeReminder: true);

        _preferenceRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<NotificationPreference?>(preference));
        _notificationRepository.GetRecentCountByTypeAsync(userId, NotificationType.StreakLost, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(0));

        // Act
        await _sut.SendAsync(request);

        // Assert
        await _emailService.DidNotReceive().SendNotificationEmailAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NotificationService_GetUnread_ReturnsOnlyUnread()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var unreadNotifications = new List<Notification>
        {
            Notification.Create(userId, NotificationType.StreakWarning, "Warning 1", "Msg 1", NotificationSeverity.Warning),
            Notification.Create(userId, NotificationType.DailyChallenge, "Challenge", "Msg 2", NotificationSeverity.Info)
        };

        _notificationRepository.GetUnreadByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(unreadNotifications));

        // Act
        var result = await _sut.GetUnreadAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(n => n.IsRead.Should().BeFalse());
    }

    [Fact]
    public async Task NotificationService_GetUnreadCount_ReturnsCorrectCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _notificationRepository.GetUnreadCountAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(5));

        // Act
        var count = await _sut.GetUnreadCountAsync(userId);

        // Assert
        count.Should().Be(5);
    }

    [Fact]
    public async Task NotificationService_MarkAllRead_MarksAll()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notification1 = Notification.Create(userId, NotificationType.StreakWarning, "T1", "M1", NotificationSeverity.Warning);
        var notification2 = Notification.Create(userId, NotificationType.DailyChallenge, "T2", "M2", NotificationSeverity.Info);

        _notificationRepository.GetAllUnreadByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<Notification> { notification1, notification2 }));

        // Act
        await _sut.MarkAllReadAsync(userId);

        // Assert
        notification1.IsRead.Should().BeTrue();
        notification2.IsRead.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task NotificationService_FrequencyLimit_DoesNotSpam()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new SendNotificationRequest(
            userId,
            NotificationType.LeagueUpdate,
            "League Update",
            "Position changed",
            NotificationSeverity.Info);

        var preference = NotificationPreference.CreateDefault(userId);
        _preferenceRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<NotificationPreference?>(preference));

        // Already 5 notifications in the last hour (at the limit)
        _notificationRepository.GetRecentCountByTypeAsync(userId, NotificationType.LeagueUpdate, TimeSpan.FromHours(1), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(5));

        // Act
        await _sut.SendAsync(request);

        // Assert - should NOT create notification due to frequency limit
        await _notificationRepository.DidNotReceive().AddAsync(
            Arg.Any<Notification>(), Arg.Any<CancellationToken>());
    }
}
