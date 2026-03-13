namespace LexiQuest.Shared.DTOs;

/// <summary>
/// DTO for reporting client-side errors to the server.
/// </summary>
public record ClientErrorDto(
    string Message,
    string? StackTrace,
    string? ComponentName,
    string? UserId,
    DateTime Timestamp,
    string? Url);
