namespace LexiQuest.Shared.DTOs.Game;

/// <summary>
/// Game session status.
/// </summary>
public enum GameSessionStatus
{
    Active = 0,
    InProgress = 0,
    Completed = 1,
    Failed = 2,
    Abandoned = 3,
    Forfeited = 3
}
