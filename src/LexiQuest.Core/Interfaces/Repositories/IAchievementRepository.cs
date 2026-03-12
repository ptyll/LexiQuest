using LexiQuest.Core.Domain.Entities;

namespace LexiQuest.Core.Interfaces.Repositories;

public interface IAchievementRepository
{
    Task<Achievement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Achievement?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<List<Achievement>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Achievement>> GetByCategoryAsync(Shared.Enums.AchievementCategory category, CancellationToken cancellationToken = default);
}

public interface IUserAchievementRepository
{
    Task<UserAchievement?> GetByUserAndAchievementAsync(Guid userId, Guid achievementId, CancellationToken cancellationToken = default);
    Task<List<UserAchievement>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserAchievement userAchievement, CancellationToken cancellationToken = default);
    void Update(UserAchievement userAchievement);
}
