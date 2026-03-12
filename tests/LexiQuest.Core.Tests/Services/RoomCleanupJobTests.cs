using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.ValueObjects;
using LexiQuest.Core.Services;
using LexiQuest.Shared.DTOs.Multiplayer;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

/// <summary>
/// Unit tests for RoomCleanupJob - T-503.4
/// </summary>
public class RoomCleanupJobTests
{
    private readonly RoomService _roomService;
    private readonly RoomCleanupJob _cleanupJob;
    private readonly ILogger<RoomCleanupJob> _logger;

    public RoomCleanupJobTests()
    {
        _roomService = new RoomService();
        _logger = Substitute.For<ILogger<RoomCleanupJob>>();
        _cleanupJob = new RoomCleanupJob(_roomService, _logger);
    }

    private static RoomSettingsDto DefaultSettings => new(
        WordCount: 10,
        TimeLimitMinutes: 2,
        Difficulty: DifficultyLevel.Intermediate,
        BestOf: 3);

    [Fact]
    public async Task Execute_RemovesExpiredRooms()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var (room, _) = await _roomService.CreateRoomAsync(hostId, "Host", DefaultSettings);
        
        // Simulate expiry by setting ExpiresAt in the past
        var expiryField = typeof(Room).GetProperty("ExpiresAt")!;
        expiryField.SetValue(room, DateTime.UtcNow.AddMinutes(-1));

        // Act
        await _cleanupJob.ExecuteAsync();

        // Assert
        var foundRoom = await _roomService.GetRoomAsync(room!.Code);
        foundRoom.Should().BeNull();
    }

    [Fact]
    public async Task Execute_KeepsActiveRooms()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var (room, _) = await _roomService.CreateRoomAsync(hostId, "Host", DefaultSettings);

        // Act
        await _cleanupJob.ExecuteAsync();

        // Assert
        var foundRoom = await _roomService.GetRoomAsync(room!.Code);
        foundRoom.Should().NotBeNull();
        foundRoom!.Code.Should().Be(room.Code);
    }

    [Fact]
    public async Task Execute_RemovesCancelledRooms()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var (room, _) = await _roomService.CreateRoomAsync(hostId, "Host", DefaultSettings);
        
        // Cancel the room
        await _roomService.LeaveRoomAsync(hostId, room!.Code);

        // Set CreatedAt to 15 minutes ago to simulate old room
        var createdField = typeof(Room).GetProperty("CreatedAt")!;
        createdField.SetValue(room, DateTime.UtcNow.AddMinutes(-15));

        // Act
        await _cleanupJob.ExecuteAsync();

        // Assert
        var foundRoom = await _roomService.GetRoomAsync(room.Code);
        foundRoom.Should().BeNull();
    }

    [Fact]
    public async Task Execute_RemovesCompletedRooms()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var guestId = Guid.NewGuid();
        var settings = new RoomSettingsDto(10, 2, DifficultyLevel.Intermediate, 1); // Best of 1
        var (room, _) = await _roomService.CreateRoomAsync(hostId, "Host", settings);
        
        await _roomService.JoinRoomAsync(guestId, "Guest", room!.Code);
        await _roomService.SetReadyAsync(hostId, room.Code);
        await _roomService.SetReadyAsync(guestId, room.Code);
        await _roomService.StartGameAsync(room.Code);
        await _roomService.RecordGameResultAsync(room.Code, hostId);

        // Set CreatedAt to 15 minutes ago
        var createdField = typeof(Room).GetProperty("CreatedAt")!;
        createdField.SetValue(room, DateTime.UtcNow.AddMinutes(-15));

        // Act
        await _cleanupJob.ExecuteAsync();

        // Assert
        var foundRoom = await _roomService.GetRoomAsync(room.Code);
        foundRoom.Should().BeNull();
    }

    [Fact]
    public async Task Execute_KeepsRecentlyCompletedRooms()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var guestId = Guid.NewGuid();
        var settings = new RoomSettingsDto(10, 2, DifficultyLevel.Intermediate, 1); // Best of 1
        var (room, _) = await _roomService.CreateRoomAsync(hostId, "Host", settings);
        
        await _roomService.JoinRoomAsync(guestId, "Guest", room!.Code);
        await _roomService.SetReadyAsync(hostId, room!.Code);
        await _roomService.SetReadyAsync(guestId, room.Code);
        await _roomService.StartGameAsync(room.Code);
        await _roomService.RecordGameResultAsync(room.Code, hostId);

        // Room completed just now - should NOT be removed

        // Act
        await _cleanupJob.ExecuteAsync();

        // Assert
        var foundRoom = await _roomService.GetRoomAsync(room.Code);
        foundRoom.Should().NotBeNull();
    }

    [Fact]
    public async Task Execute_RemovesOldCompletedRooms()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var guestId = Guid.NewGuid();
        var settings = new RoomSettingsDto(10, 2, DifficultyLevel.Intermediate, 1); // Best of 1
        var (room, _) = await _roomService.CreateRoomAsync(hostId, "Host", settings);
        
        await _roomService.JoinRoomAsync(guestId, "Guest", room!.Code);
        await _roomService.SetReadyAsync(hostId, room!.Code);
        await _roomService.SetReadyAsync(guestId, room.Code);
        await _roomService.StartGameAsync(room.Code);
        await _roomService.RecordGameResultAsync(room.Code, hostId);

        // Set CreatedAt to 15 minutes ago (old completed room)
        var createdField = typeof(Room).GetProperty("CreatedAt")!;
        createdField.SetValue(room, DateTime.UtcNow.AddMinutes(-15));

        // Act
        await _cleanupJob.ExecuteAsync();

        // Assert - old completed room should be removed
        var foundRoom = await _roomService.GetRoomAsync(room.Code);
        foundRoom.Should().BeNull();
    }
}
