using System.Collections.Concurrent;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.ValueObjects;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Multiplayer;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Services;

/// <summary>
/// In-memory service for managing private rooms.
/// </summary>
public class RoomService : IRoomService
{
    // In-memory storage for rooms
    private readonly ConcurrentDictionary<string, Room> _rooms = new();
    private readonly ConcurrentDictionary<Guid, string> _userToRoomCode = new();

    public Task<(Room? Room, string? Error)> CreateRoomAsync(Guid userId, string username, RoomSettingsDto settings, CancellationToken cancellationToken = default)
    {
        // Check if user already has an active room
        if (_userToRoomCode.TryGetValue(userId, out var existingRoomCode))
        {
            return Task.FromResult<(Room?, string?)>((null, "User already has an active room"));
        }

        // Convert DTO to entity settings
        var entitySettings = new RoomSettings(
            settings.WordCount,
            settings.TimeLimitMinutes,
            settings.Difficulty,
            settings.BestOf
        );

        // Create room
        var room = Room.Create(userId, username, entitySettings);
        
        // Store room
        if (!_rooms.TryAdd(room.Code, room))
        {
            // Very unlikely - code collision
            return Task.FromResult<(Room?, string?)>((null, "Failed to create room"));
        }

        // Track user
        _userToRoomCode.TryAdd(userId, room.Code);

        return Task.FromResult<(Room?, string?)>((room, null));
    }

    public Task<(Room? Room, string? Error)> JoinRoomAsync(Guid userId, string username, string roomCode, CancellationToken cancellationToken = default)
    {
        var normalizedCode = roomCode.ToUpperInvariant();

        // Check if user already in a room
        if (_userToRoomCode.TryGetValue(userId, out var existingCode) && existingCode != normalizedCode)
        {
            return Task.FromResult<(Room?, string?)>((null, "User already in another room"));
        }

        // Find room
        if (!_rooms.TryGetValue(normalizedCode, out var room))
        {
            return Task.FromResult<(Room?, string?)>((null, "Room not found"));
        }

        // Check if expired
        if (room.IsExpired)
        {
            room.Expire();
            return Task.FromResult<(Room?, string?)>((null, "Room expired"));
        }

        // Check if cancelled or completed
        if (room.Status == RoomStatus.Cancelled || room.Status == RoomStatus.Completed)
        {
            return Task.FromResult<(Room?, string?)>((null, "Room no longer available"));
        }

        // Check if trying to join own room
        if (room.Player1UserId == userId)
        {
            return Task.FromResult<(Room?, string?)>((room, null));
        }

        // Check if room is full
        if (room.Player2UserId.HasValue)
        {
            if (room.Player2UserId == userId)
            {
                return Task.FromResult<(Room?, string?)>((room, null));
            }
            return Task.FromResult<(Room?, string?)>((null, "Room is full"));
        }

        // Join room
        room.JoinRoom(userId, username);
        _userToRoomCode.TryAdd(userId, normalizedCode);

        return Task.FromResult<(Room?, string?)>((room, null));
    }

    public Task<(bool Success, string? Error)> LeaveRoomAsync(Guid userId, string roomCode, CancellationToken cancellationToken = default)
    {
        var normalizedCode = roomCode.ToUpperInvariant();

        if (!_rooms.TryGetValue(normalizedCode, out var room))
        {
            return Task.FromResult<(bool, string?)>((false, "Room not found"));
        }

        if (!room.HasPlayer(userId))
        {
            return Task.FromResult<(bool, string?)>((false, "Not in room"));
        }

        room.LeaveRoom(userId);
        _userToRoomCode.TryRemove(userId, out _);

        // If room cancelled, completed, or expired, remove it
        if (room.Status == RoomStatus.Cancelled || room.Status == RoomStatus.Completed || room.Status == RoomStatus.Expired)
        {
            _rooms.TryRemove(normalizedCode, out _);
        }

        return Task.FromResult<(bool, string?)>((true, null));
    }

