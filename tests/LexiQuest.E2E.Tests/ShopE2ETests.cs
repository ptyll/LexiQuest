using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LexiQuest.Shared.DTOs.Shop;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LexiQuest.E2E.Tests;

[Trait("Category", "E2E")]
[Trait("Category", "Full")]
[Trait("Category", "Visual")]
[Collection(E2ECollection.Name)]
public class ShopE2ETests : E2ETestBase
{
    public ShopE2ETests(E2EEnvironmentFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Shop_Page_ShowsBalanceCategoriesItemsAndRarity()
    {
        await RunScenarioAsync("shop", "overview-balance-categories-rarity", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("shopoverview");
            await Fixture.ForceUserCoinsAsync(user.Email, 1_000);
            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/shop");

            await Expect(page.GetByTestId(Selectors.Shop.Page)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Shop.CoinBalance)).ToContainTextAsync("1000");
            await Expect(page.GetByTestId(Selectors.Shop.TabAvatar)).ToContainTextAsync("Avatary");
            await Expect(page.GetByTestId(Selectors.Shop.TabFrame)).ToContainTextAsync("Rámečky");
            await Expect(page.GetByTestId(Selectors.Shop.TabTheme)).ToContainTextAsync("Témata");
            await Expect(page.GetByTestId(Selectors.Shop.TabBoost)).ToContainTextAsync("Boosty");

            await Expect(ItemCard(page, "sova-ucence")).ToBeVisibleAsync();
            await Expect(ItemCard(page, "dreveny-ramecek")).ToBeVisibleAsync();
            await Expect(ItemCard(page, "xp-boost-maly")).ToBeVisibleAsync();
            await Expect(ItemCard(page, "stribrny-ramecek").GetByTestId(Selectors.Shop.RarityBadge)).ToContainTextAsync("Vzácný");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "shop",
                scenario: "overview-balance-categories-rarity",
                state: "loaded",
                viewport: "1366x900",
                theme: "light",
                persona: "freeUser");
        });
    }

    [Fact]
    public async Task Shop_PurchaseOwnedEquipInsufficientAndDuplicate_WorkEndToEnd()
    {
        await RunScenarioAsync("shop", "purchase-owned-equip-insufficient-duplicate", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("shoppurchase");
            await Fixture.ForceUserCoinsAsync(user.Email, 600);
            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/shop");

            var woodenFrame = ItemCard(page, "dreveny-ramecek");
            await ClickButtonInAsync(woodenFrame.GetByTestId(Selectors.Shop.Buy));
            await Expect(page.GetByText("Předmět úspěšně zakoupen!")).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Shop.CoinBalance)).ToContainTextAsync("400");
            await Expect(woodenFrame.GetByTestId(Selectors.Shop.OwnedBadge)).ToBeVisibleAsync();

            await ClickButtonInAsync(woodenFrame.GetByTestId(Selectors.Shop.Equip));
            await Expect(woodenFrame.GetByTestId(Selectors.Shop.Equipped)).ToBeVisibleAsync(new() { Timeout = 10_000 });

            var silverFrame = ItemCard(page, "stribrny-ramecek");
            await ClickButtonInAsync(silverFrame.GetByTestId(Selectors.Shop.Buy));
            await Expect(page.GetByTestId(Selectors.Shop.CoinBalance)).ToContainTextAsync("150");
            await ClickButtonInAsync(silverFrame.GetByTestId(Selectors.Shop.Equip));
            await Expect(silverFrame.GetByTestId(Selectors.Shop.Equipped)).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(woodenFrame.GetByTestId(Selectors.Shop.Equipped)).Not.ToBeVisibleAsync();

            var woodenItem = await GetShopItemByNameAsync(user, "Dřevěný rámeček");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            using var duplicateResponse = await apiClient.PostAsJsonAsync(
                "api/v1/shop/purchase",
                new PurchaseRequest(woodenItem.Id));
            duplicateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var duplicate = await duplicateResponse.Content.ReadFromJsonAsync<PurchaseResult>();
            duplicate.Should().NotBeNull();
            duplicate!.Message.Should().Contain("již vlastníte");

            await ClickButtonInAsync(ItemCard(page, "nocni-tema").GetByTestId(Selectors.Shop.Buy));
            await Expect(page.GetByText("Nedostatek mincí")).ToBeVisibleAsync(new() { Timeout = 10_000 });
            await Expect(page.GetByTestId(Selectors.Shop.CoinBalance)).ToContainTextAsync("150");

            await Fixture.TakeCheckpointScreenshotAsync(
                page,
                area: "shop",
                scenario: "purchase-owned-equip-insufficient-duplicate",
                state: "after-equip",
                viewport: "1366x900",
                theme: "light",
                persona: "freeUser");
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Shop_PremiumOnlyItem_FreeUserShowsGateAndApiRejectsPurchase()
    {
        await RunScenarioAsync("shop", "premium-only-free-user-gate", async page =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("shopfreepremium");
            await Fixture.ForceUserCoinsAsync(user.Email, 2_000);
            await Fixture.LoginAsAsync(page, user);
            await Fixture.GoToAndWaitForAppReadyAsync(page, "/shop");

            var premiumCard = ItemCard(page, "diamantovy-avatar");
            await Expect(premiumCard.GetByTestId(Selectors.Shop.PremiumBadge)).ToContainTextAsync("Premium");
            await Expect(premiumCard.GetByTestId(Selectors.Shop.Buy).GetByRole(AriaRole.Button)).ToBeDisabledAsync();

            var item = await GetShopItemByNameAsync(user, "Diamantový avatar");
            using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            using var response = await apiClient.PostAsJsonAsync(
                "api/v1/shop/purchase",
                new PurchaseRequest(item.Id));

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var result = await response.Content.ReadFromJsonAsync<PurchaseResult>();
            result.Should().NotBeNull();
            result!.Message.Should().Contain("Premium");
        }, assertNoFailedRequests: false);
    }

    [Fact]
    public async Task Shop_ConcurrentPurchase_OnlyOneSpendSucceedsAndBalanceNeverNegative()
    {
        await RunScenarioAsync("shop", "concurrent-purchase-single-spend", async _ =>
        {
            var user = await Fixture.RegisterUniqueUserAsync("shopconcurrent");
            await Fixture.ForceUserCoinsAsync(user.Email, 200);
            var item = await GetShopItemByNameAsync(user, "Dřevěný rámeček");

            using var firstClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            using var secondClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            var first = firstClient.PostAsJsonAsync("api/v1/shop/purchase", new PurchaseRequest(item.Id));
            var second = secondClient.PostAsJsonAsync("api/v1/shop/purchase", new PurchaseRequest(item.Id));

            var responses = await Task.WhenAll(first, second);
            responses.Count(r => r.StatusCode == HttpStatusCode.OK).Should().Be(1);
            responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest).Should().Be(1);

            using var statusClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
            var coins = await statusClient.GetFromJsonAsync<CoinBalanceDto>("api/v1/shop/coins");
            coins.Should().NotBeNull();
            coins!.Balance.Should().Be(0);

            var inventory = await statusClient.GetFromJsonAsync<List<InventoryItemDto>>("api/v1/shop/inventory");
            inventory.Should().NotBeNull();
            inventory!.Count(i => i.ShopItemId == item.Id).Should().Be(1);

            foreach (var response in responses)
            {
                response.Dispose();
            }
        }, assertNoConsoleErrors: false, assertNoFailedRequests: false);
    }

    private async Task<ShopItemDto> GetShopItemByNameAsync(TestUser user, string name)
    {
        using var apiClient = await Fixture.CreateAuthenticatedApiClientAsync(user);
        var items = await apiClient.GetFromJsonAsync<List<ShopItemDto>>("api/v1/shop/items");
        items.Should().NotBeNull();
        return items!.Single(i => i.Name == name);
    }

    private static ILocator ItemCard(IPage page, string slug)
    {
        return page.GetByTestId($"shop-item-card-{slug}");
    }

    private static async Task ClickButtonInAsync(ILocator locator)
    {
        await locator.GetByRole(AriaRole.Button).ClickAsync();
    }
}
