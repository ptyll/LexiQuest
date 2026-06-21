using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Pages;
using LexiQuest.Blazor.Services;
using LexiQuest.Shared.DTOs.Notifications;
using LexiQuest.Shared.DTOs.Users;
using LexiQuest.Shared.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Tempo.Blazor.Localization;
using Tempo.Blazor.Services;
using Xunit;

namespace LexiQuest.Blazor.Tests.Pages;

public class SettingsPageTests : TestContext
{
    private readonly IStringLocalizer<Settings> _localizer;
    private readonly IUserService _userService;
    private readonly INotificationService _notificationService;

    public SettingsPageTests()
    {
        _localizer = Substitute.For<IStringLocalizer<Settings>>();
        _localizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
        
        _userService = Substitute.For<IUserService>();
        _notificationService = Substitute.For<INotificationService>();
        _notificationService.GetPreferencesAsync().Returns(new NotificationPreferenceDto(
            true,
            true,
            true,
            TimeSpan.FromHours(20),
            true,
            true,
            true));
        _notificationService.UpdatePreferencesAsync(Arg.Any<UpdatePreferencesRequest>())
            .Returns(Task.CompletedTask);
        _notificationService.SavePushSubscriptionAsync(Arg.Any<PushSubscriptionDto>())
            .Returns(Task.CompletedTask);
        Services.AddSingleton(_localizer);
        Services.AddSingleton(_userService);
        Services.AddSingleton(_notificationService);
        Services.AddSingleton(Substitute.For<IAuthService>());
        Services.AddSingleton(Substitute.For<NavigationManager>());
        Services.AddSingleton(new ToastService());
        Services.AddSingleton(Substitute.For<ITmLocalizer>());
    }

    [Fact]
    public void SettingsPage_Renders_AllSections()
    {
        // Arrange
        var profile = CreateTestProfile();
        _userService.GetProfileAsync().Returns(profile);

        // Act
        var cut = Render<Settings>();

        // Assert
        cut.Find("[data-testid='profile-section']").Should().NotBeNull();
        cut.Find("[data-testid='password-section']").Should().NotBeNull();
        cut.Find("[data-testid='preferences-section']").Should().NotBeNull();
        cut.Find("[data-testid='privacy-section']").Should().NotBeNull();
    }

    [Fact]
    public void SettingsPage_Renders_UserProfileData()
    {
        // Arrange
        var profile = CreateTestProfile();
        _userService.GetProfileAsync().Returns(profile);

        // Act
        var cut = Render<Settings>();

        // Assert
        var usernameInput = cut.Find("[data-testid='username-input'] input");
        usernameInput.GetAttribute("value").Should().Be(profile.Username);
        
        var emailInput = cut.Find("[data-testid='email-input'] input");
        emailInput.GetAttribute("value").Should().Be(profile.Email);
    }

    [Fact]
    public async Task SettingsPage_UpdateUsername_CallsApi()
    {
        // Arrange
        var profile = CreateTestProfile();
        _userService.GetProfileAsync().Returns(profile);
        _userService.UpdateProfileAsync(Arg.Any<UpdateProfileRequest>()).Returns(true);
        _userService.IsUsernameAvailableAsync("newusername", Arg.Any<CancellationToken>()).Returns(true);

        var cut = Render<Settings>();

        // Act
        var usernameInput = cut.Find("[data-testid='username-input'] input");
        usernameInput.Change("newusername");
        
        var saveButton = cut.Find("[data-testid='save-profile-btn'] button");
        await saveButton.ClickAsync();

        // Assert
        await _userService.Received(1).UpdateProfileAsync(Arg.Is<UpdateProfileRequest>(r => r.Username == "newusername"));
    }

    [Fact]
    public void SettingsPage_ChangePassword_RendersPasswordFields()
    {
        // Arrange
        var profile = CreateTestProfile();
        _userService.GetProfileAsync().Returns(profile);

        // Act
        var cut = Render<Settings>();

        // Assert
        cut.Find("[data-testid='current-password-input']").Should().NotBeNull();
        cut.Find("[data-testid='new-password-input']").Should().NotBeNull();
        cut.Find("[data-testid='confirm-password-input']").Should().NotBeNull();
    }

    [Fact]
    public async Task SettingsPage_ChangePassword_ValidatesAndSubmits()
    {
        // Arrange
        var profile = CreateTestProfile();
        _userService.GetProfileAsync().Returns(profile);
        _userService.ChangePasswordAsync(Arg.Any<ChangePasswordRequest>()).Returns(true);

        var cut = Render<Settings>();

        // Act
        cut.Find("[data-testid='current-password-input'] input").Change("OldPass123!");
        cut.Find("[data-testid='new-password-input'] input").Change("NewPass123!");
        cut.Find("[data-testid='confirm-password-input'] input").Change("NewPass123!");
        
        var changeBtn = cut.Find("[data-testid='change-password-btn'] button");
        await changeBtn.ClickAsync();

        // Assert
        await _userService.Received(1).ChangePasswordAsync(Arg.Any<ChangePasswordRequest>());
    }

    [Fact]
    public void SettingsPage_Preferences_RendersToggles()
    {
        // Arrange
        var profile = CreateTestProfile();
        _userService.GetProfileAsync().Returns(profile);

        // Act
        var cut = Render<Settings>();

        // Assert
        cut.Find("[data-testid='push-notifications-toggle']").Should().NotBeNull();
        cut.Find("[data-testid='email-notifications-toggle']").Should().NotBeNull();
        cut.Find("[data-testid='animations-toggle']").Should().NotBeNull();
        cut.Find("[data-testid='sounds-toggle']").Should().NotBeNull();
    }

