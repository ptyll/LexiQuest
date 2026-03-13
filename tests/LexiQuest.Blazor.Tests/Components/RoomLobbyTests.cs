using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components.Multiplayer;
using LexiQuest.Blazor.Pages;
using LexiQuest.Shared.DTOs.Multiplayer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using LexiQuest.Blazor.Tests.Helpers;
using Xunit;

namespace LexiQuest.Blazor.Tests.Components;

public class RoomLobbyTests : BunitContext
{
    private readonly IStringLocalizer<Multiplayer> _localizer;

    public RoomLobbyTests()
    {
        _localizer = Substitute.For<IStringLocalizer<Multiplayer>>();
        SetupLocalizer();
        
        Services.AddSingleton(_localizer);
        TempoTestHelper.RegisterTempoServices(Services);
    }

    private void SetupLocalizer()
    {
        _localizer["Room_Lobby_Title"].Returns(new LocalizedString("Room_Lobby_Title", "Lobby"));
        _localizer["Room_Lobby_WaitingForOpponent"].Returns(new LocalizedString("Room_Lobby_WaitingForOpponent", "Čekání na soupeře..."));
        _localizer["Room_Lobby_OpponentJoined"].Returns(new LocalizedString("Room_Lobby_OpponentJoined", "Soupeř se připojil!"));
        _localizer["Room_Lobby_Ready"].Returns(new LocalizedString("Room_Lobby_Ready", "Připraven ✓"));
        _localizer["Room_Lobby_NotReady"].Returns(new LocalizedString("Room_Lobby_NotReady", "Čeká..."));
        _localizer["Room_Lobby_BothReady"].Returns(new LocalizedString("Room_Lobby_BothReady", "Obě hráči připraveni!"));
        _localizer["Room_Lobby_ReadyButton"].Returns(new LocalizedString("Room_Lobby_ReadyButton", "Jsem připraven!"));
        _localizer["Room_Lobby_CancelReadyButton"].Returns(new LocalizedString("Room_Lobby_CancelReadyButton", "Zrušit připravení"));
        _localizer["Room_Lobby_Chat_Placeholder"].Returns(new LocalizedString("Room_Lobby_Chat_Placeholder", "Napište zprávu..."));
        _localizer["Room_Lobby_Chat_Send"].Returns(new LocalizedString("Room_Lobby_Chat_Send", "Odeslat"));
        _localizer["Room_Code_Label"].Returns(new LocalizedString("Room_Code_Label", "Kód místnosti"));
        _localizer["Room_Code_CopySuccess"].Returns(new LocalizedString("Room_Code_CopySuccess", "Kód zkopírován!"));
        _localizer["Room_Code_ShareText"].Returns(new LocalizedString("Room_Code_ShareText", "Připoj se do mé místnosti: {0}"));
        _localizer["Room_Settings_Title"].Returns(new LocalizedString("Room_Settings_Title", "Nastavení hry"));
        _localizer["Room_Settings_WordCount"].Returns(new LocalizedString("Room_Settings_WordCount", "Počet slov"));
        _localizer["Room_Settings_TimeLimit"].Returns(new LocalizedString("Room_Settings_TimeLimit", "Časový limit"));
        _localizer["Room_Settings_Difficulty"].Returns(new LocalizedString("Room_Settings_Difficulty", "Obtížnost"));
        _localizer["Room_Settings_BestOf"].Returns(new LocalizedString("Room_Settings_BestOf", "Best of"));
        _localizer["Room_Expired"].Returns(new LocalizedString("Room_Expired", "Místnost vypršela"));
        _localizer["Room_Leave_Confirm"].Returns(new LocalizedString("Room_Leave_Confirm", "Opravdu chcete opustit místnost?"));
        _localizer["Button_Cancel"].Returns(new LocalizedString("Button_Cancel", "Zrušit"));
        _localizer["Room_Series_GameOf"].Returns(new LocalizedString("Room_Series_GameOf", "Hra {0} z {1}"));
    }

    [Fact]
    public void RoomLobby_HostView_ShowsCodeAndWaiting()
    {
        // Arrange
        var roomStatus = CreateHostWaitingRoom();
        var cut = Render<RoomLobby>(parameters => parameters
            .Add(p => p.RoomStatus, roomStatus)
            .Add(p => p.IsHost, true));

        // Assert
        cut.Find(".room-code").TextContent.Should().Contain("LEXIQ-ABCD");
        cut.Find(".waiting-message").TextContent.Should().Contain("Čekání");
    }

    [Fact]
    public void RoomLobby_OpponentJoined_ShowsBothPlayers()
    {
        // Arrange
        var roomStatus = CreateFullRoom();
        var cut = Render<RoomLobby>(parameters => parameters
            .Add(p => p.RoomStatus, roomStatus)
            .Add(p => p.IsHost, true));

        // Assert
        cut.FindAll(".player-card").Count.Should().Be(2);
    }

