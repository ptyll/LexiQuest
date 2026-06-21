# Faze 9: Kompletní E2E Playwright a screenshot testy

> **Cil:** Otestovat vsechny funkcni oblasti popsane v `todo/*.md` pomoci realnych E2E Playwright testu a screenshot testu. Screenshoty maji dve role: vizualni/UX review a dukaz, ze se provedlo presne to, co scenar pozaduje.
> **Stav:** Implementace rozpracovana; hotova E2E infrastruktura, smtp4dev, screenshot artefakty a smoke/full scenare pro landing, auth, guest, game, email a responzivitu.
> **Testovaci DB:** SQL Server pres Testcontainers.
> **Testovani emailu:** `rnwood/smtp4dev` v Dockeru pres Testcontainers.
> **Primarni test projekt:** `tests/LexiQuest.E2E.Tests`.

---

## Zdrojove dokumenty pro rozsah

- [x] Precist kompletne `todo/TodoList.md`
- [x] Precist kompletne `todo/Faze-0-Setup.md`
- [x] Precist kompletne `todo/Faze-1-MVP-Core.md`
- [x] Precist kompletne `todo/Faze-2-MVP-Extended.md`
- [x] Precist kompletne `todo/Faze-2-MVP-Extended-Progress.md`
- [x] Precist kompletne `todo/Faze-3-Landing-Guest.md`
- [x] Precist kompletne `todo/Faze-4-Premium.md`
- [x] Precist kompletne `todo/Faze-5-Multiplayer.md`
- [x] Precist kompletne `todo/Faze-6-Advanced.md`
- [x] Precist kompletne `todo/Faze-7-Testing-Deployment.md`
- [x] Precist kompletne `todo/Faze-8-Bugfix-Review.md`
- [x] Overit soucasny stav `tests/LexiQuest.E2E.Tests`

---

## Zasady implementace

- [x] Kazdy novy E2E scenar napsat nejdriv jako failing test, az potom upravovat aplikaci nebo testovaci infrastrukturu.
- [x] Nepouzivat realnou sdilenou DB ani lokalni vyvojovou DB. Vsechny E2E testy musi pouzivat SQL Server Testcontainer.
- [x] Neposilat zadne realne emaily. Vsechny emailove scenare musi jit pres `rnwood/smtp4dev` container.
- [x] Nepouzivat wrapper tridy pro API odpovedi ani obchazet realne HTTP flow.
- [x] Texty a asserty v UI opirat o ceske texty z aplikace a stabilni `data-testid` selektory.
- [x] Pri kazdem nalezu zalozit zaznam v `todo/E2E-Nalezene-Chyby.md`.
- [x] Po oprave chyby zmenit stav bug zaznamu na `Opraveno` a po znovu-probehnuti testu na `Overeno`.
- [x] Screenshot baseline schvalit az po UX kontrole. Vadny screenshot se nesmi prijmout jako baseline.
- [x] Testy nesmi byt jen happy path. Kazda funkcni oblast musi mit uspesny, validacni, autorizacni, prazdny/loading/error a edge-case scenar.

---

## Definition of Done pro Fazi 9

