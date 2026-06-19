using System.Net.Http.Json;
using LexiQuest.Shared.DTOs.Premium;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Blazor.Services;

public class PremiumService : IPremiumService
{
    private readonly HttpClient _httpClient;

    public PremiumService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
    }

    public async Task<PremiumStatusDto?> GetStatusAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/premium/status");
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

    public async Task<CheckoutResponse> CreateCheckoutAsync(SubscriptionPlan plan)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/premium/checkout", new { Plan = plan });
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CheckoutResponse>()
                ?? new CheckoutResponse("");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create checkout: {ex.Message}", ex);
        }
    }

    public async Task<bool> IsPremiumAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/premium/status");
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
            var response = await _httpClient.PostAsync("api/v1/premium/cancel", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
