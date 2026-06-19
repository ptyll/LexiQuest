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
- [x] Přidat `Microsoft.AspNetCore.SignalR` NuGet do Api projektu
- [x] Přidat `Microsoft.AspNetCore.SignalR.Client` NuGet do Blazor projektu
- [x] Nastavit `AddSignalR()` v Program.cs
- [x] Nastavit `MapHub<MatchHub>("/hubs/match")` v pipeline
- [x] Konfigurovat CORS pro SignalR (Blazor origin)
- [x] Nastavit JWT autentizaci pro SignalR (query string token)

### T-500.2: Hub Contracts (Shared)
- [x] Vytvořit `IMatchHub` interface v Shared (server methods):
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
- [x] Vytvořit `IMatchClient` interface v Shared (client methods):
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

### T-500.3: DTOs pro Multiplayer (Shared) - ✅ HOTOVÉ
- [x] Vytvořit `MatchFoundEvent` DTO (MatchId, OpponentUsername, OpponentLevel, OpponentAvatar, StartsAt, IsPrivateRoom)
- [x] Vytvořit `MultiplayerRoundDto` DTO (RoundNumber, ScrambledWord, WordLength, TimeLimit)
- [x] Vytvořit `OpponentProgressDto` DTO (CorrectCount, TotalAnswered, ComboCount)
- [x] Vytvořit `MatchResultDto` DTO (WinnerId, YourScore, OpponentScore, YourTime, OpponentTime, XPEarned, LeagueXPEarned, IsDraw, IsPrivateRoom, RoomCode, YourResult, OpponentResult)
  - **Pozn:** `LeagueXPEarned` je vždy 0 pro Private Room zápasy
- [x] Vytvořit `PlayerMatchResult` DTO (Username, Avatar, CorrectCount, TotalTime, ComboMax, XPEarned)
- [x] Vytvořit `RoomSettingsDto` DTO (WordCount: 10/15/20, TimeLimitMinutes: 2/3/5, Difficulty: DifficultyLevel, BestOf: 1/3/5)
- [x] Vytvořit `RoomCreatedEvent` DTO (RoomCode, Settings, CreatedByUsername, ExpiresAt)
- [x] Vytvořit `PlayerJoinedRoomEvent` DTO (Username, Level, Avatar, IsReady)
- [x] Vytvořit `LobbyMessageDto` DTO (SenderUsername, Message, SentAt)
- [x] Vytvořit `RoomStatusDto` DTO (RoomCode, Settings, Players: List, BothReady, ExpiresAt, CurrentGameIndex, BestOfTotal)

### T-500.4: MatchmakingService (TDD)
- [x] **TEST:** `MatchmakingService_JoinQueue_AddsPlayer` → ✅
- [x] **TEST:** `MatchmakingService_JoinQueue_TwoPlayers_CreatesMatch` → ✅
- [x] **TEST:** `MatchmakingService_CancelQueue_RemovesPlayer` → ✅
- [x] **TEST:** `MatchmakingService_Timeout_30s_NotifiesPlayer` → ✅
- [x] **TEST:** `MatchmakingService_AlreadyInQueue_RejectsDuplicate` → ✅
- [x] **TEST:** `MatchmakingService_MatchPlayers_SimilarLevel_Preferred` → ✅
- [x] Vytvořit `IMatchmakingService` interface
- [x] Implementovat `MatchmakingService` s in-memory queue:
  - ConcurrentQueue pro čekající hráče
  - Matching algorithm: preference pro podobný level (±3)
  - Timeout: 30s → nabídnout AI soupeře nebo cancel
  - Background task pro continuous matching
- [x] **GREEN:** Všechny 9 testů prochází ✅

### T-500.5: MultiplayerGameService (TDD)
- [x] **TEST:** `MultiplayerGameService_CreateMatch_Initializes15Rounds` → ✅
- [x] **TEST:** `MultiplayerGameService_CreateMatch_3MinuteLimit` → ✅
- [x] **TEST:** `MultiplayerGameService_CreateMatch_WithCustomSettings_UsesSettings` → ✅
- [x] **TEST:** `MultiplayerGameService_SubmitAnswer_Correct_IncreasesScore` → ✅
- [x] **TEST:** `MultiplayerGameService_SubmitAnswer_Wrong_NoScoreChange` → ✅
- [x] **TEST:** `MultiplayerGameService_PlayerCompletes15Words_EndsMatch` → ✅
- [x] **TEST:** `MultiplayerGameService_DetermineWinner_ByCorrectCount` → ✅
- [x] **TEST:** `MultiplayerGameService_Forfeit_OpponentWins` → ✅
- [x] **TEST:** `MultiplayerGameService_Rewards_WinnerGetsBonus` → ✅
- [x] **TEST:** `MultiplayerGameService_Rewards_LoserGetsBase` → ✅
- [x] **TEST:** `MultiplayerGameService_IsMatchActive_ExistingMatch_ReturnsTrue` → ✅
- [x] **TEST:** `MultiplayerGameService_IsMatchActive_NonExistingMatch_ReturnsFalse` → ✅
- [x] **TEST:** `MultiplayerGameService_PlayerCompletes15Words_EndsMatch` → ✅
- [x] **TEST:** `MultiplayerGameService_DetermineWinner_Tie_BySpeed` → ✅
- [x] **TEST:** `MultiplayerGameService_TimerExpires_EndsMatch` → ✅
- [x] **TEST:** `MultiplayerGameService_Disconnect_30sGrace_ThenForfeit` → ✅
- [x] Vytvořit `IMultiplayerGameService` interface
- [x] Implementovat `MultiplayerGameService`:
  - Sdílená sada slov pro oba hráče (dle nastavení: 10/15/20)
  - Globální timer (dle nastavení: 2/3/5 min)
  - Real-time synchronizace skóre
  - Winner determination: correct count → total time
  - **Quick Match XP:** winner 100 XP + league 50 XP, loser 30 XP + league 15 XP
  - **Private Room XP:** winner 100 XP (0 liga XP), loser 30 XP (0 liga XP)
  - Best of X support: sledování skóre série (1:0, 1:1, 2:1 atd.)
  - Difficulty selection: výběr slov dle nastavené obtížnosti