- [x] E2E test projekt je soucasti `LexiQuest.slnx`.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj` spusti aplikaci, DB a smtp4dev automaticky.
- [x] Testy bezi lokalne bez rucniho spousteni API/Web projektu.
- [x] SQL Server bezi v Testcontaineru a DB se resetuje mezi testy nebo izolovanymi kolekcemi.
- [x] smtp4dev bezi v Testcontaineru a testy umi cist zachycene emaily.
- [x] Screenshot artefakty se ukladaji deterministicky a jsou navazane na test, viewport, tema a stav.
- [x] Existuje screenshot coverage pro desktop, tablet, mobil, light/dark theme a klicove error/empty/loading stavy.
- [x] Testy pokryvaji vsechny use-casy z fazi 0-8.
- [x] Vsechny nalezene chyby jsou v bug trackeru s reprodukci, screenshotem/testem a stavem opravy.
- [x] CI umi spustit smoke E2E a volitelne plnou E2E sadu.

### Prubezne overeni

- [x] `dotnet build src/LexiQuest.Api/LexiQuest.Api.csproj` probehl uspesne.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~EmailE2ETests"` probehl uspesne: 1/1.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj` probehl uspesne: 40/40.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GuestE2ETests.GuestConversion_RegisterFromCta_TransfersProgressToDashboard"` probehl uspesne: 1/1.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GuestE2ETests"` probehl uspesne: 7/7.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GuestE2ETests.GuestLimit_FifthGameAllowedAndSixthGameShowsRegistrationCta"` probehl uspesne: 1/1.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GuestE2ETests.GuestLimit_After24Hours_AllowsNewGameAgain"` probehl uspesne: 1/1.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GuestE2ETests.GuestProtectedFeatures_DashboardRedirectsToLoginWithoutAuthTokens"` probehl uspesne: 1/1.
- [x] `dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj --filter "FullyQualifiedName~GuestLimiterTests"` probehl uspesne: 6/6.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~LandingE2ETests.UnknownPublicRoute_ShowsLocalizedNotFoundPage"` probehl uspesne: 1/1.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~LandingE2ETests"` probehl uspesne: 9/9.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PwaE2ETests"` probehl uspesne: 4/4.
- [x] `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~GameServiceTests|FullyQualifiedName~GamePageTests"` probehl uspesne: 24/24.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GameFlowE2ETests"` probehl uspesne: 22/22.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Game_TimeAttackDifficulty_StartsWithExpectedLives"` probehl uspesne: 4/4.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Game_LowLives_ShowsWarningAndRegenTimer"` probehl uspesne: 1/1.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Game_MaxLives_DoesNotShowRegenOrOverflow"` probehl uspesne: 1/1.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Game_XpSpeedBonusThresholds_AreApplied|FullyQualifiedName~Game_XpComboAndStreakBonuses_AreAppliedAcrossSession|FullyQualifiedName~Game_CorrectAnswer_UpdatesUserStatsXpAndDashboardValues"` probehl uspesne: 6/6.
- [x] `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~LevelUpModalTests|FullyQualifiedName~GamePageTests|FullyQualifiedName~GameArenaTests|FullyQualifiedName~GameServiceTests"` probehl uspesne: 45/45.
- [x] `dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj --filter "FullyQualifiedName~XpServiceTests|FullyQualifiedName~LevelCalculatorTests"` probehl uspesne: 33/33.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Game_LevelUp_ShowsModalWhenXpCrossesThreshold"` probehl uspesne: 1/1; screenshot `artifacts/e2e/screenshots/game/level-up-modal/1366x900/light/visible.png` zkontrolovan.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Game_MultiLevelUpFromSingleXpGain_ShowsFinalLevelAndAllUnlocks"` probehl RED na chybejicim E2E XP hooku a po oprave GREEN: 1/1; screenshot `artifacts/e2e/screenshots/game/multi-level-up-single-xp-gain/1366x900/light/visible.png` zkontrolovan. `dotnet build src/LexiQuest.Api/LexiQuest.Api.csproj --no-restore -m:1 /p:BuildInParallel=false /v:q` a `dotnet build tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false /v:q` prosly.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~E2EPersonaSet_CreatesNamedUsersRolesAndGuestProfile"` probehl uspesne: 1/1; overuje E2E persony `freeUser`, `premiumUser`, `lockedOutUser`, `adminUser`, `contentManagerUser`, tymove role, multiplayer dvojici, `noProgressUser`, `guestBrowserProfile`, dnesni daily challenge a aktivni ligy pro vsechny tiery.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Game_LevelUnlocks_ShowExpectedRewards"` probehl uspesne: 5/5; screenshoty level 3 a 15 zkontrolovany.
- [x] `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~XpBarTests"` probehl uspesne: 3/3.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Dashboard_XpBar_MatchesStatsApiProgress"` probehl uspesne: 1/1; screenshot `artifacts/e2e/screenshots/dashboard/xp-bar-api-progress/1366x900/light/loaded.png` zkontrolovan.
- [x] `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~PathsPageTests"` probehl uspesne: 5/5.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PathsE2ETests"` probehl uspesne: 2/2; screenshoty `paths/new-user-lock-state` a `paths/level-five-unlocks-intermediate` zkontrolovany.
- [x] `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~GameArenaTests|FullyQualifiedName~LivesIndicatorTests|FullyQualifiedName~GamePageTests|FullyQualifiedName~GameServiceTests"` probehl uspesne: 49/49.
- [x] `dotnet test tests/LexiQuest.Infrastructure.Tests/LexiQuest.Infrastructure.Tests.csproj --filter "FullyQualifiedName~GameSessionServiceTests"` probehl uspesne: 21/21.
- [x] `dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj --filter "FullyQualifiedName~PathServiceTests"` probehl uspesne: 8/8.
- [x] `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~PathMapTests|FullyQualifiedName~PathsPageTests|FullyQualifiedName~LevelDetailModalTests|FullyQualifiedName~GamePageTests|FullyQualifiedName~GameServiceTests"` probehl uspesne: 33/33.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Paths_BeginnerDetail_ShowsMapLevelModalAndStartsPathGame"` probehl RED na chybejicim detailu levelu a po oprave GREEN: 1/1; screenshoty `map`, `level-modal`, `path-game` zkontrolovany.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PathsE2ETests"` probehl uspesne: 3/3.
- [x] `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~GameServiceTests"` probehl uspesne: 9/9.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Paths_CompleteLevel_UpdatesProgressAndShowsPerfectState"` probehl RED na chybejicim perfect progressu a po oprave GREEN: 1/1.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Paths_SeededCompletedLevel_ShowsCompletedStateAndCurrentNextLevel"` probehl uspesne: 1/1.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PathsE2ETests"` probehl uspesne: 5/5; screenshoty `completed-progress` a `perfect-progress` zkontrolovany.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~StreakE2ETests"` probehl uspesne: 13/13.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Layout_DashboardSidebar_IsStyledAsNavigation"` probehl RED na neostylovanych sidebar odkazech a po oprave GREEN: 1/1; screenshot `artifacts/e2e/screenshots/layout/dashboard-sidebar-styled/1366x900/light/desktop.png` zkontrolovan.
- [x] `dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj --filter "FullyQualifiedName~UserServiceTests"` probehl uspesne: 12/12.
- [x] `dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj --filter "FullyQualifiedName~LeagueServiceEdgeCaseTests|FullyQualifiedName~LeagueServiceTests"` probehl uspesne: 32/32.
- [x] `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~LeaguesPageTests"` probehl uspesne: 7/7.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Leagues_NewUser_IsAssignedToBronzeLeague"` probehl RED na prazdne lize noveho uzivatele a po oprave GREEN: 1/1; screenshot `artifacts/e2e/screenshots/leagues/new-user-bronze/1366x900/light/loaded.png` zkontrolovan.
- [x] `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~DailyChallengePageTests"` probehl uspesne: 5/5.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~DailyChallengeE2ETests"` probehl RED na neviditelnem prazdnem leaderboardu a po opravach GREEN: 3/3; screenshoty `daily/today-start`, `daily/completion-top10-second-attempt` a `daily/next-day-reset` zkontrolovany.
- [x] `dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj --filter "FullyQualifiedName~AchievementServiceTests"` probehl uspesne: 7/7.
- [x] `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~GamePageTests|FullyQualifiedName~GameServiceTests|FullyQualifiedName~AchievementsPageTests"` probehl uspesne: 31/31.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~AchievementsE2ETests"` probehl RED na auth claimu, prazdnem katalogu, chybnych UI selektorech a chybejicim unlock modalu; po opravach GREEN: 3/3.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~BossE2ETests"` probehl RED na chybejicim boss API endpointu, lokalizaci/UX a perfect bonusu; po opravach GREEN: 9/9.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~SettingsE2ETests"` probehl RED na chybejicich settings selektorech/avataru/danger zone a po opravach GREEN: 4/4.
- [x] `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~SettingsPageTests"` probehl uspesne: 10/10.
- [x] `dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj --filter "FullyQualifiedName~UserServiceTests"` probehl uspesne: 12/12.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PremiumE2ETests"` probehl RED na chybejicich premium selektorech/fake checkoutu/DI a po opravach GREEN: 7/7; screenshoty `premium/overview-free-locked-features`, `premium/fake-checkout-success-activates` a `premium/checkout-cancel-no-activation` zkontrolovany.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Premium_StripeWebhook"` probehl RED na chybejicim E2E fake webhook adapteru a po oprave GREEN: 5/5.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PremiumE2ETests"` probehl uspesne: 12/12.
- [x] `dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj --filter "FullyQualifiedName~InventoryServiceTests|FullyQualifiedName~InventoryServiceEdgeCaseTests|FullyQualifiedName~CoinServiceTests|FullyQualifiedName~CoinServiceEdgeCaseTests"` probehl uspesne: 55/55.
- [x] `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~ShopPageTests|FullyQualifiedName~ShopItemCardTests"` probehl uspesne: 16/16.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~ShopE2ETests"` probehl RED na chybejicim shop seedu/sluzbe/selektorech, spatnem resource pathu a EF mapovani `CoinTransaction`; po opravach GREEN: 4/4. Screenshoty `shop/overview-balance-categories-rarity` a `shop/purchase-owned-equip-insufficient-duplicate` zkontrolovany.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~CoinEarningE2ETests"` probehl RED na nulovem coin balance po level/boss/daily/achievement flow; po opravach GREEN: 4/4.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~DictionariesE2ETests"` probehl RED na chybejicim `/api/v1/dictionaries`, premium gate/selectorech, public/private flow, importech a custom dictionary game; po opravach GREEN: 4/4. Souvisejici core slovnikove testy prosly 55/55 a API slovnikove testy prosly 17/17. Screenshoty `dictionaries/free-user-premium-gate` a `dictionaries/premium-crud-import-public-delete` zkontrolovany.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~MultiplayerE2ETests"` probehl RED na chybejicim SignalR bool contractu, JWT query tokenu pro browser WebSocket, nestabilnich selektorech a tichém odebrani vzdalenych levelu z fronty; po opravach GREEN: 3/3. `dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj --filter "FullyQualifiedName~MatchmakingServiceTests"` probehl uspesne: 10/10. Screenshoty `multiplayer/landing-quickmatch-search-cancel` pro `landing` a `searching` zkontrolovany.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~QuickMatch_TwoBrowserPlayers_CountdownAndNavigateToRealtimeGame"` probehl RED pri screenshot review na literalnich `_username`/`_opponentUsername` a chybejicim match-found layoutu; po oprave GREEN: 1/1. Blazor matchmaking/realtime subset probehl 6/6. Screenshoty `quickmatch-countdown-realtime` pro `match-found-countdown` a `realtime-game` zkontrolovany.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~QuickMatch_RealtimeCorrectAnswer_UpdatesOwnScoreAndOpponentProgress"` probehl RED na defaultnim `TEST` placeholderu, zdisposovanem hub klientu pri navigaci a chybejicich player/opponent progress eventech; po opravach GREEN: 1/1. Core multiplayer service testy prosly 39/39 a Blazor realtime/matchmaking subset 6/6. Screenshoty `quickmatch-realtime-score-progress` pro Alici i Boba zkontrolovany.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~MultiplayerE2ETests"` probehl po Quick Match lifecycle/progress opravach uspesne: 5/5.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~QuickMatch_Forfeit_ShowsPerspectiveResultsAwardsXpAndSavesHistory"` probehl RED na chybejici result route, neulozenem vysledku a chybejicich XP/history; po opravach GREEN: 1/1. `MatchHubTests|MatchHistoryEndpointsTests` prosly 11/11 a `MatchHistoryPageTests|MatchResultTests` prosly 19/19. Screenshoty `quickmatch-forfeit-result-history` pro result a history byly zkontrolovany; navazne lokalizacni nalezy v history/sidebaru jsou overene.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~MultiplayerE2ETests"` probehl po result/history a lokalizacnich opravach uspesne: 6/6.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~QuickMatch_SinglePlayer_TimesOutAfterThirtySecondsAndShowsOptions"` probehl RED na chybějicim timeout eventu z hubu; po oprave GREEN: 1/1. `MatchHubTests` prosly 3/3 a `MatchmakingPageTests` prosly 3/3. Screenshot `quickmatch-single-player-timeout` byl zkontrolovan.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~QuickMatch_WinnerByCorrectCount_CompletesMatchAndShowsVictory"` probehl uspesne: 1/1. Screenshot `quickmatch-winner-by-correct-count` potvrzuje 15:0, victory stav a XP/liga XP.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~QuickMatch_TieBySpeed_FasterPlayerWinsAndResultPageShowsTiebreaker"` probehl uspesne: 1/1. Screenshot `quickmatch-tie-by-speed` potvrzuje 1:1 a vyhru rychlejsiho hrace.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~QuickMatch_Draw_WhenCorrectCountAndTimeAreEqual_ShowsDrawResult"` probehl uspesne: 1/1. Screenshot `quickmatch-draw-result` potvrzuje remizu bez matoucího speed-tiebreaker textu.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~QuickMatch_TimerExpiry_CompletesMatchAsDrawAndShowsResult"` probehl RED na chybejicim E2E timer endpointu a countdownem spotrebovanem match casu; po oprave GREEN: 1/1. Screenshot `quickmatch-timer-expiry` potvrzuje expired remizu, XP a league XP.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~QuickMatch_DisconnectGrace_ForfeitsAfterThirtySecondsAndAwardsOpponent"` probehl RED na chybejicim disconnect finalizeru; po oprave GREEN: 1/1. Screenshot `quickmatch-disconnect-grace` potvrzuje victory po 30s grace a odmeny.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~QuickMatch_ReconnectWithinGrace_RestoresMatchAndPreventsForfeit"` probehl uspesne: 1/1. Screenshot `quickmatch-reconnect-within-grace` potvrzuje obnovenou realtime hru a po UX oprave i neslepeny header.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_BothReady_StartsCountdownForBothPlayers"` probehl RED na chybejicim countdown selectoru a po oprave GREEN: 1/1. Navazny E2E subset `PrivateRoom_CreateModal_SelectsSettingsShowsRoomCodeAndCopiesIt|PrivateRoom_InvalidSettings_AreRejectedByHub|PrivateRoom_BothReady_StartsCountdownForBothPlayers` probehl uspesne: 3/3. Screenshoty private room countdownu a lobby nastaveni byly zkontrolovany; lokalizacni nalez `Lobby`/`Best of` je overene opraveny.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_LobbyChat_SendsMessageToBothPlayers"` probehl RED na chybejicich chat selektorech a po oprave GREEN: 1/1. `RoomLobbyTests` prosly 8/8. Screenshot `private-room-lobby-chat-send` potvrzuje zpravu u odesilatele a cisty input po odeslani.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_LobbyChat_EmptyMessage_IsRejected"` probehl uspesne: 1/1. Screenshot `private-room-lobby-chat-empty-message` potvrzuje, ze whitespace zprava nevytvori chat radek a send akce zustane blokovana.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_LobbyChat_MaxTwoHundredCharacters_IsEnforced"` probehl uspesne: 1/1 po UX oprave zalamovani dlouhe zpravy. `RoomLobbyTests` prosly 8/8. Screenshot `private-room-lobby-chat-max-length` potvrzuje, ze 200znakovy text nepreteka mimo chat panel.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_LobbyChat_RateLimit_ShowsLocalizedErrorAndKeepsLobby"` probehl RED na celostrankovem chat erroru a po oprave GREEN: 1/1. `MatchHubTests` prosly 3/3 a Blazor multiplayer/lobby subset 16/16. Screenshot `private-room-lobby-chat-rate-limit` potvrzuje cesky alert a zachovanou lobby.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_LobbyChat_XssPayload_IsDisplayedEscaped"` probehl uspesne: 1/1. Screenshot `private-room-lobby-chat-xss-escaped` potvrzuje escapovany payload bez vlozeneho DOM elementu a test overil, ze `window.__lexiquestXss` zustal nulovy v obou browserech.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_LobbyChat_"` probehl po zmene dorucovani pres server broadcast uspesne: 5/5.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_BestOf3_BothReady_NavigatesBothPlayersToRealtimeGame"` probehl RED na zustani v lobby po countdownu a po oprave GREEN: 1/1. Screenshot `private-room-best-of3-starts-realtime-game` potvrzuje navigaci do realtime hry.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_BestOf3_CompletedMatch_ShowsSeriesScore|FullyQualifiedName~PrivateRoom_BestOf5_CompletedMatch_ShowsSeriesScore"` probehl po RED/GREEN opravach series resultu a ceske pluralizace uspesne: 2/2. `MatchResultTests` prosly 11/11 a Blazor private-room subset modal/lobby/landing pro pluralizaci 21/21. Screenshoty `private-room-best-of3-series-score` a `private-room-best-of5-series-score` potvrzuji `Série: 1:0`, +100 XP, bez ligovych XP a citelny result modal.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_CompletedMatch_DoesNotAwardLeagueXp"` probehl RED na chybejicim no-league-XP selectoru a na Best of 1 series score; po opravach GREEN: 1/1. `MatchResultTests` prosly 11/11 a `dotnet build src/LexiQuest.Api/LexiQuest.Api.csproj` probehl uspesne. Screenshot `private-room-no-league-xp/host-result-no-league-xp.png` potvrzuje +100 XP, zadne liga XP, private info alert a zadne series score; screenshot `private-room-no-league-xp/host-league-still-zero.png` potvrzuje ligovy zebricek s `0 XP`.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_RematchRequest_Accept_ReturnsBothPlayersToLobby|FullyQualifiedName~PrivateRoom_RematchRequest_Decline_NotifiesRequester"` probehl RED na chybejicim rematch UI toku a po opravach GREEN: 2/2. `MatchResultTests` prosly 15/15 a API/Blazor buildy prosly. Screenshot `private-room-rematch-accept/guest-rematch-request.png` potvrzuje vyzvu s `Přijmout`/`Odmítnout` bez duplicitniho rematch CTA; `private-room-rematch-accept/host-lobby-after-accept.png` potvrzuje navrat obou hracu do stejne lobby; `private-room-rematch-decline/host-rematch-declined.png` potvrzuje notifikaci o odmitnuti.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_ExpiredRoomCleanup_RemovesOldCodeAndReleasesHost"` probehl RED na chybejicim cleanup endpointu a neprihlasenem guest overeni; po opravach GREEN: 1/1. Screenshot `private-room-expiry-cleanup/host-new-room-after-cleanup.png` potvrzuje novou mistnost po cleanupu se zmenenym kodem; `private-room-expiry-cleanup/old-code-not-found-after-cleanup.png` potvrzuje, ze stary kod konci chybou `Místnost nenalezena`.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_NoTeamState_ShowsEmptyActions"` probehl RED na chybejici `ITeamService` registraci a chybejicich Team UI selectorech; po opravach GREEN: 1/1. `TeamPageTests` prosly 3/3 a API build probehl uspesne. Screenshot `teams/no-team-state/empty-state.png` potvrzuje citelny no-team stav s akcemi `Vytvořit tým` a `Hledat tým`.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_PremiumUser_CreatesTeamForFree"` probehl RED na neimplementovanem create modalu a po opravach GREEN: 1/1. `TeamPageTests` prosly 4/4. Screenshot `teams/premium-create-free/dashboard-after-create.png` potvrzuje premium vytvoreni zdarma, dashboard tymu, tag, popis a leadera v clenech.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_FreeUser_CreatesTeamForCoinsAndDeductsBalance"` probehl RED na neodecitenem coin balance a po oprave GREEN: 1/1. API build probehl uspesne. Screenshot `teams/free-create-coins/dashboard-after-coin-create.png` potvrzuje dashboard po vytvoreni tymu za mince a API overilo zustatek `0`.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_FreeUserWithoutCoins_CreateTeamIsRejected"` probehl uspesne: 1/1. Screenshot `teams/free-create-insufficient-coins/modal-error.png` potvrzuje citelny modalovy error, zachovani no-team stavu a API overilo, ze tym nevznikl a mince zustaly `0`. Behem screenshot auditu byl opraven modal spacing; `TeamPageTests` prosly 4/4.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_CreateTeamNameValidation_RequiresThreeToThirtyCharacters"` probehl uspesne: 1/1. Test overuje required chybu, pod-minimum 2 znaky, UI `maxlength` pro 31 znaku, uspesne vytvoreni s 3 znaky a API vytvoreni s 30 znaky. Screenshot `teams/create-name-validation/name-length-error.png` potvrzuje citelny field-level error.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_CreateTeamTagValidation_RequiresTwoToFourUppercaseAlphanumericCharacters"` probehl uspesne: 1/1. Test overuje tag required, 1 znak, lowercase format, uspesne 2 znaky a API vytvoreni se 4 znaky. Screenshot `teams/create-tag-validation/tag-format-error.png` potvrzuje citelny field-level error.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_CreateTeamDuplicateNameAndTag_ShowSpecificErrors"` probehl RED na obecne chybe pro duplicitni nazev/tag a po oprave GREEN: 1/1. `TeamPageTests` prosly 4/4. Screenshot `teams/create-duplicate-name-tag/duplicate-tag-error.png` potvrzuje konkretni citelnou chybu pro duplicitni tag.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_Dashboard_ShowsStatsDescriptionMembersAndRoles"` probehl RED na chybejicich dashboard selektorech a po oprave GREEN: 1/1. `TeamPageTests` prosly 4/4. Screenshot `teams/dashboard-stats-members/dashboard.png` potvrzuje citelne stat karty, popis, role a clenskou tabulku.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_LeaderAndOfficer_CanInviteMemberByUsername"` probehl RED na chybejicim invite UI a po oprave GREEN: 1/1. API build a `TeamPageTests` prosly. Screenshoty `teams/invite-member-leader-officer/leader-invite-success.png` a `invitee-visible-invite.png` potvrzuji modal i pozvanku u pozvaneho hrace.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_RegularMember_CannotInviteMember"` probehl uspesne: 1/1. Test overuje skrytou invite akci v UI, zamitnuty primy API POST a prazdne pozvanky u ciloveho hrace. Screenshot `teams/regular-member-invite-rejected/member-dashboard-no-invite.png` potvrzuje cisty member dashboard bez management akci.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_NoTeamUser_CanCreateJoinRequestFromRanking"` probehl RED na chybejicim ranking/join UI a po oprave GREEN: 1/1. `TeamPageTests` prosly. Screenshot `teams/join-request-from-ranking/join-request-success.png` potvrzuje modal se zpravou a success stav.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_Leader_CanApproveAndRejectJoinRequests"` probehl RED na EF concurrency padu pri pridani clena a po oprave GREEN: 1/1. API build probehl uspesne. Screenshoty `teams/approve-reject-join-requests/pending-requests.png` a `after-approve-reject.png` potvrzuji cekajici zadosti, schvaleneho clena a odmitnutou zadost.
- [x] `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~TeamPageTests"` probehl po kick UI opravach uspesne: 5/5.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_Officer_CanKickRegularMember"` probehl RED na chybejicim kick selectoru a screenshot auditem odhalil spatne role akce/stale dashboard; po opravach GREEN: 1/1. Screenshoty `teams/officer-kick-member/before-kick.png` a `after-kick.png` potvrzuji role-based akci, odstraneni clena a aktualizovane XP/pocty.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_LeaderCannotBeKicked"` probehl uspesne: 1/1. Test overuje absenci kick akce u leadera, officer API pokus jako BadRequest, leader self-kick jako BadRequest a zachovane clenstvi/role. Screenshot `teams/leader-cannot-be-kicked/leader-protected.png` potvrzuje ciste UI bez matoucich akci.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_MemberCanLeaveTeam"` probehl RED na chybejicim leave selectoru a po oprave GREEN: 1/1. Screenshoty `teams/member-leave-team/before-leave.png` a `after-leave-empty.png` potvrzuji odchod clena a navrat do no-team stavu.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_LastMemberDisbandsTeam"` probehl RED na chybejicim disband selectoru a screenshot auditem odhalil zbytecne `Predat vedeni` u solo leadera; po opravach GREEN: 1/1. Screenshoty `teams/last-member-disbands-team/before-disband.png` a `after-disband-empty.png` potvrzuji rozpušteni tymu.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_LeaderCanTransferLeadershipToMember"` probehl RED na nedokoncenem transfer flow a po opravach GREEN: 1/1. `TeamPageTests` prosly 5/5. Screenshoty `teams/transfer-leadership/modal-open.png` a `after-transfer.png` potvrzuji vyber kandidata, predani role a zmenu management akci.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_WeeklyRankingOrdersTeamsByWeeklyXp"` probehl uspesne: 1/1. Test overuje API i UI poradi podle weekly XP, ranky, XP a member count. Screenshot `teams/weekly-ranking/ordered-by-weekly-xp.png` potvrzuje citelny tymovy zebricek.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~ResponsiveE2ETests" -m:1 /p:BuildInParallel=false /v:m` probehl uspesne: 18/18. Ověřuje mobile `375x812`, tablet `768x1024`, desktop `1366x900`, wide desktop `1920x1080`, light/dark landing theme a bez horizontalniho overflowu; screenshoty `responsive/landing/*`, `responsive/landing-theme/*` a navazne `accessibility-performance/mobile-navigation-menu/375x812/light/*` byly vizualne zkontrolovane.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~EmailE2ETests.PasswordReset_LinkChangesPasswordAndOldPasswordStopsWorking" -m:1 /p:BuildInParallel=false /v:m` probehl uspesne: 1/1 po oprave nejednoznacneho dashboard selectoru.
- [x] `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~GameService_ReplayQueuedRequests_ConcurrentCalls_DoNotSubmitSameQueuedAnswerTwice" -m:1 /p:BuildInParallel=false /v:m` probehl RED na 2 POSTech a po reentrancy guardu GREEN: 1/1.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Pwa_OfflineTraining_UsesCachedSeedAndReplaysQueuedAnswer|FullyQualifiedName~LandingPage_GuestCta_NavigatesToGuestPlay" -m:1 /p:BuildInParallel=false /v:m` probehl uspesne: 2/2 po opravach PWA replay race a guest CTA cekani na UI stav.
- [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj -m:1 /p:BuildInParallel=false /v:m` probehl uspesne: 287/287 za 33 m 39 s.

