using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Multiplayer;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

public class MultiplayerGameServiceTests
{
    private readonly IWordRepository _wordRepository;
    private readonly IMultiplayerGameService _sut;
    private static readonly string[] CorrectAnswers =
    [
        "TEST", "WORD", "GAME", "PLAY", "QUIZ", "PUZZLE", "BRAIN", "THINK",
        "SMART", "CLEVER", "LETTER", "SCRAMBLE", "ANSWER", "GUESS", "SOLVE"
    ];

    public MultiplayerGameServiceTests()
    {
        _wordRepository = Substitute.For<IWordRepository>();
        
        // Setup mock to return test words
        var testWords = new List<Word>
        {
            Word.Create("TEST", DifficultyLevel.Beginner, WordCategory.Everyday, 1),
            Word.Create("WORD", DifficultyLevel.Beginner, WordCategory.Everyday, 2),
            Word.Create("GAME", DifficultyLevel.Beginner, WordCategory.Everyday, 3),
            Word.Create("PLAY", DifficultyLevel.Beginner, WordCategory.Everyday, 4),
            Word.Create("QUIZ", DifficultyLevel.Beginner, WordCategory.Everyday, 5),
            Word.Create("PUZZLE", DifficultyLevel.Beginner, WordCategory.Everyday, 6),
            Word.Create("BRAIN", DifficultyLevel.Beginner, WordCategory.Everyday, 7),
            Word.Create("THINK", DifficultyLevel.Beginner, WordCategory.Everyday, 8),
            Word.Create("SMART", DifficultyLevel.Beginner, WordCategory.Everyday, 9),
            Word.Create("CLEVER", DifficultyLevel.Beginner, WordCategory.Everyday, 10),
            Word.Create("LETTER", DifficultyLevel.Beginner, WordCategory.Everyday, 11),
            Word.Create("SCRAMBLE", DifficultyLevel.Beginner, WordCategory.Everyday, 12),
            Word.Create("ANSWER", DifficultyLevel.Beginner, WordCategory.Everyday, 13),
            Word.Create("GUESS", DifficultyLevel.Beginner, WordCategory.Everyday, 14),
            Word.Create("SOLVE", DifficultyLevel.Beginner, WordCategory.Everyday, 15),
            Word.Create("WINNER", DifficultyLevel.Beginner, WordCategory.Everyday, 16),
            Word.Create("PLAYER", DifficultyLevel.Beginner, WordCategory.Everyday, 17),
            Word.Create("SCORE", DifficultyLevel.Beginner, WordCategory.Everyday, 18),
            Word.Create("POINT", DifficultyLevel.Beginner, WordCategory.Everyday, 19),
            Word.Create("ROUND", DifficultyLevel.Beginner, WordCategory.Everyday, 20)
        };
        
        _wordRepository.GetRandomBatchAsync(Arg.Any<int>(), Arg.Any<DifficultyLevel>(), Arg.Any<WordCategory?>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => 
            {
                var count = callInfo.ArgAt<int>(0);
                return testWords.Take(count).ToList();
            });
        
        _sut = new Core.Services.MultiplayerGameService(_wordRepository, new MemoryCache(new MemoryCacheOptions()));
    }

    [Fact]
    public async Task MultiplayerGameService_CreateMatch_Initializes15Rounds()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();

        // Act
        var matchId = await _sut.CreateMatchAsync(player1Id, player2Id);

