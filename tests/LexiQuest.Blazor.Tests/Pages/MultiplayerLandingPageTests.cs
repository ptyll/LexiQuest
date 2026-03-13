using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Components;
using LexiQuest.Blazor.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using LexiQuest.Blazor.Tests.Helpers;
using Xunit;

namespace LexiQuest.Blazor.Tests.Pages;

public class MultiplayerLandingPageTests : BunitContext
{
    private readonly IStringLocalizer<LexiQuest.Blazor.Pages.Multiplayer> _localizer;
    private readonly NavigationManager _navigationManager;

    public MultiplayerLandingPageTests()
    {
        _localizer = Substitute.For<IStringLocalizer<LexiQuest.Blazor.Pages.Multiplayer>>();
        SetupLocalizer();
        
        _navigationManager = new TestNavigationManager();
        Services.AddSingleton(_localizer);
        Services.AddSingleton(_navigationManager);
        Services.AddSingleton(Substitute.For<LexiQuest.Blazor.Services.IMatchHubClient>());
        TempoTestHelper.RegisterTempoServices(Services);
    }

    private void SetupLocalizer()
    {
        _localizer["Page_Title"].Returns(new LocalizedString("Page_Title", "Multiplayer"));
        _localizer["QuickMatch_Title"].Returns(new LocalizedString("QuickMatch_Title", "⚔️ 1v1 Souboj"));
        _localizer["QuickMatch_Description"].Returns(new LocalizedString("QuickMatch_Description", "Náhodný soupeř, plné XP + liga body"));
        _localizer["QuickMatch_Button"].Returns(new LocalizedString("QuickMatch_Button", "Rychlý zápas"));
        _localizer["QuickMatch_LeagueXP"].Returns(new LocalizedString("QuickMatch_LeagueXP", "Liga XP ✓"));
        _localizer["PrivateRoom_Title"].Returns(new LocalizedString("PrivateRoom_Title", "🏠 Soukromá místnost"));
        _localizer["PrivateRoom_Description"].Returns(new LocalizedString("PrivateRoom_Description", "Pozvi kamaráda, vlastní pravidla"));
        _localizer["PrivateRoom_CreateButton"].Returns(new LocalizedString("PrivateRoom_CreateButton", "Vytvořit místnost"));
        _localizer["PrivateRoom_JoinButton"].Returns(new LocalizedString("PrivateRoom_JoinButton", "Připojit se"));
        _localizer["PrivateRoom_NoLeagueXP"].Returns(new LocalizedString("PrivateRoom_NoLeagueXP", "Bez liga XP"));
        _localizer["MatchHistory_Link"].Returns(new LocalizedString("MatchHistory_Link", "Historie zápasů"));
        _localizer["Room_Create_Title"].Returns(new LocalizedString("Room_Create_Title", "Vytvořit soukromou místnost"));
        _localizer["Room_Settings_Title"].Returns(new LocalizedString("Room_Settings_Title", "Nastavení hry"));
        _localizer["Room_Settings_WordCount"].Returns(new LocalizedString("Room_Settings_WordCount", "Počet slov"));
        _localizer["Room_Settings_TimeLimit"].Returns(new LocalizedString("Room_Settings_TimeLimit", "Časový limit"));
        _localizer["Room_Settings_Difficulty"].Returns(new LocalizedString("Room_Settings_Difficulty", "Obtížnost"));
        _localizer["Room_Settings_BestOf"].Returns(new LocalizedString("Room_Settings_BestOf", "Best of"));
        _localizer["Room_Code_Label"].Returns(new LocalizedString("Room_Code_Label", "Kód místnosti"));
        _localizer["Room_Code_CopySuccess"].Returns(new LocalizedString("Room_Code_CopySuccess", "Kód zkopírován!"));
        _localizer["Room_Code_ShareText"].Returns(new LocalizedString("Room_Code_ShareText", "Připoj se do mé místnosti: {0}"));
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
        _localizer["Room_Expired"].Returns(new LocalizedString("Room_Expired", "Místnost vypršela"));
        _localizer["Room_Full"].Returns(new LocalizedString("Room_Full", "Místnost je plná"));
        _localizer["Room_NotFound"].Returns(new LocalizedString("Room_NotFound", "Místnost nenalezena"));
        _localizer["Room_InvalidCode"].Returns(new LocalizedString("Room_InvalidCode", "Neplatný kód místnosti"));
        _localizer["Room_Series_Score"].Returns(new LocalizedString("Room_Series_Score", "Série: {0}:{1}"));
        _localizer["Room_Series_GameOf"].Returns(new LocalizedString("Room_Series_GameOf", "Hra {0} z {1}"));
        _localizer["Room_Rematch_Request"].Returns(new LocalizedString("Room_Rematch_Request", "Chci odvetu!"));
        _localizer["Room_Rematch_Accept"].Returns(new LocalizedString("Room_Rematch_Accept", "Přijmout"));
        _localizer["Room_Rematch_Decline"].Returns(new LocalizedString("Room_Rematch_Decline", "Odmítnout"));
        _localizer["Room_Leave_Confirm"].Returns(new LocalizedString("Room_Leave_Confirm", "Opravdu chcete opustit místnost?"));
        _localizer["Room_NoLeagueXP_Info"].Returns(new LocalizedString("Room_NoLeagueXP_Info", "Soukromé místnosti nedávají liga XP (prevence farmení)"));
        _localizer["Matchmaking_Searching"].Returns(new LocalizedString("Matchmaking_Searching", "Hledání soupeře..."));
        _localizer["Matchmaking_Cancel"].Returns(new LocalizedString("Matchmaking_Cancel", "Zrušit hledání"));
        _localizer["Matchmaking_MatchFound"].Returns(new LocalizedString("Matchmaking_MatchFound", "Soupeř nalezen!"));
        _localizer["Matchmaking_StartingIn"].Returns(new LocalizedString("Matchmaking_StartingIn", "Začínáme za..."));
        _localizer["Matchmaking_Timeout"].Returns(new LocalizedString("Matchmaking_Timeout", "Soupeř nenalezen"));
        _localizer["Matchmaking_Retry"].Returns(new LocalizedString("Matchmaking_Retry", "Zkusit znovu"));
        _localizer["Validation_RoomCode_Required"].Returns(new LocalizedString("Validation_RoomCode_Required", "Kód místnosti je povinný"));
        _localizer["Validation_RoomCode_InvalidFormat"].Returns(new LocalizedString("Validation_RoomCode_InvalidFormat", "Kód musí být ve formátu LEXIQ-XXXX"));
        _localizer["MatchHistory_Title"].Returns(new LocalizedString("MatchHistory_Title", "Historie zápasů"));
        _localizer["MatchHistory_Empty"].Returns(new LocalizedString("MatchHistory_Empty", "Zatím žádné zápasy"));
        _localizer["MatchHistory_Empty_Description"].Returns(new LocalizedString("MatchHistory_Empty_Description", "Zahraj si svůj první multiplayer zápas!"));
        _localizer["MatchHistory_Tab_All"].Returns(new LocalizedString("MatchHistory_Tab_All", "Vše"));
        _localizer["MatchHistory_Tab_QuickMatch"].Returns(new LocalizedString("MatchHistory_Tab_QuickMatch", "⚔️ Quick Match"));
        _localizer["MatchHistory_Tab_PrivateRoom"].Returns(new LocalizedString("MatchHistory_Tab_PrivateRoom", "🏠 Private Room"));
        _localizer["MatchHistory_Stats_Played"].Returns(new LocalizedString("MatchHistory_Stats_Played", "Odehráno"));
        _localizer["MatchHistory_Stats_Wins"].Returns(new LocalizedString("MatchHistory_Stats_Wins", "Výhry"));
        _localizer["MatchHistory_Stats_Losses"].Returns(new LocalizedString("MatchHistory_Stats_Losses", "Prohry"));
        _localizer["MatchHistory_Stats_WinRate"].Returns(new LocalizedString("MatchHistory_Stats_WinRate", "Win Rate"));
        _localizer["MatchHistory_Result_Win"].Returns(new LocalizedString("MatchHistory_Result_Win", "Výhra"));
        _localizer["MatchHistory_Result_Loss"].Returns(new LocalizedString("MatchHistory_Result_Loss", "Prohra"));
        _localizer["MatchHistory_Result_Draw"].Returns(new LocalizedString("MatchHistory_Result_Draw", "Remíza"));
        _localizer["MatchHistory_Today"].Returns(new LocalizedString("MatchHistory_Today", "Dnes"));
        _localizer["MatchHistory_Yesterday"].Returns(new LocalizedString("MatchHistory_Yesterday", "Včera"));
        _localizer["MatchHistory_ThisWeek"].Returns(new LocalizedString("MatchHistory_ThisWeek", "Tento týden"));
        _localizer["MatchHistory_Older"].Returns(new LocalizedString("MatchHistory_Older", "Starší"));
        _localizer["MatchHistory_Type_Quick"].Returns(new LocalizedString("MatchHistory_Type_Quick", "⚔️ Quick"));
        _localizer["MatchHistory_Type_Private"].Returns(new LocalizedString("MatchHistory_Type_Private", "🏠 Private"));
        _localizer["MatchHistory_Duration"].Returns(new LocalizedString("MatchHistory_Duration", "Délka"));
        _localizer["MatchHistory_XP"].Returns(new LocalizedString("MatchHistory_XP", "XP"));
        _localizer["MatchHistory_Back"].Returns(new LocalizedString("MatchHistory_Back", "Zpět"));
        _localizer["Button_Cancel"].Returns(new LocalizedString("Button_Cancel", "Zrušit"));
        _localizer["Button_Create"].Returns(new LocalizedString("Button_Create", "Vytvořit"));
        _localizer["Button_Join"].Returns(new LocalizedString("Button_Join", "Připojit se"));
    }