---

## T-900: E2E infrastruktura

### T-900.1 Balicky a solution

- [x] Pridat `tests/LexiQuest.E2E.Tests` do `LexiQuest.slnx`.
- [x] Pridat NuGet balicky:
  - [x] `Testcontainers.MsSql`
  - [x] `DotNet.Testcontainers`
  - [x] `Microsoft.AspNetCore.Mvc.Testing` pokud bude potreba sdilet app konfiguraci - vyhodnoceno jako nepotrebne, E2E startuje realne API/Web procesy.
  - [x] `Respawn` nebo vlastni DB reset helper
  - [x] `Microsoft.Extensions.Configuration.Json`
  - [x] axe/a11y knihovnu pro Playwright, pokud bude pouzitelna s .NET testy - vyhodnoceno, pouzit vlastni `RunA11yCheckAsync` plus `AccessibilityPerformanceE2ETests`.
- [x] Doplnit Playwright browser install krok do README/CI.
- [x] Rozdelit testy pomoci `Trait` kategorii: `E2E`, `Smoke`, `Full`, `Visual`, `Email`, `SignalR`, `Admin`, `PWA`, `A11y`.

### T-900.2 Testcontainers: SQL Server

- [x] Vytvorit `E2EEnvironmentFixture`.
- [x] Spoustet `mcr.microsoft.com/mssql/server:2022-latest` pres `MsSqlContainer`.
- [x] Generovat silne testovaci SA heslo a connection string pred startem API.
- [x] Aplikovat EF migrace pred prvnim testem.
- [x] Seedovat deterministicka data po migracich.
- [x] Resetovat DB mezi testy bez restartu containeru.
- [x] Ukladat SQL/container logy pri padu testu do `artifacts/e2e/logs`.

### T-900.3 Testcontainers: smtp4dev

- [x] Spoustet `rnwood/smtp4dev` jako generic Testcontainer.
- [x] Mapovat SMTP port containeru `25` na nahodny localhost port.
- [x] Mapovat web/API port containeru `80` na nahodny localhost port.
- [x] Bindovat publikovane porty pouze na localhost kvuli bezpecnosti.
- [x] Nastavit API env:
  - [x] `EmailSettings__Host=127.0.0.1`
  - [x] `EmailSettings__Port={smtpPort}`
  - [x] `EmailSettings__UseSsl=false`
  - [x] `EmailSettings__Username=`
  - [x] `EmailSettings__Password=`
  - [x] `EmailSettings__FromEmail=noreply@lexiquest.test`
  - [x] `EmailSettings__BaseUrl={webBaseUrl}`
- [x] Implementovat `Smtp4DevClient` pro cteni dorucenych zprav pres web/API smtp4dev.
- [x] Testovat subject, prijemce, odkaz, HTML telo, plaintext fallback a to, ze email nebyl odeslan mimo container.
- [x] Pri implementaci vychazet z oficialniho repo `rnwood/smtp4dev`, ktere uvadi fake SMTP server, REST/OpenAPI a Docker podporu, a z jeho Docker security guideline doporuceni bindovat porty na `127.0.0.1`.

### T-900.4 Spousteni aplikace

- [x] Vytvorit `ApiProcessRunner` pro `src/LexiQuest.Api`.
- [x] Vytvorit `WebProcessRunner` pro `src/LexiQuest.Web`.
- [x] Pouzit dynamicke HTTP porty a vypnout potrebu dev certifikatu v E2E.
- [x] Nastavit API env:
  - [x] `ASPNETCORE_ENVIRONMENT=E2E`
  - [x] `ASPNETCORE_URLS=http://127.0.0.1:{apiPort}`
  - [x] `ConnectionStrings__DefaultConnection={testcontainerConnectionString}`
  - [x] `JwtSettings__SecretKey={longTestSecret}`
  - [x] `JwtSettings__Issuer=LexiQuest.E2E`
  - [x] `JwtSettings__Audience=LexiQuest.E2E.Client`
  - [x] `BlazorClient__Url=http://127.0.0.1:{webPort}`
- [x] Nastavit Web env:
  - [x] `ASPNETCORE_ENVIRONMENT=E2E`
  - [x] `ASPNETCORE_URLS=http://127.0.0.1:{webPort}`
  - [x] `ApiBaseUrl=http://127.0.0.1:{apiPort}`
- [x] Cekat na `/health/live` a `/health/ready`.
- [x] Zachytavat stdout/stderr API a Web procesu.
- [x] Korektne ukoncit procesy i pri failing testu.

### T-900.5 Deterministicka test data

- [x] Vytvorit `E2ETestDataSeeder`.
- [x] Seedovat slova s predem znamymi odpovedmi pro vsechny obtiznosti a kategorie.
- [x] Seedovat cesty, levely, boss levely, daily challenge, achievementy, shop itemy, premium features, ligy, admin role.
- [x] Vytvorit persony:
  - [x] `freeUser`
  - [x] `premiumUser`
  - [x] `lockedOutUser`
  - [x] `adminUser`
  - [x] `contentManagerUser`
  - [x] `teamLeader`
  - [x] `teamOfficer`
  - [x] `teamMember`
  - [x] `multiplayerUserA`
  - [x] `multiplayerUserB`
  - [x] `noProgressUser`
  - [x] `guestBrowserProfile`
- [x] Vytvorit API helper pro rychle prihlaseni pres realny login endpoint.
- [x] Vytvorit helper pro nastaveni stavu uzivatele pres DB seed, ne pres UI klikani, pokud scenar netestuje dany setup.

### T-900.6 Playwright fixture a artefakty

- [x] Nahradit soucasny `PlaywrightFixture` fixturem, ktery startuje celou testovaci sestavu.
- [x] Pouzivat izolovane browser contexty pro kazdy test.
- [x] Vytvorit helpers:
  - [x] `LoginAsAsync`
  - [x] `RegisterUniqueUserAsync`
  - [x] `GoToAndWaitForAppReadyAsync`
  - [x] `WaitForNoBusyIndicatorsAsync`
  - [x] `AssertNoConsoleErrorsAsync`
  - [x] `AssertNoFailedRequestsAsync`
  - [x] `TakeCheckpointScreenshotAsync`
  - [x] `RunA11yCheckAsync`
- [x] Automaticky ukladat screenshot, trace, video a console log pri failing testu.
- [x] Zapnout Playwright tracing pro failing testy a pro vizualni review job.
- [x] Nastavit viewporty:
  - [x] Mobile: `375x812`
  - [x] Tablet: `768x1024`
  - [x] Desktop: `1366x900`
  - [x] Wide desktop: `1920x1080`
- [x] Testovat light theme, dark theme a reduced motion.

### T-900.7 Stabilni selektory

- [x] Auditovat vsechny stranky a hlavni komponenty na `data-testid`.
- [x] Doplnit chybejici `data-testid` bez zmeny uzivatelskych textu.
- [x] Zakazat krehke selektory typu `.card:nth-child(3)` ve vsech novych E2E testech.
- [x] Vytvorit `Selectors` konstanty nebo page objecty.
- Overeni T-900.7: pridany `SelectorAuditE2ETests` pro verejne routy, chranene routy, hlavni komponenty a 404 stranku. `dotnet test tests/LexiQuest.Api.Tests/LexiQuest.Api.Tests.csproj --filter "FullyQualifiedName~TeamsControllerTests" -m:1 /p:BuildInParallel=false /v:m` prosel 3/3 a `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~SelectorAuditE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 2/2.

---

## T-901: Screenshot testy a UX review system

### T-901.1 Struktura screenshotu

- [x] Vytvorit `tests/LexiQuest.E2E.Tests/Screenshots` pro schvalene baseline.
- [x] Vytvorit `artifacts/e2e/screenshots` pro aktualni vystupy.
- [x] Pojmenovani: `{oblast}/{scenar}/{viewport}/{theme}/{state}.png`.
- [x] U kazdeho screenshot testu ulozit metadata: URL, persona, viewport, theme, seed verze, test name.
- [x] Screenshoty porovnavat pres Playwright `ToHaveScreenshotAsync` az po prvnim UX schvaleni.
- Overeni T-901.1: pridany approval manifest `tests/LexiQuest.E2E.Tests/Screenshots/approved-screenshots.json`, baseline README a E2E extension `ToHaveScreenshotAsync`; `TakeCheckpointScreenshotAsync` porovnava jen schvalene cesty z manifestu. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~ScreenshotApprovalManifest_ReferencesOnlyExistingPngBaselines" -m:1 /p:BuildInParallel=false /v:m` prosel 1/1.

### T-901.2 UX checklist pro kazdy screenshot

- [x] UI text je cesky a nema hardcoded anglictinu.
- [x] Texty se neprekryvaji a nevejdou-li se, zalamuji se profesionálne.
- [x] Primarni akce je zretelna, sekundarni akce neni vizualne agresivnejsi.
- [x] Loading, empty, success a error stavy jsou pochopitelne bez dodatecneho navodu.
- [x] Komponenty maji konzistentni rozestupy a zarovnani.
- [x] Modaly maji spravne fokusovani a nepreteka obsah.
- [x] Mobilni rozlozeni nema horizontalni scroll, pokud neni zamerne.
- [x] Dark theme ma dostatecny kontrast.
- [x] Animace nebrani cteni a reduced-motion varianta je klidna.
- [x] Screenshot potvrzuje funkcni stav scenare, ne jen vzhled stranky.

### T-901.3 Povinne screenshot checkpointy

- [x] Landing page: hero, feature tabs, paths preview, testimonialy, CTA, footer.
- [x] Auth: login, registrace, validacni chyby, lockout, password reset request, password reset confirm.
- [x] Dashboard: loading skeleton, naplneny dashboard, prazdny/novy uzivatel, error retry.
- [x] Game: start screen, aktivni hra, spravna odpoved, spatna odpoved, level complete, game over, low timer, no lives.
- [x] Paths: seznam cest, zamcena cesta, detail levelu, mapa levelu, boss node.
- [x] Boss: Marathon, Condition forbidden letter, Twist reveal, victory, defeat.
- [x] Streak/XP: streak normal, at risk, shield/freeze, level-up modal.
- [x] Leagues: leaderboard, promo zony, demo zony, historie, weekly reset state.
- [x] Daily challenge: nehrano, dokonceno, leaderboard, already completed.
- [x] Achievements: all tab, filtered tab, locked, in progress, unlocked, unlock modal.
- [x] Settings/Profile: vsechny taby, avatar upload, heslo, notifikace, soukromi, danger zone.
- [x] Guest: landing CTA, guest game, CTA modal, daily limit, conversion modal.
- [x] Premium: pricing, locked feature tooltip, checkout success, checkout cancel, premium badge.
- [x] Shop: kategorie, item available, owned, equipped, premium-only, insufficient coins.
- [x] Dictionaries: empty state, create modal, detail, import result, free-user premium gate.
- [x] Multiplayer: landing, matchmaking search, match found, realtime game, result modal, history.
- [x] Private rooms: create modal, room code, waiting lobby, joined lobby, chat, countdown, expired/full/error.
- [x] Teams: no-team state, create modal, dashboard, management, requests, ranking.
- [x] Notifications: bell unread, dropdown, preferences, email-triggered notification state.
- [x] Admin: dashboard, word table, word modal, import/export result, user detail drawer.
- [x] AI challenge: analysis, challenge cards, session feedback, empty-data state.
- [x] PWA/offline: install prompt, offline banner, offline training mode, service worker update prompt.
- [x] Error pages: 404, unauthorized, forbidden, global error boundary.

Overeno T-901.3 Auth:
- `AuthE2ETests` pokryva registraci, login, remember-me, refresh token, invalid refresh, invalid credentials, lockout, logout, duplicate email/username, validacni edge cases, focused auth layout a password strength; `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~AuthE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 27/27.
- `EmailE2ETests` pokryva password reset request pres smtp4dev, welcome email, kompletni reset flow, pouzity token, neplatny token, expirovany token a stejne nove heslo jako stare; `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~EmailE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 7/7.
- Focused auth screenshoty pro `/login`, `/register`, `/password-reset` a `/password-reset/neplatny-token-123` byly regenerovane testem `AuthPages_RenderFocusedLayoutWithoutAppSidebar` a vizualne zkontrolovane bez sidebaru, bez prekryvu textu a s citelnymi formulary.

Overeno T-901.3 Dashboard:
- `DashboardE2ETests` pokryva novy/prazdny dashboard, naplneny dashboard, realny loading skeleton pres E2E delay stats hook a error retry pres E2E fail-next stats hook; `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~DashboardE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 4/4.
- Screenshoty `dashboard/new-user-empty-progress`, `dashboard/populated-user-stats`, `dashboard/loading-skeleton` a `dashboard/error-retry` byly vizualne zkontrolovane. Loading checkpoint po oprave zachycuje skeleton, error checkpoint ma srozumitelny retry stav a recovered checkpoint znovu zobrazuje dashboard.

Overeno T-901.3 Game:
- `GameFlowE2ETests` pokryva start screen, aktivni hru, trening, time attack, spravnou odpoved, spatnou odpoved, skip, timer expiry, low timer, zivoty, low/no lives, game over, level complete, level-up/unlocks, reload session a pristup ciziho uzivatele; `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GameFlowE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 38/38.
- Screenshoty `game/start-screen`, `game/start-training`, `game/correct-answer-next-round`, `game/wrong-answer-feedback`, `game/level-complete`, `game/last-life-game-over`, `game/low-timer-warning`, `game/low-lives-warning-regen`, `game/level-up-modal` a `game/missing-session` byly vizualne zkontrolovane. Po opravach uz start hry nezobrazuje resource klice, aktivni hra ma stylovany input/timer a level/game-over modaly maji citelny dialog bez prekryvu achievement modalem.

