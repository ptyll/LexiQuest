using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Notifications;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace LexiQuest.Core.Services;

/// <summary>
/// Background job that sends streak warning notifications at 21:00 to users who haven't played today.
/// </summary>
public class StreakReminderJob
{
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<StreakReminderJob> _logger;

    public StreakReminderJob(
        IUserRepository userRepository,
        INotificationService notificationService,
        ILogger<StreakReminderJob> logger)
    {
        _userRepository = userRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting streak reminder job");

        var users = await _userRepository.GetUsersWithStreakNotPlayedTodayAsync(cancellationToken);

        foreach (var user in users)
        {
            await _notificationService.SendAsync(new SendNotificationRequest(
                user.Id,
                NotificationType.StreakWarning,
                "Streak Warning",
                "Your streak is at risk! Play now to keep it alive.",
                NotificationSeverity.Warning,
                "/game"), cancellationToken);
        }

        _logger.LogInformation("Streak reminder job completed. Notified {Count} users", users.Count);
    }
}
