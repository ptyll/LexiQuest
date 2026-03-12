using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Tests.Domain.Entities;

public class GameSessionTests
{
    private static GameSession CreateSession(
        GameMode mode = GameMode.Training,
        DifficultyLevel difficulty = DifficultyLevel.Beginner,
        int totalRounds = 10,
        int lives = 3) =>
        GameSession.Create(Guid.NewGuid(), mode, difficulty, totalRounds, lives);

    // --- Create ---

    [Fact]
    public void Create_SetsDefaultProperties()
    {
        var userId = Guid.NewGuid();

        var session = GameSession.Create(userId, GameMode.Training, DifficultyLevel.Beginner, 10, 3);

        session.Id.Should().NotBeEmpty();
        session.UserId.Should().Be(userId);
        session.Mode.Should().Be(GameMode.Training);
        session.Difficulty.Should().Be(DifficultyLevel.Beginner);
        session.TotalRounds.Should().Be(10);
        session.LivesRemaining.Should().Be(3);
        session.CurrentRound.Should().Be(1);
        session.TotalXP.Should().Be(0);
        session.ComboCount.Should().Be(0);
        session.CorrectAnswers.Should().Be(0);
        session.Status.Should().Be(GameSessionStatus.InProgress);
        session.EndedAt.Should().BeNull();
        session.Rounds.Should().BeEmpty();
        session.IsGameOver.Should().BeFalse();
    }

    [Fact]
    public void Create_WithPathIdAndLevel_StoresThem()
    {
        var userId = Guid.NewGuid();
        var pathId = Guid.NewGuid();

        var session = GameSession.Create(userId, GameMode.Path, DifficultyLevel.Intermediate, 5, 3, pathId, 2);

        session.PathId.Should().Be(pathId);
        session.LevelNumber.Should().Be(2);
    }

    // --- CreateBossSession Marathon ---

    [Fact]
    public void CreateBossSession_Marathon_Sets20Rounds3Lives()
    {
        var session = GameSession.CreateBossSession(Guid.NewGuid(), BossType.Marathon, DifficultyLevel.Advanced);

        session.TotalRounds.Should().Be(20);
        session.LivesRemaining.Should().Be(3);
        session.BossType.Should().Be(BossType.Marathon);
        session.Mode.Should().Be(GameMode.Boss);
        session.ForbiddenLetters.Should().BeNull();
        session.RevealedLettersCount.Should().Be(0);
    }

    // --- CreateBossSession Condition ---

    [Fact]
    public void CreateBossSession_Condition_Sets15Rounds3LivesWithForbiddenLetters()
    {
        var session = GameSession.CreateBossSession(Guid.NewGuid(), BossType.Condition, DifficultyLevel.Expert);

        session.TotalRounds.Should().Be(15);
        session.LivesRemaining.Should().Be(3);
        session.BossType.Should().Be(BossType.Condition);
        session.ForbiddenLetters.Should().NotBeNullOrEmpty();
        session.ForbiddenLetters!.Length.Should().Be(2);
        session.RevealedLettersCount.Should().Be(0);
    }

    [Fact]
    public void CreateBossSession_Condition_ForbiddenLettersAreVowels()
    {
        var session = GameSession.CreateBossSession(Guid.NewGuid(), BossType.Condition, DifficultyLevel.Expert);

        var vowels = new[] { 'A', 'E', 'I', 'O', 'U' };
        foreach (var ch in session.ForbiddenLetters!)
        {
            vowels.Should().Contain(ch);
        }
    }

    // --- CreateBossSession Twist ---

    [Fact]
    public void CreateBossSession_Twist_Sets12Rounds3LivesWith2RevealedLetters()
    {
        var session = GameSession.CreateBossSession(Guid.NewGuid(), BossType.Twist, DifficultyLevel.Intermediate);

        session.TotalRounds.Should().Be(12);
        session.LivesRemaining.Should().Be(3);
        session.BossType.Should().Be(BossType.Twist);
        session.RevealedLettersCount.Should().Be(2);
        session.ForbiddenLetters.Should().BeNull();
    }

    // --- RecordCorrectAnswer ---

    [Fact]
    public void RecordCorrectAnswer_IncrementsComboAndCorrectAnswers()
    {
        var session = CreateSession();

        session.RecordCorrectAnswer();

        session.ComboCount.Should().Be(1);
        session.CorrectAnswers.Should().Be(1);
    }

    [Fact]
    public void RecordCorrectAnswer_MultipleTimes_AccumulatesCombo()
    {
        var session = CreateSession();

        session.RecordCorrectAnswer();
        session.RecordCorrectAnswer();
        session.RecordCorrectAnswer();

        session.ComboCount.Should().Be(3);
        session.CorrectAnswers.Should().Be(3);
    }

    // --- RecordWrongAnswer ---

    [Fact]
    public void RecordWrongAnswer_ResetsComboAndDecrementsLives()
    {
        var session = CreateSession(lives: 3);
        session.RecordCorrectAnswer();
        session.RecordCorrectAnswer();

        session.RecordWrongAnswer();

        session.ComboCount.Should().Be(0);
        session.LivesRemaining.Should().Be(2);
        session.Status.Should().Be(GameSessionStatus.InProgress);
    }

