using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.DTOs.Game;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Services;

/// <summary>
/// Service for managing XP gains and level progression.
/// </summary>
public class XpService : IXpService
{
    private readonly LexiQuestDbContext _context;
    private readonly ILevelCalculator _levelCalculator;

    public XpService(LexiQuestDbContext context, ILevelCalculator levelCalculator)
    {
        _context = context;
        _levelCalculator = levelCalculator;
    }

    public async Task<XPGainedEvent> AddXpAsync(Guid userId, int amount, XpSource source, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.Stats)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        var previousXp = user.Stats.TotalXP;
        var previousLevel = _levelCalculator.GetLevelFromXp(previousXp);

        // Add XP
        user.Stats.AddXP(amount);
        
        await _context.SaveChangesAsync(cancellationToken);

        var newXp = user.Stats.TotalXP;
        var newLevel = _levelCalculator.GetLevelFromXp(newXp);
        var hasLeveledUp = newLevel > previousLevel;

        // Determine unlocks based on new level
        var unlocks = hasLeveledUp ? GetUnlocksForLevel(newLevel) : null;

        return new XPGainedEvent(
            Amount: amount,
            Source: source.ToString(),
            LeveledUp: hasLeveledUp,
            NewLevel: newLevel,
            TotalXP: newXp,
            Unlocks: unlocks
        );
    }

    private static List<UnlockableReward>? GetUnlocksForLevel(int level)
    {
        var unlocks = new List<UnlockableReward>();

        // Define unlocks per level
        switch (level)
        {
            case 3:
                unlocks.Add(new UnlockableReward("Path", "Path2", "Intermediate path unlocked"));
                break;
            case 5:
                unlocks.Add(new UnlockableReward("Feature", "Leagues", "Leagues feature unlocked"));
                break;
            case 10:
                unlocks.Add(new UnlockableReward("Path", "Path3", "Advanced path unlocked"));
                break;
        }

        return unlocks.Count > 0 ? unlocks : null;
    }
}
