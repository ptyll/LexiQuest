using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Services;
using LexiQuest.Shared.DTOs.Leagues;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

public class LeagueServiceTests
{
    private readonly ILeagueRepository _leagueRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<LeagueService> _localizer;
    private readonly LeagueService _sut;

    public LeagueServiceTests()
    {
        _leagueRepository = Substitute.For<ILeagueRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _localizer = Substitute.For<IStringLocalizer<LeagueService>>();
        
        _localizer[Arg.Any<string>()].Returns(c => new LocalizedString(c.Arg<string>(), c.Arg<string>()));
        
        _sut = new LeagueService(_leagueRepository, _unitOfWork, _localizer);
    }

    [Fact]
    public async Task LeagueService_AssignNewUser_PlacesInBronze()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var weekStart = GetWeekStart();
        var weekEnd = weekStart.AddDays(7);
        
        _leagueRepository.GetActiveLeagueForTierAsync(LeagueTier.Bronze, Arg.Any<CancellationToken>())
            .Returns((League?)null);

        League? createdLeague = null;
        _leagueRepository.When(x => x.AddAsync(Arg.Any<League>(), Arg.Any<CancellationToken>()))
            .Do(ci => createdLeague = ci.Arg<League>());

        // Act
        await _sut.AssignUserToLeagueAsync(userId, weekStart, weekEnd);

