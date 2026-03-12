using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using LexiQuest.Api.Extensions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.DTOs.Auth;
using LexiQuest.Shared.DTOs.Dictionaries;
using LexiQuest.Shared.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace LexiQuest.Api.Tests.Controllers;

public class DictionaryControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly string TestDbName = $"TestDb_Dict_{Guid.NewGuid()}";

    private WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");

                builder.ConfigureServices(services =>
                {
                    services.AddDbContext<LexiQuestDbContext>(options =>
                        options.UseInMemoryDatabase(TestDbName));
                });
            });
    }

    private async Task<(HttpClient Client, WebApplicationFactory<Program> Factory, AuthResponse Auth)> CreateAuthenticatedClientAsync()
    {
        var factory = CreateFactory();
        var client = factory.CreateClient();

        // Ensure database is created and seed words
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Seed test words if not exists
        if (!dbContext.Words.Any())
        {
            var words = new[]
            {
                Word.Create("JABLKO", DifficultyLevel.Beginner, WordCategory.Food, 1),
                Word.Create("BANÁN", DifficultyLevel.Beginner, WordCategory.Food, 2),
            };
            dbContext.Words.AddRange(words);
            await dbContext.SaveChangesAsync();
        }

        // Register and login
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var registerRequest = new RegisterRequest
        {
            Username = $"testuser_{uniqueId}",
            Email = $"test_{uniqueId}@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            AcceptTerms = true
        };

        var registerResponse = await client.PostAsJsonAsync("/api/v1/users/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();

        // Set authorization header
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.AccessToken);

        return (client, factory, authResponse);
    }

    [Fact]
    public async Task GetMyDictionaries_ReturnsUserDictionaries()
    {
        // Arrange
        var (client, _, _) = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/dictionaries/my");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<DictionaryDto>>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPublicDictionaries_ReturnsPublicDictionaries()
    {
        // Arrange
        var (client, _, _) = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/dictionaries/public");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<DictionaryDto>>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateDictionary_ValidRequest_CreatesDictionary()
    {
        // Arrange
        var (client, _, _) = await CreateAuthenticatedClientAsync();
        var request = new CreateDictionaryRequest("Nový slovník", "Popis");

        // Act
        var response = await client.PostAsJsonAsync("/api/dictionaries", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<DictionaryDto>();
        result!.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task GetDictionary_Existing_ReturnsDictionary()
    {
        // Arrange
        var (client, _, _) = await CreateAuthenticatedClientAsync();
        var createRequest = new CreateDictionaryRequest("Test slovník", "Test popis");
        var createResponse = await client.PostAsJsonAsync("/api/dictionaries", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<DictionaryDto>();

        // Act
        var response = await client.GetAsync($"/api/dictionaries/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DictionaryDto>();
        result!.Name.Should().Be("Test slovník");
    }

    [Fact]
    public async Task GetDictionary_NotFound_Returns404()
    {
        // Arrange
        var (client, _, _) = await CreateAuthenticatedClientAsync();
        var dictionaryId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/dictionaries/{dictionaryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteDictionary_Owner_DeletesSuccessfully()
    {
        // Arrange
        var (client, _, _) = await CreateAuthenticatedClientAsync();
        var createRequest = new CreateDictionaryRequest("Ke smazání", "Popis");
        var createResponse = await client.PostAsJsonAsync("/api/dictionaries", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<DictionaryDto>();

        // Act
        var response = await client.DeleteAsync($"/api/dictionaries/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteDictionary_NotOwner_Returns403()
    {
        // Arrange
        var (client1, _, _) = await CreateAuthenticatedClientAsync();
        var createRequest = new CreateDictionaryRequest("Cizí slovník", "Popis");
        var createResponse = await client1.PostAsJsonAsync("/api/dictionaries", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<DictionaryDto>();

        // Create second user
        var (client2, _, _) = await CreateAuthenticatedClientAsync();

        // Act - second user tries to delete first user's dictionary
        var response = await client2.DeleteAsync($"/api/dictionaries/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AddWord_ValidRequest_AddsWord()
    {
        // Arrange
        var (client, _, _) = await CreateAuthenticatedClientAsync();
        var createRequest = new CreateDictionaryRequest("Slovník", "Popis");
        var createResponse = await client.PostAsJsonAsync("/api/dictionaries", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<DictionaryDto>();
        
        var request = new AddWordRequest("slunce", DifficultyLevel.Intermediate);

        // Act
        var response = await client.PostAsJsonAsync($"/api/dictionaries/{created!.Id}/words", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task ImportCsv_ValidContent_ImportsWords()
    {
        // Arrange
        var (client, _, _) = await CreateAuthenticatedClientAsync();
        var createRequest = new CreateDictionaryRequest("Import slovník", "Popis");
        var createResponse = await client.PostAsJsonAsync("/api/dictionaries", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<DictionaryDto>();
        
        var csvContent = "pes,Beginner\nslunce,Intermediate";

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/dictionaries/{created!.Id}/import-csv", 
            new { Content = csvContent });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ImportTxt_ValidContent_ImportsWords()
    {
        // Arrange
        var (client, _, _) = await CreateAuthenticatedClientAsync();
        var createRequest = new CreateDictionaryRequest("Import slovník", "Popis");
        var createResponse = await client.PostAsJsonAsync("/api/dictionaries", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<DictionaryDto>();
        
        var txtContent = "pes\nslunce\nměsíc";

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/dictionaries/{created!.Id}/import-txt", 
            new { Content = txtContent });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
