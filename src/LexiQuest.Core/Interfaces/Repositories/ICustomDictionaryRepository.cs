using LexiQuest.Core.Domain.Entities;

namespace LexiQuest.Core.Interfaces.Repositories;

public interface ICustomDictionaryRepository
{
    Task<CustomDictionary?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<CustomDictionary>> GetByUserIdAsync(Guid userId);
    Task<IReadOnlyList<CustomDictionary>> GetPublicDictionariesAsync();
    Task AddAsync(CustomDictionary dictionary);
    void Update(CustomDictionary dictionary);
    void Delete(CustomDictionary dictionary);
    Task<int> GetWordCountAsync(Guid dictionaryId);
}
