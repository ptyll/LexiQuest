using FluentAssertions;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

public class GuestLimiterTests : IDisposable
{
    private readonly IMemoryCache _memoryCache;
    private readonly GuestLimiter _limiter;

    public GuestLimiterTests()
    {
        _memoryCache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new MemoryCacheOptions());
        _limiter = new GuestLimiter(_memoryCache);
    }

    public void Dispose()
    {
        (_memoryCache as IDisposable)?.Dispose();
    }

    [Fact]
    public void CanStartGame_FirstGame_Allows()
    {
        // Arrange
        var ipAddress = "192.168.1.1";

        // Act
        var result = _limiter.CanStartGame(ipAddress);

        // Assert
        result.Allowed.Should().BeTrue();
        result.RemainingGames.Should().Be(4); // 5 max - 1 used = 4 remaining
    }

    [Fact]
    public void CanStartGame_5thGame_Allows()
    {
        // Arrange
        var ipAddress = "192.168.1.2";
        // Record 4 games
        for (int i = 0; i < 4; i++)
        {
            _limiter.RecordGame(ipAddress);
        }

        // Act
        var result = _limiter.CanStartGame(ipAddress);

        // Assert
        result.Allowed.Should().BeTrue();
        result.RemainingGames.Should().Be(0); // 5 max - 5 used = 0 remaining
    }

    [Fact]
    public void CanStartGame_6thGame_Denies()
    {
        // Arrange
        var ipAddress = "192.168.1.3";
        // Record 5 games
        for (int i = 0; i < 5; i++)
        {
            _limiter.RecordGame(ipAddress);
        }

        // Act
        var result = _limiter.CanStartGame(ipAddress);

        // Assert
        result.Allowed.Should().BeFalse();
        result.RemainingGames.Should().Be(0);
        result.ResetTime.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void CanStartGame_After24h_ResetsCounter()
    {
        // This test is difficult to simulate with real MemoryCache
        // We'll verify the logic works by checking the status before and after would reset
        // Arrange
        var ipAddress = "192.168.1.4";
        
        // Play 5 games
        for (int i = 0; i < 5; i++)
        {
            _limiter.RecordGame(ipAddress);
        }

        // Verify limit reached
        var statusBefore = _limiter.GetStatus(ipAddress);
        statusBefore.Remaining.Should().Be(0);

        // Act - in real scenario, 24h passes and counter resets
        // We simulate by manually removing the cache entries (simulating expiration)
        _memoryCache.Remove($"guest_games_count_{ipAddress}");
        _memoryCache.Remove($"guest_games_last_{ipAddress}");

        var result = _limiter.CanStartGame(ipAddress);

        // Assert - counter reset, so new game allowed
        result.Allowed.Should().BeTrue();
        result.RemainingGames.Should().Be(4); // Counter reset, so 4 remaining after this game
    }

    [Fact]
    public void RecordGame_IncrementsCounter()
    {
        // Arrange
        var ipAddress = "192.168.1.5";

        // Act - record 3 games
        _limiter.RecordGame(ipAddress);
        _limiter.RecordGame(ipAddress);
        _limiter.RecordGame(ipAddress);

        // Assert - check status reflects 3 games used
        var status = _limiter.GetStatus(ipAddress);
        status.Used.Should().Be(3);
        status.Remaining.Should().Be(2);
    }

    [Fact]
    public void GetStatus_ReturnsCorrectRemainingCount()
    {
        // Arrange
        var ipAddress = "192.168.1.6";
        // Play 3 games
        _limiter.RecordGame(ipAddress);
        _limiter.RecordGame(ipAddress);
        _limiter.RecordGame(ipAddress);

        // Act
        var result = _limiter.GetStatus(ipAddress);

        // Assert
        result.TotalAllowed.Should().Be(5);
        result.Used.Should().Be(3);
        result.Remaining.Should().Be(2);
    }
}
