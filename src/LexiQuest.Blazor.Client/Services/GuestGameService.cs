using System.Net;
using System.Net.Http.Json;
using LexiQuest.Shared.DTOs.Game;

namespace LexiQuest.Blazor.Services;

/// <summary>
/// Implementation of guest game service.
/// </summary>
public class GuestGameService : IGuestGameService
{
    private readonly HttpClient _httpClient;

    public GuestGameService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
    }

    public async Task<GuestStartResponse?> StartGameAsync()
    {
        var response = await _httpClient.PostAsync("api/v1/game/guest/start", null);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GuestStartResponse>();
    }

    public async Task<GuestStartResponse?> GetSessionAsync(Guid sessionId)
    {
        var response = await _httpClient.GetAsync($"api/v1/game/guest/status?sessionId={sessionId}");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<GuestStartResponse>();
    }

    public async Task<GuestAnswerResponse?> SubmitAnswerAsync(Guid sessionId, Guid wordId, string answer)
    {
        var request = new GuestAnswerRequest(sessionId, wordId, answer);
        var response = await _httpClient.PostAsJsonAsync("api/v1/game/guest/answer", request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<GuestAnswerResponse>();
    }
}
