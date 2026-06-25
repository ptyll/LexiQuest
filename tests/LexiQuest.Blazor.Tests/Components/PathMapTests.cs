using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components.Paths;
using LexiQuest.Shared.DTOs.Game;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using LexiQuest.Blazor.Tests.Helpers;
using NSubstitute;

namespace LexiQuest.Blazor.Tests.Components;

public class PathMapTests : BunitContext
{
    private readonly IStringLocalizer<PathMap> _localizer;

    public PathMapTests()
    {
        _localizer = Substitute.For<IStringLocalizer<PathMap>>();
        _localizer["Level"].Returns(new LocalizedString("Level", "Level"));
        Services.AddSingleton(_localizer);
        TempoTestHelper.RegisterTempoServices(Services);
    }

    [Fact]
    public void PathMap_Renders_AllLevelNodes()
    {
        // Arrange
        var levels = CreateTestLevels(5, 2);

        // Act
        var cut = Render<PathMap>(parameters => parameters
            .Add(p => p.Levels, levels)
            .Add(p => p.CurrentLevel, 3));

        // Assert
        cut.FindAll(".level-node").Count.Should().Be(5);
    }

    [Fact]
    public void PathMap_CurrentLevel_HasPulsingEffect()
    {
        // Arrange
        var levels = CreateTestLevels(5, 3);

        // Act
        var cut = Render<PathMap>(parameters => parameters
            .Add(p => p.Levels, levels)
            .Add(p => p.CurrentLevel, 3));

        // Assert
        cut.Find(".level-current").Should().NotBeNull();
    }

    [Fact]
    public void PathMap_BossLevel_ShowsCrownIcon()
    {
        // Arrange
        var levels = new List<PathLevelDto>
        {
            new(Guid.NewGuid(), 1, "Available", true, false),
            new(Guid.NewGuid(), 5, "Locked", true, false) // Boss level
        };

        // Act
        var cut = Render<PathMap>(parameters => parameters
            .Add(p => p.Levels, levels)
            .Add(p => p.CurrentLevel, 1));

        // Assert
        cut.Find("[data-level-icon='crown']").Should().NotBeNull();
    }

    [Fact]
    public void PathMap_RendersLevelsAsDuolingoStyleSnakeSteps()
    {
        // Arrange
        var levels = CreateTestLevels(6, 3);

        // Act
        var cut = Render<PathMap>(parameters => parameters
            .Add(p => p.Levels, levels)
            .Add(p => p.CurrentLevel, 3));

        // Assert
        var steps = cut.FindAll(".path-level-step");
        steps.Count.Should().Be(6);
        steps[0].ClassList.Should().Contain("path-step-center");
        steps[1].ClassList.Should().Contain("path-step-left");
        steps[2].ClassList.Should().Contain("path-step-center");
        steps[3].ClassList.Should().Contain("path-step-right");
        steps[4].ClassList.Should().Contain("path-step-center");
        steps[5].ClassList.Should().Contain("path-step-left");
    }

    [Fact]
    public void PathMap_RendersCurvedSegmentsBetweenSnakeSteps()
    {
        // Arrange
        var levels = CreateTestLevels(4, 2);

        // Act
        var cut = Render<PathMap>(parameters => parameters
            .Add(p => p.Levels, levels)
            .Add(p => p.CurrentLevel, 2));

        // Assert
        var segments = cut.FindAll(".path-segment");
        segments.Count.Should().Be(3);
        segments[0].ClassList.Should().Contain("segment-center-to-left");
        segments[1].ClassList.Should().Contain("segment-left-to-center");
        segments[2].ClassList.Should().Contain("segment-center-to-right");
    }

    [Fact]
    public void PathMap_CurrentLevel_ExposesAriaCurrentStep()
    {
        // Arrange
        var levels = CreateTestLevels(5, 3);

        // Act
        var cut = Render<PathMap>(parameters => parameters
            .Add(p => p.Levels, levels)
            .Add(p => p.CurrentLevel, 3));

        // Assert
        cut.Find(".level-current").GetAttribute("aria-current").Should().Be("step");
    }

    private List<PathLevelDto> CreateTestLevels(int count, int currentLevel)
    {
        var levels = new List<PathLevelDto>();
        for (int i = 1; i <= count; i++)
        {
            var status = i < currentLevel ? "Completed" : (i == currentLevel ? "Current" : "Locked");
            levels.Add(new PathLevelDto(Guid.NewGuid(), i, status, i % 5 == 0, false));
        }
        return levels;
    }
}