- [x] **GREEN:** Všechny 15 testů prochází ✅

### T-500.6: MatchHub Implementation - ✅ HOTOVÉ
- [x] **TEST:** `MatchHub_JoinMatchmaking_AddsToQueue` → RED → GREEN
- [x] **TEST:** `MatchHub_CancelMatchmaking_RemovesFromQueue` → RED → GREEN
- [x] **TEST:** `MatchHub_Forfeit_UserNotInMatch_DoesNotCallForfeit` → RED → GREEN
- [x] Vytvořit `MatchHub : Hub<IMatchClient>` implementující `IMatchHub`
- [x] Implementovat connection management (group per match)
- [x] Implementovat JoinMatchmaking → trigger MatchmakingService
- [x] Implementovat SubmitAnswer → validate → broadcast to opponent
- [x] Implementovat disconnect handling s 30s grace period
- [x] `[Authorize]` na hub
- [x] **GREEN:** Testy prochází (3/3)

---

## T-501: UC-020 Multiplayer 1v1 - Frontend

### T-501.1: SignalR Client Setup - ✅ HOTOVÉ
- [x] Vytvořit `IMatchHubClient` service v Blazor/Services/
- [x] Implementovat `MatchHubClient`:
  - `HubConnection` builder s JWT token
  - Auto-reconnect s exponential backoff (0s, 1s, 3s, 5s, 10s)
  - Connection state management (Connecting, Connected, Disconnected, Reconnecting)
  - Event handlers pro všechny IMatchClient metody
- [x] Zaregistrovat v DI jako Scoped
- [x] Implementovat `IAsyncDisposable` pro cleanup

### T-501.2: Matchmaking Screen (Tempo.Blazor) - ✅ HOTOVÉ
- [x] **TEST (bUnit):** `MatchmakingScreen_Renders_SearchingState` → RED → GREEN
- [x] **TEST (bUnit):** `MatchmakingScreen_MatchFound_ShowsOpponent` → RED → GREEN
- [x] **TEST (bUnit):** `MatchmakingScreen_Timeout_ShowsOptions` → RED → GREEN
- [x] Vytvořit `Matchmaking.razor` (`@page "/multiplayer/quick-match"`)
- [x] `@inject IStringLocalizer<Multiplayer> L`
- [x] **Searching State**:
  - "⚔️ 1v1 SOUBOJ ⚔️" heading
  - "Hledání soupeře..." s `TmSpinner` (Lg)
  - Váš `TmAvatar` + animované "VS" + "?" `TmAvatar`
  - Timer: countdown "00:{seconds}" s `TmProgressBar`
  - `TmButton Variant="Ghost"` "Zrušit hledání"
  - Pravidla: `TmCard` s seznamem pravidel

- [x] **Match Found State**:
  - "⚔️ SOUPEŘ NALEZEN! ⚔️"
  - Oba `TmAvatar` (váš + opponent) s jmény a levely
  - Countdown: "Začínáme za: 3... 2... 1..." (velké číslo, pulse animace)
  - "Připrav se!" text

- [x] **Timeout State**:
  - "Soupeř nenalezen"
  - `TmButton` "Zkusit znovu" / `TmButton` "Hrát proti AI" / `TmButton` "Zpět"

- [x] **GREEN:** Testy prochází

### T-501.3: Real-time Game komponenta (Tempo.Blazor) - ✅ HOTOVÉ
- [x] **TEST (bUnit):** `RealtimeGame_Renders_BothPlayerCards` → RED → GREEN
- [x] **TEST (bUnit):** `RealtimeGame_SubmitAnswer_UpdatesScore` → RED → GREEN
- [x] **TEST (bUnit):** `RealtimeGame_OpponentAnswered_UpdatesOpponentCard` → RED → GREEN
- [x] Vytvořit `RealtimeGame.razor` komponentu
- [x] **Header**: `TmProgressBar` timer (3:00 odpočet), "Slovo {n}/15"
- [x] **Player Cards** (side-by-side):
  - Left (Vy): `TmCard` s `TmAvatar`, skóre "{x}/15 ✓", `TmProgressBar`, combo `TmBadge` "🔥 x{n}"
  - Right (Soupeř): `TmCard` s `TmAvatar`, skóre, `TmProgressBar`, combo
  - Opponent card: brief green flash při správné odpovědi
- [x] **Game Board** (center):
  - Scrambled word (velká písmena v boxech, jako v GameArena)
  - `TmTextInput` pro odpověď (uppercase, center-aligned)
  - `TmButton Variant="Primary"` "Odeslat"
- [x] **Feedback**:
  - Correct: zelený flash, skóre update, +XP float
  - Wrong: červený shake
- [x] Real-time updates přes SignalR (opponent progress, timer sync)
- [x] **GREEN:** Testy prochází

### T-501.4: Match Result Screen (Tempo.Blazor) ✅ HOTOVÉ
- [x] **TEST (bUnit):** `MatchResult_Victory_ShowsConfetti` → RED → GREEN
- [x] **TEST (bUnit):** `MatchResult_Defeat_ShowsMotivation` → RED → GREEN
- [x] **TEST (bUnit):** `MatchResult_Draw_ShowsSpeedWinner` → RED → GREEN
- [x] Vytvořit PlayerResultCard.razor komponentu (reusable player card)
- [x] Vytvořit MatchResult.razor komponentu (používá PlayerResultCard)

