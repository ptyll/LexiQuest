using LexiQuest.Shared.DTOs.Achievements;

namespace LexiQuest.Blazor.Services;

public interface IAchievementService
{
    Task<List<AchievementDto>> GetAchievementsAsync();
}
