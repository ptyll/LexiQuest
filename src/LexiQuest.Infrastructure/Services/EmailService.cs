using LexiQuest.Core.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Globalization;
using System.Net;
using System.Resources;

namespace LexiQuest.Infrastructure.Services;

public class EmailService : IEmailService
{
    private static readonly ResourceManager PasswordResetResources = new(
        "LexiQuest.Infrastructure.Resources.Email.PasswordResetEmail",
        typeof(EmailService).Assembly);

    private static readonly ResourceManager WelcomeResources = new(
        "LexiQuest.Infrastructure.Resources.Email.WelcomeEmail",
        typeof(EmailService).Assembly);

    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> settings,
        ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken cancellationToken = default)
    {
        var resetUrl = $"{_settings.BaseUrl}/password-reset/{resetToken}";
        var subject = PasswordReset("Subject");

        var htmlBody = BuildHtmlTemplate(
            _settings.FromName,
            BuildPasswordResetContent(toEmail, resetUrl),
            PasswordReset("Footer"));
        var textBody = BuildPasswordResetText(toEmail, resetUrl);

        await SendEmailAsync(toEmail, subject, htmlBody, textBody, cancellationToken);

        _logger.LogInformation("Password reset email sent to {Email}", toEmail);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string username, CancellationToken cancellationToken = default)
    {
        var subject = Welcome("Subject");

        var htmlBody = BuildHtmlTemplate(
            _settings.FromName,
            BuildWelcomeContent(username),
            Welcome("Footer"));
        var textBody = BuildWelcomeText(username);

        await SendEmailAsync(toEmail, subject, htmlBody, textBody, cancellationToken);

        _logger.LogInformation("Welcome email sent to {Email}", toEmail);
    }

    public async Task SendNotificationEmailAsync(string toEmail, string title, string message, CancellationToken cancellationToken = default)
    {
        var htmlBody = BuildHtmlTemplate(
            _settings.FromName,
            BuildNotificationContent(title, message),
            Welcome("Footer"));
        var textBody = $"{title}{Environment.NewLine}{Environment.NewLine}{message}";

        await SendEmailAsync(toEmail, title, htmlBody, textBody, cancellationToken);

        _logger.LogInformation("Notification email sent to {Email}: {Title}", toEmail, title);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody, string textBody, CancellationToken cancellationToken)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = textBody
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();

        try
        {
            var secureSocketOptions = _settings.UseSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await client.ConnectAsync(_settings.Host, _settings.Port, secureSocketOptions, cancellationToken);
            if (!string.IsNullOrWhiteSpace(_settings.Username))
            {
                await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            }

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

    private static string BuildHtmlTemplate(string appName, string content, string footer)
    {
        var encodedAppName = Html(appName);
        var encodedFooter = Html(footer);

        return $$"""
            <!DOCTYPE html>
            <html lang="cs">
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
                            <h1>{{encodedAppName}}</h1>
                        </div>
                        <div class="content">
                            {{content}}
                        </div>
                        <div class="footer">
                            &copy; {{DateTime.UtcNow.Year.ToString(CultureInfo.InvariantCulture)}} {{encodedAppName}}. {{encodedFooter}}
                        </div>
                    </div>
                </div>
            </body>
            </html>
            """;
    }

    private string BuildPasswordResetContent(string toEmail, string resetUrl)
    {
        var encodedUrl = Html(resetUrl);

        return $"""
            <p>{Html(PasswordReset("Greeting", toEmail))}</p>
            <p>{Html(PasswordReset("Body"))}</p>
            <p style="text-align: center;">
                <a href="{encodedUrl}" class="btn">{Html(PasswordReset("Action"))}</a>
            </p>
            <p>{Html(PasswordReset("Expiry", 1))}</p>
            <p>{Html(PasswordReset("Ignore"))}</p>
            <p style="font-size: 12px; color: #999;">{Html(PasswordReset("FallbackUrl"))}<br />{encodedUrl}</p>
            """;
    }

    private string BuildPasswordResetText(string toEmail, string resetUrl)
    {
        return $"""
            {PasswordReset("Greeting", toEmail)}

            {PasswordReset("Body")}
            {PasswordReset("Action")}: {resetUrl}
            {PasswordReset("Expiry", 1)}
            {PasswordReset("Ignore")}
            """;
    }

    private string BuildWelcomeContent(string username)
    {
        return $"""
            <p>{Html(Welcome("Greeting", username))}</p>
            <p>{Html(Welcome("Body"))}</p>
            <p style="text-align: center;">
                <a href="{Html(_settings.BaseUrl)}" class="btn">{Html(Welcome("Action"))}</a>
            </p>
            """;
    }

    private string BuildWelcomeText(string username)
    {
        return $"""
            {Welcome("Greeting", username)}

            {Welcome("Body")}
            {Welcome("Action")}: {_settings.BaseUrl}
            """;
    }

    private static string BuildNotificationContent(string title, string message)
    {
        return $"""
            <h2 style="margin-top: 0;">{Html(title)}</h2>
            <p>{Html(message)}</p>
            """;
    }

    private static string PasswordReset(string key, params object[] args) => FormatResource(PasswordResetResources, key, args);

    private static string Welcome(string key, params object[] args) => FormatResource(WelcomeResources, key, args);

    private static string FormatResource(ResourceManager resourceManager, string key, params object[] args)
    {
        var value = resourceManager.GetString(key, CultureInfo.CurrentUICulture)
            ?? throw new InvalidOperationException($"Missing email resource '{key}'.");

        return args.Length == 0
            ? value
            : string.Format(CultureInfo.CurrentCulture, value, args);
    }

    private static string Html(string value) => WebUtility.HtmlEncode(value);
}
