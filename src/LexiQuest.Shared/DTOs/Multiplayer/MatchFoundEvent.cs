namespace LexiQuest.Shared.DTOs.Multiplayer;

/// <summary>
/// Event raised when a match is found for a player.
/// </summary>
public record MatchFoundEvent(
    Guid MatchId,
    string OpponentUsername,
    int OpponentLevel,
    string? OpponentAvatar,
    DateTime StartsAt,
    bool IsPrivateRoom
);
