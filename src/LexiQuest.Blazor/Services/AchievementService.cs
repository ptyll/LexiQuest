using System.Net.Http.Json;
using LexiQuest.Shared.DTOs.Achievements;

namespace LexiQuest.Blazor.Services;

public class AchievementService : IAchievementService
{
    private readonly HttpClient _httpClient;

    public AchievementService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
    }

    public async Task<List<AchievementDto>> GetAchievementsAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<AchievementDto>>("api/v1/achievements");
            return result ?? new List<AchievementDto>();
        }
        catch (HttpRequestException)
        {
            return new List<AchievementDto>();
        }
    }
}
