using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using LexiQuest.Shared.DTOs.Notifications;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

public class StreakReminderJobTests
{
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<StreakReminderJob> _logger;
    private readonly StreakReminderJob _sut;

    public StreakReminderJobTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _notificationService = Substitute.For<INotificationService>();
        _logger = Substitute.For<ILogger<StreakReminderJob>>();
        _sut = new StreakReminderJob(_userRepository, _notificationService, _logger);
    }

    [Fact]
    public async Task StreakReminderJob_Execute_SendsNotificationToUsersWhoHaventPlayedToday()
    {
        // Arrange
        var user = User.Create("test@test.com", "testuser");
        _userRepository.GetUsersWithStreakNotPlayedTodayAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<User> { user }));

        // Act
        await _sut.ExecuteAsync();

        // Assert
        await _notificationService.Received(1).SendAsync(
            Arg.Is<SendNotificationRequest>(r =>
                r.UserId == user.Id &&
                r.Type == NotificationType.StreakWarning),
            Arg.Any<CancellationToken>());
    }
}

public class DailyChallengeReminderJobTests
{
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<DailyChallengeReminderJob> _logger;
    private readonly DailyChallengeReminderJob _sut;

    public DailyChallengeReminderJobTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _notificationService = Substitute.For<INotificationService>();
        _logger = Substitute.For<ILogger<DailyChallengeReminderJob>>();
        _sut = new DailyChallengeReminderJob(_userRepository, _notificationService, _logger);
    }

    [Fact]
    public async Task DailyChallengeReminderJob_Execute_SendsNotificationToActiveUsers()
    {
        // Arrange
        var user = User.Create("test@test.com", "testuser");
        _userRepository.GetActiveUsersAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<User> { user }));

        // Act
        await _sut.ExecuteAsync();

        // Assert
        await _notificationService.Received(1).SendAsync(
            Arg.Is<SendNotificationRequest>(r =>
                r.UserId == user.Id &&
                r.Type == NotificationType.DailyChallenge),
            Arg.Any<CancellationToken>());
    }
}

public class InactiveReminderJobTests
{
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly ILogger<InactiveReminderJob> _logger;
    private readonly InactiveReminderJob _sut;

    public InactiveReminderJobTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _notificationService = Substitute.For<INotificationService>();
        _emailService = Substitute.For<IEmailService>();
        _logger = Substitute.For<ILogger<InactiveReminderJob>>();
        _sut = new InactiveReminderJob(_userRepository, _notificationService, _emailService, _logger);
    }

    [Fact]
    public async Task InactiveReminderJob_Execute_Sends7DaysInactiveEmail()
    {
        // Arrange
        var user = User.Create("test@test.com", "testuser");
        _userRepository.GetInactiveUsersAsync(7, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<User> { user }));

        // Act
        await _sut.ExecuteAsync();

        // Assert
        await _emailService.Received(1).SendNotificationEmailAsync(
            "test@test.com",
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }
}
