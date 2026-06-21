using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.DTOs.Shop;
using LexiQuest.Shared.DTOs.Streak;
using LexiQuest.Shared.Enums;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Full")]
[Collection(E2ECollection.Name)]
public class StreakE2ETests : E2ETestBase
{
    public StreakE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Streak_FirstCompletedSession_SetsCurrentStreakToOne()
    {
        await RunScenarioAsync("streak", "first-completed-session", async _ =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("streakfirst");
            using var api = await Fixture.CreateAuthenticatedApiClientAsync(user);

            var game = await Fixture.StartGameViaApiAsync(
                user,
                new StartGameRequest(GameMode.Training, DifficultyLevel.Beginner));
            await Fixture.ForceSessionTotalRoundsAsync(game.SessionId, totalRounds: 1);

            var answer = await Fixture.GetActiveRoundAnswerAsync(game.SessionId);
            var result = await Fixture.SubmitAnswerViaApiAsync(api, game.SessionId, answer, 5_000);

            result.IsLevelComplete.Should().BeTrue();

            var stats = await Fixture.GetUserStatsViaApiAsync(user);
            stats.CurrentStreak.Should().Be(1);
            stats.LongestStreak.Should().Be(1);
        });
    }

    [Fact]
    public async Task Streak_SameDayCompletedSession_DoesNotIncrementAgain()
    {
        await RunScenarioAsync("streak", "same-day-no-increment", async _ =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("streaksame");
            await Fixture.ForceUserStreakAsync(
                user.Email,
                currentDays: 3,
                longestDays: 3,
                lastActivityUtc: DateTime.UtcNow.AddHours(-2));

            await CompleteOneRoundTrainingSessionAsync(user);

            var stats = await Fixture.GetUserStatsViaApiAsync(user);
            stats.CurrentStreak.Should().Be(3);
            stats.LongestStreak.Should().Be(3);
        });
    }

    [Fact]
    public async Task Streak_NextDayCompletedSession_Increments()
    {
        await RunScenarioAsync("streak", "next-day-increment", async _ =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("streaknext");
            await Fixture.ForceUserStreakAsync(
                user.Email,
                currentDays: 3,
                longestDays: 3,
                lastActivityUtc: DateTime.UtcNow.AddDays(-1));

            await CompleteOneRoundTrainingSessionAsync(user);

            var stats = await Fixture.GetUserStatsViaApiAsync(user);
            stats.CurrentStreak.Should().Be(4);
            stats.LongestStreak.Should().Be(4);
        });
    }

    [Fact]
    public async Task Streak_GracePeriodWithinFortyEightHours_KeepsAndIncrementsStreak()
    {
        await RunScenarioAsync("streak", "grace-period-47-hours", async _ =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("streakgrace");
            await Fixture.ForceUserStreakAsync(
                user.Email,
                currentDays: 5,
                longestDays: 5,
                lastActivityUtc: DateTime.UtcNow.AddHours(-47));

            await CompleteOneRoundTrainingSessionAsync(user);

            var stats = await Fixture.GetUserStatsViaApiAsync(user);
            stats.CurrentStreak.Should().Be(6);
            stats.LongestStreak.Should().Be(6);
        });
    }

    [Fact]
    public async Task Streak_MissedGracePeriod_ResetsCurrentStreak()
    {
        await RunScenarioAsync("streak", "missed-period-reset", async _ =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("streakmissed");
            await Fixture.ForceUserStreakAsync(
                user.Email,
                currentDays: 5,
                longestDays: 5,
                lastActivityUtc: DateTime.UtcNow.AddHours(-73));

            await CompleteOneRoundTrainingSessionAsync(user);

            var stats = await Fixture.GetUserStatsViaApiAsync(user);
            stats.CurrentStreak.Should().Be(1);
            stats.LongestStreak.Should().Be(5);
        });
    }

    [Fact]
    public async Task Streak_DashboardNormal_ShowsStableStreakAndXpBar()
    {
        await RunScenarioAsync("streak", "dashboard-normal-streak-xp", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("streaknormal");
            await Fixture.ForceUserStreakAsync(
                user.Email,
                currentDays: 5,
                longestDays: 7,
                lastActivityUtc: DateTime.UtcNow.AddHours(-6));
            await Fixture.ForceUserStatsAsync(
                user.Email,
                totalXp: 325,
                level: 3,
                totalWordsSolved: 21,
                accuracy: 86);
            await Fixture.LoginAsAsync(page, user);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dashboard");

            await Expect(page.GetByTestId(Selectors.Dashboard.StreakIndicator)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Dashboard.StreakIndicator)).ToContainTextAsync("5");
            await Expect(page.GetByTestId(Selectors.Dashboard.StreakRisk)).Not.ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Dashboard.XpBar)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Dashboard.XpBarLevel)).ToContainTextAsync("Úroveň 3");
            await Expect(page.GetByTestId(Selectors.Dashboard.XpBarText)).ToContainTextAsync("75/225 XP");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "streak",
                scenario: "dashboard-normal-streak-xp",
                state: "normal",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");
        });
    }

    [Fact]
    public async Task Streak_DashboardAtRisk_ShowsCountdown()
    {
        await RunScenarioAsync("streak", "dashboard-at-risk-countdown", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("streakrisk");
            await Fixture.ForceUserStreakAsync(
                user.Email,
                currentDays: 4,
                longestDays: 4,
                lastActivityUtc: DateTime.UtcNow.AddHours(-26));
            await Fixture.LoginAsAsync(page, user);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dashboard");

            await Expect(page.GetByTestId(Selectors.Dashboard.StreakIndicator)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Dashboard.StreakRisk)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Dashboard.StreakTimer)).ToContainTextAsync("Zbývá");
            await Expect(page.GetByTestId(Selectors.Dashboard.StreakTimer)).Not.ToContainTextAsync("00:00");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "streak",
                scenario: "dashboard-at-risk-countdown",
                state: "at-risk",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");
        });
    }

    [Fact]
    public async Task Streak_DashboardFreeShield_CanActivate()
    {
        await RunScenarioAsync("streak", "dashboard-free-shield-activate", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("shieldfree");
            await Fixture.ForceUserStreakAsync(
                user.Email,
                currentDays: 4,
                longestDays: 4,
                lastActivityUtc: DateTime.UtcNow.AddHours(-26));
            await Fixture.LoginAsAsync(page, user);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dashboard");

            await Expect(page.GetByTestId(Selectors.Dashboard.ShieldActivate)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await page.GetByTestId(Selectors.Dashboard.ShieldActivate).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Dashboard.ShieldActive)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Dashboard.ShieldActive)).ToContainTextAsync("Štít aktivní");
            await Expect(page.GetByTestId(Selectors.Dashboard.ShieldActivate)).Not.ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "streak",
                scenario: "dashboard-free-shield-activate",
                state: "shield-active",
                viewport: "1366x900",
                theme: "light",
                persona: "registeredUser");
        });
    }

    [Fact]
    public async Task Streak_DashboardPremiumFreeze_ShowsFreezeBadge()
    {
        await RunScenarioAsync("streak", "dashboard-premium-freeze-badge", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("freezebadge");
            await Fixture.ForceUserPremiumAsync(user.Email);
            await Fixture.ForceUserStreakAsync(
                user.Email,
                currentDays: 12,
                longestDays: 12,
                lastActivityUtc: DateTime.UtcNow.AddHours(-10));
            await Fixture.ForceStreakProtectionAsync(
                user.Email,
                freezeUsedThisWeek: false,
                lastShieldActivatedAtUtc: DateTime.UtcNow.AddDays(-2));
            await Fixture.LoginAsAsync(page, user);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dashboard");

            await Expect(page.GetByTestId(Selectors.Dashboard.StreakIndicator)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Dashboard.FreezeBadge)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Dashboard.FreezeBadge)).ToContainTextAsync("Zmrazení dostupné");
            await Expect(page.GetByTestId(Selectors.Dashboard.ShieldActive)).Not.ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "streak",
                scenario: "dashboard-premium-freeze-badge",
                state: "freeze-available",
                viewport: "1366x900",
                theme: "light",
                persona: "premiumUser");
        });
    }

    [Fact]
    public async Task Streak_ShieldCannotBeActivatedTwice()
    {
        await RunScenarioAsync("streak", "shield-double-activate", async _ =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("shieldtwice");
            using var api = await Fixture.CreateAuthenticatedApiClientAsync(user);

            var first = await PostActivateShieldAsync(api);
            first.StatusCode.Should().Be(HttpStatusCode.OK);

            var second = await PostActivateShieldAsync(api);
            second.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var protection = await GetProtectionAsync(api);
            protection.HasActiveShield.Should().BeTrue();
            protection.ShieldsRemaining.Should().Be(0);
        });
    }

    [Fact]
    public async Task Streak_FreeAndPremiumShieldCooldowns_AreApplied()
    {
        await RunScenarioAsync("streak", "shield-cooldowns", async _ =>
        {
            var freeUser = await Fixture.RegisterUniqueUserAsync("shieldfreecool");
            await Fixture.ForceStreakProtectionAsync(
                freeUser.Email,
                lastShieldActivatedAtUtc: DateTime.UtcNow.AddDays(-15));
            using var freeApi = await Fixture.CreateAuthenticatedApiClientAsync(freeUser);

            var freeTooSoon = await PostActivateShieldAsync(freeApi);
            freeTooSoon.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            await Fixture.ForceStreakProtectionAsync(
                freeUser.Email,
                lastShieldActivatedAtUtc: DateTime.UtcNow.AddDays(-31));
            var freeAfterMonth = await PostActivateShieldAsync(freeApi);
            freeAfterMonth.StatusCode.Should().Be(HttpStatusCode.OK);

            var premiumUser = await Fixture.RegisterUniqueUserAsync("shieldpremcool");
            await Fixture.ForceUserPremiumAsync(premiumUser.Email);
            await Fixture.ForceStreakProtectionAsync(
                premiumUser.Email,
                lastShieldActivatedAtUtc: DateTime.UtcNow.AddDays(-8));
            using var premiumApi = await Fixture.CreateAuthenticatedApiClientAsync(premiumUser);

            var premiumAfterWeek = await PostActivateShieldAsync(premiumApi);
            premiumAfterWeek.StatusCode.Should().Be(HttpStatusCode.OK);

            var premiumTooSoonUser = await Fixture.RegisterUniqueUserAsync("shieldpremsoon");
            await Fixture.ForceUserPremiumAsync(premiumTooSoonUser.Email);
            await Fixture.ForceStreakProtectionAsync(
                premiumTooSoonUser.Email,
                lastShieldActivatedAtUtc: DateTime.UtcNow.AddDays(-5));
            using var premiumTooSoonApi = await Fixture.CreateAuthenticatedApiClientAsync(premiumTooSoonUser);

            var premiumTooSoon = await PostActivateShieldAsync(premiumTooSoonApi);
            premiumTooSoon.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task Streak_PurchaseShields_DeductsCoinsAndAddsShields()
    {
        await RunScenarioAsync("streak", "shield-purchase-coins", async _ =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("shieldbuy");
            await Fixture.ForceUserCoinsAsync(user.Email, coinBalance: 500);
            using var api = await Fixture.CreateAuthenticatedApiClientAsync(user);

            using var response = await api.PostAsJsonAsync(
                "api/v1/streak/shield/purchase",
                new PurchaseShieldsRequest(3));

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var purchase = await response.Content.ReadFromJsonAsync<PurchaseShieldsResponse>();
            purchase.Should().NotBeNull();
            purchase!.Success.Should().BeTrue();
            purchase.TotalShields.Should().Be(3);
            purchase.RemainingCoins.Should().Be(0);

            var protection = await GetProtectionAsync(api);
            protection.ShieldsRemaining.Should().Be(3);

            var coins = await GetCoinBalanceAsync(api);
            coins.Balance.Should().Be(0);
        });
    }

    [Fact]
    public async Task Streak_EmergencyShield_IsPremiumOnlyAndDeductsCoins()
    {
        await RunScenarioAsync("streak", "emergency-shield-premium-only", async _ =>
        {
            var freeUser = await Fixture.RegisterUniqueUserAsync("emergencyfree");
            await Fixture.ForceUserCoinsAsync(freeUser.Email, coinBalance: 300);
            using var freeApi = await Fixture.CreateAuthenticatedApiClientAsync(freeUser);

            using var forbidden = await freeApi.PostAsync("api/v1/streak/shield/emergency", content: null);
            forbidden.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            var premiumUser = await Fixture.RegisterUniqueUserAsync("emergencypremium");
            await Fixture.ForceUserPremiumAsync(premiumUser.Email);
            await Fixture.ForceUserCoinsAsync(premiumUser.Email, coinBalance: 300);
            using var premiumApi = await Fixture.CreateAuthenticatedApiClientAsync(premiumUser);

            using var response = await premiumApi.PostAsync("api/v1/streak/shield/emergency", content: null);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var emergency = await response.Content.ReadFromJsonAsync<EmergencyShieldResponse>();
            emergency.Should().NotBeNull();
            emergency!.Success.Should().BeTrue();
            emergency.IsShieldActive.Should().BeTrue();
            emergency.RemainingCoins.Should().Be(0);

            var protection = await GetProtectionAsync(premiumApi);
            protection.HasActiveShield.Should().BeTrue();

            var coins = await GetCoinBalanceAsync(premiumApi);
            coins.Balance.Should().Be(0);
        });
    }

    [Fact]
    public async Task Streak_ActiveShield_MissedGracePeriod_PreservesStreakAndConsumesShield()
    {
        await RunScenarioAsync("streak", "active-shield-preserves-missed-streak", async _ =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("shieldprotect");
            await Fixture.ForceUserStreakAsync(
                user.Email,
                currentDays: 5,
                longestDays: 5,
                lastActivityUtc: DateTime.UtcNow.AddHours(-73));
            await Fixture.ForceStreakProtectionAsync(
                user.Email,
                hasActiveShield: true,
                lastShieldActivatedAtUtc: DateTime.UtcNow.AddDays(-1));

            await CompleteOneRoundTrainingSessionAsync(user);

            var stats = await Fixture.GetUserStatsViaApiAsync(user);
            stats.CurrentStreak.Should().Be(6);
            stats.LongestStreak.Should().Be(6);

            using var api = await Fixture.CreateAuthenticatedApiClientAsync(user);
            var protection = await GetProtectionAsync(api);
            protection.HasActiveShield.Should().BeFalse();
        });
    }

    [Fact]
    public async Task Streak_PremiumAutoFreeze_MissedGracePeriod_PreservesStreak()
    {
        await RunScenarioAsync("streak", "premium-auto-freeze-preserves-streak", async _ =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("freezepremium");
            await Fixture.ForceUserPremiumAsync(user.Email);
            await Fixture.ForceUserStreakAsync(
                user.Email,
                currentDays: 5,
                longestDays: 5,
                lastActivityUtc: DateTime.UtcNow.AddHours(-73));
            await Fixture.ForceStreakProtectionAsync(
                user.Email,
                freezeUsedThisWeek: false);

            await CompleteOneRoundTrainingSessionAsync(user);

            var stats = await Fixture.GetUserStatsViaApiAsync(user);
            stats.CurrentStreak.Should().Be(6);
            stats.LongestStreak.Should().Be(6);

            using var api = await Fixture.CreateAuthenticatedApiClientAsync(user);
            var protection = await GetProtectionAsync(api);
            protection.FreezeUsedThisWeek.Should().BeTrue();
        });
    }

    private async Task CompleteOneRoundTrainingSessionAsync(TestUser user)
    {
        using var api = await Fixture.CreateAuthenticatedApiClientAsync(user);

        var game = await Fixture.StartGameViaApiAsync(
            user,
            new StartGameRequest(GameMode.Training, DifficultyLevel.Beginner));
        await Fixture.ForceSessionTotalRoundsAsync(game.SessionId, totalRounds: 1);

        var answer = await Fixture.GetActiveRoundAnswerAsync(game.SessionId);
        var result = await Fixture.SubmitAnswerViaApiAsync(api, game.SessionId, answer, 5_000);
        result.IsLevelComplete.Should().BeTrue();
    }

    private static async Task<HttpResponseMessage> PostActivateShieldAsync(HttpClient api)
    {
        return await api.PostAsync("api/v1/streak/shield/activate", content: null);
    }

    private static async Task<StreakProtectionDto> GetProtectionAsync(HttpClient api)
    {
        using var response = await api.GetAsync("api/v1/streak/protection");
        response.EnsureSuccessStatusCode();

        var protection = await response.Content.ReadFromJsonAsync<StreakProtectionDto>();
        protection.Should().NotBeNull();
        return protection!;
    }

    private static async Task<CoinBalanceDto> GetCoinBalanceAsync(HttpClient api)
    {
        using var response = await api.GetAsync("api/v1/shop/coins");
        response.EnsureSuccessStatusCode();

        var coins = await response.Content.ReadFromJsonAsync<CoinBalanceDto>();
        coins.Should().NotBeNull();
        return coins!;
    }
}
