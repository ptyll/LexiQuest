using LexiQuest.Shared.DTOs.Multiplayer;
using Microsoft.AspNetCore.SignalR.Client;

namespace LexiQuest.Blazor.Services;

/// <summary>
/// SignalR client implementation for multiplayer functionality.
/// </summary>
public class MatchHubClient : IMatchHubClient
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    private HubConnection? _hubConnection;
    private readonly CancellationTokenSource _disposeCts = new();
    
    private HubConnectionState _connectionState = HubConnectionState.Disconnected;
    public HubConnectionState ConnectionState 
    { 
        get => _connectionState;
        private set
        {
            if (_connectionState != value)
            {
                _connectionState = value;
                ConnectionStateChanged?.Invoke(this, value);
            }
        }
    }
    
    public event EventHandler<HubConnectionState>? ConnectionStateChanged;
    public event EventHandler<MatchFoundEvent>? OnMatchFound;
    public event EventHandler? OnMatchmakingTimeout;
    public event EventHandler<int>? OnCountdownTick;
    public event EventHandler<MultiplayerRoundDto>? OnRoundStarted;
    public event EventHandler? OnPlayerFinished;
    public event EventHandler<OpponentProgressDto>? OnPlayerProgress;
    public event EventHandler<OpponentProgressDto>? OnOpponentProgress;
    public event EventHandler<MatchResultDto>? OnMatchEnded;
    public event EventHandler? OnOpponentDisconnected;
    public event EventHandler<RoomCreatedEvent>? OnRoomCreated;
    public event EventHandler<string>? OnRoomCreationFailed;
    public event EventHandler<string>? OnRoomJoined;
    public event EventHandler<string>? OnRoomJoinFailed;
    public event EventHandler<PlayerJoinedRoomEvent>? OnPlayerJoinedRoom;
    public event EventHandler? OnPlayerLeftRoom;
    public event EventHandler<Guid>? OnPlayerReady;
    public event EventHandler<PlayerReadyStateDto>? OnPlayerReadyStateChanged;
    public event EventHandler<IReadOnlyList<PlayerReadyStateDto>>? OnRoomStateReset;
    public event EventHandler? OnRoomExpired;
    public event EventHandler<Guid>? OnRematchRequested;
    public event EventHandler<Guid>? OnRematchDeclined;
    public event EventHandler<LobbyMessageDto>? OnLobbyMessage;
    public event EventHandler<string>? OnChatError;

    public MatchHubClient(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    public async Task StartAsync()
    {
        if (_hubConnection?.State == Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Connected)
        {
            return;
        }

        ConnectionState = HubConnectionState.Connecting;

        var apiBaseUrl = _configuration["ApiBaseUrl"]
            ?? _configuration["ApiUrl"]
            ?? "https://localhost:5000";
        var hubUrl = $"{apiBaseUrl.TrimEnd('/')}/hubs/match";
        var token = await _authService.GetTokenAsync();

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
            })
            .WithAutomaticReconnect(new[] { 
                TimeSpan.Zero, 
                TimeSpan.FromSeconds(1), 
                TimeSpan.FromSeconds(3), 
                TimeSpan.FromSeconds(5), 
                TimeSpan.FromSeconds(10) 
            })
            .Build();

        // Connection state handlers
        _hubConnection.Reconnecting += error =>
        {
            ConnectionState = HubConnectionState.Reconnecting;
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += connectionId =>
        {
            ConnectionState = HubConnectionState.Connected;
            return Task.CompletedTask;
        };

        _hubConnection.Closed += error =>
        {
            ConnectionState = HubConnectionState.Disconnected;
            return Task.CompletedTask;
        };

        // Register event handlers
        RegisterEventHandlers();

        try
        {
            await _hubConnection.StartAsync(_disposeCts.Token);
            ConnectionState = HubConnectionState.Connected;
        }
        catch
        {
            ConnectionState = HubConnectionState.Disconnected;
            throw;
        }
    }

    public async Task StopAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync(_disposeCts.Token);
            ConnectionState = HubConnectionState.Disconnected;
        }
    }

    private void RegisterEventHandlers()
    {
        if (_hubConnection == null) return;

        // Quick Match events
        _hubConnection.On<MatchFoundEvent>("MatchFound", e => OnMatchFound?.Invoke(this, e));
        _hubConnection.On("MatchmakingTimeout", () => OnMatchmakingTimeout?.Invoke(this, EventArgs.Empty));
        
        // Common game events
        _hubConnection.On<int>("CountdownTick", s => OnCountdownTick?.Invoke(this, s));
        _hubConnection.On<MultiplayerRoundDto>("RoundStarted", r => OnRoundStarted?.Invoke(this, r));
        _hubConnection.On("PlayerFinished", () => OnPlayerFinished?.Invoke(this, EventArgs.Empty));
        _hubConnection.On<OpponentProgressDto>("PlayerProgressUpdated", progress =>
            OnPlayerProgress?.Invoke(this, progress));
        _hubConnection.On<OpponentProgressDto>("OpponentAnswered", progress =>
            OnOpponentProgress?.Invoke(this, progress));
        _hubConnection.On<MatchResultDto>("MatchEnded", r => OnMatchEnded?.Invoke(this, r));
        _hubConnection.On("OpponentDisconnected", () => OnOpponentDisconnected?.Invoke(this, EventArgs.Empty));
        
        // Private Room events
        _hubConnection.On<RoomCreatedEvent>("RoomCreated", e => OnRoomCreated?.Invoke(this, e));
        _hubConnection.On<string>("RoomCreationFailed", error => OnRoomCreationFailed?.Invoke(this, error));
        _hubConnection.On<string>("RoomJoined", code => OnRoomJoined?.Invoke(this, code));
        _hubConnection.On<string>("RoomJoinFailed", error => OnRoomJoinFailed?.Invoke(this, error));
        _hubConnection.On<PlayerJoinedRoomEvent>("PlayerJoinedRoom", e => OnPlayerJoinedRoom?.Invoke(this, e));
        _hubConnection.On("PlayerLeftRoom", () => OnPlayerLeftRoom?.Invoke(this, EventArgs.Empty));
        _hubConnection.On<Guid>("PlayerReady", id => OnPlayerReady?.Invoke(this, id));
        _hubConnection.On<PlayerReadyStateDto>("PlayerReadyStateChanged", state =>
            OnPlayerReadyStateChanged?.Invoke(this, state));
        _hubConnection.On<IReadOnlyList<PlayerReadyStateDto>>("RoomStateReset", states =>
            OnRoomStateReset?.Invoke(this, states));
        _hubConnection.On("RoomExpired", () => OnRoomExpired?.Invoke(this, EventArgs.Empty));
        _hubConnection.On<Guid>("RematchRequested", id => OnRematchRequested?.Invoke(this, id));
        _hubConnection.On<Guid>("RematchDeclined", id => OnRematchDeclined?.Invoke(this, id));
        _hubConnection.On<LobbyMessageDto>("LobbyMessage", m => OnLobbyMessage?.Invoke(this, m));
        _hubConnection.On<string>("ChatError", error => OnChatError?.Invoke(this, error));
    }

    #region Hub Methods

    public async Task JoinMatchmakingAsync()
    {
        EnsureConnected();
        await _hubConnection!.InvokeAsync<bool>("JoinMatchmaking");
    }

    public async Task CancelMatchmakingAsync()
    {
        EnsureConnected();
        await _hubConnection!.InvokeAsync<bool>("CancelMatchmaking");
    }

    public async Task CreateRoomAsync(RoomSettingsDto settings)
    {
        EnsureConnected();
        await _hubConnection!.InvokeAsync("CreateRoom", settings);
    }

    public async Task JoinRoomAsync(string roomCode)
    {
        EnsureConnected();
        await _hubConnection!.InvokeAsync("JoinRoom", roomCode);
    }

    public async Task<RoomStatusDto?> GetRoomStatusAsync(string roomCode)
    {
        EnsureConnected();
        return await _hubConnection!.InvokeAsync<RoomStatusDto?>("GetRoomStatus", roomCode);
    }

    public async Task LeaveRoomAsync()
    {
        EnsureConnected();
        await _hubConnection!.InvokeAsync("LeaveRoom");
    }

    public async Task SetReadyAsync(bool isReady = true)
    {
        EnsureConnected();
        await _hubConnection!.InvokeAsync("SetReady", isReady);
    }

    public async Task RequestRematchAsync()
    {
        EnsureConnected();
        await _hubConnection!.InvokeAsync("RequestRematch");
    }

    public async Task AcceptRematchAsync()
    {
        EnsureConnected();
        await _hubConnection!.InvokeAsync("AcceptRematch");
    }

    public async Task DeclineRematchAsync()
    {
        EnsureConnected();
        await _hubConnection!.InvokeAsync("DeclineRematch");
    }

    public async Task<bool> JoinMatchAsync(Guid matchId)
    {
        EnsureConnected();
        return await _hubConnection!.InvokeAsync<bool>("JoinMatch", matchId);
    }

    public async Task SubmitAnswerAsync(string answer, int timeSpentMs)
    {
        EnsureConnected();
        await _hubConnection!.InvokeAsync("SubmitAnswer", answer, timeSpentMs);
    }

    public async Task ForfeitAsync()
    {
        EnsureConnected();
        await _hubConnection!.InvokeAsync("Forfeit");
    }

    public async Task ExpireMatchAsync()
    {
        EnsureConnected();
        await _hubConnection!.InvokeAsync("ExpireMatch");
    }

    public async Task SendLobbyMessageAsync(string message)
    {
        EnsureConnected();
        await _hubConnection!.InvokeAsync("SendLobbyMessage", message);
    }

    #endregion

    private void EnsureConnected()
    {
        if (_hubConnection?.State != Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Connected)
        {
            throw new InvalidOperationException("Hub connection is not established. Call StartAsync first.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        _disposeCts.Cancel();
        
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
        
        _disposeCts.Dispose();
    }
}
