using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

/// <summary>
/// Tests for GameSession and GameRound domain logic edge cases.
/// The GameSessionService lives in Infrastructure and depends on DbContext,
/// so we test the core domain behavior directly.
/// </summary>
public class GameSessionEdgeCaseTests
{
    // --- Answer submission edge cases ---

    [Fact]
    public void GameSession_RecordCorrectAnswer_IncreasesComboAndCorrectCount()
    {
        // Arrange
        var session = GameSession.Create(
            Guid.NewGuid(), GameMode.Training, DifficultyLevel.Beginner, 10, 5);

        // Act
        session.RecordCorrectAnswer();
        session.RecordCorrectAnswer();

        // Assert
        session.ComboCount.Should().Be(2);
        session.CorrectAnswers.Should().Be(2);
    }

    [Fact]
    public void GameSession_RecordWrongAnswer_ResetsComboAndDecreasesLives()
    {
        // Arrange
        var session = GameSession.Create(
            Guid.NewGuid(), GameMode.Training, DifficultyLevel.Beginner, 10, 5);
        session.RecordCorrectAnswer(); // Build combo to 1

        // Act
        session.RecordWrongAnswer();

        // Assert
        session.ComboCount.Should().Be(0);
        session.LivesRemaining.Should().Be(4);
    }

    [Fact]
    public void GameSession_RecordWrongAnswer_LastLife_FailsSession()
    {
        // Arrange
        var session = GameSession.Create(
            Guid.NewGuid(), GameMode.Training, DifficultyLevel.Beginner, 10, 1);

        // Act
        session.RecordWrongAnswer();

        // Assert
        session.LivesRemaining.Should().Be(0);
        session.IsGameOver.Should().BeTrue();
        session.Status.Should().Be(GameSessionStatus.Failed);
    }

    [Fact]
    public void GameSession_MultipleWrongAnswers_AllLivesLost_GameOver()
    {
        // Arrange
        var session = GameSession.Create(
            Guid.NewGuid(), GameMode.Training, DifficultyLevel.Beginner, 10, 3);

        // Act
        session.RecordWrongAnswer(); // 2 lives
        session.RecordWrongAnswer(); // 1 life
        session.RecordWrongAnswer(); // 0 lives - game over

        // Assert
        session.LivesRemaining.Should().Be(0);
        session.IsGameOver.Should().BeTrue();
    }

    [Fact]
    public void GameSession_CorrectAfterWrong_ComboStartsFromOne()
    {
        // Arrange
        var session = GameSession.Create(
            Guid.NewGuid(), GameMode.Training, DifficultyLevel.Beginner, 10, 5);

        // Act
        session.RecordCorrectAnswer(); // Combo 1
        session.RecordCorrectAnswer(); // Combo 2
        session.RecordWrongAnswer();   // Combo 0
        session.RecordCorrectAnswer(); // Combo 1

        // Assert
        session.ComboCount.Should().Be(1);
        session.CorrectAnswers.Should().Be(3);
    }

    [Fact]
    public void GameSession_AddXP_Accumulates()
    {
        // Arrange
        var session = GameSession.Create(
            Guid.NewGuid(), GameMode.Training, DifficultyLevel.Beginner, 10, 5);

        // Act
        session.AddXP(50);
        session.AddXP(30);

        // Assert
        session.TotalXP.Should().Be(80);
    }

    [Fact]
    public void GameSession_Complete_SetsStatusAndEndTime()
    {
        // Arrange
        var session = GameSession.Create(
            Guid.NewGuid(), GameMode.Training, DifficultyLevel.Beginner, 10, 5);

        // Act
        session.Complete();

        // Assert
        session.Status.Should().Be(GameSessionStatus.Completed);
        session.EndedAt.Should().NotBeNull();
    }

    [Fact]
    public void GameSession_Forfeit_SetsStatusAndEndTime()
    {
        // Arrange
        var session = GameSession.Create(
            Guid.NewGuid(), GameMode.Training, DifficultyLevel.Beginner, 10, 5);

        // Act
        session.Forfeit();

        // Assert
        session.Status.Should().Be(GameSessionStatus.Forfeited);
        session.EndedAt.Should().NotBeNull();
    }

    [Fact]
    public void GameSession_AdvanceToNextRound_IncrementsCurrentRound()
    {
        // Arrange
        var session = GameSession.Create(
            Guid.NewGuid(), GameMode.Training, DifficultyLevel.Beginner, 10, 5);
        session.CurrentRound.Should().Be(1);

        // Act
        session.AdvanceToNextRound();

        // Assert
        session.CurrentRound.Should().Be(2);
    }

    [Fact]
    public void GameSession_GetWordIdForRound_ValidRound_ReturnsWordId()
    {
        // Arrange
        var session = GameSession.Create(
            Guid.NewGuid(), GameMode.Training, DifficultyLevel.Beginner, 3, 5);
        var wordIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        session.SetWordIds(wordIds);

        // Act
        var wordId = session.GetWordIdForRound(2);

        // Assert
        wordId.Should().Be(wordIds[1]);
    }

