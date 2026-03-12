using LexiQuest.Core.Domain.Entities;

namespace LexiQuest.Core.Interfaces.Repositories;

public interface IStreakProtectionRepository
{
    Task<StreakProtection?> GetByUserIdAsync(Guid userId);
    Task AddAsync(StreakProtection protection);
    void Update(StreakProtection protection);
}
