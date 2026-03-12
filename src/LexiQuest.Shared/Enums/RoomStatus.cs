namespace LexiQuest.Shared.Enums;

/// <summary>
/// Status of a private multiplayer room.
/// </summary>
public enum RoomStatus
{
    /// <summary>Room created, waiting for second player to join.</summary>
    WaitingForOpponent = 0,
    
    /// <summary>Both players in lobby, waiting for ready status.</summary>
    Lobby = 1,
    
    /// <summary>Countdown before game starts.</summary>
    Countdown = 2,
    
    /// <summary>Game in progress.</summary>
    Playing = 3,
    
    /// <summary>Between games in a Best of X series.</summary>
    BetweenGames = 4,
    
    /// <summary>Series completed.</summary>
    Completed = 5,
    
    /// <summary>Room expired (code timeout).</summary>
    Expired = 6,
    
    /// <summary>Room cancelled by host.</summary>
    Cancelled = 7
}
