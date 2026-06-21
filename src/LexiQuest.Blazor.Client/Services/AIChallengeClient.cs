using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Blazor.Services;

public class AIChallengeClient : IAIChallengeClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public AIChallengeClient(IHttpClientFactory httpClientFactory, IAuthService authService)
    {
        _httpClient = httpClientFactory.CreateClient("PublicApiClient");
        _authService = authService;
    }

    public async Task<PlayerAnalysisDto?> GetAnalysisAsync()
    {
        try
        {
            using var response = await SendAuthorizedAsync(HttpMethod.Get, "api/v1/challenges/ai/analysis");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<PlayerAnalysisDto>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<AIChallengeDto?> GenerateChallengeAsync(AIChallengeType type)
    {
        try
        {
            var request = new AIChallengeRequest(type);
            using var response = await SendAuthorizedAsync(HttpMethod.Post, "api/v1/challenges/ai/start", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AIChallengeDto>();
            }
            return null;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private async Task<HttpResponseMessage> SendAuthorizedAsync(
        HttpMethod method,
        string requestUri,
        object? jsonBody = null)
    {
        var response = await SendAuthorizedOnceAsync(method, requestUri, jsonBody);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        response.Dispose();
        var refresh = await _authService.RefreshTokenAsync();
        if (!refresh.Success)
        {
            return await SendAuthorizedOnceAsync(method, requestUri, jsonBody);
        }

        return await SendAuthorizedOnceAsync(method, requestUri, jsonBody);
    }

    private async Task<HttpResponseMessage> SendAuthorizedOnceAsync(
        HttpMethod method,
        string requestUri,
        object? jsonBody)
    {
        using var request = new HttpRequestMessage(method, requestUri);
        if (jsonBody is not null)
        {
            request.Content = JsonContent.Create(jsonBody, options: JsonOptions);
        }

        var token = await _authService.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await _httpClient.SendAsync(request);
    }
}
