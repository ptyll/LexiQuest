using LexiQuest.Shared.DTOs.Multiplayer;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Domain.Entities;

/// <summary>
/// Entity representing a completed multiplayer match result.
/// Stored in database for match history.
/// </summary>
public class MatchResult
{
    public Guid Id { get; private set; }
    public Guid MatchId { get; private set; }
    public Guid Player1Id { get; private set; }
    public Guid Player2Id { get; private set; }
    public string Player1Username { get; private set; } = string.Empty;
    public string Player2Username { get; private set; } = string.Empty;
    public string? Player1Avatar { get; private set; }
    public string? Player2Avatar { get; private set; }
    
    public int Player1Score { get; private set; }
    public int Player2Score { get; private set; }
    public TimeSpan Player1Time { get; private set; }
    public TimeSpan Player2Time { get; private set; }
    public int Player1MaxCombo { get; private set; }
    public int Player2MaxCombo { get; private set; }
    
    public Guid? WinnerId { get; private set; }
    public bool IsDraw { get; private set; }
    
    public int Player1XPEarned { get; private set; }
    public int Player2XPEarned { get; private set; }
    public int Player1LeagueXPEarned { get; private set; }
    public int Player2LeagueXPEarned { get; private set; }
    
    public bool IsPrivateRoom { get; private set; }
    public string? RoomCode { get; private set; }
    public int? SeriesPlayer1Wins { get; private set; }
    public int? SeriesPlayer2Wins { get; private set; }
    
    public int WordCount { get; private set; }
    public int TimeLimitMinutes { get; private set; }
    public DifficultyLevel Difficulty { get; private set; }
    
    public DateTime StartedAt { get; private set; }
    public DateTime CompletedAt { get; private set; }
    public TimeSpan Duration => CompletedAt - StartedAt;

    private MatchResult() { } // EF Core constructor

    public static MatchResult Create(
        Guid matchId,
        Guid player1Id,
        Guid player2Id,
        string player1Username,
        string player2Username,
        int player1Score,
        int player2Score,
        TimeSpan player1Time,
        TimeSpan player2Time,
        int player1MaxCombo,
        int player2MaxCombo,
        Guid? winnerId,
        bool isDraw,
        int player1XpEarned,
        int player2XpEarned,
        int player1LeagueXpEarned,
        int player2LeagueXpEarned,
        bool isPrivateRoom,
        string? roomCode,
        int? seriesPlayer1Wins,
        int? seriesPlayer2Wins,
        int wordCount,
        int timeLimitMinutes,
        DifficultyLevel difficulty,
        DateTime startedAt)
    {
        return new MatchResult
        {
            Id = Guid.NewGuid(),
            MatchId = matchId,
            Player1Id = player1Id,
            Player2Id = player2Id,
            Player1Username = player1Username,
            Player2Username = player2Username,
            Player1Score = player1Score,
            Player2Score = player2Score,
            Player1Time = player1Time,
            Player2Time = player2Time,
            Player1MaxCombo = player1MaxCombo,
            Player2MaxCombo = player2MaxCombo,
            WinnerId = winnerId,
            IsDraw = isDraw,
            Player1XPEarned = player1XpEarned,
            Player2XPEarned = player2XpEarned,
            Player1LeagueXPEarned = player1LeagueXpEarned,
            Player2LeagueXPEarned = player2LeagueXpEarned,
            IsPrivateRoom = isPrivateRoom,
            RoomCode = roomCode,
            SeriesPlayer1Wins = seriesPlayer1Wins,
            SeriesPlayer2Wins = seriesPlayer2Wins,
            WordCount = wordCount,
            TimeLimitMinutes = timeLimitMinutes,
            Difficulty = difficulty,
            StartedAt = startedAt,
            CompletedAt = DateTime.UtcNow
        };
    }

    public void SetPlayerAvatars(string? player1Avatar, string? player2Avatar)
    {
        Player1Avatar = player1Avatar;
        Player2Avatar = player2Avatar;
    }

    /// <summary>
    /// Gets the result from perspective of specified player.
    /// </summary>
    public MatchResultType GetResultForPlayer(Guid playerId)
    {
        if (IsDraw)
            return MatchResultType.Draw;
        
        return WinnerId == playerId ? MatchResultType.Win : MatchResultType.Loss;
    }

    /// <summary>
    /// Gets the opponent's user ID for specified player.
    /// </summary>
    public Guid GetOpponentId(Guid playerId)
    {
        return playerId == Player1Id ? Player2Id : Player1Id;
    }

    /// <summary>
    /// Gets the opponent's username for specified player.
    /// </summary>
    public string GetOpponentUsername(Guid playerId)
    {
        return playerId == Player1Id ? Player2Username : Player1Username;
    }

    /// <summary>
    /// Gets the opponent's avatar for specified player.
    /// </summary>
    public string? GetOpponentAvatar(Guid playerId)
    {
        return playerId == Player1Id ? Player2Avatar : Player1Avatar;
    }

    /// <summary>
    /// Gets the score for specified player.
    /// </summary>
    public int GetPlayerScore(Guid playerId)
    {
        return playerId == Player1Id ? Player1Score : Player2Score;
    }

    /// <summary>
    /// Gets the opponent's score for specified player.
    /// </summary>
    public int GetOpponentScore(Guid playerId)
    {
        return playerId == Player1Id ? Player2Score : Player1Score;
    }

    /// <summary>
    /// Gets the XP earned for specified player.
    /// </summary>
    public int GetPlayerXPEarned(Guid playerId)
    {
        return playerId == Player1Id ? Player1XPEarned : Player2XPEarned;
    }

    /// <summary>
    /// Gets the series score from perspective of specified player.
    /// </summary>
    public (int YourWins, int OpponentWins)? GetSeriesScore(Guid playerId)
    {
        if (!SeriesPlayer1Wins.HasValue || !SeriesPlayer2Wins.HasValue)
            return null;

        return playerId == Player1Id 
            ? (SeriesPlayer1Wins.Value, SeriesPlayer2Wins.Value)
            : (SeriesPlayer2Wins.Value, SeriesPlayer1Wins.Value);
    }
}
