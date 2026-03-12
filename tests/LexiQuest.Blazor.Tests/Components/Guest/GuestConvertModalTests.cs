using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components.Guest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Tempo.Blazor.Localization;
using Xunit;

namespace LexiQuest.Blazor.Tests.Components.Guest;

/// <summary>
/// Tests for GuestConvertModal component (T-302.4).
/// </summary>
public class GuestConvertModalTests : TestContext
{
    private readonly IStringLocalizer<GuestConvertModal> _localizer;

    public GuestConvertModalTests()
    {
        _localizer = Substitute.For<IStringLocalizer<GuestConvertModal>>();
        _localizer["Title"].Returns(new LocalizedString("Title", "Hra dokončena!"));
        _localizer["Description"].Returns(new LocalizedString("Description", "Gratulujeme! Úspěšně jste dokončili hru."));
        _localizer["YourResults"].Returns(new LocalizedString("YourResults", "Vaše výsledky"));
        _localizer["WordsSolved"].Returns(new LocalizedString("WordsSolved", "Vyřešená slova: {0}"));
        _localizer["TotalXp"].Returns(new LocalizedString("TotalXp", "Celkem XP: {0}"));
        _localizer["SaveProgressDescription"].Returns(new LocalizedString("SaveProgressDescription", "Zaregistrujte se a získejte svých {0} XP!"));
        _localizer["SaveProgress"].Returns(new LocalizedString("SaveProgress", "Uložit pokrok"));
        _localizer["PlayAgain"].Returns(new LocalizedString("PlayAgain", "Hrát znovu"));

        Services.AddSingleton(_localizer);
        Services.AddSingleton(Substitute.For<ITmLocalizer>());
    }

    [Fact]
    public void GuestConvertModal_Renders_WhenIsOpenTrue()
    {
        // Act
        var cut = Render<GuestConvertModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.WordsSolved, 5)
            .Add(p => p.TotalXp, 50));

        // Assert
        cut.Find("[data-testid='guest-convert-modal']").Should().NotBeNull();
    }

    [Fact]
    public void GuestConvertModal_DoesNotRender_WhenIsOpenFalse()
    {
        // Act
        var cut = Render<GuestConvertModal>(parameters => parameters
            .Add(p => p.IsOpen, false)
            .Add(p => p.WordsSolved, 5)
            .Add(p => p.TotalXp, 50));

        // Assert
        cut.FindAll("[data-testid='guest-convert-modal']").Count.Should().Be(0);
    }

    [Fact]
    public void GuestConvertModal_Renders_Title()
    {
        // Act
        var cut = Render<GuestConvertModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.WordsSolved, 5)
            .Add(p => p.TotalXp, 50));

        // Assert
        cut.Find(".modal-header h3").TextContent.Should().Be("Hra dokončena!");
    }

    [Fact]
    public void GuestConvertModal_Renders_WordsSolved()
    {
        // Act
        var cut = Render<GuestConvertModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.WordsSolved, 5)
            .Add(p => p.TotalXp, 50));

        // Assert
        cut.Find(".results-card").TextContent.Should().Contain("Vyřešená slova: 5");
    }

    [Fact]
    public void GuestConvertModal_Renders_TotalXp()
    {
        // Act
        var cut = Render<GuestConvertModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.WordsSolved, 5)
            .Add(p => p.TotalXp, 50));

        // Assert
        cut.Find(".results-card").TextContent.Should().Contain("Celkem XP: 50");
    }

    [Fact]
    public void GuestConvertModal_SaveProgressButton_TriggersOnRegister()
    {
        // Arrange
        var registerClicked = false;
        var cut = Render<GuestConvertModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.WordsSolved, 5)
            .Add(p => p.TotalXp, 50)
            .Add(p => p.OnRegister, () => { registerClicked = true; }));

        // Act
        cut.Find("[data-testid='btn-save-progress'] button").Click();

        // Assert
        registerClicked.Should().BeTrue();
    }

    [Fact]
    public void GuestConvertModal_PlayAgainButton_TriggersOnPlayAgain()
    {
        // Arrange
        var playAgainClicked = false;
        var cut = Render<GuestConvertModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.WordsSolved, 5)
            .Add(p => p.TotalXp, 50)
            .Add(p => p.OnPlayAgain, () => { playAgainClicked = true; }));

        // Act
        cut.Find("[data-testid='btn-play-again'] button").Click();

        // Assert
        playAgainClicked.Should().BeTrue();
    }

    [Fact]
    public void GuestConvertModal_CloseButton_TriggersOnClose()
    {
        // Arrange
        var closeClicked = false;
        var cut = Render<GuestConvertModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.WordsSolved, 5)
            .Add(p => p.TotalXp, 50)
            .Add(p => p.OnClose, () => { closeClicked = true; }));

        // Act
        cut.Find(".btn-close").Click();

        // Assert
        closeClicked.Should().BeTrue();
    }

    [Theory]
    [InlineData(3, 30)]
    [InlineData(5, 55)]
    [InlineData(0, 0)]
    public void GuestConvertModal_Renders_CorrectStats(int wordsSolved, int totalXp)
    {
        // Act
        var cut = Render<GuestConvertModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.WordsSolved, wordsSolved)
            .Add(p => p.TotalXp, totalXp));

        // Assert
        var resultsCard = cut.Find(".results-card");
        resultsCard.TextContent.Should().Contain($"Vyřešená slova: {wordsSolved}");
        resultsCard.TextContent.Should().Contain($"Celkem XP: {totalXp}");
    }
}
