using System.Net.Http.Json;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Blazor.Services;

public class AIChallengeClient : IAIChallengeClient
{
    private readonly HttpClient _httpClient;

    public AIChallengeClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
    }

    public async Task<PlayerAnalysisDto?> GetAnalysisAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<PlayerAnalysisDto>("api/v1/game/ai-challenge/analysis");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<AIChallengeDto?> GenerateChallengeAsync(AIChallengeType type)
    {
        try
        {
            var request = new AIChallengeRequest(type);
            var response = await _httpClient.PostAsJsonAsync("api/v1/game/ai-challenge/generate", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AIChallengeDto>();
            }
            return null;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}
