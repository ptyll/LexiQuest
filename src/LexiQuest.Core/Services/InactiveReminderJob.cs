using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Notifications;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace LexiQuest.Core.Services;

/// <summary>
/// Background job that sends reminders to users inactive for 7 days via email.
/// </summary>
public class InactiveReminderJob
{
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly ILogger<InactiveReminderJob> _logger;

    public InactiveReminderJob(
        IUserRepository userRepository,
        INotificationService notificationService,
        IEmailService emailService,
        ILogger<InactiveReminderJob> logger)
    {
        _userRepository = userRepository;
        _notificationService = notificationService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting inactive reminder job");

        var inactiveUsers = await _userRepository.GetInactiveUsersAsync(7, cancellationToken);

        foreach (var user in inactiveUsers)
        {
            await _emailService.SendNotificationEmailAsync(
                user.Email,
                "We miss you!",
                $"Hi {user.Username}, it's been a week since you last played. Come back and keep learning!",
                cancellationToken);

            await _notificationService.SendAsync(new SendNotificationRequest(
                user.Id,
                NotificationType.SystemMessage,
                "We miss you!",
                "It's been a while since you last played. Come back and keep learning!",
                NotificationSeverity.Info,
                "/game"), cancellationToken);
        }

        _logger.LogInformation("Inactive reminder job completed. Notified {Count} users", inactiveUsers.Count);
    }
}
