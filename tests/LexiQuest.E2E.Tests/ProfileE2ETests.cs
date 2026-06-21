using FluentAssertions;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Full")]
[Trait("Category", "Visual")]
[Collection(E2ECollection.Name)]
public class ProfileE2ETests : E2ETestBase
{
    public ProfileE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Profile_Page_ShowsPremiumStatsAndAchievementsSummary()
    {
        await RunScenarioAsync("profile", "summary-premium-stats-achievements", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("profilefull");
            await Fixture.ForceUserPremiumAsync(
                user.Email,
                expiresAtUtc: DateTime.UtcNow.AddDays(45),
                premiumPlan: "Yearly",
                subscriptionPlan: "Yearly");
            await Fixture.ForceUserStatsAsync(
                user.Email,
                totalXp: 1_250,
                level: 7,
                totalWordsSolved: 42,
                accuracy: 88);
            await Fixture.ForceUserStreakAsync(
                user.Email,
                currentDays: 5,
                longestDays: 11,
                lastActivityUtc: DateTime.UtcNow.Date);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/profile");

            await Expect(page.GetByTestId(Selectors.Profile.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Profile.Heading)).ToContainTextAsync("Můj profil");
            await Expect(page.GetByTestId(Selectors.Profile.SummaryCard)).ToContainTextAsync(user.Username);
            await Expect(page.GetByTestId(Selectors.Profile.SummaryCard)).ToContainTextAsync(user.Email);
            await Expect(page.GetByTestId(Selectors.Profile.PremiumBadge)).ToContainTextAsync("Premium - Roční");

            var stats = page.GetByTestId(Selectors.Profile.StatsCard);
            await Expect(stats).ToContainTextAsync("Statistiky");
            await Expect(stats).ToContainTextAsync("Úroveň");
            await Expect(stats).ToContainTextAsync("7");
            await Expect(stats).ToContainTextAsync("Vyřešená slova");
            await Expect(stats).ToContainTextAsync("42");
            await Expect(stats).ToContainTextAsync("Aktuální série");
            await Expect(stats).ToContainTextAsync("5");
            await Expect(stats).ToContainTextAsync("Nejdelší série");
            await Expect(stats).ToContainTextAsync("11");
            await Expect(stats).ToContainTextAsync("Přesnost");
            await Expect(stats).ToContainTextAsync("88%");
            await Expect(stats).Not.ToContainTextAsync("8800%");

            var levelBox = await page.GetByTestId(Selectors.Profile.StatLevel).BoundingBoxAsync();
            var xpBox = await page.GetByTestId(Selectors.Profile.StatXp).BoundingBoxAsync();
            var accuracyBox = await page.GetByTestId(Selectors.Profile.StatAccuracy).BoundingBoxAsync();

            levelBox.Should().NotBeNull();
            xpBox.Should().NotBeNull();
            accuracyBox.Should().NotBeNull();
            xpBox!.Y.Should().BeApproximately(levelBox!.Y, 8);
            xpBox.X.Should().BeGreaterThan(levelBox.X);
            accuracyBox!.Y.Should().BeGreaterThan(levelBox.Y);

            foreach (var statTestId in new[]
            {
                Selectors.Profile.StatLevel,
                Selectors.Profile.StatXp,
                Selectors.Profile.StatWords,
                Selectors.Profile.StatCurrentStreak,
                Selectors.Profile.StatLongestStreak,
                Selectors.Profile.StatAccuracy
            })
            {
                await Expect(page.GetByTestId(statTestId).Locator("svg")).ToBeVisibleAsync();
            }

            var achievements = page.GetByTestId(Selectors.Profile.AchievementsCard);
            await Expect(achievements).ToContainTextAsync("Úspěchy");
            await Expect(achievements).ToContainTextAsync("Přehled odemčených úspěchů");
            await Expect(page.GetByText("Achievementy")).Not.ToBeVisibleAsync();
            await Expect(page.GetByText("Profile_")).Not.ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "profile",
                scenario: "summary-premium-stats-achievements",
                state: "loaded",
                viewport: "1366x900",
                theme: "light",
                persona: "premiumUser");
        });
    }
}
