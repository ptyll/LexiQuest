using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Multiplayer;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Services;

/// <summary>
/// Service for managing match history.
/// </summary>
public class MatchHistoryService : IMatchHistoryService
{
    private readonly IMatchResultRepository _matchResultRepository;

    public MatchHistoryService(IMatchResultRepository matchResultRepository)
    {
        _matchResultRepository = matchResultRepository;
    }

    public async Task SaveMatchResultAsync(
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
        DateTime startedAt,
        CancellationToken cancellationToken = default)
    {
        var matchResult = MatchResult.Create(
            matchId,
            player1Id,
            player2Id,
            player1Username,
            player2Username,
            player1Score,
            player2Score,
            player1Time,
            player2Time,
            player1MaxCombo,
            player2MaxCombo,
            winnerId,
            isDraw,
            player1XpEarned,
            player2XpEarned,
            player1LeagueXpEarned,
            player2LeagueXpEarned,
            isPrivateRoom,
            roomCode,
            seriesPlayer1Wins,
            seriesPlayer2Wins,
            wordCount,
            timeLimitMinutes,
            difficulty,
            startedAt);

        await _matchResultRepository.AddAsync(matchResult, cancellationToken);
    }

    public async Task<MatchHistoryResponseDto> GetMatchHistoryAsync(
        Guid playerId,
        MatchHistoryFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var matches = await _matchResultRepository.GetByPlayerIdAsync(
            playerId, filter, pageNumber, pageSize, cancellationToken);

        var totalCount = await _matchResultRepository.GetTotalCountByPlayerIdAsync(
            playerId, filter, cancellationToken);

        var entries = matches.Select(m => MapToHistoryEntry(m, playerId)).ToList();

        return new MatchHistoryResponseDto(
            Entries: entries,
            TotalCount: totalCount,
            PageNumber: pageNumber,
            PageSize: pageSize
        );
    }

    public async Task<MultiplayerStatsDto> GetStatsAsync(
        Guid playerId,
        CancellationToken cancellationToken = default)
    {
        var stats = await _matchResultRepository.GetStatsForPlayerAsync(playerId, cancellationToken);

        return new MultiplayerStatsDto(
            TotalMatchesPlayed: stats.TotalMatchesPlayed,
            Wins: stats.Wins,
            Losses: stats.Losses,
            Draws: stats.Draws,
            WinRatePercentage: stats.WinRatePercentage,
            TotalXPEarned: stats.TotalXPEarned,
            QuickMatchStats: new Shared.DTOs.Multiplayer.MatchTypeStats(
                MatchesPlayed: stats.QuickMatchStats.MatchesPlayed,
                Wins: stats.QuickMatchStats.Wins,
                Losses: stats.QuickMatchStats.Losses,
                Draws: stats.QuickMatchStats.Draws,
                WinRatePercentage: stats.QuickMatchStats.WinRatePercentage
            ),
            PrivateRoomStats: new Shared.DTOs.Multiplayer.MatchTypeStats(
                MatchesPlayed: stats.PrivateRoomStats.MatchesPlayed,
                Wins: stats.PrivateRoomStats.Wins,
                Losses: stats.PrivateRoomStats.Losses,
                Draws: stats.PrivateRoomStats.Draws,
                WinRatePercentage: stats.PrivateRoomStats.WinRatePercentage
            )
        );
    }

    public async Task<MatchResultDto?> GetMatchResultAsync(
        Guid matchId,
        Guid playerId,
        CancellationToken cancellationToken = default)
    {
        var match = await _matchResultRepository.GetByMatchIdAsync(matchId, cancellationToken);
        if (match == null || (match.Player1Id != playerId && match.Player2Id != playerId))
        {
            return null;
        }

        return MapToResultDto(match, playerId);
    }

    public async Task<MatchResult?> GetMatchByIdAsync(
        Guid matchId,
        CancellationToken cancellationToken = default)
    {
        return await _matchResultRepository.GetByMatchIdAsync(matchId, cancellationToken);
    }

    private static MatchHistoryEntryDto MapToHistoryEntry(MatchResult match, Guid playerId)
    {
        var seriesScore = match.GetSeriesScore(playerId);

        return new MatchHistoryEntryDto(
            MatchId: match.MatchId,
            OpponentUsername: match.GetOpponentUsername(playerId),
            OpponentAvatar: match.GetOpponentAvatar(playerId),
            YourScore: match.GetPlayerScore(playerId),
            OpponentScore: match.GetOpponentScore(playerId),
            Result: match.GetResultForPlayer(playerId),
            XPEarned: match.GetPlayerXPEarned(playerId),
            Duration: match.Duration,
            PlayedAt: match.CompletedAt,
            Type: match.IsPrivateRoom ? Shared.DTOs.Multiplayer.MatchType.PrivateRoom : Shared.DTOs.Multiplayer.MatchType.QuickMatch,
            RoomCode: match.RoomCode,
            SeriesScoreYou: seriesScore?.YourWins,
            SeriesScoreOpponent: seriesScore?.OpponentWins
        );
    }

    private static MatchResultDto MapToResultDto(MatchResult match, Guid playerId)
    {
        var isPlayer1 = playerId == match.Player1Id;

        var yourUsername = isPlayer1 ? match.Player1Username : match.Player2Username;
        var opponentUsername = isPlayer1 ? match.Player2Username : match.Player1Username;
        var yourAvatar = isPlayer1 ? match.Player1Avatar : match.Player2Avatar;
        var opponentAvatar = isPlayer1 ? match.Player2Avatar : match.Player1Avatar;
        var yourScore = isPlayer1 ? match.Player1Score : match.Player2Score;
        var opponentScore = isPlayer1 ? match.Player2Score : match.Player1Score;
        var yourTime = isPlayer1 ? match.Player1Time : match.Player2Time;
        var opponentTime = isPlayer1 ? match.Player2Time : match.Player1Time;
        var yourCombo = isPlayer1 ? match.Player1MaxCombo : match.Player2MaxCombo;
        var opponentCombo = isPlayer1 ? match.Player2MaxCombo : match.Player1MaxCombo;
        var yourXp = isPlayer1 ? match.Player1XPEarned : match.Player2XPEarned;
        var opponentXp = isPlayer1 ? match.Player2XPEarned : match.Player1XPEarned;
        var yourLeagueXp = isPlayer1 ? match.Player1LeagueXPEarned : match.Player2LeagueXPEarned;

        return new MatchResultDto(
            WinnerId: match.WinnerId,
            YourScore: yourScore,
            OpponentScore: opponentScore,
            YourTime: yourTime,
            OpponentTime: opponentTime,
            XPEarned: yourXp,
            LeagueXPEarned: yourLeagueXp,
            IsDraw: match.IsDraw,
            IsPrivateRoom: match.IsPrivateRoom,
            RoomCode: match.RoomCode,
            YourResult: new PlayerMatchResult(
                Username: yourUsername,
                Avatar: yourAvatar,
                CorrectCount: yourScore,
                TotalTime: yourTime,
                ComboMax: yourCombo,
                XPEarned: yourXp
            ),
            OpponentResult: new PlayerMatchResult(
                Username: opponentUsername,
                Avatar: opponentAvatar,
                CorrectCount: opponentScore,
                TotalTime: opponentTime,
                ComboMax: opponentCombo,
                XPEarned: opponentXp
            ),
            SeriesPlayer1Wins: match.SeriesPlayer1Wins,
            SeriesPlayer2Wins: match.SeriesPlayer2Wins,
            BestOf: match.SeriesPlayer1Wins.HasValue && match.SeriesPlayer2Wins.HasValue ? 3 : 1
        );
    }
}
