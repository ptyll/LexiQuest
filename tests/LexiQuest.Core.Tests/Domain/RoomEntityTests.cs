using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Core.Domain.ValueObjects;
using LexiQuest.Shared.Enums;
using Xunit;

namespace LexiQuest.Core.Tests.Domain;

/// <summary>
/// Unit tests for Room domain entity - T-503.1
/// </summary>
public class RoomEntityTests
{
    private static RoomSettings DefaultSettings => new(
        WordCount: 10,
        TimeLimitMinutes: 2,
        Difficulty: DifficultyLevel.Intermediate,
        BestOf: 3);

    [Fact]
    public void Room_Create_GeneratesUniqueCode()
    {
        // Act
        var room1 = Room.Create(Guid.NewGuid(), "Player1", DefaultSettings);
        var room2 = Room.Create(Guid.NewGuid(), "Player2", DefaultSettings);

        // Assert
        room1.Code.Should().NotBe(room2.Code);
        room1.Code.Should().StartWith("LEXIQ-");
        room2.Code.Should().StartWith("LEXIQ-");
    }

    [Fact]
    public void Room_Create_CodeFormat_LEXIQ_4AlphaNum()
    {
        // Act
        var room = Room.Create(Guid.NewGuid(), "Player1", DefaultSettings);

        // Assert
        room.Code.Should().MatchRegex("^LEXIQ-[A-Z0-9]{4}$");
    }

    [Fact]
    public void Room_Create_SetsExpiresAt_5MinFromNow()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow;

        // Act
        var room = Room.Create(Guid.NewGuid(), "Player1", DefaultSettings);

