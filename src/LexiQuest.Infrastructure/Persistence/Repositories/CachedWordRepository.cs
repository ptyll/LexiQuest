using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Infrastructure.Persistence.Repositories;

public class CachedWordRepository : IWordRepository
{
    private readonly IWordRepository _innerRepository;
    private readonly ICacheService _cacheService;
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(30);

    public CachedWordRepository(IWordRepository innerRepository, ICacheService cacheService)
    {
        _innerRepository = innerRepository;
        _cacheService = cacheService;
    }

    public async Task<Word?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _innerRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<Word>> GetByDifficultyAsync(DifficultyLevel difficulty, CancellationToken cancellationToken = default)
    {
        return await _cacheService.GetOrCreateAsync(
            $"words:difficulty:{difficulty}",
            () => _innerRepository.GetByDifficultyAsync(difficulty, cancellationToken),
            CacheExpiration);
    }

    public async Task<IReadOnlyList<Word>> GetByCategoryAsync(WordCategory category, CancellationToken cancellationToken = default)
    {
        return await _cacheService.GetOrCreateAsync(
            $"words:category:{category}",
            () => _innerRepository.GetByCategoryAsync(category, cancellationToken),
            CacheExpiration);
    }

    public async Task<Word?> GetRandomAsync(DifficultyLevel? difficulty = null, WordCategory? category = null, CancellationToken cancellationToken = default)
    {
        return await _innerRepository.GetRandomAsync(difficulty, category, cancellationToken);
    }

    public async Task<IReadOnlyList<Word>> GetRandomBatchAsync(int count, DifficultyLevel? difficulty = null, WordCategory? category = null, CancellationToken cancellationToken = default)
    {
        return await _innerRepository.GetRandomBatchAsync(count, difficulty, category, cancellationToken);
    }

    public async Task AddAsync(Word word, CancellationToken cancellationToken = default)
    {
        await _innerRepository.AddAsync(word, cancellationToken);
        InvalidateCache();
    }

    public async Task AddRangeAsync(IEnumerable<Word> words, CancellationToken cancellationToken = default)
    {
        await _innerRepository.AddRangeAsync(words, cancellationToken);
        InvalidateCache();
    }

    public void Remove(Word word)
    {
        _innerRepository.Remove(word);
        InvalidateCache();
    }

    public async Task<Word?> GetByNormalizedAsync(string normalized, CancellationToken cancellationToken = default)
    {
        return await _innerRepository.GetByNormalizedAsync(normalized, cancellationToken);
    }

    public async Task<(IReadOnlyList<Word> Items, int TotalCount)> GetPaginatedAsync(
        string? search, DifficultyLevel? difficulty, WordCategory? category,
        int? minLength, int? maxLength, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _innerRepository.GetPaginatedAsync(search, difficulty, category, minLength, maxLength, page, pageSize, cancellationToken);
    }

    public async Task<IReadOnlyList<Word>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _cacheService.GetOrCreateAsync(
            "words:all",
            () => _innerRepository.GetAllAsync(cancellationToken),
            CacheExpiration);
    }

    public async Task<int> CountByDifficultyAsync(DifficultyLevel difficulty, CancellationToken cancellationToken = default)
    {
        return await _innerRepository.CountByDifficultyAsync(difficulty, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _innerRepository.SaveChangesAsync(cancellationToken);
    }

    private void InvalidateCache()
    {
        _cacheService.Remove("words:all");
        foreach (var difficulty in Enum.GetValues<DifficultyLevel>())
        {
            _cacheService.Remove($"words:difficulty:{difficulty}");
        }
        foreach (var category in Enum.GetValues<WordCategory>())
        {
            _cacheService.Remove($"words:category:{category}");
        }
    }
}
