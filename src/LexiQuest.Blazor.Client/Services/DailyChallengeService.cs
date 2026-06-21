using System.Net.Http.Json;
using LexiQuest.Shared.DTOs.Game;

namespace LexiQuest.Blazor.Services;

public class DailyChallengeService : IDailyChallengeService
{
    private readonly HttpClient _httpClient;

    public DailyChallengeService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
    }

    public async Task<DailyChallengeDto?> GetTodayAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<DailyChallengeDto>("api/v1/game/daily");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<List<DailyLeaderboardEntryDto>> GetLeaderboardAsync()
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<DailyLeaderboardEntryDto>>("api/v1/game/daily/leaderboard");
            return result ?? new List<DailyLeaderboardEntryDto>();
        }
        catch (HttpRequestException)
        {
            return new List<DailyLeaderboardEntryDto>();
        }
    }

    public async Task<bool> HasCompletedTodayAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<bool>("api/v1/game/daily/completed");
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    public async Task<ChallengeResultDto?> SubmitAnswerAsync(string answer, TimeSpan timeTaken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/v1/game/daily/submit",
                new DailyChallengeSubmitRequest(answer, (int)Math.Max(0, timeTaken.TotalMilliseconds)));

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ChallengeResultDto>();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
}
