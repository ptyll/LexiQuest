using LexiQuest.Shared.DTOs.Multiplayer;

namespace LexiQuest.Blazor.Services;

/// <summary>
/// SignalR client for multiplayer matchmaking and game coordination.
/// </summary>
public interface IMatchHubClient : IAsyncDisposable
{
    /// <summary>
    /// Current connection state.
    /// </summary>
    HubConnectionState ConnectionState { get; }
    
    /// <summary>
    /// Event raised when connection state changes.
    /// </summary>
    event EventHandler<HubConnectionState>? ConnectionStateChanged;
    
    /// <summary>
    /// Event raised when a match is found.
    /// </summary>
    event EventHandler<MatchFoundEvent>? OnMatchFound;
    
    /// <summary>
    /// Event raised when matchmaking times out.
    /// </summary>
    event EventHandler? OnMatchmakingTimeout;
    
    /// <summary>
    /// Event raised during countdown before match start.
    /// </summary>
    event EventHandler<int>? OnCountdownTick;
    
    /// <summary>
    /// Event raised when a new round starts.
    /// </summary>
    event EventHandler<MultiplayerRoundDto>? OnRoundStarted;

    /// <summary>
    /// Event raised when the current player has completed all words and waits for the opponent.
    /// </summary>
    event EventHandler? OnPlayerFinished;
    
    /// <summary>
    /// Event raised when opponent's progress updates.
    /// </summary>
    event EventHandler<OpponentProgressDto>? OnOpponentProgress;

    /// <summary>
    /// Event raised when current player's progress updates.
    /// </summary>
    event EventHandler<OpponentProgressDto>? OnPlayerProgress;
    
    /// <summary>
    /// Event raised when the match ends.
    /// </summary>
    event EventHandler<MatchResultDto>? OnMatchEnded;
    
    /// <summary>
    /// Event raised when opponent disconnects.
    /// </summary>
    event EventHandler? OnOpponentDisconnected;
    
    /// <summary>
    /// Event raised when a room is created (private rooms).
    /// </summary>
    event EventHandler<RoomCreatedEvent>? OnRoomCreated;

    /// <summary>
    /// Event raised when room creation fails.
    /// </summary>
    event EventHandler<string>? OnRoomCreationFailed;

    /// <summary>
    /// Event raised when the current player joins a private room.
    /// </summary>
    event EventHandler<string>? OnRoomJoined;

    /// <summary>
    /// Event raised when joining a private room fails.
    /// </summary>
    event EventHandler<string>? OnRoomJoinFailed;
    
    /// <summary>
    /// Event raised when a player joins the room.
    /// </summary>
    event EventHandler<PlayerJoinedRoomEvent>? OnPlayerJoinedRoom;
    
    /// <summary>
    /// Event raised when a player leaves the room.
    /// </summary>
    event EventHandler? OnPlayerLeftRoom;
    
    /// <summary>
    /// Event raised when a player is ready.
    /// </summary>
    event EventHandler<Guid>? OnPlayerReady;

    /// <summary>
    /// Event raised when a player's ready state changes.
    /// </summary>
    event EventHandler<PlayerReadyStateDto>? OnPlayerReadyStateChanged;

    /// <summary>
    /// Event raised when the room state is reset for rematch.
    /// </summary>
    event EventHandler<IReadOnlyList<PlayerReadyStateDto>>? OnRoomStateReset;
    
    /// <summary>
    /// Event raised when room expires.
    /// </summary>
    event EventHandler? OnRoomExpired;
    
    /// <summary>
    /// Event raised when rematch is requested.
    /// </summary>
    event EventHandler<Guid>? OnRematchRequested;

    /// <summary>
    /// Event raised when rematch is declined.
    /// </summary>
    event EventHandler<Guid>? OnRematchDeclined;
    
    /// <summary>
    /// Event raised when lobby message is received.
    /// </summary>
    event EventHandler<LobbyMessageDto>? OnLobbyMessage;

    /// <summary>
    /// Event raised when a lobby chat message is rejected.
    /// </summary>
    event EventHandler<string>? OnChatError;
    
    /// <summary>
    /// Starts the connection to the hub.
    /// </summary>
    Task StartAsync();
    
    /// <summary>
    /// Stops the connection to the hub.
    /// </summary>
    Task StopAsync();
    
    /// <summary>
    /// Joins the matchmaking queue.
    /// </summary>
    Task JoinMatchmakingAsync();
    
    /// <summary>
    /// Cancels matchmaking.
    /// </summary>
    Task CancelMatchmakingAsync();
    
    /// <summary>
    /// Creates a private room.
    /// </summary>
    Task CreateRoomAsync(RoomSettingsDto settings);
    
    /// <summary>
    /// Joins a private room by code.
    /// </summary>
    Task JoinRoomAsync(string roomCode);

    /// <summary>
    /// Gets current private room status.
    /// </summary>
    Task<RoomStatusDto?> GetRoomStatusAsync(string roomCode);
    
    /// <summary>
    /// Leaves the current room.
    /// </summary>
    Task LeaveRoomAsync();
    
    /// <summary>
    /// Sets the player as ready.
    /// </summary>
    Task SetReadyAsync(bool isReady = true);
    
    /// <summary>
    /// Requests a rematch.
    /// </summary>
    Task RequestRematchAsync();

    /// <summary>
    /// Accepts a rematch request.
    /// </summary>
    Task AcceptRematchAsync();

    /// <summary>
    /// Declines a rematch request.
    /// </summary>
    Task DeclineRematchAsync();

    /// <summary>
    /// Joins an existing active match group.
    /// </summary>
    Task<bool> JoinMatchAsync(Guid matchId);
    
    /// <summary>
    /// Submits an answer.
    /// </summary>
    Task SubmitAnswerAsync(string answer, int timeSpentMs);
    
    /// <summary>
    /// Forfeits the current match.
    /// </summary>
    Task ForfeitAsync();

    /// <summary>
    /// Requests server-side completion when the match timer has expired.
    /// </summary>
    Task ExpireMatchAsync();
    
    /// <summary>
    /// Sends a lobby message.
    /// </summary>
    Task SendLobbyMessageAsync(string message);
}

/// <summary>
/// Hub connection states.
/// </summary>
public enum HubConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting
}
