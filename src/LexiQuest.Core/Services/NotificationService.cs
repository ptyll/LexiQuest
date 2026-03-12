using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Notifications;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPreferenceRepository _preferenceRepository;
    private readonly IPushService _pushService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    private const int MaxNotificationsPerHour = 5;

    public NotificationService(
        INotificationRepository notificationRepository,
        INotificationPreferenceRepository preferenceRepository,
        IPushService pushService,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _preferenceRepository = preferenceRepository;
        _pushService = pushService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task SendAsync(SendNotificationRequest request, CancellationToken cancellationToken = default)
    {
        var preference = await _preferenceRepository.GetByUserIdAsync(request.UserId, cancellationToken)
            ?? NotificationPreference.CreateDefault(request.UserId);

        if (!IsNotificationTypeEnabled(preference, request.Type))
            return;

        var recentCount = await _notificationRepository.GetRecentCountByTypeAsync(
            request.UserId, request.Type, TimeSpan.FromHours(1), cancellationToken);

        if (recentCount >= MaxNotificationsPerHour)
            return;

        var notification = Notification.Create(
            request.UserId,
            request.Type,
            request.Title,
            request.Message,
            request.Severity,
            request.ActionUrl);

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (preference.PushEnabled)
        {
            await _pushService.SendPushAsync(
                request.UserId, request.Title, request.Message, request.ActionUrl, cancellationToken);
        }

        if (preference.EmailEnabled)
        {
            await _emailService.SendNotificationEmailAsync(
                request.UserId.ToString(), request.Title, request.Message, cancellationToken);
        }
    }

    public async Task<List<NotificationDto>> GetByUserIdAsync(Guid userId, int skip = 0, int take = 20, CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(userId, skip, take, cancellationToken);
        return notifications.Select(MapToDto).ToList();
    }

    public async Task<List<NotificationDto>> GetUnreadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.GetUnreadByUserIdAsync(userId, cancellationToken);
        return notifications.Select(MapToDto).ToList();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _notificationRepository.GetUnreadCountAsync(userId, cancellationToken);
    }

    public async Task MarkReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
        if (notification == null || notification.UserId != userId)
            return;

        notification.MarkRead();
        _notificationRepository.Update(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var unread = await _notificationRepository.GetAllUnreadByUserIdAsync(userId, cancellationToken);
        foreach (var notification in unread)
        {
            notification.MarkRead();
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<NotificationPreferenceDto> GetPreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var preference = await _preferenceRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? NotificationPreference.CreateDefault(userId);

        return new NotificationPreferenceDto(
            preference.PushEnabled,
            preference.EmailEnabled,
            preference.StreakReminder,
            preference.StreakReminderTime,
            preference.LeagueUpdates,
            preference.AchievementNotifications,
            preference.DailyChallengeReminder);
    }

    public async Task UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest request, CancellationToken cancellationToken = default)
    {
        var preference = await _preferenceRepository.GetByUserIdAsync(userId, cancellationToken);
        if (preference == null)
        {
            preference = NotificationPreference.CreateDefault(userId);
            await _preferenceRepository.AddAsync(preference, cancellationToken);
        }

        preference.Update(
            request.PushEnabled,
            request.EmailEnabled,
            request.StreakReminder,
            request.StreakReminderTime,
            request.LeagueUpdates,
            request.AchievementNotifications,
            request.DailyChallengeReminder);

        _preferenceRepository.Update(preference);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static bool IsNotificationTypeEnabled(NotificationPreference preference, NotificationType type)
    {
        return type switch
        {
            NotificationType.StreakWarning => preference.StreakReminder,
            NotificationType.StreakLost => preference.StreakReminder,
            NotificationType.DailyChallenge => preference.DailyChallengeReminder,
            NotificationType.LeagueUpdate => preference.LeagueUpdates,
            NotificationType.AchievementUnlocked => preference.AchievementNotifications,
            NotificationType.Milestone => true,
            NotificationType.SystemMessage => true,
            _ => true
        };
    }

    private static NotificationDto MapToDto(Notification notification)
    {
        return new NotificationDto(
            notification.Id,
            notification.Type,
            notification.Title,
            notification.Message,
            notification.Severity,
            notification.IsRead,
            notification.ReadAt,
            notification.CreatedAt,
            notification.ActionUrl);
    }
}