Overeno T-901.3 Paths:
- `PathsE2ETests` pokryva seznam cest, zamcene cesty, odemceni pokrocile cesty na levelu 5, detail/mapu zacatecnicke cesty, boss node, detail levelu, start path hry, dokoncenou/perfektni uroven a posun aktualniho levelu; `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PathsE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 5/5.
- Screenshoty `paths/new-user-lock-state`, `paths/level-five-unlocks-intermediate`, `paths/beginner-detail-starts-path-game` (`map`, `level-modal`, `path-game`), `paths/complete-level-perfect-progress` a `paths/completed-progress-state` byly vizualne zkontrolovane. Zamcene/odemcene cesty, boss node i level modal jsou citelne a bez prekryvu.

Overeno T-901.3 Boss:
- `BossE2ETests` pokryva Marathon start bez regen, victory modal s bonusy, defeat modal bez regen, Condition forbidden letters a penalty rules, Twist reveal pred/po odhaleni a early-guess bonus prahy; `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~BossE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 9/9.
- Screenshoty `boss/marathon-start-no-regen`, `boss/condition-forbidden-penalty`, `boss/twist-reveal-after-three-seconds`, `boss/marathon-victory-perfect-speed` a `boss/marathon-defeat-no-regen` byly vizualne zkontrolovane. Twist checkpoint po oprave zachycuje stabilni reveal stav a nasledne pribyvajici pismeno.

Overeno T-901.3 Streak/XP:
- `StreakE2ETests` pokryva prvni dokonceni session, stejny den bez navyseni, dalsi den navyseni, grace period, missed reset, aktivni shield, premium auto-freeze, free/premium shield cooldowny, nakup shieldů, emergency shield premium-only a dashboard screenshoty pro normal/at-risk/shield/freeze; `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~StreakE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 15/15.
- `StreakIndicatorTests` overuji komponentu streaku vcetne aktivniho stitu, dostupneho zmrazeni a ceske pluralizace `4 dny`; `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~StreakIndicatorTests" -m:1 /p:BuildInParallel=false /v:m` prosel 10/10.
- Screenshoty `streak/dashboard-normal-streak-xp`, `streak/dashboard-at-risk-countdown`, `streak/dashboard-free-shield-activate`, `streak/dashboard-premium-freeze-badge`, `dashboard/xp-bar-api-progress`, `game/level-up-modal` a `game/multi-level-up-single-xp-gain` byly vizualne zkontrolovane. Behem UX review byly opraveny lokalizacni nalezy `E2E-BUG-0202` a `E2E-BUG-0203`; finalni screenshoty zobrazuji `Přehled`, `Úroveň`, `Zmrazení dostupné` a spravne `4 dny`.

Overeno T-901.3 Leagues:
- `LeaguesE2ETests` pokryva prirazeni noveho uzivatele do Bronze ligy, leaderboard sortovani, highlight aktualniho uzivatele, promotion/demotion zony, countdown warning/critical stavy, weekly reset s postupem/sestupem a historii, plus unauthenticated redirect; `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~LeaguesE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 6/6.
- Screenshoty `leagues/new-user-bronze`, `leagues/leaderboard-zones`, `leagues/countdown-warning`, `leagues/countdown-critical`, `leagues/weekly-reset-tier-moves` (`promoted-silver`, `demoted-bronze`) a `leagues/unauthenticated-rejected` byly vizualne zkontrolovane. Leaderboard zony, historie resetu, odmeny i urgency countdownu jsou citelne a bez prekryvu.

Overeno T-901.3 Daily challenge:
- `DailyChallengeE2ETests` pokryva nehranou dnesni vyzvu s prazdnym leaderboardem, start/play panel, dokonceni, top 10 leaderboard, API odmítnutí druheho pokusu a reset dalsi den pres E2E time travel; `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~DailyChallengeE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 3/3.
- `DailyChallengePageTests` overuji render challenge, modifier badge, completed state, leaderboard, empty state a sub-second vysledek jako `1s`; `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~DailyChallengePageTests" -m:1 /p:BuildInParallel=false /v:m` prosel 6/6.
- Screenshoty `daily/today-start`, `daily/completion-top10-second-attempt` a `daily/next-day-reset` byly vizualne zkontrolovane. Behem UX review byl opraven `E2E-BUG-0204`; finalni completed screenshot ukazuje `1s` misto zavadejiciho `0s`.

Overeno T-901.3 Achievements:
- `AchievementsE2ETests` pokryva autentizovane API, seedovany katalog, all tab s unlocked/in-progress/locked stavem, Performance a Streak filtry, unlock modal pri prvnim slovu a kontrolu, ze se `first_word` neduplikuje; `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~AchievementsE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 3/3.
- Screenshoty `achievements/overview-filter-card-states` (`all-states`, `performance-filter`, `streak-filter`) a `achievements/first-word-unlock-no-duplicate/unlock-modal` byly vizualne zkontrolovane. Behem UX review byl opraven `E2E-BUG-0205`; finalni page title je sjednoceny na `Úspěchy`.

Overeno T-901.3 Settings/Profile:
- `SettingsE2ETests` pokryva profilove udaje, username duplicitu, validni avatar upload a preview, spatny typ avataru, prilis velky avatar, zmenu hesla se spatnym aktualnim heslem i uspesnou zmenu, notifikacni predvolby, theme/language, soukromi public/friends/private, odhlaseni, deaktivaci a smazani uctu s potvrzovacim textem; `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~SettingsE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 4/4.
- `ProfileE2ETests` pokryva samostatnou `/profile` stranku s premium badge, seeded statistikami, streak hodnotami, korektnim formatem presnosti, viditelnymi ikonami a kartou uspechu; `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~ProfileE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 1/1.
- `SettingsPageTests` probehly po doplneni inline password statusu: `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~SettingsPageTests" -m:1 /p:BuildInParallel=false /v:m` prosel 10/10.
- Screenshoty `settings/profile-username-avatar-validation/profile-saved`, `settings/password-change-and-wrong-current/wrong-current-password`, `settings/preferences-theme-language-privacy/saved`, `settings/danger-zone-logout-deactivate-delete/deactivate-confirm`, `delete-confirm`, `login-after-delete` a `profile/summary-premium-stats-achievements/loaded` byly vizualne zkontrolovane. Behem UX review byly opraveny `E2E-BUG-0206`, `E2E-BUG-0207`, `E2E-BUG-0208`, `E2E-BUG-0209`, `E2E-BUG-0210` a `E2E-BUG-0211`.

Overeno T-901.3 Guest:
- `GuestE2ETests` pokryva verejnou `/play` route bez uctu, start guest hry, disabled empty submit, spatnou odpoved se spravnym resenim, CTA modal po spravne odpovedi, pokracovani ve hre, convert modal po dokonceni hry, ulozeni guest progressu do `localStorage`, prenos progressu pri registraci, denni limit 5 her, limit reset po 24 hodinach pres E2E time travel a redirect chranene route na login bez tokenu; `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GuestE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 7/7.
- `GuestGamePageTests` probehly po prepojeni na realne guest komponenty: `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~GuestGamePageTests" -m:1 /p:BuildInParallel=false /v:m` prosel 8/8. Komponentove guest testy `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --no-build --filter "FullyQualifiedName~Components.Guest" -m:1 /p:BuildInParallel=false /v:m` prosly 27/27.
- Screenshoty `guest/welcome/loaded`, `guest/start-game/active-game`, `guest/wrong-answer-feedback/feedback-visible`, `guest/cta-modal-after-correct-answer/visible`, `guest/conversion-transfers-progress/conversion-modal`, `guest/daily-limit-registration-cta/limit-reached`, `guest/daily-limit-reset-after-24h/reset-allows-game` a `guest/protected-features-redirect/login-required` byly vizualne zkontrolovane. CTA/convert checkpointy maji navic E2E geometrii, ktera overuje, ze modal i overlay skutecne protinaji viewport; behem UX review byly opraveny `E2E-BUG-0212`, `E2E-BUG-0213` a `E2E-BUG-0214`.

Overeno T-901.3 Premium:
- `PremiumE2ETests` pokryva free premium overview, pricing karty, best-value badge, locked feature tooltip, fake checkout redirect pro monthly/yearly/lifetime, fake checkout success aktivujici premium, aktivni premium badge na `/premium` i `/profile`, checkout cancel bez aktivace, cancel subscription, expired status, Stripe webhook checkout completed/invoice paid/invoice failed/subscription cancelled, invalid signature reject a expiry reminder email zachyceny ve smtp4dev; `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PremiumE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 13/13.
- `PremiumPageTests` overuji pricing, hero, feature groups, best-value badge, discounted price, subscribe calls, payment/cancel notice a active status; `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~PremiumPageTests" -m:1 /p:BuildInParallel=false /v:m` prosel 12/12.
- Screenshoty `premium/overview-free-locked-features/free`, `premium/fake-checkout-success-activates/success-page`, `premium/fake-checkout-success-activates/active`, `premium/checkout-cancel-no-activation/cancel` a `premium/expiry-reminder-email-smtp4dev/active-expiring-soon` byly vizualne zkontrolovane. Behem UX review byl opraven `E2E-BUG-0215`, aby checkout success i cancel mely samostatne focused viewport checkpointy.

Overeno T-901.3 Dictionaries:
- `DictionariesE2ETests` pokryva free-user premium gate vcetne API rejectu, premium empty state, create modal s validaci nazvu, vytvoreni verejneho slovniku, pridani slova, CSV import preview, persistentni import result, detail modal se skutecne importovanymi slovy, public tab a smazani slovniku; `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~DictionariesE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 4/4.
- `DictionaryServiceTests` a `DictionaryServiceEdgeCaseTests` pokryvaji core slovnikove edge casy vcetne detailu se slovy, duplicit, limitu, validace slov a importu CSV/TXT/JSON; `dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj --filter "FullyQualifiedName~DictionaryServiceTests|FullyQualifiedName~DictionaryServiceEdgeCaseTests" -m:1 /p:BuildInParallel=false /v:m` prosel 39/39. `DictionaryControllerTests` a `DictionaryFlowTests` prosly 17/17.
- Screenshoty `dictionaries/free-user-premium-gate/gate`, `dictionaries/premium-crud-import-public-delete/empty`, `create-validation`, `import-preview`, `import-result`, `detail` a `public-visible` byly vizualne zkontrolovane. Detail endpoint vraci `DictionaryDto.Words`; UI detail modal zobrazuje `měsíc/slunce/strom` vcetne obtiznosti. Behem UX review byl opraven `E2E-BUG-0216`, aby checkpointy nebyly prekryte historickymi toast notifikacemi.

Overeno T-901.3 Private rooms:
- `MultiplayerE2ETests` private-room subset pokryva create modal, room code format a copy, waiting lobby, join valid room, joined lobby pro oba hrace, invalid code validation, not-found error, expired code, full room, single active room, host/guest leave, ready/cancel ready, countdown, lobby chat send/empty/max-length/rate-limit/XSS, Best of 3/5 series score, no league XP, rematch accept/decline a expiry cleanup; `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_" -m:1 /p:BuildInParallel=false /v:m` prosel 23/23.
- `RoomLobbyTests` a `MultiplayerLandingPageTests` overuji lobby render, ready/countdown mezistav, chat a landing private-room modal/controls; `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~RoomLobbyTests|FullyQualifiedName~MultiplayerLandingPageTests" -m:1 /p:BuildInParallel=false /v:m` prosel 17/17.
- Screenshoty `private-room-create-settings-code-copy/host-lobby-code-settings`, `private-room-join-valid/guest-joined-lobby`, `private-room-lobby-chat-send/host-message-visible`, `private-room-both-ready-countdown/countdown-started`, `private-room-join-expired-code/expired-error` a `private-room-join-full-room/full-room-error` byly vizualne zkontrolovane. Behem opakovaneho full subsetu byl opraven `E2E-BUG-0217`, aby countdown UI bylo viditelne i pri race mezi `CountdownTick` a refresh room statusu.

---

## T-910: Public web, routing, lokalizace, PWA

- [x] Landing page se nacte bez console errors a failed requests.
- [x] Landing CTA registrace vede na `/register`.
- [x] Landing CTA guest vede na `/play` nebo aktualni guest route.
- [x] Login link vede na `/login`.
- [x] Footer odkazy vedou na About, Terms, Privacy, Contact.
- [x] SEO meta tagy, Open Graph a JSON-LD jsou dostupne.
- [x] 404 route zobrazi lokalizovanou NotFound stranku.
- [x] Neautorizovana chranena route presmeruje na login nebo zobrazi korektni 401/403 stav.
- [x] Navigace funguje klavesnici i mysi na mobilu/tabletu/desktopu.
- [x] Service worker se registruje.
- [x] Manifest je dostupny a install prompt se zobrazi v podporovanem kontextu.
- [x] Offline banner se zobrazi pri offline rezimu.
- [x] Offline training mode funguje pouze tam, kde je to povoleno.
- [x] Queue API volani pri offline rezimu se po navratu online korektne prehraje.

---

## T-911: Registrace, prihlaseni, tokeny, obnova hesla

### Registrace

- [x] Uspesna registrace noveho uzivatele.
- [x] Duplicitni email vrati viditelnou chybu.
- [x] Duplicitni username vrati viditelnou chybu.
- [x] Validace emailu: prazdny, spatny format, dlouhy vstup.
- [x] Validace username: prazdny, min-1, max+1, nepovolene znaky, diakritika pokud nepovolena.
- [x] Validace hesla: kratke, bez uppercase, bez lowercase, bez cisla, bez special znaku.
- [x] Confirm password mismatch.
- [x] Neodsouhlasene podminky.
- [x] Password strength indicator meni stav.
- [x] Po registraci dorazi welcome email do smtp4dev, pokud je ve flow zapnuty.
- [x] Registrace po guest conversion prenese progress.

### Prihlaseni a session

