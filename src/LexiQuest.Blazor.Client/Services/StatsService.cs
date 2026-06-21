using System.Net.Http.Json;
using System.Net;
using System.Net.Http.Headers;
using LexiQuest.Shared.DTOs.Stats;

namespace LexiQuest.Blazor.Services;

public class StatsService : IStatsService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public StatsService(IHttpClientFactory httpClientFactory, IAuthService authService)
    {
        _httpClient = httpClientFactory.CreateClient("PublicApiClient");
        _authService = authService;
    }

    public async Task<UserStatsSummaryDto> GetUserStatsAsync()
    {
        var response = await SendAuthorizedAsync();
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            response.Dispose();

            var refreshResult = await _authService.RefreshTokenAsync();
            if (!refreshResult.Success)
            {
                await _authService.LogoutAsync();
                throw new UnauthorizedAccessException();
            }

            response = await SendAuthorizedAsync();
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            response.Dispose();
            await _authService.LogoutAsync();
            throw new UnauthorizedAccessException();
        }

        try
        {
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserStatsSummaryDto>()
                ?? throw new InvalidOperationException("Failed to deserialize user stats");
        }
        finally
        {
            response.Dispose();
        }
    }

    private async Task<HttpResponseMessage> SendAuthorizedAsync()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/stats/user");
        var token = await _authService.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await _httpClient.SendAsync(request);
    }
}
