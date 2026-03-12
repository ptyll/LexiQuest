using LexiQuest.Shared.DTOs.Leagues;

namespace LexiQuest.Blazor.Services;

public interface ILeagueService
{
    Task<LeagueInfoDto?> GetCurrentLeagueAsync();
    Task<List<LeagueParticipantDto>> GetLeaderboardAsync();
    Task<List<LeagueHistoryDto>> GetLeagueHistoryAsync();
    Task<List<LeagueRewardsDto>> GetRewardsAsync();
}
