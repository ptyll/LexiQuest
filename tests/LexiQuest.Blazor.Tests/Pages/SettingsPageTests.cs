using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Pages;
using LexiQuest.Blazor.Services;
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

    public SettingsPageTests()
    {
        _localizer = Substitute.For<IStringLocalizer<Settings>>();
        _localizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
        
        _userService = Substitute.For<IUserService>();
        Services.AddSingleton(_localizer);
        Services.AddSingleton(_userService);
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

        var cut = Render<Settings>();

        // Act
        var usernameInput = cut.Find("[data-testid='username-input'] input");
        usernameInput.Input("newusername");
        
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
        cut.Find("[data-testid='current-password-input'] input").Input("OldPass123!");
        cut.Find("[data-testid='new-password-input'] input").Input("NewPass123!");
        cut.Find("[data-testid='confirm-password-input'] input").Input("NewPass123!");
        
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
}
