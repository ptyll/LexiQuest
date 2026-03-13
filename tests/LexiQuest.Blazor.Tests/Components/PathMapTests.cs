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
        cut.Markup.Should().Contain("👑");
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
