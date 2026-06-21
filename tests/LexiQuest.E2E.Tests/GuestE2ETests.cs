using FluentAssertions;
using Microsoft.Playwright;
using System.Net;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Smoke")]
[Trait("Category", "Visual")]
[Trait("Category", "Full")]
[Collection(E2ECollection.Name)]
public class GuestE2ETests : E2ETestBase
{
    public GuestE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GuestPlay_LoadsWithoutAccount_AndStoresWelcomeScreenshot()
    {
        await RunScenarioAsync("guest", "welcome", async page =>
        {
            var response = await Fixture.GoToAndWaitForAppReadyAsync(page, "/play");

            response.Should().NotBeNull();
            response!.Ok.Should().BeTrue($"guest play returned {response.Status}");
            page.Url.Should().NotContain("/login");

            await Expect(page.GetByTestId(Selectors.Guest.Welcome)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Guest.StartButton)).ToBeEnabledAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "guest",
                scenario: "welcome",
                state: "loaded",
                viewport: "1366x900",
                theme: "light",
                persona: "guestBrowserProfile");
        });
    }

    [Fact]
    public async Task GuestPlay_StartGame_ShowsArenaAndDisabledEmptySubmit()
    {
        await RunScenarioAsync("guest", "start-game", async page =>
        {
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/play");

            await page.GetByTestId(Selectors.Guest.StartButton).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Guest.Arena)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Guest.RemainingGames)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Guest.AnswerInput)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Guest.Submit)).ToBeDisabledAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "guest",
                scenario: "start-game",
                state: "active-game",
                viewport: "1366x900",
                theme: "light",
                persona: "guestBrowserProfile");
        });
    }

    [Fact]
    public async Task GuestPlay_WrongAnswer_ShowsFeedbackWithCorrectAnswer()
    {
        await RunScenarioAsync("guest", "wrong-answer-feedback", async page =>
        {
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/play");

            await page.GetByTestId(Selectors.Guest.StartButton).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Guest.Arena)).ToBeVisibleAsync();

            await page.GetByTestId(Selectors.Guest.AnswerInput).FillAsync("neexistujiciodpoved");
            await page.GetByTestId(Selectors.Guest.Submit).ClickAsync();

            var feedback = page.GetByTestId(Selectors.Guest.Feedback);
            await Expect(feedback).ToBeVisibleAsync(new() { Timeout = 5_000 });
            await Expect(feedback).ToContainTextAsync("Špatně");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "guest",
                scenario: "wrong-answer-feedback",
                state: "feedback-visible",
                viewport: "1366x900",
                theme: "light",
                persona: "guestBrowserProfile");
        });
    }

    [Fact]
    public async Task GuestConversion_RegisterFromCta_TransfersProgressToDashboard()
    {
        await RunScenarioAsync("guest", "conversion-transfers-progress", async page =>
        {
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/play");

            await page.GetByTestId(Selectors.Guest.StartButton).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Guest.Arena)).ToBeVisibleAsync();
            var expectedXp = 0;
            const int guestWordsCount = 5;

            for (var index = 0; index < guestWordsCount; index++)
            {
                await Expect(page.GetByText($"Slovo {index + 1} z {guestWordsCount}")).ToBeVisibleAsync();

                var scrambled = await page.GetByTestId(Selectors.Guest.ScrambledWord).TextContentAsync();
                scrambled.Should().NotBeNullOrWhiteSpace();
                var answer = await Fixture.GetBeginnerOriginalForScrambledWordAsync(scrambled!);
                expectedXp += 10 + answer.Length * 2;

                await page.GetByTestId(Selectors.Guest.AnswerInput).FillAsync(answer);
                await page.GetByTestId(Selectors.Guest.Submit).ClickAsync();

                await Expect(page.GetByTestId(Selectors.Guest.Feedback)).ToBeVisibleAsync();
                await Expect(page.GetByTestId(Selectors.Guest.Feedback)).ToContainTextAsync("Správně");

                if (index < guestWordsCount - 1)
                {
                    var ctaModal = page.GetByTestId(Selectors.Guest.CtaModal);
                    await Expect(ctaModal).ToBeVisibleAsync();
                    await Expect(ctaModal).ToContainTextAsync("Skvělé");
                    await Expect(ctaModal).ToContainTextAsync("Ukládání pokroku");
                    await AssertLocatorIntersectsViewportAsync(page, page.GetByTestId(Selectors.Guest.ModalOverlay));
                    await AssertLocatorIntersectsViewportAsync(page, ctaModal);
                    if (index == 0)
                    {
                        await Fixture.TakeCheckpointScreenshotAsync(
                            page,
                            area: "guest",
                            scenario: "cta-modal-after-correct-answer",
                            state: "visible",
                            viewport: "1366x900",
                            theme: "light",
                            persona: "guestBrowserProfile",
                            fullPage: false);
                    }

                    await ctaModal.GetByRole(AriaRole.Button, new() { Name = "Pokračovat ve hře" }).ClickAsync();
                    await Expect(ctaModal).Not.ToBeVisibleAsync();
                }
            }

            expectedXp.Should().BeGreaterThan(0);

            var convertModal = page.GetByTestId(Selectors.Guest.ConvertModal);
            await Expect(convertModal).ToBeVisibleAsync();
            await Expect(convertModal).ToContainTextAsync("Hra dokončena");
            await Expect(convertModal).ToContainTextAsync("Vyřešená slova");
            await Expect(convertModal).ToContainTextAsync(expectedXp.ToString());
            await AssertLocatorIntersectsViewportAsync(page, page.GetByTestId(Selectors.Guest.ModalOverlay));
            await AssertLocatorIntersectsViewportAsync(page, convertModal);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "guest",
                scenario: "conversion-transfers-progress",
                state: "conversion-modal",
                viewport: "1366x900",
                theme: "light",
                persona: "guestBrowserProfile",
                fullPage: false);

            await convertModal.GetByRole(AriaRole.Button, new() { Name = "Uložit pokrok" }).ClickAsync();
            await page.WaitForURLAsync("**/register?guestProgress=1");

            var storedProgress = await page.EvaluateAsync<string?>("() => window.localStorage.getItem('guest_progress')");
            storedProgress.Should().NotBeNullOrWhiteSpace();
            storedProgress.Should().Contain("TransferToken");
            storedProgress.Should().Contain(expectedXp.ToString());

            var progressBanner = page.GetByTestId(Selectors.Guest.ProgressBanner);
            await Expect(progressBanner).ToBeVisibleAsync();
            await Expect(progressBanner).ToContainTextAsync(expectedXp.ToString());
            await Expect(progressBanner).ToContainTextAsync("5");

            var unique = Guid.NewGuid().ToString("N")[..10];
            await page.GetByLabel("Email").FillAsync($"guest.{unique}@lexiquest.test");
            await page.GetByLabel("Uživatelské jméno").FillAsync($"guest{unique}"[..15]);
            await page.GetByPlaceholder("Min. 8 znaků, 1 velké písmeno, 1 číslo").FillAsync("TestPass123!");
            await page.GetByLabel("Potvrdit heslo").FillAsync("TestPass123!");
            await page.GetByLabel("Souhlasím s podmínkami použití").CheckAsync();
            await page.GetByRole(AriaRole.Button, new() { Name = "Vytvořit účet" }).ClickAsync();

            await page.WaitForURLAsync("**/dashboard");
            await Expect(page.GetByText("Celkové XP")).ToBeVisibleAsync();
            await Expect(page.Locator(".stats-grid")).ToContainTextAsync(expectedXp.ToString());
            await Expect(page.Locator(".stats-grid")).ToContainTextAsync("5 slov");

            var progressAfterRegister = await page.EvaluateAsync<string?>("() => window.localStorage.getItem('guest_progress')");
            progressAfterRegister.Should().BeNull();
        });
    }

    [Fact]
    public async Task GuestLimit_FifthGameAllowedAndSixthGameShowsRegistrationCta()
    {
        await RunScenarioAsync("guest", "daily-limit-registration-cta", async page =>
        {
            for (var gameNumber = 1; gameNumber <= 5; gameNumber++)
            {
                var result = await Fixture.StartGuestGameViaApiAsync();
                result.StatusCode.Should().Be(HttpStatusCode.OK);
                result.Game.Should().NotBeNull();
                result.Game!.RemainingGames.Should().Be(5 - gameNumber);
            }

            var sixthGame = await Fixture.StartGuestGameViaApiAsync();
            sixthGame.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/play");
            await page.GetByTestId(Selectors.Guest.StartButton).ClickAsync();

            var limit = page.GetByTestId(Selectors.Guest.LimitReached);
            await Expect(limit).ToBeVisibleAsync();
            await Expect(limit).ToContainTextAsync("denního limitu");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "guest",
                scenario: "daily-limit-registration-cta",
                state: "limit-reached",
                viewport: "1366x900",
                theme: "light",
                persona: "guestBrowserProfile");

            await limit.GetByRole(AriaRole.Button, new() { Name = "Zaregistrovat se" }).ClickAsync();
            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Vytvořit účet" })).ToBeVisibleAsync();
            page.Url.Should().Contain("/register");
        });
    }

    [Fact]
    public async Task GuestLimit_After24Hours_AllowsNewGameAgain()
    {
        await RunScenarioAsync("guest", "daily-limit-reset-after-24h", async page =>
        {
            for (var gameNumber = 1; gameNumber <= 5; gameNumber++)
            {
                var result = await Fixture.StartGuestGameViaApiAsync();
                result.StatusCode.Should().Be(HttpStatusCode.OK);
            }

            var blocked = await Fixture.StartGuestGameViaApiAsync();
            blocked.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

            await Fixture.AdvanceE2ETimeAsync(TimeSpan.FromHours(24).Add(TimeSpan.FromMinutes(1)));

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/play");
            await page.GetByTestId(Selectors.Guest.StartButton).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Guest.Arena)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Guest.RemainingGames)).ToContainTextAsync("4");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "guest",
                scenario: "daily-limit-reset-after-24h",
                state: "reset-allows-game",
                viewport: "1366x900",
                theme: "light",
                persona: "guestBrowserProfile");
        });
    }

    [Fact]
    public async Task GuestProtectedFeatures_DashboardRedirectsToLoginWithoutAuthTokens()
    {
        await RunScenarioAsync("guest", "protected-features-redirect", async page =>
        {
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/play");
            await Expect(page.GetByTestId(Selectors.Guest.Welcome)).ToBeVisibleAsync();

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dashboard");
            await page.WaitForURLAsync("**/login");

            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Přihlášení" })).ToBeVisibleAsync();
            await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Přehled" })).Not.ToBeVisibleAsync();

            var tokens = await page.EvaluateAsync<string[]>(
                """
                () => [
                    window.localStorage.getItem('access_token') || '',
                    window.localStorage.getItem('refresh_token') || ''
                ]
                """);
            tokens.Should().OnlyContain(token => string.IsNullOrEmpty(token));

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "guest",
                scenario: "protected-features-redirect",
                state: "login-required",
                viewport: "1366x900",
                theme: "light",
                persona: "guestBrowserProfile");
        });
    }

    private static async Task AssertLocatorIntersectsViewportAsync(IPage page, ILocator locator)
    {
        var box = await locator.BoundingBoxAsync();
        box.Should().NotBeNull();

        var viewport = page.ViewportSize;
        viewport.Should().NotBeNull();

        box!.Width.Should().BeGreaterThan(0);
        box.Height.Should().BeGreaterThan(0);
        box.X.Should().BeLessThan(viewport!.Width);
        (box.X + box.Width).Should().BeGreaterThan(0);
        box.Y.Should().BeLessThan(viewport.Height);
        (box.Y + box.Height).Should().BeGreaterThan(0);
    }
}