    [Fact]
    public void SettingsPage_Preferences_LoadsNotificationPreferences()
    {
        // Arrange
        var profile = CreateTestProfile();
        profile.Preferences.PushNotificationsEnabled = true;
        profile.Preferences.EmailNotificationsEnabled = true;
        profile.Preferences.LeagueUpdatesEnabled = true;
        profile.Preferences.AchievementNotificationsEnabled = true;
        profile.Preferences.DailyChallengeReminderEnabled = true;

        _userService.GetProfileAsync().Returns(profile);
        _notificationService.GetPreferencesAsync().Returns(new NotificationPreferenceDto(
            false,
            false,
            true,
            TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(30)),
            false,
            false,
            false));

        // Act
        var cut = Render<Settings>();

        // Assert
        IsChecked(cut, "[data-testid='push-notifications-toggle']").Should().BeFalse();
        IsChecked(cut, "[data-testid='email-notifications-toggle']").Should().BeFalse();
        IsChecked(cut, "[data-testid='league-updates-toggle']").Should().BeFalse();
        IsChecked(cut, "[data-testid='achievement-notifications-toggle']").Should().BeFalse();
        IsChecked(cut, "[data-testid='daily-challenge-reminder-toggle']").Should().BeFalse();
        cut.Find("[data-testid='streak-reminder-time-input'] input")
            .GetAttribute("value")
            .Should()
            .Be("19:30");
    }

    [Fact]
    public async Task SettingsPage_Preferences_SaveUpdatesNotificationPreferences()
    {
        // Arrange
        var profile = CreateTestProfile();
        _userService.GetProfileAsync().Returns(profile);
        _userService.UpdatePreferencesAsync(Arg.Any<UserPreferencesDto>()).Returns(true);

        var cut = Render<Settings>();

        // Act
        cut.Find("[data-testid='push-notifications-toggle'] input").Change(false);
        cut.Find("[data-testid='email-notifications-toggle'] input").Change(false);
        cut.Find("[data-testid='league-updates-toggle'] input").Change(false);
        cut.Find("[data-testid='achievement-notifications-toggle'] input").Change(false);
        cut.Find("[data-testid='daily-challenge-reminder-toggle'] input").Change(false);
        cut.Find("[data-testid='streak-reminder-time-input'] input").Change("07:05");

        await cut.Find("[data-testid='save-preferences-btn'] button").ClickAsync();

        // Assert
        await _userService.Received(1).UpdatePreferencesAsync(Arg.Is<UserPreferencesDto>(p =>
            !p.PushNotificationsEnabled
            && !p.EmailNotificationsEnabled
            && !p.LeagueUpdatesEnabled
            && !p.AchievementNotificationsEnabled
            && !p.DailyChallengeReminderEnabled
            && p.StreakReminderTime == TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(5))));

        await _notificationService.Received(1).UpdatePreferencesAsync(Arg.Is<UpdatePreferencesRequest>(r =>
            !r.PushEnabled
            && !r.EmailEnabled
            && r.StreakReminder
            && r.StreakReminderTime == TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(5))
            && !r.LeagueUpdates
            && !r.AchievementNotifications
            && !r.DailyChallengeReminder));
    }

    [Fact]
    public async Task SettingsPage_PushNotifications_EnableRequestsAndStoresSubscription()
    {
        // Arrange
        var profile = CreateTestProfile();
        _userService.GetProfileAsync().Returns(profile);
        _notificationService.GetPreferencesAsync().Returns(new NotificationPreferenceDto(
            false,
            true,
            true,
            TimeSpan.FromHours(20),
            true,
            true,
            true));

        var subscription = new PushSubscriptionDto(
            "https://push.lexiquest.test/unit",
            "unit-p256dh",
            "unit-auth");
        JSInterop
            .Setup<PushSubscriptionDto?>("lexiQuestPush.requestSubscription")
            .SetResult(subscription);

        var cut = Render<Settings>();

        // Act
        await cut.Find("[data-testid='push-notifications-toggle'] input").ChangeAsync(true);

        // Assert
        await _notificationService.Received(1).SavePushSubscriptionAsync(subscription);
        IsChecked(cut, "[data-testid='push-notifications-toggle']").Should().BeTrue();
    }

    [Fact]
    public void SettingsPage_Privacy_RendersVisibilityOptions()
    {
        // Arrange
        var profile = CreateTestProfile();
        _userService.GetProfileAsync().Returns(profile);

        // Act
        var cut = Render<Settings>();

        // Assert
        cut.Find("[data-testid='profile-visibility-group']").Should().NotBeNull();
        cut.Find("[data-testid='leaderboard-visibility-toggle']").Should().NotBeNull();
        cut.Find("[data-testid='stats-sharing-toggle']").Should().NotBeNull();
    }

    private static UserProfileDto CreateTestProfile()
    {
        return new UserProfileDto
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            Stats = new LexiQuest.Shared.DTOs.Users.UserStatsDto { Level = 5, TotalXP = 1000 },
            Preferences = new UserPreferencesDto
            {
                Theme = AppTheme.Light,
                Language = "cs",
                AnimationsEnabled = true,
                SoundsEnabled = true,
                PushNotificationsEnabled = true
            },
            Privacy = new PrivacySettingsDto
            {
                ProfileVisibility = ProfileVisibility.Public,
                LeaderboardVisible = true
            }
        };
    }

    private static bool IsChecked(IRenderedComponent<Settings> cut, string testId)
    {
        return cut.Find($"{testId} input").HasAttribute("checked");
    }
}
