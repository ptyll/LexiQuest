using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Game;

namespace LexiQuest.Core.Services;

public class XpService : IXpService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILevelCalculator _levelCalculator;

    public XpService(IUserRepository userRepository, IUnitOfWork unitOfWork, ILevelCalculator levelCalculator)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _levelCalculator = levelCalculator;
    }

    public async Task<XPGainedEvent> AddXpAsync(Guid userId, int amount, XpSource source, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User {userId} not found");
        }

        var previousXp = user.Stats.TotalXP;
        var previousLevel = _levelCalculator.GetLevelFromXp(previousXp);

        // Add XP to user stats
        user.Stats.AddXP(amount);
        
        var newXp = user.Stats.TotalXP;
        var newLevel = _levelCalculator.GetLevelFromXp(newXp);
        var hasLeveledUp = newLevel > previousLevel;

        // Update user's level property
        if (hasLeveledUp)
        {
            typeof(Domain.ValueObjects.UserStats)
                .GetProperty("Level")
                ?.SetValue(user.Stats, newLevel);
        }

        // Determine unlocks if level up occurred
        List<UnlockableReward>? unlocks = null;
        if (hasLeveledUp)
        {
            unlocks = GetUnlocksForLevel(newLevel);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new XPGainedEvent(
            Amount: amount,
            Source: source.ToString(),
            LeveledUp: hasLeveledUp,
            NewLevel: newLevel,
            TotalXP: newXp,
            Unlocks: unlocks
        );
    }

    private List<UnlockableReward> GetUnlocksForLevel(int level)
    {
        var unlocks = new List<UnlockableReward>();

        // Level 3: Unlock Path 2 (Intermediate)
        if (level == 3)
        {
            unlocks.Add(new UnlockableReward(
                Type: "Path",
                Name: "Intermediate",
                Description: "Cesta pro pokročilé - 5-7 písmen"
            ));
        }

        // Level 5: Unlock Leagues
        if (level == 5)
        {
            unlocks.Add(new UnlockableReward(
                Type: "Feature",
                Name: "Leagues",
                Description: "Žebříčky a ligy - soutěžte s ostatními hráči"
            ));
        }

        // Level 7: Unlock Advanced Path (Path 3)
        if (level == 7)
        {
            unlocks.Add(new UnlockableReward(
                Type: "Path",
                Name: "Advanced",
                Description: "Pokročilá cesta - 7-10 písmen"
            ));
        }

        // Level 10: Unlock Expert Path (Path 4)
        if (level == 10)
        {
            unlocks.Add(new UnlockableReward(
                Type: "Path",
                Name: "Expert",
                Description: "Expertní cesta - 10+ písmen"
            ));
        }

        // Level 15: Unlock Multiplayer
        if (level == 15)
        {
            unlocks.Add(new UnlockableReward(
                Type: "Feature",
                Name: "Multiplayer",
                Description: "Hrajte proti ostatním hráčům v reálném čase"
            ));
        }

        return unlocks;
    }
}
