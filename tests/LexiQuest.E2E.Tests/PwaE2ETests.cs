using System.Text.Json;
using FluentAssertions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "PWA")]
[Trait("Category", "Visual")]
[Trait("Category", "Full")]
[Collection(E2ECollection.Name)]
public class PwaE2ETests : E2ETestBase
{
    public PwaE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Pwa_ManifestAndServiceWorker_AreInstallable()
    {
        await RunScenarioAsync("pwa", "manifest-service-worker", async page =>
        {
            await Fixture.GoToAndWaitForAppReadyAsync(page);

            var manifestJson = await page.EvaluateAsync<string>(
                """
                async () => {
                    const manifestResponse = await fetch('/manifest.json', { cache: 'no-store' });
                    const manifest = await manifestResponse.json();
                    const iconStatuses = await Promise.all((manifest.icons || []).map(async icon => {
                        const response = await fetch(icon.src, { cache: 'no-store' });
                        return { src: icon.src, sizes: icon.sizes, purpose: icon.purpose || '', status: response.status, ok: response.ok };
                    }));

                    return JSON.stringify({
                        ok: manifestResponse.ok,
                        name: manifest.name || '',
                        shortName: manifest.short_name || '',
                        startUrl: manifest.start_url || '',
                        display: manifest.display || '',
                        lang: manifest.lang || '',
                        iconStatuses
                    });
                }
                """);
            var manifest = JsonSerializer.Deserialize<ManifestProbe>(
                manifestJson,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
            manifest.Should().NotBeNull();

            manifest!.Ok.Should().BeTrue();
            manifest.Name.Should().Be("LexiQuest");
            manifest.ShortName.Should().Be("LexiQuest");
            manifest.StartUrl.Should().NotBeNullOrWhiteSpace();
            manifest.Display.Should().Be("standalone");
            manifest.Lang.Should().Be("cs");
            manifest.IconStatuses.Should().Contain(icon => icon.Sizes == "192x192" && icon.Ok);
            manifest.IconStatuses.Should().Contain(icon => icon.Sizes == "512x512" && icon.Ok);

            var serviceWorkerState = await page.EvaluateAsync<string>(
                """
                async () => {
                    if (!('serviceWorker' in navigator)) {
                        return 'unsupported';
                    }

                    return await Promise.race([
                        navigator.serviceWorker.ready.then(registration => registration.active?.state || 'no-active-worker'),
                        new Promise(resolve => setTimeout(() => resolve('timeout'), 5000))
                    ]);
                }
                """);

            serviceWorkerState.Should().Be("activated");
        }, resetDatabase: false);
    }

    [Fact]
    public async Task Pwa_InstallPrompt_CanBeAcceptedAndDismissed()
    {
        await RunScenarioAsync("pwa", "install-prompt", async page =>
        {
            await Fixture.GoToAndWaitForAppReadyAsync(page);

            await page.EvaluateAsync(
                """
                () => {
                    const event = new Event('beforeinstallprompt', { cancelable: true });
                    event.prompt = () => {
                        window.__lexiQuestInstallPromptCalled = true;
                        return Promise.resolve();
                    };
                    event.userChoice = Promise.resolve({ outcome: 'accepted', platform: 'web' });
                    window.dispatchEvent(event);
                }
                """);

            await Expect(page.GetByTestId(Selectors.Pwa.InstallPrompt)).ToBeVisibleAsync();
            await Expect(page.GetByText("Nainstaluj LexiQuest")).ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "pwa",
                scenario: "install-prompt",
                state: "visible",
                viewport: "1366x900",
                theme: "light",
                persona: "anonymous");

            await page.GetByTestId(Selectors.Pwa.InstallPromptInstall).ClickAsync();

            var promptCalled = await page.EvaluateAsync<bool>("() => window.__lexiQuestInstallPromptCalled === true");
            promptCalled.Should().BeTrue();
            await Expect(page.GetByTestId(Selectors.Pwa.InstallPrompt)).Not.ToBeVisibleAsync();
        }, resetDatabase: false);
    }

