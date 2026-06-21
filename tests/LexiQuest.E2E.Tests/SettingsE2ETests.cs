using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using LexiQuest.Shared.DTOs.Auth;
using LexiQuest.Shared.DTOs.Users;
using LexiQuest.Shared.Enums;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Full")]
[Trait("Category", "Visual")]
[Collection(E2ECollection.Name)]
public class SettingsE2ETests : E2ETestBase
{
    public SettingsE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Settings_ProfileUsernameDuplicateAndAvatarValidation_WorkEndToEnd()
    {
        await RunScenarioAsync("settings", "profile-username-avatar-validation", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("settings");
            var existing = await Fixture.RegisterUniqueUserAsync("taken");
            var newUsername = $"profil{Guid.NewGuid():N}"[..16];

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/settings");

            await Expect(page.GetByTestId(Selectors.Settings.Page)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Settings.ProfileSection)).ToBeVisibleAsync();
            await Expect(page.GetByText("Settings_")).Not.ToBeVisibleAsync();

            await page.GetByTestId(Selectors.Settings.UsernameInput).Locator("input").FillAsync(newUsername);

            var validAvatar = await CreateTempFileAsync(
                "avatar.png",
                Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+/p9sAAAAASUVORK5CYII="));
            await page.GetByTestId(Selectors.Settings.AvatarFileInput).SetInputFilesAsync(validAvatar);
            await Expect(page.GetByTestId(Selectors.Settings.AvatarPreview)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Settings.AvatarError)).Not.ToBeVisibleAsync();

            await ClickButtonInAsync(page.GetByTestId(Selectors.Settings.SaveProfile));
            await Expect(page.GetByText("Profil byl úspěšně uložen.")).ToBeVisibleAsync(new() { Timeout = 10_000 });

            var profile = await GetProfileAsync(user);
            profile.Username.Should().Be(newUsername);
            profile.AvatarUrl.Should().StartWith("data:image/png;base64,");

            await PrepareForFullPageSettingsScreenshotAsync(page);
            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "settings",
                scenario: "profile-username-avatar-validation",
                state: "profile-saved",
                viewport: "1366x900",
                theme: "light",
                persona: "settingsUser");

            var invalidType = await CreateTempFileAsync("avatar.txt", "nejsem obrazek"u8.ToArray());
            await page.GetByTestId(Selectors.Settings.AvatarFileInput).SetInputFilesAsync(invalidType);
            await Expect(page.GetByTestId(Selectors.Settings.AvatarError)).ToContainTextAsync("PNG, JPG nebo WebP");

            var oversizedAvatar = await CreateTempFileAsync("avatar-large.png", new byte[600 * 1024]);
            await page.GetByTestId(Selectors.Settings.AvatarFileInput).SetInputFilesAsync(oversizedAvatar);
            await Expect(page.GetByTestId(Selectors.Settings.AvatarError)).ToContainTextAsync("512 KB");

            await page.GetByTestId(Selectors.Settings.UsernameInput).Locator("input").FillAsync(existing.Username);
            await ClickButtonInAsync(page.GetByTestId(Selectors.Settings.SaveProfile));
            await Expect(page.GetByText("Uživatelské jméno je už obsazené.")).ToBeVisibleAsync(new() { Timeout = 10_000 });

