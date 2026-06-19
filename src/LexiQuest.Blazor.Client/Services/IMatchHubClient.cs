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
    /// Event raised when opponent's progress updates.
    /// </summary>
    event EventHandler<OpponentProgressDto>? OnOpponentProgress;
    
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
    /// Event raised when room expires.
    /// </summary>
    event EventHandler? OnRoomExpired;
    
    /// <summary>
    /// Event raised when rematch is requested.
    /// </summary>
    event EventHandler<Guid>? OnRematchRequested;
    
    /// <summary>
    /// Event raised when lobby message is received.
    /// </summary>
    event EventHandler<LobbyMessageDto>? OnLobbyMessage;
    
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
    /// Leaves the current room.
    /// </summary>
    Task LeaveRoomAsync();
    
    /// <summary>
    /// Sets the player as ready.
    /// </summary>
    Task SetReadyAsync();
    
    /// <summary>
    /// Requests a rematch.
    /// </summary>
    Task RequestRematchAsync();
    
    /// <summary>
    /// Submits an answer.
    /// </summary>
    Task SubmitAnswerAsync(string answer, int timeSpentMs);
    
    /// <summary>
    /// Forfeits the current match.
    /// </summary>
    Task ForfeitAsync();
    
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