- [x] **Victory**: `TmModal` (Size: Large)
  - "🎉 VÍTĚZSTVÍ!" heading s confetti
  - Oba player cards s score/time
  - Winner badge na vaší kartě (🏆)
  - Quick Match rewards: "⭐ +100 XP", "📈 Liga: +50 XP"
  - Private Room rewards: "⭐ +100 XP" (bez liga řádku)
  - `TmButton Variant="Primary"` "Další zápas" + `TmButton Variant="Ghost"` "Domů"

- [x] **Defeat**: `TmModal`
  - "😔 PROHRA" heading
  - Opponent card s winner badge
  - Quick Match reward: "+30 XP" + liga XP
  - Private Room reward: "+30 XP" (bez liga)
  - Motivace: "💪 Příště to dáš!"
  - Quick Match: `TmButton` "Odveta" / `TmButton` "Domů"
  - Private Room: `TmButton` "Rematch" / `TmButton` "Domů"

- [x] **Draw**: `TmModal`
  - "🤝 REMÍZA!" heading
  - Oba cards se stejným skóre
  - Speed tiebreaker: "Rychlejší vyhrává!" s časy
  - Rewards
  - `TmButton` "Další zápas" / `TmButton` "Domů"

- [x] **GREEN:** Testy procházejí (12/12 - MatchResult + PlayerResultCard)

### T-501.5: Match History ✅ HOTOVÉ
- [x] Vytvořit `GET /api/v1/multiplayer/history` endpoint (s filtrací: all/quick-match/private-room)
- [x] Vytvořit `GET /api/v1/multiplayer/stats` endpoint (wins, losses, win rate - celkové i per type)
- [x] Vytvořit `MatchHistory.razor` komponentu (`@page "/multiplayer/history"`):
  - Stats header: `TmStatCard` × 4 (Played, Wins, Losses, Win Rate %)
  - Filter: `TmTabs` (Vše / Quick Match / Private Room)
  - History list: time-grouped (Today, Yesterday, This Week)
  - Each entry: opponent `TmAvatar` + username, score ("12:9"), result `TmBadge` (Win/Loss/Draw), XP, time, type `TmBadge` ("⚔️ Quick" / "🏠 Private")
  - Private Room entries: série skóre pokud Best of > 1 (např. "Série: 2:1")
- [x] **GREEN:** Testy prochází (8 testů)

---

## T-503: Private Rooms - Backend

### T-503.1: Room Entity a Code Generation (TDD) ✅ HOTOVÉ
- [x] **TEST:** `Room_Create_GeneratesUniqueCode` → RED → GREEN
- [x] **TEST:** `Room_Create_CodeFormat_LEXIQ_4AlphaNum` (formát "LEXIQ-XXXX") → RED → GREEN
- [x] **TEST:** `Room_Create_SetsExpiresAt_5MinFromNow` → RED → GREEN
- [x] **TEST:** `Room_IsExpired_ReturnsTrueAfterExpiry` → RED → GREEN
- [x] **TEST:** `Room_IsExpired_ReturnsFalseBeforeExpiry` → RED → GREEN
- [x] **TEST:** `Room_JoinRoom_AddsPlayer2` → GREEN
- [x] **TEST:** `Room_SetReady_Player1_SetsFlag` → GREEN
- [x] **TEST:** `Room_BothReady_WhenBothSetReady_ReturnsTrue` → GREEN
- [x] **TEST:** `Room_StartGame_WhenBothReady_ChangesStatusToPlaying` → GREEN
- [x] **TEST:** `Room_IsSeriesComplete_BestOf3_Player1Wins2_ReturnsTrue` → GREEN
- [x] **TEST:** `Room_IsSeriesComplete_BestOf5_Player2Wins3_ReturnsTrue` → GREEN
- [x] Vytvořit `Room` entitu v Core/Domain/Entities/:
  - Id (Guid), Code (string, unique), CreatedByUserId, Settings (RoomSettings owned type)
  - Status (RoomStatus enum), ExpiresAt, CreatedAt
  - Player1ConnectionId, Player2ConnectionId
  - Player1Ready, Player2Ready
  - CurrentMatchId (Guid?), GamesPlayed (int), Player1Wins (int), Player2Wins (int)
- [x] Vytvořit `RoomStatus` enum (WaitingForOpponent, Lobby, Countdown, Playing, BetweenGames, Completed, Expired, Cancelled)
- [x] Vytvořit `RoomSettings` value object (WordCount: int, TimeLimitMinutes: int, Difficulty: DifficultyLevel, BestOf: int)
- [x] Implementovat code generation: `GenerateRoomCode()` → "LEXIQ-" + 4 random uppercase alphanumeric
- [x] **GREEN:** 25 testů prochází ✅

### T-503.2: RoomSettingsValidator (TDD) ✅ HOTOVÉ
- [x] **TEST:** `RoomSettingsValidator_WordCount10_Valid` → RED → GREEN
- [x] **TEST:** `RoomSettingsValidator_WordCount15_Valid` → RED → GREEN
- [x] **TEST:** `RoomSettingsValidator_WordCount20_Valid` → RED → GREEN
- [x] **TEST:** `RoomSettingsValidator_WordCount7_Invalid` → RED → GREEN
- [x] **TEST:** `RoomSettingsValidator_TimeLimit2_Valid` → RED → GREEN
- [x] **TEST:** `RoomSettingsValidator_TimeLimit3_Valid` → RED → GREEN
- [x] **TEST:** `RoomSettingsValidator_TimeLimit5_Valid` → RED → GREEN
- [x] **TEST:** `RoomSettingsValidator_TimeLimit10_Invalid` → RED → GREEN
- [x] **TEST:** `RoomSettingsValidator_BestOf1_Valid` → RED → GREEN
- [x] **TEST:** `RoomSettingsValidator_BestOf3_Valid` → RED → GREEN
- [x] **TEST:** `RoomSettingsValidator_BestOf5_Valid` → RED → GREEN
- [x] **TEST:** `RoomSettingsValidator_BestOf4_Invalid` → RED → GREEN
- [x] **TEST:** `RoomSettingsValidator_DifficultyBeginner_Valid` → RED → GREEN
- [x] Vytvořit `RoomSettingsValidator : AbstractValidator<RoomSettingsDto>`
- [x] Povolené WordCount: [10, 15, 20]
- [x] Povolené TimeLimitMinutes: [2, 3, 5]
- [x] Povolené BestOf: [1, 3, 5]
- [x] Povolené Difficulty: libovolný DifficultyLevel
- [x] **GREEN:** 25 testů prochází ✅

