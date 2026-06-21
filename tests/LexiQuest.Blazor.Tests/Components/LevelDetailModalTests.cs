using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components.Paths;
using LexiQuest.Shared.DTOs.Game;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace LexiQuest.Blazor.Tests.Components;

public class LevelDetailModalTests : BunitContext
{
    private readonly IStringLocalizer<LevelDetailModal> _localizer;

    public LevelDetailModalTests()
    {
        _localizer = Substitute.For<IStringLocalizer<LevelDetailModal>>();
        SetupLocalizer();
        Services.AddSingleton(_localizer);
    }

    [Fact]
    public void LevelDetailModal_Renders_LevelRulesAndRewards()
    {
        // Arrange
        var level = new PathLevelDto(
            Id: Guid.NewGuid(),
            LevelNumber: 1,
            Status: "Current",
            IsBoss: false,
            IsPerfect: false,
            WordCount: 10,
            WordLengthMin: 3,
            WordLengthMax: 5,
            TimePerWordSeconds: 30,
            HintCount: 3,
            Lives: 5,
            XpReward: 100);

        // Act
        var cut = Render<LevelDetailModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.PathId, Guid.NewGuid())
            .Add(p => p.Level, level));

        // Assert
        cut.Markup.Should().Contain("10 slov");
        cut.Markup.Should().Contain("30 s");
        cut.Markup.Should().Contain("3 nápovědy");
        cut.Markup.Should().Contain("5 životů");
        cut.Markup.Should().Contain("100 XP");
    }

    private void SetupLocalizer()
    {
        _localizer[Arg.Any<string>()].Returns(call =>
        {
            var key = call.Arg<string>();
            var value = key switch
            {
                "Title" => "Detail úrovně",
                "Label.Status" => "Stav",
                "Preview.Text" => "Slova v této úrovni mají {0}-{1} písmen.",
                "Section.Info" => "Informace o úrovni",
                "Section.Rewards" => "Odměny",
                "Label.WordCount" => "Počet slov",
                "Label.TimePerWord" => "Čas na slovo",
                "Label.Hints" => "Nápovědy",
                "Label.Lives" => "Životy",
                "Value.Words" => "{0} slov",
                "Value.Seconds" => "{0} s",
                "Value.Hints" => "{0} nápovědy",
                "Value.Lives" => "{0} životů",
                "Value.XpReward" => "{0} XP",
                "Button.Close" => "Zavřít",
                "Button.Start" => "Spustit úroveň",
                "Status.Current" => "Aktuální",
                _ => key
            };

            return new LocalizedString(key, value);
        });
    }
}
