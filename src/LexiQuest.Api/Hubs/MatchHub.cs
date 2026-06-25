using System.Collections.Concurrent;
using System.Security.Claims;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Multiplayer;
using LexiQuest.Shared.Enums;
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
    private readonly IMatchHistoryService _matchHistoryService;
    private readonly IXpService _xpService;
    private readonly ILeagueService _leagueService;
    private readonly IHubContext<MatchHub, IMatchClient> _hubContext;
    private readonly MultiplayerRuntimeSettings _runtimeSettings;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private static readonly ConcurrentDictionary<string, Guid> _connectionToMatch = new();
    private static readonly ConcurrentDictionary<string, string> _connectionToRoom = new();
    private static readonly ConcurrentDictionary<Guid, string> _userConnections = new();
    private static readonly ConcurrentDictionary<Guid, CancellationTokenSource> _matchmakingTimeouts = new();
    private static readonly ConcurrentDictionary<(Guid MatchId, Guid PlayerId), CancellationTokenSource> _disconnectFinalizers = new();
    private static readonly ConcurrentDictionary<string, Queue<DateTime>> _chatRateLimit = new();
    private const int MaxChatMessages = 10;
    private static readonly TimeSpan ChatRateWindow = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan QuickMatchTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan DisconnectGracePeriod = TimeSpan.FromSeconds(30);

    public MatchHub(
        IMatchmakingService matchmakingService,
        IMultiplayerGameService gameService,
        IUserService userService,
        IRoomService roomService,
        ILobbyChatService lobbyChatService,
        IMatchHistoryService matchHistoryService,
        IXpService xpService,
        ILeagueService leagueService,
        IHubContext<MatchHub, IMatchClient> hubContext,
        MultiplayerRuntimeSettings runtimeSettings,
        IServiceScopeFactory serviceScopeFactory)
    {
        _matchmakingService = matchmakingService;
        _gameService = gameService;
        _userService = userService;
        _roomService = roomService;
        _lobbyChatService = lobbyChatService;
        _matchHistoryService = matchHistoryService;
        _xpService = xpService;
        _leagueService = leagueService;
        _hubContext = hubContext;
        _runtimeSettings = runtimeSettings;
        _serviceScopeFactory = serviceScopeFactory;
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
            CancelMatchmakingTimeout(userId);
            
            // Handle disconnect from room
            if (_connectionToRoom.TryGetValue(Context.ConnectionId, out var roomCode))
            {
                await _roomService.LeaveRoomAsync(userId, roomCode);
                await Clients.OthersInGroup($"room:{roomCode}").PlayerLeftRoom();
                _connectionToRoom.TryRemove(Context.ConnectionId, out _);
            }

            // Handle disconnect from active match
            if (_connectionToMatch.TryGetValue(Context.ConnectionId, out var matchId))
            {
                await _gameService.HandleDisconnectAsync(matchId, userId);
                ScheduleDisconnectFinalization(matchId, userId);
                
                // Notify opponent
                var match = await _gameService.GetMatchStateAsync(matchId);
                if (match != null)
                {
                    var opponentId = userId == match.Player1Id ? match.Player2Id : match.Player1Id;
                    await Clients.User(opponentId.ToString()).OpponentDisconnected();
                }
            }
            
            _connectionToMatch.TryRemove(Context.ConnectionId, out _);
            _userConnections.TryRemove(userId, out _);
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    #region Quick Match

    public async Task<bool> JoinMatchmaking()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return false;

        var user = await _userService.GetProfileAsync(userId);
        if (user == null) return false;

        if (await _matchmakingService.IsInQueueAsync(userId))
        {
            return false;
        }

        _userConnections[userId] = Context.ConnectionId;

        var result = await _matchmakingService.JoinQueueAndTryMatchAsync(userId, user.Stats.Level, user.Username, user.AvatarUrl);
        if (result.Match != null)
        {
            await HandleMatchFound(result.Match);
        }
        else if (result.Joined)
        {
            ScheduleMatchmakingTimeout(userId);
        }

        return result.Joined;
    }

    public async Task<bool> CancelMatchmaking()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return false;

        var cancelled = await _matchmakingService.CancelQueueAsync(userId);
        if (cancelled)
        {
            CancelMatchmakingTimeout(userId);
        }

        return cancelled;
    }

    #endregion

    #region Private Rooms

    public async Task CreateRoom(RoomSettingsDto settings)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return;

        var user = await _userService.GetProfileAsync(userId);
        if (user == null) return;
        _userConnections[userId] = Context.ConnectionId;

        var (room, error) = await _roomService.CreateRoomAsync(userId, user.Username, settings);
        if (room == null || error != null)
        {
            await Clients.Caller.RoomCreationFailed(FormatRoomError(error, "Místnost se nepodařilo vytvořit"));
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
        _userConnections[userId] = Context.ConnectionId;

        var (room, error) = await _roomService.JoinRoomAsync(userId, user.Username, roomCode);
        if (room == null)
        {
            await Clients.Caller.RoomJoinFailed(FormatRoomError(error, "Místnost nenalezena"));
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

    public async Task<RoomStatusDto?> GetRoomStatus(string roomCode)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return null;
        }

        _userConnections[userId] = Context.ConnectionId;

        var room = await _roomService.GetRoomAsync(roomCode);
        if (room == null || !room.HasPlayer(userId))
        {
            return null;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"room:{room.Code}");
        _connectionToRoom[Context.ConnectionId] = room.Code;

        return await _roomService.GetRoomStatusAsync(room.Code);
    }

    public async Task LeaveRoom()
    {
        var userId = GetUserId();
        if (_connectionToRoom.TryGetValue(Context.ConnectionId, out var roomCode))
        {
            await _roomService.LeaveRoomAsync(userId, roomCode);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room:{roomCode}");
            _connectionToRoom.TryRemove(Context.ConnectionId, out _);

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

    public async Task DeclineRematch()
    {
        var userId = GetUserId();
        if (_connectionToRoom.TryGetValue(Context.ConnectionId, out var roomCode))
        {
            await Clients.OthersInGroup($"room:{roomCode}").RematchDeclined(userId);
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

        var room = await _roomService.GetRoomAsync(roomCode);
        if (room?.Player2UserId == null)
        {
            return;
        }

        var (success, _, error) = await _roomService.StartGameWithUserAsync(roomCode, room.Player1UserId);
        if (!success)
        {
            await Clients.Group($"room:{roomCode}").RoomJoinFailed(FormatRoomError(error, "Místnost se nepodařilo spustit"));
            return;
        }

        var settings = new RoomSettingsDto(
            room.Settings.WordCount,
            room.Settings.TimeLimitMinutes,
            room.Settings.Difficulty,
            room.Settings.BestOf);

        var matchId = await _gameService.CreateMatchAsync(
            room.Player1UserId,
            room.Player2UserId.Value,
            isPrivateRoom: true,
            settings: settings);

        await _roomService.SetCurrentMatchAsync(roomCode, matchId);

        var player1ConnectionId = _userConnections.GetValueOrDefault(room.Player1UserId);
        var player2ConnectionId = _userConnections.GetValueOrDefault(room.Player2UserId.Value);

        if (!string.IsNullOrWhiteSpace(player1ConnectionId))
        {
            await Groups.AddToGroupAsync(player1ConnectionId, $"match:{matchId}");
            _connectionToMatch[player1ConnectionId] = matchId;
        }

        if (!string.IsNullOrWhiteSpace(player2ConnectionId))
        {
            await Groups.AddToGroupAsync(player2ConnectionId, $"match:{matchId}");
            _connectionToMatch[player2ConnectionId] = matchId;
        }

        var startsAt = DateTime.UtcNow;
        var player1MatchFoundEvent = new MatchFoundEvent(
            MatchId: matchId,
            OpponentUsername: room.Player2Username ?? string.Empty,
            OpponentLevel: 1,
            OpponentAvatar: null,
            StartsAt: startsAt,
            IsPrivateRoom: true);

        var player2MatchFoundEvent = new MatchFoundEvent(
            MatchId: matchId,
            OpponentUsername: room.Player1Username,
            OpponentLevel: 1,
            OpponentAvatar: null,
            StartsAt: startsAt,
            IsPrivateRoom: true);

        if (!string.IsNullOrWhiteSpace(player1ConnectionId))
        {
            await Clients.Client(player1ConnectionId).MatchFound(player1MatchFoundEvent);
        }

        if (!string.IsNullOrWhiteSpace(player2ConnectionId))
        {
            await Clients.Client(player2ConnectionId).MatchFound(player2MatchFoundEvent);
        }

        var round = await _gameService.StartMatchAsync(matchId);
        await Clients.Group($"match:{matchId}").RoundStarted(round);
    }

    #endregion

    #region Common (Quick Match + Private Room)

    public async Task<bool> JoinMatch(Guid matchId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
        {
            return false;
        }

        var match = await _gameService.GetMatchStateAsync(matchId);
        if (match == null || (match.Player1Id != userId && match.Player2Id != userId))
        {
            return false;
        }

        if (!await _gameService.HandleReconnectAsync(matchId, userId))
        {
            return false;
        }

        CancelDisconnectFinalization(matchId, userId);

        await Groups.AddToGroupAsync(Context.ConnectionId, $"match:{matchId}");
        _connectionToMatch[Context.ConnectionId] = matchId;
        _userConnections[userId] = Context.ConnectionId;

        var currentRound = await _gameService.GetCurrentRoundAsync(matchId, userId);
        if (currentRound != null)
        {
            await Clients.Caller.RoundStarted(currentRound);
        }
        else
        {
            var playerProgress = await _gameService.GetPlayerProgressAsync(matchId, userId);
            if (playerProgress.TotalAnswered >= match.TotalRounds)
            {
                await Clients.Caller.PlayerFinished();
            }
        }

        return true;
    }

    public async Task SubmitAnswer(string answer, int timeSpentMs)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return;

        if (!_connectionToMatch.TryGetValue(Context.ConnectionId, out var matchId))
        {
            return;
        }

        var result = await _gameService.SubmitAnswerAsync(matchId, userId, answer, timeSpentMs);

        var playerProgress = await _gameService.GetPlayerProgressAsync(matchId, userId);
        await Clients.Caller.PlayerProgressUpdated(playerProgress);
        await Clients.OthersInGroup($"match:{matchId}")
            .OpponentAnswered(playerProgress);

        // If match complete, notify both players
        if (result.IsMatchComplete)
        {
            await CompleteMatchAsync(matchId);
            return;
        }

        if (result.IsPlayerComplete)
        {
            await Clients.Caller.PlayerFinished();
            return;
        }

        var nextRound = await _gameService.GetCurrentRoundAsync(matchId, userId);
        if (nextRound != null)
        {
            await Clients.Caller.RoundStarted(nextRound);
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
        await CompleteMatchAsync(matchId);
    }

    public async Task ExpireMatch()
    {
        if (!_connectionToMatch.TryGetValue(Context.ConnectionId, out var matchId))
        {
            return;
        }

        var state = await _gameService.GetMatchStateAsync(matchId);
        if (state == null || (state.IsActive && state.TimeRemaining > TimeSpan.Zero))
        {
            return;
        }

        await CompleteMatchAsync(matchId);
    }

    public async Task SendLobbyMessage(string message)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return;

        // Reject empty/whitespace-only messages
        if (string.IsNullOrWhiteSpace(message)) return;

        // Rate limiting: max 10 messages per 10 seconds per connection
        var connectionId = Context.ConnectionId;
        var timestamps = _chatRateLimit.GetOrAdd(connectionId, _ => new Queue<DateTime>());
        var isRateLimited = false;
        lock (timestamps)
        {
            var now = DateTime.UtcNow;
            while (timestamps.Count > 0 && now - timestamps.Peek() > ChatRateWindow)
            {
                timestamps.Dequeue();
            }

            if (timestamps.Count >= MaxChatMessages)
            {
                isRateLimited = true;
            }
            else
            {
                timestamps.Enqueue(now);
            }
        }

        if (isRateLimited)
        {
            await Clients.Caller.ChatError("Chat.RateLimit");
            return;
        }

        // HTML/XSS sanitization
        var sanitized = System.Text.Encodings.Web.HtmlEncoder.Default.Encode(message);

        // Truncate to 200 characters
        if (sanitized.Length > 200)
            sanitized = sanitized[..200];

        var user = await _userService.GetProfileAsync(userId);

        var lobbyMessage = new LobbyMessageDto(
            SenderUsername: user?.Username ?? "Unknown",
            Message: sanitized,
            SentAt: DateTime.UtcNow
        );

        await Clients.Group(GetCurrentRoomGroup()).LobbyMessage(lobbyMessage);
    }

    #endregion

    #region Private Helpers

    private Guid GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private static string FormatRoomError(string? error, string fallback)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            return fallback;
        }

        return error switch
        {
            "Room not found" => "Místnost nenalezena",
            "Room expired" => "Místnost vypršela",
            "Room is full" => "Místnost je plná",
            "Room no longer available" => "Místnost už není dostupná",
            "User already in another room" => "Už máš aktivní místnost",
            "User already has an active room" => "Už máš aktivní místnost",
            _ => error
        };
    }

    private async Task HandleMatchFound(MatchFoundEventArgs args)
    {
        CancelMatchmakingTimeout(args.Player1Id);
        CancelMatchmakingTimeout(args.Player2Id);

        if (!_userConnections.TryGetValue(args.Player1Id, out var player1ConnectionId) ||
            !_userConnections.TryGetValue(args.Player2Id, out var player2ConnectionId))
        {
            return;
        }

        var quickMatchTimeLimitSeconds = _runtimeSettings.QuickMatchTimeLimitSeconds;
        var matchSettings = new RoomSettingsDto(
            WordCount: 15,
            TimeLimitMinutes: Math.Max(1, (int)Math.Ceiling(quickMatchTimeLimitSeconds / 60d)),
            Difficulty: DifficultyLevel.Beginner,
            BestOf: 1)
        {
            TimeLimitSeconds = quickMatchTimeLimitSeconds
        };

        var matchId = await _gameService.CreateMatchAsync(
            args.Player1Id,
            args.Player2Id,
            isPrivateRoom: false,
            settings: matchSettings);

        // Add connections to match group
        await Groups.AddToGroupAsync(player1ConnectionId, $"match:{matchId}");
        await Groups.AddToGroupAsync(player2ConnectionId, $"match:{matchId}");
        
        // Store connection-match mapping
        _connectionToMatch[player1ConnectionId] = matchId;
        _connectionToMatch[player2ConnectionId] = matchId;
        
        // Notify both players
        var player1MatchFoundEvent = new MatchFoundEvent(
            MatchId: matchId,
            OpponentUsername: args.Player2Username,
            OpponentLevel: args.Player2Level,
            OpponentAvatar: args.Player2Avatar,
            StartsAt: DateTime.UtcNow.AddSeconds(3),
            IsPrivateRoom: false
        );

        var player2MatchFoundEvent = new MatchFoundEvent(
            MatchId: matchId,
            OpponentUsername: args.Player1Username,
            OpponentLevel: args.Player1Level,
            OpponentAvatar: args.Player1Avatar,
            StartsAt: player1MatchFoundEvent.StartsAt,
            IsPrivateRoom: false
        );

        await Clients.Client(player1ConnectionId).MatchFound(player1MatchFoundEvent);
        await Clients.Client(player2ConnectionId).MatchFound(player2MatchFoundEvent);

        await StartQuickMatchCountdownAsync(matchId);
    }

    private async Task StartQuickMatchCountdownAsync(Guid matchId)
    {
        for (int i = 3; i > 0; i--)
        {
            await Clients.Group($"match:{matchId}").CountdownTick(i);
            await Task.Delay(1000);
        }

        var round = await _gameService.StartMatchAsync(matchId);
        await Clients.Group($"match:{matchId}").RoundStarted(round);
    }

    private void ScheduleMatchmakingTimeout(Guid userId)
    {
        CancelMatchmakingTimeout(userId);

        var cts = new CancellationTokenSource();
        _matchmakingTimeouts[userId] = cts;

        _ = NotifyMatchmakingTimeoutAsync(userId, cts.Token);
    }

    private static void CancelMatchmakingTimeout(Guid userId)
    {
        if (_matchmakingTimeouts.TryRemove(userId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
    }

    private async Task NotifyMatchmakingTimeoutAsync(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(QuickMatchTimeout, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            _matchmakingTimeouts.TryRemove(userId, out _);
            await _matchmakingService.CancelQueueAsync(userId);

            if (_userConnections.TryGetValue(userId, out var connectionId))
            {
                await _hubContext.Clients.Client(connectionId).MatchmakingTimeout();
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when the player is matched, cancels, or disconnects before timeout.
        }
    }

    private void ScheduleDisconnectFinalization(Guid matchId, Guid userId)
    {
        CancelDisconnectFinalization(matchId, userId);

        var key = (matchId, userId);
        var cts = new CancellationTokenSource();
        _disconnectFinalizers[key] = cts;

        _ = FinalizeDisconnectAfterGraceAsync(matchId, userId, cts.Token);
    }

    private static void CancelDisconnectFinalization(Guid matchId, Guid userId)
    {
        var key = (matchId, userId);
        if (_disconnectFinalizers.TryRemove(key, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
    }

    private async Task FinalizeDisconnectAfterGraceAsync(
        Guid matchId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(DisconnectGracePeriod, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (_disconnectFinalizers.TryRemove((matchId, userId), out var completedCts))
            {
                completedCts.Dispose();
            }

            using var scope = _serviceScopeFactory.CreateScope();
            var gameService = scope.ServiceProvider.GetRequiredService<IMultiplayerGameService>();
            var matchHistoryService = scope.ServiceProvider.GetRequiredService<IMatchHistoryService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var xpService = scope.ServiceProvider.GetRequiredService<IXpService>();
            var leagueService = scope.ServiceProvider.GetRequiredService<ILeagueService>();
            var roomService = scope.ServiceProvider.GetRequiredService<IRoomService>();

            await gameService.FinalizeDisconnectAsync(matchId, userId);

            var state = await gameService.GetMatchStateAsync(matchId);
            if (state is { IsActive: false })
            {
                await CompleteMatchAsync(
                    matchId,
                    gameService,
                    matchHistoryService,
                    userService,
                    xpService,
                    leagueService,
                    roomService);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when the player reconnects or the match ends before the grace period.
        }
    }

    private async Task CompleteMatchAsync(Guid matchId)
    {
        await CompleteMatchAsync(
            matchId,
            _gameService,
            _matchHistoryService,
            _userService,
            _xpService,
            _leagueService,
            _roomService);
    }

    private async Task CompleteMatchAsync(
        Guid matchId,
        IMultiplayerGameService gameService,
        IMatchHistoryService matchHistoryService,
        IUserService userService,
        IXpService xpService,
        ILeagueService leagueService,
        IRoomService roomService)
    {
        var inMemoryResult = await gameService.EndMatchAsync(matchId);
        var state = await gameService.GetMatchStateAsync(matchId);
        if (state == null)
        {
            await _hubContext.Clients.Group($"match:{matchId}").MatchEnded(inMemoryResult);
            return;
        }

        CancelDisconnectFinalization(matchId, state.Player1Id);
        CancelDisconnectFinalization(matchId, state.Player2Id);

        var existingResult = await matchHistoryService.GetMatchByIdAsync(matchId);
        if (existingResult == null)
        {
            var room = await UpdatePrivateRoomSeriesAsync(matchId, inMemoryResult, roomService);
            await PersistMatchResultAsync(matchId, state, inMemoryResult, room, matchHistoryService, userService);
            await AwardMatchRewardsAsync(state, inMemoryResult, xpService, leagueService);
        }

        var player1Result = await matchHistoryService.GetMatchResultAsync(matchId, state.Player1Id);
        var player2Result = await matchHistoryService.GetMatchResultAsync(matchId, state.Player2Id);

        await SendMatchEndedAsync(state.Player1Id, player1Result ?? inMemoryResult);
        await SendMatchEndedAsync(state.Player2Id, player2Result ?? inMemoryResult);
    }

    private async Task PersistMatchResultAsync(
        Guid matchId,
        MatchStateDto state,
        MatchResultDto result,
        Room? room,
        IMatchHistoryService matchHistoryService,
        IUserService userService)
    {
        var player1 = await userService.GetProfileAsync(state.Player1Id);
        var player2 = await userService.GetProfileAsync(state.Player2Id);

        var player1LeagueXp = GetLeagueXpForPlayer(state.Player1Id, state, result);
        var player2LeagueXp = GetLeagueXpForPlayer(state.Player2Id, state, result);
        var shouldPersistSeriesScore = room?.Settings.BestOf > 1;

        await matchHistoryService.SaveMatchResultAsync(
            matchId: matchId,
            player1Id: state.Player1Id,
            player2Id: state.Player2Id,
            player1Username: player1?.Username ?? string.Empty,
            player2Username: player2?.Username ?? string.Empty,
            player1Score: result.YourScore,
            player2Score: result.OpponentScore,
            player1Time: result.YourTime,
            player2Time: result.OpponentTime,
            player1MaxCombo: result.YourResult.ComboMax,
            player2MaxCombo: result.OpponentResult.ComboMax,
            winnerId: result.WinnerId,
            isDraw: result.IsDraw,
            player1XpEarned: result.XPEarned,
            player2XpEarned: result.OpponentResult.XPEarned,
            player1LeagueXpEarned: player1LeagueXp,
            player2LeagueXpEarned: player2LeagueXp,
            isPrivateRoom: result.IsPrivateRoom,
            roomCode: room?.Code ?? result.RoomCode,
            seriesPlayer1Wins: shouldPersistSeriesScore ? room?.Player1Wins : null,
            seriesPlayer2Wins: shouldPersistSeriesScore ? room?.Player2Wins : null,
            wordCount: room?.Settings.WordCount ?? state.TotalRounds,
            timeLimitMinutes: room?.Settings.TimeLimitMinutes ?? Math.Max(1, (int)Math.Ceiling((DateTime.UtcNow - state.StartedAt + state.TimeRemaining).TotalMinutes)),
            difficulty: room?.Settings.Difficulty ?? DifficultyLevel.Beginner,
            startedAt: state.StartedAt);
    }

    private static async Task<Room?> UpdatePrivateRoomSeriesAsync(
        Guid matchId,
        MatchResultDto result,
        IRoomService roomService)
    {
        if (!result.IsPrivateRoom)
        {
            return null;
        }

        var rooms = await roomService.GetActiveRoomsAsync();
        var room = rooms.FirstOrDefault(r => r.CurrentMatchId == matchId);
        if (room == null)
        {
            return null;
        }

        if (result.WinnerId.HasValue)
        {
            await roomService.RecordGameResultAsync(room.Code, result.WinnerId.Value);
            return await roomService.GetRoomAsync(room.Code) ?? room;
        }

        return room;
    }

    private async Task AwardMatchRewardsAsync(
        MatchStateDto state,
        MatchResultDto result,
        IXpService xpService,
        ILeagueService leagueService)
    {
        await AwardPlayerRewardsAsync(
            state.Player1Id,
            result.XPEarned,
            GetLeagueXpForPlayer(state.Player1Id, state, result),
            result.IsPrivateRoom,
            xpService,
            leagueService);

        await AwardPlayerRewardsAsync(
            state.Player2Id,
            result.OpponentResult.XPEarned,
            GetLeagueXpForPlayer(state.Player2Id, state, result),
            result.IsPrivateRoom,
            xpService,
            leagueService);
    }

    private async Task AwardPlayerRewardsAsync(
        Guid userId,
        int xp,
        int leagueXp,
        bool isPrivateRoom,
        IXpService xpService,
        ILeagueService leagueService)
    {
        if (xp > 0)
        {
            await xpService.AddXpAsync(userId, xp, XpSource.Game);
        }

        if (isPrivateRoom || leagueXp <= 0)
        {
            return;
        }

        try
        {
            await leagueService.AddXPAsync(userId, leagueXp);
        }
        catch (InvalidOperationException)
        {
            var weekStart = GetCurrentUtcWeekStart();
            await leagueService.AssignUserToLeagueAsync(userId, weekStart, weekStart.AddDays(7));
            await leagueService.AddXPAsync(userId, leagueXp);
        }
    }

    private async Task SendMatchEndedAsync(Guid userId, MatchResultDto result)
    {
        if (_userConnections.TryGetValue(userId, out var connectionId))
        {
            await _hubContext.Clients.Client(connectionId).MatchEnded(result);
            return;
        }

        await _hubContext.Clients.User(userId.ToString()).MatchEnded(result);
    }

    private static int GetLeagueXpForPlayer(Guid playerId, MatchStateDto state, MatchResultDto result)
    {
        if (result.IsPrivateRoom)
        {
            return 0;
        }

        if (result.IsDraw)
        {
            return 15;
        }

        return result.WinnerId == playerId ? 50 : 15;
    }

    private static DateTime GetCurrentUtcWeekStart()
    {
        var today = DateTime.UtcNow.Date;
        var daysSinceMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return today.AddDays(-daysSinceMonday);
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
