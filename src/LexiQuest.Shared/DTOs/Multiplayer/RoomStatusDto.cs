namespace LexiQuest.Shared.DTOs.Multiplayer;

/// <summary>
/// DTO for room status in lobby.
/// </summary>
public record RoomStatusDto(
    string RoomCode,
    RoomSettingsDto Settings,
    IReadOnlyList<LobbyPlayerDto> Players,
    bool BothReady,
    DateTime ExpiresAt,
    int CurrentGameIndex,
    int BestOfTotal);

/// <summary>
/// DTO for player in lobby.
/// </summary>
public record LobbyPlayerDto(
    string Username,
    string? Avatar,
    int Level,
    bool IsHost,
    bool IsReady);
