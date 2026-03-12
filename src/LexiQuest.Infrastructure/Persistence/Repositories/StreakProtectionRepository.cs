using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Persistence.Repositories;

public class StreakProtectionRepository : IStreakProtectionRepository
{
    private readonly LexiQuestDbContext _context;

    public StreakProtectionRepository(LexiQuestDbContext context)
    {
        _context = context;
    }

    public async Task<StreakProtection?> GetByUserIdAsync(Guid userId)
    {
        return await _context.StreakProtections
            .FirstOrDefaultAsync(sp => sp.UserId == userId);
    }

    public async Task AddAsync(StreakProtection protection)
    {
        await _context.StreakProtections.AddAsync(protection);
    }

    public void Update(StreakProtection protection)
    {
        _context.StreakProtections.Update(protection);
    }
}
