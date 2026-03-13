using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Persistence.Repositories;

public class WordRepository : IWordRepository
{
    private readonly LexiQuestDbContext _context;

    public WordRepository(LexiQuestDbContext context)
    {
        _context = context;
    }

    public async Task<Word?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Words.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<Word>> GetByDifficultyAsync(DifficultyLevel difficulty, CancellationToken cancellationToken = default)
    {
        return await _context.Words
            .AsNoTracking()
            .Where(w => w.Difficulty == difficulty)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Word>> GetByCategoryAsync(WordCategory category, CancellationToken cancellationToken = default)
    {
        return await _context.Words
            .AsNoTracking()
            .Where(w => w.Category == category)
            .ToListAsync(cancellationToken);
    }

    public async Task<Word?> GetRandomAsync(DifficultyLevel? difficulty = null, WordCategory? category = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Words.AsNoTracking();

        if (difficulty.HasValue)
            query = query.Where(w => w.Difficulty == difficulty.Value);

        if (category.HasValue)
            query = query.Where(w => w.Category == category.Value);

        var count = await query.CountAsync(cancellationToken);
        if (count == 0)
            return null;

        var randomIndex = Random.Shared.Next(count);
        return await query.Skip(randomIndex).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Word>> GetRandomBatchAsync(int count, DifficultyLevel? difficulty = null, WordCategory? category = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Words.AsNoTracking().AsQueryable();

        if (difficulty.HasValue)
            query = query.Where(w => w.Difficulty == difficulty.Value);

        if (category.HasValue)
            query = query.Where(w => w.Category == category.Value);

        // Load only the IDs matching the filter
        var matchingIds = await query
            .Select(w => w.Id)
            .ToListAsync(cancellationToken);

        if (matchingIds.Count == 0)
            return new List<Word>();

        // Shuffle in memory and take the requested count
        Random.Shared.Shuffle(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(matchingIds));
        var selectedIds = matchingIds.Take(Math.Min(count, matchingIds.Count)).ToList();

        // Load full entities for the selected IDs
        var words = await _context.Words
            .AsNoTracking()
            .Where(w => selectedIds.Contains(w.Id))
            .ToListAsync(cancellationToken);

        return words;
    }

    public async Task AddAsync(Word word, CancellationToken cancellationToken = default)
    {
        await _context.Words.AddAsync(word, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Word> words, CancellationToken cancellationToken = default)
    {
        await _context.Words.AddRangeAsync(words, cancellationToken);
    }

    public void Remove(Word word)
    {
        _context.Words.Remove(word);
    }

    public async Task<Word?> GetByNormalizedAsync(string normalized, CancellationToken cancellationToken = default)
    {
        return await _context.Words
            .FirstOrDefaultAsync(w => w.Normalized == normalized, cancellationToken);
    }

    public async Task<(IReadOnlyList<Word> Items, int TotalCount)> GetPaginatedAsync(
        string? search, DifficultyLevel? difficulty, WordCategory? category,
        int? minLength, int? maxLength, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Words.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(w => w.Original.Contains(search) || w.Normalized.Contains(search.ToLowerInvariant()));

        if (difficulty.HasValue)
            query = query.Where(w => w.Difficulty == difficulty.Value);

        if (category.HasValue)
            query = query.Where(w => w.Category == category.Value);

        if (minLength.HasValue)
            query = query.Where(w => w.Length >= minLength.Value);

        if (maxLength.HasValue)
            query = query.Where(w => w.Length <= maxLength.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(w => w.Original)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Word>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Words
            .AsNoTracking()
            .OrderBy(w => w.Original)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByDifficultyAsync(DifficultyLevel difficulty, CancellationToken cancellationToken = default)
    {
        return await _context.Words
            .CountAsync(w => w.Difficulty == difficulty, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
