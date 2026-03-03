namespace LexiQuest.Shared.DTOs.Game;

/// <summary>
/// Represents the current lives status of a player.
/// </summary>
public record LivesStatus(
    int Current,
    int Max,
    DateTime? NextRegenAt,
    bool IsInfinite
);
