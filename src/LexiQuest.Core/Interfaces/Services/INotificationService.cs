using LexiQuest.Shared.DTOs.Notifications;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Interfaces.Services;

/// <summary>
/// Service for managing notifications.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification, respecting user preferences for push/email delivery.
    /// </summary>
    Task SendAsync(SendNotificationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated notifications for a user.
    /// </summary>
    Task<List<NotificationDto>> GetByUserIdAsync(Guid userId, int skip = 0, int take = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets unread notifications for a user.
    /// </summary>
    Task<List<NotificationDto>> GetUnreadAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of unread notifications.
    /// </summary>
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    Task MarkReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks all notifications as read for a user.
    /// </summary>
    Task MarkAllReadAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets notification preferences for a user.
    /// </summary>
    Task<NotificationPreferenceDto> GetPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates notification preferences for a user.
    /// </summary>
    Task UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest request, CancellationToken cancellationToken = default);
}
