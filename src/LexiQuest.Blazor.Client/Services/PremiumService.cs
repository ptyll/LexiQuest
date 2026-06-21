using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LexiQuest.Shared.DTOs.Premium;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Blazor.Services;

public class PremiumService : IPremiumService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public PremiumService(IHttpClientFactory httpClientFactory, IAuthService authService)
    {
        _httpClient = httpClientFactory.CreateClient("PublicApiClient");
        _authService = authService;
    }

    public async Task<PremiumStatusDto?> GetStatusAsync()
    {
        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Get, "api/v1/premium/status");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PremiumStatusDto>();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<PremiumFeatureDto>> GetFeaturesAsync()
    {
        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Get, "api/v1/premium/features");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IReadOnlyList<PremiumFeatureDto>>()
                    ?? [];
            }

            return [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<CheckoutResponse> CreateCheckoutAsync(SubscriptionPlan plan)
    {
        try
        {
            using var response = await SendAuthorizedAsync(
                HttpMethod.Post,
                "api/v1/premium/checkout",
                new { Plan = plan });
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CheckoutResponse>()
                ?? new CheckoutResponse("");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create checkout: {ex.Message}", ex);
        }
    }

    public async Task<SubscriptionStatusDto?> CompleteFakeCheckoutAsync(string sessionId, SubscriptionPlan plan)
    {
        using var response = await SendAuthorizedAsync(
            HttpMethod.Post,
            "api/v1/premium/checkout/fake-complete",
            new CompleteFakeCheckoutRequest(sessionId, plan));

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SubscriptionStatusDto>();
    }

    public async Task<bool> IsPremiumAsync()
    {
        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Get, "api/v1/premium/status");
            if (response.IsSuccessStatusCode)
            {
                var status = await response.Content.ReadFromJsonAsync<PremiumStatusDto>();
                return status?.IsActive == true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CancelSubscriptionAsync()
    {
        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Post, "api/v1/premium/cancel");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
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
