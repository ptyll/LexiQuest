using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Interfaces.Repositories;

public interface ILeagueRepository
{
    Task<League?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<League?> GetActiveLeagueForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<League?> GetActiveLeagueForTierAsync(LeagueTier tier, CancellationToken cancellationToken = default);
    Task<List<League>> GetActiveLeaguesAsync(CancellationToken cancellationToken = default);
    Task<List<League>> GetLeaguesByWeekAsync(DateTime weekStart, CancellationToken cancellationToken = default);
    Task AddAsync(League league, CancellationToken cancellationToken = default);
    void Update(League league);
}
