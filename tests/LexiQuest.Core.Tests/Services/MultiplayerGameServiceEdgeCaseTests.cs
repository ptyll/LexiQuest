using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Services;
using LexiQuest.Shared.DTOs.Multiplayer;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

public class MultiplayerGameServiceEdgeCaseTests
{
    private readonly IWordRepository _wordRepository = Substitute.For<IWordRepository>();
    private readonly MultiplayerGameService _sut;

    public MultiplayerGameServiceEdgeCaseTests()
    {
        var testWords = new List<Word>();
        var wordTexts = new[]
        {
            "TEST", "WORD", "GAME", "PLAY", "QUIZ", "PUZZLE", "BRAIN", "THINK",
            "SMART", "CLEVER", "LETTER", "SCRAMBLE", "ANSWER", "GUESS", "SOLVE",
            "WINNER", "PLAYER", "SCORE", "POINT", "ROUND"
        };
        for (int i = 0; i < wordTexts.Length; i++)
        {
            testWords.Add(Word.Create(wordTexts[i], DifficultyLevel.Beginner, WordCategory.Everyday, i + 1));
        }

        _wordRepository.GetRandomBatchAsync(Arg.Any<int>(), Arg.Any<DifficultyLevel>(), Arg.Any<WordCategory?>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var count = callInfo.ArgAt<int>(0);
                return testWords.Take(count).ToList();
            });

