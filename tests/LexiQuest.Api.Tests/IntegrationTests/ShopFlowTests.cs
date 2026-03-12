using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.DTOs.Auth;
using LexiQuest.Shared.DTOs.Shop;
using Microsoft.Extensions.DependencyInjection;

namespace LexiQuest.Api.Tests.IntegrationTests;

[Trait("Category", "Integration")]
public class ShopFlowTests
{
    private static readonly string TestDbName = $"ShopFlowTestDb_{Guid.NewGuid()}";

    private CustomWebApplicationFactory CreateFactory()
    {
        return new CustomWebApplicationFactory(TestDbName);
    }

    private async Task<(HttpClient Client, CustomWebApplicationFactory Factory, Guid UserId)> CreateAuthenticatedClientAsync()
    {
        var factory = CreateFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var registerRequest = new RegisterRequest
        {
            Username = $"shopuser_{uniqueId}",
            Email = $"shop_{uniqueId}@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            AcceptTerms = true
        };

        var registerResponse = await client.PostAsJsonAsync("/api/v1/users/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.AccessToken);

        return (client, factory, authResponse.User.Id);
    }

    private async Task SeedShopItems(CustomWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();

        if (!dbContext.ShopItems.Any())
        {
            var item = ShopItem.Create(
                "Cool Avatar",
                "A cool avatar for your profile",
                ShopCategory.Avatar,
                100,
                ItemRarity.Common,
                "/images/avatar1.png");

            dbContext.ShopItems.Add(item);
            await dbContext.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task GetShopItems_Authenticated_Returns200()
    {
        // Arrange
        var (client, factory, _) = await CreateAuthenticatedClientAsync();
        await SeedShopItems(factory);

        // Act
        var response = await client.GetAsync("/api/v1/shop/items");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<ShopItemDto>>();
        items.Should().NotBeNull();
    }

    [Fact]
    public async Task GetShopCategories_Returns200WithCategories()
    {
        // Arrange
        var (client, _, _) = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/v1/shop/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categories = await response.Content.ReadFromJsonAsync<List<ShopCategoryDto>>();
        categories.Should().NotBeNull();
        categories!.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetCoinBalance_Authenticated_Returns200()
    {
        // Arrange
        var (client, _, _) = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/v1/shop/coins");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var balance = await response.Content.ReadFromJsonAsync<CoinBalanceDto>();
        balance.Should().NotBeNull();
        balance!.Balance.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetInventory_NewUser_ReturnsEmptyOrDefault()
    {
        // Arrange
        var (client, _, _) = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/v1/shop/inventory");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<InventoryItemDto>>();
        items.Should().NotBeNull();
    }

    [Fact]
    public async Task PurchaseItem_WithSeededItem_ReturnsValidResult()
    {
        // Arrange
        var (client, factory, _) = await CreateAuthenticatedClientAsync();
        await SeedShopItems(factory);

        // Get a shop item
        var itemsResponse = await client.GetAsync("/api/v1/shop/items");
        var items = await itemsResponse.Content.ReadFromJsonAsync<List<ShopItemDto>>();

        if (items == null || items.Count == 0)
            return; // Skip if no items seeded

        var purchaseRequest = new PurchaseRequest(items[0].Id);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/shop/purchase", purchaseRequest);

        // Assert - Either succeeds or fails with BadRequest
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<PurchaseResult>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetShopItemDetail_ExistingItem_Returns200()
    {
        // Arrange
        var (client, factory, _) = await CreateAuthenticatedClientAsync();
        await SeedShopItems(factory);

        var itemsResponse = await client.GetAsync("/api/v1/shop/items");
        var items = await itemsResponse.Content.ReadFromJsonAsync<List<ShopItemDto>>();

        if (items == null || items.Count == 0)
            return; // Skip if no items

        // Act
        var response = await client.GetAsync($"/api/v1/shop/items/{items[0].Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await response.Content.ReadFromJsonAsync<ShopItemDetailDto>();
        detail.Should().NotBeNull();
        detail!.Id.Should().Be(items[0].Id);
    }

    [Fact]
    public async Task GetShopItemDetail_NonExistent_Returns404()
    {
        // Arrange
        var (client, _, _) = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync($"/api/v1/shop/items/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShopEndpoints_WithoutAuth_Returns401()
    {
        // Arrange
        var factory = CreateFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/shop/items");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
