using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using LexiQuest.Blazor.Services;
using NSubstitute;

namespace LexiQuest.Blazor.Tests.Services;

public class AuthorizationMessageHandlerTests
{
    private readonly IAuthService _authService;
    private readonly AuthorizationMessageHandler _handler;
    private readonly HttpMessageInvoker _invoker;

    public AuthorizationMessageHandlerTests()
    {
        _authService = Substitute.For<IAuthService>();
        _handler = new AuthorizationMessageHandler(_authService)
        {
            InnerHandler = new TestHandler()
        };
        _invoker = new HttpMessageInvoker(_handler);
    }

    [Fact]
    public async Task SendAsync_UserAuthenticated_AddsAuthorizationHeader()
    {
        // Arrange
        _authService.IsAuthenticatedAsync().Returns(true);
        _authService.GetTokenAsync().Returns("test-jwt-token");
        
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

        // Act
        var response = await _invoker.SendAsync(request, CancellationToken.None);

        // Assert
        request.Headers.Authorization.Should().NotBeNull();
        request.Headers.Authorization!.Scheme.Should().Be("Bearer");
        request.Headers.Authorization.Parameter.Should().Be("test-jwt-token");
    }

    [Fact]
    public async Task SendAsync_UserNotAuthenticated_DoesNotAddAuthorizationHeader()
    {
        // Arrange
        _authService.IsAuthenticatedAsync().Returns(false);
        
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

        // Act
        var response = await _invoker.SendAsync(request, CancellationToken.None);

        // Assert
        request.Headers.Authorization.Should().BeNull();
    }

    [Fact]
    public async Task SendAsync_TokenNull_DoesNotAddAuthorizationHeader()
    {
        // Arrange
        _authService.IsAuthenticatedAsync().Returns(true);
        _authService.GetTokenAsync().Returns((string?)null);
        
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

        // Act
        var response = await _invoker.SendAsync(request, CancellationToken.None);

        // Assert
        request.Headers.Authorization.Should().BeNull();
    }

    [Fact]
    public async Task SendAsync_Receives401_AttemptsTokenRefresh()
    {
        // Arrange
        _authService.IsAuthenticatedAsync().Returns(true);
        _authService.GetTokenAsync().Returns("expired-token");
        _authService.RefreshTokenAsync().Returns(new AuthResult { Success = true });
        
        var handlerWith401 = new AuthorizationMessageHandler(_authService)
        {
            InnerHandler = new TestHandler(statusCode: HttpStatusCode.Unauthorized)
        };
        var invoker = new HttpMessageInvoker(handlerWith401);
        
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

        // Act
        var response = await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        await _authService.Received(1).RefreshTokenAsync();
    }

    [Fact]
    public async Task SendAsync_RefreshFails_Returns401Response()
    {
        // Arrange
        _authService.IsAuthenticatedAsync().Returns(true);
        _authService.GetTokenAsync().Returns("expired-token");
        _authService.RefreshTokenAsync().Returns(new AuthResult { Success = false });
        
        var handlerWith401 = new AuthorizationMessageHandler(_authService)
        {
            InnerHandler = new TestHandler(statusCode: HttpStatusCode.Unauthorized)
        };
        var invoker = new HttpMessageInvoker(handlerWith401);
        
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

        // Act
        var response = await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private class TestHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;

        public TestHandler(HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(_statusCode));
        }
    }
}
