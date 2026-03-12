using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Persistence.Repositories;

public class AchievementRepository : IAchievementRepository
{
    private readonly LexiQuestDbContext _context;

    public AchievementRepository(LexiQuestDbContext context)
    {
        _context = context;
    }

    public async Task<Achievement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Achievements
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Achievement?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.Achievements
            .FirstOrDefaultAsync(a => a.Key == key, cancellationToken);
    }

    public async Task<List<Achievement>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Achievements
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Achievement>> GetByCategoryAsync(AchievementCategory category, CancellationToken cancellationToken = default)
    {
        return await _context.Achievements
            .Where(a => a.Category == category)
            .ToListAsync(cancellationToken);
    }
}

public class UserAchievementRepository : IUserAchievementRepository
{
    private readonly LexiQuestDbContext _context;

    public UserAchievementRepository(LexiQuestDbContext context)
    {
        _context = context;
    }

    public async Task<UserAchievement?> GetByUserAndAchievementAsync(Guid userId, Guid achievementId, CancellationToken cancellationToken = default)
    {
        return await _context.UserAchievements
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId, cancellationToken);
    }

    public async Task<List<UserAchievement>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserAchievements
            .Where(ua => ua.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(UserAchievement userAchievement, CancellationToken cancellationToken = default)
    {
        await _context.UserAchievements.AddAsync(userAchievement, cancellationToken);
    }

    public void Update(UserAchievement userAchievement)
    {
        _context.UserAchievements.Update(userAchievement);
    }
}
