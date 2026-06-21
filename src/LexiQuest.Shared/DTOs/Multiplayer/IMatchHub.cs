namespace LexiQuest.Shared.DTOs.Multiplayer;

/// <summary>
/// Server-side methods for MatchHub (called by clients).
/// </summary>
public interface IMatchHub
{
    // Quick Match
    Task<bool> JoinMatchmaking();
    Task<bool> CancelMatchmaking();
    
    // Private Rooms
    Task CreateRoom(RoomSettingsDto settings);
    Task JoinRoom(string roomCode);
    Task<RoomStatusDto?> GetRoomStatus(string roomCode);
    Task LeaveRoom();
    Task SetReady(bool isReady);
    Task RequestRematch();
    Task AcceptRematch();
    Task DeclineRematch();
    
    // Common (Quick Match + Private Room)
    Task<bool> JoinMatch(Guid matchId);
    Task SubmitAnswer(string answer, int timeSpentMs);
    Task Forfeit();
    Task ExpireMatch();
    Task SendLobbyMessage(string message);
}
