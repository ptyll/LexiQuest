using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Core.Domain.ValueObjects;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using LexiQuest.Shared.DTOs;
using LexiQuest.Shared.DTOs.Multiplayer;
using LexiQuest.Shared.Enums;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

/// <summary>
/// Unit tests for MatchHub room extension functionality.
/// Tests room state synchronization, ready status, and game start via SignalR patterns.
/// </summary>
public class MatchHubRoomExtensionTests
{
    private readonly RoomService _roomService;

    public MatchHubRoomExtensionTests()
    {
        _roomService = new RoomService();
    }

    private static RoomSettingsDto DefaultSettings => new(
        WordCount: 10,
        TimeLimitMinutes: 2,
        Difficulty: DifficultyLevel.Intermediate,
        BestOf: 3);

    [Fact]
    public async Task JoinRoomGroup_UserInRoom_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "TestPlayer";
        var (room, _) = await _roomService.CreateRoomAsync(userId, username, DefaultSettings);

        // Act
        var (success, error) = await _roomService.JoinRoomGroupAsync(userId, room!.Code);

        // Assert
        success.Should().BeTrue();
        error.Should().BeNull();
    }

    [Fact]
    public async Task JoinRoomGroup_UserNotInRoom_ReturnsError()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var ownerName = "Owner";
        var (room, _) = await _roomService.CreateRoomAsync(ownerId, ownerName, DefaultSettings);

        var outsiderId = Guid.NewGuid();

        // Act
        var (success, error) = await _roomService.JoinRoomGroupAsync(outsiderId, room!.Code);

        // Assert
        success.Should().BeFalse();
        error.Should().Contain("not a participant");
    }

    [Fact]
    public async Task LeaveRoomGroup_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "TestPlayer";
        var (room, _) = await _roomService.CreateRoomAsync(userId, username, DefaultSettings);

        // Act
        var (success, error) = await _roomService.LeaveRoomGroupAsync(userId, room!.Code);

        // Assert
        success.Should().BeTrue();
        error.Should().BeNull();
    }

    [Fact]
    public async Task GetRoomState_ReturnsFullState()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "TestPlayer";
        var (room, _) = await _roomService.CreateRoomAsync(userId, username, DefaultSettings);

        // Act
        var state = await _roomService.GetRoomStateAsync(room!.Code);

        // Assert
        state.Should().NotBeNull();
        state!.Code.Should().Be(room.Code);
        state.Player1.Should().NotBeNull();
        state.Player1!.UserId.Should().Be(userId);
        state.Player1.Username.Should().Be(username);
        state.Player2.Should().BeNull();
        state.Settings.WordCount.Should().Be(10);
        state.Settings.BestOf.Should().Be(3);
    }

    [Fact]
    public async Task GetRoomState_WithTwoPlayers_ReturnsBothPlayers()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player1Name = "Player1";
        var player2Id = Guid.NewGuid();
        var player2Name = "Player2";

        var (room, _) = await _roomService.CreateRoomAsync(player1Id, player1Name, DefaultSettings);
        await _roomService.JoinRoomAsync(player2Id, player2Name, room!.Code);

        // Act
        var state = await _roomService.GetRoomStateAsync(room.Code);

        // Assert
        state.Should().NotBeNull();
        state!.Player1.Should().NotBeNull();
        state.Player2.Should().NotBeNull();
        state.Player2!.UserId.Should().Be(player2Id);
        state.Player2.Username.Should().Be(player2Name);
    }

    [Fact]
    public async Task SetReady_UpdatesReadyState()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "TestPlayer";
        var (room, _) = await _roomService.CreateRoomAsync(userId, username, DefaultSettings);

        // Act
        var (success, readyState, error) = await _roomService.SetPlayerReadyAsync(userId, room!.Code, true);

        // Assert
        success.Should().BeTrue();
        error.Should().BeNull();
        readyState.Should().NotBeNull();
        readyState!.IsReady.Should().BeTrue();
        readyState.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task SetNotReady_UpdatesReadyState()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "TestPlayer";
        var (room, _) = await _roomService.CreateRoomAsync(userId, username, DefaultSettings);
        await _roomService.SetPlayerReadyAsync(userId, room!.Code, true);

        // Act
        var (success, readyState, error) = await _roomService.SetPlayerReadyAsync(userId, room!.Code, false);

        // Assert
        success.Should().BeTrue();
        readyState!.IsReady.Should().BeFalse();
    }

    [Fact]
    public async Task GetReadyState_WithBothPlayers_ReturnsBothStates()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player1Name = "Player1";
        var player2Id = Guid.NewGuid();
        var player2Name = "Player2";

        var (room, _) = await _roomService.CreateRoomAsync(player1Id, player1Name, DefaultSettings);
        await _roomService.JoinRoomAsync(player2Id, player2Name, room!.Code);

        await _roomService.SetPlayerReadyAsync(player1Id, room!.Code, true);
        await _roomService.SetPlayerReadyAsync(player2Id, room.Code, false);

        // Act
        var readyStates = await _roomService.GetPlayersReadyStateAsync(room.Code);

        // Assert
        readyStates.Should().HaveCount(2);
        readyStates.Should().Contain(r => r.UserId == player1Id && r.IsReady);
        readyStates.Should().Contain(r => r.UserId == player2Id && !r.IsReady);
    }

    [Fact]
    public async Task CanStartGame_BothReady_ReturnsTrue()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player1Name = "Player1";
        var player2Id = Guid.NewGuid();
        var player2Name = "Player2";

        var (room, _) = await _roomService.CreateRoomAsync(player1Id, player1Name, DefaultSettings);
        await _roomService.JoinRoomAsync(player2Id, player2Name, room!.Code);

        await _roomService.SetPlayerReadyAsync(player1Id, room!.Code, true);
        await _roomService.SetPlayerReadyAsync(player2Id, room.Code, true);

        // Act
        var canStart = await _roomService.CanStartGameAsync(room.Code);

        // Assert
        canStart.Should().BeTrue();
    }

    [Fact]
    public async Task CanStartGame_OneNotReady_ReturnsFalse()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player1Name = "Player1";
        var player2Id = Guid.NewGuid();
        var player2Name = "Player2";

        var (room, _) = await _roomService.CreateRoomAsync(player1Id, player1Name, DefaultSettings);
        await _roomService.JoinRoomAsync(player2Id, player2Name, room!.Code);

        await _roomService.SetPlayerReadyAsync(player1Id, room!.Code, true);
        await _roomService.SetPlayerReadyAsync(player2Id, room.Code, false);

        // Act
        var canStart = await _roomService.CanStartGameAsync(room.Code);

        // Assert
        canStart.Should().BeFalse();
    }

    [Fact]
    public async Task CanStartGame_MissingPlayer_ReturnsFalse()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player1Name = "Player1";

        var (room, _) = await _roomService.CreateRoomAsync(player1Id, player1Name, DefaultSettings);
        await _roomService.SetPlayerReadyAsync(player1Id, room!.Code, true);

        // Act
        var canStart = await _roomService.CanStartGameAsync(room.Code);

        // Assert
        canStart.Should().BeFalse();
    }

    [Fact]
    public async Task StartGame_BothReady_UpdatesStatusToPlaying()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player1Name = "Player1";
        var player2Id = Guid.NewGuid();
        var player2Name = "Player2";

        var (room, _) = await _roomService.CreateRoomAsync(player1Id, player1Name, DefaultSettings);
        await _roomService.JoinRoomAsync(player2Id, player2Name, room!.Code);

        await _roomService.SetPlayerReadyAsync(player1Id, room!.Code, true);
        await _roomService.SetPlayerReadyAsync(player2Id, room.Code, true);

        // Act
        var (success, matchId, error) = await _roomService.StartGameWithUserAsync(room.Code, player1Id);

        // Assert
        success.Should().BeTrue();
        error.Should().BeNull();
        matchId.Should().NotBe(Guid.Empty);

        var state = await _roomService.GetRoomStateAsync(room.Code);
        state!.Status.Should().Be(RoomStatus.Playing);
        state.CurrentMatchId.Should().Be(matchId);
    }

    [Fact]
    public async Task StartGame_NotReady_ReturnsError()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player1Name = "Player1";
        var player2Id = Guid.NewGuid();
        var player2Name = "Player2";

        var (room, _) = await _roomService.CreateRoomAsync(player1Id, player1Name, DefaultSettings);
        await _roomService.JoinRoomAsync(player2Id, player2Name, room!.Code);

        await _roomService.SetPlayerReadyAsync(player1Id, room!.Code, true);
        // Player 2 is not ready

        // Act
        var (success, matchId, error) = await _roomService.StartGameWithUserAsync(room.Code, player1Id);

        // Assert
        success.Should().BeFalse();
        error.Should().Contain("must be ready");
        matchId.Should().Be(Guid.Empty);
    }

    [Fact]
    public async Task StartGame_NonParticipant_ReturnsError()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player1Name = "Player1";
        var player2Id = Guid.NewGuid();
        var player2Name = "Player2";
        var outsiderId = Guid.NewGuid();

        var (room, _) = await _roomService.CreateRoomAsync(player1Id, player1Name, DefaultSettings);
        await _roomService.JoinRoomAsync(player2Id, player2Name, room!.Code);

        await _roomService.SetPlayerReadyAsync(player1Id, room!.Code, true);
        await _roomService.SetPlayerReadyAsync(player2Id, room.Code, true);

        // Act
        var (success, matchId, error) = await _roomService.StartGameWithUserAsync(room.Code, outsiderId);

        // Assert
        success.Should().BeFalse();
        error.Should().Contain("not a participant");
    }

    [Fact]
    public async Task GetRoomState_NonExistentRoom_ReturnsNull()
    {
        // Act
        var state = await _roomService.GetRoomStateAsync("INVALID-CODE");

        // Assert
        state.Should().BeNull();
    }
}
