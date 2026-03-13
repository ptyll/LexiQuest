namespace LexiQuest.Shared.DTOs.Multiplayer;

/// <summary>
/// Client-side methods for MatchHub (called by server).
/// </summary>
public interface IMatchClient
{
    // Quick Match
    Task MatchFound(MatchFoundEvent match);
    Task MatchmakingTimeout();
    
    // Private Rooms
    Task RoomCreated(RoomCreatedEvent room);
    Task RoomCreationFailed(string error);
    Task RoomJoined(string roomCode);
    Task RoomJoinFailed(string error);
    Task PlayerJoinedRoom(PlayerJoinedRoomEvent player);
    Task PlayerLeftRoom();
    Task PlayerReady(Guid playerId);
    Task PlayerReadyStateChanged(PlayerReadyStateDto readyState);
    Task RoomStateReset(IReadOnlyList<PlayerReadyStateDto> readyStates);
    Task RoomExpired();
    Task RematchRequested(Guid playerId);
    Task LobbyMessage(LobbyMessageDto message);
    Task ChatError(string error);

    // Common
    Task CountdownTick(int secondsRemaining);
    Task RoundStarted(MultiplayerRoundDto round);
    Task OpponentAnswered(OpponentProgressDto progress);
    Task OpponentProgress(int correctCount, int totalAnswered);
    Task MatchEnded(MatchResultDto result);
    Task OpponentDisconnected();
}
