using LexiQuest.Shared.DTOs.Dictionaries;

namespace LexiQuest.Blazor.Services;

public interface IDictionaryService
{
    Task<List<DictionaryDto>> GetMyDictionariesAsync();
    Task<List<DictionaryDto>> GetPublicDictionariesAsync();
    Task<DictionaryDto?> GetDictionaryAsync(Guid id);
    Task<DictionaryDto> CreateDictionaryAsync(CreateDictionaryRequest request);
    Task<bool> DeleteDictionaryAsync(Guid id);
    Task<DictionaryWordDto> AddWordAsync(Guid dictionaryId, AddWordRequest request);
    Task<ImportResultDto> ImportCsvAsync(Guid dictionaryId, string csvContent);
    Task<ImportResultDto> ImportTxtAsync(Guid dictionaryId, string txtContent);
}
