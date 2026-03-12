namespace LexiQuest.Shared.DTOs;

/// <summary>
/// DTO reprezentující zprávu v lobby chatu místnosti
/// </summary>
public record LobbyChatMessageDto(
    Guid Id,
    string RoomCode,
    Guid UserId,
    string Username,
    string Content,
    DateTime Timestamp);
