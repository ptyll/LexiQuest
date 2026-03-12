using System.Text.Json;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Dictionaries;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Services;

public class DictionaryService : IDictionaryService
{
    private readonly ICustomDictionaryRepository _dictionaryRepo;
    private readonly IDictionaryWordRepository _wordRepo;
    private readonly IUnitOfWork _unitOfWork;

    public DictionaryService(
        ICustomDictionaryRepository dictionaryRepo,
        IDictionaryWordRepository wordRepo,
        IUnitOfWork unitOfWork)
    {
        _dictionaryRepo = dictionaryRepo;
        _wordRepo = wordRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<DictionaryDto> CreateDictionaryAsync(Guid userId, CreateDictionaryRequest request)
    {
        var dictionary = CustomDictionary.Create(userId, request.Name, request.Description);
        await _dictionaryRepo.AddAsync(dictionary);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(dictionary);
    }

    public async Task<IReadOnlyList<DictionaryDto>> GetUserDictionariesAsync(Guid userId)
    {
        var dictionaries = await _dictionaryRepo.GetByUserIdAsync(userId);
        return dictionaries.Select(MapToDto).ToList();
    }

    public async Task<DictionaryDto?> GetDictionaryByIdAsync(Guid id, Guid userId)
    {
        var dictionary = await _dictionaryRepo.GetByIdAsync(id);
        if (dictionary == null || !dictionary.CanBeAccessedBy(userId))
            return null;

        return MapToDto(dictionary);
    }

    public async Task<bool> DeleteDictionaryAsync(Guid id, Guid userId)
    {
        var dictionary = await _dictionaryRepo.GetByIdAsync(id);
        if (dictionary == null || !dictionary.CanBeModifiedBy(userId))
            return false;

        _dictionaryRepo.Delete(dictionary);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<DictionaryWordDto> AddWordAsync(Guid dictionaryId, Guid userId, AddWordRequest request)
    {
        var dictionary = await _dictionaryRepo.GetByIdAsync(dictionaryId);
        if (dictionary == null || !dictionary.CanBeModifiedBy(userId))
            throw new UnauthorizedAccessException("User cannot modify this dictionary");

        var word = DictionaryWord.Create(dictionaryId, request.Word, request.Difficulty);
        await _wordRepo.AddAsync(word);

        dictionary.AddWord();
        _dictionaryRepo.Update(dictionary);

        await _unitOfWork.SaveChangesAsync();

        return MapToWordDto(word);
    }

    public async Task<ImportResultDto> ImportWordsFromCsvAsync(Guid dictionaryId, Guid userId, string csvContent)
    {
        var dictionary = await _dictionaryRepo.GetByIdAsync(dictionaryId);
        if (dictionary == null || !dictionary.CanBeModifiedBy(userId))
            throw new UnauthorizedAccessException("User cannot modify this dictionary");

        var result = new ImportResultDto();
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var parts = line.Split(',');
            if (parts.Length < 1)
            {
                result.Errors.Add($"Invalid line: {line}");
                continue;
            }

            var wordText = parts[0].Trim();
            var difficulty = parts.Length > 1 && Enum.TryParse<DifficultyLevel>(parts[1].Trim(), true, out var d)
                ? d
                : DictionaryWord.AutoDetectDifficulty(wordText);

            if (await _wordRepo.ExistsInDictionaryAsync(dictionaryId, wordText))
            {
                result.SkippedCount++;
                continue;
            }

            try
            {
                var word = DictionaryWord.Create(dictionaryId, wordText, difficulty);
                await _wordRepo.AddAsync(word);
                result.ImportedCount++;
            }
            catch (ArgumentException ex)
            {
                result.Errors.Add($"{wordText}: {ex.Message}");
            }
        }

        if (result.ImportedCount > 0)
        {
            for (int i = 0; i < result.ImportedCount; i++)
                dictionary.AddWord();
            _dictionaryRepo.Update(dictionary);
            await _unitOfWork.SaveChangesAsync();
        }

        return result;
    }

    public async Task<ImportResultDto> ImportWordsFromTxtAsync(Guid dictionaryId, Guid userId, string txtContent)
    {
        var dictionary = await _dictionaryRepo.GetByIdAsync(dictionaryId);
        if (dictionary == null || !dictionary.CanBeModifiedBy(userId))
            throw new UnauthorizedAccessException("User cannot modify this dictionary");

        var result = new ImportResultDto();
        var lines = txtContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var wordText = line.Trim();
            if (string.IsNullOrEmpty(wordText))
                continue;

            if (await _wordRepo.ExistsInDictionaryAsync(dictionaryId, wordText))
            {
                result.SkippedCount++;
                continue;
            }

            var difficulty = DictionaryWord.AutoDetectDifficulty(wordText);

            try
            {
                var word = DictionaryWord.Create(dictionaryId, wordText, difficulty);
                await _wordRepo.AddAsync(word);
                result.ImportedCount++;
            }
            catch (ArgumentException ex)
            {
                result.Errors.Add($"{wordText}: {ex.Message}");
            }
        }

        if (result.ImportedCount > 0)
        {
            for (int i = 0; i < result.ImportedCount; i++)
                dictionary.AddWord();
            _dictionaryRepo.Update(dictionary);
            await _unitOfWork.SaveChangesAsync();
        }

        return result;
    }

