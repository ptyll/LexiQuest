using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.Enums;
using Xunit;

namespace LexiQuest.Core.Tests.Domain.Entities;

public class BossLevelTests
{
    [Fact]
    public void GameSession_CreateBoss_Marathon_Sets20WordsAnd3Lives()
    {
        // Arrange & Act
        var session = GameSession.CreateBossSession(Guid.NewGuid(), BossType.Marathon, DifficultyLevel.Intermediate);

        // Assert
        session.BossType.Should().Be(BossType.Marathon);
        session.LivesRemaining.Should().Be(3);
        session.TotalRounds.Should().Be(20);
    }

    [Fact]
    public void GameSession_CreateBoss_Condition_SetsForbiddenLetterPattern()
    {
        // Arrange & Act
        var session = GameSession.CreateBossSession(Guid.NewGuid(), BossType.Condition, DifficultyLevel.Intermediate);

        // Assert
        session.BossType.Should().Be(BossType.Condition);
        session.ForbiddenLetters.Should().NotBeEmpty();
    }

    [Fact]
    public void GameSession_CreateBoss_Twist_SetsRevealMechanic()
    {
        // Arrange & Act
        var session = GameSession.CreateBossSession(Guid.NewGuid(), BossType.Twist, DifficultyLevel.Intermediate);

        // Assert
        session.BossType.Should().Be(BossType.Twist);
        session.RevealedLettersCount.Should().Be(2);
    }

    [Fact]
    public void MarathonBoss_WrongAnswer_DecreasesLife_NoRegen()
    {
        // Arrange
        var session = GameSession.CreateBossSession(Guid.NewGuid(), BossType.Marathon, DifficultyLevel.Intermediate);
        var initialLives = session.LivesRemaining;

        // Act
        session.LoseLife();

        // Assert
        session.LivesRemaining.Should().Be(initialLives - 1);
    }

    [Fact]
    public void MarathonBoss_0Lives_GameOver()
    {
        // Arrange
        var session = GameSession.CreateBossSession(Guid.NewGuid(), BossType.Marathon, DifficultyLevel.Intermediate);

        // Act
        session.LoseLife();
        session.LoseLife();
        session.LoseLife();

        // Assert
        session.LivesRemaining.Should().Be(0);
        session.IsGameOver.Should().BeTrue();
    }
}

public class BossRoundTests
{
    [Fact]
    public void GameRound_SetForbiddenLetters_SetsLetters()
    {
        // Arrange
        var round = GameRound.Create(Guid.NewGuid(), 1, Guid.NewGuid(), "TEST", "TEST", 30);

        // Act
        round.SetForbiddenLetters("AEI");

        // Assert
        round.ForbiddenLetters.Should().Be("AEI");
    }

    [Fact]
    public void GameRound_RevealLetter_IncreasesRevealedCount()
    {
        // Arrange
        var round = GameRound.Create(Guid.NewGuid(), 1, Guid.NewGuid(), "TEST", "TEST", 30);
        round.SetRevealedPositions(new[] { 0, 2 });

        // Act
        round.RevealLetter();

        // Assert
        round.RevealedLettersCount.Should().Be(3);
    }

    [Fact]
    public void GameRound_AnswerContainsForbiddenLetter_PenaltyApplied()
    {
        // Arrange
        var round = GameRound.Create(Guid.NewGuid(), 1, Guid.NewGuid(), "TEST", "TEST", 30);
        round.SetForbiddenLetters("X");

        // Act
        var hasForbidden = round.ContainsForbiddenLetter("TESTX");

        // Assert
        hasForbidden.Should().BeTrue();
    }
}
