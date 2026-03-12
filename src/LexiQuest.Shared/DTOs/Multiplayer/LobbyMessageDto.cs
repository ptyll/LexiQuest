namespace LexiQuest.Shared.DTOs.Multiplayer;

/// <summary>
/// DTO for lobby chat message.
/// </summary>
public record LobbyMessageDto(
    string SenderUsername,
    string Message,
    DateTime SentAt
);