    public Task<(bool Success, bool BothReady, string? Error)> SetReadyAsync(Guid userId, string roomCode, CancellationToken cancellationToken = default)
    {
        var normalizedCode = roomCode.ToUpperInvariant();

        if (!_rooms.TryGetValue(normalizedCode, out var room))
        {
            return Task.FromResult<(bool, bool, string?)>((false, false, "Room not found"));
        }

        if (!room.HasPlayer(userId))
        {
            return Task.FromResult<(bool, bool, string?)>((false, false, "Not in room"));
        }

        room.SetReady(userId);
        return Task.FromResult<(bool, bool, string?)>((true, room.BothReady, null));
    }

    public Task<(bool Success, string? Error)> SetNotReadyAsync(Guid userId, string roomCode, CancellationToken cancellationToken = default)
    {
        var normalizedCode = roomCode.ToUpperInvariant();

        if (!_rooms.TryGetValue(normalizedCode, out var room))
        {
            return Task.FromResult<(bool, string?)>((false, "Room not found"));
        }

        if (!room.HasPlayer(userId))
        {
            return Task.FromResult<(bool, string?)>((false, "Not in room"));
        }

        room.SetNotReady(userId);
        return Task.FromResult<(bool, string?)>((true, null));
    }

    public Task<(bool Success, string? Error)> StartGameAsync(string roomCode, CancellationToken cancellationToken = default)
    {
        var normalizedCode = roomCode.ToUpperInvariant();

        if (!_rooms.TryGetValue(normalizedCode, out var room))
        {
            return Task.FromResult<(bool, string?)>((false, "Room not found"));
        }

        if (!room.BothReady)
        {
            return Task.FromResult<(bool, string?)>((false, "Both players must be ready"));
        }

        room.StartGame();
        return Task.FromResult<(bool, string?)>((true, null));
    }

    public Task<(bool Success, bool SeriesComplete, string? Error)> RecordGameResultAsync(string roomCode, Guid winnerId, CancellationToken cancellationToken = default)
    {
        var normalizedCode = roomCode.ToUpperInvariant();

        if (!_rooms.TryGetValue(normalizedCode, out var room))
        {
            return Task.FromResult<(bool, bool, string?)>((false, false, "Room not found"));
        }

        room.RecordGameResult(winnerId);
        
        return Task.FromResult<(bool, bool, string?)>((true, room.IsSeriesComplete, null));
    }

    public Task<(bool Success, string? Error)> RequestRematchAsync(string roomCode, Guid userId, CancellationToken cancellationToken = default)
    {
        var normalizedCode = roomCode.ToUpperInvariant();

        if (!_rooms.TryGetValue(normalizedCode, out var room))
        {
            return Task.FromResult<(bool, string?)>((false, "Room not found"));
        }

        if (!room.HasPlayer(userId))
        {
            return Task.FromResult<(bool, string?)>((false, "Not in room"));
        }

        room.ResetForRematch();
        return Task.FromResult<(bool, string?)>((true, null));
    }

    public Task<Room?> GetRoomAsync(string roomCode, CancellationToken cancellationToken = default)
    {
        var normalizedCode = roomCode.ToUpperInvariant();
        _rooms.TryGetValue(normalizedCode, out var room);
        return Task.FromResult(room);
    }

    public Task<RoomStatusDto?> GetRoomStatusAsync(string roomCode, CancellationToken cancellationToken = default)
    {
        var normalizedCode = roomCode.ToUpperInvariant();
        
        if (!_rooms.TryGetValue(normalizedCode, out var room))
        {
            return Task.FromResult<RoomStatusDto?>(null);
        }

        var players = new List<LobbyPlayerDto>
        {
            new(room.Player1Username, null, 1, true, room.Player1Ready)
        };

        if (room.Player2UserId.HasValue && !string.IsNullOrEmpty(room.Player2Username))
        {
            players.Add(new(room.Player2Username, null, 1, false, room.Player2Ready));
        }

        var dto = new RoomStatusDto(
            room.Code,
            new RoomSettingsDto(room.Settings.WordCount, room.Settings.TimeLimitMinutes, room.Settings.Difficulty, room.Settings.BestOf),
            players,
            room.BothReady,
            room.ExpiresAt,
            room.CurrentGameIndex,
            room.Settings.BestOf
        );

        return Task.FromResult<RoomStatusDto?>(dto);
    }

