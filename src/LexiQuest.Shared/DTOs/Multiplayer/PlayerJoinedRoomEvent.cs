namespace LexiQuest.Shared.DTOs.Multiplayer;

/// <summary>
/// Event raised when a player joins a room.
/// </summary>
public record PlayerJoinedRoomEvent(
    string Username,
    int Level,
    string? Avatar,
    bool IsReady
);