- [x] Uspesny login existujiciho uzivatele.
- [x] Neexistujici email ma generic error a neprozradi existenci uctu.
- [x] Spatne heslo ma generic error.
- [x] Pet spatnych pokusu zamkne ucet.
- [x] Zamceny ucet zobrazi lokalizovany lockout stav.
- [x] Remember me zustane prihlasen po reloadu.
- [x] Logout smaze tokeny a chranene route jsou nepristupne.
- [x] Expired access token se obnovi pres refresh token.
- [x] Neplatny refresh token odhlasi uzivatele.
- [x] Soubezne browser contexty se navzajem neovlivni.

### Obnova hesla pres smtp4dev

- [x] Request resetu pro existujici email vrati stejny public stav jako neexistujici email.
- [x] Reset email je zachycen ve smtp4dev.
- [x] Reset email obsahuje spravneho prijemce, subject, odkaz a cesky obsah.
- [x] Reset odkaz otevira confirm stranku.
- [x] Validni token zmeni heslo.
- [x] Pouzity token nejde pouzit znovu.
- [x] Expirovany token zobrazi chybu.
- [x] Neplatny token zobrazi chybu.
- [x] Nove heslo stejne jako stare je odmitnuto.
- [x] Po resetu funguje login s novym heslem a nefunguje se starym.

---

## T-912: Zakladni hra, zivoty, XP, levely, cesty, streak

### Herni smycka

- [x] Start training hry.
- [x] Start timed/path hry.
- [x] Scrambled word je zobrazen a neni stejne jako original, pokud to slovo umoznuje.
- [x] Spravna odpoved zvysi XP, combo a posune k dalsimu kolu.
- [x] Spatna odpoved vynuluje combo a ukaze spravnou odpoved.
- [x] Odpoved je case-insensitive.
- [x] Leading/trailing whitespace se trimuje.
- [x] Diakritika se musi shodovat.
- [x] Prazdna odpoved je validacni chyba nebo disabled submit.
- [x] Odpoved nad limit je odmitnuta.
- [x] Timer expiry se projevi jako spatna/prazdna odpoved podle pravidel.
- [x] Skip/forfeit ukonci nebo posune hru podle pravidel.
- [x] Reload aktivni session obnovi stav.
- [x] Neexistujici session zobrazi 404/error state.
- [x] Session jineho uzivatele je nepristupna.

### Zivoty

- [x] Training mode ma nekonecne zivoty.
- [x] Beginner/Intermediate/Advanced/Expert maji spravny pocet zivotu.
- [x] Spatna odpoved ubere zivot.
- [x] Pri nule zivotu nastane game over.
- [x] Regen timer je viditelny.
- [x] Regenerace neprekroci maximum.
- [x] Low-lives warning je viditelny.

### XP a levely

- [x] Speed bonusy pro hranice pod 3s, 5s, 10s.
- [x] Combo multiplikatory 3+, 5+, 10+.
- [x] Streak bonus.
- [x] Level-up modal pri prekroceni hranice.
- [x] Vice level-upu v jednom zisku XP. Service vetev overena v `GameSessionService_SubmitAnswer_LargeXpGain_ReturnsMultiLevelXpEvent`; realny UI/API tok overuje `Game_MultiLevelUpFromSingleXpGain_ShowsFinalLevelAndAllUnlocks` pres E2E-only deterministickou odmenu 500 XP za spravnou odpoved.
- [x] Unlocky na levelech 3, 5, 10 a dalsich definovanych levelech.
- [x] XP hodnoty odpovidaji DB/API.
- [x] XP bar odpovida DB/API.

### Cesty

- [x] Seznam 4 cest.
- [x] Beginner odemcena pro noveho uzivatele.
- [x] Intermediate/Advanced/Expert zamcene podle pravidel.
- [x] Odemceni cesty po splneni podminek.
- [x] Path map zobrazi locked/current/completed/perfect/boss stavy.
- [x] Detail levelu zobrazi slova, cas, hints, zivoty, odmeny.
- [x] Start levelu otevre hru.
- [x] Dokonceni levelu zmeni progress.
- [x] Perfect completion zobrazi perfect stav.

### Streak a ochrany

- [x] Prvni den nastavi streak na 1.
- [x] Stejny den neinkrementuje.
- [x] Nasledujici den inkrementuje.
- [x] Grace period 48h funguje.
- [x] Missed day resetuje streak nebo pouzije ochranu.
- [x] At-risk stav ukaze countdown.
- [x] Shield lze aktivovat.
- [x] Shield nelze aktivovat dvakrat.
- [x] Free/premium limity shieldu.
- [x] Premium auto-freeze ochrani streak.
- [x] Nakup shieldu za mince odecte mince.
- [x] Emergency shield jen pro premium.

Overeno:
- `StreakE2ETests` pokryva prvni dokonceni session, stejne datum, navazujici den, 47h grace period a reset po 73h bez ochrany.
- `StreakServiceTests` pokryva presnou 48h grace period na service/domene.
- `Streak_DashboardAtRisk_ShowsCountdown` overuje dashboard countdown a screenshot `artifacts/e2e/screenshots/streak/dashboard-at-risk-countdown/1366x900/light/at-risk.png` byl zkontrolovan z pohledu UX.
- `Streak_DashboardFreeShield_CanActivate` overuje aktivaci free shieldu z dashboardu a screenshot `artifacts/e2e/screenshots/streak/dashboard-free-shield-activate/1366x900/light/shield-active.png` byl zkontrolovan z pohledu UX.
- `Streak_ShieldCannotBeActivatedTwice` a `Streak_FreeAndPremiumShieldCooldowns_AreApplied` overuji API pravidla double aktivace a free/premium cooldownu.
- `Streak_PurchaseShields_DeductsCoinsAndAddsShields` overuje pridani 3 shieldu a odecteni 500 minci pres `/api/v1/shop/coins`.
- `Streak_EmergencyShield_IsPremiumOnlyAndDeductsCoins` overuje 403 pro free uzivatele a premium emergency shield za 300 minci.
- `Streak_ActiveShield_MissedGracePeriod_PreservesStreakAndConsumesShield` a `Streak_PremiumAutoFreeze_MissedGracePeriod_PreservesStreak` overuji zachovani missed streaku pres aktivni shield a premium auto-freeze.

---

## T-913: MVP Extended

### Ligy

- [x] Novy uzivatel je v Bronze.
- [x] Leaderboard je serazeny podle weekly XP.
- [x] Top promo zona a bottom demo zona jsou vizualne oznacene.
- [x] Aktualni uzivatel je zvyraznen.
- [x] Countdown do weekly resetu meni vizualni stav pod 24h a pod 6h.
- [x] Weekly reset posune promoted/demoted uzivatele.
- [x] Historie lig se zobrazi.
- [x] Odmeny dle tieru jsou viditelne.
- [x] Neprihlaseny uzivatel je odmitnut.

Overeno:
- `Leagues_NewUser_IsAssignedToBronzeLeague` overuje automaticke zarazeni noveho uzivatele do Bronze ligy a viditelne odmeny tieru; screenshot `artifacts/e2e/screenshots/leagues/new-user-bronze/1366x900/light/loaded.png` byl zkontrolovan z pohledu UX.
- `Leagues_Leaderboard_SortsHighlightsAndMarksPromotionDemotionZones` overuje razeni podle weekly XP, zelenou promo zonu, cervenou demo zonu a modre zvyrazneni aktualniho uzivatele; screenshot `artifacts/e2e/screenshots/leagues/leaderboard-zones/1366x900/light/ranked.png` byl zkontrolovan z pohledu UX.
- `Leagues_Countdown_UnderThresholds_UsesVisualState` overuje `warning` stav pod 24h a `critical` stav pod 6h; screenshoty `artifacts/e2e/screenshots/leagues/countdown-warning/1366x900/light/warning.png` a `artifacts/e2e/screenshots/leagues/countdown-critical/1366x900/light/critical.png` byly zkontrolovane z pohledu UX.
- `Leagues_WeeklyReset_MovesPromotedAndDemotedUsers` overuje Bronze -> Silver promotion, Silver -> Bronze demotion a historii minule ligy pres E2E reset job; screenshoty `artifacts/e2e/screenshots/leagues/weekly-reset-tier-moves/1366x900/light/promoted-silver.png` a `artifacts/e2e/screenshots/leagues/weekly-reset-tier-moves/1366x900/light/demoted-bronze.png` byly zkontrolovane z pohledu UX.
- `Leagues_UnauthenticatedUser_IsRejected` overuje 401 z league API a redirect `/leagues` bez tokenu na login; screenshot `artifacts/e2e/screenshots/leagues/unauthenticated-rejected/1366x900/light/login-required.png` byl zkontrolovan z pohledu UX.

### Denni vyzva

- [x] Dnesni vyzva se zobrazi.
- [x] Modifier odpovida dni v tydnu.
- [x] Start daily challenge.
- [x] Dokonceni ukaze cas, XP a rank.
- [x] Leaderboard top 10.
- [x] Druhy pokus ve stejny den je odmitnut.
- [x] Reset dalsi den zpristupni novou vyzvu.

Overeno:
- `DailyChallenge_TodayChallenge_DisplaysExpectedModifierAndStarts` overuje zobrazeni dnesni vyzvy, modifier podle dne v tydnu, viditelny empty leaderboard a start challenge; screenshot `artifacts/e2e/screenshots/daily/today-start/1366x900/light/ready.png` byl zkontrolovan z pohledu UX.
- `DailyChallenge_Completion_ShowsResultTopTenAndRejectsSecondAttempt` overuje dokonceni spravnou odpovedi, cas, XP, rank, top-10 leaderboard a odmítnuti druheho pokusu ve stejnem dni pres API 409; screenshot `artifacts/e2e/screenshots/daily/completion-top10-second-attempt/1366x900/light/completed.png` byl zkontrolovan z pohledu UX.
- `DailyChallenge_NextDayReset_AllowsNewChallenge` overuje, ze po posunu E2E casu o jeden den je dostupna nova vyzva; screenshot `artifacts/e2e/screenshots/daily/next-day-reset/1366x900/light/available.png` byl zkontrolovan z pohledu UX.
- `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~DailyChallengeE2ETests"` probehl uspesne: 3/3.

### Achievementy

- [x] Stranka zobrazi celkovy progress.
- [x] Filtrovani podle kategorii.
- [x] Locked karta.
- [x] In-progress karta s progress barem.
- [x] Unlocked karta s datumem a XP.
- [x] Unlock modal po splneni achievementu.
- [x] Achievement se neodemkne duplicitne.
- [x] Progress odpovida skutecnym datum.

Overeno:
- `Achievements_ApiAuthenticated_ReturnsSeededCatalog` overuje authenticated achievement API a seednuty katalog `first_word`/`streak_7`.
- `Achievements_Page_ShowsProgressFiltersAndCardStates` overuje celkovy progress, filtrovani kategorii, locked/in-progress/unlocked karty, progress `50 / 100` a datum odemceni `17.06.2026`; screenshot `artifacts/e2e/screenshots/achievements/overview-filter-card-states/1366x900/light/streak-filter.png` byl zkontrolovan z pohledu UX.
- `Achievements_FirstWordUnlock_ShowsModalAndDoesNotDuplicate` overuje realne odemceni `first_word` po spravne odpovedi ve hre, modal s XP a to, ze opakovane splneni nevytvori duplicitni unlock zaznam; screenshot `artifacts/e2e/screenshots/achievements/first-word-unlock-no-duplicate/1366x900/light/unlock-modal.png` byl zkontrolovan z pohledu UX.
- `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~AchievementsE2ETests"` probehl uspesne: 3/3.

### Boss levely

- [x] Marathon startuje 20 slov a 3 zivoty.
- [x] Marathon nema regen.
- [x] Marathon perfect bonus.
- [x] Marathon speed bonus pod 5 min.
- [x] Condition boss ma 15 slov.
- [x] Kazde 3. slovo ma forbidden letter pravidlo.
- [x] Zakazane pismeno v odpovedi penalizuje XP.
- [x] Validni odpoved bez zakazaneho pismene nepenalizuje.
- [x] Twist boss startuje s odkrytymi pismeny.
- [x] Twist reveal po 3s.
- [x] Early guess bonus pro 2/3/4/5 odkrytych pismen.
- [x] Victory modal.
- [x] Defeat modal.

Overeno:
- `Boss_Marathon_StartsWithTwentyWordsThreeLivesAndNoRegen` overuje API start `201 Created`, 20 kol, 3 zivoty, zadny regen text a screenshot `artifacts/e2e/screenshots/boss/marathon-start-no-regen/1366x900/light/start.png`.
- `Boss_Marathon_VictoryModal_ShowsPerfectAndSpeedBonuses` overuje perfect bonus `+200 XP`, speed bonus `+50 XP`, celkove XP vcetne bonusu a victory modal; screenshot `artifacts/e2e/screenshots/boss/marathon-victory-perfect-speed/1366x900/light/victory-modal.png` byl zkontrolovan z pohledu UX.
- `Boss_Marathon_DefeatModal_ShowsAfterLastLifeLostWithoutRegen` overuje defeat modal po ztrate posledniho zivota a absenci regen textu; screenshot `artifacts/e2e/screenshots/boss/marathon-defeat-no-regen/1366x900/light/defeat-modal.png` byl zkontrolovan z pohledu UX.
- `Boss_Condition_EveryThirdRoundHasForbiddenLettersAndPenaltyRules` overuje 15 kol, forbidden letters na 3. kole, penalizaci `-5 XP` i validni odpoved bez penalizace; screenshot `artifacts/e2e/screenshots/boss/condition-forbidden-penalty/1366x900/light/third-round-warning.png` byl zkontrolovan z pohledu UX.
- `Boss_Twist_StartsWithRevealedLettersAndRevealsAfterThreeSeconds` overuje odkryta pismena, UI reveal po 3 sekundach a screenshoty `loaded-reveal-state`/`after-three-seconds`.
- `Boss_Twist_EarlyGuessBonus_MatchesRevealedLetterCount` overuje early guess bonusy pro 2/3/4/5 odkrytych pismen: `+10`, `+7`, `+5`, `+2 XP`.
- `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~BossE2ETests"` probehl uspesne: 9/9.

### Nastaveni a UX polish

- [x] Profil: zmena username.
- [x] Profil: duplicitni username.
- [x] Profil: avatar upload preview a validace typu/velikosti.
- [x] Heslo: zmena hesla.
- [x] Heslo: spatne stare heslo.
- [x] Notifikace: vsechny toggly a cas reminderu.
- [x] Zobrazeni: light/dark/auto theme.
- [x] Jazyk selector neporusi ceskou default kulturu.
- [x] Animace/zvuky toggly.
- [x] Soukromi: public/friends/private.
- [x] Danger zone: logout, deaktivace, delete s double confirm.
- [x] Skeleton loading na vsech hlavnich strankach.
- [x] Error boundary retry.
- [x] Toast success/error/warning/info.

