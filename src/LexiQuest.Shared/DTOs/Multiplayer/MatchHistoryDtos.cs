namespace LexiQuest.Shared.DTOs.Multiplayer;

/// <summary>
/// DTO for match history entry.
/// </summary>
public record MatchHistoryEntryDto(
    Guid MatchId,
    string OpponentUsername,
    string? OpponentAvatar,
    int YourScore,
    int OpponentScore,
    MatchResultType Result,
    int XPEarned,
    TimeSpan Duration,
    DateTime PlayedAt,
    MatchType Type,
    string? RoomCode,
    int? SeriesScoreYou,
    int? SeriesScoreOpponent
);

/// <summary>
/// DTO for paginated match history response.
/// </summary>
public record MatchHistoryResponseDto(
    IReadOnlyList<MatchHistoryEntryDto> Entries,
    int TotalCount,
    int PageNumber,
    int PageSize
);

/// <summary>
/// DTO for multiplayer statistics.
/// </summary>
public record MultiplayerStatsDto(
    int TotalMatchesPlayed,
    int Wins,
    int Losses,
    int Draws,
    double WinRatePercentage,
    int TotalXPEarned,
    MatchTypeStats QuickMatchStats,
    MatchTypeStats PrivateRoomStats
);

/// <summary>
/// Statistics for specific match type.
/// </summary>
public record MatchTypeStats(
    int MatchesPlayed,
    int Wins,
    int Losses,
    int Draws,
    double WinRatePercentage
);

/// <summary>
/// Match result type.
/// </summary>
public enum MatchResultType
{
    Win,
    Loss,
    Draw
}

/// <summary>
/// Match type.
/// </summary>
public enum MatchType
{
    QuickMatch,
    PrivateRoom
}

/// <summary>
/// Filter for match history.
/// </summary>
public enum MatchHistoryFilter
{
    All,
    QuickMatch,
    PrivateRoom
}
