using LexiQuest.Core.Interfaces.Services;

namespace LexiQuest.Infrastructure.Services;

public class MockEmailService : IEmailService
{
    public Task SendPasswordResetEmailAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        // Mock implementation - in production this would send real email
        return Task.CompletedTask;
    }

    public Task SendWelcomeEmailAsync(string toEmail, string username, CancellationToken cancellationToken = default)
    {
        // Mock implementation - in production this would send real email
        return Task.CompletedTask;
    }

    public Task SendNotificationEmailAsync(string toEmail, string title, string message, CancellationToken cancellationToken = default)
    {
        // Mock implementation - in production this would send real email
        return Task.CompletedTask;
    }
}
