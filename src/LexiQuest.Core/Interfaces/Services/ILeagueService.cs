using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.DTOs.Leagues;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Interfaces.Services;

public interface ILeagueService
{
    Task AssignUserToLeagueAsync(Guid userId, DateTime weekStart, DateTime weekEnd, CancellationToken cancellationToken = default);
    Task<LeagueInfoDto?> GetCurrentLeagueAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddXPAsync(Guid userId, int xp, CancellationToken cancellationToken = default);
    Task<List<LeagueParticipantDto>> GetLeaderboardAsync(Guid userId, CancellationToken cancellationToken = default);
    Task CalculatePromotionsAndDemotionsAsync(League league, CancellationToken cancellationToken = default);
    int GetRewards(LeagueTier tier);
    Task<List<LeagueHistoryDto>> GetLeagueHistoryAsync(Guid userId, CancellationToken cancellationToken = default);
}
