namespace LexiQuest.Shared.DTOs.Multiplayer;

/// <summary>
/// Result of submitting an answer in a multiplayer match.
/// </summary>
public record MultiplayerAnswerResultDto(
    bool IsCorrect,
    int Score,
    bool IsMatchComplete,
    bool IsPlayerComplete
);
