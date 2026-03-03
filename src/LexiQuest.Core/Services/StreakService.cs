using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Game;

namespace LexiQuest.Core.Services;

public class StreakService : IStreakService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StreakService(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<StreakStatus> CheckStreakAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User {userId} not found");
        }

        var now = DateTime.UtcNow;
        var today = now.Date;
        var lastActivity = user.Streak.LastActivityDate;

        // Determine if we need to update the streak
        if (lastActivity == null)
        {
            // First activity ever
            user.Streak.RecordActivity(now);
        }
        else if (lastActivity.Value.Date == today)
        {
            // Already recorded today - no change
        }
        else if (lastActivity.Value.Date == today.AddDays(-1))
        {
            // Yesterday - consecutive day, continue streak
            user.Streak.RecordActivity(now);
        }
        else if (IsWithinGracePeriod(lastActivity.Value, now))
        {
            // Within 48-hour grace period, continue streak
            user.Streak.RecordActivity(now);
        }
        else
        {
            // Streak broken - more than 48 hours, reset to 1
            user.Streak.RecordActivity(now);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Calculate next reset time (48 hours after last activity, or tomorrow if played today)
        var lastActivityDate = user.Streak.LastActivityDate ?? now;
        var nextResetAt = lastActivityDate.Date.AddDays(2);
        var timeRemaining = nextResetAt - now;
        if (timeRemaining < TimeSpan.Zero)
        {
            timeRemaining = TimeSpan.Zero;
        }

        // Is at risk if last activity was yesterday (haven't played today yet)
        // After RecordActivity, this should be false
        var isAtRisk = (user.Streak.LastActivityDate?.Date == today.AddDays(-1));

        var fireLevel = GetFireLevel(user.Streak.CurrentDays);

        return new StreakStatus(
            CurrentDays: user.Streak.CurrentDays,
            LongestDays: user.Streak.LongestDays,
            FireLevel: fireLevel.ToString(),
            NextResetAt: nextResetAt,
            TimeRemaining: timeRemaining,
            IsAtRisk: isAtRisk
        );
    }

    public FireLevel GetFireLevel(int days)
    {
        if (days <= 0) return FireLevel.Cold;
        if (days <= 3) return FireLevel.Small;
        if (days <= 7) return FireLevel.Medium;
        if (days <= 30) return FireLevel.Large;
        return FireLevel.Legendary;
    }

    private bool IsWithinGracePeriod(DateTime lastActivity, DateTime now)
    {
        // 48-hour grace period from the exact last activity time
        var timeSinceLastActivity = now - lastActivity;
        return timeSinceLastActivity <= TimeSpan.FromHours(48);
    }
}
