using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.ValueObjects;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Tests.Domain.Entities;

public class RoomTests
{
    [Fact]
    public void Create_GeneratesUniqueCode()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var settings = new RoomSettings(15, 3, DifficultyLevel.Beginner, 1);

        // Act
        var room1 = Room.Create(userId, "host", settings);
        var room2 = Room.Create(userId, "host", settings);

        // Assert
        room1.Code.Should().NotBe(room2.Code);
    }

    [Fact]
    public void Create_CodeFormat_LEXIQ_4AlphaNum()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var settings = new RoomSettings(15, 3, DifficultyLevel.Beginner, 1);

        // Act
        var room = Room.Create(userId, "host", settings);

        // Assert
        room.Code.Should().MatchRegex(@"^LEXIQ-[A-Z0-9]{4}$");
    }

    [Fact]
    public void Create_SetsExpiresAt_5MinFromNow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var settings = new RoomSettings(15, 3, DifficultyLevel.Beginner, 1);
        var beforeCreate = DateTime.UtcNow;

        // Act
        var room = Room.Create(userId, "host", settings);
        var afterCreate = DateTime.UtcNow;

        // Assert
        room.ExpiresAt.Should().BeOnOrAfter(beforeCreate.AddMinutes(5));
        room.ExpiresAt.Should().BeOnOrBefore(afterCreate.AddMinutes(5));
    }

    [Fact]
    public void IsExpired_ReturnsTrueAfterExpiry()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var settings = new RoomSettings(15, 3, DifficultyLevel.Beginner, 1);
        var room = Room.Create(userId, "host", settings);
        
        // Simulate time passing - manually set expiry to past
        room.GetType().GetProperty("ExpiresAt")?.SetValue(room, DateTime.UtcNow.AddMinutes(-1));

        // Act & Assert
        room.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_ReturnsFalseBeforeExpiry()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var settings = new RoomSettings(15, 3, DifficultyLevel.Beginner, 1);
        var room = Room.Create(userId, "host", settings);

        // Act & Assert
        room.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void Create_SetsInitialStatusToWaitingForOpponent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var settings = new RoomSettings(15, 3, DifficultyLevel.Beginner, 1);

        // Act
        var room = Room.Create(userId, "host", settings);

        // Assert
        room.Status.Should().Be(RoomStatus.WaitingForOpponent);
    }

    [Fact]
    public void Create_SetsHostAsPlayer1()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var settings = new RoomSettings(15, 3, DifficultyLevel.Beginner, 1);

        // Act
        var room = Room.Create(userId, "host", settings);

        // Assert
        room.Player1UserId.Should().Be(userId);
        room.Player1Username.Should().Be("host");
    }

    [Fact]
    public void JoinRoom_AddsPlayer2()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var guestId = Guid.NewGuid();
        var settings = new RoomSettings(15, 3, DifficultyLevel.Beginner, 1);
        var room = Room.Create(hostId, "host", settings);

        // Act
        room.JoinRoom(guestId, "guest");

        // Assert
        room.Player2UserId.Should().Be(guestId);
        room.Player2Username.Should().Be("guest");
        room.Status.Should().Be(RoomStatus.Lobby);
    }

    [Fact]
    public void JoinRoom_WhenFull_ThrowsException()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var guestId = Guid.NewGuid();
        var thirdUserId = Guid.NewGuid();
        var settings = new RoomSettings(15, 3, DifficultyLevel.Beginner, 1);
        var room = Room.Create(hostId, "host", settings);
        room.JoinRoom(guestId, "guest");

        // Act & Assert
        var act = () => room.JoinRoom(thirdUserId, "third");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SetReady_Player1_SetsPlayer1Ready()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var settings = new RoomSettings(15, 3, DifficultyLevel.Beginner, 1);
        var room = Room.Create(hostId, "host", settings);

        // Act
        room.SetReady(hostId);

        // Assert
        room.Player1Ready.Should().BeTrue();
        room.Player2Ready.Should().BeFalse();
    }

    [Fact]
    public void SetReady_BothReady_BothReadyTrue()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var guestId = Guid.NewGuid();
        var settings = new RoomSettings(15, 3, DifficultyLevel.Beginner, 1);
        var room = Room.Create(hostId, "host", settings);
        room.JoinRoom(guestId, "guest");

        // Act
        room.SetReady(hostId);
        room.SetReady(guestId);

        // Assert
        room.Player1Ready.Should().BeTrue();
        room.Player2Ready.Should().BeTrue();
        room.BothReady.Should().BeTrue();
    }

    [Fact]
    public void LeaveRoom_Host_CancelsRoom()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var settings = new RoomSettings(15, 3, DifficultyLevel.Beginner, 1);
        var room = Room.Create(hostId, "host", settings);

        // Act
        room.LeaveRoom(hostId);

        // Assert
        room.Status.Should().Be(RoomStatus.Cancelled);
    }

    [Fact]
    public void LeaveRoom_Guest_RemovesGuest()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var guestId = Guid.NewGuid();
        var settings = new RoomSettings(15, 3, DifficultyLevel.Beginner, 1);
        var room = Room.Create(hostId, "host", settings);
        room.JoinRoom(guestId, "guest");

        // Act
        room.LeaveRoom(guestId);

        // Assert
        room.Player2UserId.Should().BeNull();
        room.Player2Username.Should().BeNull();
        room.Status.Should().Be(RoomStatus.WaitingForOpponent);
    }

    [Fact]
    public void StartGame_SetsStatusToPlaying()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var guestId = Guid.NewGuid();
        var settings = new RoomSettings(15, 3, DifficultyLevel.Beginner, 1);
        var room = Room.Create(hostId, "host", settings);
        room.JoinRoom(guestId, "guest");
        room.SetReady(hostId);
        room.SetReady(guestId);

        // Act
        room.StartGame();

        // Assert
        room.Status.Should().Be(RoomStatus.Playing);
        room.CurrentGameIndex.Should().Be(1);
    }

    [Fact]
    public void RecordGameResult_Player1Wins_IncrementsPlayer1Wins()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var guestId = Guid.NewGuid();
        var settings = new RoomSettings(15, 3, DifficultyLevel.Beginner, 3);
        var room = Room.Create(hostId, "host", settings);
        room.JoinRoom(guestId, "guest");

        // Act
        room.RecordGameResult(hostId);

        // Assert
        room.Player1Wins.Should().Be(1);
        room.Player2Wins.Should().Be(0);
    }

    [Fact]
    public void IsSeriesComplete_BestOf3_Player1Wins2_ReturnsTrue()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var guestId = Guid.NewGuid();
        var settings = new RoomSettings(15, 3, DifficultyLevel.Beginner, 3);
        var room = Room.Create(hostId, "host", settings);
        room.JoinRoom(guestId, "guest");
        room.RecordGameResult(hostId);
        room.RecordGameResult(hostId);

        // Act & Assert
        room.IsSeriesComplete.Should().BeTrue();
    }

    [Fact]
    public void IsSeriesComplete_BestOf5_Player2Wins3_ReturnsTrue()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var guestId = Guid.NewGuid();
        var settings = new RoomSettings(15, 3, DifficultyLevel.Beginner, 5);
        var room = Room.Create(hostId, "host", settings);
        room.JoinRoom(guestId, "guest");
        room.RecordGameResult(guestId);
        room.RecordGameResult(guestId);
        room.RecordGameResult(guestId);

        // Act & Assert
        room.IsSeriesComplete.Should().BeTrue();
    }

    [Fact]
    public void ResetForRematch_ResetsGameState()
    {
        // Arrange
        var hostId = Guid.NewGuid();
        var guestId = Guid.NewGuid();
        var settings = new RoomSettings(15, 3, DifficultyLevel.Beginner, 3);
        var room = Room.Create(hostId, "host", settings);
        room.JoinRoom(guestId, "guest");
        room.SetReady(hostId);
        room.SetReady(guestId);
        room.StartGame();
        room.RecordGameResult(hostId);

        // Act
        room.ResetForRematch();

        // Assert
        room.Player1Ready.Should().BeFalse();
        room.Player2Ready.Should().BeFalse();
        room.Player1Wins.Should().Be(0);
        room.Player2Wins.Should().Be(0);
        room.CurrentGameIndex.Should().Be(1); // Games start at 1, not 0
        room.CurrentMatchId.Should().BeNull();
        room.Status.Should().Be(RoomStatus.Lobby);
    }
}
