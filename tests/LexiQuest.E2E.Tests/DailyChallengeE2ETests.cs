using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Full")]
[Trait("Category", "Visual")]
[Collection(E2ECollection.Name)]
public class DailyChallengeE2ETests : E2ETestBase
{
    public DailyChallengeE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task DailyChallenge_TodayChallenge_DisplaysExpectedModifierAndStarts()
    {
        await RunScenarioAsync("daily", "today-start", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("dailytoday");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            var challenge = await apiClient.GetFromJsonAsync<DailyChallengeDto>("api/v1/game/daily");
            challenge.Should().NotBeNull();
            challenge!.ScrambledWord.Should().NotBeNullOrWhiteSpace();
            challenge.Modifier.Should().Be(ExpectedModifierFor(challenge.Date.DayOfWeek));

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/daily-challenge");

            await Expect(page.GetByTestId(Selectors.Daily.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Daily.ChallengeCard)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Daily.ChallengeCard)).ToContainTextAsync("Dnešní výzva");
            await Expect(page.GetByTestId(Selectors.Daily.Modifier)).ToContainTextAsync(ModifierName(challenge.Modifier));
            await Expect(page.GetByTestId(Selectors.Daily.Leaderboard)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Daily.LeaderboardEmpty)).ToContainTextAsync("Zatím tu není žádný výsledek");

            await page.GetByTestId(Selectors.Daily.Start).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Daily.PlayPanel)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Daily.ScrambledWord)).ToContainTextAsync(challenge.ScrambledWord);
            await Expect(page.GetByTestId(Selectors.Daily.PlayPanel)).ToContainTextAsync($"{challenge.WordLength} písmen");
            await Expect(page.GetByTestId(Selectors.Daily.AnswerInput)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Daily.Submit)).ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "daily",
                scenario: "today-start",
                state: "ready",
                viewport: "1366x900",
                theme: "light",
                persona: "dailyUser");
        });
    }

    [Fact]
    public async Task DailyChallenge_Completion_ShowsResultTopTenAndRejectsSecondAttempt()
    {
        await RunScenarioAsync("daily", "completion-top10-second-attempt", async page =>
        {
            var currentUser = await Fixture.RegisterUniqueUserAsync("dailywin");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(currentUser);
            var challenge = await apiClient.GetFromJsonAsync<DailyChallengeDto>("api/v1/game/daily");
            challenge.Should().NotBeNull();

            for (var i = 0; i < 11; i++)
            {
                var seededUser = await Fixture.RegisterUniqueUserAsync($"dailytop{i:00}");
                await Fixture.SeedDailyChallengeCompletionAsync(
                    seededUser.Email,
                    challenge!.Date,
                    TimeSpan.FromSeconds(30 + i),
                    xpEarned: 20 + i);
            }

            var originals = await Fixture.GetWordOriginalsAsync([challenge!.WordId]);
            var answer = originals[challenge.WordId];

            await Fixture.LoginAsAsync(page, currentUser);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/daily-challenge");

            await page.GetByTestId(Selectors.Daily.Start).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Daily.PlayPanel)).ToBeVisibleAsync();
            await page.GetByTestId(Selectors.Daily.AnswerInput).FillAsync(answer);
            await page.GetByTestId(Selectors.Daily.Submit).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Daily.Completed)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Daily.ResultTime)).ToContainTextAsync("s");
            await Expect(page.GetByTestId(Selectors.Daily.ResultTime)).Not.ToContainTextAsync("0s");
            await Expect(page.GetByTestId(Selectors.Daily.ResultXp)).ToContainTextAsync("+");
            await Expect(page.GetByTestId(Selectors.Daily.ResultRank)).ToContainTextAsync("#");
            await Expect(page.GetByTestId(Selectors.Daily.LeaderboardRow)).ToHaveCountAsync(10);
            await Expect(page.GetByTestId(Selectors.Daily.Leaderboard)).ToContainTextAsync(currentUser.Username);

            using var secondAttempt = await apiClient.PostAsJsonAsync(
                "api/v1/game/daily/submit",
                new DailyChallengeSubmitRequest(answer, 1_000));
            secondAttempt.StatusCode.Should().Be(HttpStatusCode.Conflict);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "daily",
                scenario: "completion-top10-second-attempt",
                state: "completed",
                viewport: "1366x900",
                theme: "light",
                persona: "dailyWinner");
        });
    }

    [Fact]
    public async Task DailyChallenge_NextDayReset_AllowsNewChallenge()
    {
        await RunScenarioAsync("daily", "next-day-reset", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("dailynext");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            var challenge = await apiClient.GetFromJsonAsync<DailyChallengeDto>("api/v1/game/daily");
            challenge.Should().NotBeNull();

            var originals = await Fixture.GetWordOriginalsAsync([challenge!.WordId]);
            using var submitResponse = await apiClient.PostAsJsonAsync(
                "api/v1/game/daily/submit",
                new DailyChallengeSubmitRequest(originals[challenge.WordId], 2_000));
            submitResponse.EnsureSuccessStatusCode();

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/daily-challenge");
            await Expect(page.GetByTestId(Selectors.Daily.Completed)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await Fixture.AdvanceE2ETimeAsync(TimeSpan.FromDays(1));
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/daily-challenge");

            await Expect(page.GetByTestId(Selectors.Daily.ChallengeCard)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Daily.Start)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Daily.Completed)).Not.ToBeVisibleAsync();

            using var nextDayChallengeResponse = await apiClient.GetAsync("api/v1/game/daily");
            nextDayChallengeResponse.EnsureSuccessStatusCode();
            var nextDayChallenge = await nextDayChallengeResponse.Content.ReadFromJsonAsync<DailyChallengeDto>();
            nextDayChallenge.Should().NotBeNull();
            nextDayChallenge!.Date.Should().Be(challenge.Date.AddDays(1));

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "daily",
                scenario: "next-day-reset",
                state: "available",
                viewport: "1366x900",
                theme: "light",
                persona: "dailyReturningUser");
        });
    }

    private static DailyModifier ExpectedModifierFor(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => DailyModifier.Category,
        DayOfWeek.Tuesday => DailyModifier.Speed,
        DayOfWeek.Wednesday => DailyModifier.NoHints,
        DayOfWeek.Thursday => DailyModifier.DoubleLetters,
        DayOfWeek.Friday => DailyModifier.Team,
        DayOfWeek.Saturday => DailyModifier.Hard,
        DayOfWeek.Sunday => DailyModifier.Easy,
        _ => DailyModifier.Easy
    };

    private static string ModifierName(DailyModifier modifier) => modifier switch
    {
        DailyModifier.Category => "Kategorie",
        DailyModifier.Speed => "Rychlost",
        DailyModifier.NoHints => "Bez nápověd",
        DailyModifier.DoubleLetters => "Dvojitá písmena",
        DailyModifier.Team => "Tým",
        DailyModifier.Hard => "Obtížné",
        DailyModifier.Easy => "Jednoduché",
        _ => modifier.ToString()
    };
}
