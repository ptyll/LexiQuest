using System.Net.Http.Json;
using LexiQuest.Shared.DTOs.Notifications;

namespace LexiQuest.Blazor.Services;

public class NotificationService : INotificationService
{
    private readonly HttpClient _httpClient;

    public NotificationService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
    }

    public async Task<List<NotificationDto>> GetNotificationsAsync(int skip = 0, int take = 20)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<NotificationDto>>($"api/v1/notifications?skip={skip}&take={take}");
            return result ?? [];
        }
        catch (HttpRequestException)
        {
            return [];
        }
    }

    public async Task<int> GetUnreadCountAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<int>("api/v1/notifications/unread-count");
        }
        catch (HttpRequestException)
        {
            return 0;
        }
    }

    public async Task MarkReadAsync(Guid notificationId)
    {
        await _httpClient.PostAsync($"api/v1/notifications/{notificationId}/read", null);
    }

    public async Task MarkAllReadAsync()
    {
        await _httpClient.PostAsync("api/v1/notifications/read-all", null);
    }

    public async Task<NotificationPreferenceDto> GetPreferencesAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<NotificationPreferenceDto>("api/v1/notifications/preferences");
            return result ?? new NotificationPreferenceDto(true, true, true, TimeSpan.FromHours(20), true, true, true);
        }
        catch (HttpRequestException)
        {
            return new NotificationPreferenceDto(true, true, true, TimeSpan.FromHours(20), true, true, true);
        }
    }

    public async Task UpdatePreferencesAsync(UpdatePreferencesRequest request)
    {
        await _httpClient.PutAsJsonAsync("api/v1/notifications/preferences", request);
    }

    public async Task SavePushSubscriptionAsync(PushSubscriptionDto subscription)
    {
        await _httpClient.PostAsJsonAsync("api/v1/notifications/push-subscription", subscription);
    }
}
