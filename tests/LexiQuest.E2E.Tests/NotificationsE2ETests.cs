using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using LexiQuest.E2E.Tests.Infrastructure;
using LexiQuest.Shared.DTOs.Notifications;
using LexiQuest.Shared.Enums;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Full")]
[Trait("Category", "Visual")]
[Collection(E2ECollection.Name)]
public class NotificationsE2ETests : E2ETestBase
{
    public NotificationsE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Notifications_BellDropdownGroupingAndReadActions_WorkEndToEnd()
    {
        await RunScenarioAsync("notifications", "bell-dropdown-read-actions", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("notify");
            var today = DateTime.UtcNow;
            var yesterday = DateTime.UtcNow.Date.AddDays(-1).AddHours(10);
            var thisWeek = DateTime.UtcNow.Date.AddDays(-3).AddHours(9);
            var older = DateTime.UtcNow.Date.AddDays(-10).AddHours(8);

            await Fixture.SeedNotificationAsync(
                user.Email,
                NotificationType.DailyChallenge,
                "Dnešní výzva",
                "Nová denní výzva čeká na odehrání.",
                NotificationSeverity.Info,
                createdAtUtc: today.AddMinutes(-5));
            await Fixture.SeedNotificationAsync(
                user.Email,
                NotificationType.StreakWarning,
                "Pozor na streak",
                "Včera jsi dostal upozornění na streak.",
                NotificationSeverity.Warning,
                createdAtUtc: yesterday);
            await Fixture.SeedNotificationAsync(
                user.Email,
                NotificationType.AchievementUnlocked,
                "Nový úspěch",
                "Tento týden se odemkl nový úspěch.",
                NotificationSeverity.Success,
                createdAtUtc: thisWeek);
            await Fixture.SeedNotificationAsync(
                user.Email,
                NotificationType.SystemMessage,
                "Starší zpráva",
                "Tahle zpráva už byla přečtená.",
                NotificationSeverity.Info,
                isRead: true,
                createdAtUtc: older);

            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            (await GetUnreadCountAsync(apiClient)).Should().Be(3);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dashboard");

            await Expect(page.GetByTestId(Selectors.Notifications.Bell)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Notifications.UnreadBadge)).ToContainTextAsync("3");

            await page.GetByTestId(Selectors.Notifications.BellButton).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Notifications.Dropdown)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Notifications.Item)).ToHaveCountAsync(4);

            var groupLabels = await page.GetByTestId(Selectors.Notifications.GroupLabel).AllTextContentsAsync();
            groupLabels.Should().Contain(["Dnes", "Včera", "Tento týden", "Starší"]);
            await Expect(page.GetByTestId(Selectors.Notifications.Item).Filter(new() { HasText = "Dnešní výzva" })).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Notifications.Item).Filter(new() { HasText = "Pozor na streak" })).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Notifications.Item).Filter(new() { HasText = "Nový úspěch" })).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Notifications.Item).Filter(new() { HasText = "Starší zpráva" })).ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "notifications",
                scenario: "bell-dropdown-read-actions",
                state: "grouped-unread",
                viewport: "1366x900",
                theme: "light",
                persona: "standardUser");

            var todayItem = page.GetByTestId(Selectors.Notifications.Item).Filter(new() { HasText = "Dnešní výzva" });
            await todayItem.ClickAsync();
            await Expect(page.GetByTestId(Selectors.Notifications.UnreadBadge)).ToContainTextAsync("2");
            await Expect(todayItem).ToHaveAttributeAsync("data-notification-read", "true");
            (await GetUnreadCountAsync(apiClient)).Should().Be(2);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "notifications",
                scenario: "bell-dropdown-read-actions",
                state: "after-mark-one-read",
                viewport: "1366x900",
                theme: "light",
                persona: "standardUser");

            await page.GetByTestId(Selectors.Notifications.MarkAllRead).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Notifications.UnreadBadge)).ToHaveCountAsync(0);
            (await GetUnreadCountAsync(apiClient)).Should().Be(0);

            var notifications = await GetNotificationsAsync(apiClient);
            notifications.Should().OnlyContain(notification => notification.IsRead);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "notifications",
                scenario: "bell-dropdown-read-actions",
                state: "after-mark-all-read",
                viewport: "1366x900",
                theme: "light",
                persona: "standardUser");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Notifications_PreferencesLoadAndSave_WorkEndToEnd()
    {
        await RunScenarioAsync("notifications", "preferences-load-save", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("notifyprefs");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);

            var initialPreferences = new UpdatePreferencesRequest(
                PushEnabled: false,
                EmailEnabled: false,
                StreakReminder: true,
                StreakReminderTime: TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(30)),
                LeagueUpdates: false,
                AchievementNotifications: false,
                DailyChallengeReminder: false);
            await UpdateNotificationPreferencesAsync(apiClient, initialPreferences);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/settings");
            await Expect(page.GetByTestId(Selectors.Settings.PreferencesSection)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await ExpectToggleAsync(page, Selectors.Settings.PushNotificationsToggle, false);
            await ExpectToggleAsync(page, Selectors.Settings.EmailNotificationsToggle, false);
            await ExpectToggleAsync(page, Selectors.Settings.LeagueUpdatesToggle, false);
            await ExpectToggleAsync(page, Selectors.Settings.AchievementNotificationsToggle, false);
            await ExpectToggleAsync(page, Selectors.Settings.DailyChallengeReminderToggle, false);
            await Expect(page.GetByTestId(Selectors.Settings.StreakReminderTimeInput).Locator("input")).ToHaveValueAsync("19:30");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "notifications",
                scenario: "preferences-load-save",
                state: "loaded",
                viewport: "1366x900",
                theme: "light",
                persona: "standardUser");

            var preferencesPushEndpoint = $"https://push.lexiquest.test/preferences/{Guid.NewGuid():N}";
            await InstallPushSubscriptionStubAsync(page, preferencesPushEndpoint);
            await page.GetByTestId(Selectors.Settings.PushNotificationsToggle).Locator("input").ClickAsync();
            await WaitForPushSubscriptionRequestAsync(page);
            (await WaitForPushSubscriptionCountAsync(user.Email, preferencesPushEndpoint))
                .Should().Be(1, "ulozeni preferenci se zapnutymi push notifikacemi musi mit platnou subscription");

            await SetToggleAsync(page, Selectors.Settings.EmailNotificationsToggle, true);
            await SetToggleAsync(page, Selectors.Settings.LeagueUpdatesToggle, true);
            await SetToggleAsync(page, Selectors.Settings.AchievementNotificationsToggle, true);
            await SetToggleAsync(page, Selectors.Settings.DailyChallengeReminderToggle, true);
            await page.GetByTestId(Selectors.Settings.StreakReminderTimeInput).Locator("input").FillAsync("06:15");

            await ClickButtonInAsync(page.GetByTestId(Selectors.Settings.SavePreferences));
            await Expect(page.GetByText("Předvolby byly uloženy.")).ToBeVisibleAsync(new() { Timeout = 10_000 });

            var saved = await GetNotificationPreferencesAsync(apiClient);
            saved.PushEnabled.Should().BeTrue();
            saved.EmailEnabled.Should().BeTrue();
            saved.StreakReminder.Should().BeTrue();
            saved.StreakReminderTime.Should().Be(TimeSpan.FromHours(6).Add(TimeSpan.FromMinutes(15)));
            saved.LeagueUpdates.Should().BeTrue();
            saved.AchievementNotifications.Should().BeTrue();
            saved.DailyChallengeReminder.Should().BeTrue();

            await PrepareSettingsScreenshotAsync(page);
            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "notifications",
                scenario: "preferences-load-save",
                state: "saved",
                viewport: "1366x900",
                theme: "light",
                persona: "standardUser");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Notifications_PushEnable_RequestsPermissionAndStoresSubscription()
    {
        await RunScenarioAsync("notifications", "push-permission-enable", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("notifypush");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            var endpoint = $"https://push.lexiquest.test/{Guid.NewGuid():N}";

            await UpdateNotificationPreferencesAsync(apiClient, new UpdatePreferencesRequest(
                PushEnabled: false,
                EmailEnabled: true,
                StreakReminder: true,
                StreakReminderTime: TimeSpan.FromHours(20),
                LeagueUpdates: true,
                AchievementNotifications: true,
                DailyChallengeReminder: true));

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/settings");
            await Expect(page.GetByTestId(Selectors.Settings.PreferencesSection)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await ExpectToggleAsync(page, Selectors.Settings.PushNotificationsToggle, false);

            await InstallPushSubscriptionStubAsync(page, endpoint);

            await page.GetByTestId(Selectors.Settings.PushNotificationsToggle).Locator("input").ClickAsync();
            await WaitForPushSubscriptionRequestAsync(page);

            var requestCount = await page.EvaluateAsync<int>(
                "() => window.__lexiQuestPushCalls?.requestCount ?? 0");
            requestCount.Should().Be(1, "zapnuti push notifikaci musi vyvolat browser permission/subscription tok");

            (await WaitForPushSubscriptionCountAsync(user.Email, endpoint))
                .Should().Be(1, "push subscription se musi ulozit do testovaci SQL databaze pres realny API endpoint");

            await ClickButtonInAsync(page.GetByTestId(Selectors.Settings.SavePreferences));
            await Expect(page.GetByText("Předvolby byly uloženy.")).ToBeVisibleAsync(new() { Timeout = 10_000 });

            var preferences = await GetNotificationPreferencesAsync(apiClient);
            preferences.PushEnabled.Should().BeTrue();

            await PrepareSettingsScreenshotAsync(page);
            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "notifications",
                scenario: "push-permission-enable",
                state: "enabled",
                viewport: "1366x900",
                theme: "light",
                persona: "standardUser");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Notifications_PushDisabled_DoesNotSendPushButKeepsInAppNotification()
    {
        await RunScenarioAsync("notifications", "push-disabled-respects-preference", async page =>
        {
            await using var pushServer = PushEndpointServer.Start();
            var user = await Fixture.RegisterUniqueUserAsync("notifypushoff");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);

            await SavePushSubscriptionAsync(apiClient, new PushSubscriptionDto(
                pushServer.Url,
                "e2e-p256dh-key",
                "e2e-auth-secret"));

            await UpdateNotificationPreferencesAsync(apiClient, new UpdatePreferencesRequest(
                PushEnabled: false,
                EmailEnabled: false,
                StreakReminder: true,
                StreakReminderTime: TimeSpan.FromHours(20),
                LeagueUpdates: true,
                AchievementNotifications: true,
                DailyChallengeReminder: true));

            await SendE2ENotificationAsync(
                user.Email,
                NotificationType.DailyChallenge,
                "Push vypnutý",
                "Tahle notifikace má zůstat jen v aplikaci.",
                NotificationSeverity.Info,
                "/daily-challenge");

            (await pushServer.WaitForRequestCountAsync(1, TimeSpan.FromMilliseconds(700)))
                .Should().BeFalse("PushEnabled=false nesmi odeslat Web Push POST ani pri ulozene subscription");
            pushServer.RequestCount.Should().Be(0);

            var notifications = await GetNotificationsAsync(apiClient);
            notifications.Should().Contain(notification =>
                notification.Title == "Push vypnutý"
                && notification.Type == NotificationType.DailyChallenge
                && !notification.IsRead);
            (await GetUnreadCountAsync(apiClient)).Should().Be(1);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dashboard");
            await Expect(page.GetByTestId(Selectors.Notifications.UnreadBadge)).ToContainTextAsync("1");
            await page.GetByTestId(Selectors.Notifications.BellButton).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Notifications.Item).Filter(new() { HasText = "Push vypnutý" }))
                .ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "notifications",
                scenario: "push-disabled-respects-preference",
                state: "in-app-only",
                viewport: "1366x900",
                theme: "light",
                persona: "standardUser");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Notifications_EmailDisabled_DoesNotSendEmailButKeepsInAppNotification()
    {
        await RunScenarioAsync("notifications", "email-disabled-respects-preference", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("notifyemailoff");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            await Fixture.Smtp4Dev.ClearMessagesAsync();

            await UpdateNotificationPreferencesAsync(apiClient, new UpdatePreferencesRequest(
                PushEnabled: false,
                EmailEnabled: false,
                StreakReminder: true,
                StreakReminderTime: TimeSpan.FromHours(20),
                LeagueUpdates: true,
                AchievementNotifications: true,
                DailyChallengeReminder: true));

            await SendE2ENotificationAsync(
                user.Email,
                NotificationType.SystemMessage,
                "Email vypnutý",
                "Tahle notifikace nesmí odejít emailem.",
                NotificationSeverity.Info,
                "/dashboard");

            await Task.Delay(700);
            var messages = await Fixture.Smtp4Dev.GetMessagesAsync();
            messages.GetRawText().Should().NotContain("Email vypnutý");
            messages.GetRawText().Should().NotContain(user.Email, "EmailEnabled=false nesmi poslat notifikacni email do smtp4dev");

            var notifications = await GetNotificationsAsync(apiClient);
            notifications.Should().Contain(notification =>
                notification.Title == "Email vypnutý"
                && notification.Type == NotificationType.SystemMessage
                && !notification.IsRead);

            await LoginThroughUiAsync(page, user);
            await WaitForBrowserUnreadCountAsync(page, expected: 1);
            await Expect(page.GetByTestId(Selectors.Notifications.UnreadBadge)).ToContainTextAsync("1");
            await page.GetByTestId(Selectors.Notifications.BellButton).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Notifications.Item).Filter(new() { HasText = "Email vypnutý" }))
                .ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "notifications",
                scenario: "email-disabled-respects-preference",
                state: "in-app-only",
                viewport: "1366x900",
                theme: "light",
                persona: "standardUser");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Notifications_StreakWarningJob_SendsEmailPushAndInAppNotification()
    {
        await RunScenarioAsync("notifications", "streak-warning-email-push-in-app", async page =>
        {
            await using var pushServer = PushEndpointServer.Start();
            var user = await Fixture.RegisterUniqueUserAsync("notifystreak");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            await Fixture.Smtp4Dev.ClearMessagesAsync();

            await SavePushSubscriptionAsync(apiClient, new PushSubscriptionDto(
                pushServer.Url,
                "e2e-p256dh-key",
                "e2e-auth-secret"));

            await UpdateNotificationPreferencesAsync(apiClient, new UpdatePreferencesRequest(
                PushEnabled: true,
                EmailEnabled: true,
                StreakReminder: true,
                StreakReminderTime: TimeSpan.FromHours(20),
                LeagueUpdates: false,
                AchievementNotifications: false,
                DailyChallengeReminder: false));

            await Fixture.ForceUserStreakAsync(
                user.Email,
                currentDays: 5,
                longestDays: 5,
                lastActivityUtc: DateTime.UtcNow.Date.AddDays(-1).AddHours(12));

            await RunStreakReminderJobAsync();

            (await pushServer.WaitForRequestCountAsync(1, TimeSpan.FromSeconds(5)))
                .Should().BeTrue("streak warning s PushEnabled=true musi odeslat Web Push POST");
            var pushRequest = pushServer.Requests.Should().ContainSingle().Which;
            pushRequest.Method.Should().Be("POST");
            using var pushPayload = JsonDocument.Parse(pushRequest.Body);
            pushPayload.RootElement.GetProperty("title").GetString().Should().Be("Streak je v ohrožení");
            pushPayload.RootElement.GetProperty("body").GetString().Should().Contain("Dnes ještě nemáte splněnou denní výzvu.");

            var emailText = await Fixture.Smtp4Dev.WaitForMessageTextAsync(
                text => text.Contains(user.Email, StringComparison.OrdinalIgnoreCase)
                    && text.Contains("Streak je v ohrožení", StringComparison.Ordinal)
                    && text.Contains("Dnes ještě", StringComparison.Ordinal),
                TimeSpan.FromSeconds(10));
            emailText.Should().Contain(user.Email);
            emailText.Should().Contain("Streak je v ohrožení");

            var notifications = await GetNotificationsAsync(apiClient);
            notifications.Should().Contain(notification =>
                notification.Type == NotificationType.StreakWarning
                && notification.Title == "Streak je v ohrožení"
                && notification.Message.Contains("Dnes ještě nemáte splněnou denní výzvu", StringComparison.Ordinal)
                && !notification.IsRead);
            (await GetUnreadCountAsync(apiClient)).Should().Be(1);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dashboard");
            await Expect(page.GetByTestId(Selectors.Notifications.UnreadBadge)).ToContainTextAsync("1");
            await page.GetByTestId(Selectors.Notifications.BellButton).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Notifications.Item).Filter(new() { HasText = "Streak je v ohrožení" }))
                .ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "notifications",
                scenario: "streak-warning-email-push-in-app",
                state: "all-channels",
                viewport: "1366x900",
                theme: "light",
                persona: "standardUser");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Notifications_DailyChallengeReminderJob_SendsEmailPushAndInAppNotification()
    {
        await RunScenarioAsync("notifications", "daily-challenge-reminder-email-push-in-app", async page =>
        {
            await using var pushServer = PushEndpointServer.Start();
            var user = await Fixture.RegisterUniqueUserAsync("notifydaily");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            await Fixture.Smtp4Dev.ClearMessagesAsync();

            await SavePushSubscriptionAsync(apiClient, new PushSubscriptionDto(
                pushServer.Url,
                "e2e-p256dh-key",
                "e2e-auth-secret"));

            await UpdateNotificationPreferencesAsync(apiClient, new UpdatePreferencesRequest(
                PushEnabled: true,
                EmailEnabled: true,
                StreakReminder: false,
                StreakReminderTime: TimeSpan.FromHours(20),
                LeagueUpdates: false,
                AchievementNotifications: false,
                DailyChallengeReminder: true));

            await RunDailyReminderJobAsync();

            (await pushServer.WaitForRequestCountAsync(1, TimeSpan.FromSeconds(5)))
                .Should().BeTrue("daily reminder s PushEnabled=true musi odeslat Web Push POST");
            var pushRequest = pushServer.Requests.Should().ContainSingle().Which;
            pushRequest.Method.Should().Be("POST");
            using var pushPayload = JsonDocument.Parse(pushRequest.Body);
            pushPayload.RootElement.GetProperty("title").GetString().Should().Be("Denní výzva je připravena");
            pushPayload.RootElement.GetProperty("body").GetString().Should().Contain("Nová denní výzva čeká na odehrání.");

            var emailText = await Fixture.Smtp4Dev.WaitForMessageTextAsync(
                text => text.Contains(user.Email, StringComparison.OrdinalIgnoreCase)
                    && text.Contains("Denní výzva je připravena", StringComparison.Ordinal)
                    && text.Contains("Nová denní výzva", StringComparison.Ordinal),
                TimeSpan.FromSeconds(10));
            emailText.Should().Contain(user.Email);
            emailText.Should().Contain("Denní výzva je připravena");

            var notifications = await GetNotificationsAsync(apiClient);
            notifications.Should().Contain(notification =>
                notification.Type == NotificationType.DailyChallenge
                && notification.Title == "Denní výzva je připravena"
                && notification.Message.Contains("Nová denní výzva čeká na odehrání", StringComparison.Ordinal)
                && notification.ActionUrl == "/daily-challenge"
                && !notification.IsRead);
            (await GetUnreadCountAsync(apiClient)).Should().Be(1);

            await LoginThroughUiAsync(page, user);
            await Expect(page.GetByTestId(Selectors.Notifications.UnreadBadge)).ToContainTextAsync("1");
            await page.GetByTestId(Selectors.Notifications.BellButton).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Notifications.Item).Filter(new() { HasText = "Denní výzva je připravena" }))
                .ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "notifications",
                scenario: "daily-challenge-reminder-email-push-in-app",
                state: "all-channels",
                viewport: "1366x900",
                theme: "light",
                persona: "standardUser");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Notifications_AchievementUnlocked_ShowsToastAndInAppNotification()
    {
        await RunScenarioAsync("notifications", "achievement-unlocked-toast-notification", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("notifyachievement");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);

            await UpdateNotificationPreferencesAsync(apiClient, new UpdatePreferencesRequest(
                PushEnabled: false,
                EmailEnabled: false,
                StreakReminder: false,
                StreakReminderTime: TimeSpan.FromHours(20),
                LeagueUpdates: false,
                AchievementNotifications: true,
                DailyChallengeReminder: false));

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/game");
            await page.GetByTestId(Selectors.Game.ModeTraining).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Game.Arena)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            var scrambled = NormalizeLetters(await page.GetByTestId(Selectors.Game.ScrambledWord).TextContentAsync());
            var answer = await Fixture.GetBeginnerOriginalForScrambledWordAsync(scrambled);

            await page.GetByTestId(Selectors.Game.AnswerInput).FillAsync(answer);
            await page.GetByTestId(Selectors.Game.Submit).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Achievements.UnlockModal)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Achievements.UnlockModal)).ToContainTextAsync("První slovo");
            await Expect(page.GetByText("Úspěch odemčen: První slovo")).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "notifications",
                scenario: "achievement-unlocked-toast-notification",
                state: "toast-and-modal",
                viewport: "1366x900",
                theme: "light",
                persona: "firstAchievementUser");

            var notifications = await GetNotificationsAsync(apiClient);
            notifications.Should().Contain(notification =>
                notification.Type == NotificationType.AchievementUnlocked
                && notification.Title == "Úspěch odemčen"
                && notification.Message.Contains("První slovo", StringComparison.Ordinal)
                && notification.ActionUrl == "/achievements"
                && !notification.IsRead);
            (await GetUnreadCountAsync(apiClient)).Should().Be(1);

            await page.GetByTestId(Selectors.Achievements.UnlockModalContinue).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Notifications.UnreadBadge)).ToContainTextAsync("1");
            await page.GetByTestId(Selectors.Notifications.BellButton).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Notifications.Item).Filter(new() { HasText = "Úspěch odemčen" }))
                .ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "notifications",
                scenario: "achievement-unlocked-toast-notification",
                state: "notification-dropdown",
                viewport: "1366x900",
                theme: "light",
                persona: "firstAchievementUser");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Notifications_FrequencyLimit_MaxFivePerHour_SuppressesSixthDelivery()
    {
        await RunScenarioAsync("notifications", "frequency-limit-max-five-per-hour", async page =>
        {
            await using var pushServer = PushEndpointServer.Start();
            var user = await Fixture.RegisterUniqueUserAsync("notifylimit");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            await Fixture.Smtp4Dev.ClearMessagesAsync();

            await SavePushSubscriptionAsync(apiClient, new PushSubscriptionDto(
                pushServer.Url,
                "e2e-p256dh-key",
                "e2e-auth-secret"));

            await UpdateNotificationPreferencesAsync(apiClient, new UpdatePreferencesRequest(
                PushEnabled: true,
                EmailEnabled: true,
                StreakReminder: false,
                StreakReminderTime: TimeSpan.FromHours(20),
                LeagueUpdates: true,
                AchievementNotifications: false,
                DailyChallengeReminder: false));

            for (var i = 1; i <= 6; i++)
            {
                await SendE2ENotificationAsync(
                    user.Email,
                    NotificationType.LeagueUpdate,
                    $"Limit liga {i}",
                    $"Test frekvencniho limitu {i}.",
                    NotificationSeverity.Info,
                    "/leagues");
            }

            (await pushServer.WaitForRequestCountAsync(5, TimeSpan.FromSeconds(5)))
                .Should().BeTrue("prvnich pet league notifikaci v hodine ma projit do push kanalu");
            (await pushServer.WaitForRequestCountAsync(6, TimeSpan.FromMilliseconds(700)))
                .Should().BeFalse("sesta league notifikace v hodine uz nesmi odeslat push");
            pushServer.RequestCount.Should().Be(5);

            var emailText = await Fixture.Smtp4Dev.WaitForMessageTextAsync(
                text => text.Contains(user.Email, StringComparison.OrdinalIgnoreCase)
                    && text.Contains("Limit liga 5", StringComparison.Ordinal),
                TimeSpan.FromSeconds(10));
            emailText.Should().Contain("Limit liga 5");
            (await Fixture.Smtp4Dev.GetMessagesAsync()).GetRawText()
                .Should().NotContain("Limit liga 6", "sesta notifikace se nema dostat ani do emailu");

            var notifications = await GetNotificationsAsync(apiClient);
            var limitedNotifications = notifications
                .Where(notification => notification.Title.StartsWith("Limit liga ", StringComparison.Ordinal))
                .ToArray();
            limitedNotifications.Should().HaveCount(5);
            limitedNotifications.Should().Contain(notification => notification.Title == "Limit liga 1");
            limitedNotifications.Should().Contain(notification => notification.Title == "Limit liga 5");
            limitedNotifications.Should().NotContain(notification => notification.Title == "Limit liga 6");
            (await GetUnreadCountAsync(apiClient)).Should().Be(5);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dashboard");
            await Expect(page.GetByTestId(Selectors.Notifications.UnreadBadge)).ToContainTextAsync("5");
            await page.GetByTestId(Selectors.Notifications.BellButton).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Notifications.Item).Filter(new() { HasText = "Limit liga" }))
                .ToHaveCountAsync(5);
            await Expect(page.GetByTestId(Selectors.Notifications.Item).Filter(new() { HasText = "Limit liga 6" }))
                .ToHaveCountAsync(0);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "notifications",
                scenario: "frequency-limit-max-five-per-hour",
                state: "five-notifications-only",
                viewport: "1366x900",
                theme: "light",
                persona: "standardUser");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    private async Task<int> WaitForPushSubscriptionCountAsync(string email, string endpoint)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        int count;
        do
        {
            count = await Fixture.GetPushSubscriptionCountAsync(email, endpoint);
            if (count > 0)
            {
                return count;
            }

            await Task.Delay(250);
        }
        while (DateTime.UtcNow < deadline);

        return count;
    }

    private static async Task InstallPushSubscriptionStubAsync(IPage page, string endpoint)
    {
        await page.EvaluateAsync(
            """
            endpoint => {
                window.__lexiQuestPushCalls = { requestCount: 0 };
                window.lexiQuestPush = {
                    requestSubscription: async () => {
                        window.__lexiQuestPushCalls.requestCount += 1;
                        return {
                            Endpoint: endpoint,
                            P256dh: 'e2e-p256dh-key',
                            Auth: 'e2e-auth-secret'
                        };
                    }
                };
            }
            """,
            endpoint);

        var isPushStubInstalled = await page.EvaluateAsync<bool>(
            "() => window.lexiQuestPush?.requestSubscription?.toString().includes('__lexiQuestPushCalls') === true");
        isPushStubInstalled.Should().BeTrue("E2E musi prepsat browser push helper deterministickym stubem");
    }

    private static async Task WaitForPushSubscriptionRequestAsync(IPage page)
    {
        await page.WaitForFunctionAsync(
            "() => window.__lexiQuestPushCalls?.requestCount === 1",
            null,
            new() { Timeout = 10_000 });
    }

    private static async Task<int> GetUnreadCountAsync(HttpClient apiClient)
    {
        using var response = await apiClient.GetAsync("api/v1/notifications/unread-count");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<int>();
    }

    private static async Task<List<NotificationDto>> GetNotificationsAsync(HttpClient apiClient)
    {
        using var response = await apiClient.GetAsync("api/v1/notifications");
        response.EnsureSuccessStatusCode();
        var notifications = await response.Content.ReadFromJsonAsync<List<NotificationDto>>();
        notifications.Should().NotBeNull();
        return notifications!;
    }

    private static async Task<NotificationPreferenceDto> GetNotificationPreferencesAsync(HttpClient apiClient)
    {
        using var response = await apiClient.GetAsync("api/v1/notifications/preferences");
        response.EnsureSuccessStatusCode();
        var preferences = await response.Content.ReadFromJsonAsync<NotificationPreferenceDto>();
        preferences.Should().NotBeNull();
        return preferences!;
    }

    private static async Task UpdateNotificationPreferencesAsync(HttpClient apiClient, UpdatePreferencesRequest request)
    {
        using var response = await apiClient.PutAsJsonAsync("api/v1/notifications/preferences", request);
        response.EnsureSuccessStatusCode();
    }

    private static async Task SavePushSubscriptionAsync(HttpClient apiClient, PushSubscriptionDto request)
    {
        using var response = await apiClient.PostAsJsonAsync("api/v1/notifications/push-subscription", request);
        response.EnsureSuccessStatusCode();
    }

    private async Task SendE2ENotificationAsync(
        string email,
        NotificationType type,
        string title,
        string message,
        NotificationSeverity severity,
        string? actionUrl)
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri($"{Fixture.ApiBaseUrl}/") };
        using var response = await httpClient.PostAsJsonAsync("api/v1/e2e/notifications/send", new
        {
            Email = email,
            Type = type,
            Title = title,
            Message = message,
            Severity = severity,
            ActionUrl = actionUrl
        });
        response.EnsureSuccessStatusCode();
    }

    private async Task RunStreakReminderJobAsync()
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri($"{Fixture.ApiBaseUrl}/") };
        using var response = await httpClient.PostAsync("api/v1/e2e/notifications/run-streak-reminders", null);
        response.EnsureSuccessStatusCode();
    }

    private async Task RunDailyReminderJobAsync()
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri($"{Fixture.ApiBaseUrl}/") };
        using var response = await httpClient.PostAsync("api/v1/e2e/notifications/run-daily-reminders", null);
        response.EnsureSuccessStatusCode();
    }

    private async Task LoginThroughUiAsync(IPage page, TestUser user)
    {
        await Fixture.GoToAndWaitForAppReadyAsync(page, "/login");
        await page.GetByLabel("Email").FillAsync(user.Email);
        await page.GetByLabel("Heslo").FillAsync(user.Password);
        await page.GetByRole(AriaRole.Button, new() { Name = "Přihlásit se" }).ClickAsync();
        await page.WaitForURLAsync("**/dashboard", new() { Timeout = 10_000 });
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Přehled" })).ToBeVisibleAsync();
    }

    private async Task WaitForBrowserUnreadCountAsync(IPage page, int expected)
    {
        await page.WaitForFunctionAsync(
            """
            async ({ apiBaseUrl, expected }) => {
                const token = window.localStorage.getItem('access_token');
                if (!token) {
                    window.__lexiQuestUnreadDiagnostics = { hasToken: false };
                    return false;
                }

                const response = await fetch(`${apiBaseUrl}/api/v1/notifications/unread-count`, {
                    headers: { Authorization: `Bearer ${token}` }
                });
                const body = await response.text();
                const count = response.ok ? Number(body) : null;
                window.__lexiQuestUnreadDiagnostics = {
                    hasToken: true,
                    status: response.status,
                    body,
                    count
                };

                return response.ok && count === expected;
            }
            """,
            new { apiBaseUrl = Fixture.ApiBaseUrl, expected },
            new() { Timeout = 10_000 });
    }

    private static string NormalizeLetters(string? value)
    {
        return new string((value ?? string.Empty)
            .Where(char.IsLetter)
            .Select(char.ToLowerInvariant)
            .ToArray());
    }

    private static async Task ExpectToggleAsync(IPage page, string testId, bool expected)
    {
        var input = page.GetByTestId(testId).Locator("input");
        if (expected)
        {
            await Expect(input).ToBeCheckedAsync();
        }
        else
        {
            await Expect(input).Not.ToBeCheckedAsync();
        }
    }

    private static async Task SetToggleAsync(IPage page, string testId, bool enabled)
    {
        var input = page.GetByTestId(testId).Locator("input");
        var isChecked = await input.IsCheckedAsync();
        if (isChecked == enabled)
        {
            return;
        }

        if (enabled)
        {
            await input.CheckAsync();
        }
        else
        {
            await input.UncheckAsync();
        }
    }

    private static async Task ClickButtonInAsync(ILocator wrapper)
    {
        await wrapper.Locator("button").ClickAsync();
    }

    private static async Task PrepareSettingsScreenshotAsync(IPage page)
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
}