        _sut = new MultiplayerGameService(_wordRepository, new MemoryCache(new MemoryCacheOptions()));
    }

    private async Task<Guid> CreateAndStartMatch(Guid player1Id, Guid player2Id, int wordCount = 15, int timeLimitMinutes = 3)
    {
        var settings = new RoomSettingsDto(WordCount: wordCount, TimeLimitMinutes: timeLimitMinutes, Difficulty: DifficultyLevel.Beginner, BestOf: 1);
        var matchId = await _sut.CreateMatchAsync(player1Id, player2Id, false, settings);
        await _sut.StartMatchAsync(matchId);
        return matchId;
    }

    // --- Disconnect marks player ---

    [Fact]
    public async Task HandleDisconnect_MarksPlayer_MatchStillActive()
    {
        // Arrange
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        var matchId = await CreateAndStartMatch(p1, p2);

        // Act
        await _sut.HandleDisconnectAsync(matchId, p1);

        // Assert - match should still be active (grace period)
        var isActive = await _sut.IsMatchActiveAsync(matchId);
        isActive.Should().BeTrue();
    }

    [Fact]
    public async Task HandleDisconnect_NonExistentMatch_DoesNotThrow()
    {
        // Act
        var act = () => _sut.HandleDisconnectAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        await act.Should().NotThrowAsync();
    }

    // --- Reconnect within grace period restores state ---

    [Fact]
    public async Task HandleReconnect_WithinGracePeriod_ReturnsTrue()
    {
        // Arrange
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        var matchId = await CreateAndStartMatch(p1, p2);
        await _sut.HandleDisconnectAsync(matchId, p1);

        // Act
        var result = await _sut.HandleReconnectAsync(matchId, p1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HandleReconnect_ClearsDisconnectState()
    {
        // Arrange
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        var matchId = await CreateAndStartMatch(p1, p2);
        await _sut.HandleDisconnectAsync(matchId, p1);

        // Act
        await _sut.HandleReconnectAsync(matchId, p1);

        // After reconnect, finalize should NOT forfeit because disconnect was cleared
        await _sut.FinalizeDisconnectAsync(matchId, p1);

        // Assert - match should still be active since disconnect was cleared
        var isActive = await _sut.IsMatchActiveAsync(matchId);
        isActive.Should().BeTrue();
    }

    [Fact]
    public async Task HandleReconnect_NonExistentMatch_ReturnsFalse()
    {
        // Act
        var result = await _sut.HandleReconnectAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HandleReconnect_MatchAlreadyEnded_ReturnsFalse()
    {
        // Arrange
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        var matchId = await CreateAndStartMatch(p1, p2);
        await _sut.ForfeitAsync(matchId, p1); // End the match

        // Act
        var result = await _sut.HandleReconnectAsync(matchId, p1);

        // Assert
        result.Should().BeFalse();
    }

    // --- Forfeit after grace period ---

    [Fact]
    public async Task FinalizeDisconnect_PlayerStillDisconnected_ForfeitsMatch()
    {
        // Arrange
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        var matchId = await CreateAndStartMatch(p1, p2);
        await _sut.HandleDisconnectAsync(matchId, p1);

        // Act
        await _sut.FinalizeDisconnectAsync(matchId, p1);

        // Assert
        var isActive = await _sut.IsMatchActiveAsync(matchId);
        isActive.Should().BeFalse();

        var result = await _sut.EndMatchAsync(matchId);
        result.WinnerId.Should().Be(p2); // Other player wins
    }

    [Fact]
    public async Task FinalizeDisconnect_NonExistentMatch_DoesNotThrow()
    {
        // Act
        var act = () => _sut.FinalizeDisconnectAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task FinalizeDisconnect_MatchAlreadyInactive_DoesNotForfeit()
    {
        // Arrange
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        var matchId = await CreateAndStartMatch(p1, p2);
        await _sut.HandleDisconnectAsync(matchId, p1);
        await _sut.ForfeitAsync(matchId, p2); // Match ended by p2 forfeit

        // Act - finalize disconnect should be a no-op
        await _sut.FinalizeDisconnectAsync(matchId, p1);

        // Assert - p2 forfeited, so p1 should be winner
        var result = await _sut.EndMatchAsync(matchId);
        result.WinnerId.Should().Be(p1);
    }

    // --- Submit answer after match ended ---

    [Fact]
    public async Task SubmitAnswer_AfterForfeit_ThrowsOrHandles()
    {
        // Arrange
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        var matchId = await CreateAndStartMatch(p1, p2, wordCount: 5);
        await _sut.ForfeitAsync(matchId, p1); // Match ended

        // Act - submitting answer should still work (match tracks internally)
        // The match.CurrentRound advances but match is already marked inactive
        var result = await _sut.SubmitAnswerAsync(matchId, p2, "TEST", 1000);

        // Assert - the answer is processed but match was already inactive
        // The important thing is it doesn't crash
        result.Score.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task SubmitAnswer_NonExistentMatch_Throws()
    {
        // Act
        var act = () => _sut.SubmitAnswerAsync(Guid.NewGuid(), Guid.NewGuid(), "TEST", 1000);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Match not found*");
    }

    // --- Score calculation edge cases ---

    [Fact]
    public async Task SubmitAnswer_VeryFastCorrect_GetsMaxTimeBonus()
    {
        // Arrange
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        var matchId = await CreateAndStartMatch(p1, p2);

        // Act - answer in under 5000ms -> base 10 + time bonus 5 = 15
        var result = await _sut.SubmitAnswerAsync(matchId, p1, "TEST", 2000);

        // Assert
        result.IsCorrect.Should().BeTrue();
        result.Score.Should().Be(15); // 10 base + 5 time bonus
    }

    [Fact]
    public async Task SubmitAnswer_MediumSpeed_GetsMediumTimeBonus()
    {
        // Arrange
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        var matchId = await CreateAndStartMatch(p1, p2);

        // Act - answer between 5000-10000ms -> base 10 + time bonus 3 = 13
        var result = await _sut.SubmitAnswerAsync(matchId, p1, "TEST", 7000);

        // Assert
        result.IsCorrect.Should().BeTrue();
        result.Score.Should().Be(13);
    }

    [Fact]
    public async Task SubmitAnswer_SlowCorrect_NoTimeBonus()
    {
        // Arrange
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        var matchId = await CreateAndStartMatch(p1, p2);

        // Act - answer over 10000ms -> base 10 + 0 time bonus = 10
        var result = await _sut.SubmitAnswerAsync(matchId, p1, "TEST", 15000);

        // Assert
        result.IsCorrect.Should().BeTrue();
        result.Score.Should().Be(10);
    }

    [Fact]
    public async Task SubmitAnswer_WrongAnswer_BreaksCombo()
    {
        // Arrange
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        var matchId = await CreateAndStartMatch(p1, p2);

        // Get correct answer first to build combo
        await _sut.SubmitAnswerAsync(matchId, p1, "TEST", 1000);

        // Act - wrong answer
        var result = await _sut.SubmitAnswerAsync(matchId, p1, "WRONG", 1000);

        // Assert
        result.IsCorrect.Should().BeFalse();
        result.Score.Should().Be(15); // Score doesn't decrease, just no addition
    }

    [Fact]
    public async Task SubmitAnswer_CaseInsensitive()
    {
        // Arrange
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        var matchId = await CreateAndStartMatch(p1, p2);

        // Act - lowercase answer for "TEST"
        var result = await _sut.SubmitAnswerAsync(matchId, p1, "test", 1000);

        // Assert
        result.IsCorrect.Should().BeTrue();
    }

    // --- Match completion ---

    [Fact]
    public async Task SubmitAnswer_AllRoundsComplete_MatchEnds()
    {
        // Arrange
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        var matchId = await CreateAndStartMatch(p1, p2, wordCount: 3);

        // Act - complete all 3 rounds
        for (int i = 0; i < 3; i++)
        {
            var result = await _sut.SubmitAnswerAsync(matchId, p1, "TEST", 1000);
            if (i == 2)
                result.IsMatchComplete.Should().BeTrue();
        }

        // Assert
        var isActive = await _sut.IsMatchActiveAsync(matchId);
        isActive.Should().BeFalse();
    }

    // --- EndMatch result calculations ---

    [Fact]
    public async Task EndMatch_NoAnswers_IsDrawTrue()
    {
        // Arrange - neither player submits any answer, both have 0 correct and 0 time
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        var matchId = await CreateAndStartMatch(p1, p2, wordCount: 5);

        // Act - end match immediately with no answers
        var result = await _sut.EndMatchAsync(matchId);

        // Assert - both have 0 correct, 0 time -> draw
        result.IsDraw.Should().BeTrue();
        result.WinnerId.Should().BeNull();
    }

    [Fact]
    public async Task EndMatch_PrivateRoom_NoLeagueXP()
    {
        // Arrange
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        var matchId = await _sut.CreateMatchAsync(p1, p2, isPrivateRoom: true);
        await _sut.StartMatchAsync(matchId);

        // Player 1 wins
        await _sut.SubmitAnswerAsync(matchId, p1, "TEST", 1000);
        await _sut.ForfeitAsync(matchId, p2);

        // Act
        var result = await _sut.EndMatchAsync(matchId);

        // Assert
        result.IsPrivateRoom.Should().BeTrue();
        result.LeagueXPEarned.Should().Be(0);
    }

    [Fact]
    public async Task EndMatch_QuickMatch_WinnerGets100XP_50LeagueXP()
    {
        // Arrange
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        var matchId = await CreateAndStartMatch(p1, p2);

        await _sut.SubmitAnswerAsync(matchId, p1, "TEST", 1000);
        await _sut.ForfeitAsync(matchId, p2);

        // Act
        var result = await _sut.EndMatchAsync(matchId);

        // Assert
        result.WinnerId.Should().Be(p1);
        result.XPEarned.Should().Be(100);
        result.LeagueXPEarned.Should().Be(50);
    }

    // --- GetOpponentProgress ---

    [Fact]
    public async Task GetOpponentProgress_ReturnsOpponentData()
    {
        // Arrange
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        var matchId = await CreateAndStartMatch(p1, p2);

        // Player 2 answers correctly
        await _sut.SubmitAnswerAsync(matchId, p2, "TEST", 2000);

        // Act - get p2's progress from p1's perspective
        var progress = await _sut.GetOpponentProgressAsync(matchId, p1);

        // Assert
        progress.CorrectCount.Should().Be(1);
        progress.TotalAnswered.Should().Be(1);
    }

    [Fact]
    public async Task GetOpponentProgress_NonExistentMatch_Throws()
    {
        // Act
        var act = () => _sut.GetOpponentProgressAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // --- Match expiry ---

    [Fact]
    public async Task IsMatchActive_ExpiredMatch_ReturnsFalse()
    {
        // Arrange - create match with 0 minute limit
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        var settings = new RoomSettingsDto(WordCount: 5, TimeLimitMinutes: 0, Difficulty: DifficultyLevel.Beginner, BestOf: 1);
        var matchId = await _sut.CreateMatchAsync(p1, p2, false, settings);

        // Act
        var isActive = await _sut.IsMatchActiveAsync(matchId);

        // Assert
        isActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleReconnect_ExpiredMatch_ReturnsFalse()
    {
        // Arrange
        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();
        var settings = new RoomSettingsDto(WordCount: 5, TimeLimitMinutes: 0, Difficulty: DifficultyLevel.Beginner, BestOf: 1);
        var matchId = await _sut.CreateMatchAsync(p1, p2, false, settings);
        await _sut.HandleDisconnectAsync(matchId, p1);

        // Act
        var result = await _sut.HandleReconnectAsync(matchId, p1);

        // Assert
        result.Should().BeFalse();
    }
}
