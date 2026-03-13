using LexiQuest.Core.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace LexiQuest.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken cancellationToken = default)
    {
        var resetUrl = $"{_settings.BaseUrl}/password-reset/{resetToken}";

        var htmlBody = BuildHtmlTemplate(
            _settings.FromName,
            BuildPasswordResetContent(resetUrl));

        await SendEmailAsync(toEmail, $"{_settings.FromName} - Password Reset", htmlBody, cancellationToken);

        _logger.LogInformation("Password reset email sent to {Email}", toEmail);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string username, CancellationToken cancellationToken = default)
    {
        var htmlBody = BuildHtmlTemplate(
            _settings.FromName,
            BuildWelcomeContent(username));

        await SendEmailAsync(toEmail, $"Welcome to {_settings.FromName}!", htmlBody, cancellationToken);

        _logger.LogInformation("Welcome email sent to {Email}", toEmail);
    }

    public async Task SendNotificationEmailAsync(string toEmail, string title, string message, CancellationToken cancellationToken = default)
    {
        var htmlBody = BuildHtmlTemplate(
            _settings.FromName,
            BuildNotificationContent(title, message));

        await SendEmailAsync(toEmail, title, htmlBody, cancellationToken);

        _logger.LogInformation("Notification email sent to {Email}: {Title}", toEmail, title);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();

        try
        {
            var secureSocketOptions = _settings.UseSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await client.ConnectAsync(_settings.Host, _settings.Port, secureSocketOptions, cancellationToken);
            await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email} with subject {Subject}", toEmail, subject);
            throw;
        }
        finally
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync(true, cancellationToken);
            }
        }
    }

    private static string BuildHtmlTemplate(string appName, string content)
    {
        return $$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <style>
                    body { margin: 0; padding: 0; background-color: #f4f4f7; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; }
                    .container { max-width: 600px; margin: 0 auto; padding: 20px; }
                    .card { background-color: #ffffff; border-radius: 8px; padding: 32px; margin-top: 20px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
                    .header { text-align: center; padding-bottom: 20px; border-bottom: 1px solid #eaeaea; margin-bottom: 24px; }
                    .header h1 { color: #6c5ce7; margin: 0; font-size: 24px; }
                    .content { color: #333333; line-height: 1.6; font-size: 16px; }
                    .btn { display: inline-block; background-color: #6c5ce7; color: #ffffff; text-decoration: none; padding: 12px 32px; border-radius: 6px; font-weight: 600; margin: 20px 0; }
                    .footer { text-align: center; color: #999999; font-size: 12px; margin-top: 24px; padding-top: 16px; border-top: 1px solid #eaeaea; }
                </style>
            </head>
            <body>
                <div class="container">
                    <div class="card">
                        <div class="header">
                            <h1>{{appName}}</h1>
                        </div>
                        <div class="content">
                            {{content}}
                        </div>
                        <div class="footer">
                            &copy; {{DateTime.UtcNow.Year}} {{appName}}. All rights reserved.
                        </div>
                    </div>
                </div>
            </body>
            </html>
            """;
    }

    private static string BuildPasswordResetContent(string resetUrl)
    {
        return $"""
            <p>You requested a password reset. Click the button below to set a new password:</p>
            <p style="text-align: center;">
                <a href="{resetUrl}" class="btn">Reset Password</a>
            </p>
            <p>If you did not request this, you can safely ignore this email. The link will expire shortly.</p>
            <p style="font-size: 12px; color: #999;">If the button does not work, copy and paste this URL into your browser:<br />{resetUrl}</p>
            """;
    }

    private static string BuildWelcomeContent(string username)
    {
        return $"""
            <p>Hi <strong>{username}</strong>,</p>
            <p>Welcome aboard! Your account has been created successfully.</p>
            <p>Start your language learning journey now and challenge yourself with word quests, daily challenges, and more.</p>
            """;
    }

    private static string BuildNotificationContent(string title, string message)
    {
        return $"""
            <h2 style="margin-top: 0;">{title}</h2>
            <p>{message}</p>
            """;
    }
}
