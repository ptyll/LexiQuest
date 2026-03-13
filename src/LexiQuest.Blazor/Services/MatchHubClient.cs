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
    public event EventHandler<OpponentProgressDto>? OnOpponentProgress;
    public event EventHandler<MatchResultDto>? OnMatchEnded;
    public event EventHandler? OnOpponentDisconnected;
    public event EventHandler<RoomCreatedEvent>? OnRoomCreated;
    public event EventHandler<PlayerJoinedRoomEvent>? OnPlayerJoinedRoom;
    public event EventHandler? OnPlayerLeftRoom;
    public event EventHandler<Guid>? OnPlayerReady;
    public event EventHandler? OnRoomExpired;
    public event EventHandler<Guid>? OnRematchRequested;
    public event EventHandler<LobbyMessageDto>? OnLobbyMessage;

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

        var hubUrl = $"{_configuration["ApiUrl"]}hubs/match";
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
        _hubConnection.On<OpponentProgressDto>("OpponentAnswered", progress =>
            OnOpponentProgress?.Invoke(this, progress));
        _hubConnection.On<MatchResultDto>("MatchEnded", r => OnMatchEnded?.Invoke(this, r));
        _hubConnection.On("OpponentDisconnected", () => OnOpponentDisconnected?.Invoke(this, EventArgs.Empty));
        
        // Private Room events
        _hubConnection.On<RoomCreatedEvent>("RoomCreated", e => OnRoomCreated?.Invoke(this, e));
        _hubConnection.On<PlayerJoinedRoomEvent>("PlayerJoinedRoom", e => OnPlayerJoinedRoom?.Invoke(this, e));
        _hubConnection.On("PlayerLeftRoom", () => OnPlayerLeftRoom?.Invoke(this, EventArgs.Empty));
        _hubConnection.On<Guid>("PlayerReady", id => OnPlayerReady?.Invoke(this, id));
        _hubConnection.On("RoomExpired", () => OnRoomExpired?.Invoke(this, EventArgs.Empty));
        _hubConnection.On<Guid>("RematchRequested", id => OnRematchRequested?.Invoke(this, id));
        _hubConnection.On<LobbyMessageDto>("LobbyMessage", m => OnLobbyMessage?.Invoke(this, m));
    }

    #region Hub Methods

    public async Task JoinMatchmakingAsync()
    {
        EnsureConnected();
        await _hubConnection!.InvokeAsync("JoinMatchmaking");
    }

    public async Task CancelMatchmakingAsync()
    {
        EnsureConnected();
        await _hubConnection!.InvokeAsync("CancelMatchmaking");
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

    public async Task LeaveRoomAsync()
    {
        EnsureConnected();
        await _hubConnection!.InvokeAsync("LeaveRoom");
    }

    public async Task SetReadyAsync()
    {
        EnsureConnected();
        await _hubConnection!.InvokeAsync("SetReady");
    }

    public async Task RequestRematchAsync()
    {
        EnsureConnected();
        await _hubConnection!.InvokeAsync("RequestRematch");
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
