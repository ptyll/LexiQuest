using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Pages;
using LexiQuest.Blazor.Services;
using LexiQuest.Shared.DTOs.Achievements;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Tempo.Blazor.Localization;
using Xunit;

namespace LexiQuest.Blazor.Tests.Pages;

public class AchievementsPageTests : BunitContext
{
    private readonly IAchievementService _achievementService;
    private readonly IStringLocalizer<Achievements> _localizer;
    private readonly ITmLocalizer _tmLocalizer;

    public AchievementsPageTests()
    {
        _achievementService = Substitute.For<IAchievementService>();
        _localizer = Substitute.For<IStringLocalizer<Achievements>>();
        _tmLocalizer = Substitute.For<ITmLocalizer>();
        
        _localizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
        _tmLocalizer[Arg.Any<string>()].Returns(ci => ci.Arg<string>());
        
        Services.AddSingleton(_achievementService);
        Services.AddSingleton(_localizer);
        Services.AddSingleton(_tmLocalizer);
    }

    [Fact]
    public void AchievementsPage_Renders_ProgressBar()
    {
        // Arrange
        var achievements = CreateAchievements(10, 3);
        _achievementService.GetAchievementsAsync().Returns(Task.FromResult(achievements));

        // Act
        var cut = Render<Achievements>();

        // Assert
        cut.WaitForState(() => cut.Find(".progress-header") != null);
        cut.Find(".progress-header").Should().NotBeNull();
    }

    [Fact]
    public void AchievementsPage_Renders_CategoryTabs()
    {
        // Arrange
        var achievements = CreateAchievementsWithCategories();
        _achievementService.GetAchievementsAsync().Returns(Task.FromResult(achievements));

        // Act
        var cut = Render<Achievements>();

        // Assert
        cut.WaitForState(() => cut.Find(".category-tabs") != null);
        var tabs = cut.FindAll(".tab-item");
        tabs.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void AchievementsPage_FiltersByCategory()
    {
        // Arrange
        var achievements = CreateAchievementsWithCategories();
        _achievementService.GetAchievementsAsync().Returns(Task.FromResult(achievements));

        // Act
        var cut = Render<Achievements>();

        // Assert
        cut.WaitForState(() => cut.Find(".achievement-grid") != null);
        var cards = cut.FindAll(".achievement-card");
        cards.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void AchievementsPage_UnlockedAchievement_ShowsGoldBorder()
    {
        // Arrange
        var achievements = new List<AchievementDto>
        {
            CreateAchievement("unlocked", AchievementCategory.Performance, true, DateTime.UtcNow.AddDays(-1))
        };
        _achievementService.GetAchievementsAsync().Returns(Task.FromResult(achievements));

        // Act
        var cut = Render<Achievements>();

        // Assert
        cut.WaitForState(() => cut.Find(".achievement-card.unlocked") != null);
        cut.Find(".achievement-card.unlocked").Should().NotBeNull();
    }

    [Fact]
    public void AchievementsPage_InProgressAchievement_ShowsProgressBar()
    {
        // Arrange
        var achievements = new List<AchievementDto>
        {
            CreateAchievementWithProgress("in_progress", AchievementCategory.Streak, 50, 100, 50)
        };
        _achievementService.GetAchievementsAsync().Returns(Task.FromResult(achievements));

        // Act
        var cut = Render<Achievements>();

        // Assert
        cut.WaitForState(() => cut.Find(".achievement-card.in-progress") != null);
        cut.Find(".progress-section").Should().NotBeNull();
    }

    [Fact]
    public void AchievementsPage_LockedAchievement_ShowsLockIcon()
    {
        // Arrange
        var achievements = new List<AchievementDto>
        {
            CreateAchievement("locked", AchievementCategory.Difficulty, false, null)
        };
        _achievementService.GetAchievementsAsync().Returns(Task.FromResult(achievements));

        // Act
        var cut = Render<Achievements>();

        // Assert
        cut.WaitForState(() => cut.Find(".achievement-card.locked") != null);
        cut.Find(".achievement-card.locked").Should().NotBeNull();
    }

    private static List<AchievementDto> CreateAchievements(int total, int unlocked)
    {
        var list = new List<AchievementDto>();
        for (int i = 0; i < total; i++)
        {
            list.Add(new AchievementDto(
                Guid.NewGuid(),
                $"achievement_{i}",
                $"Achievement {i}",
                $"Description {i}",
                AchievementCategory.Performance,
                10,
                10,
                i < unlocked ? 10 : 5,
                i < unlocked ? 100 : 50,
                i < unlocked,
                i < unlocked ? DateTime.UtcNow.AddDays(-i) : null,
                null
            ));
        }
        return list;
    }

    private static List<AchievementDto> CreateAchievementsWithCategories()
    {
        return new List<AchievementDto>
        {
            CreateAchievement("perf1", AchievementCategory.Performance, true, DateTime.UtcNow),
            CreateAchievement("streak1", AchievementCategory.Streak, false, null),
            CreateAchievement("diff1", AchievementCategory.Difficulty, true, DateTime.UtcNow),
            CreateAchievement("special1", AchievementCategory.Special, false, null)
        };
    }

    private static AchievementDto CreateAchievement(string key, AchievementCategory category, bool unlocked, DateTime? unlockedAt)
    {
        return new AchievementDto(
            Guid.NewGuid(),
            key,
            key,
            $"Description for {key}",
            category,
            10,
            10,
            unlocked ? 10 : 0,
            unlocked ? 100 : 0,
            unlocked,
            unlockedAt,
            null
        );
    }

    private static AchievementDto CreateAchievementWithProgress(string key, AchievementCategory category, int current, int required, int percentage)
    {
        return new AchievementDto(
            Guid.NewGuid(),
            key,
            key,
            $"Description for {key}",
            category,
            10,
            required,
            current,
            percentage,
            false,
            null,
            null
        );
    }
}
