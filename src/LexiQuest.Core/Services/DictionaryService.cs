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
    private const int MaxDictionariesPerUser = 10;
    private const int MaxWordsPerDictionary = 100;
    private const string PremiumRequiredMessage = "Tato funkce vyžaduje Premium účet";
    private const string DictionaryLimitMessage = "Můžete mít maximálně 10 slovníků.";
    private const string WordLimitMessage = "Slovník může obsahovat maximálně 100 slov.";
    private const string DuplicateWordMessage = "Slovo už ve slovníku existuje.";

    private readonly ICustomDictionaryRepository _dictionaryRepo;
    private readonly IDictionaryWordRepository _wordRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository? _userRepository;

    public DictionaryService(
        ICustomDictionaryRepository dictionaryRepo,
        IDictionaryWordRepository wordRepo,
        IUnitOfWork unitOfWork,
        IUserRepository? userRepository = null)
    {
        _dictionaryRepo = dictionaryRepo;
        _wordRepo = wordRepo;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
    }

    public async Task<DictionaryDto> CreateDictionaryAsync(Guid userId, CreateDictionaryRequest request)
    {
        await EnsurePremiumAsync(userId);

        var existingDictionaries = await _dictionaryRepo.GetByUserIdAsync(userId);
        if (existingDictionaries.Count >= MaxDictionariesPerUser)
        {
            throw new InvalidOperationException(DictionaryLimitMessage);
        }

        var dictionary = CustomDictionary.Create(userId, request.Name, request.Description);
        dictionary.SetPublicStatus(request.IsPublic);
        await _dictionaryRepo.AddAsync(dictionary);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(dictionary);
    }

    public async Task<IReadOnlyList<DictionaryDto>> GetUserDictionariesAsync(Guid userId)
    {
        var dictionaries = await _dictionaryRepo.GetByUserIdAsync(userId);
        return dictionaries.Select(dictionary => MapToDto(dictionary)).ToList();
    }

    public async Task<DictionaryDto?> GetDictionaryByIdAsync(Guid id, Guid userId)
    {
        var dictionary = await _dictionaryRepo.GetByIdAsync(id);
        if (dictionary == null || !dictionary.CanBeAccessedBy(userId))
            return null;

        var words = await _wordRepo.GetByDictionaryIdAsync(id);
        return MapToDto(dictionary, words);
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

        await EnsureCanAddWordAsync(dictionaryId, request.Word);

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
        await EnsureCanImportAsync(dictionaryId, lines.Length);

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
        await EnsureCanImportAsync(dictionaryId, lines.Length);

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
                await EnsureCanImportAsync(dictionaryId, document.RootElement.GetArrayLength());

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
        return dictionaries.Select(dictionary => MapToDto(dictionary)).ToList();
    }

    private async Task EnsurePremiumAsync(Guid userId)
    {
        if (_userRepository == null)
        {
            return;
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user?.Premium.IsActive(DateTime.UtcNow) != true)
        {
            throw new UnauthorizedAccessException(PremiumRequiredMessage);
        }
    }

    private async Task EnsureCanAddWordAsync(Guid dictionaryId, string word)
    {
        if (await _wordRepo.CountByDictionaryIdAsync(dictionaryId) >= MaxWordsPerDictionary)
        {
            throw new InvalidOperationException(WordLimitMessage);
        }

        if (await _wordRepo.ExistsInDictionaryAsync(dictionaryId, word))
        {
            throw new InvalidOperationException(DuplicateWordMessage);
        }
    }

    private async Task EnsureCanImportAsync(Guid dictionaryId, int requestedWordCount)
    {
        if (requestedWordCount <= 0)
        {
            return;
        }

        var currentCount = await _wordRepo.CountByDictionaryIdAsync(dictionaryId);
        if (currentCount + requestedWordCount > MaxWordsPerDictionary)
        {
            throw new InvalidOperationException(WordLimitMessage);
        }
    }

    public static bool IsDuplicateWordError(InvalidOperationException exception) =>
        exception.Message == DuplicateWordMessage;

    private static DictionaryDto MapToDto(CustomDictionary dictionary, IReadOnlyList<DictionaryWord>? words = null)
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
            UpdatedAt = dictionary.UpdatedAt,
            Words = words?.Select(MapToWordDto).ToList() ?? new List<DictionaryWordDto>()
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
