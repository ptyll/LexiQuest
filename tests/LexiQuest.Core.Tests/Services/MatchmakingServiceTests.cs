using FluentAssertions;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

public class MatchmakingServiceTests
{
    private readonly IMatchmakingService _sut;

    public MatchmakingServiceTests()
    {
        _sut = new MatchmakingService();
    }

    [Fact]
    public async Task MatchmakingService_JoinQueue_AddsPlayer()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var level = 5;
        var username = "TestPlayer";

        // Act
        var result = await _sut.JoinQueueAsync(userId, level, username, null);

        // Assert
        result.Should().BeTrue();
        var isInQueue = await _sut.IsInQueueAsync(userId);
        isInQueue.Should().BeTrue();
        var queueCount = await _sut.GetQueueCountAsync();
        queueCount.Should().Be(1);
    }

    [Fact]
    public async Task MatchmakingService_JoinQueue_TwoPlayers_CreatesMatch()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        MatchFoundEventArgs? matchFoundEvent = null;
        
        _sut.OnMatchFound += (sender, args) => matchFoundEvent = args;

        // Act
        await _sut.JoinQueueAsync(player1Id, 5, "Player1", null);
        await _sut.JoinQueueAsync(player2Id, 6, "Player2", null);

        // Give some time for the matching algorithm
        await Task.Delay(300);

        // Assert
        matchFoundEvent.Should().NotBeNull();
        matchFoundEvent!.Player1Id.Should().Be(player1Id);
        matchFoundEvent.Player2Id.Should().Be(player2Id);
    }

    [Fact]
    public async Task MatchmakingService_CancelQueue_RemovesPlayer()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await _sut.JoinQueueAsync(userId, 5, "TestPlayer", null);

        // Act
        var result = await _sut.CancelQueueAsync(userId);

        // Assert
        result.Should().BeTrue();
        var isInQueue = await _sut.IsInQueueAsync(userId);
        isInQueue.Should().BeFalse();
        var queueCount = await _sut.GetQueueCountAsync();
        queueCount.Should().Be(0);
    }

    [Fact]
    public async Task MatchmakingService_AlreadyInQueue_RejectsDuplicate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await _sut.JoinQueueAsync(userId, 5, "TestPlayer", null);

        // Act
        var result = await _sut.JoinQueueAsync(userId, 5, "TestPlayer", null);

        // Assert
        result.Should().BeFalse();
        var queueCount = await _sut.GetQueueCountAsync();
        queueCount.Should().Be(1);
    }

    [Fact]
    public async Task MatchmakingService_MatchPlayers_SimilarLevel_Preferred()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var player3Id = Guid.NewGuid();
        MatchFoundEventArgs? matchFoundEvent = null;
        
        _sut.OnMatchFound += (sender, args) => matchFoundEvent = args;

        // Act - Player1 (level 5) and Player3 (level 7) should match first (similar level)
        // Player2 (level 20) should wait longer
        await _sut.JoinQueueAsync(player1Id, 5, "Player1", null);
        await _sut.JoinQueueAsync(player2Id, 20, "Player2", null);
        await _sut.JoinQueueAsync(player3Id, 7, "Player3", null);

        // Give some time for the matching algorithm
        await Task.Delay(200);

        // Assert
        matchFoundEvent.Should().NotBeNull();
        // Player1 and Player3 should be matched (levels 5 and 7, difference = 2)
        // They are within ±3 level range
        var matchedPlayerIds = new[] { matchFoundEvent!.Player1Id, matchFoundEvent.Player2Id };
        matchedPlayerIds.Should().Contain(player1Id);
        matchedPlayerIds.Should().Contain(player3Id);
    }

    [Fact]
    public async Task MatchmakingService_Timeout_30s_NotifiesPlayer()
    {
        // Arrange
        var userId = Guid.NewGuid();
        MatchmakingTimeoutEventArgs? timeoutEvent = null;
        
        _sut.OnMatchmakingTimeout += (sender, args) => timeoutEvent = args;
        
        // Use a shorter timeout for testing
        var sutWithShortTimeout = new MatchmakingService(TimeSpan.FromMilliseconds(50));
        MatchmakingTimeoutEventArgs? shortTimeoutEvent = null;
        sutWithShortTimeout.OnMatchmakingTimeout += (sender, args) => shortTimeoutEvent = args;

        // Act
        await sutWithShortTimeout.JoinQueueAsync(userId, 5, "TestPlayer", null);
        
        // Wait for timeout
        await Task.Delay(200);

        // Assert
        shortTimeoutEvent.Should().NotBeNull();
        shortTimeoutEvent!.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task MatchmakingService_CancelQueue_NotInQueue_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _sut.CancelQueueAsync(userId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task MatchmakingService_GetQueueCount_EmptyQueue_ReturnsZero()
    {
        // Act
        var count = await _sut.GetQueueCountAsync();

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public async Task MatchmakingService_IsInQueue_NotInQueue_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _sut.IsInQueueAsync(userId);

        // Assert
        result.Should().BeFalse();
    }
}
