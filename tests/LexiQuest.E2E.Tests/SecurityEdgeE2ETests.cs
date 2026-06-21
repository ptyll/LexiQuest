using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using LexiQuest.Shared.DTOs.Dictionaries;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Full")]
[Trait("Category", "Visual")]
[Collection(E2ECollection.Name)]
public class SecurityEdgeE2ETests : E2ETestBase
{
    private const string Area = "security-edge";
    private const string Viewport = "1366x900";
    private const string Theme = "light";

    public SecurityEdgeE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Security_AdminSearchInputs_SqlLikePayloadsDoNotErrorOrBypassFilters()
    {
        await RunScenarioAsync(Area, "admin-search-sql-like-inputs", async page =>
        {
            var admin = await Fixture.RegisterUniqueUserAsync("secadmin");
            var managedUser = await Fixture.RegisterUniqueUserAsync("secmanaged");
            await Fixture.ForceAdminRoleAsync(admin.Email, AdminRole.Admin);

            const string sqlLikePayload = "' OR 1=1;--";

            await Fixture.LoginAsAsync(page, admin);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/admin/words");

            await Expect(page.GetByTestId(Selectors.AdminWords.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await page.GetByTestId(Selectors.AdminWords.Search).Locator("input").FillAsync(sqlLikePayload);
            await ClickButtonInAsync(page.GetByTestId(Selectors.AdminWords.ApplyFilters));
            await Expect(page.GetByTestId(Selectors.AdminWords.Page)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.AdminWords.WordCell).Filter(new() { HasTextString = "pes" })).Not.ToBeVisibleAsync();
            await AssertNoBlazorErrorAsync(page);

            await Fixture.GoToAndWaitForAppReadyAsync(page, "/admin/users");
            await Expect(page.GetByTestId(Selectors.AdminUsers.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await page.GetByTestId(Selectors.AdminUsers.Search).Locator("input").FillAsync(sqlLikePayload);
            await ClickButtonInAsync(page.GetByTestId(Selectors.AdminUsers.ApplyFilters));
            await Expect(page.GetByTestId(Selectors.AdminUsers.Page)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.AdminUsers.EmailCell).Filter(new() { HasTextString = managedUser.Email })).Not.ToBeVisibleAsync();
            await AssertNoBlazorErrorAsync(page);
            await AssertNoHorizontalOverflowAsync(page);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                Area,
                "admin-search-sql-like-inputs",
                "safe-empty-filter-result",
                Viewport,
                Theme,
                admin.Username);
        });
    }

    [Fact]
    public async Task Security_DictionaryInputs_EscapeXssClampLongStringsAndRejectBadFiles()
    {
        await RunScenarioAsync(Area, "dictionary-inputs-xss-long-files", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("secdict");
            await Fixture.ForceUserPremiumAsync(user.Email);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dictionaries");

            await Expect(page.GetByTestId(Selectors.Dictionaries.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Dictionaries.CreateButton)).ToBeVisibleAsync();

            await page.EvaluateAsync("() => window.__lexiQuestXssFired = 0");

            const string xssPayload = "<img src=x onerror=window.__lexiQuestXssFired=1>";
            await page.GetByTestId(Selectors.Dictionaries.CreateButton).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Dictionaries.CreateDialog)).ToBeVisibleAsync();
            await page.GetByTestId(Selectors.Dictionaries.NameInput).FillAsync(xssPayload);
            await page.GetByTestId(Selectors.Dictionaries.DescriptionInput).FillAsync($"{xssPayload} popis");
            await page.GetByTestId(Selectors.Dictionaries.PublicToggle).CheckAsync();
            await page.GetByTestId(Selectors.Dictionaries.SaveCreate).ClickAsync();

            var xssCard = page.GetByTestId(Selectors.Dictionaries.Card).Filter(new() { HasTextString = xssPayload });
            await Expect(xssCard).ToBeVisibleAsync(new() { Timeout = 10_000 });
            var xssFired = await page.EvaluateAsync<int>("() => window.__lexiQuestXssFired || 0");
            xssFired.Should().Be(0);
            var injectedImageCount = await xssCard.Locator("img[onerror]").CountAsync();
            injectedImageCount.Should().Be(0);

            await page.GetByTestId(Selectors.Dictionaries.CreateButton).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Dictionaries.CreateDialog)).ToBeVisibleAsync();
            await page.GetByTestId(Selectors.Dictionaries.NameInput).FillAsync(new string('N', 160));
            await page.GetByTestId(Selectors.Dictionaries.DescriptionInput).FillAsync(new string('D', 620));
            (await page.GetByTestId(Selectors.Dictionaries.NameInput).InputValueAsync()).Should().HaveLength(100);
            (await page.GetByTestId(Selectors.Dictionaries.DescriptionInput).InputValueAsync()).Should().HaveLength(500);
            await page.GetByTestId(Selectors.Dictionaries.CreateDialog)
                .GetByRole(AriaRole.Button, new() { Name = "Zrušit" })
                .ClickAsync();

            await xssCard.GetByTestId(Selectors.Dictionaries.Import).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Dictionaries.ImportDialog)).ToBeVisibleAsync();

            await page.GetByTestId(Selectors.Dictionaries.ImportFile).SetInputFilesAsync(new FilePayload
            {
                Name = "slova.exe",
                MimeType = "application/octet-stream",
                Buffer = Encoding.UTF8.GetBytes("strom")
            });
            await Expect(page.GetByTestId(Selectors.Dictionaries.ImportError)).ToContainTextAsync(".csv");
            await Expect(page.GetByTestId(Selectors.Dictionaries.ImportPreview)).Not.ToBeVisibleAsync();

            await page.GetByTestId(Selectors.Dictionaries.ImportFile).SetInputFilesAsync(new FilePayload
            {
                Name = "slova.csv",
                MimeType = "text/csv",
                Buffer = new byte[1024 * 1024 + 1]
            });
            await Expect(page.GetByTestId(Selectors.Dictionaries.ImportError)).ToContainTextAsync("1 MB");
            await Expect(page.GetByTestId(Selectors.Dictionaries.ImportPreview)).Not.ToBeVisibleAsync();

            await AssertNoHorizontalOverflowAsync(page);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                Area,
                "dictionary-inputs-xss-long-files",
                "invalid-import-file",
                Viewport,
                Theme,
                user.Username);
        });
    }

    [Fact]
    public async Task Security_PrivateDictionaryIdManipulation_DoesNotExposeOtherUserData()
    {
        await RunScenarioAsync(Area, "private-dictionary-owner-boundary", async page =>
        {
            var owner = await Fixture.RegisterUniqueUserAsync("secowner");
            var other = await Fixture.RegisterUniqueUserAsync("secother");
            await Fixture.ForceUserPremiumAsync(owner.Email);
            await Fixture.ForceUserPremiumAsync(other.Email);

            using var ownerClient = await Fixture.CreateAuthenticatedApiClientAsync(owner);
            using var otherClient = await Fixture.CreateAuthenticatedApiClientAsync(other);

            var privateDictionary = await CreateDictionaryAsync(
                ownerClient,
                "Privátní bezpečnostní slovník",
                isPublic: false);

            using var addWordResponse = await ownerClient.PostAsJsonAsync(
                $"api/v1/dictionaries/{privateDictionary.Id}/words",
                new AddWordRequest("azurit", DifficultyLevel.Beginner));
            addWordResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            using var otherDetail = await otherClient.GetAsync($"api/v1/dictionaries/{privateDictionary.Id}");
            otherDetail.StatusCode.Should().Be(HttpStatusCode.NotFound);

            using var otherAddWord = await otherClient.PostAsJsonAsync(
                $"api/v1/dictionaries/{privateDictionary.Id}/words",
                new AddWordRequest("rubin", DifficultyLevel.Beginner));
            otherAddWord.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            using var otherDelete = await otherClient.DeleteAsync($"api/v1/dictionaries/{privateDictionary.Id}");
            otherDelete.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            using var otherCustomGame = await otherClient.PostAsJsonAsync(
                "api/v1/game/start",
                new StartGameRequest(
                    GameMode.Training,
                    DifficultyLevel.Beginner,
                    CustomDictionaryId: privateDictionary.Id));
            otherCustomGame.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            await Fixture.LoginAsAsync(page, other);
            await Fixture.GoToAndWaitForAppReadyAsync(page, $"/dictionaries?dictionaryId={privateDictionary.Id}");

            await Expect(page.GetByTestId(Selectors.Dictionaries.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await page.GetByTestId(Selectors.Dictionaries.PublicTab).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Dictionaries.Card).Filter(new()
            {
                HasTextString = privateDictionary.Name
            })).Not.ToBeVisibleAsync();
            await AssertNoBlazorErrorAsync(page);

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                Area,
                "private-dictionary-owner-boundary",
                "private-dictionary-not-visible",
                Viewport,
                Theme,
                other.Username);
        });
    }

    private static async Task AssertNoHorizontalOverflowAsync(IPage page)
    {
        var hasHorizontalOverflow = await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth > document.documentElement.clientWidth + 1");

        hasHorizontalOverflow.Should().BeFalse("security edge strings should not create horizontal overflow");
    }

    private static async Task AssertNoBlazorErrorAsync(IPage page)
    {
        var hasBlazorError = await page.Locator("#blazor-error-ui:visible").CountAsync();
        hasBlazorError.Should().Be(0, "edge payloads should not crash Blazor");
    }

    private static async Task ClickButtonInAsync(ILocator container)
    {
        await container.GetByRole(AriaRole.Button).First.ClickAsync();
    }

    private static async Task<DictionaryDto> CreateDictionaryAsync(HttpClient apiClient, string name, bool isPublic)
    {
        using var response = await apiClient.PostAsJsonAsync(
            "api/v1/dictionaries",
            new CreateDictionaryRequest(name, "E2E bezpečnostní slovník", isPublic));
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var dictionary = await response.Content.ReadFromJsonAsync<DictionaryDto>();
        dictionary.Should().NotBeNull();
        dictionary!.IsPublic.Should().Be(isPublic);
        return dictionary;
    }
}