    public async Task<ImportResultDto> ImportWordsFromJsonAsync(Guid dictionaryId, Guid userId, string jsonContent)
    {
        var dictionary = await _dictionaryRepo.GetByIdAsync(dictionaryId);
        if (dictionary == null || !dictionary.CanBeModifiedBy(userId))
            throw new UnauthorizedAccessException("User cannot modify this dictionary");

        var result = new ImportResultDto();

        try
        {
            using var document = JsonDocument.Parse(jsonContent);
            
            if (document.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in document.RootElement.EnumerateArray())
                {
                    await ProcessJsonElementAsync(element, dictionaryId, result);
                }
            }
            else
            {
                result.Errors.Add("JSON musí být pole (array)");
            }
        }
        catch (JsonException ex)
        {
            result.Errors.Add($"Chyba při parsování JSON: {ex.Message}");
        }

        if (result.ImportedCount > 0)
        {
            for (int i = 0; i < result.ImportedCount; i++)
                dictionary.AddWord();
            _dictionaryRepo.Update(dictionary);
            await _unitOfWork.SaveChangesAsync();
        }

        return result;
    }

    private async Task ProcessJsonElementAsync(JsonElement element, Guid dictionaryId, ImportResultDto result)
    {
        string? wordText = null;
        DifficultyLevel? difficulty = null;

        if (element.ValueKind == JsonValueKind.String)
        {
            wordText = element.GetString();
        }
        else if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty("word", out var wordProperty))
            {
                wordText = wordProperty.GetString();
            }

            if (element.TryGetProperty("difficulty", out var difficultyProperty))
            {
                var diffString = difficultyProperty.GetString();
                if (!string.IsNullOrEmpty(diffString) && 
                    Enum.TryParse<DifficultyLevel>(diffString, true, out var parsedDifficulty))
                {
                    difficulty = parsedDifficulty;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(wordText))
        {
            result.Errors.Add("Přeskočeno: prázdné slovo");
            return;
        }

        wordText = wordText.Trim();

        if (await _wordRepo.ExistsInDictionaryAsync(dictionaryId, wordText))
        {
            result.SkippedCount++;
            return;
        }

        var finalDifficulty = difficulty ?? DictionaryWord.AutoDetectDifficulty(wordText);

        try
        {
            var word = DictionaryWord.Create(dictionaryId, wordText, finalDifficulty);
            await _wordRepo.AddAsync(word);
            result.ImportedCount++;
        }
        catch (ArgumentException ex)
        {
            result.Errors.Add($"{wordText}: {ex.Message}");
        }
    }

    public async Task<IReadOnlyList<DictionaryDto>> GetPublicDictionariesAsync()
    {
        var dictionaries = await _dictionaryRepo.GetPublicDictionariesAsync();
        return dictionaries.Select(MapToDto).ToList();
    }

    private static DictionaryDto MapToDto(CustomDictionary dictionary)
    {
        return new DictionaryDto
        {
            Id = dictionary.Id,
            UserId = dictionary.UserId,
            Name = dictionary.Name,
            Description = dictionary.Description,
            IsPublic = dictionary.IsPublic,
            WordCount = dictionary.WordCount,
            CreatedAt = dictionary.CreatedAt,
            UpdatedAt = dictionary.UpdatedAt
        };
    }

    private static DictionaryWordDto MapToWordDto(DictionaryWord word)
    {
        return new DictionaryWordDto
        {
            Id = word.Id,
            DictionaryId = word.DictionaryId,
            Word = word.Word,
            Difficulty = word.Difficulty
        };
    }
}
