namespace LexiQuest.Blazor.Services;

/// <summary>
/// Service for sending client-side errors to the server for centralized logging.
/// </summary>
public interface IErrorLoggingService
{
    /// <summary>
    /// Logs an error by sending it to the server-side logging endpoint.
    /// </summary>
    Task LogErrorAsync(string message, string? stackTrace, string? componentName, string? userId);
}
