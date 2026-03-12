using System.Security.Claims;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Multiplayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LexiQuest.Api.Hubs;

/// <summary>
/// SignalR hub for multiplayer matchmaking and game coordination.
/// </summary>
[Authorize]
public class MatchHub : Hub<IMatchClient>, IMatchHub
{
    private readonly IMatchmakingService _matchmakingService;
    private readonly IMultiplayerGameService _gameService;
    private readonly IUserService _userService;
    private readonly IRoomService _roomService;
    private readonly ILobbyChatService _lobbyChatService;
    private static readonly Dictionary<string, Guid> _connectionToMatch = new();
    private static readonly Dictionary<string, string> _connectionToRoom = new();

    public MatchHub(
        IMatchmakingService matchmakingService,
        IMultiplayerGameService gameService,
        IUserService userService,
        IRoomService roomService,
        ILobbyChatService lobbyChatService)
    {
        _matchmakingService = matchmakingService;
        _gameService = gameService;
        _userService = userService;
        _roomService = roomService;
        _lobbyChatService = lobbyChatService;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId != Guid.Empty)
        {
            // Cancel matchmaking if in queue
            await _matchmakingService.CancelQueueAsync(userId);
            
            // Handle disconnect from room
            if (_connectionToRoom.TryGetValue(Context.ConnectionId, out var roomCode))
            {
                await _roomService.LeaveRoomAsync(userId, roomCode);
                await Clients.OthersInGroup($"room:{roomCode}").PlayerLeftRoom();
                _connectionToRoom.Remove(Context.ConnectionId);
            }
            
            // Handle disconnect from active match
            if (_connectionToMatch.TryGetValue(Context.ConnectionId, out var matchId))
            {
                await _gameService.HandleDisconnectAsync(matchId, userId);
                
                // Notify opponent
                var match = await _gameService.GetMatchStateAsync(matchId);
                if (match != null)
                {
                    var opponentId = userId == match.Player1Id ? match.Player2Id : match.Player1Id;
                    await Clients.User(opponentId.ToString()).OpponentDisconnected();
                }
            }
            
            _connectionToMatch.Remove(Context.ConnectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    #region Quick Match

    public async Task JoinMatchmaking()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return;

        var user = await _userService.GetProfileAsync(userId);
        if (user == null) return;

        // Subscribe to matchmaking events
        _matchmakingService.OnMatchFound += async (sender, args) =>
        {
            if (args.Player1Id == userId || args.Player2Id == userId)
            {
                await HandleMatchFound(args);
            }
        };

        _matchmakingService.OnMatchmakingTimeout += async (sender, args) =>
        {
            if (args.UserId == userId)
            {
                await Clients.Caller.MatchmakingTimeout();
            }
        };

        await _matchmakingService.JoinQueueAsync(userId, user.Stats.Level, user.Username, user.AvatarUrl);
    }

    public async Task CancelMatchmaking()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return;

        await _matchmakingService.CancelQueueAsync(userId);
    }

    #endregion

    #region Private Rooms

    public async Task CreateRoom(RoomSettingsDto settings)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return;

        var user = await _userService.GetProfileAsync(userId);
        if (user == null) return;

        var (room, error) = await _roomService.CreateRoomAsync(userId, user.Username, settings);
        if (room == null || error != null)
        {
            await Clients.Caller.RoomCreationFailed(error ?? "Failed to create room");
            return;
        }

        // Add to room group
        await Groups.AddToGroupAsync(Context.ConnectionId, $"room:{room.Code}");
        _connectionToRoom[Context.ConnectionId] = room.Code;

        // Notify creator
        var roomCreatedEvent = new RoomCreatedEvent(
            RoomCode: room.Code,
            Settings: settings,
            CreatedByUsername: user.Username,
            ExpiresAt: room.ExpiresAt
        );

