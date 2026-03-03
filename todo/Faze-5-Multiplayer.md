# Fáze 5: Multiplayer & Social (Týden 8-9)

> **Cíl:** Real-time 1v1 souboje přes SignalR (Quick Match + Private Rooms), týmy a klany
> **Závislost:** Fáze 1 (herní smyčka), Fáze 2 (ligy)
> **Tempo.Blazor komponenty:** TmCard, TmButton, TmBadge, TmModal, TmIcon, TmProgressBar, TmAvatar, TmAvatarGroup, TmTextInput, TmTextArea, TmNumberInput, TmSelect, TmFormField, TmFormSection, TmAlert, TmSpinner, TmEmptyState, TmTabs, TmTabPanel, TmChip, TmTooltip, TmDataTable, TmCopyButton, TmRadioGroup, FluentValidationValidator, ToastService

---

## ⚠️ KRITICKÁ PRAVIDLA

- **TDD:** Test FIRST → RED → GREEN → REFACTOR
- **Žádné hardcoded texty** → vše z `.resx`
- **FluentValidation** na FE i BE s lokalizací
- **DTOs** v `LexiQuest.Shared`
- **SignalR:** Strongly-typed hubs, connection state management
- **Produkční kód** od prvního řádku

---

## T-500: UC-020 Multiplayer 1v1 - Backend (SignalR)

### T-500.1: SignalR Setup
- [ ] Přidat `Microsoft.AspNetCore.SignalR` NuGet do Api projektu
- [ ] Přidat `Microsoft.AspNetCore.SignalR.Client` NuGet do Blazor projektu
- [ ] Nastavit `AddSignalR()` v Program.cs
- [ ] Nastavit `MapHub<MatchHub>("/hubs/match")` v pipeline
- [ ] Konfigurovat CORS pro SignalR (Blazor origin)
- [ ] Nastavit JWT autentizaci pro SignalR (query string token)

### T-500.2: Hub Contracts (Shared)
- [ ] Vytvořit `IMatchHub` interface v Shared (server methods):
  - **Quick Match:**
  - `JoinMatchmaking()` → přidá hráče do fronty
  - `CancelMatchmaking()` → odebere z fronty
  - **Private Rooms:**
  - `CreateRoom(RoomSettingsDto settings)` → vytvoří místnost, vrátí kód
  - `JoinRoom(string roomCode)` → připojí se do místnosti
  - `LeaveRoom()` → opustí místnost/lobby
  - `SetReady()` → hráč je připraven
  - `RequestRematch()` → rematch ve stejné místnosti po skončení hry
  - **Společné (Quick Match + Private Room):**
  - `SubmitAnswer(string answer, int timeSpentMs)` → odešle odpověď
  - `Forfeit()` → vzdá zápas
  - `SendLobbyMessage(string message)` → chat zpráva v lobby
- [ ] Vytvořit `IMatchClient` interface v Shared (client methods):
  - **Quick Match:**
  - `MatchFound(MatchFoundEvent match)` → nalezen soupeř
  - `MatchmakingTimeout()` → timeout (30s)
  - **Private Rooms:**
  - `RoomCreated(RoomCreatedEvent room)` → místnost vytvořena s kódem
  - `PlayerJoinedRoom(PlayerJoinedRoomEvent player)` → soupeř se připojil do lobby
  - `PlayerLeftRoom()` → soupeř opustil lobby
  - `PlayerReady(Guid playerId)` → hráč je připraven
  - `RoomExpired()` → kód místnosti vypršel
  - `RematchRequested(Guid playerId)` → soupeř chce rematch
  - `LobbyMessage(LobbyMessageDto message)` → chat zpráva
  - **Společné:**
  - `CountdownTick(int secondsRemaining)` → odpočet před startem
  - `RoundStarted(MultiplayerRoundDto round)` → nové slovo
  - `OpponentAnswered(OpponentProgressDto progress)` → soupeř odpověděl
  - `OpponentProgress(int correctCount, int totalAnswered)` → update progress
  - `MatchEnded(MatchResultDto result)` → konec zápasu
  - `OpponentDisconnected()` → soupeř se odpojil

### T-500.3: DTOs pro Multiplayer (Shared)
- [ ] Vytvořit `MatchFoundEvent` DTO (MatchId, OpponentUsername, OpponentLevel, OpponentAvatar, StartsAt, IsPrivateRoom)
- [ ] Vytvořit `MultiplayerRoundDto` DTO (RoundNumber, ScrambledWord, WordLength, TimeLimit)
- [ ] Vytvořit `OpponentProgressDto` DTO (CorrectCount, TotalAnswered, ComboCount)
- [ ] Vytvořit `MatchResultDto` DTO (WinnerId, YourScore, OpponentScore, YourTime, OpponentTime, XPEarned, LeagueXPEarned, IsDraw, IsPrivateRoom, RoomCode)
  - **Pozn:** `LeagueXPEarned` je vždy 0 pro Private Room zápasy
- [ ] Vytvořit `PlayerMatchResult` DTO (Username, Avatar, CorrectCount, TotalTime, ComboMax, XPEarned)
- [ ] Vytvořit `RoomSettingsDto` DTO (WordCount: 10/15/20, TimeLimitMinutes: 2/3/5, Difficulty: DifficultyLevel, BestOf: 1/3/5)
- [ ] Vytvořit `RoomCreatedEvent` DTO (RoomCode, Settings, CreatedByUsername, ExpiresAt)
- [ ] Vytvořit `PlayerJoinedRoomEvent` DTO (Username, Level, Avatar, IsReady)
- [ ] Vytvořit `LobbyMessageDto` DTO (SenderUsername, Message, SentAt)
- [ ] Vytvořit `RoomStatusDto` DTO (RoomCode, Settings, Players: List, BothReady, ExpiresAt, CurrentGameIndex, BestOfTotal)