    [Fact]
    public void RoomLobby_BothReady_ShowsCountdown()
    {
        // Arrange
        var roomStatus = CreateReadyRoom();
        var cut = Render<RoomLobby>(parameters => parameters
            .Add(p => p.RoomStatus, roomStatus)
            .Add(p => p.IsHost, true));

        // Assert
        cut.Find(".countdown").Should().NotBeNull();
    }

    [Fact]
    public void RoomLobby_Chat_SendsAndReceivesMessages()
    {
        // Arrange
        string? sentMessage = null;
        var roomStatus = CreateFullRoom();
        var cut = Render<RoomLobby>(parameters => parameters
            .Add(p => p.RoomStatus, roomStatus)
            .Add(p => p.IsHost, true)
            .Add(p => p.OnSendMessage, msg => { sentMessage = msg; }));

        // Act
        var input = cut.Find(".chat-input");
        input.Input("Ahoj!");
        
        var sendButton = cut.Find(".chat-send-button");
        sendButton.Click();

        // Assert
        sentMessage.Should().Be("Ahoj!");
    }

    [Fact]
    public void RoomLobby_ReadyButton_TogglesReadyState()
    {
        // Arrange
        var readyClicked = false;
        var roomStatus = CreateFullRoom();
        var cut = Render<RoomLobby>(parameters => parameters
            .Add(p => p.RoomStatus, roomStatus)
            .Add(p => p.IsHost, true)
            .Add(p => p.OnSetReady, () => { readyClicked = true; }));

        // Act
        var readyButton = cut.Find(".ready-button");
        readyButton.Click();

        // Assert
        readyClicked.Should().BeTrue();
    }

    [Fact]
    public void RoomLobby_Expiry_ShowsExpiredMessage()
    {
        // Arrange
        var cut = Render<RoomLobby>(parameters => parameters
            .Add(p => p.IsExpired, true));

        // Assert
        cut.Find(".expired-message").TextContent.Should().Contain("vypršela");
    }

    [Fact]
    public void RoomLobby_ShowsSettingsSummary()
    {
        // Arrange
        var roomStatus = CreateHostWaitingRoom();
        var cut = Render<RoomLobby>(parameters => parameters
            .Add(p => p.RoomStatus, roomStatus)
            .Add(p => p.IsHost, true));

        // Assert
        cut.Find(".settings-summary").TextContent.Should().Contain("15");
        cut.Find(".settings-summary").TextContent.Should().Contain("3");
    }

    [Fact]
    public void RoomLobby_CopyCode_CallsCallback()
    {
        // Arrange
        var copied = false;
        var roomStatus = CreateHostWaitingRoom();
        var cut = Render<RoomLobby>(parameters => parameters
            .Add(p => p.RoomStatus, roomStatus)
            .Add(p => p.IsHost, true)
            .Add(p => p.OnCopyCode, () => { copied = true; }));

        // Act
        var copyButton = cut.Find(".copy-code-button");
        copyButton.Click();

        // Assert
        copied.Should().BeTrue();
    }

    private static RoomStatusDto CreateHostWaitingRoom()
    {
        return new RoomStatusDto(
            RoomCode: "LEXIQ-ABCD",
            Settings: new RoomSettingsDto(15, 3, LexiQuest.Shared.Enums.DifficultyLevel.Beginner, 1),
            Players: new List<LexiQuest.Shared.DTOs.Multiplayer.LobbyPlayerDto>
            {
                new("HostPlayer", null, 10, true, true)
            },
            BothReady: false,
            ExpiresAt: DateTime.UtcNow.AddMinutes(5),
            CurrentGameIndex: 0,
            BestOfTotal: 1
        );
    }

    private static RoomStatusDto CreateFullRoom()
    {
        return new RoomStatusDto(
            RoomCode: "LEXIQ-ABCD",
            Settings: new RoomSettingsDto(15, 3, LexiQuest.Shared.Enums.DifficultyLevel.Beginner, 1),
            Players: new List<LexiQuest.Shared.DTOs.Multiplayer.LobbyPlayerDto>
            {
                new("HostPlayer", null, 10, true, false),
                new("Opponent", null, 8, false, false)
            },
            BothReady: false,
            ExpiresAt: DateTime.UtcNow.AddMinutes(5),
            CurrentGameIndex: 0,
            BestOfTotal: 1
        );
    }

    private static RoomStatusDto CreateReadyRoom()
    {
        return new RoomStatusDto(
            RoomCode: "LEXIQ-ABCD",
            Settings: new RoomSettingsDto(15, 3, LexiQuest.Shared.Enums.DifficultyLevel.Beginner, 1),
            Players: new List<LexiQuest.Shared.DTOs.Multiplayer.LobbyPlayerDto>
            {
                new("HostPlayer", null, 10, true, true),
                new("Opponent", null, 8, false, true)
            },
            BothReady: true,
            ExpiresAt: DateTime.UtcNow.AddMinutes(5),
            CurrentGameIndex: 0,
            BestOfTotal: 1
        );
    }
}