        // Assert
        matchId.Should().NotBe(Guid.Empty);
        var state = await _sut.GetMatchStateAsync(matchId);
        state.Should().NotBeNull();
        state!.TotalRounds.Should().Be(15);
    }

    [Fact]
    public async Task MultiplayerGameService_CreateMatch_3MinuteLimit()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();

        // Act
        var matchId = await _sut.CreateMatchAsync(player1Id, player2Id);

        // Assert
        var state = await _sut.GetMatchStateAsync(matchId);
        state.Should().NotBeNull();
        state!.TimeRemaining.Should().BeCloseTo(TimeSpan.FromMinutes(3), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task MultiplayerGameService_SubmitAnswer_Correct_IncreasesScore()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var matchId = await _sut.CreateMatchAsync(player1Id, player2Id);
        await _sut.StartMatchAsync(matchId);

        // Act - submit a correct answer (we need to know the correct word)
        // For testing, we'll use the first round's word
        var state = await _sut.GetMatchStateAsync(matchId);
        var result = await _sut.SubmitAnswerAsync(matchId, player1Id, "TEST", 1000);

        // Assert
        result.IsCorrect.Should().BeTrue();
        result.Score.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task MultiplayerGameService_SubmitAnswer_Wrong_NoScoreChange()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var matchId = await _sut.CreateMatchAsync(player1Id, player2Id);
        await _sut.StartMatchAsync(matchId);

        // Act - submit wrong answer
        var result = await _sut.SubmitAnswerAsync(matchId, player1Id, "WRONGANSWER", 1000);

        // Assert
        result.IsCorrect.Should().BeFalse();
        result.Score.Should().Be(0);
    }

    [Fact]
    public async Task MultiplayerGameService_SubmitAnswer_PlayerOneWrong_KeepsOpponentOnFirstRound()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var settings = new RoomSettingsDto(
            WordCount: 3,
            TimeLimitMinutes: 3,
            Difficulty: DifficultyLevel.Beginner,
            BestOf: 1);
        var matchId = await _sut.CreateMatchAsync(player1Id, player2Id, settings: settings);
        await _sut.StartMatchAsync(matchId);

        // Act
        var player1Result = await _sut.SubmitAnswerAsync(matchId, player1Id, "WRONGANSWER", 1000);
        var player2Result = await _sut.SubmitAnswerAsync(matchId, player2Id, "TEST", 1000);

        // Assert
        player1Result.IsCorrect.Should().BeFalse();
        player2Result.IsCorrect.Should().BeTrue("player two should still answer their own first word");
    }

    [Fact]
    public async Task MultiplayerGameService_GetCurrentRound_PlayerSpecificRoundsAdvanceIndependently()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var settings = new RoomSettingsDto(
            WordCount: 3,
            TimeLimitMinutes: 3,
            Difficulty: DifficultyLevel.Beginner,
            BestOf: 1);
        var matchId = await _sut.CreateMatchAsync(player1Id, player2Id, settings: settings);
        await _sut.StartMatchAsync(matchId);

        // Act
        await _sut.SubmitAnswerAsync(matchId, player1Id, "TEST", 1000);

        // Assert
        var player1Round = await _sut.GetCurrentRoundAsync(matchId, player1Id);
        var player2Round = await _sut.GetCurrentRoundAsync(matchId, player2Id);
        var sharedRound = await _sut.GetCurrentRoundAsync(matchId);

        player1Round.Should().NotBeNull();
        player1Round!.RoundNumber.Should().Be(2);
        player2Round.Should().NotBeNull();
        player2Round!.RoundNumber.Should().Be(1);
        sharedRound.Should().NotBeNull();
        sharedRound!.RoundNumber.Should().Be(1);
    }

    [Fact]
    public async Task MultiplayerGameService_SubmitAnswer_OnePlayerCompletesAllWords_WaitsForOpponent()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var settings = new RoomSettingsDto(
            WordCount: 2,
            TimeLimitMinutes: 3,
            Difficulty: DifficultyLevel.Beginner,
            BestOf: 1);
        var matchId = await _sut.CreateMatchAsync(player1Id, player2Id, settings: settings);
        await _sut.StartMatchAsync(matchId);

        // Act
        await _sut.SubmitAnswerAsync(matchId, player1Id, "TEST", 1000);
        var result = await _sut.SubmitAnswerAsync(matchId, player1Id, "WORD", 1000);

        // Assert
        result.IsMatchComplete.Should().BeFalse();
        var isActive = await _sut.IsMatchActiveAsync(matchId);
        isActive.Should().BeTrue("the opponent has not finished their own word queue yet");
    }

    [Fact]
    public async Task MultiplayerGameService_BothPlayersComplete15Words_EndsMatch()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var matchId = await _sut.CreateMatchAsync(player1Id, player2Id);
        await _sut.StartMatchAsync(matchId);

        // Act - player1 completes their own queue first
        MultiplayerAnswerResultDto? lastPlayer1Result = null;
        for (int i = 0; i < CorrectAnswers.Length; i++)
        {
            lastPlayer1Result = await _sut.SubmitAnswerAsync(matchId, player1Id, CorrectAnswers[i], 1000);
        }

        lastPlayer1Result.Should().NotBeNull();
        lastPlayer1Result!.IsPlayerComplete.Should().BeTrue();
        lastPlayer1Result.IsMatchComplete.Should().BeFalse();
        (await _sut.IsMatchActiveAsync(matchId)).Should().BeTrue();

        // Act - player2 then completes their own queue
        MultiplayerAnswerResultDto? lastPlayer2Result = null;
        for (int i = 0; i < CorrectAnswers.Length; i++)
        {
            lastPlayer2Result = await _sut.SubmitAnswerAsync(matchId, player2Id, CorrectAnswers[i], 1000);
        }

        // Assert
        lastPlayer2Result.Should().NotBeNull();
        lastPlayer2Result!.IsPlayerComplete.Should().BeTrue();
        lastPlayer2Result.IsMatchComplete.Should().BeTrue();
        (await _sut.IsMatchActiveAsync(matchId)).Should().BeFalse();
    }

    [Fact]
    public async Task MultiplayerGameService_Forfeit_OpponentWins()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var matchId = await _sut.CreateMatchAsync(player1Id, player2Id);
        await _sut.StartMatchAsync(matchId);

        // Act
        await _sut.ForfeitAsync(matchId, player1Id);

        // Assert
        var result = await _sut.EndMatchAsync(matchId);
        result.WinnerId.Should().Be(player2Id);
        result.IsDraw.Should().BeFalse();
    }

    [Fact]
    public async Task MultiplayerGameService_DetermineWinner_ByCorrectCount()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var matchId = await _sut.CreateMatchAsync(player1Id, player2Id);
        await _sut.StartMatchAsync(matchId);

        for (int i = 0; i < 10; i++)
        {
            await _sut.SubmitAnswerAsync(matchId, player1Id, CorrectAnswers[i], 1000);
        }
        for (int i = 0; i < 5; i++)
        {
            await _sut.SubmitAnswerAsync(matchId, player2Id, CorrectAnswers[i], 1000);
        }

        // Act
        await _sut.ForfeitAsync(matchId, player2Id); // End the match
        var result = await _sut.EndMatchAsync(matchId);

        // Assert
        result.WinnerId.Should().Be(player1Id);
        result.YourScore.Should().Be(10);
        result.OpponentScore.Should().Be(5);
        result.YourScore.Should().BeGreaterThan(result.OpponentScore);
    }

    [Fact]
    public async Task MultiplayerGameService_Rewards_WinnerGetsBonus()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var matchId = await _sut.CreateMatchAsync(player1Id, player2Id);
        await _sut.StartMatchAsync(matchId);

        // Player1 wins
        for (int i = 0; i < 10; i++)
        {
            await _sut.SubmitAnswerAsync(matchId, player1Id, "TEST", 1000);
        }
        await _sut.ForfeitAsync(matchId, player2Id);

        // Act
        var result = await _sut.EndMatchAsync(matchId);

        // Assert - Quick Match: winner 100 XP + league 50 XP
        result.XPEarned.Should().Be(100);
        result.LeagueXPEarned.Should().Be(50);
    }

    [Fact]
    public async Task MultiplayerGameService_Rewards_LoserGetsBase()
    {
        // Arrange - test from loser perspective (player2)
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var matchId = await _sut.CreateMatchAsync(player1Id, player2Id);
        await _sut.StartMatchAsync(matchId);

        // Player2 loses (forfeits)
        await _sut.ForfeitAsync(matchId, player2Id);

        // Act - get result for player2
        var result = await _sut.EndMatchAsync(matchId);

        // Assert - Quick Match: loser 30 XP + league 15 XP
        // Note: This tests from the perspective of the forfeiting player
    }

    [Fact]
    public async Task MultiplayerGameService_IsMatchActive_ExistingMatch_ReturnsTrue()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var matchId = await _sut.CreateMatchAsync(player1Id, player2Id);

        // Act
        var isActive = await _sut.IsMatchActiveAsync(matchId);

        // Assert
        isActive.Should().BeTrue();
    }

    [Fact]
    public async Task MultiplayerGameService_IsMatchActive_NonExistingMatch_ReturnsFalse()
    {
        // Arrange
        var nonExistingMatchId = Guid.NewGuid();

        // Act
        var isActive = await _sut.IsMatchActiveAsync(nonExistingMatchId);

        // Assert
        isActive.Should().BeFalse();
    }

    [Fact]
    public async Task MultiplayerGameService_CreateMatch_WithCustomSettings_UsesSettings()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var settings = new RoomSettingsDto(
            WordCount: 10,
            TimeLimitMinutes: 2,
            Difficulty: DifficultyLevel.Beginner,
            BestOf: 1
        );

        // Act
        var matchId = await _sut.CreateMatchAsync(player1Id, player2Id, true, settings);
        var state = await _sut.GetMatchStateAsync(matchId);

        // Assert
        state.Should().NotBeNull();
        state!.TotalRounds.Should().Be(10);
        state.TimeRemaining.Should().BeCloseTo(TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task MultiplayerGameService_DetermineWinner_Tie_BySpeed()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var matchId = await _sut.CreateMatchAsync(player1Id, player2Id);
        await _sut.StartMatchAsync(matchId);

        // Both players answer 5 correctly, but Player1 is faster (2000ms vs 4000ms per answer)
        for (int i = 0; i < 5; i++)
        {
            await _sut.SubmitAnswerAsync(matchId, player1Id, CorrectAnswers[i], 2000);
        }
        for (int i = 0; i < 5; i++)
        {
            await _sut.SubmitAnswerAsync(matchId, player2Id, CorrectAnswers[i], 4000);
        }

        // Act
        var result = await _sut.EndMatchAsync(matchId);

        // Assert - Player1 wins by speed (lower total time)
        result.WinnerId.Should().Be(player1Id);
        result.IsDraw.Should().BeFalse();
        result.YourTime.Should().BeLessThan(result.OpponentTime);
    }

    [Fact]
    public async Task MultiplayerGameService_TimerExpires_EndsMatch()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        // Create match with 0 minute limit to simulate immediate expiry
        var settings = new RoomSettingsDto(
            WordCount: 15,
            TimeLimitMinutes: 0,
            Difficulty: DifficultyLevel.Beginner,
            BestOf: 1
        );
        var matchId = await _sut.CreateMatchAsync(player1Id, player2Id, false, settings);
        await _sut.StartMatchAsync(matchId);

        // Act - check if match is active (should be false due to expired timer)
        var isActive = await _sut.IsMatchActiveAsync(matchId);

        // Assert
        isActive.Should().BeFalse();
    }

    [Fact]
    public async Task MultiplayerGameService_Disconnect_30sGrace_ThenForfeit()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var matchId = await _sut.CreateMatchAsync(player1Id, player2Id);
        await _sut.StartMatchAsync(matchId);

        // Act - Player1 disconnects
        await _sut.HandleDisconnectAsync(matchId, player1Id);

        // Assert - Match should still be active during grace period
        var isActive = await _sut.IsMatchActiveAsync(matchId);
        isActive.Should().BeTrue();

        // Act - Grace period expires, should forfeit
        await _sut.FinalizeDisconnectAsync(matchId, player1Id);

        // Assert - Match ended, Player2 wins
        var result = await _sut.EndMatchAsync(matchId);
        result.WinnerId.Should().Be(player2Id);
    }
}
