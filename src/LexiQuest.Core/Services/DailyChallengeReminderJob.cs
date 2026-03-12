using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Notifications;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace LexiQuest.Core.Services;

/// <summary>
/// Background job that sends daily challenge reminder notifications at 8:00.
/// </summary>
public class DailyChallengeReminderJob
{
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<DailyChallengeReminderJob> _logger;

    public DailyChallengeReminderJob(
        IUserRepository userRepository,
        INotificationService notificationService,
        ILogger<DailyChallengeReminderJob> logger)
    {
        _userRepository = userRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting daily challenge reminder job");

        var users = await _userRepository.GetActiveUsersAsync(cancellationToken);

        foreach (var user in users)
        {
            await _notificationService.SendAsync(new SendNotificationRequest(
                user.Id,
                NotificationType.DailyChallenge,
                "Daily Challenge",
                "A new daily challenge is available! Can you beat today's challenge?",
                NotificationSeverity.Info,
                "/daily-challenge"), cancellationToken);
        }

        _logger.LogInformation("Daily challenge reminder job completed. Notified {Count} users", users.Count);
    }
}