### T-503.3: RoomService (TDD) ✅ HOTOVÉ
- [x] **TEST:** `RoomService_CreateRoom_ReturnsRoomWithCode` → RED → GREEN
- [x] **TEST:** `RoomService_CreateRoom_UserAlreadyHasActiveRoom_ReturnsError` → RED → GREEN
- [x] **TEST:** `RoomService_CreateRoom_SetsExpiresAt5Min` → RED → GREEN
- [x] **TEST:** `RoomService_JoinRoom_ValidCode_AddsPlayer2` → RED → GREEN
- [x] **TEST:** `RoomService_JoinRoom_InvalidCode_ReturnsError` → RED → GREEN
- [x] **TEST:** `RoomService_JoinRoom_ExpiredCode_ReturnsError` → RED → GREEN
- [x] **TEST:** `RoomService_JoinRoom_RoomFull_ReturnsError` → RED → GREEN
- [x] **TEST:** `RoomService_JoinRoom_OwnRoom_ReturnsRoom` → RED → GREEN
- [x] **TEST:** `RoomService_JoinRoom_UserAlreadyInAnotherRoom_ReturnsError` → RED → GREEN
- [x] **TEST:** `RoomService_LeaveRoom_Host_CancelsRoom` → RED → GREEN
- [x] **TEST:** `RoomService_LeaveRoom_Guest_RemovesFromRoom` → RED → GREEN
- [x] **TEST:** `RoomService_SetReady_Player1_SetsFlag` → RED → GREEN
- [x] **TEST:** `RoomService_SetReady_BothReady_StartsCountdown` → RED → GREEN
- [x] **TEST:** `RoomService_RequestRematch_StartsNewGame` → RED → GREEN
- [x] **TEST:** `RoomService_BestOf3_Player1Wins2_SeriesComplete` → RED → GREEN
- [x] Vytvořit `IRoomService` interface
- [x] Implementovat `RoomService`:
  - In-memory ConcurrentDictionary<string, Room>
  - Max 1 aktivní místnost per hráč
  - Kód platí 5 minut od vytvoření
  - Best of X: automatický start další hry
  - Rematch: nová série se stejnými settings
- [x] **GREEN:** 23 testů prochází ✅

### T-503.4: Room Cleanup Background Job (TDD) ✅ HOTOVÉ
- [x] **TEST:** `RoomCleanupJob_RemovesExpiredRooms` → RED → GREEN
- [x] **TEST:** `RoomCleanupJob_KeepsActiveRooms` → RED → GREEN
- [x] **TEST:** `RoomCleanupJob_RemovesCancelledRooms` → RED → GREEN
- [x] **TEST:** `RoomCleanupJob_RemovesCompletedRooms` → RED → GREEN
- [x] **TEST:** `RoomCleanupJob_RemovesOldCompletedRooms` → RED → GREEN
- [x] **TEST:** `RoomCleanupJob_KeepsRecentlyCompletedRooms` → RED → GREEN
- [x] Vytvořit `RoomCleanupJob` (Hangfire RecurringJob)
- [x] Odstraní rooms se statusem Expired, Cancelled, Completed (starší než 10 min)
- [x] Expiruje rooms ve stavu WaitingForOpponent kde ExpiresAt < now
- [x] **GREEN:** 6 testů prochází ✅

### T-503.5: Lobby Chat (TDD) ✅ HOTOVÉ
- [x] **TEST:** `LobbyChat_SendMessage_UserInRoom_SendsMessage` → RED → GREEN
- [x] **TEST:** `LobbyChat_SendMessage_UserNotInRoom_ReturnsError` → RED → GREEN
- [x] **TEST:** `LobbyChat_MessageTooLong_ReturnsError` (max 200 znaků) → RED → GREEN
- [x] **TEST:** `LobbyChat_EmptyMessage_ReturnsError` → RED → GREEN
- [x] **TEST:** `LobbyChat_RateLimit_ReturnsError` (5 zpráv/10s) → RED → GREEN
- [x] **TEST:** `LobbyChat_GetChatHistory_ReturnsMessagesInOrder` → RED → GREEN
- [x] **TEST:** `LobbyChat_GetChatHistory_LimitsMessages` (max 100) → RED → GREEN
- [x] **TEST:** `LobbyChat_ClearChat_DeletesAllMessages` → RED → GREEN
- [x] Vytvořit `ILobbyChatService` interface
- [x] Implementovat `LobbyChatService`:
  - Rate limiting: 5 zpráv za 10 sekund
  - Max délka zprávy: 200 znaků
  - Historie: posledních 100 zpráv
  - Thread-safe pomocí ConcurrentDictionary
- [x] **GREEN:** 11 testů prochází ✅

