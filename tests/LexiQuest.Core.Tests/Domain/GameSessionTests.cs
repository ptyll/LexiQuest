using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.Enums;
using LexiQuest.Shared.DTOs.Game;

namespace LexiQuest.Core.Tests.Domain;

public class GameSessionTests
{
    [Fact]
    public void Create_InitializesCorrectly()
    {
        var userId = Guid.NewGuid();

        var session = GameSession.Create(userId, GameMode.Training, DifficultyLevel.Beginner, 10, 3);

        session.Id.Should().NotBe(Guid.Empty);
        session.UserId.Should().Be(userId);
        session.Mode.Should().Be(GameMode.Training);
        session.Status.Should().Be(GameSessionStatus.Active);
        session.LivesRemaining.Should().Be(3);
        session.TotalXP.Should().Be(0);
        session.Rounds.Should().NotBeNull();
        session.Rounds.Should().BeEmpty();
        session.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        session.EndedAt.Should().BeNull();
    }

    [Fact]
    public void AddRound_AddsToRoundsList()
    {
        var session = GameSession.Create(Guid.NewGuid(), GameMode.Training, DifficultyLevel.Beginner, 10, 3);
        var wordId = Guid.NewGuid();

        session.AddRound(wordId, "lbkjao");

        session.Rounds.Should().HaveCount(1);
        session.Rounds[0].WordId.Should().Be(wordId);
        session.Rounds[0].ScrambledWord.Should().Be("lbkjao");
        session.Rounds[0].IsCorrect.Should().BeFalse();
    }
}
