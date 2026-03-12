using LexiQuest.Core.Domain.Entities;

namespace LexiQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository for notification data access.
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    /// Gets a notification by its ID.
    /// </summary>
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated notifications for a user.
    /// </summary>
    Task<List<Notification>> GetByUserIdAsync(Guid userId, int skip, int take, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets unread notifications for a user.
    /// </summary>
    Task<List<Notification>> GetUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of unread notifications for a user.
    /// </summary>
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent notifications of a specific type for frequency limiting.
    /// </summary>
    Task<int> GetRecentCountByTypeAsync(Guid userId, Shared.Enums.NotificationType type, TimeSpan period, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new notification.
    /// </summary>
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a notification.
    /// </summary>
    void Update(Notification notification);

    /// <summary>
    /// Gets all unread notifications for bulk mark-read operations.
    /// </summary>
    Task<List<Notification>> GetAllUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
