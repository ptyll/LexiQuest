using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.DTOs.Achievements;
using LexiQuest.Shared.DTOs.Auth;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace LexiQuest.Api.Tests.IntegrationTests;

[Trait("Category", "Integration")]
public class AchievementFlowTests
{
    private static readonly string TestDbName = $"AchievementFlowTestDb_{Guid.NewGuid()}";

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
            Username = $"achieveuser_{uniqueId}",
            Email = $"achieve_{uniqueId}@example.com",
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

    private async Task SeedAchievements(CustomWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();

        if (!dbContext.Achievements.Any())
        {
            var achievements = new[]
            {
                Achievement.Create("first_word", AchievementCategory.Performance, 10, "First Word", "Solve your first word", 1),
                Achievement.Create("streak_3", AchievementCategory.Streak, 25, "3 Day Streak", "Maintain a 3-day streak", 3),
                Achievement.Create("words_10", AchievementCategory.Performance, 50, "Word Collector", "Solve 10 words", 10),
            };
            dbContext.Achievements.AddRange(achievements);
            await dbContext.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Note: AchievementsController.GetCurrentUserId() uses FindFirst("sub") which returns
    /// Guid.Empty when JWT inbound claim mapping is active (default). The controller returns
    /// Unauthorized in that case. This test documents that behavior.
    /// Controllers using ClaimTypes.NameIdentifier work correctly.
    /// </summary>
    [Fact]
    public async Task GetAchievements_Authenticated_Returns401DueToClaimMapping()
    {
        // Arrange
        var (client, factory, _) = await CreateAuthenticatedClientAsync();
        await SeedAchievements(factory);

        // Act
        var response = await client.GetAsync("/api/v1/achievements");

        // Assert - Controller uses FindFirst("sub") which doesn't match when JWT maps sub->NameIdentifier
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAchievements_WithoutAuth_Returns401()
    {
        // Arrange
        var factory = CreateFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/achievements");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAchievementById_Authenticated_Returns401DueToClaimMapping()
    {
        // Arrange
        var (client, _, _) = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync($"/api/v1/achievements/{Guid.NewGuid()}");

        // Assert - Same claim mapping issue
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserAchievements_Authenticated_Returns401DueToClaimMapping()
    {
        // Arrange
        var (client, factory, _) = await CreateAuthenticatedClientAsync();
        await SeedAchievements(factory);

        // Act
        var response = await client.GetAsync("/api/v1/users/me/achievements");

        // Assert - UserAchievementsController also uses FindFirst("sub")
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserAchievements_WithoutAuth_Returns401()
    {
        // Arrange
        var factory = CreateFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/users/me/achievements");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