### T-500.4: MatchmakingService (TDD)
- [ ] **TEST:** `MatchmakingService_JoinQueue_AddsPlayer` → RED
- [ ] **TEST:** `MatchmakingService_JoinQueue_TwoPlayers_CreatesMatch` → RED
- [ ] **TEST:** `MatchmakingService_CancelQueue_RemovesPlayer` → RED
- [ ] **TEST:** `MatchmakingService_Timeout_30s_NotifiesPlayer` → RED
- [ ] **TEST:** `MatchmakingService_AlreadyInQueue_RejectsDuplicate` → RED
- [ ] **TEST:** `MatchmakingService_MatchPlayers_SimilarLevel_Preferred` → RED
- [ ] Vytvořit `IMatchmakingService` interface
- [ ] Implementovat `MatchmakingService` s in-memory queue:
  - ConcurrentQueue pro čekající hráče
  - Matching algorithm: preference pro podobný level (±3)
  - Timeout: 30s → nabídnout AI soupeře nebo cancel
  - Background task pro continuous matching
- [ ] **GREEN:** Všechny testy prochází

### T-500.5: MultiplayerGameService (TDD)
- [ ] **TEST:** `MultiplayerGameService_CreateMatch_Initializes15Rounds` → RED
- [ ] **TEST:** `MultiplayerGameService_CreateMatch_3MinuteLimit` → RED
- [ ] **TEST:** `MultiplayerGameService_SubmitAnswer_Correct_IncreasesScore` → RED
- [ ] **TEST:** `MultiplayerGameService_SubmitAnswer_Wrong_NoScoreChange` → RED
- [ ] **TEST:** `MultiplayerGameService_BothComplete15Words_EndsMatch` → RED
- [ ] **TEST:** `MultiplayerGameService_TimerExpires_EndsMatch` → RED
- [ ] **TEST:** `MultiplayerGameService_DetermineWinner_ByCorrectCount` → RED
- [ ] **TEST:** `MultiplayerGameService_DetermineWinner_Tie_BySpeed` → RED
- [ ] **TEST:** `MultiplayerGameService_Forfeit_OpponentWins` → RED
- [ ] **TEST:** `MultiplayerGameService_Disconnect_30sGrace_ThenForfeit` → RED
- [ ] **TEST:** `MultiplayerGameService_Rewards_WinnerGetsBonus` → RED
- [ ] **TEST:** `MultiplayerGameService_Rewards_LoserGetsBase` → RED
- [ ] Vytvořit `IMultiplayerGameService` interface
- [ ] Implementovat `MultiplayerGameService`:
  - Sdílená sada slov pro oba hráče (dle nastavení: 10/15/20)
  - Globální timer (dle nastavení: 2/3/5 min)
  - Real-time synchronizace skóre
  - Winner determination: correct count → total time
  - **Quick Match XP:** winner 100 XP + league 50 XP, loser 30 XP + league 15 XP
  - **Private Room XP:** winner 100 XP (0 liga XP), loser 30 XP (0 liga XP)
  - Best of X support: sledování skóre série (1:0, 1:1, 2:1 atd.)
  - Difficulty selection: výběr slov dle nastavené obtížnosti
- [ ] **GREEN:** Všechny testy prochází

### T-500.6: MatchHub Implementation
- [ ] **TEST:** `MatchHub_JoinMatchmaking_AddsToQueue` → RED
- [ ] **TEST:** `MatchHub_MatchFound_NotifiesBothPlayers` → RED
- [ ] **TEST:** `MatchHub_SubmitAnswer_BroadcastsToOpponent` → RED
- [ ] **TEST:** `MatchHub_Disconnect_NotifiesOpponent` → RED
- [ ] Vytvořit `MatchHub : Hub<IMatchClient>` implementující `IMatchHub`
- [ ] Implementovat connection management (group per match)
- [ ] Implementovat JoinMatchmaking → trigger MatchmakingService
- [ ] Implementovat SubmitAnswer → validate → broadcast to opponent
- [ ] Implementovat disconnect handling s 30s grace period
- [ ] `[Authorize]` na hub
- [ ] **GREEN:** Testy prochází

---

## T-501: UC-020 Multiplayer 1v1 - Frontend

### T-501.1: SignalR Client Setup
- [ ] Vytvořit `IMatchHubClient` service v Blazor/Services/
- [ ] Implementovat `MatchHubClient`:
  - `HubConnection` builder s JWT token
  - Auto-reconnect s exponential backoff
  - Connection state management (Connecting, Connected, Disconnected, Reconnecting)
  - Event handlers pro všechny IMatchClient metody
- [ ] Zaregistrovat v DI jako Scoped
- [ ] Implementovat `IAsyncDisposable` pro cleanup

### T-501.2: Matchmaking Screen (Tempo.Blazor)
- [ ] **TEST (bUnit):** `MatchmakingScreen_Renders_SearchingState` → RED
- [ ] **TEST (bUnit):** `MatchmakingScreen_MatchFound_ShowsOpponent` → RED
- [ ] **TEST (bUnit):** `MatchmakingScreen_Timeout_ShowsOptions` → RED
- [ ] Vytvořit `Matchmaking.razor` (`@page "/multiplayer/quick-match"`)
- [ ] `@inject IStringLocalizer<Multiplayer> L`
- [ ] **Searching State**:
  - "⚔️ 1v1 SOUBOJ ⚔️" heading
  - "Hledání soupeře..." s `TmSpinner` (Lg)
  - Váš `TmAvatar` + animované "VS" + "?" `TmAvatar`
  - Timer: countdown "00:{seconds}" s `TmProgressBar`
  - `TmButton Variant="Ghost"` "Zrušit hledání"
  - Pravidla: `TmCard` s seznamem pravidel

