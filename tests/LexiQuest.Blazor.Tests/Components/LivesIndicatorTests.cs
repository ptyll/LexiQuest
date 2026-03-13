using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components.Game;
using LexiQuest.Shared.DTOs.Game;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using LexiQuest.Blazor.Tests.Helpers;
using NSubstitute;

namespace LexiQuest.Blazor.Tests.Components;

public class LivesIndicatorTests : BunitContext
{
    private readonly IStringLocalizer<LivesIndicator> _localizer;

    public LivesIndicatorTests()
    {
        _localizer = Substitute.For<IStringLocalizer<LivesIndicator>>();
        _localizer["Lives.Label"].Returns(new LocalizedString("Lives.Label", "Životy"));
        _localizer["Tooltip.Full"].Returns(new LocalizedString("Tooltip.Full", "Všechny životy"));
        _localizer["Tooltip.RegenIn"].Returns(new LocalizedString("Tooltip.RegenIn", "Další život za"));
        
        Services.AddSingleton(_localizer);
        TempoTestHelper.RegisterTempoServices(Services);
    }

    [Fact]
    public void LivesIndicator_Renders_CorrectNumberOfHearts()
    {
        // Arrange
        var livesStatus = new LivesStatus(
            Current: 3,
            Max: 5,
            NextRegenAt: DateTime.UtcNow.AddMinutes(15),
            IsInfinite: false
        );

        // Act
        var cut = Render<LivesIndicator>(parameters => parameters
            .Add(p => p.Lives, livesStatus)
        );

        // Assert
        var hearts = cut.FindAll(".lives-heart");
        hearts.Count.Should().Be(5); // Max 5 hearts
        
        var filledHearts = cut.FindAll(".lives-heart.filled");
        filledHearts.Count.Should().Be(3); // 3 filled
        
        var emptyHearts = cut.FindAll(".lives-heart.empty");
        emptyHearts.Count.Should().Be(2); // 2 empty
    }

    [Fact]
    public void LivesIndicator_ZeroLives_AllHeartsEmpty()
    {
        // Arrange
        var livesStatus = new LivesStatus(
            Current: 0,
            Max: 5,
            NextRegenAt: DateTime.UtcNow.AddMinutes(15),
            IsInfinite: false
        );

        // Act
        var cut = Render<LivesIndicator>(parameters => parameters
            .Add(p => p.Lives, livesStatus)
        );

        // Assert
        var filledHearts = cut.FindAll(".lives-heart.filled");
        filledHearts.Count.Should().Be(0);
        
        var emptyHearts = cut.FindAll(".lives-heart.empty");
        emptyHearts.Count.Should().Be(5);
    }

    [Fact]
    public void LivesIndicator_ShowsRegenTimer_WhenNotFull()
    {
        // Arrange
        var livesStatus = new LivesStatus(
            Current: 3,
            Max: 5,
            NextRegenAt: DateTime.UtcNow.AddMinutes(15),
            IsInfinite: false
        );

        // Act
        var cut = Render<LivesIndicator>(parameters => parameters
            .Add(p => p.Lives, livesStatus)
        );

        // Assert
        cut.Find(".lives-regen-timer").Should().NotBeNull();
    }

    [Fact]
    public void LivesIndicator_InfiniteLives_ShowsInfinitySymbol()
    {
        // Arrange
        var livesStatus = new LivesStatus(
            Current: 999,
            Max: 999,
            NextRegenAt: null,
            IsInfinite: true
        );

        // Act
        var cut = Render<LivesIndicator>(parameters => parameters
            .Add(p => p.Lives, livesStatus)
        );

        // Assert
        cut.Find(".lives-infinite").Should().NotBeNull();
        cut.FindAll(".lives-heart").Should().BeEmpty();
    }

    [Fact]
    public void LivesIndicator_FullLives_NoRegenTimer()
    {
        // Arrange
        var livesStatus = new LivesStatus(
            Current: 5,
            Max: 5,
            NextRegenAt: null,
            IsInfinite: false
        );

        // Act
        var cut = Render<LivesIndicator>(parameters => parameters
            .Add(p => p.Lives, livesStatus)
        );

        // Assert
        cut.FindAll(".lives-regen-timer").Should().BeEmpty();
    }

    [Fact]
    public void LivesIndicator_DisplaysLabel()
    {
        // Arrange
        var livesStatus = new LivesStatus(
            Current: 3,
            Max: 5,
            NextRegenAt: null,
            IsInfinite: false
        );

        // Act
        var cut = Render<LivesIndicator>(parameters => parameters
            .Add(p => p.Lives, livesStatus)
        );

        // Assert
        cut.Find(".lives-label").TextContent.Should().Contain("Životy");
    }
}
