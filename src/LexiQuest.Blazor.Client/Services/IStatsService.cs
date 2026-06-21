using LexiQuest.Shared.DTOs.Stats;

namespace LexiQuest.Blazor.Services;

public interface IStatsService
{
    Task<UserStatsSummaryDto> GetUserStatsAsync();
}
