namespace LexiQuest.Core.Interfaces.Services;

/// <summary>
/// Service for sending push notifications via Web Push API.
/// </summary>
public interface IPushService
{
    /// <summary>
    /// Sends a push notification to all subscriptions for a user.
    /// </summary>
    Task SendPushAsync(Guid userId, string title, string message, string? actionUrl = null, CancellationToken cancellationToken = default);
}