- [ ] **Match Found State**:
  - "⚔️ SOUPEŘ NALEZEN! ⚔️"
  - Oba `TmAvatar` (váš + opponent) s jmény a levely
  - Countdown: "Začínáme za: 3... 2... 1..." (velké číslo, pulse animace)
  - "Připrav se!" text

- [ ] **Timeout State**:
  - "Soupeř nenalezen"
  - `TmButton` "Zkusit znovu" / `TmButton` "Hrát proti AI" / `TmButton` "Zpět"

- [ ] **GREEN:** Testy prochází

### T-501.3: Real-time Game komponenta (Tempo.Blazor)
- [ ] **TEST (bUnit):** `RealtimeGame_Renders_BothPlayerCards` → RED
- [ ] **TEST (bUnit):** `RealtimeGame_SubmitAnswer_UpdatesScore` → RED
- [ ] **TEST (bUnit):** `RealtimeGame_OpponentAnswered_UpdatesOpponentCard` → RED
- [ ] Vytvořit `RealtimeGame.razor` komponentu
- [ ] **Header**: `TmProgressBar` timer (3:00 odpočet), "Slovo {n}/15"
- [ ] **Player Cards** (side-by-side):
  - Left (Vy): `TmCard` s `TmAvatar`, skóre "{x}/15 ✓", `TmProgressBar`, combo `TmBadge` "🔥 x{n}"
  - Right (Soupeř): `TmCard` s `TmAvatar`, skóre, `TmProgressBar`, combo
  - Opponent card: brief green flash při správné odpovědi
- [ ] **Game Board** (center):
  - Scrambled word (velká písmena v boxech, jako v GameArena)
  - `TmTextInput` pro odpověď (uppercase, center-aligned)
  - `TmButton Variant="Primary"` "Odeslat"
- [ ] **Feedback**:
  - Correct: zelený flash, skóre update, +XP float
  - Wrong: červený shake
- [ ] Real-time updates přes SignalR (opponent progress, timer sync)
- [ ] **GREEN:** Testy prochází

### T-501.4: Match Result Screen (Tempo.Blazor)
- [ ] **TEST (bUnit):** `MatchResult_Victory_ShowsConfetti` → RED
- [ ] **TEST (bUnit):** `MatchResult_Defeat_ShowsMotivation` → RED
- [ ] **TEST (bUnit):** `MatchResult_Draw_ShowsSpeedWinner` → RED
- [ ] Vytvořit `MatchResult.razor` komponentu

- [ ] **Victory**: `TmModal` (Size: Large)
  - "🎉 VÍTĚZSTVÍ!" heading s confetti
  - Oba player cards s score/time
  - Winner badge na vaší kartě (🏆)
  - Quick Match rewards: "⭐ +100 XP", "📈 Liga: +50 XP"
  - Private Room rewards: "⭐ +100 XP" (bez liga řádku)
  - `TmButton Variant="Primary"` "Další zápas" + `TmButton Variant="Ghost"` "Domů"

- [ ] **Defeat**: `TmModal`
  - "😔 PROHRA" heading
  - Opponent card s winner badge
  - Quick Match reward: "+30 XP" + liga XP
  - Private Room reward: "+30 XP" (bez liga)
  - Motivace: "💪 Příště to dáš!"
  - Quick Match: `TmButton` "Odveta" / `TmButton` "Domů"
  - Private Room: `TmButton` "Rematch" / `TmButton` "Domů"

- [ ] **Draw**: `TmModal`
  - "🤝 REMÍZA!" heading
  - Oba cards se stejným skóre
  - Speed tiebreaker: "Rychlejší vyhrává!" s časy
  - Rewards
  - `TmButton` "Další zápas" / `TmButton` "Domů"

- [ ] **GREEN:** Testy prochází

### T-501.5: Match History
- [ ] Vytvořit `GET /api/v1/multiplayer/history` endpoint (s filtrací: all/quick-match/private-room)
- [ ] Vytvořit `GET /api/v1/multiplayer/stats` endpoint (wins, losses, win rate - celkové i per type)
- [ ] Vytvořit `MatchHistory.razor` komponentu (`@page "/multiplayer/history"`):
  - Stats header: `TmStatCard` × 4 (Played, Wins, Losses, Win Rate %)
  - Filter: `TmTabs` (Vše / Quick Match / Private Room)
  - History list: time-grouped (Today, Yesterday, This Week)
  - Each entry: opponent `TmAvatar` + username, score ("12:9"), result `TmBadge` (Win/Loss/Draw), XP, time, type `TmBadge` ("⚔️ Quick" / "🏠 Private")
  - Private Room entries: série skóre pokud Best of > 1 (např. "Série: 2:1")

---

## T-503: Private Rooms - Backend

### T-503.1: Room Entity a Code Generation (TDD)
- [ ] **TEST:** `Room_Create_GeneratesUniqueCode` → RED
- [ ] **TEST:** `Room_Create_CodeFormat_LEXIQ_4AlphaNum` (formát "LEXIQ-XXXX") → RED
- [ ] **TEST:** `Room_Create_SetsExpiresAt_5MinFromNow` → RED
- [ ] **TEST:** `Room_IsExpired_ReturnsTrueAfterExpiry` → RED
- [ ] **TEST:** `Room_IsExpired_ReturnsFalseBeforeExpiry` → RED
- [ ] Vytvořit `Room` entitu v Core/Domain/Entities/:
  - Id (Guid), Code (string, unique), CreatedByUserId, Settings (RoomSettings owned type)
  - Status (RoomStatus enum), ExpiresAt, CreatedAt
  - Player1ConnectionId, Player2ConnectionId
  - Player1Ready, Player2Ready
  - CurrentMatchId (Guid?), GamesPlayed (int), Player1Wins (int), Player2Wins (int)
