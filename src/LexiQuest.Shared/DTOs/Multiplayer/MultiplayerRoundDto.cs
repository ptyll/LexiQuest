namespace LexiQuest.Shared.DTOs.Multiplayer;

/// <summary>
/// DTO for a multiplayer game round.
/// </summary>
public record MultiplayerRoundDto(
    int RoundNumber,
    string ScrambledWord,
    int WordLength,
    int TimeLimit,
    int SequenceNumber = 0
);
