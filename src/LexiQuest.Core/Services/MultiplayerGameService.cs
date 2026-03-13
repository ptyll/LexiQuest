using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Multiplayer;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Caching.Memory;

namespace LexiQuest.Core.Services;

/// <summary>
/// Service for managing multiplayer game sessions.
/// </summary>
public class MultiplayerGameService : IMultiplayerGameService
{
    private readonly IWordRepository _wordRepository;
    private readonly IMemoryCache _cache;
    private readonly Random _random = new();

    private static readonly TimeSpan MatchExpiration = TimeSpan.FromHours(2);

    public MultiplayerGameService(IWordRepository wordRepository, IMemoryCache cache)
    {
        _wordRepository = wordRepository;
        _cache = cache;
    }

    private static string GetMatchKey(Guid matchId) => $"mp_match:{matchId}";

    public async Task<Guid> CreateMatchAsync(Guid player1Id, Guid player2Id, bool isPrivateRoom = false, RoomSettingsDto? settings = null, CancellationToken cancellationToken = default)
    {
        var matchId = Guid.NewGuid();
        var wordCount = settings?.WordCount ?? 15;
        var timeLimitMinutes = settings?.TimeLimitMinutes ?? 3;
        var difficulty = settings?.Difficulty ?? DifficultyLevel.Beginner;

        // Get words for the match
        var words = await _wordRepository.GetRandomBatchAsync(wordCount, difficulty, null, cancellationToken);
        var matchWords = words.Select(w => new MatchWord
        {
            WordId = w.Id,
            Original = w.Original,
            Scrambled = w.Scramble(_random),
            Difficulty = w.Difficulty
        }).ToList();

        var match = new MultiplayerMatch
        {
            Id = matchId,
            Player1Id = player1Id,
            Player2Id = player2Id,
            IsPrivateRoom = isPrivateRoom,
            TotalRounds = wordCount,
            TimeLimitMinutes = timeLimitMinutes,
            Words = matchWords,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(timeLimitMinutes),
            IsActive = true
        };

        _cache.Set(GetMatchKey(matchId), match, MatchExpiration);
        return matchId;
    }

    public Task<MultiplayerRoundDto> StartMatchAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        if (!_cache.TryGetValue(GetMatchKey(matchId), out MultiplayerMatch? match) || match == null)
        {
            throw new InvalidOperationException("Match not found");
        }

        match.StartedAt = DateTime.UtcNow;
        match.CurrentRound = 1;