Overeno:
- Landing page: `LandingPage_LoadsAllPrimarySections_AndStoresUxCheckpoint` uklada full-page checkpoint pro hero, how-it-works, feature sekci, paths preview, testimonialy, CTA a footer; `LandingPage_FeatureTabs_RenderAllPanels_AndStoreUxCheckpoints` uklada samostatne checkpointy `rpg`, `souboje`, `souteze` pro feature taby. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~LandingE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 10/10 a screenshoty feature tabu byly UX zkontrolovany.
- `Settings_ProfileUsernameDuplicateAndAvatarValidation_WorkEndToEnd` overuje zmenu username, duplicitni username, avatar preview, validaci typu a velikosti; screenshot `artifacts/e2e/screenshots/settings/profile-username-avatar-validation/1366x900/light/profile-saved.png` byl zkontrolovan z pohledu UX.
- `Settings_PasswordChangeAndWrongCurrentPassword_WorkEndToEnd` overuje spatne stare heslo, uspesnou zmenu hesla a login jen s novym heslem.
- `Settings_PreferencesThemeLanguageNotificationsAndPrivacy_PersistToProfile` overuje notifikacni toggly, reminder cas `18:45`, light/dark/auto theme, cesky jazyk selector, animace/zvuky a soukromi public/friends/private; screenshot `artifacts/e2e/screenshots/settings/preferences-theme-language-privacy/1366x900/light/saved.png` byl zkontrolovan z pohledu UX.
- `Settings_DangerZone_LogoutDeactivateAndDeleteRequireConfirmation` overuje logout, deaktivaci s potvrzenim a smazani uctu s potvrzenim.
- `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~SettingsE2ETests"` probehl uspesne: 4/4.
- `Layout_MainPages_ShowSkeletonLoadingStates` pozdrzuje realne API requesty pres E2E-only hook a overuje skeleton/loading checkpointy pro `/dashboard`, `/settings`, `/profile`, `/team`, `/paths` a `/ai-challenge`; `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Layout_MainPages_ShowSkeletonLoadingStates" -m:1 /p:BuildInParallel=false /v:m` prosel 1/1. Screenshoty `artifacts/e2e/screenshots/layout/main-pages-loading-states/1366x900/light/{dashboard,settings,profile,team,paths,ai-challenge}.png` byly vizualne zkontrolovane; behem review byla opravena `E2E-BUG-0219`.
- `Layout_GlobalErrorBoundary_ShowsCzechFallbackAndRetry` overuje globalni cesky error fallback, absenci defaultni anglicke Blazor hlasky a retry zotaveni; `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Layout_GlobalErrorBoundary_ShowsCzechFallbackAndRetry" -m:1 /p:BuildInParallel=false /v:m` prosel 1/1. `ErrorBoundaryTests` prosly 3/3 a screenshot `artifacts/e2e/screenshots/layout/global-error-boundary-retry/1366x900/light/fallback.png` byl vizualne zkontrolovan; behem review byla opravena `E2E-BUG-0218`.
- `Layout_Toasts_RenderSuccessErrorWarningAndInfoVariants` overuje realny `TmToastContainer` pro success/error/warning/info varianty; `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Layout_Toasts_RenderSuccessErrorWarningAndInfoVariants" -m:1 /p:BuildInParallel=false /v:m` prosel 1/1. Screenshot `artifacts/e2e/screenshots/layout/toast-variants/1366x900/light/all-variants.png` byl vizualne zkontrolovan.
- `TeamPageTests` po uprave team loading skeletonu prosly 5/5.

---

## T-914: Landing, guest mode, premium, shop, slovniky

### Guest mode

- [x] Guest muze spustit hru bez loginu.
- [x] Guest hra pouziva beginner slova.
- [x] Guest hra ma 5 slov.
- [x] Po hre se zobrazi CTA modal.
- [x] Zbyvajici pocet her se zobrazi.
- [x] 5. hra za 24h je povolena.
- [x] 6. hra za 24h je odmitnuta s registracni CTA.
- [x] Limit se po 24h resetuje.
- [x] Guest progress je v localStorage.
- [x] Registrace z CTA prenese XP/statistiky.
- [x] Guest nema pristup k prihlasenym funkcim.

### Premium

- [x] Premium page zobrazi 3 plany.
- [x] Yearly ma best value badge.
- [x] Free uzivatel vidi zamcene premium features s tooltipem.
- [x] Checkout monthly/yearly/lifetime vrati checkout URL nebo E2E fake redirect.
- [x] Checkout success aktivuje premium stav.
- [x] Checkout cancel neaktivuje premium.
- [x] Premium badge v profilu/navigaci.
- [x] Cancel subscription zmeni stav podle pravidel.
- [x] Expired subscription odebere premium.
- [x] Stripe webhook scenare overit pres testovy webhook/fake adapter: completed, invoice paid, failed, cancelled, invalid signature.

### Shop a mince

- [x] Balance minci se zobrazi.
- [x] Kategorie: avatary, ramecky, temata, boosty.
- [x] Koupit dostupnou polozku.
- [x] Nedostatek minci.
- [x] Jiz vlastnena polozka.
- [x] Premium-only item pro free uzivatele.
- [x] Equip item.
- [x] Equip item odvybavi predchozi polozku ve stejne kategorii.
- [x] Rarity badge.
- [x] Coin earning za level, boss, daily, achievement.
- [x] Concurrent purchase/spend edge case osetrit minimalne API helperem a UI validaci.

### Vlastni slovniky

- [x] Free user vidi premium gate.
- [x] Premium user vytvori slovnik.
- [x] Validace nazvu/popu.
- [x] Max 10 slovniku.
- [x] Detail slovniku.
- [x] Pridat slovo.
- [x] Validace slova: delka 3-20, nepovolene znaky, duplicita.
- [x] Import CSV.
- [x] Import TXT.
- [x] Import JSON.
- [x] Malformed import file.
- [x] Import nad 100 slov.
- [x] Public/private toggle.
- [x] Verejne slovniky.
- [x] Start game with custom dictionary.
- [x] Delete dictionary only by owner.

---

## T-915: Multiplayer, private rooms, tymy

### Quick Match

- [x] SignalR connection s JWT tokenem.
- [x] Join matchmaking.
- [x] Duplicate join je odmitnut.
- [x] Cancel matchmaking.
- [x] Dva uzivatele najdou match.
- [x] Preferuje podobny level.
- [x] Timeout po 30s.
- [x] Countdown 3-2-1.
- [x] Realtime game zobrazi oba hrace.
- [x] Spravna odpoved aktualizuje vlastni score.
- [x] Opponent progress event aktualizuje soupere.
- [x] Timer expiruje match.
- [x] Winner by correct count.
- [x] Tie by speed.
- [x] Forfeit da vyhru souperi.
- [x] Disconnect grace 30s.
- [x] Reconnect v grace obnovi match.
- [x] Result modal victory/defeat/draw.
- [x] Quick Match dava osobni XP i league XP.
- [x] Match history a stats.

### Private Rooms

- [x] Multiplayer landing ukaze Quick Match a Private Room.
- [x] Create room modal: word count 10/15/20.
- [x] Create room modal: time 2/3/5.
- [x] Create room modal: difficulty.
- [x] Create room modal: best of 1/3/5.
- [x] Neplatne settings maji validacni chyby.
- [x] Room code ma format `LEXIQ-XXXX`.
- [x] Copy room code.
- [x] Join valid room.
- [x] Join invalid code.
- [x] Join expired code.
- [x] Join full room.
- [x] Uzivatel nemuze mit dve aktivni mistnosti.
- [x] Host leave zrusi room.
- [x] Guest leave odebere guest.
- [x] Ready toggle.
- [x] Oba ready spusti countdown.
- [x] Lobby chat odesle zpravu.
- [x] Chat prazdna zprava odmitnuta.
- [x] Chat max 200 znaku.
- [x] Chat rate limit.
- [x] Chat XSS payload se zobrazi escapovane.
- [x] Best of 3 a 5 series score.
- [x] Private room nedava league XP.
- [x] Rematch request/accept/decline.
- [x] Room expiry a cleanup.

Overeno:
- `PrivateRoom_CreateModal_SelectsSettingsShowsRoomCodeAndCopiesIt` (Chromium 1366x900, light) pokryva create modal volby 10/15/20, 2/3/5, obtiznost, best of 1/3/5, route `/multiplayer/room/LEXIQ-XXXX`, zobrazeni kodu/nastaveni, clipboard copy a screenshot UX baseline `artifacts/e2e/screenshots/multiplayer/private-room-create-settings-code-copy/1366x900/light/host-lobby-code-settings.png`.
- `PrivateRoom_InvalidSettings_AreRejectedByHub` overuje, ze SignalR `CreateRoom` odmita neplatne settings ceskou validacni chybou a neposila `RoomCreated`.
- `PrivateRoom_JoinValidRoom_ShowsBothPlayersInLobby` overuje host create + guest join pres UI, route shodu, synchronizaci obou lobby a screenshot baseline `artifacts/e2e/screenshots/multiplayer/private-room-join-valid/1366x900/light/`.
- `PrivateRoom_JoinInvalidCode_ShowsValidationAndNotFoundErrors` overuje malformed kod v modalu, validne vypadajici neexistujici kod, ceskou not-found chybu a screenshot baseline `artifacts/e2e/screenshots/multiplayer/private-room-join-invalid-code/1366x900/light/not-found-error.png`.
- `PrivateRoom_JoinExpiredCode_ShowsExpiredError` overuje deterministickou E2E expiraci private room, ceskou expired chybu a screenshot baseline `artifacts/e2e/screenshots/multiplayer/private-room-join-expired-code/1366x900/light/expired-error.png`.
- `PrivateRoom_JoinFullRoom_ShowsFullError` overuje, ze treti hrac se do obsazene mistnosti nepripoji, vidi ceskou chybu `Místnost je plná` a screenshot baseline `artifacts/e2e/screenshots/multiplayer/private-room-join-full-room/1366x900/light/full-room-error.png`.
- `PrivateRoom_UserCannotCreateTwoActiveRooms` overuje paralelni dve stranky stejneho uctu, blokaci druhe aktivni mistnosti a screenshot baseline `artifacts/e2e/screenshots/multiplayer/private-room-single-active-room/1366x900/light/second-room-blocked.png`.
- `PrivateRoom_HostLeave_CancelsRoomAndNotifiesGuest` overuje host leave, navrat hosta na multiplayer, guest cancelled stav a screenshot baseline `artifacts/e2e/screenshots/multiplayer/private-room-host-leave-cancels-room/1366x900/light/guest-room-cancelled.png`.
- `PrivateRoom_GuestLeave_RemovesGuestAndHostReturnsToWaiting` overuje guest leave, navrat guest hrace na multiplayer, host lobby zpet s jednim hracem/kodem a screenshot baseline `artifacts/e2e/screenshots/multiplayer/private-room-guest-leave-removes-guest/1366x900/light/host-waiting-after-guest-left.png`.
- `PrivateRoom_ReadyToggle_SetsAndCancelsReadyState` overuje ready/cancel ready, SignalR synchronizaci ready badge do druheho browseru a screenshot baseline `artifacts/e2e/screenshots/multiplayer/private-room-ready-toggle/1366x900/light/host-ready-cancelled.png`.

### Tymy a klany

- [x] No-team state.
- [x] Create team premium zdarma.
- [x] Create team free za mince.
- [x] Create team free bez minci odmitnut.
- [x] Validace name 3-30.
- [x] Validace tag 2-4.
- [x] Duplicitni name/tag.
- [x] Team dashboard.
- [x] Invite member jako leader/officer.
- [x] Regular member invite odmitnut.
- [x] Join request.
- [x] Approve/reject request.
- [x] Kick member jako officer.
- [x] Nelze kicknout leadera.
- [x] Leave team.
- [x] Last member disbands team.
- [x] Transfer leadership.
- [x] Weekly ranking.
- [x] Role-based management options.