- [ ] Vytvořit `RoomStatus` enum (WaitingForOpponent, Lobby, Countdown, Playing, BetweenGames, Completed, Expired, Cancelled)
- [ ] Vytvořit `RoomSettings` value object (WordCount: int, TimeLimitMinutes: int, Difficulty: DifficultyLevel, BestOf: int)
- [ ] Implementovat code generation: `GenerateRoomCode()` → "LEXIQ-" + 4 random uppercase alphanumeric
- [ ] Zajistit unikátnost kódu (retry pokud kolize v active rooms)
- [ ] **GREEN:** Testy prochází

### T-503.2: RoomSettingsValidator (TDD)
- [ ] **TEST:** `RoomSettingsValidator_WordCount10_Valid` → RED
- [ ] **TEST:** `RoomSettingsValidator_WordCount15_Valid` → RED
- [ ] **TEST:** `RoomSettingsValidator_WordCount20_Valid` → RED
- [ ] **TEST:** `RoomSettingsValidator_WordCount7_Invalid` → RED
- [ ] **TEST:** `RoomSettingsValidator_TimeLimit2_Valid` → RED
- [ ] **TEST:** `RoomSettingsValidator_TimeLimit3_Valid` → RED
- [ ] **TEST:** `RoomSettingsValidator_TimeLimit5_Valid` → RED
- [ ] **TEST:** `RoomSettingsValidator_TimeLimit10_Invalid` → RED
- [ ] **TEST:** `RoomSettingsValidator_BestOf1_Valid` → RED
- [ ] **TEST:** `RoomSettingsValidator_BestOf3_Valid` → RED
- [ ] **TEST:** `RoomSettingsValidator_BestOf5_Valid` → RED
- [ ] **TEST:** `RoomSettingsValidator_BestOf4_Invalid` → RED
- [ ] **TEST:** `RoomSettingsValidator_DifficultyBeginner_Valid` → RED
- [ ] Vytvořit `RoomSettingsValidator : AbstractValidator<RoomSettingsDto>` s lokalizovanými zprávami
- [ ] Povolené WordCount: [10, 15, 20]
- [ ] Povolené TimeLimitMinutes: [2, 3, 5]
- [ ] Povolené BestOf: [1, 3, 5]
- [ ] Povolené Difficulty: libovolný DifficultyLevel + Mix
- [ ] **GREEN:** Všechny testy prochází

### T-503.3: RoomService (TDD)
- [ ] **TEST:** `RoomService_CreateRoom_ReturnsRoomWithCode` → RED
- [ ] **TEST:** `RoomService_CreateRoom_UserAlreadyHasActiveRoom_Returns409` → RED
- [ ] **TEST:** `RoomService_CreateRoom_SetsExpiresAt5Min` → RED
- [ ] **TEST:** `RoomService_JoinRoom_ValidCode_AddsPlayer2` → RED
- [ ] **TEST:** `RoomService_JoinRoom_InvalidCode_Returns404` → RED
- [ ] **TEST:** `RoomService_JoinRoom_ExpiredCode_Returns410Gone` → RED
- [ ] **TEST:** `RoomService_JoinRoom_RoomFull_Returns409` → RED
- [ ] **TEST:** `RoomService_JoinRoom_OwnRoom_Returns400` → RED
- [ ] **TEST:** `RoomService_JoinRoom_UserAlreadyInAnotherRoom_Returns409` → RED
- [ ] **TEST:** `RoomService_LeaveRoom_Host_CancelsRoom` → RED
- [ ] **TEST:** `RoomService_LeaveRoom_Guest_RemovesFromRoom` → RED
- [ ] **TEST:** `RoomService_SetReady_Player1_SetsFlag` → RED
- [ ] **TEST:** `RoomService_SetReady_BothReady_StartsCountdown` → RED
- [ ] **TEST:** `RoomService_Rematch_BothAccept_StartsNewGame` → RED
- [ ] **TEST:** `RoomService_Rematch_BestOfNotFinished_AutoStartsNextGame` → RED
- [ ] **TEST:** `RoomService_BestOf3_Player1Wins2_SeriesComplete` → RED
- [ ] **TEST:** `RoomService_BestOf5_Player2Wins3_SeriesComplete` → RED
- [ ] **TEST:** `RoomService_ExpiredRoom_CleanupJob_RemovesFromMemory` → RED
- [ ] Vytvořit `IRoomService` interface
- [ ] Implementovat `RoomService`:
  - In-memory ConcurrentDictionary<string, Room> (kód → Room)
  - Max 1 aktivní místnost per hráč (check oba: host i guest)
  - Kód platí 5 minut od vytvoření
  - Po připojení obou hráčů: kód už není potřeba (room přejde do Lobby stavu)
  - Best of X: automatický start další hry po dokončení, dokud série neskončí
  - Po dokončení série: nabídnout rematch (nová série se stejnými settings)
- [ ] **GREEN:** Všechny testy prochází

### T-503.4: Room Cleanup Background Job
- [ ] **TEST:** `RoomCleanupJob_RemovesExpiredRooms` → RED
- [ ] **TEST:** `RoomCleanupJob_KeepsActiveRooms` → RED
- [ ] Vytvořit `RoomCleanupJob` (Hangfire RecurringJob, běží každou minutu)
- [ ] Odstraní rooms se statusem Expired, Cancelled, Completed (starší než 10 min)
- [ ] Expiruje rooms ve stavu WaitingForOpponent kde ExpiresAt < now
- [ ] **GREEN:** Testy prochází

