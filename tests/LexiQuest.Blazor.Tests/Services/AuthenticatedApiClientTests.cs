using System.Net;
using FluentAssertions;
using LexiQuest.Blazor.Services;
using NSubstitute;

namespace LexiQuest.Blazor.Tests.Services;

public class AuthenticatedApiClientTests
{
    private readonly RecordingHttpMessageHandler _handler = new();
    private readonly IHttpClientFactory _httpClientFactory = Substitute.For<IHttpClientFactory>();
    private readonly IAuthService _authService = Substitute.For<IAuthService>();

    public AuthenticatedApiClientTests()
    {
        var httpClient = new HttpClient(_handler)
        {
            BaseAddress = new Uri("https://localhost:5000/")
        };

        _httpClientFactory.CreateClient("PublicApiClient").Returns(httpClient);
    }

    [Fact]
    public async Task GetAsync_AuthenticatedUser_SendsBearerToken()
    {
        _authService.GetTokenAsync().Returns("current-token");
        _handler.Enqueue(HttpStatusCode.OK);
        var client = new AuthenticatedApiClient(_httpClientFactory, _authService);

        using var response = await client.GetAsync("api/v1/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var request = _handler.Requests.Should().ContainSingle().Subject;
        request.Headers.Authorization.Should().NotBeNull();
        request.Headers.Authorization!.Scheme.Should().Be("Bearer");
        request.Headers.Authorization.Parameter.Should().Be("current-token");
    }

    [Fact]
    public async Task GetAsync_Unauthorized_RefreshesTokenAndRetriesWithNewBearerToken()
    {
        _authService.GetTokenAsync().Returns("expired-token", "fresh-token");
        _authService.RefreshTokenAsync().Returns(new AuthResult { Success = true });
        _handler.Enqueue(HttpStatusCode.Unauthorized);
        _handler.Enqueue(HttpStatusCode.OK);
        var client = new AuthenticatedApiClient(_httpClientFactory, _authService);

        using var response = await client.GetAsync("api/v1/paths");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _handler.Requests.Should().HaveCount(2);
        _handler.Requests[0].Headers.Authorization!.Parameter.Should().Be("expired-token");
        _handler.Requests[1].Headers.Authorization!.Parameter.Should().Be("fresh-token");
        await _authService.Received(1).RefreshTokenAsync();
    }

    private sealed class RecordingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<HttpStatusCode> _responses = new();

        public List<HttpRequestMessage> Requests { get; } = [];

        public void Enqueue(HttpStatusCode statusCode) => _responses.Enqueue(statusCode);

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            var statusCode = _responses.Count > 0 ? _responses.Dequeue() : HttpStatusCode.OK;
            return Task.FromResult(new HttpResponseMessage(statusCode));
        }
    }
}
