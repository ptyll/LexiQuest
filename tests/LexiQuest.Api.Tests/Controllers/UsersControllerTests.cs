using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LexiQuest.Api;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LexiQuest.Api.Tests.Controllers;

public class UsersControllerTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;

    public UsersControllerTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices((context, services) =>
            {
                // Remove all EF Core related services
                var efDescriptors = services
                    .Where(d => d.ServiceType.Namespace?.StartsWith("Microsoft.EntityFrameworkCore") == true ||
                               d.ImplementationType?.Namespace?.StartsWith("Microsoft.EntityFrameworkCore") == true)
                    .ToList();
                
                foreach (var descriptor in efDescriptors)
                {
                    services.Remove(descriptor);
                }

                // Remove DbContextOptions specifically
                var optionsDescriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions) ||
                               d.ServiceType == typeof(DbContextOptions<LexiQuestDbContext>))
                    .ToList();
                
                foreach (var descriptor in optionsDescriptors)
                {
                    services.Remove(descriptor);
                }

                // Add InMemory database
                services.AddDbContext<LexiQuestDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                });
            });
        });
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    private HttpClient CreateClient() => _factory.CreateClient();

    [Fact]
    public async Task RegisterEndpoint_ValidRequest_Returns201Created()
    {
        // Arrange
        var client = CreateClient();
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "Strong1!Pass",
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/users/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result.Should().NotBeNull();
        result!.User.Email.Should().Be(request.Email);
        result.User.Username.Should().Be(request.Username);
    }

    [Fact]
    public async Task RegisterEndpoint_InvalidRequest_Returns400WithValidationErrors()
    {
        // Arrange
        var client = CreateClient();
        var request = new RegisterRequest
        {
            Email = "invalid-email",
            Username = "ab",
            Password = "short",
            ConfirmPassword = "different",
            AcceptTerms = false
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/users/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterEndpoint_DuplicateEmail_Returns409Conflict()
    {
        // Arrange - create new factory for this test to ensure isolation
        var dbName = $"TestDb_{Guid.NewGuid()}";
        var isolatedFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices((context, services) =>
            {
                // Remove all EF Core related services
                var efDescriptors = services
                    .Where(d => d.ServiceType.Namespace?.StartsWith("Microsoft.EntityFrameworkCore") == true ||
                               d.ImplementationType?.Namespace?.StartsWith("Microsoft.EntityFrameworkCore") == true)
                    .ToList();
                
                foreach (var descriptor in efDescriptors)
                {
                    services.Remove(descriptor);
                }

                // Remove DbContextOptions specifically
                var optionsDescriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions) ||
                               d.ServiceType == typeof(DbContextOptions<LexiQuestDbContext>))
                    .ToList();
                
                foreach (var descriptor in optionsDescriptors)
                {
                    services.Remove(descriptor);
                }

                // Add InMemory database with specific name for this test
                services.AddDbContext<LexiQuestDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });
            });
        });
        
        var client = isolatedFactory.CreateClient();
        
        // First register a user
        var firstRequest = new RegisterRequest
        {
            Email = "duplicate@example.com",
            Username = "firstuser",
            Password = "Strong1!Pass",
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true
        };
        var firstResponse = await client.PostAsJsonAsync("/api/v1/users/register", firstRequest);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Try to register another user with same email
        var secondRequest = new RegisterRequest
        {
            Email = "duplicate@example.com",
            Username = "seconduser",
            Password = "Strong1!Pass",
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/users/register", secondRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        
        isolatedFactory.Dispose();
    }
}
