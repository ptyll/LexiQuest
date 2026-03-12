namespace LexiQuest.Shared.DTOs.Multiplayer;

/// <summary>
/// Event raised when a private room is created.
/// </summary>
public record RoomCreatedEvent(
    string RoomCode,
    RoomSettingsDto Settings,
    string CreatedByUsername,
    DateTime ExpiresAt
);
