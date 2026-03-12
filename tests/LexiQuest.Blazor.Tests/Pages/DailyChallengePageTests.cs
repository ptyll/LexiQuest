using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Pages;
using LexiQuest.Blazor.Services;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Tempo.Blazor.Localization;
using Xunit;

namespace LexiQuest.Blazor.Tests.Pages;

public class DailyChallengePageTests : BunitContext
{
    private readonly IDailyChallengeService _dailyChallengeService;
    private readonly IStringLocalizer<DailyChallenge> _localizer;
    private readonly ITmLocalizer _tmLocalizer;

    public DailyChallengePageTests()
    {
        _dailyChallengeService = Substitute.For<IDailyChallengeService>();
        _localizer = Substitute.For<IStringLocalizer<DailyChallenge>>();
        _tmLocalizer = Substitute.For<ITmLocalizer>();
        
        _localizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
        _tmLocalizer[Arg.Any<string>()].Returns(ci => ci.Arg<string>());
        
        Services.AddSingleton(_dailyChallengeService);
        Services.AddSingleton(_localizer);
        Services.AddSingleton(_tmLocalizer);
    }

    [Fact]
    public void DailyChallengePage_Renders_TodaysChallenge()
    {
        // Arrange
        var challenge = new DailyChallengeDto(
            Date: DateTime.UtcNow.Date,
            WordId: Guid.NewGuid(),
            Modifier: DailyModifier.Speed,
            ModifierDescription: "Bonus za rychlost",
            XPMultiplier: 150
        );
        
        _dailyChallengeService.GetTodayAsync().Returns(Task.FromResult(challenge));
        _dailyChallengeService.GetLeaderboardAsync().Returns(Task.FromResult(new List<DailyLeaderboardEntryDto>()));

        // Act
        var cut = Render<DailyChallenge>();

        // Assert
        cut.WaitForState(() => cut.Find(".challenge-header") != null);
        cut.Find(".challenge-header").Should().NotBeNull();
    }

    [Fact]
    public void DailyChallengePage_ShowsModifier_Badge()
    {
        // Arrange
        var challenge = new DailyChallengeDto(
            Date: DateTime.UtcNow.Date,
            WordId: Guid.NewGuid(),
            Modifier: DailyModifier.Hard,
            ModifierDescription: "Obtížná slova",
            XPMultiplier: 200
        );
        
        _dailyChallengeService.GetTodayAsync().Returns(Task.FromResult(challenge));
        _dailyChallengeService.GetLeaderboardAsync().Returns(Task.FromResult(new List<DailyLeaderboardEntryDto>()));

        // Act
        var cut = Render<DailyChallenge>();

        // Assert
        cut.WaitForState(() => cut.Find(".modifier-badge") != null);
        cut.Find(".modifier-badge").TextContent.Should().Contain("Hard");
    }

    [Fact]
    public void DailyChallengePage_Completed_ShowsResults()
    {
        // Arrange
        var challenge = new DailyChallengeDto(
            Date: DateTime.UtcNow.Date,
            WordId: Guid.NewGuid(),
            Modifier: DailyModifier.Easy,
            ModifierDescription: "Jednoduchá slova",
            XPMultiplier: 100
        );
        
        _dailyChallengeService.GetTodayAsync().Returns(Task.FromResult(challenge));
        _dailyChallengeService.GetLeaderboardAsync().Returns(Task.FromResult(new List<DailyLeaderboardEntryDto>()));
        _dailyChallengeService.HasCompletedTodayAsync().Returns(Task.FromResult(true));

        // Act
        var cut = Render<DailyChallenge>();

        // Assert
        cut.WaitForState(() => cut.Find(".completed-state") != null);
        cut.Find(".completed-state").Should().NotBeNull();
    }

    [Fact]
    public void DailyChallengePage_Renders_Leaderboard()
    {
        // Arrange
        var challenge = new DailyChallengeDto(
            Date: DateTime.UtcNow.Date,
            WordId: Guid.NewGuid(),
            Modifier: DailyModifier.Speed,
            ModifierDescription: "Bonus za rychlost",
            XPMultiplier: 150
        );
        
        var leaderboard = new List<DailyLeaderboardEntryDto>
        {
            new(Guid.NewGuid(), "User1", null, TimeSpan.FromSeconds(5), 150, 1, false),
            new(Guid.NewGuid(), "User2", null, TimeSpan.FromSeconds(8), 150, 2, false),
            new(Guid.NewGuid(), "CurrentUser", null, TimeSpan.FromSeconds(12), 150, 3, true)
        };
        
        _dailyChallengeService.GetTodayAsync().Returns(Task.FromResult(challenge));
        _dailyChallengeService.GetLeaderboardAsync().Returns(Task.FromResult(leaderboard));
        _dailyChallengeService.HasCompletedTodayAsync().Returns(Task.FromResult(false));

        // Act
        var cut = Render<DailyChallenge>();

        // Assert
        cut.WaitForState(() => cut.Find(".leaderboard") != null);
        var rows = cut.FindAll(".leaderboard-row");
        rows.Count.Should().Be(3);
    }

    [Fact]
    public void DailyChallengePage_NotAvailable_ShowsEmptyState()
    {
        // Arrange
        _dailyChallengeService.GetTodayAsync().Returns(Task.FromResult<DailyChallengeDto?>(null));

        // Act
        var cut = Render<DailyChallenge>();

        // Assert
        cut.WaitForState(() => cut.Find(".empty-state") != null);
        cut.Find(".empty-state").Should().NotBeNull();
    }
}