### T-503.5: Lobby Chat (TDD)
- [ ] **TEST:** `LobbyChat_SendMessage_BroadcastsToBothPlayers` → RED
- [ ] **TEST:** `LobbyChat_MessageTooLong_Returns400` (max 200 znaků) → RED
- [ ] **TEST:** `LobbyChat_RateLimit_Max10PerMinute` → RED
- [ ] Implementovat chat v MatchHub:
  - `SendLobbyMessage(string message)` → broadcast do room group
  - Validace: max 200 znaků, rate limit 10 zpráv/min
  - Sanitizace: HTML escape, basic profanity filter
- [ ] **GREEN:** Testy prochází

### T-503.6: MatchHub rozšíření pro Private Rooms
- [ ] **TEST:** `MatchHub_CreateRoom_ReturnsRoomCreatedEvent` → RED
- [ ] **TEST:** `MatchHub_JoinRoom_ValidCode_NotifiesBothPlayers` → RED
- [ ] **TEST:** `MatchHub_JoinRoom_InvalidCode_ReturnsError` → RED
- [ ] **TEST:** `MatchHub_SetReady_BothReady_StartsCountdown` → RED
- [ ] **TEST:** `MatchHub_LeaveRoom_NotifiesOpponent` → RED
- [ ] **TEST:** `MatchHub_Disconnect_InLobby_CancelsRoom` → RED
- [ ] **TEST:** `MatchHub_Rematch_BothAccept_StartsNewMatch` → RED
- [ ] Rozšířit `MatchHub` o room metody:
  - `CreateRoom` → vytvořit room, přidat hráče do SignalR group `room:{code}`
  - `JoinRoom` → přidat do group, notifikovat hostitele
  - `LeaveRoom` → odebrat z group, notifikovat soupeře
  - `SetReady` → check oba ready → countdown → start match
  - `RequestRematch` → po skončení hry/série → nová hra/série
  - `SendLobbyMessage` → broadcast do room group
- [ ] Room-specific SignalR group: `room:{roomCode}`
- [ ] Match-specific group: `match:{matchId}` (reuse z Quick Match)
- [ ] **GREEN:** Testy prochází

### T-503.7: Resources pro Private Rooms
- [ ] Rozšířit `Multiplayer.resx` o klíče:
  - Room.Create.Title, Room.Create.Button, Room.Create.Settings.Title
  - Room.Settings.WordCount, Room.Settings.TimeLimit, Room.Settings.Difficulty, Room.Settings.BestOf
  - Room.Code.Label, Room.Code.CopySuccess, Room.Code.ShareText
  - Room.Lobby.Title, Room.Lobby.WaitingForOpponent, Room.Lobby.OpponentJoined
  - Room.Lobby.Ready, Room.Lobby.NotReady, Room.Lobby.BothReady
  - Room.Lobby.Chat.Placeholder, Room.Lobby.Chat.Send
  - Room.Expired, Room.Full, Room.NotFound, Room.InvalidCode
  - Room.Series.Score ("Série: {0}:{1}"), Room.Series.GameOf ("Hra {0} z {1}")
  - Room.Rematch.Request, Room.Rematch.Accept, Room.Rematch.Decline
  - Room.Leave.Confirm, Room.NoLeagueXP.Info

---

## T-504: Private Rooms - Frontend

### T-504.1: Multiplayer Landing Page (Tempo.Blazor)
- [ ] **TEST (bUnit):** `MultiplayerLanding_Renders_QuickMatchAndPrivateRoom` → RED
- [ ] **TEST (bUnit):** `MultiplayerLanding_ClickQuickMatch_NavigatesToMatchmaking` → RED
- [ ] **TEST (bUnit):** `MultiplayerLanding_ClickCreateRoom_ShowsSettingsModal` → RED
- [ ] **TEST (bUnit):** `MultiplayerLanding_ClickJoinRoom_ShowsCodeInput` → RED
- [ ] Přepracovat `Multiplayer.razor` (`@page "/multiplayer"`) na landing s výběrem režimu:
- [ ] `@inject IStringLocalizer<Multiplayer> L`
- [ ] Layout: 2 velké `TmCard` (Elevated) vedle sebe:

- [ ] **Quick Match Card** (`TmCard`):
  - `TmIcon` (⚔️ swords) velký
  - Titulek: `@L["QuickMatch.Title"]`
  - Popis: "Náhodný soupeř, plné XP + liga body"
  - `TmBadge Variant="Success"` "Liga XP ✓"
  - Pravidla: 15 slov, 3 min, automatický matchmaking
  - `TmButton Variant="Primary" Size="Lg" Block="true"` → `@L["QuickMatch.Button"]`

- [ ] **Private Room Card** (`TmCard`):
  - `TmIcon` (🏠 house/door) velký
  - Titulek: `@L["PrivateRoom.Title"]`
  - Popis: "Pozvi kamaráda, vlastní pravidla"
  - `TmBadge Variant="Warning"` "Bez liga XP"
  - Dvě akce:
    - `TmButton Variant="Primary" Size="Lg" Block="true"` → `@L["Room.Create.Button"]` (Vytvořit místnost)
    - `TmButton Variant="Outline" Size="Lg" Block="true"` → `@L["Room.Join.Button"]` (Připojit se)

- [ ] **Match History link**: `TmButton Variant="Ghost"` "Historie zápasů" → `/multiplayer/history`
- [ ] **GREEN:** Testy prochází
- [ ] **REFACTOR:** Responsive (side-by-side desktop, stack mobile)