### T-503.6: MatchHub rozšíření pro Private Rooms ✅ HOTOVÉ
- [x] Rozšířit `MatchHub` o room metody:
  - `CreateRoom` → vytvořit room, přidat hráče do SignalR group `room:{code}`
  - `JoinRoom` → přidat do group, notifikovat hostitele
  - `LeaveRoom` → odebrat z group, notifikovat soupeře
  - `SetReady(bool isReady)` → check oba ready → countdown → start match
  - `RequestRematch` → požadavek na rematch
  - `AcceptRematch` → přijetí rematch, reset room stavu
  - `SendLobbyMessage` → broadcast do room group
- [x] Přidat `IRoomService` a `ILobbyChatService` do konstruktoru
- [x] Aktualizovat `OnDisconnectedAsync` pro správu odpojení z místnosti
- [x] Aktualizovat `IMatchClient` o nové metody:
  - `RoomCreated`, `RoomCreationFailed`, `RoomJoined`, `RoomJoinFailed`
  - `PlayerReadyStateChanged`, `RoomStateReset`
- [x] Aktualizovat `IMatchHub` o parametry pro `SetReady` a `AcceptRematch`
- [x] Room-specific SignalR group: `room:{roomCode}`
- [x] Match-specific group: `match:{matchId}` (reuse z Quick Match)
- [x] **GREEN:** Build prochází ✅

### T-503.7: Resources pro Private Rooms ✅ HOTOVÉ
- [x] Vytvořit `Multiplayer.resx` v `LexiQuest.Blazor/Resources/Pages/` s klíči:
  - Page.Title, QuickMatch.Title, QuickMatch.Description, QuickMatch.Button, QuickMatch.LeagueXP
  - PrivateRoom.Title, PrivateRoom.Description, PrivateRoom.CreateButton, PrivateRoom.JoinButton, PrivateRoom.NoLeagueXP
  - MatchHistory.Link
  - Room.Create.Title, Room.Settings.Title
  - Room.Settings.WordCount, Room.Settings.TimeLimit, Room.Settings.Difficulty, Room.Settings.BestOf
  - Room.Code.Label, Room.Code.CopySuccess, Room.Code.ShareText
  - Room.Lobby.Title, Room.Lobby.WaitingForOpponent, Room.Lobby.OpponentJoined
  - Room.Lobby.Ready, Room.Lobby.NotReady, Room.Lobby.BothReady
  - Room.Lobby.ReadyButton, Room.Lobby.CancelReadyButton
  - Room.Lobby.Chat.Placeholder, Room.Lobby.Chat.Send
  - Room.Expired, Room.Full, Room.NotFound, Room.InvalidCode
  - Room.Series.Score, Room.Series.GameOf
  - Room.Rematch.Request, Room.Rematch.Accept, Room.Rematch.Decline
  - Room.Leave.Confirm, Room.NoLeagueXP.Info
  - Matchmaking.Searching, Matchmaking.Cancel, Matchmaking.MatchFound, Matchmaking.StartingIn, Matchmaking.Timeout, Matchmaking.Retry
  - Validation.RoomCode.Required, Validation.RoomCode.InvalidFormat
- [x] **GREEN:** Resource soubor vytvořen ✅

---

## T-504: Private Rooms - Frontend

### T-504.1: Multiplayer Landing Page (Tempo.Blazor) ✅ HOTOVÉ
- [x] **TEST (bUnit):** `MultiplayerLanding_Renders_QuickMatchAndPrivateRoom` → RED → GREEN
- [x] **TEST (bUnit):** `MultiplayerLanding_ClickQuickMatch_NavigatesToMatchmaking` → RED → GREEN
- [x] **TEST (bUnit):** `MultiplayerLanding_ClickCreateRoom_ShowsSettingsModal` → RED → GREEN
- [x] **TEST (bUnit):** `MultiplayerLanding_ClickJoinRoom_ShowsCodeInput` → RED → GREEN
- [x] Přepracovat `Multiplayer.razor` (`@page "/multiplayer"`) na landing s výběrem režimu:
- [x] `@inject IStringLocalizer<Multiplayer> L`
- [x] Layout: 2 velké `TmCard` (Elevated) vedle sebe:

- [x] **Quick Match Card** (`TmCard`):
  - `TmIcon` (⚔️ swords) velký
  - Titulek: `@L["QuickMatch.Title"]`
  - Popis: "Náhodný soupeř, plné XP + liga body"
  - `TmBadge Variant="Success"` "Liga XP ✓"
  - Pravidla: 15 slov, 3 min, automatický matchmaking
  - `TmButton Variant="Primary" Size="Lg" Block="true"` → `@L["QuickMatch.Button"]`

- [x] **Private Room Card** (`TmCard`):
  - `TmIcon` (🏠 house/door) velký
  - Titulek: `@L["PrivateRoom.Title"]`
  - Popis: "Pozvi kamaráda, vlastní pravidla"
  - `TmBadge Variant="Warning"` "Bez liga XP"
  - Dvě akce:
    - `TmButton Variant="Primary" Size="Lg" Block="true"` → `@L["Room.Create.Button"]` (Vytvořit místnost)
    - `TmButton Variant="Outline" Size="Lg" Block="true"` → `@L["Room.Join.Button"]` (Připojit se)

- [x] **Match History link**: `TmButton Variant="Ghost"` "Historie zápasů" → `/multiplayer/history`
- [x] **GREEN:** Testy prochází (5 testů)
- [x] **REFACTOR:** Responsive (side-by-side desktop, stack mobile)

### T-504.2: Create Room Modal - Settings (Tempo.Blazor) ✅ HOTOVÉ
- [x] **TEST (bUnit):** `CreateRoomModal_Renders_AllSettings` → RED → GREEN
- [x] **TEST (bUnit):** `CreateRoomModal_InvalidSettings_ShowsValidation` → RED → GREEN
- [x] **TEST (bUnit):** `CreateRoomModal_Submit_CreatesRoomAndShowsCode` → RED → GREEN
- [x] Vytvořit `CreateRoomModal.razor` komponentu
- [x] `TmModal` (Size: Medium) s formulářem:

- [x] **Počet slov** (`TmFormField`):
  - `TmRadioGroup` s možnostmi: 10 / 15 (default) / 20
  - Label: `@L["Room.Settings.WordCount"]`

- [x] **Časový limit** (`TmFormField`):
  - `TmRadioGroup` s možnostmi: 2 min / 3 min (default) / 5 min
  - Label: `@L["Room.Settings.TimeLimit"]`

- [x] **Obtížnost** (`TmFormField`):
  - `TmSelect` s možnostmi: Beginner 🌱 / Intermediate 🌿 / Advanced 🌳 / Expert 🔥 / Mix (default)
  - Label: `@L["Room.Settings.Difficulty"]`

- [x] **Best of** (`TmFormField`):
  - `TmRadioGroup` s možnostmi: 1 hra (default) / Best of 3 / Best of 5
  - Label: `@L["Room.Settings.BestOf"]`

- [x] `<FluentValidationValidator />`
- [x] `TmAlert Severity="Info"` → `@L["Room.NoLeagueXP.Info"]` ("Private room zápasy nedávají liga XP")
- [x] Footer: `TmButton Variant="Primary"` "Vytvořit místnost" / `TmButton Variant="Ghost"` "Zrušit"
- [x] **GREEN:** Testy prochází (8 testů)

### T-504.3: Join Room Modal (Tempo.Blazor) ✅ HOTOVÉ
- [x] **TEST (bUnit):** `JoinRoomModal_Renders_CodeInput` → RED → GREEN
- [x] **TEST (bUnit):** `JoinRoomModal_InvalidCode_ShowsError` → RED → GREEN
- [x] **TEST (bUnit):** `JoinRoomModal_ValidCode_JoinsRoom` → RED → GREEN
- [x] **TEST (bUnit):** `JoinRoomModal_ExpiredCode_ShowsExpiredError` → RED → GREEN
- [x] Vytvořit `JoinRoomModal.razor` komponentu
- [x] `TmModal` (Size: Small):
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
- [x] **GREEN:** Testy prochází (10 testů)

### T-504.4: Room Lobby Screen (Tempo.Blazor) ✅ HOTOVÉ
- [x] **TEST (bUnit):** `RoomLobby_HostView_ShowsCodeAndWaiting` → RED → GREEN
- [x] **TEST (bUnit):** `RoomLobby_OpponentJoined_ShowsBothPlayers` → RED → GREEN
- [x] **TEST (bUnit):** `RoomLobby_BothReady_ShowsCountdown` → RED → GREEN
- [x] **TEST (bUnit):** `RoomLobby_Chat_SendsAndReceivesMessages` → RED → GREEN
- [x] **TEST (bUnit):** `RoomLobby_Expiry_ShowsExpiredMessage` → RED → GREEN
- [x] Vytvořit `RoomLobby.razor` komponentu

- [x] **Waiting for Opponent State** (host čeká):
  - Titulek: `@L["Room.Lobby.WaitingForOpponent"]`
  - Room Code velkým fontem (monospace, 32px): "LEXIQ-7K3M"
  - `TmCopyButton` pro zkopírování kódu do schránky → toast `@L["Room.Code.CopySuccess"]`
  - Share text: `@L["Room.Code.ShareText"]` ("Připoj se do mé místnosti: LEXIQ-7K3M")
  - Countdown do expirace: `TmProgressBar` s zbývajícím časem (5 min)
  - Host `TmAvatar` (vlevo) + `TmSpinner` + "?" placeholder (vpravo)
  - Settings summary: `TmCard` s přehledem nastavení (slov, čas, obtížnost, best of)
  - `TmButton Variant="Ghost"` "Zrušit místnost"

- [x] **Lobby State** (oba hráči přítomni):
  - Oba `TmAvatar` s username, level, streak `TmBadge`
  - Settings summary: `TmCard` s pravidly hry
  - Ready indikátory:
    - Per hráč: `TmBadge Variant="Success"` "Připraven ✓" / `TmBadge Variant="Warning"` "Čeká..."
  - `TmButton Variant="Primary" Size="Lg"` "Jsem připraven!" (toggle → po kliknutí "Čekám na soupeře...")
  - Oba ready → automatický countdown 3-2-1 (pulse animace)

- [x] **Chat v lobby**:
  - Chatovací okno (scroll, max výška 200px)
  - Zprávy: `TmAvatar` (mini) + username + text + čas
  - Input: `TmTextInput` + `TmButton Variant="Primary" Size="Sm"` "Odeslat"
  - Max 200 znaků, Enter pro odeslání

- [x] **GREEN:** Testy prochází (6 testů)
- [x] **REFACTOR:** Responsive layout, animace

### T-504.5: Series Score Overlay (Best of X) ✅ HOTOVÉ
- [x] **TEST (bUnit):** `SeriesScore_BestOf3_ShowsScoreAndGameNumber` → RED → GREEN
- [x] **TEST (bUnit):** `SeriesScore_SeriesComplete_ShowsFinalResult` → RED → GREEN
- [x] Vytvořit `SeriesScore.razor` komponentu
- [x] Zobrazení během hry (v header area):
  - `TmBadge` "Hra {current} z {bestOf}" (např. "Hra 2 z 3")
  - Série skóre: "Série: {player1Wins} : {player2Wins}" s `TmAvatar` mini po stranách
  - Indikátor kdo vede (bold/highlight na vedoucím hráči)
- [x] Mezi hrami (BetweenGames state):
  - `TmModal` s výsledkem aktuální hry
  - Série skóre velké ("1 : 1")
  - Pokud série neskončila: "Další hra začíná za..." countdown (5s)
  - Pokud série skončila: finální výsledky + rematch option
