using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.DTOs.Multiplayer;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Interfaces.Services;

/// <summary>
/// Service for managing match history.
/// </summary>
public interface IMatchHistoryService
{
    Task SaveMatchResultAsync(
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
        CancellationToken cancellationToken = default);

    Task<MatchHistoryResponseDto> GetMatchHistoryAsync(
        Guid playerId,
        MatchHistoryFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<MultiplayerStatsDto> GetStatsAsync(
        Guid playerId,
        CancellationToken cancellationToken = default);

    Task<MatchResult?> GetMatchByIdAsync(
        Guid matchId,
        CancellationToken cancellationToken = default);
}
