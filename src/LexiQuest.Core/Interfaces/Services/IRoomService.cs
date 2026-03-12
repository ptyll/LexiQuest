using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.DTOs.Multiplayer;

namespace LexiQuest.Core.Interfaces.Services;

/// <summary>
/// Service for managing private rooms.
/// </summary>
public interface IRoomService
{
    /// <summary>
    /// Creates a new room.
    /// </summary>
    Task<(Room? Room, string? Error)> CreateRoomAsync(Guid userId, string username, RoomSettingsDto settings, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Joins a room by code.
    /// </summary>
    Task<(Room? Room, string? Error)> JoinRoomAsync(Guid userId, string username, string roomCode, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Leaves a room.
    /// </summary>
    Task<(bool Success, string? Error)> LeaveRoomAsync(Guid userId, string roomCode, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sets player ready status and returns both ready state.
    /// </summary>
    Task<(bool Success, bool BothReady, string? Error)> SetReadyAsync(Guid userId, string roomCode, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sets player not ready status.
    /// </summary>
    Task<(bool Success, string? Error)> SetNotReadyAsync(Guid userId, string roomCode, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Starts the game in a room.
    /// </summary>
    Task<(bool Success, string? Error)> StartGameAsync(string roomCode, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Records game result and handles series progression.
    /// </summary>
    Task<(bool Success, bool SeriesComplete, string? Error)> RecordGameResultAsync(string roomCode, Guid winnerId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Resets room for rematch.
    /// </summary>
    Task<(bool Success, string? Error)> RequestRematchAsync(string roomCode, Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets room by code.
    /// </summary>
    Task<Room?> GetRoomAsync(string roomCode, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets room status DTO.
    /// </summary>
    Task<RoomStatusDto?> GetRoomStatusAsync(string roomCode, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all active rooms (for cleanup).
    /// </summary>
    Task<IReadOnlyList<Room>> GetActiveRoomsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if user is in any active room.
    /// </summary>
    Task<bool> IsUserInAnyRoomAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates the current match ID for a room.
    /// </summary>
    Task SetCurrentMatchAsync(string roomCode, Guid matchId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a room (for cleanup).
    /// </summary>
    Task DeleteRoomAsync(string roomCode, CancellationToken cancellationToken = default);

    // MatchHub Extension Methods (T-503.6)

    /// <summary>
    /// Joins SignalR group for room (called when user connects to hub).
    /// </summary>
    Task<(bool Success, string? Error)> JoinRoomGroupAsync(Guid userId, string roomCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Leaves SignalR group for room (called when user disconnects from hub).
    /// </summary>
    Task<(bool Success, string? Error)> LeaveRoomGroupAsync(Guid userId, string roomCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets full room state for SignalR broadcast.
    /// </summary>
    Task<RoomStateDto?> GetRoomStateAsync(string roomCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets player ready status and returns the state.
    /// </summary>
    Task<(bool Success, PlayerReadyStateDto? ReadyState, string? Error)> SetPlayerReadyAsync(Guid userId, string roomCode, bool isReady, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets ready state of all players.
    /// </summary>
    Task<IReadOnlyList<PlayerReadyStateDto>> GetPlayersReadyStateAsync(string roomCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if both players are ready and room can start game.
    /// </summary>
    Task<bool> CanStartGameAsync(string roomCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts game and returns match ID (with user verification).
    /// </summary>
    Task<(bool Success, Guid MatchId, string? Error)> StartGameWithUserAsync(string roomCode, Guid initiatedByUserId, CancellationToken cancellationToken = default);
}