        await Clients.Caller.RoomCreated(roomCreatedEvent);
    }

    public async Task JoinRoom(string roomCode)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return;

        var user = await _userService.GetProfileAsync(userId);
        if (user == null) return;

        var (room, error) = await _roomService.JoinRoomAsync(userId, user.Username, roomCode);
        if (room == null)
        {
            await Clients.Caller.RoomJoinFailed(error ?? "Room not found");
            return;
        }

        // Add to room group
        await Groups.AddToGroupAsync(Context.ConnectionId, $"room:{room.Code}");
        _connectionToRoom[Context.ConnectionId] = room.Code;

        // Notify joiner about success
        await Clients.Caller.RoomJoined(room.Code);

        // Notify others in room about new player
        if (room.Player2UserId == userId)
        {
            var playerJoinedEvent = new PlayerJoinedRoomEvent(
                Username: user.Username,
                Level: user.Stats.Level,
                Avatar: user.AvatarUrl,
                IsReady: false
            );
            await Clients.OthersInGroup($"room:{room.Code}").PlayerJoinedRoom(playerJoinedEvent);
        }
    }

    public async Task LeaveRoom()
    {
        var userId = GetUserId();
        if (_connectionToRoom.TryGetValue(Context.ConnectionId, out var roomCode))
        {
            await _roomService.LeaveRoomAsync(userId, roomCode);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room:{roomCode}");
            _connectionToRoom.Remove(Context.ConnectionId);

            // Notify others
            await Clients.OthersInGroup($"room:{roomCode}").PlayerLeftRoom();
        }
    }

    public async Task SetReady(bool isReady)
    {
        var userId = GetUserId();
        if (_connectionToRoom.TryGetValue(Context.ConnectionId, out var roomCode))
        {
            var (success, readyState, error) = await _roomService.SetPlayerReadyAsync(userId, roomCode, isReady);
            if (success && readyState != null)
            {
                // Notify all players about ready state change
                await Clients.Group($"room:{roomCode}").PlayerReadyStateChanged(readyState);

                // Check if both ready - start countdown
                var canStart = await _roomService.CanStartGameAsync(roomCode);
                if (canStart)
                {
                    await StartRoomCountdown(roomCode);
                }
            }
        }
    }

    public async Task RequestRematch()
    {
        var userId = GetUserId();
        if (_connectionToRoom.TryGetValue(Context.ConnectionId, out var roomCode))
        {
            await Clients.OthersInGroup($"room:{roomCode}").RematchRequested(userId);
        }
    }

    public async Task AcceptRematch()
    {
        var userId = GetUserId();
        if (_connectionToRoom.TryGetValue(Context.ConnectionId, out var roomCode))
        {
            var (success, error) = await _roomService.RequestRematchAsync(roomCode, userId);
            if (success)
            {
                // Reset ready states and notify both players
                var readyStates = await _roomService.GetPlayersReadyStateAsync(roomCode);
                await Clients.Group($"room:{roomCode}").RoomStateReset(readyStates);
            }
        }
    }

    private async Task StartRoomCountdown(string roomCode)
    {
        // Countdown 3-2-1
        for (int i = 3; i > 0; i--)
        {
            await Clients.Group($"room:{roomCode}").CountdownTick(i);
            await Task.Delay(1000);
        }

        // Start the game
        var (success, matchId, error) = await _roomService.StartGameWithUserAsync(roomCode, Guid.Empty);
        if (success)
        {
            // Store connection-match mapping
            _connectionToMatch[Context.ConnectionId] = matchId;

            // Start the match and get first round
            var round = await _gameService.StartMatchAsync(matchId);
            await Clients.Group($"room:{roomCode}").RoundStarted(round);
        }
    }

    #endregion

    #region Common (Quick Match + Private Room)

    public async Task SubmitAnswer(string answer, int timeSpentMs)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return;

        if (!_connectionToMatch.TryGetValue(Context.ConnectionId, out var matchId))
        {
            return;
        }

        var result = await _gameService.SubmitAnswerAsync(matchId, userId, answer, timeSpentMs);
        
        // Get opponent's progress to broadcast
        var opponentProgress = await _gameService.GetOpponentProgressAsync(matchId, userId);
        
        // Notify opponent about our progress
        await Clients.OthersInGroup($"match:{matchId}")
            .OpponentProgress(opponentProgress.CorrectCount, opponentProgress.TotalAnswered);

        // If match complete, notify both players
        if (result.IsMatchComplete)
        {
            var matchResult = await _gameService.EndMatchAsync(matchId);
            await Clients.Group($"match:{matchId}").MatchEnded(matchResult);
        }
    }

    public async Task Forfeit()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return;

        if (!_connectionToMatch.TryGetValue(Context.ConnectionId, out var matchId))
        {
            return;
        }

        await _gameService.ForfeitAsync(matchId, userId);
        
        var matchResult = await _gameService.EndMatchAsync(matchId);
        await Clients.Group($"match:{matchId}").MatchEnded(matchResult);
    }

    public async Task SendLobbyMessage(string message)
    {
        var userId = GetUserId();
        var user = await _userService.GetProfileAsync(userId);
        
        var lobbyMessage = new LobbyMessageDto(
            SenderUsername: user?.Username ?? "Unknown",
            Message: message.Length > 200 ? message[..200] : message,
            SentAt: DateTime.UtcNow
        );

        await Clients.OthersInGroup(GetCurrentRoomGroup()).LobbyMessage(lobbyMessage);
    }

    #endregion

    #region Private Helpers

    private Guid GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private async Task HandleMatchFound(MatchFoundEventArgs args)
    {
        // Create the actual match in game service
        var matchId = await _gameService.CreateMatchAsync(
            args.Player1Id, 
            args.Player2Id, 
            isPrivateRoom: false);

        // Add connections to match group
        await Groups.AddToGroupAsync(Context.ConnectionId, $"match:{matchId}");
        
        // Store connection-match mapping
        _connectionToMatch[Context.ConnectionId] = matchId;

        var currentUserId = GetUserId();
        
        // Notify both players
        var matchFoundEvent = new MatchFoundEvent(
            MatchId: matchId,
            OpponentUsername: args.Player1Id == currentUserId ? args.Player2Username : args.Player1Username,
            OpponentLevel: args.Player1Id == currentUserId ? args.Player2Level : args.Player1Level,
            OpponentAvatar: args.Player1Id == currentUserId ? args.Player2Avatar : args.Player1Avatar,
            StartsAt: DateTime.UtcNow.AddSeconds(3),
            IsPrivateRoom: false
        );

        await Clients.Caller.MatchFound(matchFoundEvent);
        
        // Start countdown
        for (int i = 3; i > 0; i--)
        {
            await Clients.Group($"match:{matchId}").CountdownTick(i);
            await Task.Delay(1000);
        }

        // Start the match
        var round = await _gameService.StartMatchAsync(matchId);
        await Clients.Group($"match:{matchId}").RoundStarted(round);
    }

    private string GetCurrentRoomGroup()
    {
        if (_connectionToRoom.TryGetValue(Context.ConnectionId, out var roomCode))
        {
            return $"room:{roomCode}";
        }
        
        if (_connectionToMatch.TryGetValue(Context.ConnectionId, out var matchId))
        {
            return $"match:{matchId}";
        }
        
        return string.Empty;
    }

    #endregion
}
