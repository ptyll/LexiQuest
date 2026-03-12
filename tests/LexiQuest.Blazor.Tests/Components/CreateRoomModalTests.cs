using Bunit;
using FluentAssertions;
using FluentValidation;
using LexiQuest.Blazor.Components.Multiplayer;
using LexiQuest.Blazor.Pages;
using LexiQuest.Shared.DTOs.Multiplayer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Xunit;

namespace LexiQuest.Blazor.Tests.Components;

public class CreateRoomModalTests : BunitContext
{
    private readonly IStringLocalizer<Multiplayer> _localizer;

    public CreateRoomModalTests()
    {
        _localizer = Substitute.For<IStringLocalizer<Multiplayer>>();
        SetupLocalizer();
        
        Services.AddSingleton(_localizer);
        Services.AddSingleton<IValidator<RoomSettingsDto>>(new RoomSettingsValidator(_localizer));
    }

    private void SetupLocalizer()
    {
        _localizer["Room_Create_Title"].Returns(new LocalizedString("Room_Create_Title", "Vytvořit soukromou místnost"));
        _localizer["Room_Settings_Title"].Returns(new LocalizedString("Room_Settings_Title", "Nastavení hry"));
        _localizer["Room_Settings_WordCount"].Returns(new LocalizedString("Room_Settings_WordCount", "Počet slov"));
        _localizer["Room_Settings_TimeLimit"].Returns(new LocalizedString("Room_Settings_TimeLimit", "Časový limit"));
        _localizer["Room_Settings_Difficulty"].Returns(new LocalizedString("Room_Settings_Difficulty", "Obtížnost"));
        _localizer["Room_Settings_BestOf"].Returns(new LocalizedString("Room_Settings_BestOf", "Best of"));
        _localizer["Room_NoLeagueXP_Info"].Returns(new LocalizedString("Room_NoLeagueXP_Info", "Soukromé místnosti nedávají liga XP (prevence farmení)"));
        _localizer["Button_Create"].Returns(new LocalizedString("Button_Create", "Vytvořit"));
        _localizer["Button_Cancel"].Returns(new LocalizedString("Button_Cancel", "Zrušit"));
        _localizer["Difficulty_Beginner"].Returns(new LocalizedString("Difficulty_Beginner", "Beginner 🌱"));
        _localizer["Difficulty_Intermediate"].Returns(new LocalizedString("Difficulty_Intermediate", "Intermediate 🌿"));
        _localizer["Difficulty_Advanced"].Returns(new LocalizedString("Difficulty_Advanced", "Advanced 🌳"));
        _localizer["Difficulty_Expert"].Returns(new LocalizedString("Difficulty_Expert", "Expert 🔥"));
        _localizer["Difficulty_Mix"].Returns(new LocalizedString("Difficulty_Mix", "Mix (všechny)"));
        _localizer["Validation_WordCount_Invalid"].Returns(new LocalizedString("Validation_WordCount_Invalid", "Počet slov musí být 10, 15 nebo 20"));
        _localizer["Validation_TimeLimit_Invalid"].Returns(new LocalizedString("Validation_TimeLimit_Invalid", "Časový limit musí být 2, 3 nebo 5 minut"));
        _localizer["Validation_BestOf_Invalid"].Returns(new LocalizedString("Validation_BestOf_Invalid", "Best of musí být 1, 3 nebo 5"));
    }

    [Fact]
    public void CreateRoomModal_Renders_AllSettings()
    {
        // Arrange & Act
        var cut = Render<CreateRoomModal>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Assert
        cut.Find(".word-count-section").Should().NotBeNull();
        cut.Find(".time-limit-section").Should().NotBeNull();
        cut.Find(".difficulty-section").Should().NotBeNull();
        cut.Find(".best-of-section").Should().NotBeNull();
    }

    [Fact]
    public void CreateRoomModal_ValidSettings_CreatesRoom()
    {
        // Arrange
        RoomSettingsDto? createdSettings = null;
        var cut = Render<CreateRoomModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.OnCreate, settings => { createdSettings = settings; }));

        // Act
        var submitButton = cut.Find(".create-button");
        submitButton.Click();

        // Assert
        createdSettings.Should().NotBeNull();
        createdSettings!.WordCount.Should().Be(15);
        createdSettings.TimeLimitMinutes.Should().Be(3);
    }

    [Fact]
    public void CreateRoomModal_CancelButton_ClosesModal()
    {
        // Arrange
        var closed = false;
        var cut = Render<CreateRoomModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.OnClose, () => { closed = true; }));

        // Act
        var cancelButton = cut.Find(".cancel-button");
        cancelButton.Click();

        // Assert
        closed.Should().BeTrue();
    }

    [Fact]
    public void CreateRoomModal_ShowsNoLeagueXPInfo()
    {
        // Arrange & Act
        var cut = Render<CreateRoomModal>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Assert
        cut.Find(".no-league-info").TextContent.Should().Contain("Soukromé místnosti");
    }

    [Fact]
    public void CreateRoomModal_NotVisible_DoesNotRender()
    {
        // Arrange & Act
        var cut = Render<CreateRoomModal>(parameters => parameters
            .Add(p => p.IsVisible, false));

        // Assert
        cut.FindAll(".create-room-modal").Count.Should().Be(0);
    }
}

public class RoomSettingsValidator : AbstractValidator<RoomSettingsDto>
{
    public RoomSettingsValidator(IStringLocalizer<Multiplayer> localizer)
    {
        RuleFor(x => x.WordCount)
            .Must(x => x is 10 or 15 or 20)
            .WithMessage(localizer["Validation_WordCount_Invalid"]);

        RuleFor(x => x.TimeLimitMinutes)
            .Must(x => x is 2 or 3 or 5)
            .WithMessage(localizer["Validation_TimeLimit_Invalid"]);

        RuleFor(x => x.BestOf)
            .Must(x => x is 1 or 3 or 5)
            .WithMessage(localizer["Validation_BestOf_Invalid"]);
    }
}
