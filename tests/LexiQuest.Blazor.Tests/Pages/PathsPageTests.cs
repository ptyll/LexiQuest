using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Pages;
using LexiQuest.Blazor.Services;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace LexiQuest.Blazor.Tests.Pages;

public class PathsPageTests : BunitContext
{
    private readonly IPathService _pathService;
    private readonly IStringLocalizer<Paths> _localizer;

    public PathsPageTests()
    {
        _pathService = Substitute.For<IPathService>();
        _localizer = Substitute.For<IStringLocalizer<Paths>>();

        SetupLocalizer();

        Services.AddSingleton(_pathService);
        Services.AddSingleton(_localizer);
    }

    private void SetupLocalizer()
    {
        _localizer["Title"].Returns(new LocalizedString("Title", "Výběr cesty"));
        _localizer["Subtitle"].Returns(new LocalizedString("Subtitle", "Vyberte si cestu učení"));
        _localizer["Path.Beginner.Name"].Returns(new LocalizedString("Path.Beginner.Name", "Začátečník"));
        _localizer["Path.Intermediate.Name"].Returns(new LocalizedString("Path.Intermediate.Name", "Pokročilý"));
        _localizer["Path.Advanced.Name"].Returns(new LocalizedString("Path.Advanced.Name", "Expert"));
        _localizer["Path.Expert.Name"].Returns(new LocalizedString("Path.Expert.Expert", "Mistr"));
        _localizer["Status.Locked"].Returns(new LocalizedString("Status.Locked", "Zamčeno"));
        _localizer["Status.Unlocked"].Returns(new LocalizedString("Status.Unlocked", "Odemčeno"));
        _localizer["Button.Start"].Returns(new LocalizedString("Button.Start", "Začít"));
        _localizer["Button.Continue"].Returns(new LocalizedString("Button.Continue", "Pokračovat"));
        _localizer["Progress.Label"].Returns(new LocalizedString("Progress.Label", "{0}% dokončeno"));
    }

    [Fact]
    public void PathsPage_Renders_4PathCards()
    {
        // Arrange
        var paths = CreateDefaultPaths();
        _pathService.GetPathsAsync().Returns(paths);

        // Act
        var cut = Render<Paths>();

        // Assert
        cut.FindAll(".path-card").Count.Should().Be(4);
    }

    [Fact]
    public void PathsPage_Renders_PathNames()
    {
        // Arrange
        var paths = CreateDefaultPaths();
        _pathService.GetPathsAsync().Returns(paths);

        // Act
        var cut = Render<Paths>();

        // Assert - should display path names from DTO
        cut.Markup.Should().Contain("Beginner");
        cut.Markup.Should().Contain("path-name");
    }

    [Fact]
    public void PathsPage_LockedPath_ShowsLockIcon()
    {
        // Arrange
        var paths = new List<LearningPathDto>
        {
            new(Guid.NewGuid(), "Expert", "Description", DifficultyLevel.Expert, 40, 0, false, 0)
        };
        _pathService.GetPathsAsync().Returns(paths);

        // Act
        var cut = Render<Paths>();

        // Assert
        cut.Markup.Should().Contain("Zamčeno");
    }

    [Fact]
    public void PathsPage_UnlockedPath_ShowsProgress()
    {
        // Arrange
        var paths = new List<LearningPathDto>
        {
            new(Guid.NewGuid(), "Beginner", "Description", DifficultyLevel.Beginner, 20, 10, true, 50)
        };
        _pathService.GetPathsAsync().Returns(paths);

        // Act
        var cut = Render<Paths>();

        // Assert
        cut.Markup.Should().Contain("50%");
    }

    [Fact]
    public void PathsPage_Renders_Title()
    {
        // Arrange
        var paths = CreateDefaultPaths();
        _pathService.GetPathsAsync().Returns(paths);

        // Act
        var cut = Render<Paths>();

        // Assert
        cut.Find(".paths-title").Should().NotBeNull();
    }

    private List<LearningPathDto> CreateDefaultPaths()
    {
        return new List<LearningPathDto>
        {
            new(Guid.NewGuid(), "Beginner", "Description1", DifficultyLevel.Beginner, 20, 0, true, 0),
            new(Guid.NewGuid(), "Intermediate", "Description2", DifficultyLevel.Intermediate, 25, 0, false, 0),
            new(Guid.NewGuid(), "Advanced", "Description3", DifficultyLevel.Advanced, 30, 0, false, 0),
            new(Guid.NewGuid(), "Expert", "Description4", DifficultyLevel.Expert, 40, 0, false, 0)
        };
    }
}