        // Assert
        createdLeague.Should().NotBeNull();
        createdLeague!.Tier.Should().Be(LeagueTier.Bronze);
        createdLeague.Participants.Should().ContainSingle(p => p.UserId == userId);
    }

    [Fact]
    public async Task LeagueService_AssignUser_Bronze_Max30Participants()
    {
        // Arrange
        var weekStart = GetWeekStart();
        var weekEnd = weekStart.AddDays(7);
        var existingLeague = League.Create(LeagueTier.Bronze, weekStart, weekEnd);
        
        // Fill league with 30 participants
        for (int i = 0; i < 30; i++)
        {
            existingLeague.AddParticipant(Guid.NewGuid());
        }

        _leagueRepository.GetActiveLeagueForTierAsync(LeagueTier.Bronze, Arg.Any<CancellationToken>())
            .Returns(existingLeague);

        League? newLeague = null;
        _leagueRepository.When(x => x.AddAsync(Arg.Any<League>(), Arg.Any<CancellationToken>()))
            .Do(ci => newLeague = ci.Arg<League>());

        // Act
        await _sut.AssignUserToLeagueAsync(Guid.NewGuid(), weekStart, weekEnd);

        // Assert
        newLeague.Should().NotBeNull();
        newLeague!.Tier.Should().Be(LeagueTier.Bronze);
    }

    [Fact]
    public async Task LeagueService_GetCurrentLeague_ReturnsActiveLeague()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var league = CreateLeagueWithParticipant(LeagueTier.Gold, userId);
        
        _leagueRepository.GetActiveLeagueForUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(league);

        // Act
        var result = await _sut.GetCurrentLeagueAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Tier.Should().Be(LeagueTier.Gold);
    }

    [Fact]
    public async Task LeagueService_GetCurrentLeague_NoActiveLeague_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        _leagueRepository.GetActiveLeagueForUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns((League?)null);

        // Act
        var result = await _sut.GetCurrentLeagueAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LeagueService_AddXP_UpdatesParticipantXP()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var league = CreateLeagueWithParticipant(LeagueTier.Silver, userId);
        
        _leagueRepository.GetActiveLeagueForUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(league);

        // Act
        await _sut.AddXPAsync(userId, 150);

        // Assert
        var participant = league.Participants.First(p => p.UserId == userId);
        participant.WeeklyXP.Should().Be(150);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LeagueService_GetLeaderboard_ReturnsSortedByXP()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var league = League.Create(LeagueTier.Diamond, GetWeekStart(), GetWeekStart().AddDays(7));
        
        // Add participants with different XP
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var user3 = Guid.NewGuid();
        
        league.AddParticipant(user1);
        league.AddParticipant(user2);
        league.AddParticipant(user3);
        
        league.Participants.First(p => p.UserId == user1).AddXP(300);
        league.Participants.First(p => p.UserId == user2).AddXP(500);
        league.Participants.First(p => p.UserId == user3).AddXP(100);

        _leagueRepository.GetActiveLeagueForUserAsync(userId, Arg.Any<CancellationToken>())
            .Returns(league);

        // Act
        var result = await _sut.GetLeaderboardAsync(userId);

        // Assert
        result.Should().HaveCount(3);
        result[0].UserId.Should().Be(user2); // 500 XP
        result[1].UserId.Should().Be(user1); // 300 XP
        result[2].UserId.Should().Be(user3); // 100 XP
    }

    [Fact]
    public async Task LeagueService_CalculatePromotions_Top5Promoted()
    {
        // Arrange
        var league = League.Create(LeagueTier.Bronze, GetWeekStart(), GetWeekStart().AddDays(7));
        
        // Add 10 participants with different XP
        for (int i = 0; i < 10; i++)
        {
            var userId = Guid.NewGuid();
            league.AddParticipant(userId);
            league.Participants.First(p => p.UserId == userId).AddXP((10 - i) * 10); // 100, 90, 80...
        }
        
        league.UpdateRanks();

        // Act
        await _sut.CalculatePromotionsAndDemotionsAsync(league);

        // Assert
        var top5 = league.Participants.Where(p => p.Rank <= 5).ToList();
        top5.Should().AllSatisfy(p => p.IsPromoted.Should().BeTrue());
        
        var bottom5 = league.Participants.Where(p => p.Rank > 5).ToList();
        bottom5.Should().AllSatisfy(p => p.IsPromoted.Should().BeFalse());
    }

    [Fact]
    public async Task LeagueService_CalculateDemotions_Bottom5Demoted()
    {
        // Arrange
        var league = League.Create(LeagueTier.Gold, GetWeekStart(), GetWeekStart().AddDays(7));
        
        // Add 10 participants with different XP
        for (int i = 0; i < 10; i++)
        {
            var userId = Guid.NewGuid();
            league.AddParticipant(userId);
            league.Participants.First(p => p.UserId == userId).AddXP((10 - i) * 10);
        }
        
        league.UpdateRanks();

        // Act
        await _sut.CalculatePromotionsAndDemotionsAsync(league);

        // Assert
        var bottom5 = league.Participants.Where(p => p.Rank > 5).ToList();
        bottom5.Should().AllSatisfy(p => p.IsDemoted.Should().BeTrue());
    }

    [Fact]
    public async Task LeagueService_LegendTier_Top3Promoted()
    {
        // Arrange
        var league = League.Create(LeagueTier.Legend, GetWeekStart(), GetWeekStart().AddDays(7));
        
        // Add 10 participants
        for (int i = 0; i < 10; i++)
        {
            var userId = Guid.NewGuid();
            league.AddParticipant(userId);
            league.Participants.First(p => p.UserId == userId).AddXP((10 - i) * 10);
        }
        
        league.UpdateRanks();

        // Act
        await _sut.CalculatePromotionsAndDemotionsAsync(league);

        // Assert
        var top3 = league.Participants.Where(p => p.Rank <= 3).ToList();
        top3.Should().AllSatisfy(p => p.IsPromoted.Should().BeTrue());
        
        var ranks4To10 = league.Participants.Where(p => p.Rank > 3).ToList();
        ranks4To10.Should().AllSatisfy(p => p.IsPromoted.Should().BeFalse());
    }

    [Fact]
    public async Task LeagueService_LegendTier_Bottom10Demoted()
    {
        // Arrange
        var league = League.Create(LeagueTier.Legend, GetWeekStart(), GetWeekStart().AddDays(7));
        
        // Add 20 participants
        for (int i = 0; i < 20; i++)
        {
            var userId = Guid.NewGuid();
            league.AddParticipant(userId);
            league.Participants.First(p => p.UserId == userId).AddXP((20 - i) * 10);
        }
        
        league.UpdateRanks();

        // Act
        await _sut.CalculatePromotionsAndDemotionsAsync(league);

        // Assert
        var bottom10 = league.Participants.Where(p => p.Rank > 10).ToList();
        bottom10.Should().AllSatisfy(p => p.IsDemoted.Should().BeTrue());
    }

    [Theory]
    [InlineData(LeagueTier.Bronze, 50)]
    [InlineData(LeagueTier.Silver, 100)]
    [InlineData(LeagueTier.Gold, 200)]
    [InlineData(LeagueTier.Diamond, 500)]
    [InlineData(LeagueTier.Legend, 1000)]
    public void LeagueService_GetRewards_ReturnsCorrectXP(LeagueTier tier, int expectedReward)
    {
        // Act
        var result = _sut.GetRewards(tier);

        // Assert
        result.Should().Be(expectedReward);
    }

    private static DateTime GetWeekStart()
    {
        var today = DateTime.UtcNow.Date;
        return today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
    }

    private static League CreateLeagueWithParticipant(LeagueTier tier, Guid userId)
    {
        var league = League.Create(tier, GetWeekStart(), GetWeekStart().AddDays(7));
        league.AddParticipant(userId);
        return league;
    }
}
