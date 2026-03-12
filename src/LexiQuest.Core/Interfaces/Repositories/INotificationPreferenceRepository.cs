using LexiQuest.Core.Domain.Entities;

namespace LexiQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository for notification preference data access.
/// </summary>
public interface INotificationPreferenceRepository
{
    /// <summary>
    /// Gets notification preferences for a user.
    /// </summary>
    Task<NotificationPreference?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds new notification preferences.
    /// </summary>
    Task AddAsync(NotificationPreference preference, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates notification preferences.
    /// </summary>
    void Update(NotificationPreference preference);
}
