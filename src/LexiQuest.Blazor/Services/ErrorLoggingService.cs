using System.Net.Http.Json;
using LexiQuest.Shared.DTOs;
using Microsoft.AspNetCore.Components;

namespace LexiQuest.Blazor.Services;

/// <summary>
/// Implementation of error logging service that posts client-side errors to the API.
/// </summary>
public class ErrorLoggingService : IErrorLoggingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ErrorLoggingService> _logger;
    private readonly NavigationManager _navigationManager;

    public ErrorLoggingService(
        IHttpClientFactory httpClientFactory,
        ILogger<ErrorLoggingService> logger,
        NavigationManager navigationManager)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
        _logger = logger;
        _navigationManager = navigationManager;
    }

    /// <inheritdoc />
    public async Task LogErrorAsync(string message, string? stackTrace, string? componentName, string? userId)
    {
        try
        {
            var dto = new ClientErrorDto(
                Message: message,
                StackTrace: stackTrace,
                ComponentName: componentName,
                UserId: userId,
                Timestamp: DateTime.UtcNow,
                Url: _navigationManager.Uri);

            await _httpClient.PostAsJsonAsync("api/v1/client-errors", dto);
        }
        catch (Exception ex)
        {
            // Avoid recursive error logging - only log locally
            _logger.LogWarning(ex, "Failed to send client error to server");
        }
    }
}
