using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components.Multiplayer;
using LexiQuest.Blazor.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using LexiQuest.Blazor.Tests.Helpers;
using Xunit;

namespace LexiQuest.Blazor.Tests.Components;

public class JoinRoomModalTests : BunitContext
{
    private readonly IStringLocalizer<Multiplayer> _localizer;

    public JoinRoomModalTests()
    {
        _localizer = Substitute.For<IStringLocalizer<Multiplayer>>();
        SetupLocalizer();
        
        Services.AddSingleton(_localizer);
        TempoTestHelper.RegisterTempoServices(Services);
    }

    private void SetupLocalizer()
    {
        _localizer["Room_Join_Title"].Returns(new LocalizedString("Room_Join_Title", "Připojit se do místnosti"));
        _localizer["Room_Code_Label"].Returns(new LocalizedString("Room_Code_Label", "Kód místnosti"));
        _localizer["Room_Code_Placeholder"].Returns(new LocalizedString("Room_Code_Placeholder", "LEXIQ-XXXX"));
        _localizer["Button_Join"].Returns(new LocalizedString("Button_Join", "Připojit se"));
        _localizer["Button_Cancel"].Returns(new LocalizedString("Button_Cancel", "Zrušit"));
        _localizer["Room_NotFound"].Returns(new LocalizedString("Room_NotFound", "Místnost nenalezena"));
        _localizer["Room_Expired"].Returns(new LocalizedString("Room_Expired", "Kód vypršel"));
        _localizer["Room_Full"].Returns(new LocalizedString("Room_Full", "Místnost je plná"));
        _localizer["Validation_RoomCode_Required"].Returns(new LocalizedString("Validation_RoomCode_Required", "Kód místnosti je povinný"));
        _localizer["Validation_RoomCode_InvalidFormat"].Returns(new LocalizedString("Validation_RoomCode_InvalidFormat", "Kód musí být ve formátu LEXIQ-XXXX"));
    }

    [Fact]
    public void JoinRoomModal_Renders_CodeInput()
    {
        // Arrange & Act
        var cut = Render<JoinRoomModal>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Assert
        cut.Find(".room-code-input").Should().NotBeNull();
        cut.Find(".room-code-input").GetAttribute("placeholder").Should().Be("LEXIQ-XXXX");
    }

    [Fact]
    public void JoinRoomModal_EmptyCode_ShowsError()
    {
        // Arrange
        var cut = Render<JoinRoomModal>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Act
        var joinButton = cut.Find(".join-button");
        joinButton.Click();

        // Assert
        cut.Find(".validation-message").TextContent.Should().Contain("povinný");
    }

    [Fact]
    public void JoinRoomModal_InvalidCode_ShowsError()
    {
        // Arrange
        var cut = Render<JoinRoomModal>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Act
        var input = cut.Find(".room-code-input");
        input.Input("INVALID");
        
        var joinButton = cut.Find(".join-button");
        joinButton.Click();

        // Assert
        cut.Find(".validation-message").TextContent.Should().Contain("LEXIQ-XXXX");
    }

    [Fact]
    public void JoinRoomModal_ValidCode_JoinsRoom()
    {
        // Arrange
        string? joinedCode = null;
        var cut = Render<JoinRoomModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.OnJoin, code => { joinedCode = code; }));

        // Act
        var input = cut.Find(".room-code-input");
        input.Input("LEXIQ-ABCD");
        
        var joinButton = cut.Find(".join-button");
        joinButton.Click();

        // Assert
        joinedCode.Should().Be("LEXIQ-ABCD");
    }

    [Fact]
    public void JoinRoomModal_Code_AutoUppercase()
    {
        // Arrange
        var cut = Render<JoinRoomModal>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Act
        var input = cut.Find(".room-code-input");
        input.Input("lexiq-abcd");

        // Assert
        cut.Instance.RoomCode.Should().Be("LEXIQ-ABCD");
    }

    [Fact]
    public void JoinRoomModal_Code_MaxLength()
    {
        // Arrange
        var cut = Render<JoinRoomModal>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Act
        var input = cut.Find(".room-code-input");
        input.Input("LEXIQ-ABCD-EXTRA");

        // Assert
        cut.Instance.RoomCode.Should().Be("LEXIQ-ABCD");
    }

    [Fact]
    public void JoinRoomModal_ShowsError_NotFound()
    {
        // Arrange
        var cut = Render<JoinRoomModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.ErrorMessage, "Místnost nenalezena"));

        // Assert
        cut.Find(".error-alert").TextContent.Should().Contain("nenalezena");
    }

    [Fact]
    public void JoinRoomModal_ShowsError_Expired()
    {
        // Arrange
        var cut = Render<JoinRoomModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.ErrorMessage, "Kód vypršel"));

        // Assert
        cut.Find(".error-alert").TextContent.Should().Contain("vypršel");
    }

    [Fact]
    public void JoinRoomModal_ShowsError_Full()
    {
        // Arrange
        var cut = Render<JoinRoomModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.ErrorMessage, "Místnost je plná"));

        // Assert
        cut.Find(".error-alert").TextContent.Should().Contain("plná");
    }

    [Fact]
    public void JoinRoomModal_IsLoading_ShowsSpinner()
    {
        // Arrange
        var cut = Render<JoinRoomModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.IsLoading, true));

        // Assert
        cut.Find(".join-button .spinner").Should().NotBeNull();
    }

    [Fact]
    public void JoinRoomModal_CancelButton_ClosesModal()
    {
        // Arrange
        var closed = false;
        var cut = Render<JoinRoomModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.OnClose, () => { closed = true; }));

        // Act
        var cancelButton = cut.Find(".cancel-button");
        cancelButton.Click();

        // Assert
        closed.Should().BeTrue();
    }

    [Fact]
    public void JoinRoomModal_NotVisible_DoesNotRender()
    {
        // Arrange & Act
        var cut = Render<JoinRoomModal>(parameters => parameters
            .Add(p => p.IsVisible, false));

        // Assert
        cut.FindAll(".join-room-modal").Count.Should().Be(0);
    }

    [Fact]
    public void JoinRoomModal_Input_AutoFocus()
    {
        // Arrange & Act
        var cut = Render<JoinRoomModal>(parameters => parameters
            .Add(p => p.IsVisible, true));

        // Assert
        cut.Find(".room-code-input").HasAttribute("autofocus").Should().BeTrue();
    }
}