            profile = await GetProfileAsync(user);
            profile.Username.Should().Be(newUsername);
        });
    }

    [Fact]
    public async Task Settings_PasswordChangeAndWrongCurrentPassword_WorkEndToEnd()
    {
        await RunScenarioAsync("settings", "password-change-and-wrong-current", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("password");
            var newPassword = "NoveHeslo123!";

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/settings");

            await FillPasswordFormAsync(page, "SpatneHeslo123!", newPassword);
            await ClickButtonInAsync(page.GetByTestId(Selectors.Settings.ChangePassword));
            await Expect(page.GetByTestId(Selectors.Settings.PasswordStatus))
                .ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Settings.PasswordStatus))
                .ToContainTextAsync("Změna hesla se nezdařila. Zkontrolujte aktuální heslo.");

            await PrepareForFullPageSettingsScreenshotAsync(page);
            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "settings",
                scenario: "password-change-and-wrong-current",
                state: "wrong-current-password",
                viewport: "1366x900",
                theme: "light",
                persona: "settingsUser");

            await FillPasswordFormAsync(page, user.Password, newPassword);
            await ClickButtonInAsync(page.GetByTestId(Selectors.Settings.ChangePassword));
            await Expect(page.GetByTestId(Selectors.Settings.PasswordStatus))
                .ToContainTextAsync("Heslo bylo úspěšně změněno.", new() { Timeout = 10_000 });

            using var httpClient = new HttpClient { BaseAddress = new Uri($"{Fixture.ApiBaseUrl}/") };
            using var oldLogin = await httpClient.PostAsJsonAsync("api/v1/users/login", new LoginRequest
            {
                Email = user.Email,
                Password = user.Password
            });
            oldLogin.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            using var newLogin = await httpClient.PostAsJsonAsync("api/v1/users/login", new LoginRequest
            {
                Email = user.Email,
                Password = newPassword
            });
            newLogin.StatusCode.Should().Be(HttpStatusCode.OK);
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Settings_PreferencesThemeLanguageNotificationsAndPrivacy_PersistToProfile()
    {
        await RunScenarioAsync("settings", "preferences-theme-language-privacy", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("prefs");

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/settings");

            await Expect(page.GetByRole(AriaRole.Heading, new() { NameRegex = new Regex("^Nastavení$") })).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Settings.PreferencesSection)).ToBeVisibleAsync();

            await SetToggleAsync(page, Selectors.Settings.PushNotificationsToggle, false);
            await SetToggleAsync(page, Selectors.Settings.EmailNotificationsToggle, false);
            await SetToggleAsync(page, Selectors.Settings.LeagueUpdatesToggle, false);
            await SetToggleAsync(page, Selectors.Settings.AchievementNotificationsToggle, false);
            await SetToggleAsync(page, Selectors.Settings.DailyChallengeReminderToggle, false);
            await SetToggleAsync(page, Selectors.Settings.AnimationsToggle, false);
            await SetToggleAsync(page, Selectors.Settings.SoundsToggle, false);
            await page.GetByTestId(Selectors.Settings.StreakReminderTimeInput).Locator("input").FillAsync("18:45");

            await SelectRadioAsync(page, Selectors.Settings.ThemeDark);
            await ClickButtonInAsync(page.GetByTestId(Selectors.Settings.SavePreferences));
            await Expect(page.GetByText("Předvolby byly uloženy.")).ToBeVisibleAsync(new() { Timeout = 10_000 });

            var profile = await GetProfileAsync(user);
            profile.Preferences.Theme.Should().Be(AppTheme.Dark);
            profile.Preferences.Language.Should().Be("cs");
            profile.Preferences.PushNotificationsEnabled.Should().BeFalse();
            profile.Preferences.EmailNotificationsEnabled.Should().BeFalse();
            profile.Preferences.LeagueUpdatesEnabled.Should().BeFalse();
            profile.Preferences.AchievementNotificationsEnabled.Should().BeFalse();
            profile.Preferences.DailyChallengeReminderEnabled.Should().BeFalse();
            profile.Preferences.AnimationsEnabled.Should().BeFalse();
            profile.Preferences.SoundsEnabled.Should().BeFalse();
            profile.Preferences.StreakReminderTime.Should().Be(TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(45)));

            await SelectRadioAsync(page, Selectors.Settings.ThemeAuto);
            await ClickButtonInAsync(page.GetByTestId(Selectors.Settings.SavePreferences));
            profile = await GetProfileAsync(user);
            profile.Preferences.Theme.Should().Be(AppTheme.Auto);

            await page.GetByTestId(Selectors.Settings.LanguageSelect).Locator("select").SelectOptionAsync("cs");
            await Expect(page.GetByRole(AriaRole.Heading, new() { NameRegex = new Regex("^Nastavení$") })).ToBeVisibleAsync();
            await Expect(page.GetByText("Settings_")).Not.ToBeVisibleAsync();

            await SelectRadioAsync(page, Selectors.Settings.VisibilityPublic);
            await ClickButtonInAsync(page.GetByTestId(Selectors.Settings.SavePrivacy));
            profile = await GetProfileAsync(user);
            profile.Privacy.ProfileVisibility.Should().Be(ProfileVisibility.Public);

            await SelectRadioAsync(page, Selectors.Settings.VisibilityFriends);
            await ClickButtonInAsync(page.GetByTestId(Selectors.Settings.SavePrivacy));
            profile = await GetProfileAsync(user);
            profile.Privacy.ProfileVisibility.Should().Be(ProfileVisibility.Friends);

            await SelectRadioAsync(page, Selectors.Settings.VisibilityPrivate);
            await SetToggleAsync(page, Selectors.Settings.LeaderboardVisibilityToggle, false);
            await SetToggleAsync(page, Selectors.Settings.StatsSharingToggle, false);
            await ClickButtonInAsync(page.GetByTestId(Selectors.Settings.SavePrivacy));
            await Expect(page.GetByText("Nastavení soukromí bylo uloženo.").First).ToBeVisibleAsync(new() { Timeout = 10_000 });

            profile = await GetProfileAsync(user);
            profile.Privacy.ProfileVisibility.Should().Be(ProfileVisibility.Private);
            profile.Privacy.LeaderboardVisible.Should().BeFalse();
            profile.Privacy.StatsSharingEnabled.Should().BeFalse();

            await PrepareForFullPageSettingsScreenshotAsync(page);
            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "settings",
                scenario: "preferences-theme-language-privacy",
                state: "saved",
                viewport: "1366x900",
                theme: "light",
                persona: "settingsUser");
        });
    }

    [Fact]
    public async Task Settings_DangerZone_LogoutDeactivateAndDeleteRequireConfirmation()
    {
        await RunScenarioAsync("settings", "danger-zone-logout-deactivate-delete", async page =>
        {
            var logoutUser = await Fixture.RegisterUniqueUserAsync("logoutsettings");
            await Fixture.LoginAsAsync(page, logoutUser);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/settings");

            await ClickButtonInAsync(page.GetByTestId(Selectors.Settings.Logout));
            await page.WaitForURLAsync("**/login");
            var tokens = await ReadStoredTokensAsync(page);
            tokens.Should().OnlyContain(token => string.IsNullOrWhiteSpace(token));

            var deactivateUser = await Fixture.RegisterUniqueUserAsync("deactivate");
            await Fixture.LoginAsAsync(page, deactivateUser);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/settings");
            await ClickButtonInAsync(page.GetByTestId(Selectors.Settings.Deactivate));
            await Expect(page.GetByTestId(Selectors.Settings.ConfirmModal)).ToBeVisibleAsync();
            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "settings",
                scenario: "danger-zone-logout-deactivate-delete",
                state: "deactivate-confirm",
                viewport: "1366x900",
                theme: "light",
                persona: "settingsUser",
                fullPage: false);
            await ClickButtonInAsync(page.GetByTestId(Selectors.Settings.ConfirmSecondary));
            await Expect(page.GetByTestId(Selectors.Settings.ConfirmModal)).Not.ToBeVisibleAsync();

            await ClickButtonInAsync(page.GetByTestId(Selectors.Settings.Deactivate));
            await page.GetByTestId(Selectors.Settings.ConfirmInput).Locator("input").FillAsync("DEAKTIVOVAT");
            await ClickButtonInAsync(page.GetByTestId(Selectors.Settings.ConfirmPrimary));
            await page.WaitForURLAsync("**/login");
            await AssertLoginStatusAsync(deactivateUser, HttpStatusCode.Locked);

            var deleteUser = await Fixture.RegisterUniqueUserAsync("delete");
            await Fixture.LoginAsAsync(page, deleteUser);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/settings");
            await ClickButtonInAsync(page.GetByTestId(Selectors.Settings.Delete));
            await Expect(page.GetByTestId(Selectors.Settings.ConfirmModal)).ToBeVisibleAsync();
            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "settings",
                scenario: "danger-zone-logout-deactivate-delete",
                state: "delete-confirm",
                viewport: "1366x900",
                theme: "light",
                persona: "settingsUser",
                fullPage: false);
            await page.GetByTestId(Selectors.Settings.ConfirmInput).Locator("input").FillAsync("SMAZAT");
            await ClickButtonInAsync(page.GetByTestId(Selectors.Settings.ConfirmPrimary));
            await page.WaitForURLAsync("**/login");
            await AssertLoginStatusAsync(deleteUser, HttpStatusCode.Unauthorized);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "settings",
                scenario: "danger-zone-logout-deactivate-delete",
                state: "login-after-delete",
                viewport: "1366x900",
                theme: "light",
                persona: "settingsUser");
        }, assertNoFailedRequests: false);
    }

    private async Task<UserProfileDto> GetProfileAsync(TestUser user)
    {
        using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
        using var response = await apiClient.GetAsync("api/v1/users/me");
        response.EnsureSuccessStatusCode();

        var profile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        profile.Should().NotBeNull();
        return profile!;
    }

    private static async Task FillPasswordFormAsync(IPage page, string currentPassword, string newPassword)
    {
        await page.GetByTestId(Selectors.Settings.CurrentPasswordInput).Locator("input").FillAsync(currentPassword);
        await page.GetByTestId(Selectors.Settings.NewPasswordInput).Locator("input").FillAsync(newPassword);
        await page.GetByTestId(Selectors.Settings.ConfirmPasswordInput).Locator("input").FillAsync(newPassword);
    }

    private static async Task SetToggleAsync(IPage page, string testId, bool enabled)
    {
        var toggle = page.GetByTestId(testId).Locator("input[type='checkbox'], button[role='switch'], button").First;
        var current = await IsToggleCheckedAsync(toggle);

        if (current != enabled)
        {
            await toggle.ClickAsync();
        }

        current = await IsToggleCheckedAsync(toggle);
        current.Should().Be(enabled, $"toggle {testId} should be {(enabled ? "enabled" : "disabled")}");
    }

    private static async Task<bool> IsToggleCheckedAsync(ILocator toggle)
    {
        var ariaChecked = await toggle.GetAttributeAsync("aria-checked");
        if (bool.TryParse(ariaChecked, out var fromAria))
        {
            return fromAria;
        }

        var checkedAttribute = await toggle.GetAttributeAsync("checked");
        if (checkedAttribute is not null)
        {
            return true;
        }

        return await toggle.IsCheckedAsync();
    }

    private static async Task SelectRadioAsync(IPage page, string testId)
    {
        var radio = page.GetByTestId(testId).Locator("input[type='radio'], button").First;
        await radio.ClickAsync();
    }

    private static async Task ClickButtonInAsync(ILocator locator)
    {
        await locator.GetByRole(AriaRole.Button).ClickAsync();
    }

    private static async Task PrepareForFullPageSettingsScreenshotAsync(IPage page)
    {
        await page.EvaluateAsync(
            """
            () => {
                document
                    .querySelectorAll('.tm-toast button, .tm-toast-close, [aria-label="Close"], [aria-label="Zavřít"]')
                    .forEach(button => button.click());
                window.scrollTo(0, 0);
            }
            """);
        await page.WaitForTimeoutAsync(250);
    }

    private static async Task<string> CreateTempFileAsync(string fileName, byte[] content)
    {
        var directory = Path.Combine(Path.GetTempPath(), "lexiquest-e2e", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, fileName);
        await File.WriteAllBytesAsync(path, content);
        return path;
    }

    private static async Task<string[]> ReadStoredTokensAsync(IPage page)
    {
        return await page.EvaluateAsync<string[]>(
            """
            () => [
                window.localStorage.getItem('access_token') || '',
                window.localStorage.getItem('refresh_token') || ''
            ]
            """);
    }

    private async Task AssertLoginStatusAsync(TestUser user, HttpStatusCode expectedStatusCode)
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri($"{Fixture.ApiBaseUrl}/") };
        using var response = await httpClient.PostAsJsonAsync("api/v1/users/login", new LoginRequest
        {
            Email = user.Email,
            Password = user.Password
        });

        response.StatusCode.Should().Be(expectedStatusCode);
    }
}
