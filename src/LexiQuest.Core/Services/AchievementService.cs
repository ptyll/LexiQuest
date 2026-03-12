using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Achievements;
using Microsoft.Extensions.Localization;

namespace LexiQuest.Core.Services;

public class AchievementService : IAchievementService
{
    private readonly IAchievementRepository _achievementRepository;
    private readonly IUserAchievementRepository _userAchievementRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<AchievementService> _localizer;

    public AchievementService(
        IAchievementRepository achievementRepository,
        IUserAchievementRepository userAchievementRepository,
        IUnitOfWork unitOfWork,
        IStringLocalizer<AchievementService> localizer)
    {
        _achievementRepository = achievementRepository;
        _userAchievementRepository = userAchievementRepository;
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    public async Task<List<AchievementUnlockResult>> CheckWordSolvedAsync(Guid userId, int totalWordsSolved, CancellationToken cancellationToken = default)
    {
        var unlocked = new List<AchievementUnlockResult>();

        // Check first word achievement
        var firstWord = await _achievementRepository.GetByKeyAsync("first_word", cancellationToken);
        if (firstWord != null && totalWordsSolved >= firstWord.RequiredValue)
        {
            var result = await TryUnlockAchievementAsync(userId, firstWord, cancellationToken);
            if (result != null) unlocked.Add(result);
        }

        // Check 100 words achievement
        var hundredWords = await _achievementRepository.GetByKeyAsync("100_words", cancellationToken);
        if (hundredWords != null && totalWordsSolved >= hundredWords.RequiredValue)
        {
            var result = await TryUnlockAchievementAsync(userId, hundredWords, cancellationToken);
            if (result != null) unlocked.Add(result);
        }

        // Check 1000 words achievement
        var thousandWords = await _achievementRepository.GetByKeyAsync("1000_words", cancellationToken);
        if (thousandWords != null && totalWordsSolved >= thousandWords.RequiredValue)
        {
            var result = await TryUnlockAchievementAsync(userId, thousandWords, cancellationToken);
            if (result != null) unlocked.Add(result);
        }

        return unlocked;
    }

    public async Task<List<AchievementUnlockResult>> CheckStreakAsync(Guid userId, int currentStreak, CancellationToken cancellationToken = default)
    {
        var unlocked = new List<AchievementUnlockResult>();

        var streakAchievements = new[] { ("streak_3", 3), ("streak_7", 7), ("streak_14", 14), ("streak_30", 30), ("streak_365", 365) };
        
        foreach (var (key, requiredStreak) in streakAchievements)
        {
            if (currentStreak >= requiredStreak)
            {
                var achievement = await _achievementRepository.GetByKeyAsync(key, cancellationToken);
                if (achievement != null)
                {
                    var result = await TryUnlockAchievementAsync(userId, achievement, cancellationToken);
                    if (result != null) unlocked.Add(result);
                }
            }
        }

        return unlocked;
    }

    public async Task<List<AchievementUnlockResult>> CheckPathCompletedAsync(Guid userId, Guid pathId, CancellationToken cancellationToken = default)
    {
        var unlocked = new List<AchievementUnlockResult>();

        var pathAchievement = await _achievementRepository.GetByKeyAsync("path_complete", cancellationToken);
        if (pathAchievement != null)
        {
            var result = await TryUnlockAchievementAsync(userId, pathAchievement, cancellationToken);
            if (result != null) unlocked.Add(result);
        }

        return unlocked;
    }

    public async Task<List<AchievementUnlockResult>> CheckBossDefeatedAsync(Guid userId, bool perfectRun, CancellationToken cancellationToken = default)
    {
        var unlocked = new List<AchievementUnlockResult>();

        var bossAchievement = await _achievementRepository.GetByKeyAsync("boss_defeated", cancellationToken);
        if (bossAchievement != null)
        {
            var result = await TryUnlockAchievementAsync(userId, bossAchievement, cancellationToken);
            if (result != null) unlocked.Add(result);
        }

        if (perfectRun)
        {
            var perfectAchievement = await _achievementRepository.GetByKeyAsync("perfect_boss", cancellationToken);
            if (perfectAchievement != null)
            {
                var result = await TryUnlockAchievementAsync(userId, perfectAchievement, cancellationToken);
                if (result != null) unlocked.Add(result);
            }
        }

        return unlocked;
    }

    public async Task<int> GetProgressAsync(Guid userId, Guid achievementId, CancellationToken cancellationToken = default)
    {
        var achievement = await _achievementRepository.GetByIdAsync(achievementId, cancellationToken);
        if (achievement == null) return 0;

        var userAchievement = await _userAchievementRepository.GetByUserAndAchievementAsync(userId, achievementId, cancellationToken);
        if (userAchievement == null) return 0;

        return userAchievement.GetProgressPercentage(achievement.RequiredValue);
    }

    public async Task<List<AchievementDto>> GetUserAchievementsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var allAchievements = await _achievementRepository.GetAllAsync(cancellationToken);
        var userAchievements = await _userAchievementRepository.GetByUserAsync(userId, cancellationToken);

        var result = new List<AchievementDto>();
        foreach (var achievement in allAchievements)
        {
            var userAchievement = userAchievements.FirstOrDefault(ua => ua.AchievementId == achievement.Id);
            
            result.Add(new AchievementDto(
                Id: achievement.Id,
                Key: achievement.Key,
                Name: achievement.Name,
                Description: achievement.Description,
                Category: achievement.Category,
                XPReward: achievement.XPReward,
                RequiredValue: achievement.RequiredValue,
                CurrentProgress: userAchievement?.Progress ?? 0,
                ProgressPercentage: userAchievement?.GetProgressPercentage(achievement.RequiredValue) ?? 0,
                IsUnlocked: userAchievement?.IsUnlocked ?? false,
                UnlockedAt: userAchievement?.UnlockedAt,
                IconName: achievement.IconName
            ));
        }

        return result;
    }

    private async Task<AchievementUnlockResult?> TryUnlockAchievementAsync(Guid userId, Achievement achievement, CancellationToken cancellationToken)
    {
        var userAchievement = await _userAchievementRepository.GetByUserAndAchievementAsync(userId, achievement.Id, cancellationToken);
        
        if (userAchievement == null)
        {
            userAchievement = UserAchievement.Create(userId, achievement.Id);
            await _userAchievementRepository.AddAsync(userAchievement, cancellationToken);
        }

        if (userAchievement.IsUnlocked)
            return null;

        userAchievement.Unlock();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AchievementUnlockResult(
            achievement.Id,
            achievement.Key,
            achievement.Name,
            achievement.XPReward
        );
    }
}