    [Fact]
    public void MultiplayerLanding_Renders_QuickMatchAndPrivateRoom()
    {
        // Arrange & Act
        var cut = Render<Multiplayer>();

        // Assert
        cut.Find(".quick-match-card").Should().NotBeNull();
        cut.Find(".private-room-card").Should().NotBeNull();
    }

    [Fact]
    public void MultiplayerLanding_QuickMatchCard_ShowsCorrectContent()
    {
        // Arrange & Act
        var cut = Render<Multiplayer>();

        // Assert
        cut.Find(".quick-match-card").TextContent.Should().Contain("1v1 Souboj");
        cut.Find(".quick-match-card").TextContent.Should().Contain("Náhodný soupeř");
        cut.Find(".quick-match-card").TextContent.Should().Contain("Liga XP");
    }

    [Fact]
    public void MultiplayerLanding_PrivateRoomCard_ShowsCorrectContent()
    {
        // Arrange & Act
        var cut = Render<Multiplayer>();

        // Assert
        cut.Find(".private-room-card").TextContent.Should().Contain("Soukromá místnost");
        cut.Find(".private-room-card").TextContent.Should().Contain("Pozvi kamaráda");
        cut.Find(".private-room-card").TextContent.Should().Contain("Bez liga XP");
    }

    [Fact]
    public void MultiplayerLanding_ClickQuickMatch_NavigatesToMatchmaking()
    {
        // Arrange
        var cut = Render<Multiplayer>();
        var quickMatchButton = cut.Find(".quick-match-button");

        // Act
        quickMatchButton.Click();

        // Assert
        _navigationManager.Uri.Should().Contain("/multiplayer/quick-match");
    }

