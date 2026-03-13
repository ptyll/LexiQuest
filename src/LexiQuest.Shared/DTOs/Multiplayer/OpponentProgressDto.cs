namespace LexiQuest.Shared.DTOs.Multiplayer;

/// <summary>
/// DTO for opponent progress update.
/// </summary>
public record OpponentProgressDto(
    int CorrectCount,
    int TotalAnswered,
    int ComboCount,
    int SequenceNumber = 0
);
