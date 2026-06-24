using System.Net.Http.Json;
using LexiQuest.Shared.DTOs.Dictionaries;

namespace LexiQuest.Blazor.Services;

public class DictionaryService : IDictionaryService
{
    private const string BasePath = "api/v1/dictionaries";

    private readonly IAuthenticatedApiClient _apiClient;

    public DictionaryService(IAuthenticatedApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<DictionaryDto>> GetMyDictionariesAsync()
    {
        try
        {
            var response = await _apiClient.GetAsync($"{BasePath}/my");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<DictionaryDto>>() ?? new List<DictionaryDto>();
            }
            return new List<DictionaryDto>();
        }
        catch
        {
            return new List<DictionaryDto>();
        }
    }

    public async Task<List<DictionaryDto>> GetPublicDictionariesAsync()
    {
        try
        {
            var response = await _apiClient.GetAsync($"{BasePath}/public");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<DictionaryDto>>() ?? new List<DictionaryDto>();
            }
            return new List<DictionaryDto>();
        }
        catch
        {
            return new List<DictionaryDto>();
        }
    }

    public async Task<DictionaryDto?> GetDictionaryAsync(Guid id)
    {
        try
        {
            var response = await _apiClient.GetAsync($"{BasePath}/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<DictionaryDto>();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<DictionaryDto> CreateDictionaryAsync(CreateDictionaryRequest request)
    {
        var response = await _apiClient.PostAsJsonAsync(BasePath, request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DictionaryDto>() 
            ?? throw new InvalidOperationException("Failed to create dictionary");
    }

    public async Task<bool> DeleteDictionaryAsync(Guid id)
    {
        try
        {
            var response = await _apiClient.DeleteAsync($"{BasePath}/{id}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<DictionaryWordDto> AddWordAsync(Guid dictionaryId, AddWordRequest request)
    {
        var response = await _apiClient.PostAsJsonAsync($"{BasePath}/{dictionaryId}/words", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DictionaryWordDto>()
            ?? throw new InvalidOperationException("Failed to add word");
    }

    public async Task<ImportResultDto> ImportCsvAsync(Guid dictionaryId, string csvContent)
    {
        var response = await _apiClient.PostAsJsonAsync(
            $"{BasePath}/{dictionaryId}/import-csv", 
            new { Content = csvContent });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ImportResultDto>()
            ?? new ImportResultDto { ImportedCount = 0, Errors = new List<string> { "Unknown error" } };
    }

    public async Task<ImportResultDto> ImportTxtAsync(Guid dictionaryId, string txtContent)
    {
        var response = await _apiClient.PostAsJsonAsync(
            $"{BasePath}/{dictionaryId}/import-txt", 
            new { Content = txtContent });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ImportResultDto>()
            ?? new ImportResultDto { ImportedCount = 0, Errors = new List<string> { "Unknown error" } };
    }

    public async Task<ImportResultDto> ImportJsonAsync(Guid dictionaryId, string jsonContent)
    {
        var response = await _apiClient.PostAsJsonAsync(
            $"{BasePath}/{dictionaryId}/import-json",
            new { Content = jsonContent });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ImportResultDto>()
            ?? new ImportResultDto { ImportedCount = 0, Errors = new List<string> { "Unknown error" } };
    }
}
