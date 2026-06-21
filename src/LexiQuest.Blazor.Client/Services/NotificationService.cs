using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LexiQuest.Shared.DTOs.Notifications;

namespace LexiQuest.Blazor.Services;

public class NotificationService : INotificationService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public NotificationService(IHttpClientFactory httpClientFactory, IAuthService authService)
    {
        _httpClient = httpClientFactory.CreateClient("PublicApiClient");
        _authService = authService;
    }

    public async Task<List<NotificationDto>> GetNotificationsAsync(int skip = 0, int take = 20)
    {
        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Get, $"api/v1/notifications?skip={skip}&take={take}");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<NotificationDto>>();
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
            using var response = await SendAuthorizedAsync(HttpMethod.Get, "api/v1/notifications/unread-count");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>();
        }
        catch (HttpRequestException)
        {
            return 0;
        }
    }

    public async Task MarkReadAsync(Guid notificationId)
    {
        using var response = await SendAuthorizedAsync(HttpMethod.Post, $"api/v1/notifications/{notificationId}/read");
        response.EnsureSuccessStatusCode();
    }

    public async Task MarkAllReadAsync()
    {
        using var response = await SendAuthorizedAsync(HttpMethod.Post, "api/v1/notifications/read-all");
        response.EnsureSuccessStatusCode();
    }

    public async Task<NotificationPreferenceDto> GetPreferencesAsync()
    {
        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Get, "api/v1/notifications/preferences");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<NotificationPreferenceDto>();
            return result ?? new NotificationPreferenceDto(true, true, true, TimeSpan.FromHours(20), true, true, true);
        }
        catch (HttpRequestException)
        {
            return new NotificationPreferenceDto(true, true, true, TimeSpan.FromHours(20), true, true, true);
        }
    }

    public async Task UpdatePreferencesAsync(UpdatePreferencesRequest request)
    {
        using var response = await SendAuthorizedAsync(HttpMethod.Put, "api/v1/notifications/preferences", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task SavePushSubscriptionAsync(PushSubscriptionDto subscription)
    {
        using var response = await SendAuthorizedAsync(HttpMethod.Post, "api/v1/notifications/push-subscription", subscription);
        response.EnsureSuccessStatusCode();
    }

    private async Task<HttpResponseMessage> SendAuthorizedAsync(HttpMethod method, string requestUri, object? jsonBody = null)
    {
        var response = await SendAuthorizedOnceAsync(method, requestUri, jsonBody);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        response.Dispose();
        var refreshResult = await _authService.RefreshTokenAsync();
        if (!refreshResult.Success)
        {
            return await SendAuthorizedOnceAsync(method, requestUri, jsonBody);
        }

        return await SendAuthorizedOnceAsync(method, requestUri, jsonBody);
    }

    private async Task<HttpResponseMessage> SendAuthorizedOnceAsync(HttpMethod method, string requestUri, object? jsonBody)
    {
        using var request = new HttpRequestMessage(method, requestUri);
        if (jsonBody is not null)
        {
            request.Content = JsonContent.Create(jsonBody);
        }

        var token = await _authService.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await _httpClient.SendAsync(request);
    }
}
