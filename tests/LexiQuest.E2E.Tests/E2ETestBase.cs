using Microsoft.Playwright;

namespace LexiQuest.E2E.Tests;

public abstract class E2ETestBase
{
    protected E2ETestBase(E2EEnvironmentFixture fixture) => Fixture = fixture;

    protected E2EEnvironmentFixture Fixture { get; }

    protected async Task RunScenarioAsync(
        string area,
        string scenario,
        Func<IPage, Task> action,
        int width = 1366,
        int height = 900,
        string theme = "light",
        bool reducedMotion = false,
        bool resetDatabase = true,
        bool assertNoConsoleErrors = true,
        bool assertNoFailedRequests = true)
    {
        if (resetDatabase)
        {
            await Fixture.ResetDatabaseAsync();
        }

        var viewport = $"{width}x{height}";
        var page = await Fixture.NewPageAsync(width, height, theme, reducedMotion, $"{area}.{scenario}.{viewport}.{theme}");

        try
        {
            await action(page);

            if (assertNoConsoleErrors)
            {
                await Fixture.AssertNoConsoleErrorsAsync(page);
            }

            if (assertNoFailedRequests)
            {
                await Fixture.AssertNoFailedRequestsAsync(page);
            }
        }
        catch
        {
            await Fixture.TakeFailureArtifactsAsync(page, area, scenario);
            await Fixture.WriteEnvironmentLogsAsync($"{area}-{scenario}");
            throw;
        }
        finally
        {
            try
            {
                await page.Context.CloseAsync();
            }
            catch (PlaywrightException ex) when (
                ex.Message.Contains("Target page, context or browser has been closed", StringComparison.OrdinalIgnoreCase))
            {
                // The browser may already be closed after a navigation or process-level failure.
            }
        }
    }
}
