using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Services;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

public class LeagueServiceEdgeCaseTests
{
    private readonly ILeagueRepository _leagueRepository = Substitute.For<ILeagueRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IStringLocalizer<LeagueService> _localizer = Substitute.For<IStringLocalizer<LeagueService>>();
    private readonly LeagueService _sut;

    public LeagueServiceEdgeCaseTests()
    {
        _localizer[Arg.Any<string>()].Returns(c => new LocalizedString(c.Arg<string>(), c.Arg<string>()));
        _sut = new LeagueService(_leagueRepository, _unitOfWork, _localizer);
    }

    private static DateTime GetWeekStart()
    {
        var today = DateTime.UtcNow.Date;
        return today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
    }

    private League CreateLeagueWithParticipants(LeagueTier tier, int count, Guid? targetUserId = null)
    {
        var league = League.Create(tier, GetWeekStart(), GetWeekStart().AddDays(7));
        for (int i = 0; i < count; i++)
        {
            var uid = (i == 0 && targetUserId.HasValue) ? targetUserId.Value : Guid.NewGuid();
            league.AddParticipant(uid);
        }
        return league;
    }

    // --- Adding XP updates rank correctly ---

    [Fact]
    public async Task AddXP_SingleParticipant_RankIsOne()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var league = CreateLeagueWithParticipants(LeagueTier.Silver, 1, userId);
        _leagueRepository.GetActiveLeagueForUserAsync(userId, Arg.Any<CancellationToken>()).Returns(league);

        // Act
        await _sut.AddXPAsync(userId, 100);
        league.UpdateRanks();