### T-504.2: Create Room Modal - Settings (Tempo.Blazor)
- [ ] **TEST (bUnit):** `CreateRoomModal_Renders_AllSettings` → RED
- [ ] **TEST (bUnit):** `CreateRoomModal_InvalidSettings_ShowsValidation` → RED
- [ ] **TEST (bUnit):** `CreateRoomModal_Submit_CreatesRoomAndShowsCode` → RED
- [ ] Vytvořit `CreateRoomModal.razor` komponentu
- [ ] `TmModal` (Size: Medium) s formulářem:

- [ ] **Počet slov** (`TmFormField`):
  - `TmRadioGroup` s možnostmi: 10 / 15 (default) / 20
  - Label: `@L["Room.Settings.WordCount"]`

- [ ] **Časový limit** (`TmFormField`):
  - `TmRadioGroup` s možnostmi: 2 min / 3 min (default) / 5 min
  - Label: `@L["Room.Settings.TimeLimit"]`

- [ ] **Obtížnost** (`TmFormField`):
  - `TmSelect` s možnostmi: Beginner 🌱 / Intermediate 🌿 / Advanced 🌳 / Expert 🔥 / Mix (default)
  - Label: `@L["Room.Settings.Difficulty"]`

- [ ] **Best of** (`TmFormField`):
  - `TmRadioGroup` s možnostmi: 1 hra (default) / Best of 3 / Best of 5
  - Label: `@L["Room.Settings.BestOf"]`

- [ ] `<FluentValidationValidator />`
- [ ] `TmAlert Severity="Info"` → `@L["Room.NoLeagueXP.Info"]` ("Private room zápasy nedávají liga XP")
- [ ] Footer: `TmButton Variant="Primary"` "Vytvořit místnost" / `TmButton Variant="Ghost"` "Zrušit"
- [ ] **GREEN:** Testy prochází

### T-504.3: Join Room Modal (Tempo.Blazor)
- [ ] **TEST (bUnit):** `JoinRoomModal_Renders_CodeInput` → RED
- [ ] **TEST (bUnit):** `JoinRoomModal_InvalidCode_ShowsError` → RED
- [ ] **TEST (bUnit):** `JoinRoomModal_ValidCode_JoinsRoom` → RED
- [ ] **TEST (bUnit):** `JoinRoomModal_ExpiredCode_ShowsExpiredError` → RED
- [ ] Vytvořit `JoinRoomModal.razor` komponentu
- [ ] `TmModal` (Size: Small):
  - Titulek: `@L["Room.Join.Title"]`
  - `TmFormField` + `TmTextInput` pro kód:
    - Placeholder: "LEXIQ-XXXX"
    - Uppercase auto-transform
    - Max length: 10 (LEXIQ-XXXX)
    - Auto-focus
  - `<FluentValidationValidator />`
  - Error states:
    - `TmAlert Severity="Error"` "Místnost nenalezena" (404)
    - `TmAlert Severity="Warning"` "Kód vypršel" (410)
    - `TmAlert Severity="Error"` "Místnost je plná" (409)
  - `TmButton Variant="Primary"` "Připojit se" (s `IsLoading`)
- [ ] **GREEN:** Testy prochází

### T-504.4: Room Lobby Screen (Tempo.Blazor)
- [ ] **TEST (bUnit):** `RoomLobby_HostView_ShowsCodeAndWaiting` → RED
- [ ] **TEST (bUnit):** `RoomLobby_OpponentJoined_ShowsBothPlayers` → RED
- [ ] **TEST (bUnit):** `RoomLobby_BothReady_ShowsCountdown` → RED
- [ ] **TEST (bUnit):** `RoomLobby_Chat_SendsAndReceivesMessages` → RED
- [ ] **TEST (bUnit):** `RoomLobby_Expiry_ShowsExpiredMessage` → RED
- [ ] Vytvořit `RoomLobby.razor` komponentu

- [ ] **Waiting for Opponent State** (host čeká):
  - Titulek: `@L["Room.Lobby.WaitingForOpponent"]`
  - Room Code velkým fontem (monospace, 32px): "LEXIQ-7K3M"
  - `TmCopyButton` pro zkopírování kódu do schránky → toast `@L["Room.Code.CopySuccess"]`
  - Share text: `@L["Room.Code.ShareText"]` ("Připoj se do mé místnosti: LEXIQ-7K3M")
  - Countdown do expirace: `TmProgressBar` s zbývajícím časem (5 min)
  - Host `TmAvatar` (vlevo) + `TmSpinner` + "?" placeholder (vpravo)
  - Settings summary: `TmCard` s přehledem nastavení (slov, čas, obtížnost, best of)
  - `TmButton Variant="Ghost"` "Zrušit místnost"

- [ ] **Lobby State** (oba hráči přítomni):
  - Oba `TmAvatar` s username, level, streak `TmBadge`
  - Settings summary: `TmCard` s pravidly hry
  - Ready indikátory:
    - Per hráč: `TmBadge Variant="Success"` "Připraven ✓" / `TmBadge Variant="Warning"` "Čeká..."
  - `TmButton Variant="Primary" Size="Lg"` "Jsem připraven!" (toggle → po kliknutí "Čekám na soupeře...")
  - Oba ready → automatický countdown 3-2-1 (pulse animace)

- [ ] **Chat v lobby**:
  - Chatovací okno (scroll, max výška 200px)
  - Zprávy: `TmAvatar` (mini) + username + text + čas
  - Input: `TmTextInput` + `TmButton Variant="Primary" Size="Sm"` "Odeslat"
  - Max 200 znaků, Enter pro odeslání

- [ ] **GREEN:** Testy prochází
- [ ] **REFACTOR:** Responsive layout, animace

