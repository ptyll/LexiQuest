using FluentAssertions;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LexiQuest.Core.Tests.Services;

public class EmailServiceTests
{
    private readonly IStringLocalizer<EmailService> _localizer;
    private readonly ILogger<EmailService> _logger;
    private readonly EmailService _sut;

    public EmailServiceTests()
    {
        _localizer = Substitute.For<IStringLocalizer<EmailService>>();
        _logger = Substitute.For<ILogger<EmailService>>();
        
        _localizer["PasswordReset.Subject"].Returns(new LocalizedString("PasswordReset.Subject", "Obnova hesla - LexiQuest"));
        _localizer["PasswordReset.Body"].Returns(new LocalizedString("PasswordReset.Body", "Klikněte na odkaz pro obnovu hesla: {0}"));
        _localizer["Welcome.Subject"].Returns(new LocalizedString("Welcome.Subject", "Vítejte v LexiQuest!"));
        _localizer["Welcome.Body"].Returns(new LocalizedString("Welcome.Body", "Dobrý den {0}, vítejte v LexiQuest!"));

        _sut = new EmailService(_localizer, _logger);
    }

    [Fact]
    public async Task EmailService_SendPasswordReset_SendsEmail()
    {
        // Arrange
        var email = "test@example.com";
        var token = "reset_token_123";

        // Act
        await _sut.SendPasswordResetEmailAsync(email, token);

        // Assert
        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Password reset email")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task EmailService_SendWelcome_SendsEmail()
    {
        // Arrange
        var email = "test@example.com";
        var username = "testuser";

        // Act
        await _sut.SendWelcomeEmailAsync(email, username);

        // Assert
        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Welcome email")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
