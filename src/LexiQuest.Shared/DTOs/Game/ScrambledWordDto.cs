using LexiQuest.Shared.Enums;

namespace LexiQuest.Shared.DTOs.Game;

/// <summary>
/// DTO containing scrambled word for display.
/// </summary>
public record ScrambledWordDto(
    Guid SessionId,
    int RoundNumber,
    string ScrambledWord,
    int WordLength,
    DifficultyLevel Difficulty,
    int TimeLimitSeconds,
    int TotalRounds,
    int LivesRemaining
);
