using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LexiQuest.Infrastructure.Services;

public class WebPushService : IPushService
{
    private readonly IPushSubscriptionRepository _subscriptionRepository;
    private readonly VapidSettings _vapidSettings;
    private readonly ILogger<WebPushService> _logger;
    private readonly HttpClient _httpClient;

    public WebPushService(
        IPushSubscriptionRepository subscriptionRepository,
        IOptions<VapidSettings> vapidSettings,
        ILogger<WebPushService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _subscriptionRepository = subscriptionRepository;
        _vapidSettings = vapidSettings.Value;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("WebPush");
    }

    public async Task SendPushAsync(Guid userId, string title, string message, string? actionUrl = null, CancellationToken cancellationToken = default)
    {
        var subscriptions = await _subscriptionRepository.GetByUserIdAsync(userId, cancellationToken);
        if (subscriptions.Count == 0)
            return;

        var payload = JsonSerializer.Serialize(new
        {
            title,
            body = message,
            url = actionUrl
        });

        foreach (var sub in subscriptions)
        {
            try
            {
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, sub.Endpoint)
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (response.StatusCode == HttpStatusCode.Gone)
                {
                    _subscriptionRepository.Remove(sub);
                    _logger.LogInformation("Removed expired push subscription for user {UserId}", userId);
                }
                else if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Push notification failed for user {UserId}: {StatusCode}", userId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send push notification to user {UserId}", userId);
            }
        }
    }
}
