using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Persistence.Repositories;

public class CustomDictionaryRepository : ICustomDictionaryRepository
{
    private readonly LexiQuestDbContext _context;

    public CustomDictionaryRepository(LexiQuestDbContext context)
    {
        _context = context;
    }

    public async Task<CustomDictionary?> GetByIdAsync(Guid id)
    {
        return await _context.CustomDictionaries.FindAsync(id);
    }

    public async Task<IReadOnlyList<CustomDictionary>> GetByUserIdAsync(Guid userId)
    {
        return await _context.CustomDictionaries
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.UpdatedAt)
            .ThenByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<CustomDictionary>> GetPublicDictionariesAsync()
    {
        return await _context.CustomDictionaries
            .Where(d => d.IsPublic)
            .OrderByDescending(d => d.WordCount)
            .ThenBy(d => d.Name)
            .ToListAsync();
    }

    public async Task AddAsync(CustomDictionary dictionary)
    {
        await _context.CustomDictionaries.AddAsync(dictionary);
    }

    public void Update(CustomDictionary dictionary)
    {
        _context.CustomDictionaries.Update(dictionary);
    }

    public void Delete(CustomDictionary dictionary)
    {
        _context.CustomDictionaries.Remove(dictionary);
    }

    public async Task<int> GetWordCountAsync(Guid dictionaryId)
    {
        return await _context.DictionaryWords
            .CountAsync(w => w.DictionaryId == dictionaryId);
    }
}
