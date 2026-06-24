using System.Net.Http.Json;
using LexiQuest.Shared.DTOs.Multiplayer;

namespace LexiQuest.Blazor.Services;

/// <summary>
/// HTTP client implementation for match history API.
/// </summary>
public class MatchHistoryClient : IMatchHistoryClient
{
    private readonly IAuthenticatedApiClient _apiClient;

    public MatchHistoryClient(IAuthenticatedApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<MatchHistoryResponseDto> GetHistoryAsync(
        MatchHistoryFilter filter = MatchHistoryFilter.All,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var url = $"api/v1/multiplayer/history?filter={filter}&pageNumber={pageNumber}&pageSize={pageSize}";
        var response = await _apiClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<MatchHistoryResponseDto>(cancellationToken);
        return result ?? new MatchHistoryResponseDto(
            Entries: new List<MatchHistoryEntryDto>(),
            TotalCount: 0,
            PageNumber: pageNumber,
            PageSize: pageSize);
    }

    public async Task<MultiplayerStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.GetAsync("api/v1/multiplayer/stats", cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<MultiplayerStatsDto>(cancellationToken);
        return result ?? new MultiplayerStatsDto(
            TotalMatchesPlayed: 0,
            Wins: 0,
            Losses: 0,
            Draws: 0,
            WinRatePercentage: 0,
            TotalXPEarned: 0,
            QuickMatchStats: new MatchTypeStats(0, 0, 0, 0, 0),
            PrivateRoomStats: new MatchTypeStats(0, 0, 0, 0, 0));
    }

    public async Task<MatchResultDto?> GetMatchResultAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.GetAsync($"api/v1/multiplayer/matches/{matchId}/result", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MatchResultDto>(cancellationToken);
    }
}