    [Fact]
    public async Task Pwa_OfflineBanner_AppearsAndDisappearsWithConnectivity()
    {
        await RunScenarioAsync("pwa", "offline-banner", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("pwa");
            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dashboard");

            await page.Context.SetOfflineAsync(true);
            await page.EvaluateAsync("() => window.dispatchEvent(new Event('offline'))");

            await Expect(page.GetByTestId(Selectors.Pwa.OfflineBanner)).ToBeVisibleAsync();
            await Expect(page.GetByText("Jsi offline")).ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "pwa",
                scenario: "offline-banner",
                state: "offline",
                viewport: "1366x900",
                theme: "light",
                persona: "registered");

            await page.Context.SetOfflineAsync(false);
            await page.EvaluateAsync("() => window.dispatchEvent(new Event('online'))");

            await Expect(page.GetByTestId(Selectors.Pwa.OfflineBanner)).Not.ToBeVisibleAsync();
        });
    }

    [Fact]
    public async Task Pwa_OfflineTraining_UsesCachedSeedAndReplaysQueuedAnswer()
    {
        await RunScenarioAsync("pwa", "offline-training-queue", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("offlinetrain");
            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/game");

            await page.GetByTestId(Selectors.Game.ModeTraining).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Game.Arena)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await page.WaitForFunctionAsync("() => window.localStorage.getItem('lexiquest_offline_training_seed')");

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/game");
            await page.Context.SetOfflineAsync(true);
            await page.EvaluateAsync("() => window.dispatchEvent(new Event('offline'))");

            await page.GetByTestId(Selectors.Game.ModeTraining).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Game.OfflineBadge)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Game.Arena)).ToBeVisibleAsync();
            var scrambled = NormalizeLetters(await page.GetByTestId(Selectors.Game.ScrambledWord).TextContentAsync());
            scrambled.Should().NotBeNullOrWhiteSpace();

            var correctAnswer = await Fixture.GetBeginnerOriginalForScrambledWordAsync(scrambled);
            await page.GetByTestId(Selectors.Game.AnswerInput).FillAsync(correctAnswer);
            await page.GetByTestId(Selectors.Game.Submit).ClickAsync();

            await Expect(page.GetByTestId(Selectors.Game.Feedback)).ToBeVisibleAsync();
            var queuedCount = await page.EvaluateAsync<int>(
                "() => JSON.parse(window.localStorage.getItem('lexiquest_offline_game_queue') || '[]').length");
            queuedCount.Should().Be(1);

            await page.Context.SetOfflineAsync(false);
            await page.EvaluateAsync("() => window.dispatchEvent(new Event('online'))");
            await page.WaitForFunctionAsync(
                "() => JSON.parse(window.localStorage.getItem('lexiquest_offline_game_queue') || '[]').length === 0",
                new PageWaitForFunctionOptions { Timeout = 10_000 });

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "pwa",
                scenario: "offline-training-queue",
                state: "replayed",
                viewport: "1366x900",
                theme: "light",
                persona: "registered");
        });
    }

    private sealed class ManifestProbe
    {
        public bool Ok { get; set; }

        public string Name { get; set; } = string.Empty;

        public string ShortName { get; set; } = string.Empty;

        public string StartUrl { get; set; } = string.Empty;

        public string Display { get; set; } = string.Empty;

        public string Lang { get; set; } = string.Empty;

        public List<ManifestIconProbe> IconStatuses { get; set; } = [];
    }

    private sealed class ManifestIconProbe
    {
        public string Src { get; set; } = string.Empty;

        public string Sizes { get; set; } = string.Empty;

        public string Purpose { get; set; } = string.Empty;

        public int Status { get; set; }

        public bool Ok { get; set; }
    }

    private static string NormalizeLetters(string? value)
    {
        return new string((value ?? string.Empty)
            .Where(char.IsLetter)
            .Select(char.ToLowerInvariant)
            .ToArray());
    }
}
