using LexiQuest.Core.Interfaces;

namespace LexiQuest.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly LexiQuestDbContext _context;

    public UnitOfWork(LexiQuestDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
