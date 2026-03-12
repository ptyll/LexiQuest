namespace LexiQuest.Shared.DTOs.Multiplayer;

/// <summary>
/// DTO for match result.
/// </summary>
public record MatchResultDto(
    Guid? WinnerId,
    int YourScore,
    int OpponentScore,
    TimeSpan YourTime,
    TimeSpan OpponentTime,
    int XPEarned,
    int LeagueXPEarned,
    bool IsDraw,
    bool IsPrivateRoom,
    string? RoomCode,
    PlayerMatchResult YourResult,
    PlayerMatchResult OpponentResult
);

/// <summary>
/// DTO for individual player match result.
/// </summary>
public record PlayerMatchResult(
    string Username,
    string? Avatar,
    int CorrectCount,
    TimeSpan TotalTime,
    int ComboMax,
    int XPEarned
);