        // Assert
        var afterCreate = DateTime.UtcNow;
        room.ExpiresAt.Should().BeAfter(beforeCreate.AddMinutes(4).AddSeconds(55));
        room.ExpiresAt.Should().BeBefore(afterCreate.AddMinutes(5).AddSeconds(5));
    }

    [Fact]
    public void Room_IsExpired_ReturnsTrueAfterExpiry()
    {
        // Arrange
        var room = Room.Create(Guid.NewGuid(), "Player1", DefaultSettings);
        
        // Act - simulate time passing by setting ExpiresAt in the past
        var expiryField = typeof(Room).GetProperty("ExpiresAt")!;
        expiryField.SetValue(room, DateTime.UtcNow.AddMinutes(-1));

        // Assert
        room.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void Room_IsExpired_ReturnsFalseBeforeExpiry()
    {
        // Arrange
        var room = Room.Create(Guid.NewGuid(), "Player1", DefaultSettings);

        // Assert
        room.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void Room_Create_SetsDefaultValues()
    {
        // Act
        var room = Room.Create(Guid.NewGuid(), "Player1", DefaultSettings);

        // Assert
        room.Status.Should().Be(RoomStatus.WaitingForOpponent);
        room.Player1Ready.Should().BeFalse();
        room.Player2Ready.Should().BeFalse();
        room.Player1Wins.Should().Be(0);
        room.Player2Wins.Should().Be(0);
        room.CurrentGameIndex.Should().Be(1);
        room.CurrentMatchId.Should().BeNull();
    }

    [Fact]
    public void Room_JoinRoom_AddsPlayer2()
    {
        // Arrange
        var room = Room.Create(Guid.NewGuid(), "Host", DefaultSettings);
        var player2Id = Guid.NewGuid();

        // Act
        room.JoinRoom(player2Id, "Guest");

        // Assert
        room.Player2UserId.Should().Be(player2Id);
        room.Player2Username.Should().Be("Guest");
        room.Status.Should().Be(RoomStatus.Lobby);
    }

    [Fact]
    public void Room_JoinRoom_WhenFull_ThrowsException()
    {
        // Arrange
        var room = Room.Create(Guid.NewGuid(), "Host", DefaultSettings);
        room.JoinRoom(Guid.NewGuid(), "Guest1");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => room.JoinRoom(Guid.NewGuid(), "Guest2"));
    }

    [Fact]
    public void Room_SetReady_Player1_SetsFlag()
    {
        // Arrange
        var room = Room.Create(Guid.NewGuid(), "Player1", DefaultSettings);

        // Act
        room.SetReady(room.Player1UserId);

        // Assert
        room.Player1Ready.Should().BeTrue();
    }

    [Fact]
    public void Room_SetReady_Player2_SetsFlag()
    {
        // Arrange
        var room = Room.Create(Guid.NewGuid(), "Host", DefaultSettings);
        var player2Id = Guid.NewGuid();
        room.JoinRoom(player2Id, "Guest");

        // Act
        room.SetReady(player2Id);

        // Assert
        room.Player2Ready.Should().BeTrue();
    }

    [Fact]
    public void Room_SetNotReady_Player1_ClearsFlag()
    {
        // Arrange
        var room = Room.Create(Guid.NewGuid(), "Player1", DefaultSettings);
        room.SetReady(room.Player1UserId);

        // Act
        room.SetNotReady(room.Player1UserId);

        // Assert
        room.Player1Ready.Should().BeFalse();
    }

    [Fact]
    public void Room_BothReady_WhenBothSetReady_ReturnsTrue()
    {
        // Arrange
        var room = Room.Create(Guid.NewGuid(), "Host", DefaultSettings);
        var player2Id = Guid.NewGuid();
        room.JoinRoom(player2Id, "Guest");

        // Act
        room.SetReady(room.Player1UserId);
        room.SetReady(player2Id);

        // Assert
        room.BothReady.Should().BeTrue();
    }

    [Fact]
    public void Room_BothReady_WhenOnlyOneReady_ReturnsFalse()
    {
        // Arrange
        var room = Room.Create(Guid.NewGuid(), "Host", DefaultSettings);
        var player2Id = Guid.NewGuid();
        room.JoinRoom(player2Id, "Guest");
        room.SetReady(room.Player1UserId);

        // Assert
        room.BothReady.Should().BeFalse();
    }

    [Fact]
    public void Room_StartGame_WhenBothReady_ChangesStatusToPlaying()
    {
        // Arrange
        var room = Room.Create(Guid.NewGuid(), "Host", DefaultSettings);
        var player2Id = Guid.NewGuid();
        room.JoinRoom(player2Id, "Guest");
        room.SetReady(room.Player1UserId);
        room.SetReady(player2Id);

        // Act
        room.StartGame();

        // Assert
        room.Status.Should().Be(RoomStatus.Playing);
    }

    [Fact]
    public void Room_StartGame_WhenNotBothReady_ThrowsException()
    {
        // Arrange
        var room = Room.Create(Guid.NewGuid(), "Host", DefaultSettings);
        room.JoinRoom(Guid.NewGuid(), "Guest");
        room.SetReady(room.Player1UserId); // Only one ready

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => room.StartGame());
    }

    [Fact]
    public void Room_LeaveRoom_Player2_RemovesPlayer2()
    {
        // Arrange
        var room = Room.Create(Guid.NewGuid(), "Host", DefaultSettings);
        var player2Id = Guid.NewGuid();
        room.JoinRoom(player2Id, "Guest");

        // Act
        room.LeaveRoom(player2Id);

        // Assert
        room.Player2UserId.Should().BeNull();
        room.Player2Username.Should().BeNull();
        room.Status.Should().Be(RoomStatus.WaitingForOpponent);
    }

    [Fact]
    public void Room_LeaveRoom_Player1_CancelsRoom()
    {
        // Arrange
        var room = Room.Create(Guid.NewGuid(), "Host", DefaultSettings);

        // Act
        room.LeaveRoom(room.Player1UserId);

        // Assert
        room.Status.Should().Be(RoomStatus.Cancelled);
    }

    [Fact]
    public void Room_RecordGameResult_Player1Wins_IncreasesPlayer1Wins()
    {
        // Arrange
        var room = Room.Create(Guid.NewGuid(), "Host", DefaultSettings);
        room.JoinRoom(Guid.NewGuid(), "Guest");

        // Act
        room.RecordGameResult(room.Player1UserId);

        // Assert
        room.Player1Wins.Should().Be(1);
        room.Player2Wins.Should().Be(0);
        room.CurrentGameIndex.Should().Be(2);
    }

    [Fact]
    public void Room_IsSeriesComplete_BestOf3_Player1Wins2_ReturnsTrue()
    {
        // Arrange
        var settings = new RoomSettings(10, 2, DifficultyLevel.Intermediate, 3); // Best of 3
        var room = Room.Create(Guid.NewGuid(), "Host", settings);
        room.JoinRoom(Guid.NewGuid(), "Guest");

        // Act - Player 1 wins 2 games
        room.RecordGameResult(room.Player1UserId);
        room.RecordGameResult(room.Player1UserId);

        // Assert
        room.IsSeriesComplete.Should().BeTrue();
        room.Status.Should().Be(RoomStatus.Completed);
    }

    [Fact]
    public void Room_IsSeriesComplete_BestOf5_Player2Wins3_ReturnsTrue()
    {
        // Arrange
        var settings = new RoomSettings(10, 2, DifficultyLevel.Intermediate, 5); // Best of 5
        var room = Room.Create(Guid.NewGuid(), "Host", settings);
        var player2Id = Guid.NewGuid();
        room.JoinRoom(player2Id, "Guest");

        // Act - Player 2 wins 3 games
        room.RecordGameResult(player2Id);
        room.RecordGameResult(player2Id);
        room.RecordGameResult(player2Id);

        // Assert
        room.IsSeriesComplete.Should().BeTrue();
        room.Status.Should().Be(RoomStatus.Completed);
    }

    [Fact]
    public void Room_ResetForRematch_ClearsState_KeepsSettings()
    {
        // Arrange
        var room = Room.Create(Guid.NewGuid(), "Host", DefaultSettings);
        var player2Id = Guid.NewGuid();
        room.JoinRoom(player2Id, "Guest");
        room.SetReady(room.Player1UserId);
        room.SetReady(player2Id);
        room.StartGame();
        room.RecordGameResult(room.Player1UserId);

        // Act
        room.ResetForRematch();

        // Assert
        room.Status.Should().Be(RoomStatus.Lobby);
        room.Player1Ready.Should().BeFalse();
        room.Player2Ready.Should().BeFalse();
        room.Player1Wins.Should().Be(0);
        room.Player2Wins.Should().Be(0);
        room.CurrentGameIndex.Should().Be(1);
        room.CurrentMatchId.Should().BeNull();
        room.Settings.Should().BeEquivalentTo(DefaultSettings);
    }

    [Fact]
    public void Room_HasPlayer_Player1_ReturnsTrue()
    {
        // Arrange
        var room = Room.Create(Guid.NewGuid(), "Host", DefaultSettings);

        // Assert
        room.HasPlayer(room.Player1UserId).Should().BeTrue();
    }

    [Fact]
    public void Room_HasPlayer_Player2_ReturnsTrue()
    {
        // Arrange
        var room = Room.Create(Guid.NewGuid(), "Host", DefaultSettings);
        var player2Id = Guid.NewGuid();
        room.JoinRoom(player2Id, "Guest");

        // Assert
        room.HasPlayer(player2Id).Should().BeTrue();
    }

    [Fact]
    public void Room_HasPlayer_Outsider_ReturnsFalse()
    {
        // Arrange
        var room = Room.Create(Guid.NewGuid(), "Host", DefaultSettings);

        // Assert
        room.HasPlayer(Guid.NewGuid()).Should().BeFalse();
    }

    [Fact]
    public void Room_Expire_SetsStatusToExpired()
    {
        // Arrange
        var room = Room.Create(Guid.NewGuid(), "Host", DefaultSettings);

        // Act
        room.Expire();

        // Assert
        room.Status.Should().Be(RoomStatus.Expired);
    }
}
