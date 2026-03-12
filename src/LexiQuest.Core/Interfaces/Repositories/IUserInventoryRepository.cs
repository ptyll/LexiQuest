using LexiQuest.Core.Domain.Entities;

namespace LexiQuest.Core.Interfaces.Repositories;

public interface IUserInventoryRepository
{
    Task<UserInventoryItem?> GetByIdAsync(Guid id);
    Task<UserInventoryItem?> GetByUserAndItemAsync(Guid userId, Guid shopItemId);
    Task<IEnumerable<UserInventoryItem>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<UserInventoryItem>> GetEquippedByUserIdAsync(Guid userId);
    Task<bool> HasItemAsync(Guid userId, Guid shopItemId);
    Task AddAsync(UserInventoryItem item);
    void Update(UserInventoryItem item);
}
