using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Persistence.Repositories;

public class DictionaryWordRepository : IDictionaryWordRepository
{
    private readonly LexiQuestDbContext _context;

    public DictionaryWordRepository(LexiQuestDbContext context)
    {
        _context = context;
    }

    public async Task<DictionaryWord?> GetByIdAsync(Guid id)
    {
        return await _context.DictionaryWords.FindAsync(id);
    }

    public async Task<IReadOnlyList<DictionaryWord>> GetByDictionaryIdAsync(Guid dictionaryId)
    {
        return await _context.DictionaryWords
            .Where(w => w.DictionaryId == dictionaryId)
            .OrderBy(w => w.Word)
            .ToListAsync();
    }

    public async Task<bool> ExistsInDictionaryAsync(Guid dictionaryId, string word)
    {
        var normalizedWord = word.ToLowerInvariant();
        return await _context.DictionaryWords
            .AnyAsync(w => w.DictionaryId == dictionaryId && 
                          w.Word.ToLower() == normalizedWord);
    }

    public async Task AddAsync(DictionaryWord word)
    {
        await _context.DictionaryWords.AddAsync(word);
    }

    public void Delete(DictionaryWord word)
    {
        _context.DictionaryWords.Remove(word);
    }

    public async Task<int> CountByDictionaryIdAsync(Guid dictionaryId)
    {
        return await _context.DictionaryWords
            .CountAsync(w => w.DictionaryId == dictionaryId);
    }
}
