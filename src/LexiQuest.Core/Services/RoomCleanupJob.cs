using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace LexiQuest.Core.Services;

/// <summary>
/// Background job for cleaning up expired rooms.
/// </summary>
public class RoomCleanupJob
{
    private readonly IRoomService _roomService;
    private readonly ILogger<RoomCleanupJob> _logger;

    public RoomCleanupJob(IRoomService roomService, ILogger<RoomCleanupJob> logger)
    {
        _roomService = roomService;
        _logger = logger;
    }

    /// <summary>
    /// Executes the cleanup job.
    /// </summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting room cleanup job");

        try
        {
            var activeRooms = await _roomService.GetActiveRoomsAsync(cancellationToken);
            var now = DateTime.UtcNow;
            var removedCount = 0;

            foreach (var room in activeRooms)
            {
                try
                {
                    // Remove expired rooms
                    if (room.IsExpired)
                    {
                        _logger.LogInformation(
                            "Removing expired room {RoomCode} (expired at {ExpiresAt})",
                            room.Code, room.ExpiresAt);

                        await _roomService.DeleteRoomAsync(room.Code, cancellationToken);
                        removedCount++;
                        continue;
                    }

                    // Remove completed/cancelled rooms older than 10 minutes
                    if ((room.Status == RoomStatus.Completed || room.Status == RoomStatus.Cancelled) 
                        && room.CreatedAt < now.AddMinutes(-10))
                    {
                        _logger.LogInformation(
                            "Removing finished room {RoomCode} (status: {Status}, created at {CreatedAt})",
                            room.Code, room.Status, room.CreatedAt);

                        await _roomService.DeleteRoomAsync(room.Code, cancellationToken);
                        removedCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cleaning up room {RoomCode}", room.Code);
                }
            }

            _logger.LogInformation(
                "Room cleanup job completed. Removed {RemovedCount} rooms, {RemainingCount} active rooms remaining",
                removedCount, activeRooms.Count - removedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing room cleanup job");
            throw;
        }
    }
}