### T-504.5: Series Score Overlay (Best of X)
- [ ] **TEST (bUnit):** `SeriesScore_BestOf3_ShowsScoreAndGameNumber` → RED
- [ ] **TEST (bUnit):** `SeriesScore_SeriesComplete_ShowsFinalResult` → RED
- [ ] Vytvořit `SeriesScore.razor` komponentu
- [ ] Zobrazení během hry (v header area):
  - `TmBadge` "Hra {current} z {bestOf}" (např. "Hra 2 z 3")
  - Série skóre: "Série: {player1Wins} : {player2Wins}" s `TmAvatar` mini po stranách
  - Indikátor kdo vede (bold/highlight na vedoucím hráči)
- [ ] Mezi hrami (BetweenGames state):
  - `TmModal` s výsledkem aktuální hry
  - Série skóre velké ("1 : 1")
  - Pokud série neskončila: "Další hra začíná za..." countdown (5s)
  - Pokud série skončila: finální výsledky + rematch option
- [ ] **GREEN:** Testy prochází

### T-504.6: Match Result rozšíření pro Private Rooms
- [ ] **TEST (bUnit):** `MatchResult_PrivateRoom_ShowsNoLeagueXP` → RED
- [ ] **TEST (bUnit):** `MatchResult_PrivateRoom_ShowsRematchButton` → RED
- [ ] **TEST (bUnit):** `MatchResult_PrivateRoom_BestOf_ShowsSeriesScore` → RED
- [ ] Rozšířit `MatchResult.razor`:
  - Private Room: nezobrazovat "📈 Liga: +50 XP" řádek
  - Private Room: `TmAlert Severity="Info"` "Private room zápasy nedávají liga body"
  - Rematch tlačítko: `TmButton Variant="Primary"` "Rematch" (nová hra/série, stejné settings, stejná místnost)
  - Pokud soupeř requestoval rematch: `TmAlert` "Soupeř chce rematch!" + `TmButton` Accept/Decline
  - Best of série: zobrazit finální série skóre ("Série: 2:1 - Výhra!")
- [ ] **GREEN:** Testy prochází

### T-504.7: Validátor pro JoinRoom (Frontend)
- [ ] **TEST (bUnit):** `JoinRoomValidator_EmptyCode_ShowsError` → RED
- [ ] **TEST (bUnit):** `JoinRoomValidator_WrongFormat_ShowsError` → RED
- [ ] **TEST (bUnit):** `JoinRoomValidator_ValidCode_NoErrors` → RED
- [ ] Vytvořit `JoinRoomModel` (Code: string)
- [ ] Vytvořit `JoinRoomModelValidator : AbstractValidator<JoinRoomModel>`:
  - NotEmpty
  - Matches regex `^LEXIQ-[A-Z0-9]{4}$`
  - Lokalizované zprávy z .resx
- [ ] **GREEN:** Testy prochází

---

## T-502: UC-021 Týmy a Klany

### T-502.1: Domain Entities (TDD)
- [ ] **TEST:** `Team_Create_SetsDefaults` → RED
- [ ] **TEST:** `Team_AddMember_IncreasesMemberCount` → RED
- [ ] **TEST:** `Team_AddMember_Max20_Throws` → RED
- [ ] **TEST:** `TeamMember_Create_DefaultRoleMember` → RED
- [ ] **TEST:** `Team_Name_3to30Chars` → RED
- [ ] **TEST:** `Team_Tag_2to4Chars` → RED
- [ ] Vytvořit `Team` entitu (Id, Name, Tag, Description, LogoUrl, LeaderId, CreatedAt, Members)
- [ ] Vytvořit `TeamMember` entitu (UserId, TeamId, Role, JoinedAt)
- [ ] Vytvořit `TeamRole` enum (Leader, Officer, Member)
- [ ] Vytvořit `TeamStats` value object (WeeklyXP, AllTimeXP, Rank, TotalWins)
- [ ] Vytvořit `TeamInvite` entitu (TeamId, InvitedUserId, InvitedByUserId, Status, ExpiresAt)
- [ ] Vytvořit `TeamJoinRequest` entitu (TeamId, UserId, Message, Status, CreatedAt)
- [ ] EF Core konfigurace + migrace
- [ ] **GREEN:** Testy prochází

### T-502.2: TeamService (TDD)
- [ ] **TEST:** `TeamService_Create_PremiumOrCoins_Success` → RED
- [ ] **TEST:** `TeamService_Create_FreeNoCcoins_Returns403` → RED
- [ ] **TEST:** `TeamService_Create_DuplicateName_Returns409` → RED
- [ ] **TEST:** `TeamService_Create_DuplicateTag_Returns409` → RED
- [ ] **TEST:** `TeamService_InviteMember_Officer_Success` → RED
- [ ] **TEST:** `TeamService_InviteMember_RegularMember_Returns403` → RED
- [ ] **TEST:** `TeamService_KickMember_Officer_Success` → RED
- [ ] **TEST:** `TeamService_KickMember_CantKickLeader` → RED
- [ ] **TEST:** `TeamService_Leave_LastMember_DisbandsTeam` → RED
- [ ] **TEST:** `TeamService_TransferLeadership_Success` → RED
- [ ] **TEST:** `TeamService_GetWeeklyRanking_ReturnsSortedByXP` → RED
- [ ] **TEST:** `TeamService_RequestJoin_CreatesRequest` → RED
- [ ] **TEST:** `TeamService_ApproveJoin_AddsMember` → RED
- [ ] Vytvořit `ITeamService` interface
- [ ] Vytvořit DTOs: `TeamDto`, `TeamMemberDto`, `CreateTeamRequest`, `InviteMemberRequest`, `TeamStatsDto`, `TeamRankingDto`
- [ ] Vytvořit validátory: `CreateTeamValidator`, `InviteMemberValidator` s lokalizací
- [ ] Implementovat `TeamService`
- [ ] **GREEN:** Všechny testy prochází

