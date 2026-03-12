using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Domain.ValueObjects;

/// <summary>
/// Value object representing room settings for a private multiplayer room.
/// </summary>
public record RoomSettings(
    int WordCount,
    int TimeLimitMinutes,
    DifficultyLevel Difficulty,
    int BestOf
);
