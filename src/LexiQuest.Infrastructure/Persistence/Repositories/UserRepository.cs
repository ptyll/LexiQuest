using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly LexiQuestDbContext _context;

    public UserRepository(LexiQuestDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Stats)
            .Include(u => u.Streak)
            .Include(u => u.Preferences)
            .Include(u => u.Premium)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Stats)
            .Include(u => u.Streak)
            .Include(u => u.Preferences)
            .Include(u => u.Premium)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Stats)
            .Include(u => u.Streak)
            .Include(u => u.Preferences)
            .Include(u => u.Premium)
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Username == username, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public async Task<User?> FindByStripeCustomerIdAsync(string stripeCustomerId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Stats)
            .Include(u => u.Streak)
            .Include(u => u.Preferences)
            .Include(u => u.Premium)
            .FirstOrDefaultAsync(u => u.StripeCustomerId == stripeCustomerId, cancellationToken);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<User>> GetUsersWithStreakNotPlayedTodayAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Users
            .Include(u => u.Streak)
            .Where(u => u.Streak != null && u.Streak.CurrentDays > 0 && u.Streak.LastActivityDate < today)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        return await _context.Users
            .Where(u => u.LastLoginAt != null && u.LastLoginAt >= sevenDaysAgo)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<User>> GetInactiveUsersAsync(int daysInactive, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysInactive);
        var cutoffStart = cutoffDate.Date;
        var cutoffEnd = cutoffStart.AddDays(1);
        return await _context.Users
            .Where(u => u.LastLoginAt != null && u.LastLoginAt >= cutoffStart && u.LastLoginAt < cutoffEnd)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
