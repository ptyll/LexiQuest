using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.DTOs.Multiplayer;

namespace LexiQuest.Core.Interfaces.Repositories;

/// <summary>
/// Repository for match results.
/// </summary>
public interface IMatchResultRepository
{
    Task<MatchResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MatchResult?> GetByMatchIdAsync(Guid matchId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MatchResult>> GetByPlayerIdAsync(Guid playerId, MatchHistoryFilter filter = MatchHistoryFilter.All, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountByPlayerIdAsync(Guid playerId, MatchHistoryFilter filter = MatchHistoryFilter.All, CancellationToken cancellationToken = default);
    Task<MultiplayerStats> GetStatsForPlayerAsync(Guid playerId, CancellationToken cancellationToken = default);
    Task AddAsync(MatchResult matchResult, CancellationToken cancellationToken = default);
    Task UpdateAsync(MatchResult matchResult, CancellationToken cancellationToken = default);
}

/// <summary>
/// Statistics model for multiplayer matches.
/// </summary>
public class MultiplayerStats
{
    public int TotalMatchesPlayed { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public double WinRatePercentage => TotalMatchesPlayed > 0 ? Math.Round((double)Wins / TotalMatchesPlayed * 100, 1) : 0;
    public int TotalXPEarned { get; set; }
    
    public MatchTypeStats QuickMatchStats { get; set; } = new();
    public MatchTypeStats PrivateRoomStats { get; set; } = new();
}

/// <summary>
/// Statistics for specific match type.
/// </summary>
public class MatchTypeStats
{
    public int MatchesPlayed { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public double WinRatePercentage => MatchesPlayed > 0 ? Math.Round((double)Wins / MatchesPlayed * 100, 1) : 0;
}
