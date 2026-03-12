using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.DTOs.Admin;
using LexiQuest.Shared.DTOs.Auth;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace LexiQuest.Api.Tests.IntegrationTests;

[Trait("Category", "Integration")]
public class AdminFlowTests
{
    private static readonly string TestDbName = $"AdminFlowTestDb_{Guid.NewGuid()}";

    private CustomWebApplicationFactory CreateFactory()
    {
        return new CustomWebApplicationFactory(TestDbName);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(CustomWebApplicationFactory? existingFactory = null)
    {
        var factory = existingFactory ?? CreateFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var registerRequest = new RegisterRequest
        {
            Username = $"adminuser_{uniqueId}",
            Email = $"admin_{uniqueId}@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            AcceptTerms = true
        };

        var registerResponse = await client.PostAsJsonAsync("/api/v1/users/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.AccessToken);

        return client;
    }

    [Fact]
    public async Task AdminWords_WithoutAuth_Returns401()
    {
        // Arrange
        var factory = CreateFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/admin/words");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminWords_NonAdminUser_Returns403()
    {
        // Arrange - Regular user (not admin)
        var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/v1/admin/words");

        // Assert - Regular user should get 403 Forbidden
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminUsers_WithoutAuth_Returns401()
    {
        // Arrange
        var factory = CreateFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/admin/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminUsers_NonAdminUser_Returns403()
    {
        // Arrange - Regular user
        var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/v1/admin/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminCreateWord_NonAdminUser_Returns403()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();
        var createRequest = new AdminWordCreateRequest("NEWWORD", "Beginner", "General");

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/admin/words", createRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminSuspendUser_NonAdminUser_Returns403()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.PostAsync($"/api/v1/admin/users/{Guid.NewGuid()}/suspend", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminWordStats_NonAdminUser_Returns403()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/v1/admin/words/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminExportWords_NonAdminUser_Returns403()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/v1/admin/words/export");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
