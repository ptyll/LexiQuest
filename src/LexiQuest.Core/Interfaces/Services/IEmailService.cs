namespace LexiQuest.Core.Interfaces.Services;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken cancellationToken = default);
    Task SendWelcomeEmailAsync(string toEmail, string username, CancellationToken cancellationToken = default);
    Task SendNotificationEmailAsync(string toEmail, string title, string message, CancellationToken cancellationToken = default);
}
