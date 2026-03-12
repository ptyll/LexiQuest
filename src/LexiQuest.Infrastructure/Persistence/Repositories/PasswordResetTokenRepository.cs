using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Persistence.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly LexiQuestDbContext _context;

    public PasswordResetTokenRepository(LexiQuestDbContext context)
    {
        _context = context;
    }

    public async Task<PasswordResetToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
    }

    public async Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
    {
        await _context.PasswordResetTokens.AddAsync(token, cancellationToken);
    }

    public async Task<List<PasswordResetToken>> GetExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PasswordResetTokens
            .Where(t => t.ExpiresAt < DateTime.UtcNow || t.IsUsed)
            .ToListAsync(cancellationToken);
    }

    public void Delete(PasswordResetToken token)
    {
        _context.PasswordResetTokens.Remove(token);
    }

    public Task DeleteAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
    {
        Delete(token);
        return Task.CompletedTask;
    }
}