        var seq = ++match.SequenceNumber;
        var word = match.Words[0];
        return Task.FromResult(new MultiplayerRoundDto(
            RoundNumber: 1,
            ScrambledWord: word.Scrambled,
            WordLength: word.Original.Length,
            TimeLimit: match.TimeLimitMinutes * 60,
            SequenceNumber: seq
        ));
    }

    public Task<(bool IsCorrect, int Score, bool IsMatchComplete)> SubmitAnswerAsync(Guid matchId, Guid playerId, string answer, int timeSpentMs, CancellationToken cancellationToken = default)
    {
        if (!_cache.TryGetValue(GetMatchKey(matchId), out MultiplayerMatch? match) || match == null)
        {
            throw new InvalidOperationException("Match not found");
        }

        var currentWord = match.Words[match.CurrentRound - 1];
        var isCorrect = currentWord.Original.Equals(answer, StringComparison.OrdinalIgnoreCase);

        // Track player progress
        var playerProgress = match.GetOrCreatePlayerProgress(playerId);
        playerProgress.TotalAnswered++;
        playerProgress.TotalTime += TimeSpan.FromMilliseconds(timeSpentMs);

        if (isCorrect)
        {
            playerProgress.CorrectCount++;
            // Calculate score based on speed
            var baseScore = 10;
            var timeBonus = timeSpentMs < 5000 ? 5 : timeSpentMs < 10000 ? 3 : 0;
            playerProgress.Score += baseScore + timeBonus;
            playerProgress.CurrentCombo++;
            playerProgress.MaxCombo = Math.Max(playerProgress.MaxCombo, playerProgress.CurrentCombo);
        }
        else
        {
            playerProgress.CurrentCombo = 0;
        }

        // Advance to next round
        match.CurrentRound++;
        var isMatchComplete = match.CurrentRound > match.TotalRounds || 
                              match.Player1Progress.CorrectCount >= match.TotalRounds ||
                              match.Player2Progress.CorrectCount >= match.TotalRounds;

        if (isMatchComplete)
        {
            match.IsActive = false;
        }

        return Task.FromResult((isCorrect, playerProgress.Score, isMatchComplete));
    }

    public Task ForfeitAsync(Guid matchId, Guid playerId, CancellationToken cancellationToken = default)
    {
        if (!_cache.TryGetValue(GetMatchKey(matchId), out MultiplayerMatch? match) || match == null)
        {
            throw new InvalidOperationException("Match not found");
        }

        match.IsActive = false;
        match.ForfeitedBy = playerId;
        return Task.CompletedTask;
    }

    public Task<MatchStateDto?> GetMatchStateAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        if (!_cache.TryGetValue(GetMatchKey(matchId), out MultiplayerMatch? match) || match == null)
        {
            return Task.FromResult<MatchStateDto?>(null);
        }

        var timeRemaining = match.ExpiresAt - DateTime.UtcNow;
        if (timeRemaining < TimeSpan.Zero)
        {
            timeRemaining = TimeSpan.Zero;
            match.IsActive = false;
        }

        return Task.FromResult<MatchStateDto?>(new MatchStateDto(
            MatchId: match.Id,
            Player1Id: match.Player1Id,
            Player2Id: match.Player2Id,
            CurrentRound: match.CurrentRound,
            TotalRounds: match.TotalRounds,
            Player1Score: match.Player1Progress?.Score ?? 0,
            Player2Score: match.Player2Progress?.Score ?? 0,
            TimeRemaining: timeRemaining,
            IsActive: match.IsActive,
            StartedAt: match.StartedAt ?? match.CreatedAt
        ));
    }

    public Task<MatchResultDto> EndMatchAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        if (!_cache.TryGetValue(GetMatchKey(matchId), out MultiplayerMatch? match) || match == null)
        {
            throw new InvalidOperationException("Match not found");
        }

        match.IsActive = false;

        // Determine winner
        Guid? winnerId = null;
        var isDraw = false;

        if (match.ForfeitedBy.HasValue)
        {
            winnerId = match.ForfeitedBy == match.Player1Id ? match.Player2Id : match.Player1Id;
        }
        else
        {
            var p1Correct = match.Player1Progress?.CorrectCount ?? 0;
            var p2Correct = match.Player2Progress?.CorrectCount ?? 0;

            if (p1Correct > p2Correct)
            {
                winnerId = match.Player1Id;
            }
            else if (p2Correct > p1Correct)
            {
                winnerId = match.Player2Id;
            }
            else
            {
                // Tie - compare total time (faster wins)
                var p1Time = match.Player1Progress?.TotalTime ?? TimeSpan.MaxValue;
                var p2Time = match.Player2Progress?.TotalTime ?? TimeSpan.MaxValue;

                if (p1Time < p2Time)
                {
                    winnerId = match.Player1Id;
                }
                else if (p2Time < p1Time)
                {
                    winnerId = match.Player2Id;
                }
                else
                {
                    isDraw = true;
                }
            }
        }

        // Calculate XP rewards
        var isPrivateRoom = match.IsPrivateRoom;
        var player1Won = winnerId == match.Player1Id;
        var player2Won = winnerId == match.Player2Id;

        // Quick Match: winner 100 XP + league 50 XP, loser 30 XP + league 15 XP
        // Private Room: winner 100 XP (0 liga), loser 30 XP (0 liga)
        var yourXPEarned = player1Won ? 100 : 30;
        var leagueXPEarned = isPrivateRoom ? 0 : (player1Won ? 50 : 15);

        var result = new MatchResultDto(
            WinnerId: winnerId,
            YourScore: match.Player1Progress?.CorrectCount ?? 0,
            OpponentScore: match.Player2Progress?.CorrectCount ?? 0,
            YourTime: match.Player1Progress?.TotalTime ?? TimeSpan.Zero,
            OpponentTime: match.Player2Progress?.TotalTime ?? TimeSpan.Zero,
            XPEarned: yourXPEarned,
            LeagueXPEarned: leagueXPEarned,
            IsDraw: isDraw,
            IsPrivateRoom: isPrivateRoom,
            RoomCode: null,
            YourResult: new PlayerMatchResult(
                Username: "Player1",
                Avatar: null,
                CorrectCount: match.Player1Progress?.CorrectCount ?? 0,
                TotalTime: match.Player1Progress?.TotalTime ?? TimeSpan.Zero,
                ComboMax: match.Player1Progress?.MaxCombo ?? 0,
                XPEarned: yourXPEarned
            ),
            OpponentResult: new PlayerMatchResult(
                Username: "Player2",
                Avatar: null,
                CorrectCount: match.Player2Progress?.CorrectCount ?? 0,
                TotalTime: match.Player2Progress?.TotalTime ?? TimeSpan.Zero,
                ComboMax: match.Player2Progress?.MaxCombo ?? 0,
                XPEarned: player2Won ? 100 : 30
            )
        );

        return Task.FromResult(result);
    }

    public Task<OpponentProgressDto> GetOpponentProgressAsync(Guid matchId, Guid playerId, CancellationToken cancellationToken = default)
    {
        if (!_cache.TryGetValue(GetMatchKey(matchId), out MultiplayerMatch? match) || match == null)
        {
            throw new InvalidOperationException("Match not found");
        }

        var opponentId = playerId == match.Player1Id ? match.Player2Id : match.Player1Id;
        var opponentProgress = match.GetOrCreatePlayerProgress(opponentId);

        var seq = ++match.SequenceNumber;
        return Task.FromResult(new OpponentProgressDto(
            CorrectCount: opponentProgress.CorrectCount,
            TotalAnswered: opponentProgress.TotalAnswered,
            ComboCount: opponentProgress.CurrentCombo,
            SequenceNumber: seq
        ));
    }

    public Task<bool> IsMatchActiveAsync(Guid matchId, CancellationToken cancellationToken = default)
    {
        if (!_cache.TryGetValue(GetMatchKey(matchId), out MultiplayerMatch? match) || match == null)
        {
            return Task.FromResult(false);
        }

        // Check if match expired
        if (DateTime.UtcNow > match.ExpiresAt)
        {
            match.IsActive = false;
            return Task.FromResult(false);
        }

        return Task.FromResult(match.IsActive);
    }

    public Task HandleDisconnectAsync(Guid matchId, Guid playerId, CancellationToken cancellationToken = default)
    {
        if (!_cache.TryGetValue(GetMatchKey(matchId), out MultiplayerMatch? match) || match == null)
        {
            return Task.CompletedTask;
        }

        // Record disconnect time - 30s grace period before forfeit
        match.DisconnectedPlayerId = playerId;
        match.DisconnectedAt = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    public Task FinalizeDisconnectAsync(Guid matchId, Guid playerId, CancellationToken cancellationToken = default)
    {
        if (!_cache.TryGetValue(GetMatchKey(matchId), out MultiplayerMatch? match) || match == null)
        {
            return Task.CompletedTask;
        }

        // Only forfeit if the player is still disconnected
        if (match.DisconnectedPlayerId == playerId && match.IsActive)
        {
            match.ForfeitedBy = playerId;
            match.IsActive = false;
        }

        return Task.CompletedTask;
    }

    public Task<bool> HandleReconnectAsync(Guid matchId, Guid playerId, CancellationToken cancellationToken = default)
    {
        if (!_cache.TryGetValue(GetMatchKey(matchId), out MultiplayerMatch? match) || match == null)
        {
            return Task.FromResult(false);
        }

        if (!match.IsActive)
        {
            return Task.FromResult(false);
        }

        // Check if match hasn't expired
        if (DateTime.UtcNow > match.ExpiresAt)
        {
            match.IsActive = false;
            return Task.FromResult(false);
        }

        // Clear disconnect state on reconnect
        if (match.DisconnectedPlayerId == playerId)
        {
            match.DisconnectedPlayerId = null;
            match.DisconnectedAt = null;
        }

        return Task.FromResult(true);
    }

    private class MultiplayerMatch
    {
        public Guid Id { get; set; }
        public Guid Player1Id { get; set; }
        public Guid Player2Id { get; set; }
        public bool IsPrivateRoom { get; set; }
        public int TotalRounds { get; set; }
        public int TimeLimitMinutes { get; set; }
        public List<MatchWord> Words { get; set; } = new();
        public int CurrentRound { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public Guid? ForfeitedBy { get; set; }
        public Guid? DisconnectedPlayerId { get; set; }
        public DateTime? DisconnectedAt { get; set; }
        public int SequenceNumber { get; set; }

        public PlayerProgress Player1Progress { get; set; } = new();
        public PlayerProgress Player2Progress { get; set; } = new();

        public PlayerProgress GetOrCreatePlayerProgress(Guid playerId)
        {
            if (playerId == Player1Id) return Player1Progress;
            if (playerId == Player2Id) return Player2Progress;
            
            // Fallback - shouldn't happen in normal flow
            return new PlayerProgress();
        }
    }

    private class MatchWord
    {
        public Guid WordId { get; set; }
        public string Original { get; set; } = string.Empty;
        public string Scrambled { get; set; } = string.Empty;
        public DifficultyLevel Difficulty { get; set; }
    }

    private class PlayerProgress
    {
        public int CorrectCount { get; set; }
        public int TotalAnswered { get; set; }
        public int Score { get; set; }
        public int CurrentCombo { get; set; }
        public int MaxCombo { get; set; }
        public TimeSpan TotalTime { get; set; }
    }
}
