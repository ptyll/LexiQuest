using LexiQuest.Shared.Enums;

namespace LexiQuest.Api.Testing;

public sealed record E2ESendNotificationRequest(
    string Email,
    NotificationType Type,
    string Title,
    string Message,
    NotificationSeverity Severity,
    string? ActionUrl);