        // Assert
        var participant = league.Participants.First(p => p.UserId == userId);
        participant.WeeklyXP.Should().Be(100);
        participant.Rank.Should().Be(1);
    }

    [Fact]
    public async Task AddXP_MultipleAdds_AccumulatesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var league = CreateLeagueWithParticipants(LeagueTier.Bronze, 1, userId);
        _leagueRepository.GetActiveLeagueForUserAsync(userId, Arg.Any<CancellationToken>()).Returns(league);

        // Act
        await _sut.AddXPAsync(userId, 50);
        await _sut.AddXPAsync(userId, 75);

        // Assert
        var participant = league.Participants.First(p => p.UserId == userId);
        participant.WeeklyXP.Should().Be(125);
    }

    [Fact]
    public async Task AddXP_UserNotInLeague_ThrowsInvalidOperation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _leagueRepository.GetActiveLeagueForUserAsync(userId, Arg.Any<CancellationToken>()).Returns((League?)null);

        // Act
        var act = () => _sut.AddXPAsync(userId, 100);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AddXP_NegativeXP_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var league = CreateLeagueWithParticipants(LeagueTier.Bronze, 1, userId);
        _leagueRepository.GetActiveLeagueForUserAsync(userId, Arg.Any<CancellationToken>()).Returns(league);

        // Act - LeagueParticipant.AddXP throws for negative XP
        var act = () => _sut.AddXPAsync(userId, -10);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    // --- Promotion/demotion boundary cases ---

    [Fact]
    public async Task CalculatePromotions_ExactlyAtCutoff_RankFiveIsPromoted()
    {
        // Arrange - 10 participants, top 5 promoted for non-Legend tiers
        var league = League.Create(LeagueTier.Bronze, GetWeekStart(), GetWeekStart().AddDays(7));
        var userIds = new List<Guid>();
        for (int i = 0; i < 10; i++)
        {
            var uid = Guid.NewGuid();
            userIds.Add(uid);
            league.AddParticipant(uid);
            league.Participants.First(p => p.UserId == uid).AddXP((10 - i) * 100);
        }
        league.UpdateRanks();

        // Act
        await _sut.CalculatePromotionsAndDemotionsAsync(league);

        // Assert - rank 5 (the cutoff) should be promoted
        var rank5 = league.Participants.First(p => p.Rank == 5);
        rank5.IsPromoted.Should().BeTrue();

        var rank6 = league.Participants.First(p => p.Rank == 6);
        rank6.IsPromoted.Should().BeFalse();
    }

    [Fact]
    public async Task CalculatePromotions_ExactlyAtDemotionCutoff_IsDemoted()
    {
        // Arrange - 10 participants, bottom 5 demoted for non-Legend tiers
        var league = League.Create(LeagueTier.Silver, GetWeekStart(), GetWeekStart().AddDays(7));
        for (int i = 0; i < 10; i++)
        {
            var uid = Guid.NewGuid();
            league.AddParticipant(uid);
            league.Participants.First(p => p.UserId == uid).AddXP((10 - i) * 100);
        }
        league.UpdateRanks();

        // Act
        await _sut.CalculatePromotionsAndDemotionsAsync(league);

        // Assert - rank 6 (first in bottom 5) should be demoted
        var rank6 = league.Participants.First(p => p.Rank == 6);
        rank6.IsDemoted.Should().BeTrue();

        var rank5 = league.Participants.First(p => p.Rank == 5);
        rank5.IsDemoted.Should().BeFalse();
    }

    [Fact]
    public async Task CalculatePromotions_AllSameXP_StillAssignsPromotionsAndDemotions()
    {
        // Arrange - all have same XP, rank by join time
        var league = League.Create(LeagueTier.Gold, GetWeekStart(), GetWeekStart().AddDays(7));
        for (int i = 0; i < 10; i++)
        {
            league.AddParticipant(Guid.NewGuid());
            // All have 0 XP
        }
        league.UpdateRanks();

        // Act
        await _sut.CalculatePromotionsAndDemotionsAsync(league);

        // Assert
        var promoted = league.Participants.Count(p => p.IsPromoted);
        var demoted = league.Participants.Count(p => p.IsDemoted);
        promoted.Should().Be(5);
        demoted.Should().Be(5);
    }

    [Fact]
    public async Task CalculatePromotions_LegendTier_OnlyTop3Promoted()
    {
        // Arrange
        var league = League.Create(LeagueTier.Legend, GetWeekStart(), GetWeekStart().AddDays(7));
        for (int i = 0; i < 10; i++)
        {
            var uid = Guid.NewGuid();
            league.AddParticipant(uid);
            league.Participants.First(p => p.UserId == uid).AddXP((10 - i) * 100);
        }
        league.UpdateRanks();

        // Act
        await _sut.CalculatePromotionsAndDemotionsAsync(league);

        // Assert
        var promoted = league.Participants.Count(p => p.IsPromoted);
        promoted.Should().Be(3);
    }

    [Fact]
    public async Task CalculatePromotions_LegendTier_DemotionIsHalfOfParticipants()
    {
        // Arrange - Legend demotion = min(10, count/2) = min(10, 5) = 5
        var league = League.Create(LeagueTier.Legend, GetWeekStart(), GetWeekStart().AddDays(7));
        for (int i = 0; i < 10; i++)
        {
            var uid = Guid.NewGuid();
            league.AddParticipant(uid);
            league.Participants.First(p => p.UserId == uid).AddXP((10 - i) * 100);
        }
        league.UpdateRanks();

        // Act
        await _sut.CalculatePromotionsAndDemotionsAsync(league);

        // Assert
        var demoted = league.Participants.Count(p => p.IsDemoted);
        demoted.Should().Be(5); // min(10, 10/2) = 5
    }

    // --- Empty league handling ---

    [Fact]
    public async Task GetLeaderboard_NoLeague_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _leagueRepository.GetActiveLeagueForUserAsync(userId, Arg.Any<CancellationToken>()).Returns((League?)null);

        // Act
        var result = await _sut.GetLeaderboardAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCurrentLeague_NoLeague_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _leagueRepository.GetActiveLeagueForUserAsync(userId, Arg.Any<CancellationToken>()).Returns((League?)null);

        // Act
        var result = await _sut.GetCurrentLeagueAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentLeague_UserNotParticipant_ReturnsNull()
    {
        // Arrange - league exists but user is not in it
        var userId = Guid.NewGuid();
        var league = League.Create(LeagueTier.Bronze, GetWeekStart(), GetWeekStart().AddDays(7));
        league.AddParticipant(Guid.NewGuid()); // someone else
        _leagueRepository.GetActiveLeagueForUserAsync(userId, Arg.Any<CancellationToken>()).Returns(league);

        // Act
        var result = await _sut.GetCurrentLeagueAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AssignUser_FullLeague_CreatesNewLeague()
    {
        // Arrange - existing league is full (30 participants)
        var weekStart = GetWeekStart();
        var weekEnd = weekStart.AddDays(7);
        var fullLeague = League.Create(LeagueTier.Bronze, weekStart, weekEnd);
        for (int i = 0; i < 30; i++)
            fullLeague.AddParticipant(Guid.NewGuid());

        _leagueRepository.GetActiveLeagueForTierAsync(LeagueTier.Bronze, Arg.Any<CancellationToken>()).Returns(fullLeague);

        League? createdLeague = null;
        _leagueRepository.When(x => x.AddAsync(Arg.Any<League>(), Arg.Any<CancellationToken>()))
            .Do(ci => createdLeague = ci.Arg<League>());

        // Act
        await _sut.AssignUserToLeagueAsync(Guid.NewGuid(), weekStart, weekEnd);

        // Assert
        createdLeague.Should().NotBeNull();
        createdLeague!.Tier.Should().Be(LeagueTier.Bronze);
        await _leagueRepository.Received(1).AddAsync(Arg.Any<League>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCurrentLeague_ReturnsCorrectThresholds()
    {
        // Arrange - 10 participants in Gold league
        var userId = Guid.NewGuid();
        var league = League.Create(LeagueTier.Gold, GetWeekStart(), GetWeekStart().AddDays(7));
        league.AddParticipant(userId);
        for (int i = 0; i < 9; i++)
        {
            var uid = Guid.NewGuid();
            league.AddParticipant(uid);
            league.Participants.First(p => p.UserId == uid).AddXP((9 - i) * 100);
        }
        _leagueRepository.GetActiveLeagueForUserAsync(userId, Arg.Any<CancellationToken>()).Returns(league);

        // Act
        var result = await _sut.GetCurrentLeagueAsync(userId);

        // Assert
        result.Should().NotBeNull();
        // Non-Legend tier: promotionCount = 5, demotionCount = 5
        // PromotionThreshold = 5, DemotionThreshold = 10 - 5 + 1 = 6
        result!.PromotionThreshold.Should().Be(5);
        result.DemotionThreshold.Should().Be(6);
    }

    [Fact]
    public async Task GetLeaderboard_SetsIsCurrentUserFlag()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var league = League.Create(LeagueTier.Bronze, GetWeekStart(), GetWeekStart().AddDays(7));
        league.AddParticipant(userId);
        league.AddParticipant(Guid.NewGuid());
        _leagueRepository.GetActiveLeagueForUserAsync(userId, Arg.Any<CancellationToken>()).Returns(league);

        // Act
        var result = await _sut.GetLeaderboardAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainSingle(p => p.IsCurrentUser);
        result.First(p => p.IsCurrentUser).UserId.Should().Be(userId);
    }
}