    [Fact]
    public void GameSession_GetWordIdForRound_OutOfRange_Throws()
    {
        // Arrange
        var session = GameSession.Create(
            Guid.NewGuid(), GameMode.Training, DifficultyLevel.Beginner, 3, 5);
        session.SetWordIds(new List<Guid> { Guid.NewGuid() });

        // Act
        var act = () => session.GetWordIdForRound(5);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GameSession_GetWordIdForRound_ZeroRound_Throws()
    {
        // Arrange
        var session = GameSession.Create(
            Guid.NewGuid(), GameMode.Training, DifficultyLevel.Beginner, 3, 5);
        session.SetWordIds(new List<Guid> { Guid.NewGuid() });

        // Act
        var act = () => session.GetWordIdForRound(0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // --- GameRound edge cases ---

    [Fact]
    public void GameRound_RecordAttempt_SetsAllFields()
    {
        // Arrange
        var round = GameRound.Create(Guid.NewGuid(), 1, Guid.NewGuid(), "TSET", "TEST", 30);

        // Act
        round.RecordAttempt("TEST", true, 5000);

        // Assert
        round.UserAnswer.Should().Be("TEST");
        round.IsCorrect.Should().BeTrue();
        round.TimeSpentMs.Should().Be(5000);
        round.IsCompleted.Should().BeTrue();
        round.AnsweredAt.Should().NotBeNull();
    }

    [Fact]
    public void GameRound_RecordAttempt_WrongAnswer_SetsIsCorrectFalse()
    {
        // Arrange
        var round = GameRound.Create(Guid.NewGuid(), 1, Guid.NewGuid(), "TSET", "TEST", 30);

        // Act
        round.RecordAttempt("WRONG", false, 3000);

        // Assert
        round.IsCorrect.Should().BeFalse();
        round.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void GameRound_ContainsForbiddenLetter_WithForbidden_ReturnsTrue()
    {
        // Arrange
        var round = GameRound.Create(Guid.NewGuid(), 1, Guid.NewGuid(), "TSET", "TEST", 30);
        round.SetForbiddenLetters("AE");

        // Act
        var result = round.ContainsForbiddenLetter("TEST"); // Contains 'E'

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GameRound_ContainsForbiddenLetter_WithoutForbidden_ReturnsFalse()
    {
        // Arrange
        var round = GameRound.Create(Guid.NewGuid(), 1, Guid.NewGuid(), "TSET", "TEST", 30);
        round.SetForbiddenLetters("XY");

        // Act
        var result = round.ContainsForbiddenLetter("TEST"); // No X or Y

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GameRound_ContainsForbiddenLetter_NullForbidden_ReturnsFalse()
    {
        // Arrange
        var round = GameRound.Create(Guid.NewGuid(), 1, Guid.NewGuid(), "TSET", "TEST", 30);

        // Act - no forbidden letters set
        var result = round.ContainsForbiddenLetter("TEST");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GameRound_ContainsForbiddenLetter_EmptyAnswer_ReturnsFalse()
    {
        // Arrange
        var round = GameRound.Create(Guid.NewGuid(), 1, Guid.NewGuid(), "TSET", "TEST", 30);
        round.SetForbiddenLetters("AE");

        // Act
        var result = round.ContainsForbiddenLetter("");

        // Assert
        result.Should().BeFalse();
    }

    // --- Boss session creation ---

    [Fact]
    public void GameSession_CreateBossSession_Marathon_20Rounds3Lives()
    {
        // Act
        var session = GameSession.CreateBossSession(Guid.NewGuid(), BossType.Marathon, DifficultyLevel.Intermediate);

        // Assert
        session.TotalRounds.Should().Be(20);
        session.LivesRemaining.Should().Be(3);
        session.BossType.Should().Be(BossType.Marathon);
        session.Mode.Should().Be(GameMode.Boss);
    }

    [Fact]
    public void GameSession_CreateBossSession_Condition_15Rounds_HasForbiddenLetters()
    {
        // Act
        var session = GameSession.CreateBossSession(Guid.NewGuid(), BossType.Condition, DifficultyLevel.Advanced);

        // Assert
        session.TotalRounds.Should().Be(15);
        session.LivesRemaining.Should().Be(3);
        session.ForbiddenLetters.Should().NotBeNullOrEmpty();
        session.ForbiddenLetters!.Length.Should().Be(2); // Two forbidden vowels
    }

    [Fact]
    public void GameSession_CreateBossSession_Twist_12Rounds_HasRevealedLetters()
    {
        // Act
        var session = GameSession.CreateBossSession(Guid.NewGuid(), BossType.Twist, DifficultyLevel.Expert);

        // Assert
        session.TotalRounds.Should().Be(12);
        session.RevealedLettersCount.Should().Be(2);
    }

    [Fact]
    public void GameSession_IsGameOver_InProgressWithLives_ReturnsFalse()
    {
        // Arrange
        var session = GameSession.Create(
            Guid.NewGuid(), GameMode.Training, DifficultyLevel.Beginner, 10, 5);

        // Assert
        session.IsGameOver.Should().BeFalse();
    }

    [Fact]
    public void GameSession_IsGameOver_FailedStatus_ReturnsTrue()
    {
        // Arrange
        var session = GameSession.Create(
            Guid.NewGuid(), GameMode.Training, DifficultyLevel.Beginner, 10, 1);
        session.RecordWrongAnswer(); // Loses last life, sets Failed status

        // Assert
        session.IsGameOver.Should().BeTrue();
    }
}
