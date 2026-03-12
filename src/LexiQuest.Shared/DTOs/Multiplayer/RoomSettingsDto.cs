using LexiQuest.Shared.Enums;

namespace LexiQuest.Shared.DTOs.Multiplayer;

/// <summary>
/// DTO for room settings when creating a private room.
/// </summary>
public record RoomSettingsDto(
    int WordCount,
    int TimeLimitMinutes,
    DifficultyLevel Difficulty,
    int BestOf
);