- [x] **GREEN:** Testy prochází (6 testů)

### T-504.6: Match Result rozšíření pro Private Rooms ✅ HOTOVÉ (částečně)
- [x] **TEST (bUnit):** `MatchResult_PrivateRoom_ShowsNoLeagueXP` → RED → GREEN
- [x] **TEST (bUnit):** `MatchResult_PrivateRoom_ShowsRematchButton` → RED → GREEN
- [x] **TEST (bUnit):** `MatchResult_PrivateRoom_BestOf_ShowsSeriesScore` → RED → GREEN
- [x] Rozšířit `MatchResult.razor`:
  - Private Room: nezobrazovat "📈 Liga: +50 XP" řádek
  - Private Room: `TmAlert Severity="Info"` "Private room zápasy nedávají liga body"
  - Rematch tlačítko: `TmButton Variant="Primary"` "Rematch" (nová hra/série, stejné settings, stejná místnost)
  - Pokud soupeř requestoval rematch: `TmAlert` "Soupeř chce rematch!" + `TmButton` Accept/Decline
  - Best of série: zobrazit finální série skóre ("Série: 2:1 - Výhra!")
- [x] **GREEN:** Testy prochází (12 testů - MatchResult)

### T-504.7: Validátor pro JoinRoom (Frontend) ✅ HOTOVÉ
- [x] **TEST (bUnit):** `JoinRoomValidator_EmptyCode_ShowsError` → RED → GREEN
- [x] **TEST (bUnit):** `JoinRoomValidator_WrongFormat_ShowsError` → RED → GREEN
- [x] **TEST (bUnit):** `JoinRoomValidator_ValidCode_NoErrors` → RED → GREEN
- [x] Vytvořit `JoinRoomModel` (Code: string)
- [x] Vytvořit `JoinRoomModelValidator : AbstractValidator<JoinRoomModel>`:
  - NotEmpty
  - Matches regex `^LEXIQ-[A-Z0-9]{4}$`
  - Lokalizované zprávy z .resx
- [x] **GREEN:** Testy prochází (6 testů)

---

## T-502: UC-021 Týmy a Klany

### T-502.1: Domain Entities (TDD) ✅ HOTOVÉ
- [x] **TEST:** `Team_Create_SetsDefaults` → RED → GREEN
- [x] **TEST:** `Team_AddMember_IncreasesMemberCount` → RED → GREEN
- [x] **TEST:** `Team_AddMember_Max20_Throws` → RED → GREEN
- [x] **TEST:** `TeamMember_Create_DefaultRoleMember` → RED → GREEN
- [x] **TEST:** `Team_Name_3to30Chars` → RED → GREEN
- [x] **TEST:** `Team_Tag_2to4Chars` → RED → GREEN
- [x] Vytvořit `Team` entitu (Id, Name, Tag, Description, LogoUrl, LeaderId, CreatedAt, Members)
- [x] Vytvořit `TeamMember` entitu (UserId, TeamId, Role, JoinedAt)
- [x] Vytvořit `TeamRole` enum (Leader, Officer, Member)
- [x] Vytvořit `TeamStats` value object (WeeklyXP, AllTimeXP, Rank, TotalWins)
- [x] Vytvořit `TeamInvite` entitu (TeamId, InvitedUserId, InvitedByUserId, Status, ExpiresAt)
- [x] Vytvořit `TeamJoinRequest` entitu (TeamId, UserId, Message, Status, CreatedAt)
- [x] EF Core konfigurace + migrace
- [x] **GREEN:** Testy prochází (39 testů)

### T-502.2: TeamService (TDD) ✅ HOTOVÉ
- [x] **TEST:** `TeamService_Create_PremiumOrCoins_Success` → RED → GREEN
- [x] **TEST:** `TeamService_Create_FreeNoCcoins_Returns403` → RED → GREEN
- [x] **TEST:** `TeamService_Create_DuplicateName_Returns409` → RED → GREEN
- [x] **TEST:** `TeamService_Create_DuplicateTag_Returns409` → RED → GREEN
- [x] **TEST:** `TeamService_InviteMember_Officer_Success` → RED → GREEN
- [x] **TEST:** `TeamService_InviteMember_RegularMember_Returns403` → RED → GREEN
- [x] **TEST:** `TeamService_KickMember_Officer_Success` → RED → GREEN
- [x] **TEST:** `TeamService_KickMember_CantKickLeader` → RED → GREEN
- [x] **TEST:** `TeamService_Leave_LastMember_DisbandsTeam` → RED → GREEN
- [x] **TEST:** `TeamService_TransferLeadership_Success` → RED → GREEN
- [x] **TEST:** `TeamService_GetWeeklyRanking_ReturnsSortedByXP` → RED → GREEN
- [x] **TEST:** `TeamService_RequestJoin_CreatesRequest` → RED → GREEN
- [x] **TEST:** `TeamService_ApproveJoin_AddsMember` → RED → GREEN
- [x] Vytvořit `ITeamService` interface
- [x] Vytvořit DTOs: `TeamDto`, `TeamMemberDto`, `CreateTeamRequest`, `InviteMemberRequest`, `TeamStatsDto`, `TeamRankingDto`
- [x] Vytvořit validátory: `CreateTeamValidator` s lokalizací
- [x] Implementovat `TeamService`
- [x] **GREEN:** Build prochází

