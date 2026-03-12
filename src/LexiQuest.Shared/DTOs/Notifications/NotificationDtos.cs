using LexiQuest.Shared.Enums;

namespace LexiQuest.Shared.DTOs.Notifications;

public record NotificationDto(
    Guid Id,
    NotificationType Type,
    string Title,
    string Message,
    NotificationSeverity Severity,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt,
    string? ActionUrl);

public record NotificationPreferenceDto(
    bool PushEnabled,
    bool EmailEnabled,
    bool StreakReminder,
    TimeSpan StreakReminderTime,
    bool LeagueUpdates,
    bool AchievementNotifications,
    bool DailyChallengeReminder);

public record UpdatePreferencesRequest(
    bool PushEnabled,
    bool EmailEnabled,
    bool StreakReminder,
    TimeSpan StreakReminderTime,
    bool LeagueUpdates,
    bool AchievementNotifications,
    bool DailyChallengeReminder);

public record SendNotificationRequest(
    Guid UserId,
    NotificationType Type,
    string Title,
    string Message,
    NotificationSeverity Severity,
    string? ActionUrl = null);

public record PushSubscriptionDto(
    string Endpoint,
    string P256dh,
    string Auth);