    public Task<IReadOnlyList<Room>> GetActiveRoomsAsync(CancellationToken cancellationToken = default)
    {
        // Return all rooms - cleanup logic handles filtering
        var activeRooms = _rooms.Values.ToList();
        
        return Task.FromResult<IReadOnlyList<Room>>(activeRooms);
    }

    public Task<bool> IsUserInAnyRoomAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_userToRoomCode.ContainsKey(userId));
    }

    public Task SetCurrentMatchAsync(string roomCode, Guid matchId, CancellationToken cancellationToken = default)
    {
        var normalizedCode = roomCode.ToUpperInvariant();
        
        if (_rooms.TryGetValue(normalizedCode, out var room))
        {
            room.CurrentMatchId = matchId;
        }

        return Task.CompletedTask;
    }

    public Task DeleteRoomAsync(string roomCode, CancellationToken cancellationToken = default)
    {
        var normalizedCode = roomCode.ToUpperInvariant();
        
        if (_rooms.TryRemove(normalizedCode, out var room))
        {
            _userToRoomCode.TryRemove(room.Player1UserId, out _);
            if (room.Player2UserId.HasValue)
            {
                _userToRoomCode.TryRemove(room.Player2UserId.Value, out _);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Cleans up expired rooms (called by background job).
    /// </summary>
    public Task CleanupExpiredRoomsAsync(CancellationToken cancellationToken = default)
    {
        var expiredRooms = _rooms.Values.Where(r => r.IsExpired).ToList();
        
        foreach (var room in expiredRooms)
        {
            room.Expire();
            _rooms.TryRemove(room.Code, out _);
            
            _userToRoomCode.TryRemove(room.Player1UserId, out _);
            if (room.Player2UserId.HasValue)
            {
                _userToRoomCode.TryRemove(room.Player2UserId.Value, out _);
            }
        }

        return Task.CompletedTask;
    }

    #region MatchHub Extension Methods (T-503.6)

    public Task<(bool Success, string? Error)> JoinRoomGroupAsync(Guid userId, string roomCode, CancellationToken cancellationToken = default)
    {
        var normalizedCode = roomCode.ToUpperInvariant();

        if (!_rooms.TryGetValue(normalizedCode, out var room))
        {
            return Task.FromResult<(bool, string?)>((false, "Room not found"));
        }

        if (!room.HasPlayer(userId))
        {
            return Task.FromResult<(bool, string?)>((false, "User is not a participant in this room"));
        }

        // In real SignalR, this would add to group - here we just validate
        return Task.FromResult<(bool, string?)>((true, null));
    }

    public Task<(bool Success, string? Error)> LeaveRoomGroupAsync(Guid userId, string roomCode, CancellationToken cancellationToken = default)
    {
        var normalizedCode = roomCode.ToUpperInvariant();

        if (!_rooms.TryGetValue(normalizedCode, out var room))
        {
            return Task.FromResult<(bool, string?)>((true, null)); // Already gone
        }

        // In real SignalR, this would remove from group - here we just validate
        return Task.FromResult<(bool, string?)>((true, null));
    }

    public Task<RoomStateDto?> GetRoomStateAsync(string roomCode, CancellationToken cancellationToken = default)
    {
        var normalizedCode = roomCode.ToUpperInvariant();

        if (!_rooms.TryGetValue(normalizedCode, out var room))
        {
            return Task.FromResult<RoomStateDto?>(null);
        }

        var settingsDto = new RoomSettingsDto(
            room.Settings.WordCount,
            room.Settings.TimeLimitMinutes,
            room.Settings.Difficulty,
            room.Settings.BestOf
        );

        var player1Dto = new PlayerDto(
            room.Player1UserId,
            room.Player1Username,
            room.Player1Ready
        );

        PlayerDto? player2Dto = null;
        if (room.Player2UserId.HasValue)
        {
            player2Dto = new PlayerDto(
                room.Player2UserId.Value,
                room.Player2Username ?? string.Empty,
                room.Player2Ready
            );
        }

        var state = new RoomStateDto(
            room.Code,
            room.Status,
            player1Dto,
            player2Dto,
            settingsDto,
            room.CurrentMatchId,
            room.ExpiresAt
        );

        return Task.FromResult<RoomStateDto?>(state);
    }

    public Task<(bool Success, PlayerReadyStateDto? ReadyState, string? Error)> SetPlayerReadyAsync(Guid userId, string roomCode, bool isReady, CancellationToken cancellationToken = default)
    {
        var normalizedCode = roomCode.ToUpperInvariant();

        if (!_rooms.TryGetValue(normalizedCode, out var room))
        {
            return Task.FromResult<(bool, PlayerReadyStateDto?, string?)>((false, null, "Room not found"));
        }

        if (!room.HasPlayer(userId))
        {
            return Task.FromResult<(bool, PlayerReadyStateDto?, string?)>((false, null, "Not in room"));
        }

        if (isReady)
        {
            room.SetReady(userId);
        }
        else
        {
            room.SetNotReady(userId);
        }

        var username = room.Player1UserId == userId ? room.Player1Username : room.Player2Username ?? string.Empty;
        var readyState = new PlayerReadyStateDto(userId, username, isReady);

        return Task.FromResult<(bool, PlayerReadyStateDto?, string?)>((true, readyState, null));
    }

    public Task<IReadOnlyList<PlayerReadyStateDto>> GetPlayersReadyStateAsync(string roomCode, CancellationToken cancellationToken = default)
    {
        var normalizedCode = roomCode.ToUpperInvariant();

        if (!_rooms.TryGetValue(normalizedCode, out var room))
        {
            return Task.FromResult<IReadOnlyList<PlayerReadyStateDto>>(Array.Empty<PlayerReadyStateDto>());
        }

        var result = new List<PlayerReadyStateDto>
        {
            new(room.Player1UserId, room.Player1Username, room.Player1Ready)
        };

        if (room.Player2UserId.HasValue)
        {
            result.Add(new(room.Player2UserId.Value, room.Player2Username ?? string.Empty, room.Player2Ready));
        }

        return Task.FromResult<IReadOnlyList<PlayerReadyStateDto>>(result);
    }

    public Task<bool> CanStartGameAsync(string roomCode, CancellationToken cancellationToken = default)
    {
        var normalizedCode = roomCode.ToUpperInvariant();

        if (!_rooms.TryGetValue(normalizedCode, out var room))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(room.BothReady);
    }

    public Task<(bool Success, Guid MatchId, string? Error)> StartGameWithUserAsync(string roomCode, Guid initiatedByUserId, CancellationToken cancellationToken = default)
    {
        var normalizedCode = roomCode.ToUpperInvariant();

        if (!_rooms.TryGetValue(normalizedCode, out var room))
        {
            return Task.FromResult<(bool, Guid, string?)>((false, Guid.Empty, "Room not found"));
        }

        if (!room.HasPlayer(initiatedByUserId))
        {
            return Task.FromResult<(bool, Guid, string?)>((false, Guid.Empty, "User is not a participant in this room"));
        }

        if (!room.BothReady)
        {
            return Task.FromResult<(bool, Guid, string?)>((false, Guid.Empty, "Both players must be ready"));
        }

        room.StartGame();

        // Generate match ID
        var matchId = Guid.NewGuid();
        room.CurrentMatchId = matchId;

        return Task.FromResult<(bool, Guid, string?)>((true, matchId, null));
    }

    #endregion
}
