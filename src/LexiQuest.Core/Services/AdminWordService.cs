using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Admin;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Services;

public class AdminWordService : IAdminWordService
{
    private readonly IWordRepository _wordRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdminWordService(IWordRepository wordRepository, IUnitOfWork unitOfWork)
    {
        _wordRepository = wordRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResult<AdminWordDto>> GetWordsAsync(AdminWordListRequest request, CancellationToken cancellationToken = default)
    {
        DifficultyLevel? difficulty = null;
        if (!string.IsNullOrEmpty(request.Difficulty) && Enum.TryParse<DifficultyLevel>(request.Difficulty, true, out var d))
            difficulty = d;

        WordCategory? category = null;
        if (!string.IsNullOrEmpty(request.Category) && Enum.TryParse<WordCategory>(request.Category, true, out var c))
            category = c;

        var (items, totalCount) = await _wordRepository.GetPaginatedAsync(
            request.Search, difficulty, category,
            request.MinLength, request.MaxLength,
            request.Page, request.PageSize,
            cancellationToken);

        var dtos = items.Select(w => new AdminWordDto(
            w.Id, w.Original, w.Difficulty.ToString(), w.Category.ToString(), w.Length, 0.0
        )).ToList();

        return new PaginatedResult<AdminWordDto>(dtos, totalCount, request.Page, request.PageSize);
    }

    public async Task<AdminWordDto> CreateWordAsync(AdminWordCreateRequest request, CancellationToken cancellationToken = default)
    {
        var difficulty = Enum.Parse<DifficultyLevel>(request.Difficulty, true);
        var category = !string.IsNullOrEmpty(request.Category)
            ? Enum.Parse<WordCategory>(request.Category, true)
            : WordCategory.Everyday;

        var word = Word.Create(request.Word, difficulty, category);
        await _wordRepository.AddAsync(word, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AdminWordDto(word.Id, word.Original, word.Difficulty.ToString(), word.Category.ToString(), word.Length, 0.0);
    }

    public async Task<AdminWordDto?> UpdateWordAsync(Guid id, AdminWordUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var word = await _wordRepository.GetByIdAsync(id, cancellationToken);
        if (word == null) return null;

        var difficulty = Enum.Parse<DifficultyLevel>(request.Difficulty, true);
        var category = !string.IsNullOrEmpty(request.Category)
            ? Enum.Parse<WordCategory>(request.Category, true)
            : WordCategory.Everyday;

        // Re-create word with updated values (Word entity is immutable-style)
        _wordRepository.Remove(word);
        var updated = Word.Create(request.Word, difficulty, category);
        await _wordRepository.AddAsync(updated, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AdminWordDto(updated.Id, updated.Original, updated.Difficulty.ToString(), updated.Category.ToString(), updated.Length, 0.0);
    }

    public async Task<bool> DeleteWordAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var word = await _wordRepository.GetByIdAsync(id, cancellationToken);
        if (word == null) return false;

        _wordRepository.Remove(word);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<BulkImportResult> BulkImportAsync(string csvContent, CancellationToken cancellationToken = default)
    {
        var imported = 0;
        var skipped = 0;
        var errors = 0;
        var errorDetails = new List<string>();
        var wordsToAdd = new List<Word>();

        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var parts = line.Trim().Split(',');
            if (parts.Length < 2)
            {
                errors++;
                errorDetails.Add($"Invalid line format: {line.Trim()}");
                continue;
            }

            var wordText = parts[0].Trim();
            var difficultyText = parts[1].Trim();
            var categoryText = parts.Length > 2 ? parts[2].Trim() : "Everyday";

            if (!Enum.TryParse<DifficultyLevel>(difficultyText, true, out var difficulty))
            {
                errors++;
                errorDetails.Add($"Invalid difficulty '{difficultyText}' for word '{wordText}'");
                continue;
            }

            if (!Enum.TryParse<WordCategory>(categoryText, true, out var category))
            {
                category = WordCategory.Everyday;
            }

            // Check for duplicates
            var existing = await _wordRepository.GetByNormalizedAsync(wordText.ToLowerInvariant(), cancellationToken);
            if (existing != null)
            {
                skipped++;
                continue;
            }

            wordsToAdd.Add(Word.Create(wordText, difficulty, category));
            imported++;
        }

        if (wordsToAdd.Count > 0)
        {
            await _wordRepository.AddRangeAsync(wordsToAdd, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new BulkImportResult(imported, skipped, errors, errorDetails);
    }

    public async Task<string> ExportAsync(CancellationToken cancellationToken = default)
    {
        var words = await _wordRepository.GetAllAsync(cancellationToken);
        var lines = words.Select(w => $"{w.Original},{w.Difficulty},{w.Category}");
        return string.Join("\n", lines);
    }

    public async Task<WordStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var distribution = new Dictionary<string, int>();
        var successRates = new Dictionary<string, double>();
        var totalWords = 0;

        foreach (var difficulty in Enum.GetValues<DifficultyLevel>())
        {
            var count = await _wordRepository.CountByDifficultyAsync(difficulty, cancellationToken);
            distribution[difficulty.ToString()] = count;
            successRates[difficulty.ToString()] = 0.0;
            totalWords += count;
        }

        return new WordStatsDto(distribution, successRates, totalWords);
    }
}
