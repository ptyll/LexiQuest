namespace LexiQuest.Shared.DTOs.Multiplayer;

/// <summary>
/// Server-side methods for MatchHub (called by clients).
/// </summary>
public interface IMatchHub
{
    // Quick Match
    Task JoinMatchmaking();
    Task CancelMatchmaking();
    
    // Private Rooms
    Task CreateRoom(RoomSettingsDto settings);
    Task JoinRoom(string roomCode);
    Task LeaveRoom();
    Task SetReady(bool isReady);
    Task RequestRematch();
    Task AcceptRematch();
    
    // Common (Quick Match + Private Room)
    Task SubmitAnswer(string answer, int timeSpentMs);
    Task Forfeit();
    Task SendLobbyMessage(string message);
}
