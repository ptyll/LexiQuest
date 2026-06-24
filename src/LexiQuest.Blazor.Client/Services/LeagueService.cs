using System.Net.Http.Json;
using LexiQuest.Shared.DTOs.Leagues;

namespace LexiQuest.Blazor.Services;

public class LeagueService : ILeagueService
{
    private readonly IAuthenticatedApiClient _apiClient;

    public LeagueService(IAuthenticatedApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<LeagueInfoDto?> GetCurrentLeagueAsync()
    {
        try
        {
            return await _apiClient.GetFromJsonAsync<LeagueInfoDto>("api/v1/leagues/current");
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<List<LeagueParticipantDto>> GetLeaderboardAsync()
    {
        try
        {
            var result = await _apiClient.GetFromJsonAsync<List<LeagueParticipantDto>>("api/v1/leagues/leaderboard");
            return result ?? new List<LeagueParticipantDto>();
        }
        catch (HttpRequestException)
        {
            return new List<LeagueParticipantDto>();
        }
    }

    public async Task<List<LeagueHistoryDto>> GetLeagueHistoryAsync()
    {
        try
        {
            var result = await _apiClient.GetFromJsonAsync<List<LeagueHistoryDto>>("api/v1/leagues/history");
            return result ?? new List<LeagueHistoryDto>();
        }
        catch (HttpRequestException)
        {
            return new List<LeagueHistoryDto>();
        }
    }

    public async Task<List<LeagueRewardsDto>> GetRewardsAsync()
    {
        try
        {
            var result = await _apiClient.GetFromJsonAsync<List<LeagueRewardsDto>>("api/v1/leagues/rewards");
            return result ?? new List<LeagueRewardsDto>();
        }
        catch (HttpRequestException)
        {
            return new List<LeagueRewardsDto>();
        }
    }
}
