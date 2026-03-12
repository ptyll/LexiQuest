using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.DTOs.Auth;
using LexiQuest.Shared.DTOs.Dictionaries;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace LexiQuest.Api.Tests.IntegrationTests;

[Trait("Category", "Integration")]
public class DictionaryFlowTests
{
    private static readonly string TestDbName = $"DictFlowTestDb_{Guid.NewGuid()}";

    private CustomWebApplicationFactory CreateFactory()
    {
        return new CustomWebApplicationFactory(TestDbName);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var factory = CreateFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var registerRequest = new RegisterRequest
        {
            Username = $"dictuser_{uniqueId}",
            Email = $"dict_{uniqueId}@example.com",
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
    public async Task CreateDictionary_GetDictionary_FullFlow()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();
        var createRequest = new CreateDictionaryRequest("My Test Dictionary", "A dictionary for testing");

        // Act - Create
        var createResponse = await client.PostAsJsonAsync("/api/dictionaries", createRequest);

        // Assert - Created
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var dictionary = await createResponse.Content.ReadFromJsonAsync<DictionaryDto>();
        dictionary.Should().NotBeNull();
        dictionary!.Name.Should().Be("My Test Dictionary");
        dictionary.Description.Should().Be("A dictionary for testing");

        // Act - Get by ID
        var getResponse = await client.GetAsync($"/api/dictionaries/{dictionary.Id}");

        // Assert - Found
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<DictionaryDto>();
        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(dictionary.Id);
        fetched.Name.Should().Be("My Test Dictionary");
    }

    [Fact]
    public async Task CreateDictionary_AddWord_WordIsCreated()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();
        var createRequest = new CreateDictionaryRequest("Word Dict", "For adding words");

        var createResponse = await client.PostAsJsonAsync("/api/dictionaries", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var dictionary = await createResponse.Content.ReadFromJsonAsync<DictionaryDto>();

        // Act - Add word
        var addWordRequest = new AddWordRequest("TESTWORD", DifficultyLevel.Beginner);
        var addResponse = await client.PostAsJsonAsync($"/api/dictionaries/{dictionary!.Id}/words", addWordRequest);

        // Assert - Word added
        addResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var word = await addResponse.Content.ReadFromJsonAsync<DictionaryWordDto>();
        word.Should().NotBeNull();
        word!.Word.Should().BeEquivalentTo("TESTWORD");
        word.Difficulty.Should().Be(DifficultyLevel.Beginner);
    }

    [Fact]
    public async Task CreateDictionary_DeleteDictionary_Returns204()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();
        var createRequest = new CreateDictionaryRequest("To Delete", "Will be deleted");

        var createResponse = await client.PostAsJsonAsync("/api/dictionaries", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var dictionary = await createResponse.Content.ReadFromJsonAsync<DictionaryDto>();

        // Act - Delete
        var deleteResponse = await client.DeleteAsync($"/api/dictionaries/{dictionary!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deleted
        var getResponse = await client.GetAsync($"/api/dictionaries/{dictionary.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMyDictionaries_ReturnsUserDictionaries()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();

        await client.PostAsJsonAsync("/api/dictionaries", new CreateDictionaryRequest("Dict 1", "First"));
        await client.PostAsJsonAsync("/api/dictionaries", new CreateDictionaryRequest("Dict 2", "Second"));

        // Act
        var response = await client.GetAsync("/api/dictionaries/my");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dictionaries = await response.Content.ReadFromJsonAsync<List<DictionaryDto>>();
        dictionaries.Should().NotBeNull();
        dictionaries!.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetPublicDictionaries_NoAuth_Returns200()
    {
        // Arrange
        var factory = CreateFactory();
        var client = factory.CreateClient();

        // Act - No auth needed for public dictionaries
        var response = await client.GetAsync("/api/dictionaries/public");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetDictionary_NonExistent_Returns404()
    {
        // Arrange
        var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync($"/api/dictionaries/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AccessDictionary_WithoutAuth_Returns401()
    {
        // Arrange
        var factory = CreateFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/dictionaries/my");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
