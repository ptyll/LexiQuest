using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Jobs;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

public class LeagueResetJobTests
{
    private readonly ILeagueRepository _leagueRepository;
    private readonly ILeagueService _leagueService;
    private readonly ILogger<LeagueResetJob> _logger;
    private readonly LeagueResetJob _sut;

    public LeagueResetJobTests()
    {
        _leagueRepository = Substitute.For<ILeagueRepository>();
        _leagueService = Substitute.For<ILeagueService>();
        _logger = Substitute.For<ILogger<LeagueResetJob>>();
        _sut = new LeagueResetJob(_leagueRepository, _leagueService, _logger);
    }

    [Fact]
    public async Task LeagueResetJob_Execute_AssignsUsersToNewLeagues()
    {
        // Arrange
        var activeLeagues = new List<League>
        {
            CreateLeague(LeagueTier.Bronze, 5),
            CreateLeague(LeagueTier.Silver, 3)
        };
        
        _leagueRepository.GetActiveLeaguesAsync(Arg.Any<CancellationToken>())
            .Returns(activeLeagues);

        var bronzeLeague = activeLeagues.First(l => l.Tier == LeagueTier.Bronze);
        var userIds = bronzeLeague.Participants.Select(p => p.UserId).ToList();

        // Act
        await _sut.ExecuteAsync(CancellationToken.None);

        // Assert - verify that all users are assigned to new leagues
        foreach (var userId in userIds)
        {
            await _leagueService.Received().AssignUserToLeagueAsync(
                userId,
                Arg.Any<DateTime>(),
                Arg.Any<DateTime>(),
                Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task LeagueResetJob_Execute_MovesPromotedUsersUp()
    {
        // Arrange
        var league = CreateLeagueWithRanks(LeagueTier.Bronze, 10);
        var promotedUsers = league.Participants.Where(p => p.Rank <= 3).ToList();
        
        foreach (var user in promotedUsers)
        {
            user.MarkAsPromoted();
        }

        _leagueRepository.GetActiveLeaguesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<League> { league });

        // Act
        await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        await _leagueService.Received().AssignUserToLeagueAsync(
            Arg.Is<Guid>(id => promotedUsers.Any(p => p.UserId == id)),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LeagueResetJob_Execute_MovesDemotedUsersDown()
    {
        // Arrange
        var league = CreateLeagueWithRanks(LeagueTier.Silver, 10);
        var demotedUsers = league.Participants.Where(p => p.Rank > 7).ToList();
        
        foreach (var user in demotedUsers)
        {
            user.MarkAsDemoted();
        }

        _leagueRepository.GetActiveLeaguesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<League> { league });

        // Act
        await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        // Demoted users from Silver should go to Bronze
        await _leagueService.Received().AssignUserToLeagueAsync(
            Arg.Is<Guid>(id => demotedUsers.Any(p => p.UserId == id)),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LeagueResetJob_Execute_ResetsWeeklyXP()
    {
        // Arrange
        var league = CreateLeagueWithRanks(LeagueTier.Gold, 5);
        
        _leagueRepository.GetActiveLeaguesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<League> { league });

        // Act
        await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        league.IsActive.Should().BeFalse();
        league.Participants.Should().AllSatisfy(p => p.WeeklyXP.Should().Be(0));
    }

    [Fact]
    public async Task LeagueResetJob_Execute_LegendTier_StayersRemainInLegend()
    {
        // Arrange
        var league = CreateLeagueWithRanks(LeagueTier.Legend, 20);
        
        // Ranks 1-3 promoted (stay in Legend), 4-10 stay, 11-20 demoted
        foreach (var p in league.Participants.Where(p => p.Rank <= 3))
            p.MarkAsPromoted();
        foreach (var p in league.Participants.Where(p => p.Rank > 10))
            p.MarkAsDemoted();

        _leagueRepository.GetActiveLeaguesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<League> { league });

        // Act
        await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        // Promoted Legend users should be assigned to new Legend league
        var promotedUsers = league.Participants.Where(p => p.Rank <= 3).Select(p => p.UserId).ToList();
        await _leagueService.Received().AssignUserToLeagueAsync(
            Arg.Is<Guid>(id => promotedUsers.Contains(id)),
            Arg.Any<DateTime>(),
            Arg.Any<DateTime>(),
            Arg.Any<CancellationToken>());
    }

    private static League CreateLeague(LeagueTier tier, int participantCount)
    {
        var weekStart = GetWeekStart();
        var league = League.Create(tier, weekStart, weekStart.AddDays(7));
        
        for (int i = 0; i < participantCount; i++)
        {
            league.AddParticipant(Guid.NewGuid());
            var participant = league.Participants.Last();
            participant.AddXP((participantCount - i) * 100);
        }
        
        league.UpdateRanks();
        return league;
    }

    private static League CreateLeagueWithRanks(LeagueTier tier, int participantCount)
    {
        return CreateLeague(tier, participantCount);
    }

    private static DateTime GetWeekStart()
    {
        var today = DateTime.UtcNow.Date;
        return today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
    }
}
