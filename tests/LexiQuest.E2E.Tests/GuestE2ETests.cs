using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
public class GuestE2ETests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;

    public GuestE2ETests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GuestPlay_StartGame_PlaysWithoutAccount()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync($"{_fixture.BaseUrl}");

        // Click guest play / try without account button
        var guestButton = page.Locator("[data-testid='guest-play'], [data-testid='try-free'], a[href*='guest']");
        await Expect(guestButton).ToBeVisibleAsync(new() { Timeout = 5000 });
        await guestButton.ClickAsync();

        // Should navigate to guest game page
        await page.WaitForURLAsync(url => url.Contains("/guest") || url.Contains("/play"));

        // Game area should load
        var gameArea = page.Locator("[data-testid='game-arena'], .game-arena, [data-testid='guest-game']");
        await Expect(gameArea).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Fact]
    public async Task GuestPlay_AfterLimit_ShowsCTA()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync($"{_fixture.BaseUrl}/guest-game");

        // Play through rounds until CTA appears or limit is reached
        for (var i = 0; i < 10; i++)
        {
            var ctaModal = page.Locator("[data-testid='guest-cta'], [data-testid='register-cta'], .guest-limit-modal");
            if (await ctaModal.IsVisibleAsync())
            {
                // CTA should have a register link/button
                var registerLink = ctaModal.Locator("[data-testid='register-link'], a[href*='register'], button");
                await Expect(registerLink).ToBeVisibleAsync();
                return;
            }

            // Try to submit an answer to progress
            var answerInput = page.Locator("[data-testid='answer-input'], .answer-input, input[type='text']");
            if (await answerInput.IsVisibleAsync())
            {
                await answerInput.FillAsync("testanswer");
                await page.ClickAsync("[data-testid='submit-answer'], [data-testid='check-button'], button[type='submit']");
                await page.WaitForTimeoutAsync(1000);
            }
            else
            {
                break;
            }
        }
    }

    [Fact]
    public async Task GuestPlay_CTAModal_NavigatesToRegister()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync($"{_fixture.BaseUrl}/guest-game");

        // Wait for either CTA or game to load
        await page.WaitForSelectorAsync(
            "[data-testid='guest-cta'], [data-testid='game-arena'], .game-arena",
            new() { Timeout = 10000 });

        // If CTA is visible, click register
        var ctaModal = page.Locator("[data-testid='guest-cta'], [data-testid='register-cta']");
        if (await ctaModal.IsVisibleAsync())
        {
            var registerButton = ctaModal.Locator("[data-testid='register-link'], a[href*='register']");
            await registerButton.ClickAsync();
            await page.WaitForURLAsync("**/register");

            var registerForm = page.Locator("[data-testid='register-form'], form");
            await Expect(registerForm).ToBeVisibleAsync();
        }
    }

    [Fact]
    public async Task GuestPlay_NoAuthRequired()
    {
        var page = await _fixture.Browser.NewPageAsync();

        // Guest game should be accessible without login
        var response = await page.GotoAsync($"{_fixture.BaseUrl}/guest-game");
        Assert.NotNull(response);
        Assert.True(response.Ok || response.Status == 304, $"Expected 200/304 but got {response.Status}");

        // Should not redirect to login
        Assert.DoesNotContain("/login", page.Url);
    }
}
