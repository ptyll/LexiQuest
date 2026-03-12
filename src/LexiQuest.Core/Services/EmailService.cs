using LexiQuest.Core.Interfaces.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LexiQuest.Core.Services;

public class EmailService : IEmailService
{
    private readonly IStringLocalizer<EmailService> _localizer;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IStringLocalizer<EmailService> localizer, ILogger<EmailService> logger)
    {
        _localizer = localizer;
        _logger = logger;
    }

    public Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken cancellationToken = default)
    {
        var subject = _localizer["PasswordReset.Subject"];
        var body = string.Format(_localizer["PasswordReset.Body"], $"https://lexiquest.cz/password-reset/{resetToken}");
        
        _logger.LogInformation("Password reset email sent to {Email} with token {Token}", toEmail, resetToken);
        
        // TODO: Implementovat skutečné odeslání emailu (SendGrid/SMTP)
        return Task.CompletedTask;
    }

    public Task SendWelcomeEmailAsync(string toEmail, string username, CancellationToken cancellationToken = default)
    {
        var subject = _localizer["Welcome.Subject"];
        var body = string.Format(_localizer["Welcome.Body"], username);

        _logger.LogInformation("Welcome email sent to {Email}", toEmail);

        // TODO: Implementovat skutečné odeslání emailu (SendGrid/SMTP)
        return Task.CompletedTask;
    }

    public Task SendNotificationEmailAsync(string toEmail, string title, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Notification email sent to {Email}: {Title}", toEmail, title);

        // TODO: Implementovat skutečné odeslání emailu (SendGrid/SMTP)
        return Task.CompletedTask;
    }
}
