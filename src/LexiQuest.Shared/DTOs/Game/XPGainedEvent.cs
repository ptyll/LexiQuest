namespace LexiQuest.Shared.DTOs.Game;

/// <summary>
/// Event raised when a user gains XP.
/// </summary>
public record XPGainedEvent(
    int Amount,
    string Source,
    bool LeveledUp,
    int NewLevel,
    int TotalXP,
    List<UnlockableReward>? Unlocks = null
);

/// <summary>
/// Represents an unlockable reward.
/// </summary>
public record UnlockableReward(
    string Type,
    string Name,
    string Description
);
