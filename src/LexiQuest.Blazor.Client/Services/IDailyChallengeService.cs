using LexiQuest.Shared.DTOs.Game;

namespace LexiQuest.Blazor.Services;

public interface IDailyChallengeService
{
    Task<DailyChallengeDto?> GetTodayAsync();
    Task<List<DailyLeaderboardEntryDto>> GetLeaderboardAsync();
    Task<bool> HasCompletedTodayAsync();
    Task<ChallengeResultDto?> SubmitAnswerAsync(string answer, TimeSpan timeTaken);
}
