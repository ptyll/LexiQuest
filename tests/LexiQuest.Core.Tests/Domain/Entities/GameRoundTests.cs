using FluentAssertions;
using LexiQuest.Core.Domain.Entities;

namespace LexiQuest.Core.Tests.Domain.Entities;

public class GameRoundTests
{
    private static GameRound CreateRound(
        string scrambled = "tca",
        string correct = "cat",
        int timeLimit = 30) =>
        GameRound.Create(Guid.NewGuid(), 1, Guid.NewGuid(), scrambled, correct, timeLimit);

    // --- Create ---

    [Fact]
    public void Create_SetsDefaultProperties()
    {
        var sessionId = Guid.NewGuid();
        var wordId = Guid.NewGuid();

        var round = GameRound.Create(sessionId, 3, wordId, "tca", "cat", 45);

        round.Id.Should().NotBeEmpty();
        round.SessionId.Should().Be(sessionId);
        round.RoundNumber.Should().Be(3);
        round.WordId.Should().Be(wordId);
        round.ScrambledWord.Should().Be("tca");
        round.CorrectAnswer.Should().Be("cat");
        round.TimeLimitSeconds.Should().Be(45);
        round.StartedAt.Should().NotBeNull();
        round.IsCorrect.Should().BeFalse();
        round.IsCompleted.Should().BeFalse();
        round.XPEarned.Should().Be(0);
        round.UserAnswer.Should().BeNull();
        round.AnsweredAt.Should().BeNull();
        round.ForbiddenLetters.Should().BeNull();
        round.RevealedLettersCount.Should().Be(0);
        round.RevealedPositions.Should().BeNull();
    }

    // --- RecordAttempt ---

    [Fact]
    public void RecordAttempt_CorrectAnswer_SetsFieldsCorrectly()
    {
        var round = CreateRound();

        round.RecordAttempt("cat", true, 1500);

        round.UserAnswer.Should().Be("cat");
        round.IsCorrect.Should().BeTrue();
        round.TimeSpentMs.Should().Be(1500);
        round.IsCompleted.Should().BeTrue();
        round.AnsweredAt.Should().NotBeNull();
    }

    [Fact]
    public void RecordAttempt_WrongAnswer_SetsFieldsCorrectly()
    {
        var round = CreateRound();

        round.RecordAttempt("tac", false, 2500);

        round.UserAnswer.Should().Be("tac");
        round.IsCorrect.Should().BeFalse();
        round.TimeSpentMs.Should().Be(2500);
        round.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void RecordAttempt_ZeroTime_Allowed()
    {
        var round = CreateRound();

        round.RecordAttempt("cat", true, 0);

        round.TimeSpentMs.Should().Be(0);
        round.IsCompleted.Should().BeTrue();
    }

    // --- SubmitAnswer ---

    [Fact]
    public void SubmitAnswer_SetsAnswerCorrectnessAndXP()
    {
        var round = CreateRound();

        round.SubmitAnswer("cat", true, 25);

        round.UserAnswer.Should().Be("cat");
        round.IsCorrect.Should().BeTrue();
        round.XPEarned.Should().Be(25);
        round.IsCompleted.Should().BeTrue();
        round.AnsweredAt.Should().NotBeNull();
    }

    [Fact]
    public void SubmitAnswer_WrongAnswer_ZeroXP()
    {
        var round = CreateRound();

        round.SubmitAnswer("bad", false, 0);

        round.IsCorrect.Should().BeFalse();
        round.XPEarned.Should().Be(0);
        round.IsCompleted.Should().BeTrue();
    }

    // --- SetXPEarned ---

    [Fact]
    public void SetXPEarned_SetsValue()
    {
        var round = CreateRound();

        round.SetXPEarned(42);

        round.XPEarned.Should().Be(42);
    }

    // --- SetForbiddenLetters ---

    [Fact]
    public void SetForbiddenLetters_SetsValue()
    {
        var round = CreateRound();

        round.SetForbiddenLetters("AE");

        round.ForbiddenLetters.Should().Be("AE");
    }

    // --- ContainsForbiddenLetter ---

    [Fact]
    public void ContainsForbiddenLetter_AnswerContainsForbidden_ReturnsTrue()
    {
        var round = CreateRound();
        round.SetForbiddenLetters("AE");

        round.ContainsForbiddenLetter("apple").Should().BeTrue();
    }

    [Fact]
    public void ContainsForbiddenLetter_AnswerDoesNotContainForbidden_ReturnsFalse()
    {
        var round = CreateRound();
        round.SetForbiddenLetters("XZ");

        round.ContainsForbiddenLetter("cat").Should().BeFalse();
    }

    [Fact]
    public void ContainsForbiddenLetter_CaseInsensitive()
    {
        var round = CreateRound();
        round.SetForbiddenLetters("A");

        round.ContainsForbiddenLetter("APPLE").Should().BeTrue();
        round.ContainsForbiddenLetter("apple").Should().BeTrue();
    }

    [Fact]
    public void ContainsForbiddenLetter_NoForbiddenLettersSet_ReturnsFalse()
    {
        var round = CreateRound();

        round.ContainsForbiddenLetter("anything").Should().BeFalse();
    }

    [Fact]
    public void ContainsForbiddenLetter_EmptyAnswer_ReturnsFalse()
    {
        var round = CreateRound();
        round.SetForbiddenLetters("AE");

        round.ContainsForbiddenLetter("").Should().BeFalse();
    }

    [Fact]
    public void ContainsForbiddenLetter_NullAnswer_ReturnsFalse()
    {
        var round = CreateRound();
        round.SetForbiddenLetters("AE");

        round.ContainsForbiddenLetter(null!).Should().BeFalse();
    }

    // --- SetRevealedPositions ---

    [Fact]
    public void SetRevealedPositions_StoresAsCommaSeparatedString()
    {
        var round = CreateRound();

        round.SetRevealedPositions(new[] { 0, 2, 4 });

        round.RevealedPositions.Should().Be("0,2,4");
        round.RevealedLettersCount.Should().Be(3);
    }

    [Fact]
    public void SetRevealedPositions_SinglePosition_NoComma()
    {
        var round = CreateRound();

        round.SetRevealedPositions(new[] { 1 });

        round.RevealedPositions.Should().Be("1");
        round.RevealedLettersCount.Should().Be(1);
    }

    [Fact]
    public void SetRevealedPositions_EmptyArray_EmptyString()
    {
        var round = CreateRound();

        round.SetRevealedPositions(Array.Empty<int>());

        round.RevealedPositions.Should().Be("");
        round.RevealedLettersCount.Should().Be(0);
    }

    // --- RevealLetter ---

    [Fact]
    public void RevealLetter_IncrementsCount()
    {
        var round = CreateRound();

        round.RevealLetter();

        round.RevealedLettersCount.Should().Be(1);
    }

    [Fact]
    public void RevealLetter_MultipleTimes_Accumulates()
    {
        var round = CreateRound();

        round.RevealLetter();
        round.RevealLetter();
        round.RevealLetter();

        round.RevealedLettersCount.Should().Be(3);
    }
}
