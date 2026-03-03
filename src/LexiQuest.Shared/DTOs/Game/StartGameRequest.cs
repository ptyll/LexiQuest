using LexiQuest.Shared.Enums;

namespace LexiQuest.Shared.DTOs.Game;

/// <summary>
/// Request to start a new game session.
/// </summary>
public record StartGameRequest(
    GameMode Mode,
    DifficultyLevel? Difficulty = null,
    Guid? PathId = null,
    int? LevelNumber = null
);
