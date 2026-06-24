using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace LexiQuest.Blazor.Services;

public interface IAuthenticatedApiClient
{
    Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default);
    Task<T?> GetFromJsonAsync<T>(string requestUri, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PostAsync(string requestUri, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PostAsJsonAsync<TValue>(
        string requestUri,
        TValue value,
        CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PutAsJsonAsync<TValue>(
        string requestUri,
        TValue value,
        CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default);
}

public class AuthenticatedApiClient : IAuthenticatedApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public AuthenticatedApiClient(IHttpClientFactory httpClientFactory, IAuthService authService)
    {
        _httpClient = httpClientFactory.CreateClient("PublicApiClient");
        _authService = authService;
    }

    public Task<HttpResponseMessage> GetAsync(
        string requestUri,
        CancellationToken cancellationToken = default) =>
        SendWithRefreshAsync(() => new HttpRequestMessage(HttpMethod.Get, requestUri), cancellationToken);

    public async Task<T?> GetFromJsonAsync<T>(
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        using var response = await GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
    }

    public Task<HttpResponseMessage> PostAsync(
        string requestUri,
        CancellationToken cancellationToken = default) =>
        SendWithRefreshAsync(() => new HttpRequestMessage(HttpMethod.Post, requestUri), cancellationToken);

    public Task<HttpResponseMessage> PostAsJsonAsync<TValue>(
        string requestUri,
        TValue value,
        CancellationToken cancellationToken = default) =>
        SendWithRefreshAsync(
            () => new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = JsonContent.Create(value)
            },
            cancellationToken);

    public Task<HttpResponseMessage> PutAsJsonAsync<TValue>(
        string requestUri,
        TValue value,
        CancellationToken cancellationToken = default) =>
        SendWithRefreshAsync(
            () => new HttpRequestMessage(HttpMethod.Put, requestUri)
            {
                Content = JsonContent.Create(value)
            },
            cancellationToken);

    public Task<HttpResponseMessage> DeleteAsync(
        string requestUri,
        CancellationToken cancellationToken = default) =>
        SendWithRefreshAsync(() => new HttpRequestMessage(HttpMethod.Delete, requestUri), cancellationToken);

    private async Task<HttpResponseMessage> SendWithRefreshAsync(
        Func<HttpRequestMessage> requestFactory,
        CancellationToken cancellationToken)
    {
        var response = await SendOnceAsync(requestFactory(), cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        var refreshResult = await _authService.RefreshTokenAsync();
        if (!refreshResult.Success)
        {
            return response;
        }

        response.Dispose();
        return await SendOnceAsync(requestFactory(), cancellationToken);
    }

    private async Task<HttpResponseMessage> SendOnceAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _authService.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await _httpClient.SendAsync(request, cancellationToken);
    }
}
