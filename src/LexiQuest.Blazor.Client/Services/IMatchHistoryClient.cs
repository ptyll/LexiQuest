using LexiQuest.Shared.DTOs.Multiplayer;

namespace LexiQuest.Blazor.Services;

/// <summary>
/// Client for accessing match history API endpoints.
/// </summary>
public interface IMatchHistoryClient
{
    /// <summary>
    /// Gets match history for the current user.
    /// </summary>
    Task<MatchHistoryResponseDto> GetHistoryAsync(
        MatchHistoryFilter filter = MatchHistoryFilter.All,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiplayer statistics for the current user.
    /// </summary>
    Task<MultiplayerStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
}
