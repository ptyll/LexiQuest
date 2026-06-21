using System.Security.Claims;
using FluentAssertions;
using LexiQuest.Api.Hubs;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Multiplayer;
using LexiQuest.Shared.DTOs.Users;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace LexiQuest.Api.Tests.Hubs;

public class MatchHubTests
{
    private readonly IMatchmakingService _matchmakingService;
    private readonly IMultiplayerGameService _gameService;
    private readonly IUserService _userService;
    private readonly MatchHub _sut;
    private readonly HubCallerContext _context;
    private readonly IGroupManager _groups;
    private readonly string _connectionId = "test-connection-id";

    public MatchHubTests()
    {
        _matchmakingService = Substitute.For<IMatchmakingService>();
        _gameService = Substitute.For<IMultiplayerGameService>();
        _userService = Substitute.For<IUserService>();
        _context = Substitute.For<HubCallerContext>();
        _groups = Substitute.For<IGroupManager>();
        
        _context.ConnectionId.Returns(_connectionId);
        
        var roomService = Substitute.For<IRoomService>();
        var lobbyChatService = Substitute.For<ILobbyChatService>();
        var matchHistoryService = Substitute.For<IMatchHistoryService>();
        var xpService = Substitute.For<IXpService>();
        var leagueService = Substitute.For<ILeagueService>();
        var hubContext = Substitute.For<IHubContext<MatchHub, IMatchClient>>();
        var runtimeSettings = new MultiplayerRuntimeSettings(new ConfigurationBuilder().Build());
        var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        _sut = new MatchHub(_matchmakingService, _gameService, _userService, roomService, lobbyChatService, matchHistoryService, xpService, leagueService, hubContext, runtimeSettings, serviceScopeFactory)
        {
            Context = _context,
            Groups = _groups
        };
    }
    
    private void SetupUser(Guid userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _context.User.Returns(principal);
        _context.UserIdentifier.Returns(userId.ToString());
    }

    [Fact]
    public async Task MatchHub_JoinMatchmaking_AddsToQueue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "TestUser";
        var level = 5;
        
        SetupUser(userId);
        _userService.GetProfileAsync(userId).Returns(new UserProfileDto 
        { 
            Id = userId, 
            Username = username, 
            Stats = new UserStatsDto { Level = level },
            AvatarUrl = null,
            CreatedAt = DateTime.UtcNow
        });
        _matchmakingService.JoinQueueAndTryMatchAsync(userId, level, username, null, Arg.Any<CancellationToken>())
            .Returns(new MatchmakingJoinResult(true, null));

        // Act
        var result = await _sut.JoinMatchmaking();

        // Assert
        result.Should().BeTrue();
        await _matchmakingService.Received(1).JoinQueueAndTryMatchAsync(userId, level, username, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MatchHub_CancelMatchmaking_RemovesFromQueue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUser(userId);

        // Act
        await _sut.CancelMatchmaking();

        // Assert
        await _matchmakingService.Received(1).CancelQueueAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MatchHub_Forfeit_UserNotInMatch_DoesNotCallForfeit()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        SetupUser(playerId);

        // Act - Forfeit when not in match should not throw and not call ForfeitAsync
        await _sut.Forfeit();

        // Assert - no exception and no call to ForfeitAsync since user is not in a match
        await _gameService.DidNotReceive().ForfeitAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
    
    private record UserDto(Guid Id, string Username, int Level, string? Avatar);
}