### T-502.3: Team Endpoints
- [ ] Vytvořit `POST /api/v1/teams` (vytvoří tým)
- [ ] Vytvořit `GET /api/v1/teams/{id}` (detail týmu)
- [ ] Vytvořit `PUT /api/v1/teams/{id}` (upraví tým - officer+)
- [ ] Vytvořit `DELETE /api/v1/teams/{id}` (disbanduje tým - leader only)
- [ ] Vytvořit `POST /api/v1/teams/{id}/invite` (pozve člena)
- [ ] Vytvořit `POST /api/v1/teams/{id}/kick/{userId}` (vyhodí člena)
- [ ] Vytvořit `POST /api/v1/teams/{id}/leave` (opustí tým)
- [ ] Vytvořit `POST /api/v1/teams/{id}/transfer-leadership` (předá vedení)
- [ ] Vytvořit `POST /api/v1/teams/{id}/join-request` (žádost o vstup)
- [ ] Vytvořit `POST /api/v1/teams/join-requests/{id}/approve` (schválí žádost)
- [ ] Vytvořit `POST /api/v1/teams/join-requests/{id}/reject` (zamítne žádost)
- [ ] Vytvořit `GET /api/v1/teams/ranking` (týmový žebříček)
- [ ] Vytvořit `GET /api/v1/users/me/team` (můj tým)

### T-502.4: Frontend - Team UI (Tempo.Blazor)
- [ ] **TEST (bUnit):** `TeamPage_NoTeam_ShowsCreateOrJoin` → RED
- [ ] **TEST (bUnit):** `TeamPage_HasTeam_ShowsDashboard` → RED
- [ ] **TEST (bUnit):** `TeamPage_Leader_ShowsManagementOptions` → RED
- [ ] Vytvořit `Team.razor` (`@page "/team"`)
- [ ] `@inject IStringLocalizer<Team> L`

- [ ] **No Team State**:
  - `TmEmptyState` "Nemáš tým"
  - `TmButton Variant="Primary"` "Vytvořit tým" (Premium/1000 coins)
  - `TmButton Variant="Outline"` "Hledat tým"
  - Browse teams: search + list

- [ ] **Create Team Modal**: `TmModal` s:
  - `TmFormField` + `TmTextInput` Název (3-30 znaků)
  - `TmFormField` + `TmTextInput` Tag (2-4 znaky)
  - `TmFormField` + `TmTextArea` Popis
  - Logo upload (optional)
  - `<FluentValidationValidator />`
  - `TmButton` "Vytvořit" (zobrazí cost: Premium free / 1000 coins)

- [ ] **Team Dashboard**:
  - Header: Logo, Team name, Tag `TmBadge`, member count
  - **Members list**: `TmDataTable` s columns: `TmAvatar`, Username, Role `TmBadge`, Weekly XP, Joined
  - **Team Stats**: `TmStatCard` × 4 (Weekly XP, All-time XP, Rank, Wins)
  - **Weekly Ranking**: pozice týmu v žebříčku
  - **Management** (Leader/Officer):
    - `TmButton` "Pozvat" → invite modal
    - `TmButton Variant="Danger"` "Vyhodit" na member rows
    - `TmButton` "Předat vedení"
    - Join requests: `TmCard` list s Accept/Reject buttons

- [ ] **GREEN:** Testy prochází
- [ ] **REFACTOR:** Styling dle UI-UX-014 principles

---

## Ověření dokončení fáze

### Quick Match
- [ ] SignalR connection funguje s JWT auth
- [ ] Matchmaking: join queue → match found → countdown → game start
- [ ] 1v1 gameplay: shared words, real-time score sync, timer
- [ ] Match result: victory/defeat/draw s XP rewards (osobní XP + liga XP)
- [ ] Disconnect handling: 30s grace period, forfeit

### Private Rooms
- [ ] Create room: nastavení pravidel → kód vygenerován → lobby
- [ ] Room code: formát LEXIQ-XXXX, expirace 5 min, copy/share funkce
- [ ] Join room: zadat kód → připojení do lobby → oba hráči viditelní
- [ ] Lobby: avatary, levely, streaky, ready tlačítka, chat
- [ ] Ready flow: oba kliknou ready → countdown 3-2-1 → hra začne
- [ ] Custom pravidla: počet slov (10/15/20), čas (2/3/5 min), obtížnost, Best of (1/3/5)
- [ ] Best of X: skóre série, automatický start další hry, finální výsledek
- [ ] XP: osobní XP ano, liga XP **NE** (prevence farmení)
- [ ] Rematch: po hře/sérii nabídnout rematch ve stejné místnosti
- [ ] Max 1 aktivní místnost per hráč
- [ ] Vyžaduje přihlášení (ne guest)
- [ ] Dostupné pro všechny (ne jen premium)
- [ ] Room expiry: automatické čištění expirovaných/dokončených rooms
- [ ] Lobby chat: max 200 znaků, rate limit, sanitizace

### Shared
- [ ] Match history se stats (Quick Match + Private Room odlišené)
- [ ] Multiplayer landing page: výběr Quick Match / Private Room

### Teams
- [ ] Teams: create (premium/coins), invite, kick, leave, transfer leadership
- [ ] Team dashboard: members, stats, weekly ranking
- [ ] Join requests: request → approve/reject

### Kvalita
- [ ] Všechny texty z .resx
- [ ] FluentValidation na FE i BE
- [ ] `dotnet test` → všechny testy zelené
