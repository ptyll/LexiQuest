using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.DTOs.Game;

namespace LexiQuest.Core.Interfaces.Services;

public interface IDailyChallengeService
{
    Task<DailyChallenge?> GetTodayAsync(CancellationToken cancellationToken = default);
    Task<DailyChallenge> GetOrCreateTodayAsync(CancellationToken cancellationToken = default);
    Task<ChallengeResultDto> SubmitAnswerAsync(Guid userId, DateTime date, string answer, TimeSpan timeTaken, CancellationToken cancellationToken = default);
    Task<List<DailyLeaderboardEntry>> GetLeaderboardAsync(DateTime date, CancellationToken cancellationToken = default);
}
