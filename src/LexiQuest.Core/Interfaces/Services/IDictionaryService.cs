using LexiQuest.Shared.DTOs.Dictionaries;

namespace LexiQuest.Core.Interfaces.Services;

public interface IDictionaryService
{
    Task<DictionaryDto> CreateDictionaryAsync(Guid userId, CreateDictionaryRequest request);
    Task<IReadOnlyList<DictionaryDto>> GetUserDictionariesAsync(Guid userId);
    Task<DictionaryDto?> GetDictionaryByIdAsync(Guid id, Guid userId);
    Task<bool> DeleteDictionaryAsync(Guid id, Guid userId);
    Task<DictionaryWordDto> AddWordAsync(Guid dictionaryId, Guid userId, AddWordRequest request);
    Task<ImportResultDto> ImportWordsFromCsvAsync(Guid dictionaryId, Guid userId, string csvContent);
    Task<ImportResultDto> ImportWordsFromTxtAsync(Guid dictionaryId, Guid userId, string txtContent);
    Task<ImportResultDto> ImportWordsFromJsonAsync(Guid dictionaryId, Guid userId, string jsonContent);
    Task<IReadOnlyList<DictionaryDto>> GetPublicDictionariesAsync();
}