Overeno:
- `Teams_NoTeamState_ShowsEmptyActions` overuje noveho prihlaseneho uzivatele bez tymu, API `GET /api/v1/teams` jako 404, no-team UI texty, akce pro vytvoreni/hledani tymu, absenci dashboardu a screenshot baseline `artifacts/e2e/screenshots/teams/no-team-state/1366x900/light/empty-state.png`.
- `Teams_PremiumUser_CreatesTeamForFree` overuje premium uzivatele s 0 mincemi, modal s cenou `Zdarma pro Premium`, vytvoreni tymu pres UI, dashboard po vytvoreni, leader member row a API `GET /api/v1/teams` s vytvorenym tymem.
- `Teams_FreeUser_CreatesTeamForCoinsAndDeductsBalance` overuje ne-premium uzivatele s 1000 mincemi, modalovou cenu, vytvoreni tymu a API coin balance `0` po uspesnem vytvoreni.
- `Teams_FreeUserWithoutCoins_CreateTeamIsRejected` overuje ne-premium uzivatele s 0 mincemi, modalovou cenu, cesky error, absenci dashboardu, API `GET /api/v1/teams` jako 404 a coin balance `0`.
- `Teams_CreateTeamNameValidation_RequiresThreeToThirtyCharacters` overuje name required, min/max hranice a screenshot field-level chyby.
- `Teams_CreateTeamTagValidation_RequiresTwoToFourUppercaseAlphanumericCharacters` overuje tag required, min/max hranice, format A-Z/0-9 a screenshot field-level chyby.
- `Teams_CreateTeamDuplicateNameAndTag_ShowSpecificErrors` overuje duplicitni nazev i duplicitni tag proti existujicimu tymu, konkretni ceske modalove chyby, absenci dashboardu po odmítnutí a screenshot `artifacts/e2e/screenshots/teams/create-duplicate-name-tag/1366x900/light/duplicate-tag-error.png`.
- `Teams_Dashboard_ShowsStatsDescriptionMembersAndRoles` overuje seedovany tym se tremi rolemi, popisem, soucty weekly/all-time XP, vyhrami, API shodu clenu a screenshot `artifacts/e2e/screenshots/teams/dashboard-stats-members/1366x900/light/dashboard.png`.
- `Teams_LeaderAndOfficer_CanInviteMemberByUsername` overuje leader i officer invite podle username, vznik pending pozvanky pres API, viditelnou sekci `Moje pozvanky` u pozvaneho hrace a screenshoty invite modalu/no-team pozvanky.
- `Teams_RegularMember_CannotInviteMember` overuje, ze regular member nema invite akci v UI, primy API invite pokus konci BadRequest a cilovy hrac nema zadnou pozvanku.
- `Teams_NoTeamUser_CanCreateJoinRequestFromRanking` overuje ranking pro hrace bez tymu, join request modal se zpravou, vznik pending zadosti pro leadera a zachovani no-team stavu zadatele.
- `Teams_Leader_CanApproveAndRejectJoinRequests` overuje dve pending zadosti, leader schvaleni jedne zadosti, odmitnuti druhe zadosti, API clenstvi schvaleneho hrace a screenshoty `artifacts/e2e/screenshots/teams/approve-reject-join-requests/1366x900/light/pending-requests.png` a `after-approve-reject.png`.
- `Teams_Officer_CanKickRegularMember` overuje officera, ktery muze v UI vyhodit pouze bezneho clena, po akci vidi aktualni clenskou tabulku/stat karty, API potvrzuje odstraneni clena a screenshoty `artifacts/e2e/screenshots/teams/officer-kick-member/1366x900/light/before-kick.png` a `after-kick.png`.
- `Teams_LeaderCannotBeKicked` overuje, ze leader nema v UI kick akci, officer nemuze leadera vyhodit pres API, leader nemuze vyhodit sam sebe a screenshot `artifacts/e2e/screenshots/teams/leader-cannot-be-kicked/1366x900/light/leader-protected.png` potvrzuje zachovane role.
- `Teams_MemberCanLeaveTeam` overuje odchod bezneho clena pres UI, API `GET /api/v1/teams` jako 404 pro odchoziho clena, zachovani leadera v tymu a screenshoty `artifacts/e2e/screenshots/teams/member-leave-team/1366x900/light/before-leave.png` a `after-leave-empty.png`.
- `Teams_LastMemberDisbandsTeam` overuje solo leadera, UI disband akci, API 404 pro muj tym i detail smazaneho tymu po rozpušteni a screenshoty `artifacts/e2e/screenshots/teams/last-member-disbands-team/1366x900/light/before-disband.png` a `after-disband-empty.png`.
- `Teams_LeaderCanTransferLeadershipToMember` overuje leader transfer modal, kandidaty bez aktualniho leadera, spravny API payload, prohozeni roli, zmizeni leader-only akci u puvodniho leadera a screenshoty `artifacts/e2e/screenshots/teams/transfer-leadership/1366x900/light/modal-open.png` a `after-transfer.png`.
- `Teams_WeeklyRankingOrdersTeamsByWeeklyXp` overuje API a UI tymovy zebricek serazeny podle tydenniho XP, ranky, clenstvi, join CTA pro hrace bez tymu a screenshot `artifacts/e2e/screenshots/teams/weekly-ranking/1366x900/light/ordered-by-weekly-xp.png`.
- `Teams_LeaderRole_ShowsLeaderManagementOptions`, `Teams_OfficerRole_ShowsOfficerManagementOptions` a `Teams_MemberRole_ShowsMemberManagementOptions` overuji role-based management UI pro leadera/officera/clena: leader vidi pozvani, predani vedeni, rozpusteni a kick pouze ne-leaderu; officer vidi pozvani, odchod a kick pouze bezneho clena; member vidi jen odchod bez management akci. Screenshoty `artifacts/e2e/screenshots/teams/role-based-management-options/1366x900/light/leader-actions.png`, `officer-actions.png` a `member-actions.png` potvrzuji i vizualni stav.

---

## T-916: Notifikace, admin, AI, performance/security

### Notifikace

- [x] Notification bell ukaze unread count.
- [x] Dropdown seskupi Today/Yesterday/This Week.
- [x] Mark one read.
- [x] Mark all read.
- [x] Preferences load/save.
- [x] Push permission dialog pri zapnuti push.
- [x] Push disabled respektuje preference.
- [x] Email disabled neposle email.
- [x] Streak warning email/push/in-app.
- [x] Daily challenge reminder.
- [x] Achievement unlocked toast + notification.
- [x] Premium expiry email zachycen ve smtp4dev.
- [x] Frequency limit max 5/h.

Overeno:
- `Notifications_BellDropdownGroupingAndReadActions_WorkEndToEnd` seeduje pres SQL Server Testcontainer dnesni/vcerejsi/tydenni/starsi notifikace, overuje unread badge `3`, dropdown skupiny `Dnes`/`Včera`/`Tento týden`/`Starší`, klik na jednu notifikaci se snizenim unread countu na `2`, `Označit vše jako přečtené` se zmizenim badge a API `GET /api/v1/notifications/unread-count` jako `0`. Screenshoty `artifacts/e2e/screenshots/notifications/bell-dropdown-read-actions/1366x900/light/grouped-unread.png`, `after-mark-one-read.png` a `after-mark-all-read.png` byly zkontrolovany z pohledu UX; behem review byla nalezena a opravena `E2E-BUG-0148`.
- `Notifications_PreferencesLoadAndSave_WorkEndToEnd` nastavuje pres API notifikacni preference noveho uzivatele na `PushEnabled=false`, `EmailEnabled=false`, `LeagueUpdates=false`, `AchievementNotifications=false`, `DailyChallengeReminder=false` a cas `19:30`, overuje jejich propsani do `/settings`, potom pres UI zapina vsechny notifikacni volby, uklada cas `06:15` a pres API `GET /api/v1/notifications/preferences` potvrzuje ulozene hodnoty. Screenshoty `artifacts/e2e/screenshots/notifications/preferences-load-save/1366x900/light/loaded.png` a `saved.png` byly zkontrolovany z pohledu UX; behem implementace byly nalezeny a opravene `E2E-BUG-0149` a `E2E-BUG-0150`. Regression kryti doplnuji bUnit testy `SettingsPage_Preferences_LoadsNotificationPreferences` a `SettingsPage_Preferences_SaveUpdatesNotificationPreferences`.
- `Notifications_PushEnable_RequestsPermissionAndStoresSubscription` startuje s `PushEnabled=false`, v `/settings` zapina push toggle, pres deterministicky E2E stub overuje zavolani `window.lexiQuestPush.requestSubscription`, ulozeni `PushSubscription` do SQL Server Testcontainer databaze, ulozeni preferenci a API potvrzeni `PushEnabled=true`. Screenshot `artifacts/e2e/screenshots/notifications/push-permission-enable/1366x900/light/enabled.png` byl zkontrolovan z pohledu UX; push toggle je zapnuty, layout zustava citelny a toast potvrzuje ulozeni. Regression kryti doplnuje bUnit test `SettingsPage_PushNotifications_EnableRequestsAndStoresSubscription`; behem implementace byla opravena `E2E-BUG-0151`.
- `Notifications_PushDisabled_DoesNotSendPushButKeepsInAppNotification` uklada push subscription na lokalni E2E HTTP endpoint, nastavuje `PushEnabled=false` a `EmailEnabled=false`, odesila notifikaci pres E2E hook skutecneho `NotificationService`, overuje nulovy pocet POSTu na push endpoint, existenci unread in-app notifikace a screenshot `artifacts/e2e/screenshots/notifications/push-disabled-respects-preference/1366x900/light/in-app-only.png`. Screenshot potvrzuje citelny dropdown s badge `1` a notifikaci `Push vypnutý`.
- `Notifications_EmailDisabled_DoesNotSendEmailButKeepsInAppNotification` cisti smtp4dev, nastavuje `EmailEnabled=false`, odesila systemovou notifikaci pres E2E hook skutecneho `NotificationService`, overuje ze smtp4dev neobsahuje predmet ani email uzivatele, potvrzuje unread in-app notifikaci a screenshot `artifacts/e2e/screenshots/notifications/email-disabled-respects-preference/1366x900/light/in-app-only.png`. Screenshot potvrzuje badge `1`, citelny dropdown a notifikaci `Email vypnutý`; behem implementace byla opravena `E2E-BUG-0154`.
- `Notifications_StreakWarningJob_SendsEmailPushAndInAppNotification` nastavuje aktivni streak s posledni aktivitou vcera, zapina `PushEnabled=true`, `EmailEnabled=true` a `StreakReminder=true`, uklada push subscription na lokalni E2E HTTP endpoint, spousti realny `StreakReminderJob` pres E2E endpoint a overuje Web Push POST payload, smtp4dev email na skutecnou adresu uzivatele, unread in-app notifikaci a screenshot `artifacts/e2e/screenshots/notifications/streak-warning-email-push-in-app/1366x900/light/all-channels.png`. Screenshot potvrzuje ceske texty `Streak je v ohrožení`, badge `1`, citelny dropdown a dashboard streak stav `Ve hrozbě`; behem implementace byly opraveny `E2E-BUG-0152` a `E2E-BUG-0153`.
- `Notifications_DailyChallengeReminderJob_SendsEmailPushAndInAppNotification` zapina `PushEnabled=true`, `EmailEnabled=true` a `DailyChallengeReminder=true`, uklada push subscription na lokalni E2E HTTP endpoint, spousti realny `DailyChallengeReminderJob` pres E2E endpoint a overuje Web Push POST payload, smtp4dev email na skutecnou adresu uzivatele, unread in-app notifikaci a screenshot `artifacts/e2e/screenshots/notifications/daily-challenge-reminder-email-push-in-app/1366x900/light/all-channels.png`. Screenshot potvrzuje ceske texty `Denní výzva je připravena`, badge `1`, citelny dropdown a neoriznutou dvouradkovou zpravu; behem implementace byla opravena `E2E-BUG-0155`.
- `Notifications_AchievementUnlocked_ShowsToastAndInAppNotification` zapina `AchievementNotifications=true`, odemkne prvni achievement ve hre, overuje toast `Úspěch odemčen: První slovo`, achievement modal, unread `AchievementUnlocked` notifikaci s action URL `/achievements`, notification bell badge `1` a dropdown. Screenshoty `artifacts/e2e/screenshots/notifications/achievement-unlocked-toast-notification/1366x900/light/toast-and-modal.png` a `notification-dropdown.png` byly zkontrolovany z pohledu UX; toast je po oprave dole vpravo a neblokuje topbar ani dropdown, loading stav submit tlacitka se po vysledku resetuje a HUD se nelepi do jednoho retezce, viz `E2E-BUG-0157`, `E2E-BUG-0160`, `E2E-BUG-0161` a `E2E-BUG-0162`.
- `Premium_ExpiryReminderEmail_IsCapturedBySmtp4Dev` nastavuje aktivni mesicni premium subscription s expiraci do 3 dnu, spousti realny `PremiumExpiryReminderJob` pres E2E endpoint, overuje cesky email `Premium brzy vyprší` zachyceny ve smtp4dev na skutecnou adresu uzivatele a zobrazuje aktivni premium stav v UI. Screenshot `artifacts/e2e/screenshots/premium/expiry-reminder-email-smtp4dev/1366x900/light/active-expiring-soon.png` potvrzuje badge `Aktivní Premium - Měsíční`, aktivni premium funkce a konzistentni stav planu; behem implementace byly opraveny `E2E-BUG-0158` a `E2E-BUG-0159`.
- `Notifications_FrequencyLimit_MaxFivePerHour_SuppressesSixthDelivery` posle 6 league notifikaci behem jedne hodiny pres realny `NotificationService`; prvnich 5 projde do SQL Server Testcontainer DB, Web Push endpointu i smtp4dev, sesta je potlacena. Test overuje push count `5`, absenci emailu `Limit liga 6`, API unread count `5` a dropdown bez seste notifikace. Screenshot `artifacts/e2e/screenshots/notifications/frequency-limit-max-five-per-hour/1366x900/light/five-notifications-only.png` potvrzuje badge `5` a pet zobrazenych notifikaci.

### Admin panel

- [x] Non-admin route guard redirect/forbidden.
- [x] Admin dashboard stats cards.
- [x] Word management table.
- [x] Search/filter/pagination/column picker.
- [x] Create word.
- [x] Edit word.
- [x] Delete word confirm.
- [x] Bulk import CSV.
- [x] Import duplicates skipped.
- [x] Export CSV.
- [x] Stats drawer.
- [x] User management table.
- [x] User detail drawer.
- [x] Suspend user.
- [x] Unsuspend user.
- [x] Reset password posle email do smtp4dev.
- [x] ContentManager ma pristup ke slovum, ne k admin sprave uzivatelu.

