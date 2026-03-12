using LexiQuest.Core.Domain.Entities;

namespace LexiQuest.Core.Interfaces.Repositories;

public interface IDailyChallengeRepository
{
    Task<DailyChallenge?> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);
    Task AddAsync(DailyChallenge challenge, CancellationToken cancellationToken = default);
    Task<bool> HasUserCompletedAsync(Guid userId, DateTime date, CancellationToken cancellationToken = default);
    Task<List<DailyLeaderboardEntry>> GetLeaderboardAsync(DateTime date, CancellationToken cancellationToken = default);
    Task RecordCompletionAsync(DailyChallengeCompletion completion, CancellationToken cancellationToken = default);
}
