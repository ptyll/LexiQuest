using System.Net.Http.Json;

namespace LexiQuest.Blazor.Services;

public class StatsService : IStatsService
{
    private readonly HttpClient _httpClient;

    public StatsService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
    }

    public async Task<UserStatsDto> GetUserStatsAsync()
    {
        var response = await _httpClient.GetAsync("api/v1/stats/user");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserStatsDto>()
            ?? throw new InvalidOperationException("Failed to deserialize user stats");
    }
}