    [Fact]
    public void MultiplayerLanding_ClickCreateRoom_ShowsSettingsModal()
    {
        // Arrange
        var cut = Render<Multiplayer>();
        var createRoomButton = cut.Find(".create-room-button");

        // Act
        createRoomButton.Click();

        // Assert
        cut.Find(".create-room-modal").Should().NotBeNull();
    }

    [Fact]
    public void MultiplayerLanding_ClickJoinRoom_ShowsCodeInput()
    {
        // Arrange
        var cut = Render<Multiplayer>();
        var joinRoomButton = cut.Find(".join-room-button");

        // Act
        joinRoomButton.Click();

        // Assert
        cut.Find(".join-room-modal").Should().NotBeNull();
    }

    [Fact]
    public void MultiplayerLanding_ClickHistory_NavigatesToHistory()
    {
        // Arrange
        var cut = Render<Multiplayer>();
        var historyButton = cut.Find(".history-link");

        // Act
        historyButton.Click();

        // Assert
        _navigationManager.Uri.Should().Contain("/multiplayer/history");
    }

    [Fact]
    public void MultiplayerLanding_PrivateRoomCard_HasTwoActions()
    {
        // Arrange & Act
        var cut = Render<Multiplayer>();

        // Assert
        cut.Find(".create-room-button").Should().NotBeNull();
        cut.Find(".join-room-button").Should().NotBeNull();
    }
}
