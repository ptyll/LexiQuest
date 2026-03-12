using LexiQuest.Core.Domain.Entities;

namespace LexiQuest.Core.Interfaces.Repositories;

public interface IDictionaryWordRepository
{
    Task<DictionaryWord?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<DictionaryWord>> GetByDictionaryIdAsync(Guid dictionaryId);
    Task<bool> ExistsInDictionaryAsync(Guid dictionaryId, string word);
    Task AddAsync(DictionaryWord word);
    void Delete(DictionaryWord word);
    Task<int> CountByDictionaryIdAsync(Guid dictionaryId);
}
