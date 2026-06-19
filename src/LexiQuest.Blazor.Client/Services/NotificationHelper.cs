using Microsoft.Extensions.Localization;
using Tempo.Blazor.Services;

namespace LexiQuest.Blazor.Services;

/// <summary>
/// Helper service for standardized toast notifications.
/// </summary>
public class NotificationHelper
{
    private readonly ToastService _toastService;
    private readonly IStringLocalizer<NotificationHelper> _localizer;

    public NotificationHelper(ToastService toastService, IStringLocalizer<NotificationHelper> localizer)
    {
        _toastService = toastService;
        _localizer = localizer;
    }

    // Success notifications
    public void ShowXPGained(int xpAmount)
    {
        _toastService.ShowSuccess(string.Format(_localizer["XP_Gained"], xpAmount));
    }

    public void ShowLevelUp(int newLevel)
    {
        _toastService.ShowSuccess(string.Format(_localizer["Level_Up"], newLevel), null, 8000);
    }

    public void ShowAchievementUnlocked(string achievementName)
    {
        _toastService.ShowSuccess(string.Format(_localizer["Achievement_Unlocked"], achievementName), null, 8000);
    }

    public void ShowWordSolved(int points)
    {
        _toastService.ShowSuccess(string.Format(_localizer["Word_Solved"], points));
    }

    public void ShowStreakUpdated(int currentStreak)
    {
        _toastService.ShowSuccess(string.Format(_localizer["Streak_Updated"], currentStreak));
    }

    // Error notifications
    public void ShowApiError(string? message = null)
    {
        _toastService.ShowError(message ?? _localizer["Api_Error"]);
    }

    public void ShowConnectionLost()
    {
        _toastService.ShowError(_localizer["Connection_Lost"]);
    }

    public void ShowValidationError(string field)
    {
        _toastService.ShowWarning(string.Format(_localizer["Validation_Error"], field));
    }

    // Warning notifications
    public void ShowStreakEnding(int hoursRemaining)
    {
        _toastService.ShowWarning(string.Format(_localizer["Streak_Ending"], hoursRemaining), null, 10000);
    }

    public void ShowLivesLow(int livesRemaining)
    {
        _toastService.ShowWarning(string.Format(_localizer["Lives_Low"], livesRemaining));
    }

    // Info notifications
    public void ShowDailyChallengeAvailable()
    {
        _toastService.ShowInfo(_localizer["Daily_Challenge_Available"], null, 10000);
    }

    public void ShowLeagueUpdated()
    {
        _toastService.ShowInfo(_localizer["League_Updated"]);
    }
}
