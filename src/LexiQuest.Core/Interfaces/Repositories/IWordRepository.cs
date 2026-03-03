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
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
