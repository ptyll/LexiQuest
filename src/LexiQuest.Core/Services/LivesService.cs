using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Services;

public class LivesService : ILivesService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LivesService(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public int GetMaxLives(DifficultyLevel difficulty)
    {
        // For training mode, we pass Beginner but with infinite flag set separately
        // For actual game modes:
        return difficulty switch
        {
            DifficultyLevel.Beginner => 5,
            DifficultyLevel.Intermediate => 4,
            DifficultyLevel.Advanced => 3,
            DifficultyLevel.Expert => 3,
            _ => 5
        };
    }

    public async Task<LivesStatus> GetLivesStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User {userId} not found");
        }

        // Check if regeneration is due
        await TryRegenerateAsync(user, cancellationToken);

        var isInfinite = user.MaxLives == int.MaxValue;
        return new LivesStatus(
            user.LivesRemaining,
            isInfinite ? int.MaxValue : user.MaxLives,
            isInfinite ? null : user.NextLifeRegenAt,
            isInfinite
        );
    }

    public async Task<bool> LoseLifeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User {userId} not found");
        }

        // Training mode - infinite lives, but still track that life was lost
        if (user.MaxLives == int.MaxValue)
        {
            return true;
        }

        if (user.LivesRemaining <= 0)
        {
            return false;
        }

        user.LoseLife();
        
        // Schedule next regeneration
        if (user.LivesRemaining < user.MaxLives)
        {
            var regenMinutes = GetRegenMinutes(user.MaxLives);
            user.ScheduleNextRegen(regenMinutes);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RegenerateLifeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User {userId} not found");
        }

        return await TryRegenerateAsync(user, cancellationToken);
    }

    public async Task RefillLivesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User {userId} not found");
        }

        user.RefillLives();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<bool> TryRegenerateAsync(User user, CancellationToken cancellationToken)
    {
        // Training mode - no regeneration needed
        if (user.MaxLives == int.MaxValue)
        {
            return false;
        }

        // Check if at max lives
        if (user.LivesRemaining >= user.MaxLives)
        {
            return false;
        }

        // Check if regeneration is due
        if (user.NextLifeRegenAt == null || user.NextLifeRegenAt > DateTime.UtcNow)
        {
            return false;
        }

        // Regenerate one life
        user.RegenerateLife();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private int GetRegenMinutes(int maxLives)
    {
        return maxLives switch
        {
            3 => 60,  // Expert/Advanced: 60 min
            4 => 30,  // Intermediate: 30 min
            5 => 20,  // Beginner: 20 min
            _ => 30
        };
    }
}
