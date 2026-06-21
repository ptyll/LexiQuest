using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LexiQuest.E2E.Tests.Infrastructure;
using LexiQuest.Shared.DTOs.Admin;
using LexiQuest.Shared.Enums;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Full")]
[Trait("Category", "Admin")]
[Trait("Category", "Visual")]
[Collection(E2ECollection.Name)]
public class AdminE2ETests : E2ETestBase
{
    public AdminE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Admin_NonAdminRouteGuard_RedirectsAndApiForbids()
    {
        await RunScenarioAsync("admin", "non-admin-route-guard", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("adminregular");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);

            using var apiResponse = await apiClient.GetAsync("api/v1/admin/dashboard/stats");
            apiResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/admin");
            await page.WaitForFunctionAsync(
                "() => !window.location.pathname.startsWith('/admin')",
                null,
                new PageWaitForFunctionOptions { Timeout = 10_000 });

            page.Url.Should().NotContain("/admin");
            await Expect(page.GetByTestId(Selectors.Admin.DashboardPage)).Not.ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "admin",
                scenario: "non-admin-route-guard",
                state: "redirected-home",
                viewport: "1366x900",
                theme: "light",
                persona: "standardUser");
        }, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Admin_DashboardStatsCards_ShowRealCounts()
    {
        await RunScenarioAsync("admin", "dashboard-stats-cards", async page =>
        {
            var admin = await Fixture.RegisterUniqueUserAsync("admindash");
            await Fixture.ForceAdminRoleAsync(admin.Email, AdminRole.Admin);

            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(admin);
            var expectedStats = await GetDashboardStatsAsync(apiClient);

            await Fixture.LoginAsAsync(page, admin);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/admin");

            await Expect(page.GetByTestId(Selectors.Admin.DashboardPage)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Admin.DashboardStats)).ToBeVisibleAsync();
            await Expect(page.GetByText("AdminDashboard_")).Not.ToBeVisibleAsync();

            await Expect(page.GetByTestId(Selectors.Admin.TotalUsers))
                .ToHaveAttributeAsync("data-value", expectedStats.TotalUsers.ToString());
            await Expect(page.GetByTestId(Selectors.Admin.ActiveToday))
                .ToHaveAttributeAsync("data-value", expectedStats.ActiveToday.ToString());
            await Expect(page.GetByTestId(Selectors.Admin.TotalWords))
                .ToHaveAttributeAsync("data-value", expectedStats.TotalWords.ToString());
            await Expect(page.GetByTestId(Selectors.Admin.DailyChallenges))
                .ToHaveAttributeAsync("data-value", expectedStats.DailyChallenges.ToString());

            await Expect(page.GetByTestId(Selectors.Admin.LinkWords)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Admin.LinkUsers)).ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "admin",
                scenario: "dashboard-stats-cards",
                state: "stats-cards",
                viewport: "1366x900",
                theme: "light",
                persona: "adminUser");
        });
    }

    [Fact]
    public async Task AdminWords_TableSearchFilterPaginationColumnPicker_WorkEndToEnd()
    {
        await RunScenarioAsync("admin", "words-table-filter-pagination-columns", async page =>
        {
            var admin = await CreateAdminUserAsync("adminwords");
            await Fixture.LoginAsAsync(page, admin);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/admin/words");

            await Expect(page.GetByTestId(Selectors.AdminWords.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.AdminWords.Table)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.AdminWords.Row)).ToHaveCountAsync(25);
            await Expect(page.GetByTestId(Selectors.AdminWords.PageInfo)).ToContainTextAsync("Strana 1");

            await ClickButtonInAsync(page.GetByTestId(Selectors.AdminWords.PaginationNext));
            await Expect(page.GetByTestId(Selectors.AdminWords.PageInfo)).ToContainTextAsync("Strana 2");

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/admin/words");
            await Expect(page.GetByTestId(Selectors.AdminWords.Table)).ToBeVisibleAsync();
            await page.GetByTestId(Selectors.AdminWords.Search).Locator("input").FillAsync("programovani");
            await page.GetByTestId(Selectors.AdminWords.MinLengthFilter).Locator("input").FillAsync("10");
            await page.GetByTestId(Selectors.AdminWords.MaxLengthFilter).Locator("input").FillAsync("12");
            await ClickButtonInAsync(page.GetByTestId(Selectors.AdminWords.ApplyFilters));

            await Expect(page.GetByTestId(Selectors.AdminWords.Row)).ToHaveCountAsync(1);
            await Expect(page.GetByTestId(Selectors.AdminWords.Row)).ToContainTextAsync("programovani");
            await Expect(page.GetByTestId(Selectors.AdminWords.Row)).ToContainTextAsync("Mistr");

            await ClickButtonInAsync(page.GetByTestId(Selectors.AdminWords.ColumnPicker));
            await Expect(page.GetByTestId(Selectors.AdminWords.ColumnPickerPanel)).ToBeVisibleAsync();
            await page.GetByTestId(Selectors.AdminWords.ColumnToggleCategory).Locator("input").ClickAsync();
            await Expect(page.GetByTestId(Selectors.AdminWords.HeadingCategory)).ToHaveCountAsync(0);
            await Expect(page.GetByTestId(Selectors.AdminWords.CategoryCell)).ToHaveCountAsync(0);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "admin",
                scenario: "words-table-filter-pagination-columns",
                state: "filtered-column-picker",
                viewport: "1366x900",
                theme: "light",
                persona: "adminUser");
        });
    }

    [Fact]
    public async Task AdminWords_CreateEditDelete_WorkEndToEnd()
    {
        await RunScenarioAsync("admin", "words-create-edit-delete", async page =>
        {
            var admin = await CreateAdminUserAsync("adminwordcrud");
            var original = $"testslovo{Guid.NewGuid():N}"[..16];
            var updated = $"{original}x";

            await Fixture.LoginAsAsync(page, admin);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/admin/words");
            await Expect(page.GetByTestId(Selectors.AdminWords.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await ClickButtonInAsync(page.GetByTestId(Selectors.AdminWords.CreateOpen));
            await Expect(page.GetByTestId(Selectors.AdminWords.Modal)).ToBeVisibleAsync();
            await page.GetByTestId(Selectors.AdminWords.FormWord).Locator("input").FillAsync(original);
            await page.GetByTestId(Selectors.AdminWords.FormDifficulty).Locator("select").SelectOptionAsync("Beginner");
            await page.GetByTestId(Selectors.AdminWords.FormCategory).Locator("select").SelectOptionAsync("Science");
            await ClickButtonInAsync(page.GetByTestId(Selectors.AdminWords.Save));
            await Expect(page.GetByText("Slovo bylo úspěšně přidáno.")).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await SearchWordAsync(page, original);
            var createdRow = page.GetByTestId(Selectors.AdminWords.Row).Filter(new() { HasText = original });
            await Expect(createdRow).ToHaveCountAsync(1);

            await ClickButtonInAsync(createdRow.GetByTestId(Selectors.AdminWords.Edit));
            await Expect(page.GetByTestId(Selectors.AdminWords.Modal)).ToBeVisibleAsync();
            await page.GetByTestId(Selectors.AdminWords.FormWord).Locator("input").FillAsync(updated);
            await page.GetByTestId(Selectors.AdminWords.FormDifficulty).Locator("select").SelectOptionAsync("Intermediate");
            await page.GetByTestId(Selectors.AdminWords.FormCategory).Locator("select").SelectOptionAsync("Technology");
            await ClickButtonInAsync(page.GetByTestId(Selectors.AdminWords.Save));
            await Expect(page.GetByText("Slovo bylo úspěšně upraveno.")).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await SearchWordAsync(page, updated);
            var updatedRow = page.GetByTestId(Selectors.AdminWords.Row).Filter(new() { HasText = updated });
            await Expect(updatedRow).ToHaveCountAsync(1);
            await Expect(updatedRow).ToContainTextAsync("Pokročilý");
            await Expect(updatedRow).ToContainTextAsync("Technologie");

            await ClickButtonInAsync(updatedRow.GetByTestId(Selectors.AdminWords.Delete));
            await Expect(page.GetByTestId(Selectors.AdminWords.DeleteModal)).ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "admin",
                scenario: "words-create-edit-delete",
                state: "delete-confirm",
                viewport: "1366x900",
                theme: "light",
                persona: "adminUser");

            await ClickButtonInAsync(page.GetByTestId(Selectors.AdminWords.DeleteConfirm));
            await Expect(page.GetByText("Slovo bylo úspěšně smazáno.")).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.AdminWords.Row).Filter(new() { HasText = updated })).ToHaveCountAsync(0);
        });
    }

    [Fact]
    public async Task AdminWords_ImportDuplicatesExportAndStats_WorkEndToEnd()
    {
        await RunScenarioAsync("admin", "words-import-export-stats", async page =>
        {
            var admin = await CreateAdminUserAsync("adminwordimport");
            var importedA = $"importslovo{Guid.NewGuid():N}"[..18];
            var importedB = $"importdata{Guid.NewGuid():N}"[..18];

            await Fixture.LoginAsAsync(page, admin);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/admin/words");
            await Expect(page.GetByTestId(Selectors.AdminWords.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await ClickButtonInAsync(page.GetByTestId(Selectors.AdminWords.ImportOpen));
            await Expect(page.GetByTestId(Selectors.AdminWords.ImportModal)).ToBeVisibleAsync();
            await page.GetByTestId(Selectors.AdminWords.ImportCsv).FillAsync(
                $"pes,Beginner,Animals\n{importedA},Beginner,Everyday\n{importedB},Intermediate,Technology");
            await ClickButtonInAsync(page.GetByTestId(Selectors.AdminWords.ImportSave));

            await Expect(page.GetByTestId(Selectors.AdminWords.ImportResult)).ToContainTextAsync("Importováno: 2");
            await Expect(page.GetByTestId(Selectors.AdminWords.ImportResult)).ToContainTextAsync("přeskočeno: 1");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "admin",
                scenario: "words-import-export-stats",
                state: "import-result",
                viewport: "1366x900",
                theme: "light",
                persona: "adminUser");

            await ClickButtonInAsync(page.GetByTestId(Selectors.AdminWords.ImportCancel));

            var download = await page.RunAndWaitForDownloadAsync(async () =>
            {
                await ClickButtonInAsync(page.GetByTestId(Selectors.AdminWords.Export));
            });
            download.SuggestedFilename.Should().Be("words-export.csv");
            var exportPath = Path.Combine(RepositoryPaths.Artifacts, $"admin-words-export-{Guid.NewGuid():N}.csv");
            await download.SaveAsAsync(exportPath);
            var csv = await File.ReadAllTextAsync(exportPath);
            csv.Should().Contain($"{importedA},Beginner,Everyday");
            csv.Should().Contain("pes,Beginner,Animals");

            await ClickButtonInAsync(page.GetByTestId(Selectors.AdminWords.StatsOpen));
            await Expect(page.GetByTestId(Selectors.AdminWords.StatsDrawer)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.AdminWords.StatsTotal)).ToContainTextAsync("107");
            await Expect(page.GetByTestId(Selectors.AdminWords.StatsDistribution)).ToContainTextAsync("Začátečník");
            await Expect(page.GetByTestId(Selectors.AdminWords.StatsDistribution)).ToContainTextAsync("Pokročilý");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "admin",
                scenario: "words-import-export-stats",
                state: "stats-drawer",
                viewport: "1366x900",
                theme: "light",
                persona: "adminUser");
        });
    }

    [Fact]
    public async Task AdminUsers_TableDetailSuspendUnsuspendResetPassword_WorkEndToEnd()
    {
        await RunScenarioAsync("admin", "users-table-detail-suspend-reset", async page =>
        {
            var admin = await CreateAdminUserAsync("adminusers");
            var managed = await Fixture.RegisterUniqueUserAsync("manageduser");
            await Fixture.ForceUserStatsAsync(managed.Email, totalXp: 1_234, level: 7, totalWordsSolved: 42, accuracy: 88);
            await Fixture.ForceUserStreakAsync(managed.Email, currentDays: 5, longestDays: 9, lastActivityUtc: DateTime.UtcNow.Date);
            await Fixture.Smtp4Dev.ClearMessagesAsync();

            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(admin);

            await Fixture.LoginAsAsync(page, admin);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/admin/users");

            await Expect(page.GetByTestId(Selectors.AdminUsers.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.AdminUsers.Table)).ToBeVisibleAsync();

            await page.GetByTestId(Selectors.AdminUsers.Search).Locator("input").FillAsync(managed.Email);
            await page.GetByTestId(Selectors.AdminUsers.MinLevelFilter).Locator("input").FillAsync("7");
            await page.GetByTestId(Selectors.AdminUsers.MaxLevelFilter).Locator("input").FillAsync("7");
            await ClickButtonInAsync(page.GetByTestId(Selectors.AdminUsers.ApplyFilters));

            var managedRow = page.GetByTestId(Selectors.AdminUsers.Row).Filter(new() { HasText = managed.Email });
            await Expect(managedRow).ToHaveCountAsync(1);
            await Expect(managedRow.GetByTestId(Selectors.AdminUsers.LevelCell)).ToContainTextAsync("7");
            await Expect(managedRow.GetByTestId(Selectors.AdminUsers.StatusCell)).ToContainTextAsync("Aktivní");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "admin",
                scenario: "users-table-detail-suspend-reset",
                state: "filtered-table",
                viewport: "1366x900",
                theme: "light",
                persona: "adminUser");

            await ClickButtonInAsync(managedRow.GetByTestId(Selectors.AdminUsers.Detail));
            await Expect(page.GetByTestId(Selectors.AdminUsers.Drawer)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.AdminUsers.DrawerEmail)).ToContainTextAsync(managed.Email);
            await Expect(page.GetByTestId(Selectors.AdminUsers.DrawerLevel)).ToContainTextAsync("7");
            await Expect(page.GetByTestId(Selectors.AdminUsers.DrawerXp)).ToContainTextAsync("1234");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "admin",
                scenario: "users-table-detail-suspend-reset",
                state: "detail-drawer",
                viewport: "1366x900",
                theme: "light",
                persona: "adminUser");

            await ClickButtonInAsync(page.GetByTestId(Selectors.AdminUsers.DrawerSuspend));
            await Expect(page.GetByText("Uživatel byl úspěšně pozastaven.")).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.AdminUsers.DrawerStatus)).ToContainTextAsync("Pozastaven");
            (await GetAdminUserByEmailAsync(apiClient, managed.Email)).IsSuspended.Should().BeTrue();

            await ClickButtonInAsync(page.GetByTestId(Selectors.AdminUsers.DrawerUnsuspend));
            await Expect(page.GetByText("Uživatel byl úspěšně obnoven.")).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.AdminUsers.DrawerStatus)).ToContainTextAsync("Aktivní");
            (await GetAdminUserByEmailAsync(apiClient, managed.Email)).IsSuspended.Should().BeFalse();

            await ClickButtonInAsync(page.GetByTestId(Selectors.AdminUsers.DrawerResetPassword));
            await Expect(page.GetByTestId(Selectors.AdminUsers.ResetPasswordResult))
                .ToContainTextAsync("E-mail pro obnovení hesla byl odeslán.");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "admin",
                scenario: "users-table-detail-suspend-reset",
                state: "reset-password-sent",
                viewport: "1366x900",
                theme: "light",
                persona: "adminUser");

            var resetEmail = await Fixture.Smtp4Dev.WaitForMessageTextAsync(raw =>
                raw.Contains(managed.Email, StringComparison.OrdinalIgnoreCase)
                && raw.Contains("Obnovení hesla - LexiQuest", StringComparison.OrdinalIgnoreCase)
                && raw.Contains("/password-reset/", StringComparison.OrdinalIgnoreCase));
            resetEmail.Should().Contain(managed.Email);
            resetEmail.Should().Contain(Fixture.WebBaseUrl);
        });
    }

    [Fact]
    public async Task Admin_ContentManager_CanManageWordsButCannotManageUsers()
    {
        await RunScenarioAsync("admin", "content-manager-role-boundary", async page =>
        {
            var contentManager = await Fixture.RegisterUniqueUserAsync("contentmanager");
            await Fixture.ForceAdminRoleAsync(contentManager.Email, AdminRole.ContentManager);

            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(contentManager);
            using var wordsResponse = await apiClient.GetAsync("api/v1/admin/words?page=1&pageSize=1");
            wordsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            using var usersResponse = await apiClient.GetAsync("api/v1/admin/users?page=1&pageSize=1");
            usersResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            await Fixture.LoginAsAsync(page, contentManager);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/admin/words");
            await Expect(page.GetByTestId(Selectors.AdminWords.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "admin",
                scenario: "content-manager-role-boundary",
                state: "words-access",
                viewport: "1366x900",
                theme: "light",
                persona: "contentManager");

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/admin/users");
            await page.WaitForFunctionAsync(
                "() => !window.location.pathname.startsWith('/admin/users')",
                null,
                new PageWaitForFunctionOptions { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.AdminUsers.Page)).Not.ToBeVisibleAsync();
        }, assertNoFailedRequests: false);
    }

    private static async Task<AdminDashboardStatsDto> GetDashboardStatsAsync(HttpClient apiClient)
    {
        using var response = await apiClient.GetAsync("api/v1/admin/dashboard/stats");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var stats = await response.Content.ReadFromJsonAsync<AdminDashboardStatsDto>();
        stats.Should().NotBeNull();
        return stats!;
    }

    private static async Task<AdminUserDto> GetAdminUserByEmailAsync(HttpClient apiClient, string email)
    {
        using var response = await apiClient.GetAsync($"api/v1/admin/users?search={Uri.EscapeDataString(email)}&page=1&pageSize=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PaginatedResult<AdminUserDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle(user => user.Email == email);
        return result.Items.Single(user => user.Email == email);
    }

    private async Task<TestUser> CreateAdminUserAsync(string prefix)
    {
        var admin = await Fixture.RegisterUniqueUserAsync(prefix);
        await Fixture.ForceAdminRoleAsync(admin.Email, AdminRole.Admin);
        return admin;
    }

    private static async Task SearchWordAsync(IPage page, string word)
    {
        await page.GetByTestId(Selectors.AdminWords.Search).Locator("input").FillAsync(word);
        await page.GetByTestId(Selectors.AdminWords.DifficultyFilter).Locator("select").SelectOptionAsync("");
        await page.GetByTestId(Selectors.AdminWords.CategoryFilter).Locator("select").SelectOptionAsync("");
        await ClickButtonInAsync(page.GetByTestId(Selectors.AdminWords.ApplyFilters));
    }

    private static async Task ClickButtonInAsync(ILocator container)
    {
        await container.GetByRole(AriaRole.Button).First.ClickAsync();
    }
}
