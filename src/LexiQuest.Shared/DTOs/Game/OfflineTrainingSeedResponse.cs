using LexiQuest.Shared.Enums;

namespace LexiQuest.Shared.DTOs.Game;

public record OfflineTrainingSeedResponse(
    Guid SessionId,
    int CurrentRound,
    int TotalRounds,
    int LivesRemaining,
    DifficultyLevel Difficulty,
    IReadOnlyList<OfflineTrainingWordDto> Words,
    int MaxLives = 5,
    bool IsInfiniteLives = false);

public record OfflineTrainingWordDto(
    int RoundNumber,
    string ScrambledWord,
    string CorrectAnswer,
    int WordLength,
    int TimeLimitSeconds);
