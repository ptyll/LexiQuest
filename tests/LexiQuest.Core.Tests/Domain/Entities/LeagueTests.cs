using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.Enums;
using Xunit;

namespace LexiQuest.Core.Tests.Domain.Entities;

public class LeagueTests
{
    [Fact]
    public void League_Create_SetsCorrectDefaults()
    {
        // Arrange
        var tier = LeagueTier.Bronze;
        var weekStart = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc); // Monday
        var weekEnd = weekStart.AddDays(7);

        // Act
        var league = League.Create(tier, weekStart, weekEnd);

        // Assert
        league.Id.Should().NotBe(Guid.Empty);
        league.Tier.Should().Be(tier);
        league.WeekStart.Should().Be(weekStart);
        league.WeekEnd.Should().Be(weekEnd);
        league.IsActive.Should().BeTrue();
        league.Participants.Should().BeEmpty();
        league.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void League_AddParticipant_AddsToParticipantsList()
    {
        // Arrange
        var league = League.Create(LeagueTier.Silver, DateTime.UtcNow, DateTime.UtcNow.AddDays(7));
        var userId = Guid.NewGuid();

        // Act
        league.AddParticipant(userId);

        // Assert
        league.Participants.Should().HaveCount(1);
        league.Participants.First().UserId.Should().Be(userId);
        league.Participants.First().LeagueId.Should().Be(league.Id);
        league.Participants.First().WeeklyXP.Should().Be(0);
        league.Participants.First().Rank.Should().Be(0);
    }

    [Fact]
    public void League_AddParticipant_DuplicateUser_ThrowsException()
    {
        // Arrange
        var league = League.Create(LeagueTier.Gold, DateTime.UtcNow, DateTime.UtcNow.AddDays(7));
        var userId = Guid.NewGuid();
        league.AddParticipant(userId);

        // Act
        Action act = () => league.AddParticipant(userId);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*already in this league*");
    }

    [Fact]
    public void League_IsFull_When30Participants_ReturnsTrue()
    {
        // Arrange
        var league = League.Create(LeagueTier.Bronze, DateTime.UtcNow, DateTime.UtcNow.AddDays(7));
        
        // Add 30 participants
        for (int i = 0; i < 30; i++)
        {
            league.AddParticipant(Guid.NewGuid());
        }

        // Act & Assert
        league.IsFull.Should().BeTrue();
    }

    [Fact]
    public void League_IsFull_WhenLessThan30_ReturnsFalse()
    {
        // Arrange
        var league = League.Create(LeagueTier.Bronze, DateTime.UtcNow, DateTime.UtcNow.AddDays(7));
        league.AddParticipant(Guid.NewGuid());

        // Act & Assert
        league.IsFull.Should().BeFalse();
    }

    [Fact]
    public void League_Deactivate_SetsIsActiveToFalse()
    {
        // Arrange
        var league = League.Create(LeagueTier.Diamond, DateTime.UtcNow, DateTime.UtcNow.AddDays(7));

        // Act
        league.Deactivate();

        // Assert
        league.IsActive.Should().BeFalse();
    }
}

public class LeagueParticipantTests
{
    [Fact]
    public void LeagueParticipant_Create_SetsProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var leagueId = Guid.NewGuid();

        // Act
        var participant = LeagueParticipant.Create(userId, leagueId);

        // Assert
        participant.Id.Should().NotBe(Guid.Empty);
        participant.UserId.Should().Be(userId);
        participant.LeagueId.Should().Be(leagueId);
        participant.WeeklyXP.Should().Be(0);
        participant.Rank.Should().Be(0);
        participant.IsPromoted.Should().BeFalse();
        participant.IsDemoted.Should().BeFalse();
    }

    [Fact]
    public void LeagueParticipant_AddXP_IncreasesWeeklyXP()
    {
        // Arrange
        var participant = LeagueParticipant.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        participant.AddXP(100);

        // Assert
        participant.WeeklyXP.Should().Be(100);
    }

    [Fact]
    public void LeagueParticipant_AddXP_MultipleTimes_Accumulates()
    {
        // Arrange
        var participant = LeagueParticipant.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        participant.AddXP(50);
        participant.AddXP(75);
        participant.AddXP(25);

        // Assert
        participant.WeeklyXP.Should().Be(150);
    }

    [Fact]
    public void LeagueParticipant_SetRank_UpdatesRank()
    {
        // Arrange
        var participant = LeagueParticipant.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        participant.SetRank(5);

        // Assert
        participant.Rank.Should().Be(5);
    }

    [Fact]
    public void LeagueParticipant_MarkAsPromoted_SetsIsPromoted()
    {
        // Arrange
        var participant = LeagueParticipant.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        participant.MarkAsPromoted();

        // Assert
        participant.IsPromoted.Should().BeTrue();
        participant.IsDemoted.Should().BeFalse();
    }

    [Fact]
    public void LeagueParticipant_MarkAsDemoted_SetsIsDemoted()
    {
        // Arrange
        var participant = LeagueParticipant.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        participant.MarkAsDemoted();

        // Assert
        participant.IsDemoted.Should().BeTrue();
        participant.IsPromoted.Should().BeFalse();
    }

    [Fact]
    public void LeagueParticipant_ResetWeeklyXP_SetsXPToZero()
    {
        // Arrange
        var participant = LeagueParticipant.Create(Guid.NewGuid(), Guid.NewGuid());
        participant.AddXP(500);

        // Act
        participant.ResetWeeklyXP();

        // Assert
        participant.WeeklyXP.Should().Be(0);
    }
}