- `Admin_NonAdminRouteGuard_RedirectsAndApiForbids` overuje, ze bez admin role API `GET /api/v1/admin/dashboard/stats` vraci 403 a UI route `/admin` uzivatele presmeruje mimo admin shell bez konzolove chyby z route guardu. Screenshot `artifacts/e2e/screenshots/admin/non-admin-route-guard/1366x900/light/redirected-home.png` potvrzuje, ze se nezobrazi admin rozhrani; behem implementace byla opravena `E2E-BUG-0163`.
- `Admin_DashboardStatsCards_ShowRealCounts` priradi uzivateli roli `Admin` primo v SQL Server Testcontainer DB, overi realne dashboard statistiky pres API, porovna hodnoty v UI kartach a ulozi screenshot `artifacts/e2e/screenshots/admin/dashboard-stats-cards/1366x900/light/stats-cards.png`. Screenshot UX review potvrzuje citelne metriky a rychle akce; behem implementace byly opraveny `E2E-BUG-0163`, `E2E-BUG-0164` a infra problem `E2E-BUG-0165`.
- `AdminWords_TableSearchFilterPaginationColumnPicker_WorkEndToEnd` prihlasuje admina, overuje tabulku slov, 25 radku na prvni strane, prechod na dalsi stranu, reset na prvni stranu, hledani `programovani`, filtr delky 10-12 a skryti sloupce kategorie pres column picker. Screenshot `artifacts/e2e/screenshots/admin/words-table-filter-pagination-columns/1366x900/light/filtered-column-picker.png` potvrzuje citelny filtracni blok, stabilni tabulku a zadne prekryvy; behem implementace byly opraveny `E2E-BUG-0166`, `E2E-BUG-0168`, `E2E-BUG-0169` a `E2E-BUG-0170`.
- `AdminWords_CreateEditDelete_WorkEndToEnd` vytvori unikatni slovo, overi jeho propsani do tabulky, upravi kategorii/obtiznost, znovu vyhleda upraveny stav a otevira potvrzovaci modal mazani. Screenshot `artifacts/e2e/screenshots/admin/words-create-edit-delete/1366x900/light/delete-confirm.png` byl zkontrolovan z pohledu UX; destruktivni akce je jasna a toasty neprekryvaji rozhodujici tlacitka. Behem implementace byly opraveny `E2E-BUG-0166`, `E2E-BUG-0167`, `E2E-BUG-0168` a `E2E-BUG-0170`.
- `AdminWords_ImportDuplicatesExportAndStats_WorkEndToEnd` importuje CSV se slovem `pes` jako duplicitou a dvema novymi slovy, overuje vysledek `Importováno: 2, přeskočeno: 1, chyby: 0`, stahuje CSV export do artefaktu `artifacts/e2e/downloads/admin/words-import-export-stats/words-export.csv` a kontroluje stats drawer s celkem `107` slovy a rozdelenim podle obtiznosti. Screenshoty `artifacts/e2e/screenshots/admin/words-import-export-stats/1366x900/light/import-result.png` a `stats-drawer.png` potvrzuji citelny modal, spravne pokryty backdrop a prehledny drawer; behem implementace byly opraveny `E2E-BUG-0166`, `E2E-BUG-0169` a `E2E-BUG-0170`.
- Overeni admin word-managementu: `dotnet build src/LexiQuest.Web/LexiQuest.Web.csproj --no-restore -m:1 /p:BuildInParallel=false /v:q` probehl uspesne a `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --no-build --filter "FullyQualifiedName~AdminWords_" -m:1 /p:BuildInParallel=false /v:m` probehl uspesne 3/3. Dashboard/guard regressions znovu prosly filtrem `FullyQualifiedName~Admin_` 2/2.
- `AdminUsers_TableDetailSuspendUnsuspendResetPassword_WorkEndToEnd` vytvori admina a spravovaneho uzivatele, nastavi mu level `7`, XP `1234`, streak `5`, vyfiltruje ho v tabulce podle emailu a levelu, otevre detail drawer, pozastavi a znovu aktivuje ucet pres realne API/UI flow a posle reset hesla do smtp4dev. Screenshoty `artifacts/e2e/screenshots/admin/users-table-detail-suspend-reset/1366x900/light/filtered-table.png`, `detail-drawer.png` a `reset-password-sent.png` potvrzuji citelnou tabulku, drawer bez prekryvu a viditelne potvrzeni resetu; smtp4dev overeni kontroluje prijemce, subject `Obnovení hesla - LexiQuest`, reset link a E2E web base URL. Behem implementace byly opraveny `E2E-BUG-0171`, `E2E-BUG-0173` a `E2E-BUG-0174`.
- `Admin_ContentManager_CanManageWordsButCannotManageUsers` priradi uzivateli roli `ContentManager`, overi API pristup `200 OK` na `/api/v1/admin/words`, `403 Forbidden` na `/api/v1/admin/users`, viditelnou UI route `/admin/words` a presmerovani mimo `/admin/users`. Screenshot `artifacts/e2e/screenshots/admin/content-manager-role-boundary/1366x900/light/words-access.png` potvrzuje, ze ContentManager vidi spravu slov, ale nedostane se do administrace uzivatelu; behem implementace byla opravena `E2E-BUG-0172`.
- Overeni admin user-managementu: `dotnet build src/LexiQuest.Web/LexiQuest.Web.csproj --no-restore -m:1 /p:BuildInParallel=false /v:q` a `dotnet build tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false /v:q` probehly uspesne. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --no-build --filter "FullyQualifiedName~AdminUsers_|FullyQualifiedName~Admin_ContentManager" -m:1 /p:BuildInParallel=false /v:m` probehl uspesne 2/2.

### AI vyzvy

- [x] Analysis zobrazi slaba pismena.
- [x] Analysis zobrazi pomale kategorie.
- [x] Empty/no-history state.
- [x] Weakness Focus start.
- [x] Speed Training start.
- [x] Memory Game start.
- [x] Pattern Recognition start.
- [x] Session reuse GameArena.
- [x] Tooltip "proc toto slovo" je viditelny.
- [x] Personalizovana challenge meni vyber podle historie.

- `AIChallenge_NoHistory_ShowsEmptyAnalysisAndChallengeCards` prihlasuje noveho uzivatele bez herni historie, overuje prazdna slaba pismena, prazdny vykon podle kategorii, neutralni no-data tip, 4 AI challenge karty, preview slova a screenshot `artifacts/e2e/screenshots/ai-challenge/no-history-empty-state/1366x900/light/empty-data.png`. Screenshot byl zkontrolovan z pohledu UX; prazdny stav nelze zaměnit za uspech hrace a karty jsou citelne bez prekryvu. Behem implementace byla opravena `E2E-BUG-0182`.
- `AIChallenge_Analysis_ShowsWeakLettersSlowCategoriesAndCzechTips` seeduje spatne a pomale odpovedi pres realnou hru v SQL Server Testcontainer DB, overuje slabe pismeno `A`, kategorii `Expert`, prumerny cas, ceske tipy a absenci raw anglickych textu. Screenshot `artifacts/e2e/screenshots/ai-challenge/analysis-weakness-and-slow-category/1366x900/light/analysis.png` potvrzuje prehlednou analyzu, citelne progress bary a bezpecne zalamani textu; behem implementace byla opravena `E2E-BUG-0178`.
- `AIChallenge_StartType_ReusesGameArenaAndShowsWhyThisWordTooltip` bezi pro `WeaknessFocus`, `SpeedTraining`, `MemoryGame` a `PatternRecognition`, overuje ceske nazvy misto raw enum hodnot, preview slova, tooltip `Proč toto slovo`, realny start session pres `GameMode.AIChallenge`, navigaci na `/game/{sessionId}`, GameArena, spatnou odpoved a session feedback. Screenshoty `challenge-cards-with-tooltip.png`, `game-arena.png` a `session-feedback.png` jsou ulozene pod `artifacts/e2e/screenshots/ai-challenge/start-*/1366x900/light/`; UX review potvrzuje citelne karty, viditelny tooltip, funkcni ikony a znovupouzitou herni arenu. Behem implementace byly opraveny `E2E-BUG-0176`, `E2E-BUG-0177`, `E2E-BUG-0179`, `E2E-BUG-0180` a `E2E-BUG-0181`.
- `AIChallenge_WeaknessFocus_ChangesWordReasonsAfterHistory` vola AI challenge API primo s autentizaci, porovnava uzivatele bez historie s uzivatelem se slabou historii a overuje, ze no-history challenge pouziva obecny trening, zatimco personalizovana challenge vraci duvody se slabym pismenem. Behem implementace byla opravena `E2E-BUG-0175`.
- Overeni AI vyzev: `dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj --no-restore --filter "FullyQualifiedName~AIChallengeServiceTests" -m:1 /p:BuildInParallel=false /v:m` probehl uspesne 8/8. `dotnet build src/LexiQuest.Api/LexiQuest.Api.csproj --no-restore -m:1 /p:BuildInParallel=false /v:q`, `dotnet build tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false /v:q` a `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --no-build --filter "FullyQualifiedName~AIChallenge" -m:1 /p:BuildInParallel=false /v:m` probehly uspesne; finalni AI E2E beh mel vysledek 7/7.

### Security a edge inputy pres UI

- [x] SQL injection-like vstupy ve vsech search/form polich nezpusobi chybu ani neobejdou validaci.
- [x] XSS payloady ve vsech textovych polich se zobrazi escapovane.
- [x] Velmi dlouhe stringy nezbouraji UI.
- [x] File upload odmita spatny typ a prilis velky soubor.
- [x] Public endpoint rate limiting se projevi srozumitelnym UI stavem.
- [x] Protected data jineho uzivatele nejsou dostupna pres URL manipulaci.

- `Security_AdminSearchInputs_SqlLikePayloadsDoNotErrorOrBypassFilters` overuje SQL-like payload `"' OR 1=1;--"` v admin search polich pro slova i uzivatele, kontroluje, ze filtr nevrati bezna data, Blazor nespadne a UI skonci v citelnem prazdnem stavu. Screenshot `artifacts/e2e/screenshots/security-edge/admin-search-sql-like-inputs/1366x900/light/safe-empty-filter-result.png` potvrzuje bezpecny empty state bez prekryvu.
- `Security_DictionaryInputs_EscapeXssClampLongStringsAndRejectBadFiles` vytvari slovnik s XSS payloadem v nazvu/popisu, overuje escapovane zobrazeni bez spusteni `onerror`, browser maxlength pro 100/500 znaku, absenci horizontalniho overflow a odmítnuti `.exe` i CSV > 1 MB. Screenshot `artifacts/e2e/screenshots/security-edge/dictionary-inputs-xss-long-files/1366x900/light/invalid-import-file.png` potvrzuje srozumitelnou file-upload chybu a opraveny import modal; behem implementace byly opraveny `E2E-BUG-0183` a `E2E-BUG-0184`.
- `Security_PrivateDictionaryIdManipulation_DoesNotExposeOtherUserData` vytvori privátní slovnik vlastnika, jako druhy uzivatel zkousi zmanipulovane `GET/POST/DELETE api/v1/dictionaries/{id}` a `POST api/v1/game/start` s cizim `CustomDictionaryId`, overuje `404/403/400` a UI `/dictionaries?dictionaryId={id}` bez zobrazeni ciziho slovniku. Screenshot `artifacts/e2e/screenshots/security-edge/private-dictionary-owner-boundary/1366x900/light/private-dictionary-not-visible.png` potvrzuje, ze verejny seznam zustava prazdny.
- Rate limiting je overeny existujicimi E2E `GuestLimit_FifthGameAllowedAndSixthGameShowsRegistrationCta` a `PrivateRoom_LobbyChat_RateLimit_ShowsLocalizedErrorAndKeepsLobby`; aktualni re-run obou testu probehl uspesne 2/2 a UI ukazuje ceske limit stavy bez rozpadu lobby/guest flow.
- Overeni security sekce: `dotnet build src/LexiQuest.Web/LexiQuest.Web.csproj --no-restore -m:1 /p:BuildInParallel=false /v:q`, `dotnet build tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false /v:q`, `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --no-build --filter "FullyQualifiedName~Security_" -m:1 /p:BuildInParallel=false /v:m` a targeted rate-limit filtr probehly uspesne.

---

## T-917: Accessibility, responzivita, performance

- [x] Spustit a11y audit pro vsechny hlavni route.
- [x] Overit tab order na landing, auth, game, settings, admin a multiplayer.
- [x] Overit focus trap ve vsech modalech.
- [x] Overit `aria-live` pro timer, score, feedback, notification count.
- [x] Overit labels/error descriptions u formularu.
- [x] Overit keyboard-only hratelnost zakladni hry.
- [x] Overit mobile navigation menu.
- [x] Overit zadny horizontalni overflow na mobile/tablet.
- [x] Overit desktop wide layout nezanecha prazdne/rozbite oblasti.
- [x] Zachytit console errors, page errors a failed network requests ve vsech testech.
- [x] Lighthouse/performance smoke: landing, dashboard, game.
- [x] Bundle/static asset cache overit na PWA smoke testu.

- `A11y_MainRoutes_HaveLabelsMetadataAndNoBasicAuditIssues` prochazi public route `/`, `/login`, `/register`, `/play` a prihlasene route `/dashboard`, `/game`, `/settings`, `/admin/words`, `/multiplayer`; spousti existujici `RunA11yCheckAsync`, kontroluje cesky `lang`, title, labely formularu, alt texty a zadny horizontalni overflow. Behem implementace byla opravena `E2E-BUG-0187`.
- `A11y_TabOrder_ReachesPrimaryControlsAcrossCoreRoutes` overuje tab order na landing/auth a pres novy skip link `Přeskočit na obsah` take na game, settings, admin a multiplayer. Behem implementace byla opravena `E2E-BUG-0188`.
- `A11y_ModalFocusTrap_KeepsKeyboardInsideDialog` overuje, ze Tab/Shift+Tab zustava uvnitr modalniho dialogu; globalni focus trap v `lexiQuestA11y` pokryva viditelne `[aria-modal="true"]` modaly. Behem implementace byla opravena `E2E-BUG-0189`.
- `A11y_GameAndNotificationLiveRegions_AreAnnounced` overuje `aria-live` pro notification count, game timer a game feedback. Behem implementace byla opravena `E2E-BUG-0186`.
- `A11y_GameKeyboardOnly_AllowsAnswerWithEnter` spousti realnou training session pres API, fokusuje answer input, pise odpoved klavesnici a potvrzuje Enterem bez mysi.
- `A11y_MobileNavigation_MenuIsReachableAndNavigates` overuje mobilni hamburger/drawer a navigaci na `/settings`; screenshoty `artifacts/e2e/screenshots/accessibility-performance/mobile-navigation-menu/375x812/light/menu-open.png` a `settings-after-menu-navigation.png` byly rucne zkontrolovane z pohledu UX. Behem implementace byla opravena `E2E-BUG-0185`.
- `Performance_LandingDashboardAndGame_LoadWithinSmokeBudget` meri smoke load budget pro landing, dashboard a game start screen a kontroluje Navigation Timing/resource entries jako lehkou nahradu Lighthouse smoke v E2E prostredi.
- `Pwa_StaticAssets_AreAvailableFromServiceWorkerCache` overuje service-worker Cache Storage pro staticke assety `/manifest.json`, `/css/app.css` a `/icon-192.png`.
- Overeni T-917: `dotnet build src/LexiQuest.Web/LexiQuest.Web.csproj --no-restore -m:1 /p:BuildInParallel=false /v:q`, `dotnet build tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false /v:q` a `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --no-build --filter "FullyQualifiedName~AccessibilityPerformanceE2ETests" -m:1 /p:BuildInParallel=false /v:m` probehly uspesne; finalni class beh mel vysledek 8/8.

---

## T-918: CI a spousteci prikazy

- [x] Pridat CI job `e2e-smoke` pro PR.
- [x] Pridat manual/ nightly CI job `e2e-full`.
- [x] Pridat manual/ nightly CI job `e2e-visual`.
- [x] Publikovat artefakty: screenshots, traces, videos, API logs, Web logs, container logs.
- [x] Dokumentovat lokalni prikazy:
  - [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "Category=Smoke"`
  - [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "Category=Full"`
  - [x] `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "Category=Visual"`
- [x] Pridat env pro debug:
  - [x] `E2E_HEADLESS=false`
  - [x] `E2E_KEEP_CONTAINERS=true`
  - [x] `E2E_TRACE=on`
  - [x] `E2E_SLOWMO_MS=100`

---

## Prubezne odskrtavani

Tento soubor je zivy implementacni checklist. Pri implementaci se budou polozky odskrtavat prubezne. Kdyz test odhali chybu aplikace nebo UX, zalozi se polozka v `todo/E2E-Nalezene-Chyby.md`; az potom se opravi kod a znovu spusti prislusny test/screenshot.
