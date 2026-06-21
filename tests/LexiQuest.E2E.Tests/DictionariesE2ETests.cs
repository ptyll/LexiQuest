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
public class DictionariesE2ETests : E2ETestBase
{
    public DictionariesE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Dictionaries_FreeUser_ShowsPremiumGateAndApiRejectsCreate()
    {
        await RunScenarioAsync("dictionaries", "free-user-premium-gate", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("dictfree");

            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            using var apiResponse = await apiClient.PostAsJsonAsync(
                "api/v1/dictionaries",
                new { Name = "Free slovník", Description = "Nemá projít", IsPublic = false });
            apiResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dictionaries");

            await Expect(page.GetByTestId(Selectors.Dictionaries.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Dictionaries.PremiumGate)).ToBeVisibleAsync();
            await Expect(page.GetByTestId(Selectors.Dictionaries.PremiumGate)).ToContainTextAsync("Premium");
            await Expect(page.GetByTestId(Selectors.Dictionaries.CreateButton)).Not.ToBeVisibleAsync();

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "dictionaries",
                scenario: "free-user-premium-gate",
                state: "gate",
                viewport: "1366x900",
                theme: "light",
                persona: "freeUser");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Dictionaries_PremiumUser_CreatesAddsImportsPublicAndDeletes()
    {
        await RunScenarioAsync("dictionaries", "premium-crud-import-public-delete", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("dictpremium");
            await Fixture.ForceUserPremiumAsync(user.Email);
            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/dictionaries");

            await Expect(page.GetByTestId(Selectors.Dictionaries.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Dictionaries.EmptyState)).ToBeVisibleAsync();

            await PrepareDictionaryScreenshotAsync(page);
            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "dictionaries",
                scenario: "premium-crud-import-public-delete",
                state: "empty",
                viewport: "1366x900",
                theme: "light",
                persona: "premiumUser");

            await page.GetByTestId(Selectors.Dictionaries.CreateButton).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Dictionaries.CreateDialog)).ToBeVisibleAsync();
            await page.GetByTestId(Selectors.Dictionaries.SaveCreate).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Dictionaries.NameError)).ToContainTextAsync("Název je povinný");

            await PrepareDictionaryScreenshotAsync(page);
            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "dictionaries",
                scenario: "premium-crud-import-public-delete",
                state: "create-validation",
                viewport: "1366x900",
                theme: "light",
                persona: "premiumUser",
                fullPage: false);

            await page.GetByTestId(Selectors.Dictionaries.NameInput).FillAsync("E2E veřejný slovník");
            await page.GetByTestId(Selectors.Dictionaries.DescriptionInput).FillAsync("Slova pro kompletní E2E ověření");
            await page.GetByTestId(Selectors.Dictionaries.PublicToggle).CheckAsync();
            await page.GetByTestId(Selectors.Dictionaries.SaveCreate).ClickAsync();

            var card = DictionaryCard(page, "E2E veřejný slovník");
            await Expect(card).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(card.GetByTestId(Selectors.Dictionaries.PublicBadge)).ToContainTextAsync("Veřejný");
            await Expect(card.GetByTestId(Selectors.Dictionaries.WordCount)).ToContainTextAsync("0");

            await card.GetByTestId(Selectors.Dictionaries.AddWord).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Dictionaries.AddWordDialog)).ToBeVisibleAsync();
            await page.GetByTestId(Selectors.Dictionaries.WordInput).FillAsync("slunce");
            await page.GetByTestId(Selectors.Dictionaries.SaveWord).ClickAsync();
            await Expect(card.GetByTestId(Selectors.Dictionaries.WordCount)).ToContainTextAsync("1", new() { Timeout = 10_000 });

            await card.GetByTestId(Selectors.Dictionaries.Import).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Dictionaries.ImportDialog)).ToBeVisibleAsync();
            await page.GetByTestId(Selectors.Dictionaries.ImportFile).SetInputFilesAsync(new FilePayload
            {
                Name = "slova.csv",
                MimeType = "text/csv",
                Buffer = Encoding.UTF8.GetBytes("strom,Beginner\nměsíc,Intermediate")
            });
            await Expect(page.GetByTestId(Selectors.Dictionaries.ImportPreview)).ToContainTextAsync("strom");

            await PrepareDictionaryScreenshotAsync(page);
            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "dictionaries",
                scenario: "premium-crud-import-public-delete",
                state: "import-preview",
                viewport: "1366x900",
                theme: "light",
                persona: "premiumUser",
                fullPage: false);

            await page.GetByTestId(Selectors.Dictionaries.ImportSave).ClickAsync();
            await Expect(card.GetByTestId(Selectors.Dictionaries.WordCount)).ToContainTextAsync("3", new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Dictionaries.ImportResult)).ToContainTextAsync("2");
            await Expect(page.GetByTestId(Selectors.Dictionaries.ImportResult)).ToContainTextAsync("0");

            await PrepareDictionaryScreenshotAsync(page);
            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "dictionaries",
                scenario: "premium-crud-import-public-delete",
                state: "import-result",
                viewport: "1366x900",
                theme: "light",
                persona: "premiumUser");

            await card.GetByTestId(Selectors.Dictionaries.Detail).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Dictionaries.DetailDialog)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Dictionaries.DetailStatus)).ToContainTextAsync("Veřejný");
            await Expect(page.GetByTestId(Selectors.Dictionaries.DetailWordList)).ToContainTextAsync("slunce");
            await Expect(page.GetByTestId(Selectors.Dictionaries.DetailWordList)).ToContainTextAsync("strom");
            await Expect(page.GetByTestId(Selectors.Dictionaries.DetailWordList)).ToContainTextAsync("měsíc");

            await PrepareDictionaryScreenshotAsync(page);
            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "dictionaries",
                scenario: "premium-crud-import-public-delete",
                state: "detail",
                viewport: "1366x900",
                theme: "light",
                persona: "premiumUser",
                fullPage: false);

            await page.GetByTestId(Selectors.Dictionaries.DetailClose).ClickAsync();
            await Expect(page.GetByTestId(Selectors.Dictionaries.DetailDialog)).Not.ToBeVisibleAsync();

            await page.GetByTestId(Selectors.Dictionaries.PublicTab).ClickAsync();
            await Expect(DictionaryCard(page, "E2E veřejný slovník")).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await page.WaitForTimeoutAsync(6_000);

            await PrepareDictionaryScreenshotAsync(page);
            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "dictionaries",
                scenario: "premium-crud-import-public-delete",
                state: "public-visible",
                viewport: "1366x900",
                theme: "light",
                persona: "premiumUser");

            page.Dialog += async (_, dialog) => await dialog.AcceptAsync();
            await page.GetByTestId(Selectors.Dictionaries.MyTab).ClickAsync();
            await card.GetByTestId(Selectors.Dictionaries.Delete).ClickAsync();
            await Expect(card).Not.ToBeVisibleAsync(new() { Timeout = 10_000 });
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Dictionaries_ApiValidationOwnerImportAndCustomGame_WorkEndToEnd()
    {
        await RunScenarioAsync("dictionaries", "api-validation-owner-import-custom-game", async _ =>
        {
            var owner = await Fixture.RegisterUniqueUserAsync("dictowner");
            var other = await Fixture.RegisterUniqueUserAsync("dictother");
            await Fixture.ForceUserPremiumAsync(owner.Email);
            await Fixture.ForceUserPremiumAsync(other.Email);

            using var ownerClient = await Fixture.CreateAuthenticatedApiClientAsync(owner);
            using var otherClient = await Fixture.CreateAuthenticatedApiClientAsync(other);

            var dictionary = await CreateDictionaryAsync(ownerClient, "API slovník", isPublic: true);
            using var detailResponse = await ownerClient.GetAsync($"api/v1/dictionaries/{dictionary.Id}");
            detailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var detail = await detailResponse.Content.ReadFromJsonAsync<DictionaryDto>();
            detail.Should().NotBeNull();
            detail!.Name.Should().Be("API slovník");
            detail.IsPublic.Should().BeTrue();

            var privateDictionary = await CreateDictionaryAsync(ownerClient, "Soukromý API slovník", isPublic: false);
            using var publicResponse = await ownerClient.GetAsync("api/v1/dictionaries/public");
            publicResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var publicDictionaries = await publicResponse.Content.ReadFromJsonAsync<List<DictionaryDto>>();
            publicDictionaries.Should().NotBeNull();
            publicDictionaries!.Should().Contain(d => d.Id == dictionary.Id);
            publicDictionaries.Should().NotContain(d => d.Id == privateDictionary.Id);
            using var privateDelete = await ownerClient.DeleteAsync($"api/v1/dictionaries/{privateDictionary.Id}");
            privateDelete.StatusCode.Should().Be(HttpStatusCode.NoContent);

            using var tooShort = await ownerClient.PostAsJsonAsync(
                $"api/v1/dictionaries/{dictionary.Id}/words",
                new AddWordRequest("ab", DifficultyLevel.Beginner));
            tooShort.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            using var tooLong = await ownerClient.PostAsJsonAsync(
                $"api/v1/dictionaries/{dictionary.Id}/words",
                new AddWordRequest("nejneobhospodarovavatelny", DifficultyLevel.Expert));
            tooLong.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            using var invalidChars = await ownerClient.PostAsJsonAsync(
                $"api/v1/dictionaries/{dictionary.Id}/words",
                new AddWordRequest("slovo123", DifficultyLevel.Beginner));
            invalidChars.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            using var validWord = await ownerClient.PostAsJsonAsync(
                $"api/v1/dictionaries/{dictionary.Id}/words",
                new AddWordRequest("xylofon", DifficultyLevel.Intermediate));
            validWord.StatusCode.Should().Be(HttpStatusCode.Created);

            using var duplicate = await ownerClient.PostAsJsonAsync(
                $"api/v1/dictionaries/{dictionary.Id}/words",
                new AddWordRequest("xylofon", DifficultyLevel.Intermediate));
            duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);

            using var jsonImport = await ownerClient.PostAsJsonAsync(
                $"api/v1/dictionaries/{dictionary.Id}/import-json",
                new { Content = """["azurit", {"word":"brilant", "difficulty":"Advanced"}]""" });
            jsonImport.StatusCode.Should().Be(HttpStatusCode.OK);
            var jsonResult = await jsonImport.Content.ReadFromJsonAsync<ImportResultDto>();
            jsonResult.Should().NotBeNull();
            jsonResult!.ImportedCount.Should().Be(2);

            using var txtImport = await ownerClient.PostAsJsonAsync(
                $"api/v1/dictionaries/{dictionary.Id}/import-txt",
                new { Content = "rubin\nsmaragd" });
            txtImport.StatusCode.Should().Be(HttpStatusCode.OK);
            var txtResult = await txtImport.Content.ReadFromJsonAsync<ImportResultDto>();
            txtResult.Should().NotBeNull();
            txtResult!.ImportedCount.Should().Be(2);

            using var malformed = await ownerClient.PostAsJsonAsync(
                $"api/v1/dictionaries/{dictionary.Id}/import-json",
                new { Content = "not json" });
            malformed.StatusCode.Should().Be(HttpStatusCode.OK);
            var malformedResult = await malformed.Content.ReadFromJsonAsync<ImportResultDto>();
            malformedResult.Should().NotBeNull();
            malformedResult!.Errors.Should().NotBeEmpty();

            var oversizedImport = string.Join('\n', Enumerable.Range(0, 101).Select(i => $"slovo{i}"));
            using var overLimit = await ownerClient.PostAsJsonAsync(
                $"api/v1/dictionaries/{dictionary.Id}/import-txt",
                new { Content = oversizedImport });
            overLimit.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            using var otherDelete = await otherClient.DeleteAsync($"api/v1/dictionaries/{dictionary.Id}");
            otherDelete.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            var game = await StartCustomDictionaryGameAsync(ownerClient, dictionary.Id);
            await Fixture.ForceSessionTotalRoundsAsync(game.SessionId, totalRounds: 1);
            var answer = await Fixture.GetActiveRoundAnswerAsync(game.SessionId);
            answer.Should().BeOneOf("xylofon", "azurit", "brilant", "rubin", "smaragd");

            using var ownerDelete = await ownerClient.DeleteAsync($"api/v1/dictionaries/{dictionary.Id}");
            ownerDelete.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Dictionaries_ApiMaxTenDictionaries_IsEnforced()
    {
        await RunScenarioAsync("dictionaries", "api-max-ten-dictionaries", async _ =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("dictmax");
            await Fixture.ForceUserPremiumAsync(user.Email);
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);

            for (var i = 1; i <= 10; i++)
            {
                using var response = await apiClient.PostAsJsonAsync(
                    "api/v1/dictionaries",
                    new { Name = $"Slovník {i:00}", Description = "Limit", IsPublic = false });
                response.StatusCode.Should().Be(HttpStatusCode.Created);
            }

            using var eleventh = await apiClient.PostAsJsonAsync(
                "api/v1/dictionaries",
                new { Name = "Jedenáctý slovník", Description = "Limit", IsPublic = false });
            eleventh.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    private static ILocator DictionaryCard(IPage page, string name)
    {
        return page.GetByTestId(Selectors.Dictionaries.Card).Filter(new() { HasTextString = name });
    }

    private static async Task PrepareDictionaryScreenshotAsync(IPage page)
    {
        await page.EvaluateAsync(
            """
            () => {
                document
                    .querySelectorAll('.tm-toast-dismiss, .tm-toast button, .tm-toast-close, [aria-label="Close"], [aria-label="Zavřít"]')
                    .forEach(button => button.click());
                document
                    .querySelectorAll('.tm-toast-container')
                    .forEach(container => container.style.display = 'none');
                window.scrollTo(0, 0);
            }
            """);
        await page.WaitForTimeoutAsync(500);
    }

    private static async Task<DictionaryDto> CreateDictionaryAsync(HttpClient apiClient, string name, bool isPublic)
    {
        using var response = await apiClient.PostAsJsonAsync(
            "api/v1/dictionaries",
            new { Name = name, Description = "E2E slovník", IsPublic = isPublic });
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var dictionary = await response.Content.ReadFromJsonAsync<DictionaryDto>();
        dictionary.Should().NotBeNull();
        dictionary!.IsPublic.Should().Be(isPublic);
        return dictionary;
    }

    private static async Task<ScrambledWordDto> StartCustomDictionaryGameAsync(HttpClient apiClient, Guid dictionaryId)
    {
        using var response = await apiClient.PostAsJsonAsync(
            "api/v1/game/start",
            new
            {
                Mode = GameMode.Training,
                Difficulty = DifficultyLevel.Beginner,
                CustomDictionaryId = dictionaryId
            });
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var game = await response.Content.ReadFromJsonAsync<ScrambledWordDto>();
        game.Should().NotBeNull();
        return game!;
    }
}
