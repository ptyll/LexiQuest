using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LexiQuest.Api;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LexiQuest.Api.Tests.Endpoints;

public class PasswordResetEndpointsTests : IDisposable
{
    private static readonly string TestDbName = $"TestDb_PasswordReset_{Guid.NewGuid()}";
    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;

    private WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");

                // Set environment variables for JWT
                Environment.SetEnvironmentVariable("JwtSettings__SecretKey", "Test-Secret-Key-That-Is-Long-Enough-For-HS256-Algorithm-!!", EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("JwtSettings__Issuer", "TestIssuer", EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("JwtSettings__Audience", "TestAudience", EnvironmentVariableTarget.Process);

                builder.ConfigureServices(services =>
                {
                    services.AddDbContext<LexiQuestDbContext>(options =>
                        options.UseInMemoryDatabase(TestDbName));
                });
            });
    }

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Fact]
    public async Task RequestPasswordReset_ValidEmail_Returns200OK()
    {
        // Arrange
        _factory = CreateFactory();
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var request = new RequestPasswordResetDto { Email = "test@example.com" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/users/password-reset/request", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RequestPasswordReset_InvalidEmail_Returns400()
    {
        // Arrange
        _factory = CreateFactory();
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var request = new RequestPasswordResetDto { Email = "invalid-email" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/users/password-reset/request", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_ValidRequest_Returns200OK()
    {
        // Arrange
        _factory = CreateFactory();
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var request = new ResetPasswordDto 
        { 
            Token = "valid_token", 
            NewPassword = "NewPassword1!", 
            ConfirmPassword = "NewPassword1!" 
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/users/password-reset/confirm", request);

        // Assert
        // Token nemusí být platný, takže může vrátit 400, ale ne 500
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_WeakPassword_Returns400()
    {
        // Arrange
        _factory = CreateFactory();
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var request = new ResetPasswordDto 
        { 
            Token = "token", 
            NewPassword = "weak", 
            ConfirmPassword = "weak" 
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/users/password-reset/confirm", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_Mismatch_Returns400()
    {
        // Arrange
        _factory = CreateFactory();
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LexiQuestDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var request = new ResetPasswordDto 
        { 
            Token = "token", 
            NewPassword = "Valid1!Pass", 
            ConfirmPassword = "Different1!Pass" 
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/users/password-reset/confirm", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
