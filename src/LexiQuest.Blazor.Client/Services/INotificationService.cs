using LexiQuest.Shared.DTOs.Notifications;

namespace LexiQuest.Blazor.Services;

public interface INotificationService
{
    Task<List<NotificationDto>> GetNotificationsAsync(int skip = 0, int take = 20);
    Task<int> GetUnreadCountAsync();
    Task MarkReadAsync(Guid notificationId);
    Task MarkAllReadAsync();
    Task<NotificationPreferenceDto> GetPreferencesAsync();
    Task UpdatePreferencesAsync(UpdatePreferencesRequest request);
    Task SavePushSubscriptionAsync(PushSubscriptionDto subscription);
}