### T-502.3: Team Endpoints ✅ HOTOVÉ
- [x] Vytvořit `POST /api/v1/teams` (vytvoří tým)
- [x] Vytvořit `GET /api/v1/teams/{id}` (detail týmu)
- [x] Vytvořit `PUT /api/v1/teams/{id}` (upraví tým - officer+)
- [x] Vytvořit `DELETE /api/v1/teams/{id}` (disbanduje tým - leader only)
- [x] Vytvořit `POST /api/v1/teams/{id}/invite` (pozve člena)
- [x] Vytvořit `POST /api/v1/teams/{id}/kick/{userId}` (vyhodí člena)
- [x] Vytvořit `POST /api/v1/teams/{id}/leave` (opustí tým)
- [x] Vytvořit `POST /api/v1/teams/{id}/transfer-leadership` (předá vedení)
- [x] Vytvořit `POST /api/v1/teams/{id}/join-request` (žádost o vstup)
- [x] Vytvořit `POST /api/v1/teams/join-requests/{id}/approve` (schválí žádost)
- [x] Vytvořit `POST /api/v1/teams/join-requests/{id}/reject` (zamítne žádost)
- [x] Vytvořit `GET /api/v1/teams/ranking` (týmový žebříček)
- [x] Vytvořit `GET /api/v1/users/me/team` (můj tým)
- [x] Vytvořit `GET /api/v1/teams/invites/my` (moje pozvánky)
- [x] Vytvořit `GET /api/v1/teams/{id}/join-requests` (žádosti o vstup)
- [x] Vytvořit `GET /api/v1/teams/can-create` (kontrola možnosti vytvořit tým)

### T-502.4: Frontend - Team UI (Tempo.Blazor) ✅ HOTOVÉ
- [x] **TEST (bUnit):** `TeamPage_NoTeam_ShowsCreateOrJoin` → RED → GREEN
- [x] **TEST (bUnit):** `TeamPage_HasTeam_ShowsDashboard` → RED → GREEN
- [x] **TEST (bUnit):** `TeamPage_Leader_ShowsManagementOptions` → RED → GREEN
- [x] Vytvořit `Team.razor` (`@page "/team"`)
- [x] `@inject IStringLocalizer<Team> L`

- [x] **No Team State**:
  - `TmEmptyState` "Nemáš tým"
  - `TmButton Variant="Primary"` "Vytvořit tým" (Premium/1000 coins)
  - `TmButton Variant="Outline"` "Hledat tým"
  - Browse teams: search + list

- [x] **Create Team Modal**: `TmModal` s:
  - `TmFormField` + `TmTextInput` Název (3-30 znaků)
  - `TmFormField` + `TmTextInput` Tag (2-4 znaky)
  - `TmFormField` + `TmTextArea` Popis
  - Logo upload (optional)
  - `<FluentValidationValidator />`
  - `TmButton` "Vytvořit" (zobrazí cost: Premium free / 1000 coins)

- [x] **Team Dashboard**:
  - Header: Logo, Team name, Tag `TmBadge`, member count
  - **Members list**: tabulka s columns: `TmAvatar`, Username, Role `TmBadge`, Weekly XP, Joined
  - **Team Stats**: `TmCard` × 4 (Weekly XP, All-time XP, Rank, Wins)
  - **Weekly Ranking**: pozice týmu v žebříčku
  - **Management** (Leader/Officer):
    - `TmButton` "Pozvat" → invite modal
    - `TmButton Variant="Danger"` "Vyhodit" na member rows
    - `TmButton` "Předat vedení"
    - Join requests: `TmCard` list s Accept/Reject buttons

- [x] **GREEN:** Testy prochází
- [x] **REFACTOR:** Styling dle UI-UX-014 principles, scoped CSS, responsive

---

## Ověření dokončení fáze

### Quick Match ✅
- [x] SignalR connection funguje s JWT auth
- [x] Matchmaking: join queue → match found → countdown → game start
- [x] 1v1 gameplay: shared words, real-time score sync, timer
- [x] Match result: victory/defeat/draw s XP rewards (osobní XP + liga XP)
- [x] Disconnect handling: 30s grace period, forfeit

### Private Rooms ✅
- [x] Create room: nastavení pravidel → kód vygenerován → lobby
- [x] Room code: formát LEXIQ-XXXX, expirace 5 min, copy/share funkce
- [x] Join room: zadat kód → připojení do lobby → oba hráči viditelní
- [x] Lobby: avatary, levely, streaky, ready tlačítka, chat
- [x] Ready flow: oba kliknou ready → countdown 3-2-1 → hra začne
- [x] Custom pravidla: počet slov (10/15/20), čas (2/3/5 min), obtížnost, Best of (1/3/5)
- [x] Best of X: skóre série, automatický start další hry, finální výsledek
- [x] XP: osobní XP ano, liga XP **NE** (prevence farmení)
- [x] Rematch: po hře/sérii nabídnout rematch ve stejné místnosti
- [x] Max 1 aktivní místnost per hráč
- [x] Vyžaduje přihlášení (ne guest)
- [x] Dostupné pro všechny (ne jen premium)
- [x] Room expiry: automatické čištění expirovaných/dokončených rooms
- [x] Lobby chat: max 200 znaků, rate limit, sanitizace

### Shared ✅
- [x] Match history se stats (Quick Match + Private Room odlišené)
- [x] Multiplayer landing page: výběr Quick Match / Private Room

### Teams ✅ HOTOVÉ (Backend)
- [x] Teams: create (premium/coins), invite, kick, leave, transfer leadership
- [x] Team dashboard: members, stats, weekly ranking
- [x] Join requests: request → approve/reject
- [x] Domain Entities: Team, TeamMember, TeamRole, TeamStats, TeamInvite, TeamJoinRequest
- [x] TeamService s plnou funkcionalitou
- [x] REST API endpoints
- [x] Frontend UI: Team.razor (T-502.4) ✅

### Kvalita ✅
- [x] Všechny texty z .resx
- [x] FluentValidation na FE i BE
- [x] `dotnet test` → všechny testy zelené