    [Fact]
    public void RecordWrongAnswer_AtOneLive_AutoFails()
    {
        var session = CreateSession(lives: 1);

        session.RecordWrongAnswer();

        session.LivesRemaining.Should().Be(0);
        session.Status.Should().Be(GameSessionStatus.Failed);
        session.EndedAt.Should().NotBeNull();
        session.IsGameOver.Should().BeTrue();
    }

    [Fact]
    public void RecordWrongAnswer_MultipleTimes_DrainLives()
    {
        var session = CreateSession(lives: 3);

        session.RecordWrongAnswer();
        session.RecordWrongAnswer();

        session.LivesRemaining.Should().Be(1);
        session.Status.Should().Be(GameSessionStatus.InProgress);
    }

    // --- AdvanceToNextRound ---

    [Fact]
    public void AdvanceToNextRound_IncrementsCurrentRound()
    {
        var session = CreateSession();

        session.AdvanceToNextRound();

        session.CurrentRound.Should().Be(2);
    }

    [Fact]
    public void AdvanceToNextRound_MultipleTimes_IncrementSequentially()
    {
        var session = CreateSession();

        session.AdvanceToNextRound();
        session.AdvanceToNextRound();
        session.AdvanceToNextRound();

        session.CurrentRound.Should().Be(4);
    }

    // --- Complete ---

    [Fact]
    public void Complete_SetsStatusAndEndedAt()
    {
        var session = CreateSession();

        session.Complete();

        session.Status.Should().Be(GameSessionStatus.Completed);
        session.EndedAt.Should().NotBeNull();
        session.EndedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    // --- Fail ---

    [Fact]
    public void Fail_SetsStatusAndEndedAt()
    {
        var session = CreateSession();

        session.Fail();

        session.Status.Should().Be(GameSessionStatus.Failed);
        session.EndedAt.Should().NotBeNull();
        session.IsGameOver.Should().BeTrue();
    }

    // --- Forfeit ---

    [Fact]
    public void Forfeit_SetsStatusAndEndedAt()
    {
        var session = CreateSession();

        session.Forfeit();

        session.Status.Should().Be(GameSessionStatus.Forfeited);
        session.EndedAt.Should().NotBeNull();
    }

    // --- AddXP ---

    [Fact]
    public void AddXP_AccumulatesXP()
    {
        var session = CreateSession();

        session.AddXP(10);
        session.AddXP(25);

        session.TotalXP.Should().Be(35);
    }

    [Fact]
    public void AddXP_ZeroXP_NoChange()
    {
        var session = CreateSession();

        session.AddXP(0);

        session.TotalXP.Should().Be(0);
    }

    // --- LoseLife ---

    [Fact]
    public void LoseLife_DecrementsLives()
    {
        var session = CreateSession(lives: 3);

        session.LoseLife();

        session.LivesRemaining.Should().Be(2);
    }

    [Fact]
    public void LoseLife_LastLife_AutoFails()
    {
        var session = CreateSession(lives: 1);

        session.LoseLife();

        session.LivesRemaining.Should().Be(0);
        session.Status.Should().Be(GameSessionStatus.Failed);
        session.IsGameOver.Should().BeTrue();
    }

    // --- SetWordIds / GetWordIdForRound ---

    [Fact]
    public void GetWordIdForRound_ValidRound_ReturnsCorrectId()
    {
        var session = CreateSession();
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        session.SetWordIds(ids);

        session.GetWordIdForRound(1).Should().Be(ids[0]);
        session.GetWordIdForRound(2).Should().Be(ids[1]);
        session.GetWordIdForRound(3).Should().Be(ids[2]);
    }

    [Fact]
    public void GetWordIdForRound_InvalidRound_ThrowsArgumentOutOfRangeException()
    {
        var session = CreateSession();
        session.SetWordIds(new List<Guid> { Guid.NewGuid() });

        var act = () => session.GetWordIdForRound(0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetWordIdForRound_RoundBeyondCount_ThrowsArgumentOutOfRangeException()
    {
        var session = CreateSession();
        session.SetWordIds(new List<Guid> { Guid.NewGuid() });

        var act = () => session.GetWordIdForRound(2);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // --- AddRound ---

    [Fact]
    public void AddRound_CreatesAndAddsRound()
    {
        var session = CreateSession();
        var wordId = Guid.NewGuid();

        var round = session.AddRound(wordId, "tca");

        session.Rounds.Should().HaveCount(1);
        round.SessionId.Should().Be(session.Id);
        round.WordId.Should().Be(wordId);
        round.ScrambledWord.Should().Be("tca");
    }

    // --- IsGameOver ---

    [Fact]
    public void IsGameOver_LivesRemaining_ReturnsFalse()
    {
        var session = CreateSession(lives: 3);

        session.IsGameOver.Should().BeFalse();
    }

    [Fact]
    public void IsGameOver_FailedStatus_ReturnsTrue()
    {
        var session = CreateSession(lives: 3);
        session.Fail();

        session.IsGameOver.Should().BeTrue();
    }
}
