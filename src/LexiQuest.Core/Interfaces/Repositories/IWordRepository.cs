using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Interfaces.Repositories;

public interface IWordRepository
{
    Task<Word?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Word>> GetByDifficultyAsync(DifficultyLevel difficulty, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Word>> GetByCategoryAsync(WordCategory category, CancellationToken cancellationToken = default);
    Task<Word?> GetRandomAsync(DifficultyLevel? difficulty = null, WordCategory? category = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Word>> GetRandomBatchAsync(int count, DifficultyLevel? difficulty = null, WordCategory? category = null, CancellationToken cancellationToken = default);
    Task AddAsync(Word word, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Word> words, CancellationToken cancellationToken = default);
    void Remove(Word word);
    Task<Word?> GetByNormalizedAsync(string normalized, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Word> Items, int TotalCount)> GetPaginatedAsync(
        string? search, DifficultyLevel? difficulty, WordCategory? category,
        int? minLength, int? maxLength, int page, int pageSize,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Word>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<int> CountByDifficultyAsync(DifficultyLevel difficulty, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
