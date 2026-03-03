namespace LexiQuest.Shared.DTOs.Game;

/// <summary>
/// Result of submitting an answer for a game round.
/// </summary>
public record GameRoundResult(
    bool IsCorrect,
    string CorrectAnswer,
    int XPEarned,
    int SpeedBonus,
    int ComboCount,
    bool IsLevelComplete,
    int LivesRemaining,
    string? NextScrambledWord = null,
    int? NextRoundNumber = null,
    bool IsGameOver = false
);
