using System.Net.Http.Headers;
using System.Net.Http.Json;
using LexiQuest.Shared.DTOs.Streak;

namespace LexiQuest.Blazor.Services;

public class StreakProtectionClient : IStreakProtectionClient
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public StreakProtectionClient(IHttpClientFactory httpClientFactory, IAuthService authService)
    {
        _httpClient = httpClientFactory.CreateClient("PublicApiClient");
        _authService = authService;
    }

    public async Task<ActivateShieldResponse?> ActivateShieldAsync(CancellationToken cancellationToken = default)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, "api/v1/streak/shield/activate");
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<ActivateShieldResponse>(cancellationToken: cancellationToken);
    }

    public async Task<PurchaseShieldsResponse?> PurchaseShieldsAsync(int quantity, CancellationToken cancellationToken = default)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, "api/v1/streak/shield/purchase");
        request.Content = JsonContent.Create(new PurchaseShieldsRequest(quantity));

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<PurchaseShieldsResponse>(cancellationToken: cancellationToken);
    }

    public async Task<EmergencyShieldResponse?> PurchaseEmergencyShieldAsync(CancellationToken cancellationToken = default)
    {
        using var request = await CreateAuthorizedRequestAsync(HttpMethod.Post, "api/v1/streak/shield/emergency");
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<EmergencyShieldResponse>(cancellationToken: cancellationToken);
    }

    private async Task<HttpRequestMessage> CreateAuthorizedRequestAsync(HttpMethod method, string uri)
    {
        var request = new HttpRequestMessage(method, uri);
        var token = await _authService.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return request;
    }
}
