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
}
