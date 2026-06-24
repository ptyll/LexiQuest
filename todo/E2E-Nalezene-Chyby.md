# E2E nalezene chyby

> **Ucel:** Evidence vsech chyb nalezenych pri Playwright E2E testech, screenshot testech a UX review screenshotu.
> **Pravidlo:** Zadna nalezena chyba se neopravuje "potichu". Nejdriv se zapise sem, po oprave se oznaci jako opravena a po opakovanem testu jako overena.

---

## Stavove hodnoty

| Stav | Vyznam |
|------|--------|
| `Nova` | Chyba nalezena, jeste neni potvrzena minimalni reprodukci. |
| `Reprodukovana` | Chyba ma jasny test nebo rucni reprodukcni postup. |
| `V oprave` | Prave probiha oprava. |
| `Opraveno` | Kod/test data byly upraveny, ale jeste neprobehlo overeni. |
| `Overeno` | Prislusny E2E/screenshot test znovu probehl a chyba se nevraci. |
| `Odlozeno` | Vedome odlozeno se zduvodnenim. |
| `Neni chyba` | Ukazalo se, ze jde o ocekavane chovani nebo chybu testu. |

## Severity

| Severity | Vyznam |
|----------|--------|
| `P0` | Blokuje spusteni aplikace, registraci/login, hlavni herni flow, nebo zpusobuje ztratu dat/bezpecnostni problem. |
| `P1` | Rozbita klicova funkce, platby, emaily, multiplayer, admin, nebo zasadni UX na hlavnim toku. |
| `P2` | Edge case, responzivita, a11y, vizualni regres, mene caste flow. |
| `P3` | Kosmetika, text, drobny polish bez funkcniho dopadu. |

---

## Sablona pro novy nalez

```markdown
### E2E-BUG-0000: Kratky nazev

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Auth / Game / Premium / Multiplayer / Admin / UX / A11y / Infra
- **Nalezeno v testu:** `NazevTestu`
- **Screenshot/trace:** `artifacts/e2e/...`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, browser/viewport/theme
- **Reprodukce:**
  1. ...
  2. ...
  3. ...
- **Ocekavani:** ...
- **Skutecnost:** ...
- **Pravdepodobna pricina:** ...
- **Oprava:** ...
- **Overeni:** ...
- **Poznamky:** ...
```

---

## Aktivni nalezy

### E2E-BUG-0222: Landing guest CTA test cekal na prilis krehky URL pattern

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Landing / Guest / Test stabilita
- **Nalezeno v testu:** `LandingE2ETests.LandingPage_GuestCta_NavigatesToGuestPlay`
- **Screenshot/trace:** `artifacts/e2e/failures/landing/guest-cta/20260621-115655.png`, trace `artifacts/e2e/traces/landing-guest-cta-20260621-115655.zip`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Spustit kompletni E2E sadu.
  2. Otevrit landing page a kliknout na guest CTA.
  3. Test ceka na URL pattern `**/play`.
- **Ocekavani:** Test pocka na realny guest welcome stav a overi, ze uzivatel skoncil na `/play`.
- **Skutecnost:** Screenshot po timeoutu uz ukazoval guest welcome stranku, ale URL wait nedobehl v limitu.
- **Pravdepodobna pricina:** Test pouzival krehke cekani na URL navigaci misto stabilniho UI stavu Blazor route.
- **Oprava:** Test po kliknuti ceka na `guest-welcome` a URL `/play` kontroluje az potom jako doplnek.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Pwa_OfflineTraining_UsesCachedSeedAndReplaysQueuedAnswer|FullyQualifiedName~LandingPage_GuestCta_NavigatesToGuestPlay" -m:1 /p:BuildInParallel=false /v:m` prosel 2/2. Kompletni E2E run `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj -m:1 /p:BuildInParallel=false /v:m` prosel 287/287 za 33 m 39 s.
- **Poznamky:** Jde o chybu E2E testu, ne produktovou regresi.

### E2E-BUG-0221: Offline training replay muze pri soubeznem online eventu odeslat stejnou odpoved dvakrat

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** PWA / Offline queue / Game
- **Nalezeno v testu:** `PwaE2ETests.Pwa_OfflineTraining_UsesCachedSeedAndReplaysQueuedAnswer`
- **Screenshot/trace:** `artifacts/e2e/failures/pwa/offline-training-queue/20260621-114308-console.log`, trace `artifacts/e2e/traces/pwa-offline-training-queue-20260621-114308.zip`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Spustit offline training PWA E2E.
  2. Odpovedet offline a ulozit jednu polozku do `lexiquest_offline_game_queue`.
  3. Prepnout browser zpet online.
- **Ocekavani:** Offline fronta se replayne jednou, queue se smaze a konzole neobsahuje `400 Bad Request`.
- **Skutecnost:** API log ukazal dva rychle POSTy na `api/v1/game/{sessionId}/answer`; jeden replay uspel, druhy narazil na uz dokoncenou roundu a vratil 400, coz se propsalo jako console error.
- **Pravdepodobna pricina:** `OfflineBanner` muze dostat vice online eventu a `GameService.ReplayQueuedRequestsAsync` nemel reentrancy guard proti soubeznemu cteni stejne localStorage fronty.
- **Oprava:** `GameService.ReplayQueuedRequestsAsync` chrani replay fronty pres interlocked guard, takze soubezny druhy replay okamzite skonci bez duplicitniho POSTu.
- **Overeni:** Unit/service test `GameService_ReplayQueuedRequests_ConcurrentCalls_DoNotSubmitSameQueuedAnswerTwice` nejdriv padal na 2 POSTech a po oprave prosel 1/1. Navazny E2E run `Pwa_OfflineTraining_UsesCachedSeedAndReplaysQueuedAnswer|LandingPage_GuestCta_NavigatesToGuestPlay` prosel 2/2 bez console erroru. Kompletni E2E run `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj -m:1 /p:BuildInParallel=false /v:m` prosel 287/287 za 33 m 39 s.
- **Poznamky:** Screenshot `artifacts/e2e/screenshots/pwa/offline-training-queue/1366x900/light/replayed.png` potvrzoval funkcni replay, chyba byla v cistem bezchybovem online flushi.

### E2E-BUG-0220: Password reset E2E pouzival nejednoznacny dashboard selector

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Email / Password reset / Test stabilita
- **Nalezeno v testu:** `EmailE2ETests.PasswordReset_LinkChangesPasswordAndOldPasswordStopsWorking`
- **Screenshot/trace:** full E2E run, Playwright strict mode violation na `GetByText("Úroveň 1")`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Spustit kompletni E2E sadu.
  2. Projit password-reset flow az na dashboard po prihlaseni novym heslem.
  3. Test vyhleda obecny text `Úroveň 1`.
- **Ocekavani:** Test overuje konkretni dashboard prvky stabilnimi `data-testid` selektory.
- **Skutecnost:** `GetByText("Úroveň 1")` nasel dve shody: statistiku celkovych XP a XP bar, takze Playwright strict mode test ukoncil.
- **Pravdepodobna pricina:** Dashboard po rozsireni legitimne zobrazuje stejnou informaci na vice mistech, zatimco test pouzival globalni textovy selector.
- **Oprava:** Test kontroluje `dashboard-total-xp-stat` pro text `Celkové XP` a `xp-bar-level` pro presnou hodnotu `Úroveň 1`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~EmailE2ETests.PasswordReset_LinkChangesPasswordAndOldPasswordStopsWorking" -m:1 /p:BuildInParallel=false /v:m` prosel 1/1. Kompletni E2E run `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj -m:1 /p:BuildInParallel=false /v:m` prosel 287/287 za 33 m 39 s.
- **Poznamky:** Jde o chybu E2E testu, ne produktovou regresi.

### E2E-BUG-0219: Tymova stranka mela misto skeleton loadingu osamoceny spinner s rozbitym rozlozenim

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Teams / Loading states / UX / Screenshot testy
- **Nalezeno v testu:** `LayoutE2ETests.Layout_MainPages_ShowSkeletonLoadingStates`
- **Screenshot/trace:** puvodni screenshot `artifacts/e2e/screenshots/layout/main-pages-loading-states/1366x900/light/team.png` pred opravou ukazoval jen spinner daleko vlevo; finalni screenshot stejné cesty po oprave ukazuje skeleton dashboard.
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. V E2E pozdrzet dalsi API request `/api/v1/teams/my`.
  2. Otevrit `/team` jako prihlaseny uzivatel.
  3. Ulozit loading checkpoint `layout/main-pages-loading-states/team`.
- **Ocekavani:** Team loading stav pouziva skeleton rozlozeni, ktere predznacuje budouci obsah a zustava zarovnane v hlavnim panelu.
- **Skutecnost:** Loading stav zobrazil pouze velky spinner izolovane vlevo od centrovaneho titulku `Tým`, takze stranka pusobila prazdne a vizualne rozpadle.
- **Pravdepodobna pricina:** `Team.razor` pouzival `TmSpinner` bez kontejneru reprezentujiciho budouci dashboard/ranking layout.
- **Oprava:** `Team.razor` pouziva `team-loading-skeleton` se skeletonem hlavicky, tri stat karet a hlavni tabulky/panelu; CSS pridava responzivni grid a konzistentni mezery.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Layout_MainPages_ShowSkeletonLoadingStates" -m:1 /p:BuildInParallel=false /v:m` prosel 1/1 a finalni screenshot `team.png` byl vizualne zkontrolovan. `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~TeamPageTests" -m:1 /p:BuildInParallel=false /v:m` prosel 5/5.
- **Poznamky:** Stejny E2E test zavedl obecny E2E-only API delay hook pro server-side Blazor requesty.

### E2E-BUG-0218: Globalni error boundary nepouzivala cesky retry fallback a prvni screenshot mel slepeny layout

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Routing / Error pages / UX / Screenshot testy
- **Nalezeno v testu:** `LayoutE2ETests.Layout_GlobalErrorBoundary_ShowsCzechFallbackAndRetry`
- **Screenshot/trace:** `artifacts/e2e/failures/layout/global-error-boundary-retry/20260621-103438.png`, `artifacts/e2e/failures/layout/global-error-boundary-retry/20260621-103727.png`, finalni screenshot `artifacts/e2e/screenshots/layout/global-error-boundary-retry/1366x900/light/fallback.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Prihlasit uzivatele a otevrit `/e2e/client-error?throw=1`.
  2. Nechat route vyvolat klientskou render vyjimku.
  3. Cekat na `data-testid="global-error-boundary"` a ulozit screenshot.
- **Ocekavani:** Aplikacni shell zobrazi cesky fallback `Nastala chyba`, srozumitelny popis, tlacitko `Zkusit znovu`, bez defaultni anglicke Blazor hlasky, a retry dokaze obnovit obsah.
- **Skutecnost:** Layout pouzival vestavenou Blazor `ErrorBoundary` bez vlastniho ceskeho fallbacku; existujici vlastni komponenta mela neodpovidajici resource klice. Po prvnim zapojeni screenshot ukazal nadpis, popis a tlacitko nalepene v jednom radku.
- **Pravdepodobna pricina:** Vlastni komponenta `LexiQuest.Blazor.Components.ErrorBoundary` nebyla zapojena do `MainLayout` a jeji lokalizacni klice neodpovidaly `Resources/Components/ErrorBoundary.resx`; chybel izolovany styl pro fallback panel.
- **Oprava:** `MainLayout` pouziva vlastni `ErrorBoundary`, komponenta ma stabilni `data-testid`, sjednocene resource klice `Error_Title`/`Error_Description`/`Error_TryAgain` a vlastni CSS pro citelny svisly alert panel. Doplnena jednorazova E2E route `/e2e/client-error` pro reprodukci a retry overeni.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~ErrorBoundaryTests" -m:1 /p:BuildInParallel=false /v:m` prosel 3/3. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Layout_GlobalErrorBoundary_ShowsCzechFallbackAndRetry" -m:1 /p:BuildInParallel=false /v:m` prosel 1/1. Finalni screenshot fallbacku byl vizualne zkontrolovan.
- **Poznamky:** Test bezi s `assertNoConsoleErrors: false`, protoze samotna reprodukce zamerne vyvolava klientskou vyjimku, kterou boundary zachytava.

### E2E-BUG-0217: Private-room countdown mohl preskocit UI checkpoint a prejit rovnou do hry

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Multiplayer / Private rooms / SignalR / UX / Screenshot testy
- **Nalezeno v testu:** `MultiplayerE2ETests.PrivateRoom_BothReady_StartsCountdownForBothPlayers`
- **Screenshot/trace:** `artifacts/e2e/failures/multiplayer/private-room-both-ready-countdown-host/20260621-101923.png`, `artifacts/e2e/failures/multiplayer/private-room-both-ready-countdown-guest/20260621-101925.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Spustit cely private-room E2E subset.
  2. V testu `PrivateRoom_BothReady_StartsCountdownForBothPlayers` nechat oba hrace kliknout na ready.
  3. Cekat na `private-room-countdown`.
- **Ocekavani:** Jakmile server posle `CountdownTick`, lobby zobrazi countdown sekci a E2E screenshot zachyti stav `Oba hráči připraveni`.
- **Skutecnost:** Klient nekdy navigoval do realtime hry driv, nez se v lobby objevil element `private-room-countdown`; failure screenshot uz ukazoval herni obrazovku.
- **Pravdepodobna pricina:** `RoomLobby` renderoval countdown pouze pri `RoomStatus.BothReady`. `CountdownTick` muze dorazit drive nez asynchronni refresh room statusu po `PlayerReadyStateChanged`.
- **Oprava:** `RoomLobby` renderuje countdown, pokud je `RoomStatus.BothReady || CountdownSeconds > 0`. Doplnen bUnit test `RoomLobby_CountdownTickBeforeReadyRefresh_ShowsCountdown`.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~RoomLobby_CountdownTickBeforeReadyRefresh_ShowsCountdown|FullyQualifiedName~RoomLobby_BothReady_ShowsCountdown" -m:1 /p:BuildInParallel=false /v:m` prosel 2/2. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_BothReady_StartsCountdownForBothPlayers" -m:1 /p:BuildInParallel=false /v:m` prosel 1/1. Cely `PrivateRoom_` E2E subset prosel 23/23 a screenshot `multiplayer/private-room-both-ready-countdown/1366x900/light/countdown-started.png` byl vizualne zkontrolovan.
- **Poznamky:** Serverovy countdown/match start fungoval; chyba byla v mezistavu klientského renderu, ktery mel byt pro hrace i screenshot viditelny.

### E2E-BUG-0216: Slovnikove screenshoty byly prekryte starymi toast notifikacemi

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Dictionaries / UX / Screenshot testy
- **Nalezeno v testu:** `DictionariesE2ETests.Dictionaries_PremiumUser_CreatesAddsImportsPublicAndDeletes`
- **Screenshot/trace:** `artifacts/e2e/screenshots/dictionaries/premium-crud-import-public-delete/1366x900/light/import-preview.png`, `import-result.png`, `detail.png`, `public-visible.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Spustit slovnikovy premium CRUD/import E2E flow.
  2. Po vytvoreni slovniku, pridani slova a importu ulozit screenshoty import preview/detail/public tab.
  3. Zkontrolovat pravy dolni roh screenshotu.
- **Ocekavani:** Screenshot checkpoint zobrazuje prave testovany stav slovniku bez rusivych historickych toastu.
- **Skutecnost:** Pred opravou zustavaly ve screenshotech success toasty z predchozich kroku a prekryvaly cast obrazovky.
- **Pravdepodobna pricina:** Screenshot priprava slovnikoveho testu nezavirala Tempo toast container; puvodni selektor netrefoval realne `.tm-toast-dismiss`.
- **Oprava:** `DictionariesE2ETests` ma `PrepareDictionaryScreenshotAsync`, ktery pred checkpointem zavre/ukryje Tempo toast container a zarovna scroll nahoru.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~DictionariesE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 4/4. Screenshoty `dictionaries/premium-crud-import-public-delete/1366x900/light/import-preview.png`, `import-result.png`, `detail.png` a `public-visible.png` byly znovu vizualne zkontrolovane bez toast prekryvu.
- **Poznamky:** Produktove toast chovani se nemenilo; uprava je jen priprava checkpointu, aby screenshot hodnotil cilovy stav.

### E2E-BUG-0215: Premium checkout success chybel jako screenshot checkpoint a cancel page nemela focused layout

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Premium / Checkout / UX / Screenshot testy
- **Nalezeno v testu:** `PremiumE2ETests.Premium_FakeCheckoutSuccess_ActivatesPremiumAndShowsActiveBadge`, `PremiumE2ETests.Premium_CheckoutCancel_DoesNotActivatePremium`
- **Screenshot/trace:** `artifacts/e2e/screenshots/premium/checkout-cancel-no-activation/1366x900/light/cancel.png`; success route `/premium/success` nebyla pred opravou samostatne vyfocena.
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Spustit `PremiumE2ETests`.
  2. Zkontrolovat checkpointy pro fake checkout success a cancel.
  3. Porovnat pozadavek T-901.3 `checkout success`/`checkout cancel` s ulozenymi screenshoty.
- **Ocekavani:** Success i cancel checkout stav maji samostatny viewport screenshot a focused confirmation layout.
- **Skutecnost:** Test po success checkoutu fotil az navrat na `/premium` s aktivnim badge; cancel stranka byla roztažena jako siroky blok pres obsah.
- **Pravdepodobna pricina:** `CheckoutSuccess.razor` a `CheckoutCancel.razor` nemely izolovane CSS pro confirmation layout a E2E test nemel checkpoint primo na success route.
- **Oprava:** Doplněny `CheckoutSuccess.razor.css` a `CheckoutCancel.razor.css` s focused confirmation layoutem; `PremiumE2ETests.Premium_FakeCheckoutSuccess_ActivatesPremiumAndShowsActiveBadge` uklada novy viewport checkpoint `success-page` primo na `/premium/success`, cancel checkpoint pouziva viewport screenshot.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PremiumE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 13/13. `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~PremiumPageTests" -m:1 /p:BuildInParallel=false /v:m` prosel 12/12. Screenshoty `premium/fake-checkout-success-activates/1366x900/light/success-page.png` a `premium/checkout-cancel-no-activation/1366x900/light/cancel.png` byly vizualne zkontrolovane.
- **Poznamky:** Funkcni flow bylo spravne, problem byl v UX/screenshot coverage.

### E2E-BUG-0214: Login redirect screenshot mel defaultni typografii a formularovy scaffold vzhled

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Auth / Login / UX / Screenshot testy
- **Nalezeno v testu:** `GuestE2ETests.GuestProtectedFeatures_DashboardRedirectsToLoginWithoutAuthTokens`
- **Screenshot/trace:** `artifacts/e2e/screenshots/guest/protected-features-redirect/1366x900/light/login-required.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Otevrit `/dashboard` bez auth tokenu.
  2. Nechat aplikaci presmerovat na `/login`.
  3. Zkontrolovat login checkpoint screenshot.
- **Ocekavani:** Login formular ma stejnou produktovou typografii a polish jako ostatni verejne obrazovky.
- **Skutecnost:** Screenshot pusobil jako defaultni HTML/scaffold: serif font, slabé rozvržení checkboxu a linků.
- **Pravdepodobna pricina:** `Login.razor.css` pouzival design tokeny bez fallback hodnot a cast vnitřních Tempo child prvku nebyla v E2E screenshotu dostatecne stabilne nastylovana.
- **Oprava:** `Login.razor.css` ma robustni fallbacky pro spacing/barvy/fonty, stylovanou kartu, deep styly pro inputy/checkbox a sjednocenou typografii formularovych prvku.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --no-build --filter "FullyQualifiedName~LoginPageTests" -m:1 /p:BuildInParallel=false /v:m` prosel 6/6. `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~LoginModelValidatorTests" -m:1 /p:BuildInParallel=false /v:m` prosel 4/4. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GuestE2ETests.GuestProtectedFeatures_DashboardRedirectsToLoginWithoutAuthTokens" -m:1 /p:BuildInParallel=false /v:m` prosel 1/1 a screenshot `guest/protected-features-redirect/1366x900/light/login-required.png` byl vizualne zkontrolovan.
- **Poznamky:** Nalez vznikl pri UX kontrole guest protected redirect checkpointu; paralelni bUnit beh jednou narazil na MSBuild static-web-assets file lock, sekvencni rerun prosel.

### E2E-BUG-0213: Guest komponenty v realne aplikaci zobrazovaly resource klice misto ceskych textu

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Guest / Lokalizace / Komponenty
- **Nalezeno v testu:** `GuestE2ETests.GuestConversion_RegisterFromCta_TransfersProgressToDashboard`, `GuestE2ETests.GuestLimit_FifthGameAllowedAndSixthGameShowsRegistrationCta`
- **Screenshot/trace:** E2E vystup s texty `Title`, `Description`, `Benefit_SaveProgress`, `Register`; navazujici screenshoty `artifacts/e2e/screenshots/guest/...`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Vykreslit guest CTA modal nebo guest limit komponentu v realne aplikaci.
  2. Zkontrolovat viditelne texty v modal/kartě.
  3. Spustit `GuestE2ETests` s asserty na ceske texty.
- **Ocekavani:** Guest komponenty zobrazují české texty z `.resx`.
- **Skutecnost:** `IStringLocalizer<T>` pro komponenty vracel nazvy klíčů (`Title`, `Description`, `Register`), protože resource soubory nebyly ve složce odpovídající namespace `Components.Guest`.
- **Pravdepodobna pricina:** Resource soubory byly uložené jako `Resources/Components/GuestCTAModal.resx`, ale typy jsou v namespace `LexiQuest.Blazor.Components.Guest`.
- **Oprava:** Doplněny resource soubory do `Resources/Components/Guest/` pro `GuestCTAModal`, `GuestConvertModal` a `GuestLimitReached`; existujici root komponentove resource zustaly zachovany.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GuestE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 7/7. Screenshoty CTA, convert a daily limit stavu uz zobrazuji ceske texty misto resource klicu.
- **Poznamky:** bUnit komponentove testy pouzivaly substituovany localizer, proto tento problem zachytil az realny E2E beh.

### E2E-BUG-0212: Guest screenshoty nezobrazovaly CTA/convert modaly a stranka pusobila jako neostylovany scaffold

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Guest / UX / Screenshot testy / Modaly
- **Nalezeno v testu:** `GuestE2ETests.GuestConversion_RegisterFromCta_TransfersProgressToDashboard`, `GuestE2ETests.GuestPlay_LoadsWithoutAccount_AndStoresWelcomeScreenshot`, `GuestE2ETests.GuestLimit_FifthGameAllowedAndSixthGameShowsRegistrationCta`
- **Screenshot/trace:** `artifacts/e2e/screenshots/guest/cta-modal-after-correct-answer/1366x900/light/visible.png`, `artifacts/e2e/screenshots/guest/conversion-transfers-progress/1366x900/light/conversion-modal.png`, `artifacts/e2e/screenshots/guest/welcome/1366x900/light/loaded.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Spustit `GuestE2ETests`.
  2. Zkontrolovat checkpointy guest welcome, CTA modal po spravne odpovedi a convert modal po dokonceni hry.
  3. Porovnat DOM asserty s viditelnym stavem na screenshotu.
- **Ocekavani:** Guest obrazovky maji finalni produktovy vzhled a modal checkpointy ukazuji modalni overlay ve viewportu.
- **Skutecnost:** Funkcni asserty prosly, ale welcome/arena/limit pusobily jako syrove HTML; CTA a convert screenshoty nezobrazily viditelny modal, protoze modalni obsah nebyl v zachycenem viewportu.
- **Pravdepodobna pricina:** `GuestGame.razor` nepouziva hotove guest modal/limit komponenty a cast stylingu zavisi na CSS tokenu nebo child komponentach, ktere v E2E screenshotu nedavaji stabilni vizualni vysledek.
- **Oprava:** `GuestGame.razor` pouziva dedikovane `GuestLimitReached`, `GuestCTAModal` a `GuestConvertModal`; guest page i komponenty maji stabilni CSS fallbacky, vlastni button/card styly a modal overlay s `data-testid`. `GuestE2ETests` kontroluje ceske modal texty a geometrii overlay/modalu ve viewportu.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~GuestGamePageTests" -m:1 /p:BuildInParallel=false /v:m` prosel 8/8. `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --no-build --filter "FullyQualifiedName~Components.Guest" -m:1 /p:BuildInParallel=false /v:m` prosel 27/27. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GuestE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 7/7. Screenshoty `guest/welcome`, `guest/start-game`, `guest/wrong-answer-feedback`, `guest/cta-modal-after-correct-answer`, `guest/conversion-transfers-progress`, `guest/daily-limit-registration-cta` a `guest/daily-limit-reset-after-24h` byly vizualne zkontrolovane.
- **Poznamky:** Nalez vznikl rucni UX kontrolou screenshotu po uspesnem behu testu 7/7; nova viewport geometrie brani tomu, aby offscreen modal znovu prosel jen diky DOM viditelnosti.

### E2E-BUG-0211: Modalni Settings screenshot pouzival full-page rezim a backdrop pokryl jen cast dlouhe stranky

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Settings / Screenshot testy / Modaly
- **Nalezeno v testu:** `SettingsE2ETests.Settings_DangerZone_LogoutDeactivateAndDeleteRequireConfirmation`
- **Screenshot/trace:** `artifacts/e2e/screenshots/settings/danger-zone-logout-deactivate-delete/1366x900/light/deactivate-confirm.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Otevrit `/settings`.
  2. Zobrazit confirm modal pro deaktivaci nebo smazani uctu.
  3. Vyfotit full-page screenshot.
- **Ocekavani:** Modalni checkpoint zachycuje realny viewport, kde backdrop korektne prekryva celou viditelnou plochu.
- **Skutecnost:** Full-page screenshot spojil dlouhou stranku s fixed backdropem jen pres viewport, takze spodni cast screenshotu zustala svetla.
- **Pravdepodobna pricina:** Modalni stav byl focen s `fullPage: true`.
- **Oprava:** Confirm modal checkpointy v Settings testu pouzivaji `fullPage: false`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~SettingsE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 4/4. Screenshoty `settings/danger-zone-logout-deactivate-delete/1366x900/light/deactivate-confirm.png` a `delete-confirm.png` jsou viewport screenshoty s korektnim backdropem.
- **Poznamky:** Runtime modal byl v poradku; chyba byla v metodice screenshot checkpointu.

### E2E-BUG-0210: Settings toast prekryva obsah formulare ve full-page screenshotu

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Settings / UX / Toasty
- **Nalezeno v testu:** `SettingsE2ETests.Settings_ProfileUsernameDuplicateAndAvatarValidation_WorkEndToEnd`, `SettingsE2ETests.Settings_PasswordChangeAndWrongCurrentPassword_WorkEndToEnd`
- **Screenshot/trace:** `artifacts/e2e/screenshots/settings/profile-username-avatar-validation/1366x900/light/profile-saved.png`, `artifacts/e2e/screenshots/settings/password-change-and-wrong-current/1366x900/light/wrong-current-password.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Otevrit `/settings`.
  2. Vyvolat success nebo error toast ve formuláři.
  3. Porovnat fixed toast pozici s obsahem nastaveni ve viewportu.
- **Ocekavani:** Pokud checkpoint netestuje primo toast, screenshot pred focenim zavre toast a neprekryva obsah formulare.
- **Skutecnost:** Toast prekryval cast pole `Čas připomínky streaku`.
- **Pravdepodobna pricina:** Globalni toast kontejner je fixovany vpravo dole a na dlouhe settings strance muze prekryt aktualni obsah.
- **Oprava:** Settings full-page checkpointy pred focenim volaji pripravu screenshotu, ktera zavre aktivni toasty, pokud toast neni predmet testu; heslova sekce navic zobrazuje inline status, aby edge-case screenshot zustal samonosny i bez toastu.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~SettingsE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 4/4. Screenshoty `settings/profile-username-avatar-validation/1366x900/light/profile-saved.png` a `settings/password-change-and-wrong-current/1366x900/light/wrong-current-password.png` byly zkontrolovany bez toast prekryvu; heslovy edge case ma inline chybu v karte.
- **Poznamky:** Globalni runtime pozice toastu zustava beze zmeny, aby se nerozbila predchozi notifikacni UX kontrola.

### E2E-BUG-0209: Profilove statistiky mely prazdne ikonove bubliny

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Profile / UX / Ikony
- **Nalezeno v testu:** `ProfileE2ETests.Profile_Page_ShowsPremiumStatsAndAchievementsSummary`
- **Screenshot/trace:** `artifacts/e2e/screenshots/profile/summary-premium-stats-achievements/1366x900/light/loaded.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Otevrit `/profile` s naplnenymi statistikami.
  2. Zkontrolovat statisticke karty `Vyřešená slova` a `Aktuální série`.
- **Ocekavani:** Kazda statisticka karta ma viditelnou ikonu, ktera pomaha rychlemu skenovani.
- **Skutecnost:** Dve ikonove bubliny byly prazdne.
- **Pravdepodobna pricina:** Profil pouzival nazvy ikon, ktere aktualni `TmIcon` sada v tomto kontextu nevykreslila.
- **Oprava:** Profilove statistiky pouzivaji `IconNames.*` konstanty (`FileText`, `Fire` a dalsi) misto ručně tipovanych stringu; E2E test nove kontroluje viditelne SVG ikony ve vsech statistickych kartach.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~ProfileE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 1/1. Test kontroluje viditelne SVG ikony ve vsech stat kartach a screenshot `profile/summary-premium-stats-achievements/1366x900/light/loaded.png` byl vizualne zkontrolovan.
- **Poznamky:** Nalez vznikl az pri rucni UX kontrole screenshotu po predchozi oprave layoutu.

### E2E-BUG-0208: Profilova stranka pouzivala neucinne Tailwind tridy a rozpadla statistiky do seznamu

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Profile / UX / CSS
- **Nalezeno v testu:** `ProfileE2ETests.Profile_Page_ShowsPremiumStatsAndAchievementsSummary`
- **Screenshot/trace:** `artifacts/e2e/screenshots/profile/summary-premium-stats-achievements/1366x900/light/loaded.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Prihlasit premium uzivatele s naplnenymi statistikami.
  2. Otevrit `/profile`.
  3. Zkontrolovat sekci `Statistiky`.
- **Ocekavani:** Statistiky jsou zobrazeny jako citelny grid se samostatnymi kartami a zarovnanymi hodnotami.
- **Skutecnost:** Tailwind utility tridy se v aplikaci neaplikovaly, takze ikony, hodnoty a labely spadly do dlouheho vertikalniho seznamu.
- **Pravdepodobna pricina:** Profilova stranka byla napsana utility tridami, ktere nejsou soucasti aktualniho CSS buildu aplikace.
- **Oprava:** Profil byl prepsan na stabilni semanticke CSS tridy v `Profile.razor.css`; E2E test kontroluje i geometrii prvni rady statistickeho gridu.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~ProfileE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 1/1. Screenshot `profile/summary-premium-stats-achievements/1366x900/light/loaded.png` zobrazuje statistiky jako 3x2 grid a test kontroluje geometrii prvni rady.
- **Poznamky:** Puvodni screenshot zaroven potvrdil, ze funkcni asserty samy o sobe nestaci pro UX stav.

### E2E-BUG-0207: Profil formatoval presnost jako nasobene procento

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Profile / Statistiky / UX
- **Nalezeno v testu:** `ProfileE2ETests.Profile_Page_ShowsPremiumStatsAndAchievementsSummary`
- **Screenshot/trace:** `artifacts/e2e/screenshots/profile/summary-premium-stats-achievements/1366x900/light/loaded.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Nastavit uzivateli profilovou statistiku `Accuracy = 88`.
  2. Otevrit `/profile`.
  3. Zkontrolovat hodnotu v karte `Přesnost`.
- **Ocekavani:** Profil zobrazi `88%`.
- **Skutecnost:** Profil pouzival format `P0`, ktery hodnotu `88` interpretuje jako pomer a zobrazil by `8800%`.
- **Pravdepodobna pricina:** API/domain uklada presnost jako procentni hodnotu 0-100, ale profil ji formatoval jako zlomek 0-1.
- **Oprava:** `Profile.razor` formatuje presnost jako `N0` s explicitnim znakem `%`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~ProfileE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 1/1. Screenshot `profile/summary-premium-stats-achievements/1366x900/light/loaded.png` zobrazuje `88%`, ne `8800%`.
- **Poznamky:** Dashboard uz pouzival procentni hodnotu primo, chyba byla lokalni pro profil.

### E2E-BUG-0206: Profilova karta uspechu pouzivala nekonzistentni termin Achievementy

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Profile / Lokalizace / UX
- **Nalezeno v testu:** `ProfileE2ETests.Profile_Page_ShowsPremiumStatsAndAchievementsSummary`
- **Screenshot/trace:** `artifacts/e2e/screenshots/profile/summary-premium-stats-achievements/1366x900/light/loaded.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Prihlasit uzivatele a otevrit `/profile`.
  2. Zkontrolovat kartu se souhrnem uspechu.
- **Ocekavani:** Profil pouziva stejnou terminologii jako navigace a stranka uspechu: `Úspěchy`.
- **Skutecnost:** Profilova karta zobrazovala `Achievementy` a popis `odemčených achievementů`.
- **Pravdepodobna pricina:** Profilovy resource zustal na starsim mixu ceskeho a anglickeho nazvoslovi.
- **Oprava:** `Resources/Pages/Profile.resx` byl sjednocen na `Úspěchy` a `odemčených úspěchů`; Settings notifikacni label byl upraven na `Notifikace o úspěších`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~ProfileE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 1/1 a `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~SettingsE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 4/4. Screenshot profilu zobrazuje `Úspěchy` a Settings screenshot zobrazuje `Notifikace o úspěších`.
- **Poznamky:** Stejny typ nekonzistence byl predtim opraven pro samostatnou stranku uspechu.

### E2E-BUG-0205: Stranka uspechu mela nekonzistentni nadpis Achievementy

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Achievements / Lokalizace / UX
- **Nalezeno v testu:** `AchievementsE2ETests.Achievements_Page_ShowsProgressFiltersAndCardStates`
- **Screenshot/trace:** `artifacts/e2e/screenshots/achievements/overview-filter-card-states/1366x900/light/all-states.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Prihlasit uzivatele s kombinaci odemceneho, rozpracovaneho a zamceneho uspechu.
  2. Otevrit `/achievements`.
  3. Porovnat nadpis stranky s navigaci.
- **Ocekavani:** Nadpis stranky i navigace pouzivaji stejnou ceskou terminologii `Úspěchy`.
- **Skutecnost:** Navigace zobrazuje `Úspěchy`, ale H1 stranky zobrazuje `Achievementy`.
- **Pravdepodobna pricina:** Page resource zustal na starsim textu po predchozi lokalizaci navigace.
- **Oprava:** `Resources/Pages/Achievements.resx` hodnota `Page.Title` byla zmenena na `Úspěchy`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~AchievementsE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 3/3. Screenshot `achievements/overview-filter-card-states/1366x900/light/all-states.png` byl vizualne zkontrolovan a zobrazuje H1 `Úspěchy`.
- **Poznamky:** Produktove texty s termínem achievement budou reseny zvlast v sirsi lokalizacni kontrole; tento nalez se tyka viditelne nekonzistence stranka vs. navigace.

### E2E-BUG-0204: Daily challenge completed stav zobrazil cas 0s

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Daily challenge / UX / Cas
- **Nalezeno v testu:** `DailyChallengeE2ETests.DailyChallenge_Completion_ShowsResultTopTenAndRejectsSecondAttempt`
- **Screenshot/trace:** `artifacts/e2e/screenshots/daily/completion-top10-second-attempt/1366x900/light/completed.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Spustit daily challenge v E2E.
  2. Okamzite odeslat spravnou odpoved.
  3. Zobrazit completed stav.
- **Ocekavani:** Kladny cas dokonceni se zobrazi minimalne jako `1s`.
- **Skutecnost:** Completed karta zobrazila `0s`, coz pusobi jako chybne mereni.
- **Pravdepodobna pricina:** UI posilalo sub-second `TimeSpan` a formatter pouzival `time.Seconds`, tedy zaokrouhloval dolu.
- **Oprava:** Daily UI zaokrouhluje kladny cas nahoru a submit nastavuje minimalni kladny cas `1s`; `0s` zustava jen pro nulovy/zaporny vstup.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~DailyChallengePageTests" -m:1 /p:BuildInParallel=false /v:m` prosel 6/6. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~DailyChallengeE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 3/3. Screenshot `daily/completion-top10-second-attempt/1366x900/light/completed.png` byl vizualne zkontrolovan a zobrazuje `1s`.
- **Poznamky:** Oprava zachova `0s` jen pro nulovy nebo zaporny vstup.

### E2E-BUG-0203: Freeze badge a shield countdown mely anglicky vyraz a spatne sklonovani dnu

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Dashboard / Streak / Lokalizace / UX
- **Nalezeno v testu:** `StreakE2ETests.Streak_DashboardPremiumFreeze_ShowsFreezeBadge`
- **Screenshot/trace:** `artifacts/e2e/screenshots/streak/dashboard-premium-freeze-badge/1366x900/light/freeze-available.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Vytvorit premium uzivatele se streak protection stavem a otevrit `/dashboard`.
  2. Zobrazit freeze badge a premium shield cooldown.
  3. Zkontrolovat texty ve streak kartě.
- **Ocekavani:** Freeze/freeze-like mechanika je pojmenovana cesky a odpočet dnu pouziva spravne tvary `den`/`dny`/`dní`.
- **Skutecnost:** UI zobrazilo `Freeze dostupný` a `Další za: 4 dní`.
- **Pravdepodobna pricina:** Resource text zustal castečně anglicky a komponenta pouzivala jediny textovy tvar `dní` pro vsechny hodnoty.
- **Oprava:** Freeze badge byl prelozen na `Zmrazení dostupné`; `StreakIndicator` pouziva ceske tvary `den`/`dny`/`dní` pro shield countdown i streak text.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~StreakIndicatorTests" -m:1 /p:BuildInParallel=false /v:m` prosel 10/10. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~StreakE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 15/15. Screenshot `streak/dashboard-premium-freeze-badge/1366x900/light/freeze-available.png` byl vizualne zkontrolovan a zobrazuje `Zmrazení dostupné` a `Další za: 4 dny`.
- **Poznamky:** Oprava zaroven pokryva singular streak text `1 den v řadě`.

### E2E-BUG-0202: Dashboard a XP bar zobrazovaly anglicke texty v ceskem UI

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Dashboard / XP / Lokalizace / UX
- **Nalezeno v testu:** `StreakE2ETests.Streak_DashboardNormal_ShowsStableStreakAndXpBar`
- **Screenshot/trace:** `artifacts/e2e/screenshots/streak/dashboard-normal-streak-xp/1366x900/light/normal.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Vytvorit prihlaseneho uzivatele se streakem a XP progressem.
  2. Otevrit `/dashboard`.
  3. Zkontrolovat H1 a XP badge.
- **Ocekavani:** Vsechny viditelne texty v ceskem UI jsou cesky, tedy `Přehled`/`Úroveň`.
- **Skutecnost:** Screenshot zobrazil anglicke `Dashboard` a `Level 3`.
- **Pravdepodobna pricina:** Nektere resource hodnoty zustaly z puvodniho scaffoldingu.
- **Oprava:** Resource hodnoty dashboard title, XP bar level, profil/path level texty a checkout dashboard tlacitka byly prevedeny do cestiny; navazujici E2E assertions byly upraveny na ceske texty.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~StreakE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 15/15. Screenshot `streak/dashboard-normal-streak-xp/1366x900/light/normal.png` byl vizualne zkontrolovan a zobrazuje `Přehled` a `Úroveň 3`.
- **Poznamky:** V ramci opravy byly sjednoceny i pribuzne resource texty pro profil, path node a checkout navigaci.

### E2E-BUG-0201: Twist reveal test zavodil s klientskym timerem a pocet odhalenych pismen byl nestabilni

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Boss / Twist / E2E stabilita
- **Nalezeno v testu:** `BossE2ETests.Boss_Twist_StartsWithRevealedLettersAndRevealsAfterThreeSeconds`
- **Screenshot/trace:** `artifacts/e2e/failures/boss/twist-reveal-after-three-seconds`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Spustit `BossE2ETests`.
  2. Zalozit Twist boss session pres API.
  3. Otevrit `/boss/twist/{sessionId}` a cekat presne 2 odhalena pismena.
- **Ocekavani:** Test deterministicky zachyti pocatecni stav se 2 odhalenymi pismeny a nasledny reveal.
- **Skutecnost:** Klientsky timer mohl behem navigace odhalit 3-5 pismen drive, nez Playwright provedl assert.
- **Pravdepodobna pricina:** Reveal stav se pocita z `GameRound.StartedAt` a klientsky `System.Timers.Timer` bezi nezavisle na serverovem E2E time hooku.
- **Oprava:** Pridan E2E helper `ForceActiveRoundStartedAtAsync`; Twist test nastavuje aktivni kolo tesne do budoucnosti pred navigaci, aby mel pocatecni UI stav stabilni reveal okno.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~BossE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 9/9. Screenshoty `boss/twist-reveal-after-three-seconds/1366x900/light/loaded-reveal-state.png` a `after-three-seconds.png` byly vizualne zkontrolovane s postupnym reveal stavem.
- **Poznamky:** API cast testu stale overuje pocatecni `RevealedPositions`/`RevealedLetters` na 2.

### E2E-BUG-0200: Path completion test blokoval achievement modal po prvni spravne odpovedi

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Paths / E2E stabilita / Achievements
- **Nalezeno v testu:** `PathsE2ETests.Paths_CompleteLevel_UpdatesProgressAndShowsPerfectState`
- **Screenshot/trace:** `artifacts/e2e/failures/paths/complete-level-perfect-progress`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Spustit `PathsE2ETests`.
  2. Ve scenari dokončení levelu odeslat prvni spravnou odpoved.
  3. Pokracovat smyckou na dalsi kolo.
- **Ocekavani:** Test path progresu projde 10 kol a ověří mapu po dokončení levelu.
- **Skutecnost:** Achievement modal `first_word` prekryl hru a blokoval klik na dalsi submit.
- **Pravdepodobna pricina:** Scenar nemel izolovana achievement preddata; checkpoint cesty byl zavisly na prvnim achievement unlocku.
- **Oprava:** Pred testem se seeduje `first_word` jako uz odemceny pro daneho uzivatele.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PathsE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 5/5. Screenshot `paths/complete-level-perfect-progress/1366x900/light/perfect-progress.png` byl vizualne zkontrolovan a zobrazuje cisty path progress bez achievement modal blokace.
- **Poznamky:** Achievement unlock zustava samostatne pokryty v achievement/notification testech.

### E2E-BUG-0199: Level complete screenshot byl prekryty achievement modalem

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Game / Screenshot testy / Overlay UX
- **Nalezeno v testu:** `GameFlowE2ETests.Game_LevelComplete_ShowsOverlayAfterFinalRound`
- **Screenshot/trace:** `artifacts/e2e/screenshots/game/level-complete/1366x900/light/visible.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Spustit level-complete E2E scenar s novym uzivatelem.
  2. Nastavit celkovy pocet kol na 1 a odeslat spravnou odpoved.
  3. Vyfotit checkpoint `level-complete`.
- **Ocekavani:** Screenshot zobrazi citelny level-complete dialog bez prekryti jinym modalem.
- **Skutecnost:** Prvni spravna odpoved odemkla achievement a modal achievementu prekryl level-complete overlay; na obrazovce byly dve vrstvy a toast.
- **Pravdepodobna pricina:** Scenar nemel izolovana achievement preddata, takze prvni-word unlock menil stav UI.
- **Oprava:** Pred spustenim hry se seeduje `first_word` jako uz odemceny.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GameFlowE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 38/38. Screenshot `game/level-complete/1366x900/light/visible.png` byl vizualne zkontrolovan a zobrazuje cisty level-complete dialog bez achievement modal prekryvu.
- **Poznamky:** Samotny achievement unlock zustava pokryty v achievement/notification testech.

### E2E-BUG-0198: Screenshot spravne odpovedi zachytil achievement modal misto herniho stavu

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Game / Screenshot testy / Achievements
- **Nalezeno v testu:** `GameFlowE2ETests.Game_CorrectAnswer_IncreasesXpComboAndMovesToNextRound`
- **Screenshot/trace:** `artifacts/e2e/screenshots/game/correct-answer-next-round/1366x900/light/round-2.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Spustit herni E2E sadu s novym uzivatelem.
  2. Odeslat prvni spravnou odpoved.
  3. Vyfotit checkpoint `correct-answer-next-round`.
- **Ocekavani:** Screenshot pro spravnou odpoved zachyti herni stav po spravne odpovedi / dalsi kolo, ne jiny modal.
- **Skutecnost:** Protoze prvni spravna odpoved odemkne achievement `first_word`, screenshot zachytil modal `Uspešch odemčen`.
- **Pravdepodobna pricina:** Test nemel izolovana preddata pro checkpoint spravne odpovedi a prekryvny achievement modal zmenil vyznam screenshotu.
- **Oprava:** Pred odeslanim odpovedi se pro tento scenar seeduje `first_word` jako uz odemceny, aby checkpoint meril samotny game flow.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GameFlowE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 38/38. Screenshot `game/correct-answer-next-round/1366x900/light/round-2.png` byl vizualne zkontrolovan a zachycuje game flow bez achievement modal prekryvu.
- **Poznamky:** Achievement modal zustava testovan v achievement/notification scenarich.

### E2E-BUG-0197: Aktivni herni obrazovka pusobila drobne a input mel syrovy browser styl

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Game / UX screenshot / CSS isolation
- **Nalezeno v testu:** `GameFlowE2ETests.StartGame_LoggedInUser_CanStartTrainingSession`
- **Screenshot/trace:** `artifacts/e2e/screenshots/game/start-training/1366x900/light/active-game.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Prihlasit E2E uzivatele.
  2. Otevrit `/game` a spustit trening.
  3. Vyfotit aktivni hru.
- **Ocekavani:** Hra ma citelnou herni plochu, odpovedni input ma design-system focus stav a casovac je zarovnany s obsahem.
- **Skutecnost:** Herni plocha byla velmi kompaktni, input pouzival defaultni browser focus outline a casovac byl sirsi nez herni obsah.
- **Pravdepodobna pricina:** CSS stale cililo na starsi `.tm-input`, ale komponenta pouziva nativni `.answer-input`; timer nemel vlastni max sirku.
- **Oprava:** Upraveny skutecne selektory pro `.answer-input`, sirka herni areny, tlacitek, feedbacku a timeru.
- **Overeni:** `dotnet build src/LexiQuest.Web/LexiQuest.Web.csproj --no-restore -m:1 /p:BuildInParallel=false /v:m` prosel bez varovani/chyb a `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GameFlowE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 38/38. Screenshot `game/start-training/1366x900/light/active-game.png` byl vizualne zkontrolovan se stylovanym inputem a zarovnanym timerem.
- **Poznamky:** Funkcni asserty pred opravou prosly, problem odhalila az UX kontrola screenshotu.

### E2E-BUG-0196: Start hry zobrazoval lokalizacni klice misto ceskych textu

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Game / Lokalizace / UX screenshot
- **Nalezeno v testu:** `GameFlowE2ETests.StartGame_LoggedInUser_CanStartTrainingSession`
- **Screenshot/trace:** `artifacts/e2e/screenshots/game/start-screen/1366x900/light/loaded.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Prihlasit E2E uzivatele.
  2. Otevrit `/game`.
  3. Vyfotit start screen pred spustenim hry.
- **Ocekavani:** Start hry zobrazi ceske texty pro nadpis, popis a tlacitka rezimu.
- **Skutecnost:** UI zobrazovalo fallback klice `Welcome`, `SelectMode`, `Mode_Training` a `Mode_TimeAttack`.
- **Pravdepodobna pricina:** `Resources/Pages/Game.resx` neobsahoval klice pouzivane v `Game.razor` pro start/loading/error stav.
- **Oprava:** Do `Game.resx` byly doplneny ceske hodnoty pro start obrazovku, loading, retry a error stavy. E2E test nově kontroluje, ze fallback klice nejsou videt.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GameFlowE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 38/38. Screenshot `game/start-screen/1366x900/light/loaded.png` byl vizualne zkontrolovan s ceskymi texty `Vyber rezim hry`, `Trenink` a `Na cas`.
- **Poznamky:** Nalez vznikl pri UX kontrole screenshotu, funkcni happy path predtim prosel.

### E2E-BUG-0195: Dashboard loading screenshot mohl zachytit uz nacteny stav misto skeletonu

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Dashboard / Screenshot testy / E2E infra
- **Nalezeno v testu:** `Dashboard_LoadingSkeleton_IsVisibleWhileStatsRequestIsPending`
- **Screenshot/trace:** `artifacts/e2e/screenshots/dashboard/loading-skeleton/1366x900/light/loading.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. V E2E testu otevrit `/dashboard` a pokusit se zdrzet `GET /api/v1/stats/user` pres Playwright route.
  2. Vyfotit loading checkpoint pred rucnim uvolnenim requestu.
  3. Porovnat ulozeny screenshot se skutecnym skeleton stavem.
- **Ocekavani:** Loading checkpoint zachyti viditelny dashboard skeleton, ne uz kompletne nacteny dashboard.
- **Skutecnost:** Protoze Web host bezi jako Blazor Interactive Auto/Server, stats request muze probehnout server-side a Playwright route ho nespolehlive nezachyti. Screenshot tak mohl ulozit uz nacteny dashboard, i kdyz test kratce videl skeleton.
- **Pravdepodobna pricina:** Browser route interception neni vhodny zdroj pravdy pro requesty, ktere muze obslouzit server-side Blazor runtime.
- **Oprava:** Pridan E2E-only `E2EStatsRuntimeSettings` s endpointy pro `fail-next`, `delay-next` a `release` stats request. Dashboard loading test ted drzi skutecny API request na serveru, vyfoti skeleton a az potom request uvolni.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~DashboardE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 4/4. Novy `loading.png` byl vizualne zkontrolovan a zobrazuje skeleton bloky, ne nactene statistiky.
- **Poznamky:** Stejny E2E stats hook se pouziva pro deterministicky `error retry` dashboard scenar.

### E2E-BUG-0194: Password reset stranky nebyly vizualne sjednocene s auth flow

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Auth / Password reset / UX screenshot
- **Nalezeno v testu:** `AuthPages_RenderFocusedLayoutWithoutAppSidebar`
- **Screenshot/trace:** `artifacts/e2e/screenshots/auth/password-reset-focused-layout/1366x900/light/loaded.png`, `artifacts/e2e/screenshots/auth/password-reset-neplatny-token-123-focused-layout/1366x900/light/loaded.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Otevrit `/password-reset`.
  2. Otevrit `/password-reset/neplatny-token-123`.
  3. Porovnat vzhled s `/login` a `/register` focused auth layoutem.
- **Ocekavani:** Password reset request i confirm pouzivaji konzistentni auth kartu, citelne inputy, primarni akci a zadny app sidebar.
- **Skutecnost:** Obe reset stranky se zobrazovaly jako temer vychozi HTML vlevo nahore, bez auth karty, spacingu a vizualne citelnych poli.
- **Pravdepodobna pricina:** Reset stranky pouzivaly raw HTML tridy bez izolovaneho CSS a bez Tempo komponent pro kartu, tlacitka a alert stavy.
- **Oprava:** `PasswordResetRequest.razor` a `PasswordResetConfirm.razor` byly sjednocene na `TmCard`, `TmButton`, `TmAlert` a focused auth CSS. Pro `InputText` byl pouzit `::deep`, aby izolovane CSS dopadlo i na rendered child inputy.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~AuthPages_RenderFocusedLayoutWithoutAppSidebar" -m:1 /p:BuildInParallel=false /v:m` prosel 4/4. `AuthE2ETests` prosly 27/27 a `EmailE2ETests` prosly 7/7. Finální password reset screenshoty byly vizualne zkontrolovane bez prekryvu a s citelnymi poli.
- **Poznamky:** Oprava zachovala existujici `data-testid` selektory pro request/confirm form a submit.

### E2E-BUG-0193: Registracni auth karta byla uzka a orezavala password placeholder

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Auth / Registrace / UX screenshot
- **Nalezeno v testu:** `AuthPages_RenderFocusedLayoutWithoutAppSidebar`
- **Screenshot/trace:** `artifacts/e2e/screenshots/auth/register-focused-layout/1366x900/light/loaded.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Otevrit `/register` na desktop viewportu 1366x900.
  2. Zkontrolovat password pole s placeholderem `Min. 8 znaku, 1 velke pismeno, 1 cislo`.
  3. Porovnat zarovnani a sirku karty s login obrazovkou.
- **Ocekavani:** Placeholder je citelny bez orezani a registracni karta ma stabilni sirku odpovidajici obsahu formulare.
- **Skutecnost:** Screenshot review ukazal, ze password placeholder byl orezany a karta nepouzila zamyslenou sirku/padding.
- **Pravdepodobna pricina:** CSS isolation neaplikovalo `.register-card` a `.login-card` pravidla na root element `TmCard`, protoze jde o child komponentu.
- **Oprava:** Do `Login.razor.css` a `Register.razor.css` byly doplnene `::deep .login-card` / `::deep .register-card` styly a stabilni spacing pro submit wrappery.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~AuthPages_RenderFocusedLayoutWithoutAppSidebar" -m:1 /p:BuildInParallel=false /v:m` prosel 4/4. `AuthE2ETests` prosly 27/27. Regenerovany register screenshot byl vizualne zkontrolovan, password placeholder uz neni orezany.
- **Poznamky:** Stejna uprava chrani i login kartu pred prilis uzkym renderem pri izolovanem CSS.

### E2E-BUG-0192: Landing feature taby obsahovaly viditelne anglicke texty

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Landing / Lokalizace / UX screenshot
- **Nalezeno v testu:** `LandingPage_FeatureTabs_RenderAllPanels_AndStoreUxCheckpoints`
- **Screenshot/trace:** `artifacts/e2e/screenshots/landing/feature-tabs/1366x900/light/rpg.png`, `artifacts/e2e/screenshots/landing/feature-tabs/1366x900/light/souboje.png`, `artifacts/e2e/screenshots/landing/feature-tabs/1366x900/light/souteze.png`
- **Prostredi:** SQL Server Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Otevrit landing page `/`.
  2. Sjet na sekci `Proč hrát LexiQuest?`.
  3. Zkontrolovat texty ve feature tabech a jejich vizualnich placeholder panelech.
- **Ocekavani:** Vsechny viditelne landing texty jsou v cestine a neobsahuji hardcoded anglictinu.
- **Skutecnost:** Prvni tab a placeholdery zobrazovaly `RPG Progress`, `Boss Battles` a `Competitions`.
- **Pravdepodobna pricina:** Landing feature komponenta mela cast placeholder textu natvrdo v anglictine a `Features.Tab1.Title` v resource souboru byl take anglicky.
- **Oprava:** `FeaturesSection.razor` pouziva lokalizovane titulky tabu i v placeholder panelech; `Features.Tab1.Title` zmenen na `RPG postup`. E2E test overuje, ze anglicke texty nejsou viditelne.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~LandingE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 10/10. Screenshoty feature tabu byly vizualne zkontrolovane bez prekryvu a bez anglictiny.
- **Poznamky:** Samostatne checkpointy pokryvaji vsechny tri taby: `rpg`, `souboje`, `souteze`.

### E2E-BUG-0191: Team empty state generoval konzolovy 404 pri beznem zobrazeni stranky

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Teams / UI data loading / E2E stabilita
- **Nalezeno v testu:** `DataTestIds_MainRoutesAndPrimaryComponents_ExposeStableSelectors`
- **Screenshot/trace:** `artifacts/e2e/failures/selector-audit/main-routes-primary-components/20260621-063755.png`, `artifacts/e2e/failures/selector-audit/main-routes-primary-components/20260621-063755-console.log`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Prihlasit uzivatele bez tymu.
  2. Otevrit `/team`.
  3. V route auditu sledovat HTTP odpovedi a browser console chyby.
- **Ocekavani:** No-team empty state je bezny UI stav a nesmi generovat konzolovy `404 Not Found`.
- **Skutecnost:** Blazor klient volal `GET /api/v1/teams`; API pro beztymoveho uzivatele spravne vracelo 404 podle puvodniho API kontraktu, ale browser to logoval jako chybu resource loadu.
- **Pravdepodobna pricina:** Stejny endpoint se pouzival pro API kontrakt i UI empty-state dotaz; klient pak normalni stav reprezentoval pres HTTP 404.
- **Oprava:** Pridan UI-friendly endpoint `GET /api/v1/teams/my`, ktery vraci `204 No Content` pro uzivatele bez tymu a `200 OK` s `TeamDto` pro uzivatele v tymu. Blazor `TeamService.GetMyTeamAsync()` pouziva novy endpoint, puvodni `GET /api/v1/teams` zustal kompatibilni a pro no-team dal vraci 404.
- **Overeni:** `dotnet test tests/LexiQuest.Api.Tests/LexiQuest.Api.Tests.csproj --filter "FullyQualifiedName~TeamsControllerTests" -m:1 /p:BuildInParallel=false /v:m` prosel 3/3. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~SelectorAuditE2ETests" -m:1 /p:BuildInParallel=false /v:m` prosel 2/2.
- **Poznamky:** RED beh selector auditu pred opravou selhal na `404 http://127.0.0.1:<port>/api/v1/teams`; po oprave zustava 404 testovany jen v oddelenem zamerne neexistujicim route scenari.

### E2E-BUG-0190: Multi-level-up E2E nemel deterministicky UI/API tok s velkou XP odmenou

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Game / XP / E2E infra / Testovatelnost
- **Nalezeno v testu:** `Game_MultiLevelUpFromSingleXpGain_ShowsFinalLevelAndAllUnlocks`
- **Screenshot/trace:** `artifacts/e2e/screenshots/game/multi-level-up-single-xp-gain/1366x900/light/visible.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. V E2E testu nastavit hrace na 50 XP a level 1.
  2. Pokusit se pres UI odeslat jednu spravnou odpoved s jednorazovou odmenou 500 XP.
  3. Volat E2E endpoint `/api/v1/e2e/xp/fixed-correct-answer` pred startem hry.
- **Ocekavani:** Test umi deterministicky vyvolat realny `GameSessionService.SubmitAnswerAsync` tok s velkou XP odmenou, zobrazit level-up modal pro level 4 a overit DB/API statistiky.
- **Skutecnost:** Endpoint pro nastaveni jednorazove velke XP odmeny neexistoval a RED beh skoncil na HTTP 404, bez moznosti overit multi-level-up v browseru.
- **Pravdepodobna pricina:** Běžná herni odpoved dava maximalne nizke desitky XP, takze bez E2E-only nastavitelneho XP kalkulatoru nejde jednou odpovedi prekrocit vice XP hranic.
- **Oprava:** Doplněn `E2EXpRuntimeSettings`, E2E-only `E2EXpCalculator`, endpoint `/api/v1/e2e/xp/fixed-correct-answer`, reset v `/api/v1/e2e/state/reset` a fixture helper `SetFixedCorrectAnswerXpAsync`.
- **Overeni:** RED: `Game_MultiLevelUpFromSingleXpGain_ShowsFinalLevelAndAllUnlocks` spadl na HTTP 404. GREEN: stejný test prosel 1/1, screenshot byl zkontrolovan z pohledu UX. `dotnet build src/LexiQuest.Api/LexiQuest.Api.csproj --no-restore -m:1 /p:BuildInParallel=false /v:q` a `dotnet build tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --no-restore -m:1 /p:BuildInParallel=false /v:q` prosly.
- **Poznamky:** Oprava je registrovana jen pro prostredi `E2E`; produkcni XP kalkulator zustava bez nastavitelne fixed odmeny.

### E2E-BUG-0189: Modal dialogy nepasti fokus pri Shift+Tab z prvniho pole

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** A11y / Modaly / Keyboard
- **Nalezeno v testu:** `A11y_ModalFocusTrap_KeepsKeyboardInsideDialog`
- **Screenshot/trace:** `artifacts/e2e/failures/accessibility-performance/modal-focus-trap/`
- **Prostredi:** SQL Server Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:** Otevrit `/dictionaries`, zobrazit dialog `Vytvořit slovník`, fokusovat prvni pole a stisknout Shift+Tab.
- **Ocekavani:** Fokus zustane uvnitr modalniho dialogu a obali se na posledni fokusovatelny prvek.
- **Skutecnost:** Fokus utekl mimo dialog do podkladove stranky.
- **Pravdepodobna pricina:** Custom modaly mely `aria-modal=true`, ale aplikace nemela focus-trap logiku pro Tab/Shift+Tab.
- **Oprava:** Do globalniho `lexiQuestA11y` helperu pridan keydown focus trap pro viditelne `[aria-modal="true"]` dialogy; Tab/Shift+Tab obaluje fokus mezi prvnim a poslednim fokusovatelnym prvkem.
- **Overeni:** `A11y_ModalFocusTrap_KeepsKeyboardInsideDialog` prosel GREEN a cely `AccessibilityPerformanceE2ETests` class prosel 8/8.
- **Poznamky:** Oprava bude globalni pro viditelne `[aria-modal="true"]` dialogy, tedy nejen pro slovniky.

### E2E-BUG-0188: App shell nemel skip link pro rychly presun klavesnici do hlavniho obsahu

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Layout / A11y / Keyboard
- **Nalezeno v testu:** `A11y_TabOrder_ReachesPrimaryControlsAcrossCoreRoutes`
- **Screenshot/trace:** `artifacts/e2e/failures/accessibility-performance/tab-order-core-routes/`
- **Prostredi:** SQL Server Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:** Prihlasit uzivatele, otevrit `/game` a jit klavesou Tab k hlavnim ovladacim prvkum hry.
- **Ocekavani:** Klavesnicovy uzivatel muze rychle preskocit opakujici se topbar/sidebar a dostat se do hlavniho obsahu.
- **Skutecnost:** Fokus zustaval dlouho v app shell navigaci a test nedosahl primarniho herniho tlacitka rozumnou cestou.
- **Pravdepodobna pricina:** Layout mel topbar a sidebar, ale chybel standardni skip link na `<main>`.
- **Oprava:** Do `MainLayout` pridan skip link `Přeskočit na obsah`, `main-content` s `tabindex=-1` a JS helper `lexiQuestA11y.focusById`.
- **Overeni:** `A11y_TabOrder_ReachesPrimaryControlsAcrossCoreRoutes` prosel GREEN a cely `AccessibilityPerformanceE2ETests` class prosel 8/8.
- **Poznamky:** Mobilni menu oprava resila navigaci na malem viewportu; tento nalez resi desktop keyboard tok.

### E2E-BUG-0187: Admin select filtry nemely dostupny label pro asistivni technologie

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Admin / A11y / Formulare
- **Nalezeno v testu:** `A11y_MainRoutes_HaveLabelsMetadataAndNoBasicAuditIssues`
- **Screenshot/trace:** `artifacts/e2e/failures/accessibility-performance/main-routes-basic-a11y-audit/`
- **Prostredi:** SQL Server Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:** Prihlasit admina, otevrit `/admin/words` a spustit basic a11y audit formularovych prvku.
- **Ocekavani:** Selecty filtru obtiznosti/kategorie a podobne admin selecty maji label, `aria-label`, `aria-labelledby` nebo placeholder.
- **Skutecnost:** Renderovany `<select class="tm-select">` nemel dostupny label; `TmFormField` text nebyl navazany na konkretni select.
- **Pravdepodobna pricina:** `TmSelect` generuje vlastni id a `TmFormField` label se na nej nepropisuje jako nativni `<label for>`.
- **Oprava:** Admin `TmSelect` prvky ve word/user filtrech a word modalu jsou obalene nativnim `<label>` se skrytym textem; doplnen globalni `.visually-hidden` a `.select-label-wrapper`.
- **Overeni:** `A11y_MainRoutes_HaveLabelsMetadataAndNoBasicAuditIssues` prosel v ramci celeho `AccessibilityPerformanceE2ETests` classu 8/8.
- **Poznamky:** Oprava se aplikuje i na admin user filtry a admin word modal selecty.

### E2E-BUG-0186: Pocet neprectenych notifikaci nebyl oznamovan jako live region

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Notifications / A11y
- **Nalezeno v testu:** `A11y_GameAndNotificationLiveRegions_AreAnnounced`
- **Screenshot/trace:** `artifacts/e2e/failures/accessibility-performance/live-regions-game-notifications/`
- **Prostredi:** SQL Server Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:** Seedovat neprectenou notifikaci, prihlasit uzivatele a otevrit `/dashboard`.
- **Ocekavani:** Badge s poctem neprectenych notifikaci ma `role=status`, `aria-live=polite`, `aria-atomic=true` a cesky popisek poctu.
- **Skutecnost:** Badge renderoval jen textovy `span` bez live-region atributu.
- **Pravdepodobna pricina:** Komponenta `NotificationBell` resila vizualni badge, ale ne asistivni oznamovani dynamicke zmeny poctu.
- **Oprava:** `NotificationBell` badge ma `role=status`, `aria-live=polite`, `aria-atomic=true` a cesky `aria-label` z resource `UnreadCount`.
- **Overeni:** `A11y_GameAndNotificationLiveRegions_AreAnnounced` prosel GREEN a cely `AccessibilityPerformanceE2ETests` class prosel 8/8.
- **Poznamky:** Timer a game feedback live-region atributy uz mely; chyba se tykala notifikacniho poctu.

### E2E-BUG-0185: Mobilni app layout nema dostupnou navigaci po skryti sidebaru

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Layout / Mobile / A11y
- **Nalezeno v testu:** `A11y_MobileNavigation_MenuIsReachableAndNavigates`
- **Screenshot/trace:** `artifacts/e2e/failures/accessibility-performance/mobile-navigation-menu/`
- **Prostredi:** SQL Server Testcontainer, Chromium headless, mobile viewport 375x812, light theme
- **Reprodukce:** Prihlasit uzivatele na mobilnim viewportu, otevrit `/dashboard` a pokusit se najit mobilni navigacni menu.
- **Ocekavani:** Po skryti desktop sidebaru existuje viditelne a pristupne mobilni menu, ktere umozni prejit na hlavni route.
- **Skutecnost:** CSS na mobilu zuzi sidebar na sirku 0, ale topbar nenabizi nahradni navigacni ovladac.
- **Pravdepodobna pricina:** Desktop sidebar byl responzivne skryt bez doplneni mobile drawer/menu patternu.
- **Oprava:** `MainLayout` ma mobilni hamburger, pristupny drawer s odkazy ze stejnych nav items, close akci a responzivni CSS bez horizontalniho overflow.
- **Overeni:** `A11y_MobileNavigation_MenuIsReachableAndNavigates` prosel GREEN; screenshoty `menu-open.png` a `settings-after-menu-navigation.png` potvrzuji funkcni mobilni navigaci.
- **Poznamky:** Dopad je zasadni pro mobilni pouzitelnost prihlasene casti aplikace.

### E2E-BUG-0184: Akcni tlacitka v dictionary import modalu se prekryvala

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Dictionaries / UX / CSS
- **Nalezeno v testu:** UX review screenshotu `Security_DictionaryInputs_EscapeXssClampLongStringsAndRejectBadFiles`
- **Screenshot/trace:** `artifacts/e2e/screenshots/security-edge/dictionary-inputs-xss-long-files/1366x900/light/invalid-import-file.png`
- **Prostredi:** SQL Server Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:** Otevrit `/dictionaries`, vytvorit slovnik, otevrit import modal a vybrat nevalidni nebo prilis velky soubor.
- **Ocekavani:** Modal ma citelny error stav a akce `Zrusit` / `Importovat slova` se neprekryvaji.
- **Skutecnost:** Modal byl renderovany mimo `.dictionaries-page`, takze nemel dostupne CSS promenne a akce se v nekterych stavech skladaly pres sebe.
- **Pravdepodobna pricina:** Design tokeny pro modal byly scoped jen na wrapper stranky, zatimco backdrop/modaly jsou sibling elementy.
- **Oprava:** CSS promenne presunuty i na `.modal-backdrop`, `dialog-actions` dostal wrap/gap a tlacitka maji stabilni sirku podle obsahu.
- **Overeni:** Security E2E prosly 3/3 a screenshot `invalid-import-file.png` ukazuje oddelena tlacitka bez prekryvu.
- **Poznamky:** Oprava zlepsuje vsechny slovnikove dialogy, nejen import.

### E2E-BUG-0183: Import slovniku neodmital spatny typ souboru a prilis velky soubor srozumitelnym UI stavem

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Dictionaries / Security / File upload
- **Nalezeno v testu:** `Security_DictionaryInputs_EscapeXssClampLongStringsAndRejectBadFiles`
- **Screenshot/trace:** `artifacts/e2e/screenshots/security-edge/dictionary-inputs-xss-long-files/1366x900/light/invalid-import-file.png`
- **Prostredi:** SQL Server Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:** Ve slovnikovem importu vybrat `slova.exe` nebo CSV soubor vetsi nez 1 MB.
- **Ocekavani:** UI soubor odmita lokalizovanou chybou, nezobrazi preview a nedovoli import neplatneho obsahu.
- **Skutecnost:** Neplatny typ nebyl odmítnut pred preview/importem a oversized soubor mohl koncit nejasnou chybou cteni.
- **Pravdepodobna pricina:** `OnImportFileSelected` nacital obsah bez explicitni kontroly pripony a limitu pred `OpenReadStream`.
- **Oprava:** Pridana kontrola `.csv`/`.txt`, limit 1 MB, lokalizovane chyby `Error_ImportInvalidType`, `Error_ImportTooLarge`, `Error_ImportReadFailed` a zakaz import tlacitka pri chybe.
- **Overeni:** RED test cekal na `dictionary-import-error`, po oprave security E2E prosly 3/3 a screenshot potvrzuje jasny error stav bez preview.
- **Poznamky:** Import JSON zustava API-only tok; UI import dokumentuje CSV/TXT formy.

### E2E-BUG-0182: AI no-history tip tvrdil, ze se hraci dari, i kdyz nema historii

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** AI challenge / UX / Lokalizace
- **Nalezeno v testu:** `AIChallenge_NoHistory_ShowsEmptyAnalysisAndChallengeCards`, UX review screenshotu
- **Screenshot/trace:** `artifacts/e2e/screenshots/ai-challenge/no-history-empty-state/1366x900/light/empty-data.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:** Prihlasit noveho uzivatele bez odehranych kol a otevrit `/ai-challenge`.
- **Ocekavani:** Prazdny stav rekne, ze zatim neni dost dat pro presne tipy.
- **Skutecnost:** Tip uvadel `Daří se vám stabilně`, coz u nulove historie vytvari falesny zaver.
- **Pravdepodobna pricina:** `AIChallengeService.GenerateTips` nerozlisoval nulova analyzovana data od hrace bez slabin.
- **Oprava:** Pridan resource `Tip.NoData`, branch pro prazdne weak letters i category performance a regression test `AIChallengeService_Analyze_NoHistory_ShowsNoDataTip`.
- **Overeni:** Core AI testy prosly 8/8 a finalni AI E2E beh 7/7; screenshot obsahuje neutralni text `Zatím nemáme dost dat`.
- **Poznamky:** `Tip.NoWeakness` zustava pro hrace, ktery uz ma historii, ale nema zjistene slabiny.

### E2E-BUG-0181: Full-page screenshot AI tooltipu posouval sticky topbar/sidebar do stredu snimku

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** E2E / Screenshot infra / UX review
- **Nalezeno v testu:** `AIChallenge_StartType_ReusesGameArenaAndShowsWhyThisWordTooltip`
- **Screenshot/trace:** `artifacts/e2e/screenshots/ai-challenge/start-memory-game/1366x900/light/challenge-cards-with-tooltip.png`
- **Prostredi:** Chromium headless, desktop 1366x900, light theme
- **Reprodukce:** Otevrit AI challenge karty, rozbalit tooltip a ulozit full-page screenshot dlouhe stranky se sticky layoutem.
- **Ocekavani:** Screenshot odpovida realnemu viewportu, ve kterem je tooltip posuzovan.
- **Skutecnost:** Full-page kompozice s posunutym sticky layoutem zhorsovala UX review a vypadala jako prekryv UI.
- **Pravdepodobna pricina:** Playwright full-page screenshot skladal dlouhou stranku se sticky prvky po scrollu ke kartam.
- **Oprava:** `TakeCheckpointScreenshotAsync` dostal volby `fullPage` a `scrollToTop`; AI tooltip checkpoint pred otevrenim tooltipu zarovna challenge grid do viewportu a uklada viewport screenshot bez nuceneho scrollu nahoru.
- **Overeni:** AI E2E znovu prosly 7/7 a novy tooltip screenshot ukazuje cely blok karet i tooltip bez sticky artefaktu.
- **Poznamky:** Defaultni chovani screenshot helperu zustava full-page pro stavajici checkpointy.

### E2E-BUG-0180: Karta Paměťová hra pouzivala nepodporovanou ikonu

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** AI challenge / UX / Ikony
- **Nalezeno v testu:** UX review screenshotu `AIChallenge_StartType_ReusesGameArenaAndShowsWhyThisWordTooltip`
- **Screenshot/trace:** `artifacts/e2e/screenshots/ai-challenge/start-memory-game/1366x900/light/challenge-cards-with-tooltip.png`
- **Prostredi:** Chromium headless, desktop 1366x900, light theme
- **Reprodukce:** Otevrit `/ai-challenge` a zkontrolovat ikonu karty `Paměťová hra`.
- **Ocekavani:** Vsechny ctyri challenge karty maji viditelnou a konzistentni ikonu.
- **Skutecnost:** Ikona `brain` se v aktualni Tempo sade nevykreslila spolehlive.
- **Pravdepodobna pricina:** Pouzity icon name neni dostupny v nasazene sade ikon.
- **Oprava:** `AIChallenge.razor` pouziva pro MemoryGame overeny icon name `award`.
- **Overeni:** Finalni AI screenshoty ukazuji ctyri vykreslene ikony a AI E2E probehly 7/7.
- **Poznamky:** Funkcne se nic nemenilo; jde o vizualni stabilitu checkpointu.

### E2E-BUG-0179: Blazor WASM framework asset console error zpusoboval falesne E2E pady

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** E2E / Infra / Diagnostics
- **Nalezeno v testu:** `AIChallenge_StartType_ReusesGameArenaAndShowsWhyThisWordTooltip`
- **Screenshot/trace:** `artifacts/e2e/failures/ai-challenge/start-pattern-recognition/20260621-043332-console.log`
- **Prostredi:** Chromium headless, Blazor WebAssembly, SQL Server Testcontainer, smtp4dev Testcontainer
- **Reprodukce:** Bezet vice AI Blazor navigaci a pak kontrolovat `page.Console` chyby.
- **Ocekavani:** Test pada na aplikacni console errors, ne na benigni framework asset fetch chybu.
- **Skutecnost:** `mono_download_assets` / `TypeError: Failed to fetch` pro `/_framework/*.wasm` shazoval jinak uspesny scenar.
- **Pravdepodobna pricina:** Blazor WASM runtime muze pri ukoncovani/navigaci zalogovat framework fetch chybu, ktera neni chyba testovane funkce.
- **Oprava:** Do E2E diagnostiky pridan uzky filtr jen pro kombinaci `mono_download_assets`, `TypeError: Failed to fetch`, `/_framework/` a `.wasm`.
- **Overeni:** AI E2E prosly opakovane 7/7 a bez maskovani beznych aplikacnich console errors.
- **Poznamky:** Filtr je zamerne uzky, aby neschoval realne API nebo UI problemy.

### E2E-BUG-0178: AI challenge service vracela anglicke/hardcoded texty a nedeterministicky vyber slov

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** AI challenge / Lokalizace / Personalizace
- **Nalezeno v testu:** `AIChallenge_Analysis_ShowsWeakLettersSlowCategoriesAndCzechTips`, `AIChallenge_WeaknessFocus_ChangesWordReasonsAfterHistory`
- **Screenshot/trace:** `artifacts/e2e/screenshots/ai-challenge/analysis-weakness-and-slow-category/1366x900/light/analysis.png`
- **Prostredi:** SQL Server Testcontainer, Chromium headless
- **Reprodukce:** Seedovat spatnou/pomalou historii a otevrit `/ai-challenge`.
- **Ocekavani:** UI a API vraci ceske tipy, ceske nazvy obtiznosti a predikovatelne personalizovane duvody slov.
- **Skutecnost:** Cast duvodu a tipu byla anglicky/hardcoded a vyber slov se opiral o nahodne batch dotazy, coz zhorsovalo stabilitu E2E i personalizaci.
- **Pravdepodobna pricina:** Sluzba byla puvodne prototypovana bez lokalizace a bez deterministickeho orderingu slovniku.
- **Oprava:** `AIChallengeService` pouziva `IStringLocalizer`, cesky `.resx`, deterministicky `GetAllAsync` ordering a ceske metadata challenge.
- **Overeni:** Core AI testy prosly 8/8; E2E overuje `Trénujte`, `Expert`, absenci `Focus` a personalizovane duvody se slabym pismenem.
- **Poznamky:** Tato oprava zaroven zlepsila stabilitu screenshotu i API direct testu.

### E2E-BUG-0177: AI challenge UI neumelo zobrazit preview, tooltip ani spustit realnou AI session

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** AI challenge / Blazor / Game flow
- **Nalezeno v testu:** `AIChallenge_NoHistory_ShowsEmptyAnalysisAndChallengeCards`, `AIChallenge_StartType_ReusesGameArenaAndShowsWhyThisWordTooltip`
- **Screenshot/trace:** `artifacts/e2e/screenshots/ai-challenge/start-weakness-focus/1366x900/light/challenge-cards-with-tooltip.png`, `game-arena.png`, `session-feedback.png`
- **Prostredi:** SQL Server Testcontainer, Chromium headless
- **Reprodukce:** Otevrit `/ai-challenge`, zkusit zjistit proc bylo slovo doporuceno a spustit jeden z typu vyzvy.
- **Ocekavani:** Stranka zobrazi preview slov, tooltip `Proč toto slovo`, stabilni E2E selektory a po startu realne pokracuje do GameArena.
- **Skutecnost:** Puvodni UI melo jen staticke karty bez preview duvodu a navigovalo na query route misto vytvoreni realne session.
- **Pravdepodobna pricina:** Backend AI challenge existoval drive nez kompletni UX a E2E kontrakt pro spusteni hry.
- **Oprava:** `AIChallenge.razor` doplnena o preview loading, tooltip, `data-testid`, realny `GameService.StartGameAsync`, `GameMode.AIChallenge` a responsive CSS.
- **Overeni:** E2E pokryva vsechny ctyri typy challenge, URL `/game/{sessionId}`, GameArena a spatny-answer feedback.
- **Poznamky:** Testy schvalne posilaji spatnou odpoved, aby checkpoint nespadal do achievement modalu prvniho slova.

### E2E-BUG-0176: AI challenge Blazor client volal spatne API routy a neobnovoval bearer autentizaci

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** AI challenge / Auth / API client
- **Nalezeno v testu:** `AIChallenge_NoHistory_ShowsEmptyAnalysisAndChallengeCards`
- **Screenshot/trace:** `artifacts/e2e/failures/ai-challenge/no-history-empty-state/20260621-041800-console.log`
- **Prostredi:** SQL Server Testcontainer, Chromium headless
- **Reprodukce:** Prihlasit uzivatele a otevrit `/ai-challenge`.
- **Ocekavani:** Blazor klient vola `api/v1/challenges/ai/analysis` a `api/v1/challenges/ai/start` s platnym bearer tokenem.
- **Skutecnost:** Klient pouzival nespravne routy/HTTP klienta a AI data se v UI nenacetla spolehlive.
- **Pravdepodobna pricina:** API routy byly zmenene pod `challenges/ai`, ale klientska sluzba zustala ve starsim tvaru.
- **Oprava:** `AIChallengeClient` pouziva `PublicApiClient`, spravne routy a stejny bearer/refresh pattern jako ostatni chranene klienty.
- **Overeni:** AI no-history, analysis a start scenare prochazeji pres realne prihlaseni a API volani.
- **Poznamky:** Bez teto opravy nebylo mozne stabilne testovat ani empty state.

### E2E-BUG-0175: AI challenge API neumelo precist user id z E2E JWT a vracelo 401

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** AI challenge / API / Auth
- **Nalezeno v testu:** `AIChallenge_WeaknessFocus_ChangesWordReasonsAfterHistory`
- **Screenshot/trace:** `artifacts/e2e/failures/ai-challenge/personalized-selection-changes-with-history/20260621-041549-console.log`
- **Prostredi:** SQL Server Testcontainer, authenticated API client
- **Reprodukce:** Zavolat `POST /api/v1/challenges/ai/start` s platnym E2E JWT.
- **Ocekavani:** Endpoint z tokenu precte user id a vrati `AIChallengeDto`.
- **Skutecnost:** Endpoint hledal jen konkretni claim `sub`; E2E JWT pouziva standardni claim mapovani, takze API vracelo 401.
- **Pravdepodobna pricina:** Controller nepouzival sdilenou extension metodu pro ziskani aktualniho user id.
- **Oprava:** `AIChallengeController` pouziva `User.GetUserId()` a korektne zachyti `UnauthorizedAccessException`.
- **Overeni:** API direct E2E test porovnava no-history a weak-history challenge a finalni AI E2E beh prosel 7/7.
- **Poznamky:** Oprava sjednocuje chovani s ostatnimi autorizovanymi endpointy.

### E2E-BUG-0174: Admin users tabulka pretekala pri dlouhem emailu a radkovych akcich

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Admin / User management / UX
- **Nalezeno v testu:** `AdminUsers_TableDetailSuspendUnsuspendResetPassword_WorkEndToEnd`
- **Screenshot/trace:** `artifacts/e2e/screenshots/admin/users-table-detail-suspend-reset/1366x900/light/filtered-table.png`, `detail-drawer.png`, `reset-password-sent.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Prihlasit admina a otevrit `/admin/users`.
  2. Vyfiltrovat uzivatele s delsim emailem.
  3. Zkontrolovat tabulku a radkove akce na desktop viewportu.
- **Ocekavani:** Email se zalomi uvnitr sloupce, akce nepretekaji mimo tabulku a dulezite operace jsou dostupne bez prekryvu.
- **Skutecnost:** Dlouhy email vizualne zasahoval do sousednich sloupcu a sada radkovych akci byla prilis siroka pro stabilni tabulku.
- **Pravdepodobna pricina:** Tabulka nemela pevne colgroup proporce, textove bunky nemely dostatecne zalamovani a reset hesla byl soucasti radkovych akci misto kontextoveho detailu.
- **Oprava:** `AdminUsers.razor.css` dostal pevne sirky sloupcu, `overflow-wrap:anywhere`, validni Tempo tokeny s fallbacky a reset hesla byl presunut do detail draweru.
- **Overeni:** `AdminUsers_TableDetailSuspendUnsuspendResetPassword_WorkEndToEnd` probehl uspesne a screenshoty ukazuji citelnou tabulku i drawer bez prekryvu.
- **Poznamky:** Reset hesla je porad dostupny ve stejnem scenari, ale v kontextu detailu, kde ma vic prostoru i bezpecnejsi UX.

### E2E-BUG-0173: Admin users UI nemelo kompletni detail/reset flow a stabilni E2E selektory

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Admin / User management / Blazor / Email
- **Nalezeno v testu:** `AdminUsers_TableDetailSuspendUnsuspendResetPassword_WorkEndToEnd`
- **Screenshot/trace:** `artifacts/e2e/screenshots/admin/users-table-detail-suspend-reset/1366x900/light/detail-drawer.png`, `reset-password-sent.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless
- **Reprodukce:**
  1. Otevrit `/admin/users` jako admin.
  2. Zkusit pres UI otevrit detail uzivatele a odeslat reset hesla.
  3. Overit doruceni reset emailu ve smtp4dev.
- **Ocekavani:** Admin vidi detail uzivatele vcetne statistik, muze pozastavit/obnovit ucet a odeslat reset hesla s viditelnym potvrzenim.
- **Skutecnost:** Blazor klient nemel metody pro detail/reset flow, UI nemelo dostatek stabilnich `data-testid` selektoru a reset potvrzeni nebylo spolehlive videt ve screenshotu.
- **Pravdepodobna pricina:** Backend admin user endpointy byly pripraveny drive nez kompletni Blazor administrace a E2E kontrakt.
- **Oprava:** Doplnene `GetUserAsync`, `ResetUserPasswordAsync`, detail drawer, suspend/unsuspend/reset akce, persistentni inline potvrzeni resetu a `Selectors.AdminUsers`.
- **Overeni:** Test overuje API stav `IsSuspended`, UI status po suspend/unsuspend, inline potvrzeni `E-mail pro obnovení hesla byl odeslán.` a smtp4dev email se subjectem `Obnovení hesla - LexiQuest`.
- **Poznamky:** Reset link se kontroluje proti E2E web base URL, aby se nepouzila vyvojova nebo produkcni adresa.

### E2E-BUG-0172: ContentManager route guard blokoval spravu slov, i kdyz API roli povolovalo

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Admin / Auth / Roles / Blazor
- **Nalezeno v testu:** `Admin_ContentManager_CanManageWordsButCannotManageUsers`
- **Screenshot/trace:** `artifacts/e2e/screenshots/admin/content-manager-role-boundary/1366x900/light/words-access.png`
- **Prostredi:** SQL Server Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Vytvorit uzivatele s roli `ContentManager`.
  2. Otevrit `/admin/words`.
  3. Otevrit `/admin/users`.
- **Ocekavani:** ContentManager smi spravovat slova, ale nesmi spravovat uzivatele ani videt admin user page.
- **Skutecnost:** UI guard pro `/admin/words` pouzival pouze obecnou admin kontrolu, takze ContentManager nemel pristup ke sprave slov.
- **Pravdepodobna pricina:** Frontend route guard nerozlisoval admin-only a content-management opravnene oblasti.
- **Oprava:** Pridan endpoint `/api/v1/admin/check/words`, klientsky `CanManageWordsAsync` a `AdminWords.razor` pouziva content-management guard misto admin-only guardu.
- **Overeni:** Test overuje API `200 OK` pro `/api/v1/admin/words`, `403 Forbidden` pro `/api/v1/admin/users`, viditelnou `/admin/words` UI stranku a presmerovani mimo `/admin/users`.
- **Poznamky:** Dashboard a sprava uzivatelu zustavaji pouze pro roli `Admin`.

### E2E-BUG-0171: Admin user list vynechaval nove uzivatele bez `LastLoginAt`

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Admin / User management / API
- **Nalezeno v testu:** `AdminUsers_TableDetailSuspendUnsuspendResetPassword_WorkEndToEnd`
- **Screenshot/trace:** `artifacts/e2e/screenshots/admin/users-table-detail-suspend-reset/1366x900/light/filtered-table.png`
- **Prostredi:** SQL Server Testcontainer, Chromium headless
- **Reprodukce:**
  1. Vytvorit noveho uzivatele pres registraci v E2E setupu.
  2. Nastavit mu statistiky primo v testovaci DB.
  3. Vyhledat jeho email v `/api/v1/admin/users` a `/admin/users`.
- **Ocekavani:** Admin seznam obsahuje vsechny registrovane uzivatele vcetne tech, kteri jeste nemaji `LastLoginAt`.
- **Skutecnost:** Service skladala seznam z aktivnich uzivatelu a neaktivnich pres hranici `0` dni; uzivatel bez `LastLoginAt` do vysledku nepropadl.
- **Pravdepodobna pricina:** Admin listing znovupouzival analyticke repository metody misto jednoducheho `GetAll` dotazu pro spravu uzivatelu.
- **Oprava:** Do `IUserRepository` a EF repository pridan `GetAllAsync` vcetne potrebnych include navigaci; `AdminUserService.GetUsersAsync` pouziva tento dotaz pred filtrovani/paginaci.
- **Overeni:** `AdminUsers_TableDetailSuspendUnsuspendResetPassword_WorkEndToEnd` uspesne najde nove vytvoreneho uzivatele podle emailu a level filtru.
- **Poznamky:** Zmena je izolovana na admin listing; aktivni/neaktivni metriky dashboardu zustavaji na puvodnich specializovanych metodach.

### E2E-BUG-0170: Admin stranky pouzivaly neexistujici Tempo CSS tokeny bez fallbacku

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Admin / UX / CSS
- **Nalezeno v testu:** `Admin_DashboardStatsCards_ShowRealCounts`, `AdminWords_TableSearchFilterPaginationColumnPicker_WorkEndToEnd`, `AdminWords_ImportDuplicatesExportAndStats_WorkEndToEnd`
- **Screenshot/trace:** `artifacts/e2e/screenshots/admin/dashboard-stats-cards/1366x900/light/stats-cards.png`, `artifacts/e2e/screenshots/admin/words-table-filter-pagination-columns/1366x900/light/filtered-column-picker.png`, `artifacts/e2e/screenshots/admin/words-import-export-stats/1366x900/light/stats-drawer.png`
- **Prostredi:** Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Otevrit `/admin` nebo `/admin/words`.
  2. Porovnat zamyslene `.razor.css` hodnoty s realnym screenshotem.
  3. Zkontrolovat padding, gapy, border a drawer panel.
- **Ocekavani:** Admin karty, filtry, table frame a drawer pouzivaji stabilni spacing, border a barvy z design systemu.
- **Skutecnost:** Cast CSS deklaraci se zahazovala, protoze pouzivala `--tm-spacing-*`, `--tm-color-text-*`, `--tm-color-border` a dalsi tokeny bez fallbacku; v runtime jsou dostupne napr. `--tm-space-*` a `--tm-text-*`.
- **Pravdepodobna pricina:** Chybny odhad nazvu Tempo.Blazor tokenu pri novem admin CSS.
- **Oprava:** `AdminDashboard.razor.css` a `AdminWords.razor.css` byly prepsany na existujici tokeny s fallback hodnotami, napr. `var(--tm-space-4, 1rem)`, `var(--tm-text-primary, #111827)` a `var(--tm-border-color, #e5e7eb)`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --no-build --filter "FullyQualifiedName~Admin_"` probehl 2/2 a `--filter "FullyQualifiedName~AdminWords_"` probehl 3/3. Nove screenshoty ukazuji citelne karty, filtry, tabulku a drawer.
- **Poznamky:** Screenshot review zachytil problem, ktery by ciste DOM aserce neodhalily.

### E2E-BUG-0169: Admin word stats drawer a modal screenshoty mely rozpadly overlay/backdrop

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Admin / UX / Screenshot
- **Nalezeno v testu:** `AdminWords_ImportDuplicatesExportAndStats_WorkEndToEnd`
- **Screenshot/trace:** `artifacts/e2e/screenshots/admin/words-import-export-stats/1366x900/light/import-result.png`, `artifacts/e2e/screenshots/admin/words-import-export-stats/1366x900/light/stats-drawer.png`
- **Prostredi:** Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Otevrit `/admin/words` jako admin.
  2. Importovat CSV a nechat otevreny import modal.
  3. Otevrit stats drawer nad tabulkou s mnoha radky.
- **Ocekavani:** Dialog/drawer maji jasny panel, pokryty backdrop a podkladova tabulka nerusi rozhodujici obsah screenshotu.
- **Skutecnost:** Drawer byl pred opravou castečně mimo vizualni fokus, podkladova tabulka prosvitala a full-page screenshot ukazoval spodní cast dokumentu mimo backdrop.
- **Pravdepodobna pricina:** Nizky stacking context proti app layoutu, neplatne spacing tokeny a prilis vysoka tabulka pod modalem.
- **Oprava:** Drawer/backdrop dostaly vysoky `z-index`, pevny viewport panel, explicitni pozadi a padding; tabulka slov ma stabilni `max-height` a scroll uvnitr ramce.
- **Overeni:** `AdminWords_ImportDuplicatesExportAndStats_WorkEndToEnd` probehl uspesne 3/3 v admin-word sade; nove screenshoty `import-result.png` a `stats-drawer.png` jsou citelne bez nekryte spodni casti.
- **Poznamky:** V realnem viewportu slo hlavne o screenshot/UX artefakt, ale pro vizualni regresi je dulezity.

### E2E-BUG-0168: Admin word filtry a column picker nebyly UX stabilni ani plne overitelne

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Admin / Word management / UX
- **Nalezeno v testu:** `AdminWords_TableSearchFilterPaginationColumnPicker_WorkEndToEnd`
- **Screenshot/trace:** `artifacts/e2e/screenshots/admin/words-table-filter-pagination-columns/1366x900/light/filtered-column-picker.png`
- **Prostredi:** SQL Server Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Otevrit `/admin/words`.
  2. Filtrovat podle slova `programovani` a delky 10-12.
  3. Otevrit vyber sloupcu a skryt kategorii.
- **Ocekavani:** Filtry maji jasne popisky, min/max delka se odesila jako cislo, column picker je citelny ovladaci blok a tabulka nezmeni layout necekanym zpusobem.
- **Skutecnost:** Pred opravou byla cast ovladani syrova radka checkboxu, min/max pole se nechovala spolehlive pro E2E a tabulka/paginace pusobily rozpadle v prazdnem prostoru.
- **Pravdepodobna pricina:** Prvni admin word page mela jen zakladni scaffold bez UX dotazeni a bez stabilnich E2E affordances pro filtry/sloupce.
- **Oprava:** Filtry byly zaramovane do jednoho panelu, min/max delka pouziva native number input, column picker je soucast filtru jako kompaktní panel a tabulka ma pevny ram, sticky header a stabilni scroll.
- **Overeni:** `AdminWords_TableSearchFilterPaginationColumnPicker_WorkEndToEnd` overuje vyhledavani, delkove filtry, paginaci i skryti kategorie a screenshot po oprave prosel UX review.
- **Poznamky:** Funkcni aserce kontroluji i to, ze kategorie sloupec po vypnuti skutecne zmizi.

### E2E-BUG-0167: Admin word create/edit modal mel spatne ovladani a enum hodnoty

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Admin / Word management / Blazor
- **Nalezeno v testu:** `AdminWords_CreateEditDelete_WorkEndToEnd`
- **Screenshot/trace:** `artifacts/e2e/screenshots/admin/words-create-edit-delete/1366x900/light/delete-confirm.png`
- **Prostredi:** SQL Server Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Otevrit `/admin/words`.
  2. Kliknout `Pridat slovo`.
  3. Vyplnit slovo, obtiznost a kategorii, ulozit a nasledne upravit.
- **Ocekavani:** Modal se otevře, ulozi validni `DifficultyLevel`/`WordCategory` hodnoty a editace se propise do tabulky.
- **Skutecnost:** Pred opravou nebyl modal spolehlive otevren pres aktualni Tempo API a volby obtiznosti nebyly sladene s domenovym enumem `Beginner/Intermediate/Advanced/Expert`.
- **Pravdepodobna pricina:** Admin stranka pouzivala starsi modal binding a historicke obtiznosti z drivejsiho navrhu.
- **Oprava:** Modal pouziva `Show`/`OnClose`, selecty pouzivaji `SelectOption.From(...)` s realnymi enum hodnotami a create/edit requesty posilaji platny shared contract.
- **Overeni:** `AdminWords_CreateEditDelete_WorkEndToEnd` probehl uspesne; test vytvori slovo, upravi ho, vyhleda novy stav a otevre delete confirm modal.
- **Poznamky:** Delete samotny se potvrzuje v navazujicim kroku testu po screenshot checkpointu.

### E2E-BUG-0166: Admin word-management nemel kompletni Blazor klientsky contract pro CRUD/import/export/stats

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Admin / API / Blazor
- **Nalezeno v testu:** `AdminWords_TableSearchFilterPaginationColumnPicker_WorkEndToEnd`, `AdminWords_CreateEditDelete_WorkEndToEnd`, `AdminWords_ImportDuplicatesExportAndStats_WorkEndToEnd`
- **Screenshot/trace:** `artifacts/e2e/screenshots/admin/words-table-filter-pagination-columns/1366x900/light/filtered-column-picker.png`, `artifacts/e2e/screenshots/admin/words-import-export-stats/1366x900/light/import-result.png`
- **Prostredi:** SQL Server Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit admin uzivatele.
  2. Otevrit `/admin/words`.
  3. Zkusit editovat slovo, importovat CSV, exportovat CSV a otevrit statistiky.
- **Ocekavani:** Admin muze spravovat slovnik kompletne pres UI a Blazor klient vola `/api/v1/admin/words` endpointy s bearer tokenem.
- **Skutecnost:** Klientsky `IAdminService`/`AdminService` nemel metody pro update/import/export/stats a import request DTO nebylo sdilene s Blazorem; export nemel browser helper.
- **Pravdepodobna pricina:** Backend admin endpoints existovaly drive nez byl dodelany plny Blazor E2E use case.
- **Oprava:** Doplnene metody `UpdateWordAsync`, `ImportWordsAsync`, `ExportWordsAsync`, `GetWordStatsAsync`, shared `AdminWordImportRequest`, manual bearer/refresh pattern v admin klientovi a `window.lexiQuestAdmin.downloadTextFile` helper.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --no-build --filter "FullyQualifiedName~AdminWords_" -m:1 /p:BuildInParallel=false /v:m` probehl uspesne 3/3.
- **Poznamky:** Export test uklada stazene CSV do `artifacts/e2e/downloads/admin/words-import-export-stats/words-export.csv`.

### E2E-BUG-0165: API build output se zacyklil do `bin/Debug/net10.0/bin/...`

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Infra / E2E
- **Nalezeno v testu:** `Admin_NonAdminRouteGuard_RedirectsAndApiForbids`, `Admin_DashboardStatsCards_ShowRealCounts`
- **Screenshot/trace:** E2E stdout/stderr z behu `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --no-build --filter "FullyQualifiedName~Admin_"`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, API start pres `dotnet run`
- **Reprodukce:**
  1. Spustit admin E2E testy po opakovanem `dotnet run` API projektu.
  2. API proces se ukonci behem buildu.
- **Ocekavani:** API proces se prelozi a nabehne na `/health/live` a `/health/ready`.
- **Skutecnost:** MSBuild se pokusil kopirovat soubory z rekurzivne zanorene cesty `src/LexiQuest.Api/bin/Debug/net10.0/bin/Debug/net10.0/...` a selhal na prilis dlouhe ceste.
- **Pravdepodobna pricina:** Web SDK v urcitem stavu build artefaktu zahrnulo vlastni vystup do dalsiho content kopirovani.
- **Oprava:** Smazany build artefakty `src/LexiQuest.Api/bin` a `src/LexiQuest.Api/obj`; do `LexiQuest.Api.csproj` pridany explicitni `DefaultItemExcludes` pro `bin/**` a `obj/**`.
- **Overeni:** `dotnet build src/LexiQuest.Api/LexiQuest.Api.csproj --no-restore` probehl uspesne a nasledne admin E2E testy probehly 2/2.
- **Poznamky:** Jde o build artefakt, ne produkcni runtime chybu.

### E2E-BUG-0164: Admin dashboard stat karty byly v desktop screenshotu stisnene a spatne citelne

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Admin / UX
- **Nalezeno v testu:** `Admin_DashboardStatsCards_ShowRealCounts`
- **Screenshot/trace:** `artifacts/e2e/screenshots/admin/dashboard-stats-cards/1366x900/light/stats-cards.png`
- **Prostredi:** Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Prihlasit admin uzivatele.
  2. Otevrit `/admin`.
  3. Zkontrolovat stat karty v prvnim radku dashboardu.
- **Ocekavani:** Hodnota, nazev metriky a pomocny text jsou v karte rychle skenovatelne a bez nehezkeho lamani.
- **Skutecnost:** Puvodni `TmStatCard` layout skladal cislo, titulky a subtext do stisnene horizontalni kompozice s rusivym zalamovanim.
- **Pravdepodobna pricina:** Genericka stat karta nebyla vhodna pro uzsi ctyrsloupcovy admin dashboard.
- **Oprava:** Dashboard pouziva vlastni kompaktni `.admin-stat-card` bloky se stabilni hodnotou, titulkem a subtextem.
- **Overeni:** `Admin_DashboardStatsCards_ShowRealCounts` probehl uspesne a novy screenshot ukazuje citelne metriky bez prekryvu.
- **Poznamky:** Funkcne slo o polish, ale screenshot review to zachytil pred odskrtnutim admin dashboardu.

### E2E-BUG-0163: Admin panel nemel funkcni autorizacni a dashboard cestu pro Blazor UI

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Admin / Auth / Blazor / API
- **Nalezeno v testu:** `Admin_NonAdminRouteGuard_RedirectsAndApiForbids`, `Admin_DashboardStatsCards_ShowRealCounts`
- **Screenshot/trace:** `artifacts/e2e/screenshots/admin/non-admin-route-guard/1366x900/light/redirected-home.png`, `artifacts/e2e/screenshots/admin/dashboard-stats-cards/1366x900/light/stats-cards.png`
- **Prostredi:** SQL Server Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit admin uzivatele s roli v `AdminRoleAssignments`.
  2. Otevrit `/admin`.
  3. Zkusit nacist dashboard statistiky.
- **Ocekavani:** Admin projde route guardem a vidi dashboard statistiky; neadmin je presmerovan a data endpoint vraci 403.
- **Skutecnost:** Blazor `AdminService` volal `api/admin/...` misto `api/v1/admin/...`, pouzival problematicky `ApiClient`, API nemelo `/api/v1/admin/check` ani `/dashboard/stats` a JWT principal neobsahoval role z `AdminRoleAssignments`.
- **Pravdepodobna pricina:** Admin frontend vznikl driv nez finalni API route/role model a nebyl pokryt E2E.
- **Oprava:** Pridan `AdminController` pro `/api/v1/admin/check` a `/api/v1/admin/dashboard/stats`, JWT bearer validace doplnuje role z DB, `AdminService` pouziva `PublicApiClient`, rucni bearer token a refresh/retry.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --no-build --filter "FullyQualifiedName~Admin_"` probehl uspesne 2/2.
- **Poznamky:** `/api/v1/admin/check` vraci `true/false` bez 403 pro prihlaseneho uzivatele, aby route guard nevyrabel ocekavanou konzolovou chybu; chranena data zustavaji pod `Authorize(Roles = "Admin")`.

### E2E-BUG-0162: Game start po UI loginu pada na 401 pres LexiQuestApi klienta

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Game / Auth / Blazor
- **Nalezeno v testu:** `Notifications_AchievementUnlocked_ShowsToastAndInAppNotification`
- **Screenshot/trace:** `artifacts/e2e/failures/notifications/achievement-unlocked-toast-notification/20260620-045950.png`
- **Prostredi:** SQL Server Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit uzivatele pres E2E `LoginAsAsync`.
  2. Otevrit `/game`.
  3. Kliknout na treningovy mod.
- **Ocekavani:** `POST /api/v1/game/start` dostane bearer token a zalozi hru.
- **Skutecnost:** API vratilo `401`, UI ukazalo `Error_StartingGame` a herni arena se nezobrazila.
- **Pravdepodobna pricina:** `GameService` spolehal na `LexiQuestApi` s `AuthorizationMessageHandler`, ktery v Blazor Web App rezimu nema spolehlivy pristup ke scoped tokenu.
- **Oprava:** `GameService` pouziva `PublicApiClient`, rucne priklada bearer token z `IAuthService`, pri `401` provede refresh a request opakuje. Kryto testem `GameService_StartGame_AuthenticatedUser_SendsBearerToken`.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~GameServiceTests|FullyQualifiedName~GamePageTests|FullyQualifiedName~NotificationBell"` probehl uspesne 30/30. `Notifications_AchievementUnlocked_ShowsToastAndInAppNotification` probehl po oprave uspesne 1/1 a screenshoty ukazuji rozehranou hru bez `Error_StartingGame`.
- **Poznamky:** Stejny auth pattern je potreba pouzivat u dalsich protected klientskych sluzeb podle potreby.

### E2E-BUG-0161: Achievement unlock nevytvari okamzitou in-app notifikaci a toast

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Notifications / Achievements
- **Nalezeno v testu:** `Notifications_AchievementUnlocked_ShowsToastAndInAppNotification`
- **Screenshot/trace:** `artifacts/e2e/screenshots/notifications/achievement-unlocked-toast-notification/1366x900/light/toast-and-modal.png`, `notification-dropdown.png`
- **Prostredi:** SQL Server Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Zapnout `AchievementNotifications=true`.
  2. Odemknout prvni achievement ve hre.
  3. Zkontrolovat toast, notification bell a API seznam notifikaci.
- **Ocekavani:** Unlock zobrazi toast, vytvori unread `AchievementUnlocked` notifikaci a bell se aktualizuje bez cekani na polling.
- **Skutecnost:** Achievement unlock zobrazoval modal, ale neexistovala samostatna achievement notifikace/toast refresh cesta pro notification bell.
- **Pravdepodobna pricina:** `AchievementService` resil progress/odmenu, ale nevolal `INotificationService`; `Game.razor` po unlocku neinformoval notification bell.
- **Oprava:** `AchievementService` posila lokalizovanou `AchievementUnlocked` notifikaci s action URL `/achievements`; `Game.razor` zobrazuje toast a vola `NotificationRefreshService`, ktery okamzite refreshne `NotificationBell`.
- **Overeni:** `AchievementServiceTests` prosly 8/8, `GamePageTests`/`NotificationBell` subset prosel a `Notifications_AchievementUnlocked_ShowsToastAndInAppNotification` probehl uspesne 1/1. Screenshoty potvrzuji toast, modal, badge `1` i dropdown s notifikaci `Úspěch odemčen`.
- **Poznamky:** Email/push preference pro achievementy zustavaji pro dalsi rozsireni mimo tento scenar.

### E2E-BUG-0160: Game HUD tiska zivoty a combo badge tesne k sobe

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Game / UX
- **Nalezeno v testu:** `Notifications_AchievementUnlocked_ShowsToastAndInAppNotification`
- **Screenshot/trace:** `artifacts/e2e/screenshots/notifications/achievement-unlocked-toast-notification/1366x900/light/toast-and-modal.png`, `notification-dropdown.png`
- **Prostredi:** SQL Server Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Odemknout achievement ve hre po spravne odpovedi.
  2. Zkontrolovat herni HUD v pozadi achievement modalu nebo pod otevrenym notification dropdownem.
- **Ocekavani:** `Životy: ∞` a combo badge jsou vizualne oddelene a necini jeden slepeny retezec.
- **Skutecnost:** Text `Životy:∞` a `x1 COMBO` byly natlacene tesne vedle sebe.
- **Pravdepodobna pricina:** `GameArena` header mel prilis maly gap a lives/combobadge nemely nowrap/fixed sizing.
- **Oprava:** Scoped CSS `GameArena.razor.css` ma vetsi header gap a `white-space: nowrap`/stabilni sizing pro lives indicator a combo badge.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~GameArenaTests"` probehl uspesne 19/19. `Notifications_AchievementUnlocked_ShowsToastAndInAppNotification` probehl uspesne 1/1 a screenshoty `toast-and-modal.png`/`notification-dropdown.png` byly po CSS fixu zkontrolovany.
- **Poznamky:** Funkcni dopad nizky, ale screenshot review to zachytilo jako polish problem.

### E2E-BUG-0159: Premium page nenacita status po UI loginu kvuli 401 z ApiClient

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Premium / Auth / Blazor
- **Nalezeno v testu:** `Premium_ExpiryReminderEmail_IsCapturedBySmtp4Dev`
- **Screenshot/trace:** `artifacts/e2e/failures/premium/expiry-reminder-email-smtp4dev/20260620-050601.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit premium uzivatele pres UI.
  2. Otevrit `/premium`.
  3. Sledovat requesty na `/api/v1/premium/status` a `/api/v1/premium/features`.
- **Ocekavani:** Premium page nacte autorizovany status a zobrazi active badge.
- **Skutecnost:** Web log ukazal `401` pro `/api/v1/premium/status` i `/api/v1/premium/features`, takze UI zustalo bez active badge.
- **Pravdepodobna pricina:** Klientsky `PremiumService` spolehal na `ApiClient` s `AuthorizationMessageHandler`, ktery v Blazor Web App rezimu nema spolehlivy pristup k tokenu.
- **Oprava:** `PremiumService` pouziva stejny vzor jako `NotificationService`/`StatsService`, tedy `PublicApiClient`, token primo z `IAuthService`, refresh pri `401` a opakovani requestu.
- **Overeni:** `Premium_ExpiryReminderEmail_IsCapturedBySmtp4Dev` probehl uspesne 1/1. Screenshot `artifacts/e2e/screenshots/premium/expiry-reminder-email-smtp4dev/1366x900/light/active-expiring-soon.png` ukazuje `Aktivní Premium - Měsíční`, aktivni funkce a spravne disabled aktualni plan.
- **Poznamky:** Stejny typ chyby byl uz overene opraven u notification bellu.

### E2E-BUG-0158: Premium expiry reminder email neni implementovany ani spustitelny v E2E

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Premium / Email / E2E
- **Nalezeno v testu:** `PremiumExpiryReminderJob_ExpiringWithinThreeDays_SendsEmail`, `Premium_ExpiryReminderEmail_IsCapturedBySmtp4Dev`
- **Screenshot/trace:** `artifacts/e2e/screenshots/premium/expiry-reminder-email-smtp4dev/1366x900/light/active-expiring-soon.png`
- **Prostredi:** Unit test + SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless
- **Reprodukce:**
  1. Nastavit aktivni premium subscription s expiraci do 3 dnu.
  2. Spustit premium expiry reminder.
  3. Zkontrolovat smtp4dev inbox.
- **Ocekavani:** Uzivatel dostane cesky email `Premium brzy vyprší` zachyceny ve smtp4dev.
- **Skutecnost:** V aplikaci existoval jen `SubscriptionExpirationJob`, ktery expirovane subscription oznaci jako expired; reminder email 3 dny pred expiraci ani E2E endpoint neexistovaly.
- **Pravdepodobna pricina:** Faze Premium mela implementovanou expiraci statusu, ale ne samostatny predexpiracni email reminder.
- **Oprava:** Doplněn `PremiumExpiryReminderJob`, repository dotaz na aktivni subscription expirovane v okne 0-3 dny, lokalizovane texty, DI registrace a E2E endpoint `/api/v1/e2e/premium/run-expiry-reminders`.
- **Overeni:** `dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj --filter "FullyQualifiedName~PremiumExpiryReminderJob"` probehl uspesne 2/2. `Premium_ExpiryReminderEmail_IsCapturedBySmtp4Dev` probehl uspesne 1/1; smtp4dev zachytil cesky email `Premium brzy vyprší` a screenshot potvrzuje aktivni expirovany-premium stav v UI.
- **Poznamky:** Lifetime subscription se nema upozornovat jako expirovana.

### E2E-BUG-0157: Achievement toast prekryva notification bell a blokuje kliknuti

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Notifications / Achievements / UX
- **Nalezeno v testu:** `Notifications_AchievementUnlocked_ShowsToastAndInAppNotification`
- **Screenshot/trace:** `artifacts/e2e/failures/notifications/achievement-unlocked-toast-notification/20260620-045422.png`, `artifacts/e2e/traces/notifications-achievement-unlocked-toast-notification-20260620-045422.zip`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit noveho uzivatele s povolenymi achievement notifikacemi.
  2. Odemknout prvni achievement ve hre.
  3. Zavrit achievement modal a kliknout na notification bell, dokud je toast stale viditelny.
- **Ocekavani:** Toast potvrdi odemceni, ale neblokuje topbar akce; zvonek jde okamzite otevrit.
- **Skutecnost:** `TmToastContainer` v `TopRight` pozici prekryva notification bell a Playwright click timeoutuje na interceptu pointer events.
- **Pravdepodobna pricina:** Topbar ma vpravo interaktivni akce a toast container sdili stejnou oblast viewportu.
- **Oprava:** Hlavni `TmToastContainer` v `MainLayout` je presunut do `BottomRight`, kde neblokuje topbar ani notification dropdown.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Notifications_AchievementUnlocked_ShowsToastAndInAppNotification"` probehl uspesne 1/1. Screenshoty `artifacts/e2e/screenshots/notifications/achievement-unlocked-toast-notification/1366x900/light/toast-and-modal.png` a `notification-dropdown.png` potvrzuji viditelny toast, modal i otevreny dropdown bez prekryvu topbar akci.
- **Poznamky:** Toast samotny je uzitecny feedback, problem je jen jeho kolize s topbar ovladanim.

### E2E-BUG-0156: Notifikacni E2E testy resetuji sdilenou DB pri paralelnim behu kolekce

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** E2E Infra / Test isolation
- **Nalezeno v testu:** `NotificationsE2ETests`
- **Screenshot/trace:** n/a
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, xUnit kolekce
- **Reprodukce:**
  1. Spustit vice metod z `NotificationsE2ETests` ve stejne xUnit kolekci.
  2. Kazdy scenar vola `RunScenarioAsync`, ktery resetuje sdilenou SQL Server Testcontainer databazi.
  3. Paralelni metoda muze mezitim ztratit uzivatele, preference nebo notifikace vytvorene jinym scenarem.
- **Ocekavani:** E2E scenare se sdilenou aplikaci, DB a smtp4dev se v jedne kolekci nespousti paralelne, pokud pouzivaji globalni reset databaze.
- **Skutecnost:** xUnit mohl spustit testovaci metody soubezne a reset jednoho scenare ovlivnil druhy scenar.
- **Pravdepodobna pricina:** `E2ECollection` nebyla oznacena `DisableParallelization = true`, presto sdili `E2EEnvironmentFixture` a resetuje stejnou DB.
- **Oprava:** `E2ECollection` je definovana jako `[CollectionDefinition(Name, DisableParallelization = true)]`, takze scenare v kolekci bezi sekvencne.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~NotificationsE2ETests"` probehl uspesne 6/6 po serializaci kolekce. Navazny scenar `Notifications_DailyChallengeReminderJob_SendsEmailPushAndInAppNotification` probehl samostatne uspesne 1/1.
- **Poznamky:** Paralelizaci lze pozdeji vratit jen s izolovanou DB/schema per test nebo bez globalniho resetu.

### E2E-BUG-0155: Daily challenge reminder job posila anglicke hardcoded texty

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Notifications / Localization
- **Nalezeno v testu:** `DailyChallengeReminderJob_Execute_SendsNotificationToActiveUsers`, `Notifications_DailyChallengeReminderJob_SendsEmailPushAndInAppNotification`
- **Screenshot/trace:** `artifacts/e2e/screenshots/notifications/daily-challenge-reminder-email-push-in-app/1366x900/light/all-channels.png`
- **Prostredi:** Unit test + SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Spustit `DailyChallengeReminderJob`.
  2. Zkontrolovat title/message vytvorene `DailyChallenge` notifikace.
- **Ocekavani:** Uzivatelske texty jsou cesky a z `.resx` resources.
- **Skutecnost:** Job pouzival hardcoded anglicke `Daily Challenge` a `A new daily challenge is available! Can you beat today's challenge?`.
- **Pravdepodobna pricina:** Job nebyl napojen na lokalizaci pri puvodni implementaci notifikaci.
- **Oprava:** `DailyChallengeReminderJob` pouziva `IStringLocalizer<DailyChallengeReminderJob>` a novy resource `src/LexiQuest.Core/Resources/Services/DailyChallengeReminderJob.resx`.
- **Overeni:** `dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj --filter "FullyQualifiedName~DailyChallengeReminderJobTests"` probehl uspesne 1/1. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Notifications_DailyChallengeReminderJob_SendsEmailPushAndInAppNotification"` probehl uspesne 1/1; smtp4dev zachytil cesky email, lokalni push endpoint dostal cesky payload a screenshot zobrazuje ceskou in-app notifikaci.
- **Poznamky:** Pri E2E overeni byl upraven `Smtp4DevClient`, aby pro emailova tela kontroloval i HTML-decoded obsah s ceskou diakritikou.

### E2E-BUG-0154: Notification bell po loginu nenacita unread count pres autorizovany klient

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Notifications / Auth / Blazor
- **Nalezeno v testu:** `NotificationsE2ETests.Notifications_EmailDisabled_DoesNotSendEmailButKeepsInAppNotification`
- **Screenshot/trace:** `artifacts/e2e/failures/notifications/email-disabled-respects-preference/20260620-063319.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Vytvorit unread in-app notifikaci pro prihlasovaneho uzivatele.
  2. Prihlasit se pres UI a prejit na `/dashboard`.
  3. Zkontrolovat notification bell badge.
- **Ocekavani:** Bell ukaze badge `1` a dropdown zobrazi unread notifikaci.
- **Skutecnost:** API z browser kontextu vracelo unread count `1`, ale `NotificationService` volany pres `ApiClient` dostaval `401`, proto badge zustal skryty.
- **Pravdepodobna pricina:** `AuthorizationMessageHandler` vytvareny pres `HttpClientFactory` bezi mimo Blazor circuit scope, a proto nema spolehlivy pristup k tokenu z `IAuthService`/JS interopu v Interactive Auto rezimu.
- **Oprava:** Klientsky `NotificationService` pouziva stejny vzor jako `StatsService`: vytvari requesty rucne, cte token primo z komponentoveho `IAuthService`, pri `401` provede refresh a request zopakuje. `AuthService` navic drzi tokeny ve scoped pameti po uspesnem loginu/refreshi.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~SettingsPageTests|FullyQualifiedName~NotificationBell|FullyQualifiedName~AuthServiceTests"` probehl uspesne 18/18. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Notifications_EmailDisabled_DoesNotSendEmailButKeepsInAppNotification"` probehl uspesne 1/1 a screenshot `artifacts/e2e/screenshots/notifications/email-disabled-respects-preference/1366x900/light/in-app-only.png` zobrazuje badge `1` i dropdown.
- **Poznamky:** Stejny pattern muze byt potreba postupne proverit i u dalsich klientskych sluzeb, ktere spolehaji na `ApiClient` s delegating handlerem.

### E2E-BUG-0153: Streak reminder job posila anglicke hardcoded texty

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Notifications / Localization
- **Nalezeno v testu:** pripravovany `Notifications_StreakWarningJob_SendsEmailPushAndInAppNotification`, regression `StreakReminderJob_Execute_SendsNotificationToUsersWhoHaventPlayedToday`
- **Screenshot/trace:** n/a
- **Prostredi:** Unit test + pripravovany E2E screenshot dropdownu
- **Reprodukce:**
  1. Spustit `StreakReminderJob`.
  2. Zkontrolovat title/message vytvorene `StreakWarning` notifikace.
- **Ocekavani:** Uzivatelske texty jsou cesky a z `.resx` resources.
- **Skutecnost:** Job pouzival hardcoded anglicke `Streak Warning` a `Your streak is at risk! Play now to keep it alive.`.
- **Pravdepodobna pricina:** Faze 6 job vznikl bez napojeni na lokalizaci.
- **Oprava:** `StreakReminderJob` pouziva `IStringLocalizer<StreakReminderJob>` a novy resource `src/LexiQuest.Core/Resources/Services/StreakReminderJob.resx`.
- **Overeni:** `dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj --filter "FullyQualifiedName~NotificationJobTests|FullyQualifiedName~StreakReminderJobTests"` probehl uspesne. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Notifications_StreakWarningJob_SendsEmailPushAndInAppNotification"` probehl uspesne 1/1 a screenshot `artifacts/e2e/screenshots/notifications/streak-warning-email-push-in-app/1366x900/light/all-channels.png` zobrazuje ceske texty v dropdownu.
- **Poznamky:** Oprava je nutna i kvuli pravidlu projektu “No hardcoded strings”.

### E2E-BUG-0152: Notification email se posila na UserId misto email adresy

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Notifications / Email
- **Nalezeno v testu:** pripravovany `Notifications_StreakWarningJob_SendsEmailPushAndInAppNotification`, regression `NotificationService_Send_EmailEnabled_SendsEmailToUserEmail`
- **Screenshot/trace:** n/a
- **Prostredi:** Unit test + pripravovany E2E se smtp4dev
- **Reprodukce:**
  1. Vytvorit notifikaci s `EmailEnabled=true`.
  2. Nechat `NotificationService.SendAsync` odeslat email.
  3. Zkontrolovat adresata predaneho do `IEmailService`.
- **Ocekavani:** Email notifikace se odesle na skutecny `User.Email`.
- **Skutecnost:** Sluzba predavala `request.UserId.ToString()`, tedy GUID misto email adresy.
- **Pravdepodobna pricina:** `SendNotificationRequest` nese jen `UserId` a `NotificationService` si nedohledavala uzivatele.
- **Oprava:** `NotificationService` si pres `IUserRepository.GetByIdAsync` dohleda uzivatele a email odesle jen na nenulovou `User.Email`.
- **Overeni:** `dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj --filter "FullyQualifiedName~NotificationService"` probehl uspesne 8/8. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Notifications_StreakWarningJob_SendsEmailPushAndInAppNotification"` probehl uspesne 1/1; smtp4dev zachytil email doruceny na testovaci adresu uzivatele.
- **Poznamky:** Bez opravy by smtp4dev E2E pro streak warning nezachytil email na adresu testovaciho uzivatele.

### E2E-BUG-0151: Zapnuti push notifikaci nevyzada browser permission ani neulozi subscription

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Notifications / Push / Settings / PWA
- **Nalezeno v testu:** `NotificationsE2ETests.Notifications_PushEnable_RequestsPermissionAndStoresSubscription`
- **Screenshot/trace:** `artifacts/e2e/failures/notifications/push-permission-enable/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. V testu nastavit `PushEnabled=false`.
  2. Otevrit `/settings` a zapnout toggle `Push notifikace`.
  3. Zkontrolovat, zda klient zavola browser permission/subscription JS API a ulozi subscription pres `/api/v1/notifications/push-subscription`.
- **Ocekavani:** Zapnuti push notifikaci vyvola permission/subscription tok a po uspesnem povoleni ulozi push subscription do DB.
- **Skutecnost:** Toggle meni pouze hodnotu v settings modelu; `window.lexiQuestPush.requestSubscription` se nezavola ani jednou a v `PushSubscriptions` nevznikne zaznam.
- **Pravdepodobna pricina:** `Settings.razor` ma push toggle napojeny jen na `@bind` a klientska JS vrstva pro Web Push subscription zatim neexistuje.
- **Oprava:** `Settings.razor` pri zapnuti push toggle vola `lexiQuestPush.requestSubscription`, uklada vracenou subscription pres `NotificationService.SavePushSubscriptionAsync` a pri chybe vraci toggle zpet. `App.razor` doplnuje browserovou Web Push JS vrstvu, service workery obsluhuji `push`/`notificationclick` udalosti a E2E instaluje deterministicky push stub az na nactene Settings strance.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Notifications_PushEnable_RequestsPermissionAndStoresSubscription"` probehl uspesne 1/1 a screenshot `artifacts/e2e/screenshots/notifications/push-permission-enable/1366x900/light/enabled.png` potvrzuje zapnuty push toggle. `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~SettingsPageTests"` probehl uspesne 10/10.
- **Poznamky:** Bez teto opravy by backendovy `WebPushService` nemel komu dorucovat push notifikace ani pri zapnute preferenci.

---

### E2E-BUG-0150: Settings nenacita ani neuklada skutecne notification preferences

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Notifications / Settings / Preferences
- **Nalezeno v testu:** `NotificationsE2ETests.Notifications_PreferencesLoadAndSave_WorkEndToEnd`
- **Screenshot/trace:** `artifacts/e2e/failures/notifications/preferences-load-save/20260620-054621.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Pres API nastavit `PUT /api/v1/notifications/preferences` s `PushEnabled=false`.
  2. Prihlasit uzivatele a otevrit `/settings`.
  3. Zkontrolovat notifikacni toggle v sekci predvoleb.
- **Ocekavani:** Settings zobrazi hodnoty z notifikacnich preferenci a pri ulozeni je zapise zpet na stejny endpoint.
- **Skutecnost:** Settings bere toggle hodnoty pouze z `UserProfileDto.Preferences`, takze push zustava zapnuty i pri `NotificationPreferences.PushEnabled=false`.
- **Pravdepodobna pricina:** Frontend ma `NotificationService.GetPreferencesAsync/UpdatePreferencesAsync`, ale `Settings.razor` ho nepouziva.
- **Oprava:** `Settings.razor` nacita skutecne notifikacni preference pres `NotificationService.GetPreferencesAsync`, aplikuje je do Settings UI a pri ulozeni vola vedle uzivatelskych preferenci i `NotificationService.UpdatePreferencesAsync`. bUnit regression testy overuji nacteni i presny ulozeny payload.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~SettingsPageTests"` probehl uspesne 9/9 a `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~NotificationsE2ETests"` probehl uspesne 2/2. Screenshoty `artifacts/e2e/screenshots/notifications/preferences-load-save/1366x900/light/loaded.png` a `saved.png` potvrzuji hodnoty `19:30` a `06:15` bez vizualnich prekryvu.
- **Poznamky:** Tahle chyba by pozdeji rozbila i scenare `Push disabled respektuje preference` a `Email disabled neposle email`.

### E2E-BUG-0149: Update notification preferences pro noveho uzivatele vraci 500

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Notifications / API / Preferences
- **Nalezeno v testu:** `NotificationsE2ETests.Notifications_PreferencesLoadAndSave_WorkEndToEnd`
- **Screenshot/trace:** `artifacts/e2e/logs/LexiQuest.Api-notifications-preferences-load-save-stdout.log`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Registrovat noveho uzivatele bez existujiciho radku v `NotificationPreferences`.
  2. Zavolat `PUT /api/v1/notifications/preferences`.
  3. API vrati 500.
- **Ocekavani:** Pro noveho uzivatele se preference vytvori a ulozi s HTTP 204.
- **Skutecnost:** EF vyhodi `DbUpdateConcurrencyException`, protoze novy preference objekt je po `AddAsync` jeste pred `SaveChanges` oznacen pres repository `Update` jako existujici.
- **Pravdepodobna pricina:** `NotificationService.UpdatePreferencesAsync` vola `_preferenceRepository.Update(preference)` i pro novou entitu.
- **Oprava:** `NotificationService.UpdatePreferencesAsync` rozlisuje novou a existujici entitu; pro novy radek vola jen `AddAsync`, nastavuje hodnoty a repository `Update` pouziva az u existujicich preferenci.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~NotificationsE2ETests"` probehl uspesne 2/2. Scenar `Notifications_PreferencesLoadAndSave_WorkEndToEnd` vytvari noveho uzivatele, vola `PUT /api/v1/notifications/preferences` bez 500 chyby a nasledne potvrzuje ulozene hodnoty pres API.
- **Poznamky:** Starsi API test chybu toleroval jako moznou; E2E ji povysuje na opravovany regres.

### E2E-BUG-0148: Notification dropdown se otevre orezany pres sidebar

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Notifications / UX / Visual
- **Nalezeno v testu:** `NotificationsE2ETests.Notifications_BellDropdownGroupingAndReadActions_WorkEndToEnd`
- **Screenshot/trace:** `artifacts/e2e/screenshots/notifications/bell-dropdown-read-actions/1366x900/light/grouped-unread.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Seedovat nekolik notifikaci s unread countem.
  2. Prihlasit uzivatele a otevrit `/dashboard`.
  3. Kliknout na notification bell.
- **Ocekavani:** Dropdown se otevre pod zvonkem, zustane cely viditelny a text notifikaci neni orezany okrajem viewportu/sidebaru.
- **Skutecnost:** Dropdown je kvuli `right: 0` relativne k uzkemu bell kontejneru vysunuty doleva, cast obsahu je mimo viewport a prekryva sidebar.
- **Pravdepodobna pricina:** Notification bell je v top baru blizko leve casti obrazovky, ale dropdown je zarovnan pravym okrajem k uzkemu kontejneru.
- **Oprava:** Notification bell je v topbaru, topbar akce jsou zarovnane doprava a `.notification-dropdown` je pozicovany `fixed` vuci viewportu (`top` pod topbarem, `right: 1rem`, sirka `min(22rem, calc(100vw - 2rem))`) s fallback barvami pro Tempo CSS promene, takze dropdown neni pruhledny ani orezany sidebar/viewportem.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~NotificationsE2ETests"` probehl uspesne 2/2 a screenshot `artifacts/e2e/screenshots/notifications/bell-dropdown-read-actions/1366x900/light/grouped-unread.png` zobrazuje cely citelny dropdown vpravo v topbaru.
- **Poznamky:** Funkcni asserty prosly; chyba byla nalezena az UX kontrolou screenshotu.

### E2E-BUG-0147: Auth service shazuje prerender pri primem vstupu na autentizovanou stranku

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Auth / Blazor prerender / Team
- **Nalezeno v testu:** `TeamE2ETests.Teams_OfficerRole_ShowsOfficerManagementOptions`, `TeamE2ETests.Teams_MemberRole_ShowsMemberManagementOptions`
- **Screenshot/trace:** `artifacts/e2e/failures/teams/role-based-management-options-officer/20260620-032636.png`, `artifacts/e2e/failures/teams/role-based-management-options-member/20260620-032712.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit uzivatele pres E2E token v `localStorage`.
  2. Otevrit primo `/team` pro non-leader clena tymu.
  3. Serverovy prerender se pokusi pres `AuthorizationMessageHandler` nacist token z `localStorage`.
- **Ocekavani:** Stranka se pri primem vstupu nesmi shodit; po interaktivnim nacteni se pouzije token z `localStorage` a zobrazi se tymovy dashboard.
- **Skutecnost:** UI zobrazilo cerveny Blazor error banner a server log hlasil `JavaScript interop calls cannot be issued at this time`, protoze `AuthService.GetTokenAsync()` volal JS interop behem statickeho renderu.
- **Pravdepodobna pricina:** Auth service nebyla odolna vuci prerender fazi Blazor Web App, kde neni dostupny browserovy `localStorage`.
- **Oprava:** `AuthService` pri prerender JS interop vyjimce vraci pro token `null` a neshazuje render; po prevzeti klientem zustava standardni nacitani z `localStorage`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~ManagementOptions"` probehl uspesne 3/3 a vytvoril ciste screenshoty `leader-actions.png`, `officer-actions.png`, `member-actions.png`.
- **Poznamky:** Chyba se projevila az pri realnem browser refresh/direct navigation toku, bUnit ji nezachytil.

---

### E2E-BUG-0146: Transfer E2E se po uspesnych screenshotech zasekl pri cleanupu videa

- **Stav:** Neni chyba
- **Severity:** P2
- **Oblast:** E2E Infra / Playwright / Video cleanup
- **Nalezeno v testu:** `TeamE2ETests.Teams_LeaderCanTransferLeadershipToMember`
- **Screenshot/trace:** `artifacts/e2e/screenshots/teams/transfer-leadership/1366x900/light/modal-open.png`, `artifacts/e2e/screenshots/teams/transfer-leadership/1366x900/light/after-transfer.png`, video `artifacts/e2e/videos/7dd7f6d0b44145e521a959570c27985b.webm`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Spustit transfer leadership E2E po doplneni modalu.
  2. Test vytvori screenshoty `modal-open` a `after-transfer`.
  3. `dotnet test` zustane viset po funkcni casti testu; video soubor uz neroste.
- **Ocekavani:** Po dokonceni scenare se Playwright context zavre a test runner vrati vysledek.
- **Skutecnost:** Proces zustal viset v cleanupu, pravdepodobne kolem `page.Context.CloseAsync()` a finalizace videa/ffmpeg.
- **Pravdepodobna pricina:** Playwright video finalizace se po tomto scenari nedokoncila, i kdyz screenshoty a aplikacni flow byly hotove.
- **Oprava:** Bez zmeny kodu; opakovane ciste behy transfer testu probehly korektne a cleanup se nezasekl.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_LeaderCanTransferLeadershipToMember"` probehl nasledne dvakrat uspesne: 1/1 a 1/1.
- **Poznamky:** Puvodni beh byl ukoncen rizenym Ctrl+C. Pokud se hang zopakuje v dalsich testech, znovu otevrit jako infra bug a doplnit timeout/odolne cleanup zachazeni s videem.

---

### E2E-BUG-0145: Transfer leadership flow je nedokonceny

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Teams / Transfer leadership / UI / API contract
- **Nalezeno v testu:** `TeamE2ETests.Teams_LeaderCanTransferLeadershipToMember`
- **Screenshot/trace:** `artifacts/e2e/failures/teams/transfer-leadership/20260619-223935.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Seedovat tym s leaderem, officerem a clenem.
  2. Prihlasit leadera a otevrit `/team`.
  3. Test hleda `data-testid="team-transfer-open"` a nasledne transfer modal.
- **Ocekavani:** Leader muze otevrit modal, vybrat jineho clena, potvrdit predani vedeni a UI/API role se aktualizuji.
- **Skutecnost:** Tlacitko nemelo stabilni selector, `ShowTransferModal()` nemel implementaci a klient posilal na endpoint objekt `{ UserId = ... }`, zatimco API ocekava primo `Guid`.
- **Pravdepodobna pricina:** Transfer leadership zustal jako placeholder v puvodni Team page.
- **Oprava:** Doplnit transfer open selector, modal s vyberem kandidata, stav vyberu/odesilani, lokalizovane texty a spravny Blazor HTTP payload jako prime `Guid`.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~TeamPageTests"` probehl uspesne: 5/5. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_LeaderCanTransferLeadershipToMember"` probehl uspesne: 1/1. Screenshoty potvrzuji modal, kandidaty, predani role a no-leader management stareho leadera.
- **Poznamky:** Screenshot potvrzuje, ze tlacitko bylo vizualne pritomne, ale tok nebyl automatizovatelny ani funkcne dokončený.

---

### E2E-BUG-0144: Solo leader vidi akci Predat vedeni bez ciloveho clena

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Teams / Transfer leadership / UX
- **Nalezeno v testu:** Screenshot review pri `TeamE2ETests.Teams_LastMemberDisbandsTeam`
- **Screenshot/trace:** `artifacts/e2e/screenshots/teams/last-member-disbands-team/1366x900/light/before-disband.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Vytvorit tym s jedinym clenem, ktery je leader.
  2. Prihlasit leadera a otevrit `/team`.
  3. Zkontrolovat management akce pod clenskou tabulkou.
- **Ocekavani:** Solo leader vidi disband akci, ale ne vidi `Predat vedeni`, protoze neexistuje zadny cilovy clen.
- **Skutecnost:** UI zobrazilo `Predat vedeni` i v solo tymu.
- **Pravdepodobna pricina:** Management akce kontrolovaly pouze `_isLeader`, ne dostupnost ciloveho clena.
- **Oprava:** Transfer leadership akce se zobrazi jen pres `CanTransferLeadership()`, tedy leaderovi s alespon jednim dalsim clenem.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_LastMemberDisbandsTeam"` probehl uspesne: 1/1. Screenshot `before-disband.png` potvrzuje solo leadera bez `Predat vedeni`.
- **Poznamky:** Plny transfer leadership tok se bude testovat v navazne polozce.

---

### E2E-BUG-0143: Disband team tlacitko nema stabilni E2E selector

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Teams / Disband / E2E stabilita
- **Nalezeno v testu:** `TeamE2ETests.Teams_LastMemberDisbandsTeam`
- **Screenshot/trace:** `artifacts/e2e/failures/teams/last-member-disbands-team/20260619-223439.png`, `artifacts/e2e/screenshots/teams/last-member-disbands-team/1366x900/light/before-disband.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Vytvorit solo tym.
  2. Prihlasit leadera a otevrit `/team`.
  3. Test klikne na `data-testid="team-disband"`.
- **Ocekavani:** Tlacitko `Rozpustit tym` ma stabilni selector pro E2E a screenshot audit.
- **Skutecnost:** Tlacitko bylo viditelne, ale bez `data-testid`; Playwright cekal do timeoutu.
- **Pravdepodobna pricina:** Leader management akce zatim nebyly selectorovane.
- **Oprava:** Doplnit `data-testid="team-disband"` na tlacitko `Rozpustit tym`.
- **Overeni:** `Teams_LastMemberDisbandsTeam` probehl uspesne: 1/1. Playwright klikl na selector, API po rozpušteni vraci 404 pro muj tym i detail smazaneho tymu.
- **Poznamky:** Funkcni endpoint existuje, ale UI akci neslo stabilne ovladat.

---

### E2E-BUG-0142: Leave team tlacitko nema stabilni E2E selector

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Teams / Leave team / E2E stabilita
- **Nalezeno v testu:** `TeamE2ETests.Teams_MemberCanLeaveTeam`
- **Screenshot/trace:** `artifacts/e2e/failures/teams/member-leave-team/20260619-223030.png`, `artifacts/e2e/screenshots/teams/member-leave-team/1366x900/light/before-leave.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Seedovat tym s leaderem a beznym clenem.
  2. Prihlasit bezneho clena a otevrit `/team`.
  3. Test klikne na `data-testid="team-leave"`.
- **Ocekavani:** Tlacitko `Opustit tym` ma stabilni selector a E2E muze overit odchod pres UI.
- **Skutecnost:** Tlacitko bylo viditelne, ale bez `data-testid`; Playwright cekal do timeoutu.
- **Pravdepodobna pricina:** Management akce byly v UI bez E2E kotvy.
- **Oprava:** Doplnit `data-testid="team-leave"` na tlacitko `Opustit tym`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_MemberCanLeaveTeam"` probehl uspesne: 1/1. Screenshoty potvrzuji dashboard pred odchodem a no-team stav po odchodu.
- **Poznamky:** Screenshot neukazuje vizualni problem, jen chybejici stabilni automatizacni selector.

---

### E2E-BUG-0141: Team dashboard po kicku neobnovi pocet clenu a stat karty

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Teams / Dashboard / State refresh / UX
- **Nalezeno v testu:** Screenshot review pri `TeamE2ETests.Teams_Officer_CanKickRegularMember`
- **Screenshot/trace:** `artifacts/e2e/screenshots/teams/officer-kick-member/1366x900/light/after-kick.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Seedovat tym s leaderem, officerem a beznym clenem.
  2. Officer vyhodi bezneho clena pres UI.
  3. Zkontrolovat dashboard po uspesnem vyhozeni.
- **Ocekavani:** Clenska tabulka, nadpis `Clenove (2/20)` a stat karty odpovidaji aktualnimu stavu tymu po vyhozeni.
- **Skutecnost:** Tabulka ukazovala dva hrace, ale nadpis zustal `Clenove (3/20)` a XP staty zustaly ve stavu pred vyhozenim.
- **Pravdepodobna pricina:** `KickMember` obnovil jen `_members`, ale neobnovil `_team` DTO se staty a `MemberCount`.
- **Oprava:** Po kicku i schvaleni join requestu se dashboard obnovuje pres `GetMyTeamAsync()` a znovu nacita clenskou tabulku, takze stat karty i `MemberCount` odpovidaji DB.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~TeamPageTests"` probehl uspesne: 5/5. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_Officer_CanKickRegularMember"` probehl uspesne: 1/1. Screenshot `after-kick.png` potvrzuje `Clenove (2/20)`, 200 weekly XP a 400 all-time XP.
- **Poznamky:** Stejne riziko existuje i po schvaleni join requestu, proto ma oprava pouzit spolecne obnoveni dashboardu.

---

### E2E-BUG-0140: Officerovi se zobrazuje kick akce u officera/sebe

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Teams / Role-based management / UX
- **Nalezeno v testu:** Screenshot review pri `TeamE2ETests.Teams_Officer_CanKickRegularMember`
- **Screenshot/trace:** `artifacts/e2e/screenshots/teams/officer-kick-member/1366x900/light/before-kick.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Seedovat tym s leaderem, officerem a beznym clenem.
  2. Prihlasit officera a otevrit `/team`.
  3. Zkontrolovat clenskou tabulku.
- **Ocekavani:** Officer vidi kick akci jen u beznych clenu; ne u leadera, officera ani u sebe.
- **Skutecnost:** UI zobrazilo `Vyhodit` i u radku officera, coz by vedlo k akci, kterou backend spravne odmita.
- **Pravdepodobna pricina:** Team page skryvala kick jen pro leader role a nerozlisovala pravomoci aktualniho uzivatele.
- **Oprava:** Doplnit `CanKickMember(...)`: leader muze vyhodit jen ne-leadery, officer jen bezne cleny a nikdo nemuze vyhodit sam sebe.
- **Overeni:** `TeamPageTests` 5/5 a `Teams_Officer_CanKickRegularMember` 1/1. Screenshot `before-kick.png` potvrzuje, ze officer vidi `Vyhodit` pouze u role `Clen`.
- **Poznamky:** Nalez vznikl ze screenshot auditu, i kdyz hlavni RED test spadl uz na chybejicim selectoru.

---

### E2E-BUG-0139: Kick tlacitko v clenske tabulce nema stabilni E2E selector

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Teams / E2E stabilita
- **Nalezeno v testu:** `TeamE2ETests.Teams_Officer_CanKickRegularMember`
- **Screenshot/trace:** `artifacts/e2e/failures/teams/officer-kick-member/20260619-221833.png`, `artifacts/e2e/screenshots/teams/officer-kick-member/1366x900/light/before-kick.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Seedovat tym s leaderem, officerem a beznym clenem.
  2. Prihlasit officera a otevrit `/team`.
  3. Test klikne na `data-testid="team-member-kick"` v radku bezneho clena.
- **Ocekavani:** Kick tlacitko ma stabilni selector v radku clena, aby E2E nemuselo klikat podle lokalizovaneho textu.
- **Skutecnost:** Tlacitko `Vyhodit` bylo viditelne, ale bez `data-testid`; Playwright cekal do timeoutu.
- **Pravdepodobna pricina:** Clenska tabulka zatim nebyla pripravena pro detailni E2E audit management akci.
- **Oprava:** Doplnit `data-testid="team-member-kick"` na povolene kick tlacitko a `data-testid="team-members-heading"` pro kontrolu aktualniho poctu clenu.
- **Overeni:** `TeamPageTests` 5/5 a `Teams_Officer_CanKickRegularMember` 1/1. Playwright klikl na selector v radku bezneho clena a overil zmenu v UI i API.
- **Poznamky:** Funkcni akce existuje, problem je stabilita automatizace a vizualni audit.

---

### E2E-BUG-0138: Schvaleni join requestu pada na EF concurrency pri pridani clena

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Teams / Join request / Backend
- **Nalezeno v testu:** `TeamE2ETests.Teams_Leader_CanApproveAndRejectJoinRequests`
- **Screenshot/trace:** API stdout pri testu, nasledne screenshoty `artifacts/e2e/screenshots/teams/approve-reject-join-requests/1366x900/light/pending-requests.png`, `artifacts/e2e/screenshots/teams/approve-reject-join-requests/1366x900/light/after-approve-reject.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Seedovat tym s leaderem.
  2. Dva hraci bez tymu poslou join request.
  3. Leader otevře `/team` a schvali jednu zadost.
- **Ocekavani:** Schvalena zadost zmeni stav na approved, uzivatel se vlozi jako clen tymu a UI ho ukaze v tabulce clenu; druha zadost jde odmitnout.
- **Skutecnost:** API pri schvaleni vratilo 500 kvuli `DbUpdateConcurrencyException`; v browseru se to projevilo jako CORS/fetch failure a zadost nesla schvalit.
- **Pravdepodobna pricina:** `TeamService.ApproveJoinRequestAsync` pridaval noveho clena pres `team.AddMember(...)` na existujicim trackovanem aggregate rootu, takze EF zkusil aktualizovat neexistujici `TeamMembers` radek misto vlozeni.
- **Oprava:** Doplnit `ITeamRepository.AddMemberAsync`, explicitne vkladat `TeamMember.Create(...)` v `ApproveJoinRequestAsync` i `AcceptInviteAsync` a sdilet validaci duplicity/maximalniho poctu clenu.
- **Overeni:** `dotnet build src/LexiQuest.Api/LexiQuest.Api.csproj` probehl uspesne. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_Leader_CanApproveAndRejectJoinRequests"` probehl uspesne: 1/1. Screenshoty potvrzuji dve cekajici zadosti, nasledne schvaleneho clena a odstraneni odmitnute zadosti.
- **Poznamky:** Stejna chyba by se mohla projevit i pri prijeti pozvanky, proto oprava pokryva oba vstupy do tymu.

---

### E2E-BUG-0137: Team ranking nema join request UI pro hrace bez tymu

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Teams / Join request / Ranking
- **Nalezeno v testu:** `TeamE2ETests.Teams_NoTeamUser_CanCreateJoinRequestFromRanking`
- **Screenshot/trace:** `artifacts/e2e/screenshots/teams/join-request-from-ranking/1366x900/light/join-request-success.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Vytvorit tym s leaderem a weekly XP.
  2. Prihlasit hrace bez tymu a otevrit `/team`.
  3. Kliknout na `Hledat tým`.
- **Ocekavani:** Ranking tymu ma stabilni selektory a hrac bez tymu muze poslat zadost o vstup se zpravou.
- **Skutecnost:** Ranking tabulka nemela stabilni E2E selector a UI neposkytovalo zadnou akci pro vytvoreni join requestu.
- **Pravdepodobna pricina:** Puvodni ranking byl jen pasivni prehled.
- **Oprava:** Ranking table dostala `data-testid`, radky dostaly selector, pro hrace bez tymu se zobrazuje `Požádat o vstup` a modal vola existujici `RequestJoinAsync`.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~TeamPageTests"` probehl uspesne: 4/4. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_NoTeamUser_CanCreateJoinRequestFromRanking"` probehl uspesne: 1/1. Screenshot potvrzuje citelny modal a success stav.

---

### E2E-BUG-0136: Team invite UI chybi a role management je nastaveny natvrdo na leadera

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Teams / Invite / Role-based management
- **Nalezeno v testu:** `TeamE2ETests.Teams_LeaderAndOfficer_CanInviteMemberByUsername`
- **Screenshot/trace:** `artifacts/e2e/screenshots/teams/invite-member-leader-officer/1366x900/light/leader-invite-success.png`, `artifacts/e2e/screenshots/teams/invite-member-leader-officer/1366x900/light/invitee-visible-invite.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Vytvorit tym s leaderem a officerem.
  2. Leader otevre `/team` a test hleda invite akci.
  3. Officer ma mit stejnou moznost pozvat hrace.
- **Ocekavani:** Leader i officer mohou v UI otevrit modal, zadat uzivatelske jmeno hrace a vytvorit pozvanku; pozvany hrac ji vidi ve svem no-team stavu.
- **Skutecnost:** UI nemelo invite modal ani stabilni invite selector; puvodni stranka navic nastavovala `_isLeader = true` pro kazdeho clena tymu.
- **Pravdepodobna pricina:** Team page mela jen kostru management akci a backend invite kontrakt pracoval s internim `UserId`, ne s uzivatelsky pouzitelnym username.
- **Oprava:** Doplnit `InviteMemberByUsernameRequest`, endpoint `POST /api/v1/teams/{id}/invite-by-username`, klientsky invite result, invite modal, selectorovanou sekci pozvanek a role vyhodnoceni podle `IUserService.GetProfileAsync()` a clenske role.
- **Overeni:** `dotnet build src/LexiQuest.Api/LexiQuest.Api.csproj` probehl uspesne. `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~TeamPageTests"` probehl uspesne: 4/4. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_LeaderAndOfficer_CanInviteMemberByUsername"` probehl uspesne: 1/1. Screenshoty potvrzuji citelny modal i pozvanku u pozvaneho hrace.

---

### E2E-BUG-0135: Team dashboard nema stabilni selektory pro popis a stat hodnoty

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Teams / Dashboard / E2E stabilita
- **Nalezeno v testu:** `TeamE2ETests.Teams_Dashboard_ShowsStatsDescriptionMembersAndRoles`
- **Screenshot/trace:** `artifacts/e2e/screenshots/teams/dashboard-stats-members/1366x900/light/dashboard.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Seedovat tym se tremi cleny, popisem a nenulovymi XP/vyhrami.
  2. Prihlasit leadera a otevrit `/team`.
  3. E2E test se pokusi overit popis a stat hodnoty pres stabilni `data-testid`.
- **Ocekavani:** Dashboard ma stabilni selektory pro popis, weekly XP, all-time XP, rank a wins.
- **Skutecnost:** Popis a stat hodnoty byly viditelne, ale bez stabilnich E2E selektoru.
- **Pravdepodobna pricina:** Puvoodni dashboard markup byl napsany pro rucni UI, ne pro detailni E2E/screenshot audit.
- **Oprava:** `Team.razor` dostal `data-testid` pro popis a vsechny ctyri stat hodnoty; bUnit test dashboardu tyto selektory hlida.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~TeamPageTests"` probehl uspesne: 4/4. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_Dashboard_ShowsStatsDescriptionMembersAndRoles"` probehl uspesne: 1/1. Screenshot potvrzuje citelny dashboard.

---

### E2E-BUG-0134: Duplicitni nazev/tag tymu ukazuje obecnou chybu misto konkretni

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Teams / Create team / Validace / UX
- **Nalezeno v testu:** `TeamE2ETests.Teams_CreateTeamDuplicateNameAndTag_ShowSpecificErrors`
- **Screenshot/trace:** `artifacts/e2e/screenshots/teams/create-duplicate-name-tag/1366x900/light/duplicate-tag-error.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Existujici uzivatel vytvori tym `Duplicitni tym` s tagem `DUPE`.
  2. Jiny premium uzivatel otevre `/team` a zkusi vytvorit tym se stejnym nazvem.
  3. Nasledne zkusi vytvorit tym s jinym nazvem, ale stejnym tagem `DUPE`.
- **Ocekavani:** Modal zobrazi konkretni ceskou chybu pro duplicitni nazev a konkretni ceskou chybu pro duplicitni tag.
- **Skutecnost:** Frontend zahodil text BadRequest odpovedi a zobrazil jen obecne `Tym se nepodarilo vytvorit. Zkontroluj udaje a zkus to znovu.`
- **Pravdepodobna pricina:** `LexiQuest.Blazor.Services.TeamService.CreateTeamAsync` vracel jen `TeamDto?`, takze `Team.razor` nemela informaci o duvodu selhani.
- **Oprava:** Blazor team service pridala strukturovany `CreateTeamClientResult` s chybami `DuplicateName`, `DuplicateTag`, `CannotCreate` a `AlreadyInTeam`; `Team.razor` mapuje stavy na lokalizovane texty z `Team.resx`.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~TeamPageTests"` probehl uspesne: 4/4. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_CreateTeamDuplicateNameAndTag_ShowSpecificErrors"` probehl uspesne: 1/1. Screenshot potvrzuje citelnou konkretni chybu pro duplicitni tag.

---

### E2E-BUG-0133: Team create modal ma rozbity spacing a nalepena tlacitka

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Teams / Create modal / UX
- **Nalezeno v testu:** `TeamE2ETests.Teams_FreeUserWithoutCoins_CreateTeamIsRejected`
- **Screenshot/trace:** `teams/free-create-insufficient-coins/modal-error.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Free uzivatel bez minci otevre create team modal.
  2. Vyplni validni udaje a submitne.
  3. Modal zobrazi error.
- **Ocekavani:** Error modal je citelny, ma normalni mezery mezi poli a tlacitka nejsou nalepena.
- **Skutecnost:** Form vypada stisnene, popisky/pole maji malo vertikalniho vzduchu a tlacitka `Zrušit`/`Vytvořit` jsou nalepena.
- **Pravdepodobna pricina:** CSS pro Team modal pouziva Tempo spacing promenne bez fallbacku; v E2E buildu nektere vyhodnoti jako neplatne hodnoty pro gap/padding/margin.
- **Oprava:** Team modal CSS dostal fallbacky pro spacing/padding/gap, minimalni sirky tlacitek, lepsi error blok a stabilni vysku textarea.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_FreeUserWithoutCoins_CreateTeamIsRejected"` probehl uspesne: 1/1. Aktualizovany screenshot `teams/free-create-insufficient-coins/modal-error.png` potvrzuje citelny modal bez nalepenych tlacitek. `TeamPageTests` prosly 4/4.

---

### E2E-BUG-0132: Free team creation neodecita 1000 minci

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Teams / Economy / Create team
- **Nalezeno v testu:** `TeamE2ETests.Teams_FreeUser_CreatesTeamForCoinsAndDeductsBalance`
- **Screenshot/trace:** Po vytvoreni tymu zustal `GET /api/v1/shop/coins` na balance `1000` misto `0`.
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit ne-premium uzivatele s 1000 mincemi.
  2. Vytvorit tym pres `/team`.
  3. Zkontrolovat coin balance pres shop API.
- **Ocekavani:** Ne-premium vytvoreni tymu stoji 1000 minci a zustatek je po uspesnem vytvoreni `0`.
- **Skutecnost:** Tým se vytvořil, ale mince zustaly na `1000`.
- **Pravdepodobna pricina:** `TeamService.CreateTeamAsync` kontroloval, jestli ma uzivatel dost minci, ale pri uspesnem vytvoreni je nestrhaval.
- **Oprava:** `TeamService.CreateTeamAsync` po validaci a kontrole duplicit u ne-premium uzivatele zapisuje coin transakci `TeamCreation` za `-1000` pred ulozenim tymu v jedne unit-of-work operaci; premium uzivatel zustava zdarma.
- **Overeni:** `dotnet build src/LexiQuest.Api/LexiQuest.Api.csproj` probehl uspesne. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_FreeUser_CreatesTeamForCoinsAndDeductsBalance"` probehl uspesne: 1/1. Screenshot `teams/free-create-coins/dashboard-after-coin-create.png` byl zkontrolovan.

---

### E2E-BUG-0131: Team create tlacitko neotevira modal

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Teams / Blazor / Create team
- **Nalezeno v testu:** `TeamE2ETests.Teams_PremiumUser_CreatesTeamForFree`
- **Screenshot/trace:** Playwright cekal na `data-testid="team-create-modal"` po kliknuti na `team-create` a timeoutoval.
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit premium uzivatele bez minci a bez tymu.
  2. Otevrit `/team`.
  3. Kliknout na `Vytvořit tým`.
- **Ocekavani:** Otevre se modal pro vytvoreni tymu s cenou `Zdarma pro Premium`.
- **Skutecnost:** `ShowCreateModal` byl prazdny handler a zadny modal neexistoval.
- **Pravdepodobna pricina:** Teams page mela pripravenou kostru dashboardu, ale create flow nebyl implementovany.
- **Oprava:** Team page pridala create modal, pole name/tag/description, cenu podle premium/coin stavu, klientskou validaci, submit pres `TeamService.CreateTeamAsync`, reload dashboardu a stabilni dashboard/member selectory.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~TeamPageTests"` probehl uspesne: 4/4. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_PremiumUser_CreatesTeamForFree"` probehl uspesne: 1/1. Screenshot `teams/premium-create-free/dashboard-after-create.png` byl zkontrolovan.

---

### E2E-BUG-0129: TeamsController nema registrovany ITeamService

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Teams / API / DI
- **Nalezeno v testu:** `TeamE2ETests.Teams_NoTeamState_ShowsEmptyActions`
- **Screenshot/trace:** `GET /api/v1/teams` vratil 500.
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Registrovat a prihlasit noveho uzivatele bez tymu.
  2. Zavolat `GET /api/v1/teams`.
- **Ocekavani:** Endpoint vrati 404, protoze uzivatel neni v tymu.
- **Skutecnost:** Endpoint pada na 500 s vyjimkou `Unable to resolve service for type 'LexiQuest.Core.Interfaces.Services.ITeamService'`.
- **Pravdepodobna pricina:** `Program.ConfigureServices` registruje `ITeamRepository`, ale neregistruje `ITeamService`.
- **Oprava:** `Program.ConfigureServices` registruje `ITeamService` na `TeamService`.
- **Overeni:** `dotnet build src/LexiQuest.Api/LexiQuest.Api.csproj` probehl uspesne. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Teams_NoTeamState_ShowsEmptyActions"` probehl uspesne: 1/1.

---

### E2E-BUG-0130: Team no-team UI nema E2E selektory a frontend vola stary my-team endpoint

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Teams / Blazor / E2E contract / API client
- **Nalezeno v testu:** `TeamE2ETests.Teams_NoTeamState_ShowsEmptyActions`
- **Screenshot/trace:** Playwright cekal na `data-testid="team-page"` a timeoutoval; failure screenshot pritom ukazal spravny no-team obsah.
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit noveho uzivatele bez tymu.
  2. Otevrit `/team`.
  3. Test hleda stabilni selektory `team-page`, `team-empty-state`, `team-create`, `team-search`.
- **Ocekavani:** Team stranka ma stabilni E2E selektory a klient nacita aktualni `GET /api/v1/teams`.
- **Skutecnost:** Stranka mela jen CSS classy a `TeamService.GetMyTeamAsync()` volal neexistujici `api/v1/users/me/team`.
- **Pravdepodobna pricina:** Teams UI bylo pripravene pred E2E contractem a frontend endpoint zustal ze starsiho navrhu rout.
- **Oprava:** Team page pridala `data-testid` pro page/loading/empty/dashboard a hlavni akce; `TeamService.GetMyTeamAsync()` vola `api/v1/teams`.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~TeamPageTests"` probehl uspesne: 3/3. `Teams_NoTeamState_ShowsEmptyActions` probehl uspesne: 1/1. Screenshot `teams/no-team-state/empty-state.png` byl zkontrolovan.

---

### E2E-BUG-0128: E2E neumi spustit RoomCleanupJob pro private rooms

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Multiplayer / Private Rooms / Cleanup / E2E infra
- **Nalezeno v testu:** `MultiplayerE2ETests.PrivateRoom_ExpiredRoomCleanup_RemovesOldCodeAndReleasesHost`
- **Screenshot/trace:** HTTP 404 na `POST /api/v1/e2e/multiplayer/cleanup-rooms`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Host vytvori private room.
  2. E2E endpoint room expiroval.
  3. Test vola cleanup endpoint.
- **Ocekavani:** E2E endpoint spusti `RoomCleanupJob`, aby slo deterministicky overit uvolneni room vazeb.
- **Skutecnost:** Endpoint vraci 404.
- **Pravdepodobna pricina:** E2E API obsahuje endpoint pro expiraci roomu, ale ne endpoint pro spusteni room cleanup jobu.
- **Oprava:** E2E API pridalo `POST /api/v1/e2e/multiplayer/cleanup-rooms`, ktery spousti `RoomCleanupJob`. Test zaroven prihlasuje hosta i guesta, aby se join overeni po cleanupu neredirectovalo na login.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_ExpiredRoomCleanup_RemovesOldCodeAndReleasesHost"` probehl uspesne: 1/1. Screenshot `private-room-expiry-cleanup/host-new-room-after-cleanup.png` potvrzuje novou mistnost po cleanupu a `private-room-expiry-cleanup/old-code-not-found-after-cleanup.png` potvrzuje chybu `Místnost nenalezena` pro stary kod.
- **Poznamky:** Test nasledne overi i to, ze host muze po cleanupu vytvorit novou mistnost a stary kod je nenalezen.

### E2E-BUG-0127: Private Room rematch request nema UI tok na result page

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Multiplayer / Private Rooms / Rematch / Result
- **Nalezeno v testu:** `MultiplayerE2ETests.PrivateRoom_RematchRequest_Accept_ReturnsBothPlayersToLobby`
- **Screenshot/trace:** Playwright assertion na `multiplayer-result-rematch-pending`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Host vyhraje Best of 1 private room.
  2. Oba hraci jsou na result page.
  3. Host klikne `Další zápas`.
  4. Test ceka na pending rematch alert a vyzvu u soupeře.
- **Ocekavani:** Host vidi `Čeká se na soupeře`, guest vidi rematch vyzvu s tlacitky `Přijmout` a `Odmítnout`; prijeti vrati oba do lobby.
- **Skutecnost:** Pending rematch alert neexistuje; dosavadni result page private room rematch nepodporuje jako UI tok.
- **Pravdepodobna pricina:** `MatchResultPage.StartNextMatch` pro private room jen naviguje na `/multiplayer`; `MatchResult` nema rematch pending/request/decline UI a klient nema accept/decline metody vystavene pro page.
- **Oprava:** `MatchResultPage` zustava pro private vysledek napojena na `MatchHubClient`, `MatchResult` zobrazuje pending/request/declined rematch stavy, klient a hub podporuji `AcceptRematch` i `DeclineRematch` a `RoomStateReset` vraci oba hrace do stejne lobby.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~MatchResultTests"` probehl uspesne: 15/15. `dotnet build src/LexiQuest.Api/LexiQuest.Api.csproj` a `dotnet build src/LexiQuest.Blazor.Client/LexiQuest.Blazor.Client.csproj` probehly uspesne. Kombinovany E2E filter `PrivateRoom_RematchRequest_Accept_ReturnsBothPlayersToLobby|PrivateRoom_RematchRequest_Decline_NotifiesRequester` probehl uspesne: 2/2.
- **Poznamky:** Screenshoty `private-room-rematch-accept/guest-rematch-request.png`, `private-room-rematch-accept/host-lobby-after-accept.png` a `private-room-rematch-decline/host-rematch-declined.png` byly zkontrolovane; po UX uprave se pri prijate vyzve nezobrazuje duplicitni spodní rematch CTA.

### E2E-BUG-0126: Best of 1 private result zobrazuje series score

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Multiplayer / Private Rooms / Result / Series
- **Nalezeno v testu:** `MultiplayerE2ETests.PrivateRoom_CompletedMatch_DoesNotAwardLeagueXp`
- **Screenshot/trace:** Playwright assertion na pocet `multiplayer-result-series-score`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Host vytvori Best of 1 private room.
  2. Host vyhraje zapas.
  3. Result page se otevre po persistenci match history.
  4. Test ocekava, ze series score pro Best of 1 neexistuje.
- **Ocekavani:** Best of 1 result nezobrazuje `Série`.
- **Skutecnost:** Result page obsahuje `multiplayer-result-series-score`.
- **Pravdepodobna pricina:** Persist match result uklada `SeriesPlayer1Wins`/`SeriesPlayer2Wins` pro vsechny private roomy vcetne `BestOf == 1`, a mapper z pritomnosti hodnot odvozuje series vysledek.
- **Oprava:** Persist match result uklada `SeriesPlayer1Wins`/`SeriesPlayer2Wins` jen pro private roomy s `BestOf > 1`.
- **Overeni:** `dotnet build src/LexiQuest.Api/LexiQuest.Api.csproj` probehl uspesne. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_CompletedMatch_DoesNotAwardLeagueXp"` probehl uspesne: 1/1. Screenshot `private-room-no-league-xp/host-result-no-league-xp.png` potvrzuje, ze Best of 1 nezobrazuje series score.
- **Poznamky:** Best of 3/5 series score musi zustat zachovany.

### E2E-BUG-0125: Private Room no-league-XP alert nema stabilni E2E selector

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Multiplayer / Private Rooms / Result / Testovatelnost
- **Nalezeno v testu:** `MultiplayerE2ETests.PrivateRoom_CompletedMatch_DoesNotAwardLeagueXp`
- **Screenshot/trace:** Playwright assertion na `multiplayer-result-no-league-info`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Host vyhraje Best of 1 private room.
  2. Result page zobrazi private-room vysledek.
  3. Test hleda informacni alert `multiplayer-result-no-league-info`.
- **Ocekavani:** Alert je dohledatelny stabilnim test id a obsahuje `Soukromé místnosti nepřidávají ligové XP`.
- **Skutecnost:** Alert text v UI existuje, ale chybi stabilni `data-testid`.
- **Pravdepodobna pricina:** `MatchResult` mel test id pro XP badge a league XP badge, ale ne pro private no-league-XP alert.
- **Oprava:** `MatchResult` pridal `data-testid="multiplayer-result-no-league-info"` na obal private no-league-XP alertu a bUnit test kontroluje cesky text.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~MatchResultTests"` probehl uspesne: 11/11. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_CompletedMatch_DoesNotAwardLeagueXp"` probehl uspesne: 1/1. Screenshoty `private-room-no-league-xp/host-result-no-league-xp.png` a `private-room-no-league-xp/host-league-still-zero.png` byly zkontrolovane.
- **Poznamky:** Test zaroven overuje, ze ligova stranka po private matchi zustava na `0 XP`.

### E2E-BUG-0124: Best of 5 ma spatny cesky tvar v nastaveni mistnosti

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Multiplayer / Private Rooms / Lokalizace / UX
- **Nalezeno v testu:** `MultiplayerE2ETests.PrivateRoom_BestOf5_CompletedMatch_ShowsSeriesScore`
- **Screenshot/trace:** Playwright assertion na `private-room-settings-best-of`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Host vytvori private room s nastavenim Best of 5.
  2. Guest se pripoji do lobby.
  3. Test kontroluje text nastaveni serie.
- **Ocekavani:** Lobby ukaze `Na 5 her`.
- **Skutecnost:** Lobby ukazuje `Na 5 hry`.
- **Pravdepodobna pricina:** Jeden resource `Room_Settings_BestOfSeries` se pouziva pro hodnoty 3 i 5, ale cestina potrebuje odlisny tvar.
- **Oprava:** Pridan resource `Room_Settings_BestOfSeriesMany` a `FormatBestOf` v create modalu i lobby vybira tvar pro 1/3/5.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~CreateRoomModalTests|FullyQualifiedName~RoomLobbyTests|FullyQualifiedName~MultiplayerLandingPageTests"` probehl uspesne: 21/21. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_BestOf5_CompletedMatch_ShowsSeriesScore"` probehl uspesne: 1/1. Kombinovany E2E filter pro Best of 3 a Best of 5 probehl uspesne: 2/2.
- **Poznamky:** Stejny formatter se pouziva v create modalu i v room lobby.

### E2E-BUG-0123: Private Room result nezobrazuje Best of series score

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Multiplayer / Private Rooms / Series / Result
- **Nalezeno v testu:** `MultiplayerE2ETests.PrivateRoom_BestOf3_CompletedMatch_ShowsSeriesScore`
- **Screenshot/trace:** `artifacts/e2e/failures/multiplayer/private-room-best-of3-series-score-host/...`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Host vytvori Best of 3 private room.
  2. Oba hraci vstoupi do realtime hry.
  3. Host vyresi vsech 10 slov a vyhraje prvni hru serie.
  4. Test ceka na `multiplayer-result-series-score`.
- **Ocekavani:** Result page ukaze `Série: 1:0`.
- **Skutecnost:** Result page se zobrazi, ale series score element neexistuje.
- **Pravdepodobna pricina:** `MatchResultDto` nenese series hodnoty z roomu a `MatchResultPage` je nepredava komponentě `MatchResult`; persist match result navic uklada `seriesPlayer1Wins`/`seriesPlayer2Wins` jako `null`.
- **Oprava:** `MatchResultDto` nese series score, private room completion aktualizuje room serii pred persistenci a result komponenta zobrazuje `multiplayer-result-series-score` pro private Best of vysledek.
- **Overeni:** `dotnet build src/LexiQuest.Api/LexiQuest.Api.csproj` probehl uspesne. `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~MatchResultTests"` probehl uspesne: 11/11. `PrivateRoom_BestOf3_CompletedMatch_ShowsSeriesScore` probehl uspesne: 1/1. `PrivateRoom_BestOf5_CompletedMatch_ShowsSeriesScore` probehl uspesne: 1/1. Kombinovany E2E filter pro Best of 3 a Best of 5 probehl uspesne: 2/2. Screenshoty `private-room-best-of3-series-score` a `private-room-best-of5-series-score` byly zkontrolovane.
- **Poznamky:** Screenshoty potvrzuji i info, ze private room nepridava ligove XP.

### E2E-BUG-0122: Private Room po countdownu nenaviguje do realtime hry

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Multiplayer / Private Rooms / Game start / SignalR
- **Nalezeno v testu:** `MultiplayerE2ETests.PrivateRoom_BestOf3_BothReady_NavigatesBothPlayersToRealtimeGame`
- **Screenshot/trace:** `artifacts/e2e/failures/multiplayer/private-room-best-of3-starts-realtime-game-host/...`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Host vytvori private room s nastavenim `Na 3 hry`.
  2. Guest se pripoji.
  3. Oba kliknou ready.
  4. Test ceka na URL `/multiplayer/game/{matchId}`.
- **Ocekavani:** Po countdownu oba hraci prejdou do realtime hry pro private room match.
- **Skutecnost:** Oba zustanou na `/multiplayer/room/{roomCode}`.
- **Pravdepodobna pricina:** `StartRoomCountdown` vola `_roomService.StartGameWithUserAsync(roomCode, Guid.Empty)`, nevytvari skutecny `MultiplayerGameService` match a klient nedostane match id event pro navigaci.
- **Oprava:** `MatchHub` pri create/join/get room status registruje private-room connectiony, po countdownu vytvori skutecny private `MultiplayerGameService` match se settings z roomu, ulozi ho k roomu, prida oba klienty do match group a posle jim `MatchFound` s `IsPrivateRoom=true`. `PrivateRoom.razor` na private `MatchFound` naviguje na `/multiplayer/game/{matchId}`.
- **Overeni:** `dotnet build src/LexiQuest.Api/LexiQuest.Api.csproj` a `dotnet build src/LexiQuest.Blazor.Client/LexiQuest.Blazor.Client.csproj` probehly uspesne. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_BestOf3_BothReady_NavigatesBothPlayersToRealtimeGame"` probehl uspesne: 1/1. Screenshot `artifacts/e2e/screenshots/multiplayer/private-room-best-of3-starts-realtime-game/1366x900/light/host-realtime-game.png` byl zkontrolovan.
- **Poznamky:** Tento start flow je nutny pred E2E overenim Best of 3/5 series score.

### E2E-BUG-0121: Private Room chat rate limit zavre lobby misto chat chyby

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Multiplayer / Private Rooms / Chat / Rate limit / UX
- **Nalezeno v testu:** `MultiplayerE2ETests.PrivateRoom_LobbyChat_RateLimit_ShowsLocalizedErrorAndKeepsLobby`
- **Screenshot/trace:** `artifacts/e2e/failures/multiplayer/private-room-lobby-chat-rate-limit-host/...`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Host a guest se pripoji do private room.
  2. Host rychle odesle 10 zpráv a pote jedenactou.
  3. Test ceka, ze lobby zustane viditelna a zobrazi se chatova rate-limit chyba.
- **Ocekavani:** Jedenacta zprava se neulozi, lobby zustane otevrena a chat ukaze ceskou rate-limit chybu.
- **Skutecnost:** `PrivateRoom` ulozi `ChatError` jako celostránkový `_errorMessage`, tím skryje lobby; puvodni server error je navic anglicky.
- **Pravdepodobna pricina:** Chat chyby nemaji oddeleny UI stav a odesilatel si zpravu lokálně pridava jeste pred serverovym potvrzenim.
- **Oprava:** `MatchHub.SendLobbyMessage` posila rate-limit jako technicky kod `Chat.RateLimit`, klient ho lokalizuje v `PrivateRoom` na cesky chat alert a `RoomLobby` ho zobrazuje pres `private-room-chat-error`. Uspesne chat zpravy se uz nepridavaji lokálně predem; server je broadcastuje cele room skupine, vcetne odesilatele.
- **Overeni:** `dotnet test tests/LexiQuest.Api.Tests/LexiQuest.Api.Tests.csproj --filter "FullyQualifiedName~MatchHubTests"` probehl uspesne: 3/3. `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~RoomLobbyTests|FullyQualifiedName~MultiplayerLandingPageTests"` probehl uspesne: 16/16. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_LobbyChat_RateLimit_ShowsLocalizedErrorAndKeepsLobby"` probehl uspesne: 1/1. Screenshot `artifacts/e2e/screenshots/multiplayer/private-room-lobby-chat-rate-limit/1366x900/light/rate-limit-error.png` byl zkontrolovan.
- **Poznamky:** Test zaroven hlida, ze jedenacta zprava po limitu nepribyde do chatu.

### E2E-BUG-0120: Dlouha chat zprava v Private Room se nezalamuje

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Multiplayer / Private Rooms / Chat / UX
- **Nalezeno v testu:** Screenshot review `MultiplayerE2ETests.PrivateRoom_LobbyChat_MaxTwoHundredCharacters_IsEnforced`
- **Screenshot/trace:** `artifacts/e2e/screenshots/multiplayer/private-room-lobby-chat-max-length/1366x900/light/message-truncated-to-max.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Host a guest se pripoji do private room.
  2. Host posle 200znakovou zpravu bez mezer.
  3. Zkontrolovat screenshot chatu.
- **Ocekavani:** Dlouha zprava se zalomi uvnitr chat panelu a nerozbije layout.
- **Skutecnost:** Souvisly text utece doprava mimo viditelnou oblast chat sekce.
- **Pravdepodobna pricina:** `.message-content` a `.message-text` nemaji `min-width: 0`, max sirku a `overflow-wrap`.
- **Oprava:** `RoomLobby` chat sekce dostala stabilni selector a CSS pro `.message-content`/`.message-text` nastavuje `min-width: 0`, `max-width: 100%`, `overflow-wrap: anywhere` a `word-break: break-word`. E2E test byl doplnen o bounding-box kontrolu, ze zprava nepretece chat panel.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~RoomLobbyTests"` probehl uspesne: 8/8. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_LobbyChat_MaxTwoHundredCharacters_IsEnforced"` probehl uspesne: 1/1. Screenshot `artifacts/e2e/screenshots/multiplayer/private-room-lobby-chat-max-length/1366x900/light/message-truncated-to-max.png` byl zkontrolovan.
- **Poznamky:** Test hlida klientsky `maxlength`, zobrazenou delku u obou hracu i vizualni nepretečení dlouhe zpravy.

### E2E-BUG-0119: Private Room lobby chat nema stabilni E2E selektory

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Multiplayer / Private Rooms / Chat / E2E contract
- **Nalezeno v testu:** `MultiplayerE2ETests.PrivateRoom_LobbyChat_SendsMessageToBothPlayers`
- **Screenshot/trace:** `artifacts/e2e/failures/multiplayer/private-room-lobby-chat-send-host/...`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Host a guest se pripoji do private room.
  2. Test zkusi vyplnit `private-room-chat-input`.
  3. Playwright ceka na chybejici selector.
- **Ocekavani:** Chat input, odesilaci tlacitko a zobrazene zpravy maji stabilni `data-testid`.
- **Skutecnost:** Chat markup pouziva jen CSS tridy, takze E2E test spadne na timeoutu.
- **Pravdepodobna pricina:** Chat UI vzniklo pred E2E kontraktem pro Private Room.
- **Oprava:** `RoomLobby` chat input, odesilaci tlacitko, message row a message text dostaly stabilni `data-testid`; bUnit test `RoomLobby_Chat_SendsAndReceivesMessages` kontroluje prijatou zpravu i chat selektory.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~RoomLobbyTests"` probehl uspesne: 8/8. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_LobbyChat_SendsMessageToBothPlayers"` probehl uspesne: 1/1. Screenshot `artifacts/e2e/screenshots/multiplayer/private-room-lobby-chat-send/1366x900/light/host-message-visible.png` byl zkontrolovan.
- **Poznamky:** Test overil lokalni zobrazeni u odesilatele i SignalR doruceni do druheho browseru.

### E2E-BUG-0118: Private Room lobby pouziva anglicke texty v ceskem UI

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Multiplayer / Private Rooms / Lokalizace / UX
- **Nalezeno v testu:** Screenshot review `MultiplayerE2ETests.PrivateRoom_BothReady_StartsCountdownForBothPlayers`
- **Screenshot/trace:** `artifacts/e2e/screenshots/multiplayer/private-room-both-ready-countdown/1366x900/light/countdown-started.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Host a guest se pripoji do private room.
  2. Oba kliknou ready.
  3. Zkontrolovat countdown screenshot.
- **Ocekavani:** Private Room lobby pouziva ceske popisky pro hlavicku i nastaveni serie.
- **Skutecnost:** Screenshot zobrazuje `Lobby`, `BEST OF` a `Best of 3`.
- **Pravdepodobna pricina:** Hodnoty v `Resources/Pages/Multiplayer.resx` zustaly z puvodni anglicke terminologie.
- **Oprava:** `Multiplayer.resx` pouziva ceske texty `Čekárna`, `Série`, `Na {0} hry` a opravenou gramatiku `Oba hráči připraveni!`; core i Blazor validace neplatne serie pouziva cesky text `Série musí být na 1, 3 nebo 5 her`.
- **Overeni:** `dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj --filter "FullyQualifiedName~RoomServiceTests|FullyQualifiedName~RoomSettingsValidatorTests"` probehl uspesne: 49/49. `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~CreateRoomModalTests|FullyQualifiedName~RoomLobbyTests|FullyQualifiedName~MultiplayerLandingPageTests"` probehl uspesne: 21/21. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_CreateModal_SelectsSettingsShowsRoomCodeAndCopiesIt|FullyQualifiedName~PrivateRoom_InvalidSettings_AreRejectedByHub|FullyQualifiedName~PrivateRoom_BothReady_StartsCountdownForBothPlayers"` probehl uspesne: 3/3. Screenshoty `artifacts/e2e/screenshots/multiplayer/private-room-both-ready-countdown/1366x900/light/countdown-started.png` a `artifacts/e2e/screenshots/multiplayer/private-room-create-settings-code-copy/1366x900/light/host-lobby-code-settings.png` byly zkontrolovany.
- **Poznamky:** Zbyvajici vyskyty `Best of` jsou technicke nazvy vlastnosti, test comments nebo doménové komentare, ne user-facing UI text.

### E2E-BUG-0117: Private Room countdown nema E2E selector

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Multiplayer / Private Rooms / Countdown / E2E contract
- **Nalezeno v testu:** `MultiplayerE2ETests.PrivateRoom_BothReady_StartsCountdownForBothPlayers`
- **Screenshot/trace:** `artifacts/e2e/failures/multiplayer/private-room-both-ready-countdown-host/...`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Host a guest se pripoji do private room.
  2. Oba kliknou ready.
  3. Test ceka na `private-room-countdown`.
- **Ocekavani:** Countdown cislo ma stabilni selector.
- **Skutecnost:** Countdown markup ma jen CSS tridu `.countdown`.
- **Pravdepodobna pricina:** Countdown flow nebyl jeste soucasti E2E kontraktu.
- **Oprava:** Countdown cislo v `RoomLobby` dostalo `data-testid="private-room-countdown"`.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~RoomLobbyTests"` probehl uspesne: 8/8. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_BothReady_StartsCountdownForBothPlayers"` probehl uspesne: 1/1. Navazny E2E subset s lokalizaci probehl uspesne: 3/3. Screenshot `artifacts/e2e/screenshots/multiplayer/private-room-both-ready-countdown/1366x900/light/countdown-started.png` byl zkontrolovan.
- **Poznamky:** Test overil synchronizovany countdown v host i guest browseru.

### E2E-BUG-0116: Private Room ready/cancel ready nema E2E selektory

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Multiplayer / Private Rooms / Ready / E2E contract
- **Nalezeno v testu:** `MultiplayerE2ETests.PrivateRoom_ReadyToggle_SetsAndCancelsReadyState`
- **Screenshot/trace:** `artifacts/e2e/failures/multiplayer/private-room-ready-toggle-host/...`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Host a guest jsou v private room lobby.
  2. Test ceka na `private-room-ready`.
- **Ocekavani:** Ready a cancel-ready akce maji stabilni E2E selektory.
- **Skutecnost:** Ready tlacitko selector nema, test ceka do timeoutu.
- **Pravdepodobna pricina:** Ready flow nebyl jeste soucasti E2E kontraktu.
- **Oprava:** `RoomLobby` ready a cancel-ready tlacitka dostala `data-testid="private-room-ready"` a `data-testid="private-room-cancel-ready"`.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~RoomLobbyTests"` probehl uspesne: 8/8. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_ReadyToggle_SetsAndCancelsReadyState"` probehl uspesne: 1/1. Screenshot `artifacts/e2e/screenshots/multiplayer/private-room-ready-toggle/1366x900/light/host-ready-cancelled.png` byl zkontrolovan.
- **Poznamky:** Test overil i SignalR ready state sync do druheho browseru.

### E2E-BUG-0115: Private Room leave button nema E2E selector a guest nema jasny cancelled stav

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Multiplayer / Private Rooms / Leave / UX
- **Nalezeno v testu:** `MultiplayerE2ETests.PrivateRoom_HostLeave_CancelsRoomAndNotifiesGuest`
- **Screenshot/trace:** `artifacts/e2e/failures/multiplayer/private-room-host-leave-cancels-room-host/...`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Host vytvori private room a guest se pripoji.
  2. Test ceka na `private-room-leave`.
- **Ocekavani:** Leave akce je stabilne testovatelna a po odchodu hosta guest vidi srozumitelny cancelled stav.
- **Skutecnost:** Leave tlacitko nema `data-testid`; dosavadni refresh room statusu navic nema explicitni UX stav pro null room po zruseni hostem.
- **Pravdepodobna pricina:** Lobby leave flow zatim nebyl napojen na E2E/screenshot kontrakt.
- **Oprava:** `RoomLobby` leave tlacitko dostalo `data-testid="private-room-leave"` a `PrivateRoom.razor` pri null room statusu po refreshi zobrazi cesky `Room_Cancelled` stav misto prazdne lobby.
- **Overeni:** `dotnet build src/LexiQuest.Blazor.Client/LexiQuest.Blazor.Client.csproj` probehl uspesne. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_HostLeave_CancelsRoomAndNotifiesGuest"` probehl uspesne: 1/1. Screenshot `artifacts/e2e/screenshots/multiplayer/private-room-host-leave-cancels-room/1366x900/light/guest-room-cancelled.png` byl zkontrolovan.
- **Poznamky:** Stejny selector bude pouzit i pro guest leave scenar.

### E2E-BUG-0114: Private Room expired-code scenar nema deterministicky E2E endpoint

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Multiplayer / Private Rooms / E2E infra / Expiry
- **Nalezeno v testu:** `MultiplayerE2ETests.PrivateRoom_JoinExpiredCode_ShowsExpiredError`
- **Screenshot/trace:** E2E helper `ExpirePrivateRoomAsync` dostal 404 na `/api/v1/e2e/multiplayer/expire-room`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Host vytvori private room.
  2. Test zavola E2E endpoint pro vynucenou expiraci mistnosti.
- **Ocekavani:** E2E umi deterministicky expirovat room bez cekani 5 minut.
- **Skutecnost:** Endpoint neexistuje a test konci HTTP 404.
- **Pravdepodobna pricina:** Faze E2E zatim mela runtime helper pro Quick Match timer, ale ne pro in-memory private room expiry.
- **Oprava:** Doplněn E2E endpoint `/api/v1/e2e/multiplayer/expire-room`, test fixture helper `ExpirePrivateRoomAsync` a `Room.Expire()` nyní posune `ExpiresAt` do minulosti, aby dalsi join deterministicky spadl do expired vetve.
- **Overeni:** `dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj --filter "FullyQualifiedName~RoomEntityTests|FullyQualifiedName~RoomServiceTests"` probehl uspesne: 50/50. `dotnet build src/LexiQuest.Api/LexiQuest.Api.csproj` probehl uspesne. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_JoinExpiredCode_ShowsExpiredError"` probehl uspesne: 1/1. Screenshot `artifacts/e2e/screenshots/multiplayer/private-room-join-expired-code/1366x900/light/expired-error.png` byl zkontrolovan.
- **Poznamky:** `Room.Expire()` musi zaroven nastavit stav tak, aby dalsi join skoncil expired chybou.

### E2E-BUG-0113: Private Room join not-found chyba se zobrazuje anglicky

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Multiplayer / Private Rooms / Join / Lokalizace
- **Nalezeno v testu:** `MultiplayerE2ETests.PrivateRoom_JoinInvalidCode_ShowsValidationAndNotFoundErrors`
- **Screenshot/trace:** E2E assertion videla `Room not found` v `private-room-error`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Otevrit join modal.
  2. Zadavat validne vypadajici, ale neexistujici kod `LEXIQ-ZZZZ`.
  3. Odeslat join.
- **Ocekavani:** UI zobrazi ceskou chybu `Místnost nenalezena`.
- **Skutecnost:** UI zobrazi anglicky text `Room not found`.
- **Pravdepodobna pricina:** `RoomService` vraci technicke anglicke stringy a `MatchHub.JoinRoom` je posila klientovi bez mapovani na lokalizovany/user-facing text.
- **Oprava:** `MatchHub` mapuje technicke room chyby z core sluzby na ceske user-facing texty (`Room not found` -> `Místnost nenalezena`, expired/full/active-room varianty).
- **Overeni:** `dotnet build src/LexiQuest.Api/LexiQuest.Api.csproj` probehl uspesne. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_JoinInvalidCode_ShowsValidationAndNotFoundErrors"` probehl uspesne: 1/1. Screenshot `artifacts/e2e/screenshots/multiplayer/private-room-join-invalid-code/1366x900/light/not-found-error.png` byl zkontrolovan.
- **Poznamky:** Stejne mapovani bude potreba pro expired/full/active-room scenare.

### E2E-BUG-0112: Private Room join invalid code nema stabilni validacni selector

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Multiplayer / Private Rooms / Join / Validation / E2E contract
- **Nalezeno v testu:** `MultiplayerE2ETests.PrivateRoom_JoinInvalidCode_ShowsValidationAndNotFoundErrors`
- **Screenshot/trace:** `artifacts/e2e/failures/multiplayer/private-room-join-invalid-code/...`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Otevrit join modal.
  2. Zadavat malformed kod `abc`.
  3. Kliknout na submit a cekat na `private-room-join-validation`.
- **Ocekavani:** Validacni hlaska ma stabilni selector a obsahuje format `LEXIQ-XXXX`.
- **Skutecnost:** Validacni text se neda E2E stabilne najit, protoze nema `data-testid`.
- **Pravdepodobna pricina:** Join modal pouziva jen CSS tridu `.validation-message`.
- **Oprava:** `JoinRoomModal` validacni hlaska dostala `data-testid="private-room-join-validation"`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_JoinInvalidCode_ShowsValidationAndNotFoundErrors"` probehl uspesne: 1/1.
- **Poznamky:** Navazna cast stejneho testu pokryje validne vypadajici, ale neexistujici kod.

### E2E-BUG-0111: Private Room join flow nema stabilni E2E selektory

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Multiplayer / Private Rooms / Join / E2E contract
- **Nalezeno v testu:** `MultiplayerE2ETests.PrivateRoom_JoinValidRoom_ShowsBothPlayersInLobby`
- **Screenshot/trace:** `artifacts/e2e/failures/multiplayer/private-room-join-valid-guest/...`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Host vytvori private room.
  2. Guest otevře `/multiplayer`, klikne na `Připojit se`.
  3. Playwright ceka na `private-room-join-modal`.
- **Ocekavani:** Join modal i lobby hraci maji stabilni `data-testid`, aby slo flow testovat bez vazby na CSS tridy.
- **Skutecnost:** `private-room-join-modal` neni v DOM, protoze `JoinRoomModal` nema E2E selektory.
- **Pravdepodobna pricina:** Join modal byl vytvoren pred fazi E2E kontraktu a pouziva jen prezencni CSS tridy.
- **Oprava:** `JoinRoomModal` dostal stabilni test id pro modal, input a submit; `RoomLobby` dostal test id pro player card a player name. Test byl upraven tak, aby kontroloval seznam hracu bez Playwright strict-mode kolize.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~JoinRoomModalTests|FullyQualifiedName~RoomLobbyTests"` probehl uspesne: 21/21. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_JoinValidRoom_ShowsBothPlayersInLobby"` probehl uspesne: 1/1. Screenshoty `host-sees-guest.png` a `guest-joined-lobby.png` byly zkontrolovany.
- **Poznamky:** Synchronizace host/guest lobby je overena stejnym testem.

### E2E-BUG-0110: Private Room hub prijme neplatne settings misto validacni chyby

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Multiplayer / Private Rooms / Validation / SignalR
- **Nalezeno v testu:** `MultiplayerE2ETests.PrivateRoom_InvalidSettings_AreRejectedByHub`
- **Screenshot/trace:** SignalR E2E timeout na `RoomCreationFailed`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, SignalR klient
- **Reprodukce:**
  1. Prihlasit uzivatele a otevrit SignalR spojeni na `/hubs/match`.
  2. Zavolat `CreateRoom` s `WordCount=12`, `TimeLimitMinutes=4`, `BestOf=2`.
  3. Cekat na `RoomCreationFailed`.
- **Ocekavani:** Hub posle validacni chybu a mistnost nevznikne.
- **Skutecnost:** `RoomCreationFailed` neprijde; server payload neodmitne v room service vrstve.
- **Pravdepodobna pricina:** `RoomSettingsValidator` existuje, ale `RoomService.CreateRoomAsync` ho nepouziva.
- **Oprava:** `RoomService.CreateRoomAsync` zapojuje existujici `RoomSettingsValidator` pred vytvorenim entity a vraci agregovanou ceskou validacni chybu. Validator byl upraven na ceske user-facing hlasky.
- **Overeni:** `dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj --filter "FullyQualifiedName~RoomServiceTests|FullyQualifiedName~RoomSettingsValidatorTests"` probehl uspesne: 49/49. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_InvalidSettings_AreRejectedByHub"` probehl uspesne: 1/1.
- **Poznamky:** UI neplatne hodnoty nenabizi, ale SignalR/API contract musi zustat chraneny i proti obejiti klienta.

### E2E-BUG-0109: Private Room lobby screenshot pusobi rozbite a nedostatecne vyvazene

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Multiplayer / Private Rooms / UX / Screenshot
- **Nalezeno v testu:** Screenshot review `MultiplayerE2ETests.PrivateRoom_CreateModal_SelectsSettingsShowsRoomCodeAndCopiesIt`
- **Screenshot/trace:** `artifacts/e2e/screenshots/multiplayer/private-room-create-settings-code-copy/1366x900/light/host-lobby-code-settings.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Vytvorit soukromou mistnost s nastavenim 20 slov, 5 minut, Best of 3.
  2. Otevrit screenshot lobby hosta.
- **Ocekavani:** Lobby jasne priorizuje kod mistnosti, copy akci, hrace a nastaveni; vizualni prvky maji vyvazene mezery a nepusobi jako nahodne technicke debug prvky.
- **Skutecnost:** Copy tlacitko je natlacene na kod mistnosti, expiracni progress bar pusobi jako tlusta modra cara pres obsah a placeholder pro soupere je prilis dominantni proti zbytku lobby.
- **Pravdepodobna pricina:** `RoomLobby` vznikla jako funkcni komponenta bez screenshot UX ladeni; progress bar a player grid nemaji dostatecne layout constraints pro realny app shell.
- **Oprava:** `RoomLobby` pouziva subtilni expiracni meter misto dominantniho progress baru, kompaktnejsi blok kodu, svetlejsi waiting placeholder a zretelne outline tlacitko `Zrusit`.
- **Overeni:** `dotnet build src/LexiQuest.Blazor.Client/LexiQuest.Blazor.Client.csproj` probehl uspesne. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_CreateModal_SelectsSettingsShowsRoomCodeAndCopiesIt"` probehl uspesne: 1/1. Screenshot `artifacts/e2e/screenshots/multiplayer/private-room-create-settings-code-copy/1366x900/light/host-lobby-code-settings.png` byl znovu zkontrolovan a je vhodny jako baseline.
- **Poznamky:** Funkcni E2E asserty i screenshot review prosly.

### E2E-BUG-0108: Private Room create flow nema stabilni modal ani routovane lobby

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Multiplayer / Private Rooms / Blazor / SignalR
- **Nalezeno v testu:** `MultiplayerE2ETests.PrivateRoom_CreateModal_SelectsSettingsShowsRoomCodeAndCopiesIt`
- **Screenshot/trace:** `artifacts/e2e/failures/multiplayer/private-room-create-settings-code-copy/...`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit hosta a otevrit `/multiplayer`.
  2. Kliknout na `Vytvorit mistnost`.
  3. Cekat na E2E selector create modalu a pote na lobby po vytvoreni mistnosti.
- **Ocekavani:** Create modal ma stabilni E2E selektory pro word count, cas, obtiznost, best of a submit; po vytvoreni se otevře `/multiplayer/room/LEXIQ-XXXX`, lobby zobrazi kod, nastaveni a tlacitko kopirovani.
- **Skutecnost:** Playwright nenajde `private-room-create-modal`; create flow navic po odeslani naviguje na obecne `/multiplayer/room`, pro ktere neni routovana lobby stranka.
- **Pravdepodobna pricina:** Private Room komponenty byly připravené jako UI fragmenty, ale nemaji E2E contract, klient neceka na `RoomCreated` SignalR event a neexistuje stranka, ktera by z kodu mistnosti nacetla aktualni `RoomStatusDto`.
- **Oprava:** `CreateRoomModal` dostal stabilni E2E selektory a lokalizovane obtiznosti, `MatchHub` umi vratit `RoomStatusDto` pres `GetRoomStatus`, SignalR klient posloucha room success/fail eventy a vznikla routovana stranka `/multiplayer/room/{RoomCode}` s lobby a copy code akcí.
- **Overeni:** `dotnet build src/LexiQuest.Api/LexiQuest.Api.csproj` a `dotnet build src/LexiQuest.Blazor.Client/LexiQuest.Blazor.Client.csproj` probehly uspesne. `dotnet test tests/LexiQuest.Api.Tests/LexiQuest.Api.Tests.csproj --filter "FullyQualifiedName~MatchHubTests"` probehl uspesne: 3/3. `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~CreateRoomModalTests|FullyQualifiedName~RoomLobbyTests|FullyQualifiedName~MultiplayerLandingPageTests"` probehl uspesne: 21/21. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PrivateRoom_CreateModal_SelectsSettingsShowsRoomCodeAndCopiesIt"` probehl uspesne: 1/1.
- **Poznamky:** Screenshot lokalizaci i lobby rozlozeni overil navazny nalez `E2E-BUG-0109`.

### E2E-BUG-0107: Realtime game header lepí progress text na tlačítko `Vzdát`

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Multiplayer / Realtime game / UX
- **Nalezeno v testu:** Screenshot review `MultiplayerE2ETests.QuickMatch_ReconnectWithinGrace_RestoresMatchAndPreventsForfeit`
- **Screenshot/trace:** `artifacts/e2e/screenshots/multiplayer/quickmatch-reconnect-within-grace/1366x900/light/alice-reconnected-game.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Otevrit realtime Quick Match.
  2. Zkontrolovat horní header s timerem, progress textem a tlačítkem `Vzdát`.
- **Ocekavani:** Text progressu a akční tlačítko mají jasný rozestup a nepůsobí jako jeden slepený řetězec.
- **Skutecnost:** Screenshot ukazuje `Slovo 1/15Vzdát`, protože progress text a tlačítko nemají dostatečnou mezeru.
- **Pravdepodobna pricina:** `.game-header` nemá horizontální gap a `.word-progress`/`.forfeit-button` nemají stabilní flex constraints.
- **Oprava:** Do `.game-header` byl doplněn stabilní `gap`, progress text má `flex-shrink: 0` a `white-space: nowrap`, tlačítko `Vzdát` má vlastní minimální šířku a padding.
- **Overeni:** `dotnet build src/LexiQuest.Blazor.Client/LexiQuest.Blazor.Client.csproj` probehl uspesne. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~QuickMatch_ReconnectWithinGrace_RestoresMatchAndPreventsForfeit"` probehl uspesne: 1/1. Screenshot `artifacts/e2e/screenshots/multiplayer/quickmatch-reconnect-within-grace/1366x900/light/alice-reconnected-game.png` byl znovu zkontrolovan a header uz neni slepeny.
- **Poznamky:** Funkce reconnectu je zelená a opravený screenshot je vhodný pro baseline.

### E2E-BUG-0106: Disconnect grace neukonci Quick Match po 30 sekundach jako forfeit

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Multiplayer / Quick Match / Disconnect / SignalR
- **Nalezeno v testu:** `MultiplayerE2ETests.QuickMatch_DisconnectGrace_ForfeitsAfterThirtySecondsAndAwardsOpponent`
- **Screenshot/trace:** `artifacts/e2e/failures/multiplayer/quickmatch-disconnect-grace-bob/...`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Spustit Quick Match se dvema browser hraci.
  2. Po startu realtime hry zavrit browser context Alice.
  3. Na Bobovi overit cekaci stav a pockat pres 30 sekund.
- **Ocekavani:** Soupeř nevyhraje okamzite, ale po 30s grace se odpojeny hrac forfaitne, Bob prejde na result page, vidi vyhru, +100 XP a +50 league XP.
- **Skutecnost:** Bob vidi cekaci stav, okamzity result neprijde, ale ani po 30s se zapas neukonci; result page se nezobrazi.
- **Pravdepodobna pricina:** `MatchHub.OnDisconnectedAsync` vola `HandleDisconnectAsync`, ale nikde neplanuje `FinalizeDisconnectAsync` po grace period.
- **Oprava:** `MatchHub` po disconnectu planuje 30s finalizer, reconnect nebo ukonceni matche ho zrusi a background finalizer bezi v novem DI scope pres `IServiceScopeFactory`, aby nepouzival scoped sluzby z puvodni hub instance. Po grace vola `FinalizeDisconnectAsync`, ulozi result, pripise XP/league XP a posle `MatchEnded` pres `IHubContext`.
- **Overeni:** `dotnet build src/LexiQuest.Api/LexiQuest.Api.csproj` probehl uspesne. `dotnet test tests/LexiQuest.Api.Tests/LexiQuest.Api.Tests.csproj --filter "FullyQualifiedName~MatchHubTests"` probehl uspesne: 3/3. `dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj --filter "FullyQualifiedName~MultiplayerGameServiceEdgeCaseTests|FullyQualifiedName~MultiplayerGameServiceTests"` probehl uspesne: 39/39. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~QuickMatch_DisconnectGrace_ForfeitsAfterThirtySecondsAndAwardsOpponent"` probehl uspesne: 1/1. Screenshot `artifacts/e2e/screenshots/multiplayer/quickmatch-disconnect-grace/1366x900/light/bob-victory-after-disconnect.png` byl zkontrolovan.
- **Poznamky:** Tento nalez pokryva checklist bod `Disconnect grace 30s`.

### E2E-BUG-0105: Quick Match timer expiry neni E2E ovladatelny a match se po vyprseni nedokonci

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Multiplayer / Quick Match / Timer / SignalR / E2E infra
- **Nalezeno v testu:** `MultiplayerE2ETests.QuickMatch_TimerExpiry_CompletesMatchAsDrawAndShowsResult`
- **Screenshot/trace:** RED zatim pada pred browser flow na chybějicim E2E endpointu.
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. V E2E testu nastavit kratky Quick Match limit pres `/api/v1/e2e/multiplayer/quick-match-time-limit`.
  2. Spustit Quick Match se dvema browser hraci.
  3. Cekat na vyprseni timeru a result page.
- **Ocekavani:** E2E umi deterministicky zkratit Quick Match cas, realtime UI zobrazi zkraceny timer a po vyprseni oba hraci prejdou na vysledek s remizou, XP a league XP.
- **Skutecnost:** Endpoint pro nastaveni Quick Match timeru vraci 404; produkcni tok navic pouziva jen lokalni klientsky timer bez serveroveho dokončení matche po nule.
- **Pravdepodobna pricina:** Quick Match time limit je zadratovany na 3 minuty a `RealtimeGame.razor` při vyprseni lokálního timeru nevolá hub. `MultiplayerGameService` sice umi oznacit expired match jako neaktivni, ale hub nema expiry command/event, ktery by ulozil result a informoval klienty.
- **Oprava:** Doplněn E2E endpoint `/api/v1/e2e/multiplayer/quick-match-time-limit`, runtime nastavení Quick Match limitu, `RoomSettingsDto.TimeLimitSeconds`, serverový hub command `ExpireMatch` a klientské volání při doběhnutí realtime timeru na nulu. `MultiplayerGameService.StartMatchAsync` resetuje expiraci až na skutečný start po countdownu a `RoundStarted` posílá zbývající čas.
- **Overeni:** `dotnet build src/LexiQuest.Api/LexiQuest.Api.csproj` a `dotnet build src/LexiQuest.Blazor.Client/LexiQuest.Blazor.Client.csproj` probehly uspesne. `dotnet test tests/LexiQuest.Api.Tests/LexiQuest.Api.Tests.csproj --filter "FullyQualifiedName~MatchHubTests"` probehl uspesne: 3/3. `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~RealtimeGamePageTests"` probehl uspesne: 3/3. `dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj --filter "FullyQualifiedName~MultiplayerGameServiceTests|FullyQualifiedName~MultiplayerGameServiceEdgeCaseTests"` probehl uspesne: 39/39. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~QuickMatch_TimerExpiry_CompletesMatchAsDrawAndShowsResult"` probehl uspesne: 1/1. Screenshot `artifacts/e2e/screenshots/multiplayer/quickmatch-timer-expiry/1366x900/light/alice-expired-draw-result.png` byl zkontrolovan.
- **Poznamky:** Tento nalez pokryva checklist bod `Timer expiruje match`.

### E2E-BUG-0104: Draw result modal zobrazuje matoucí text `Rychlejší vyhrává`

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Multiplayer / Result / UX / Lokalizace
- **Nalezeno v testu:** `MultiplayerE2ETests.QuickMatch_Draw_WhenCorrectCountAndTimeAreEqual_ShowsDrawResult`
- **Screenshot/trace:** `artifacts/e2e/failures/multiplayer/quickmatch-draw-result-alice-result/...`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Odehrat Quick Match tak, aby oba hraci meli stejny pocet spravnych odpovedi i stejny celkovy cas.
  2. Otevrit `/multiplayer/result/{matchId}`.
  3. Zkontrolovat text pod titulkem remizy.
- **Ocekavani:** Draw modal vysvetli remizu bez tvrzeni, ze rychlejsi hrac vyhrava.
- **Skutecnost:** Modal zobrazi `REMÍZA` a zaroven `Rychlejší vyhrává!`, coz je u uplne remizy protichudne.
- **Pravdepodobna pricina:** `MatchResult.razor` pouziva resource `Result_Speed_Tiebreaker` pro vsechny draw stavy misto samostatne draw zpravy.
- **Oprava:** `MatchResult.razor` pro remízu používá samostatnou zprávu `Result_Draw_Message`, komponenta dostala `.draw-text` styl a bUnit test hlídá, že se text `Rychlejší vyhrává` v draw stavu nevrací.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~MatchResultTests"` probehl uspesne: 11/11. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~QuickMatch_Draw_WhenCorrectCountAndTimeAreEqual_ShowsDrawResult"` probehl uspesne: 1/1. Screenshot `artifacts/e2e/screenshots/multiplayer/quickmatch-draw-result/1366x900/light/alice-draw-result.png` byl zkontrolovan a zobrazuje konzistentni draw text bez kolize.
- **Poznamky:** Core pravidlo draw funguje; chyba je v copy/UX result komponenty.

### E2E-BUG-0103: Quick Match timeout po 30 s neposila klientovi timeout stav

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Multiplayer / Quick Match / Timeout / SignalR
- **Nalezeno v testu:** `MultiplayerE2ETests.QuickMatch_SinglePlayer_TimesOutAfterThirtySecondsAndShowsOptions`
- **Screenshot/trace:** `artifacts/e2e/failures/multiplayer/quickmatch-single-player-timeout/...`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit jednoho hrace.
  2. Otevrit `/multiplayer/quick-match` bez dalsiho soupere.
  3. Cekat pres 30 sekund na timeout stav.
- **Ocekavani:** Po timeoutu se fronta ukonci a klient vidi stav `Soupeř nenalezen` s akcemi zkusit znovu, hra proti AI a zpet.
- **Skutecnost:** UI zustava ve stavu hledani; hub neposle browser klientovi `MatchmakingTimeout`.
- **Pravdepodobna pricina:** `MatchmakingService` ma timeout timer/event, ale `MatchHub` neni stabilne napojen na singleton service event a browser flow nema per-user timeout notifikaci.
- **Oprava:** `MatchHub` po uspesnem zarazeni bez okamziteho matche planuje per-user timeout pres `IHubContext`, timeout zrusi pri matchi, cancelu nebo disconnectu a po 30 s posle klientovi `MatchmakingTimeout`. `TimeoutView` dostal stabilni `data-testid` selektory a zpravu `Matchmaking_Timeout_Message` z resource souboru.
- **Overeni:** `dotnet test tests/LexiQuest.Api.Tests/LexiQuest.Api.Tests.csproj --filter "FullyQualifiedName~MatchHubTests"` probehl uspesne: 3/3. `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~MatchmakingPageTests"` probehl uspesne: 3/3. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~QuickMatch_SinglePlayer_TimesOutAfterThirtySecondsAndShowsOptions"` probehl uspesne. Screenshot `artifacts/e2e/screenshots/multiplayer/quickmatch-single-player-timeout/1366x900/light/timeout.png` byl zkontrolovan.
- **Poznamky:** Timeout trva realnych 30 sekund; test ma proto delsi runtime.

### E2E-BUG-0102: Globalni app sidebar obsahuje anglicke a neprirozene navigacni nazvy

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Layout / Lokalizace / UX
- **Nalezeno v testu:** Screenshot review `MultiplayerE2ETests.QuickMatch_Forfeit_ShowsPerspectiveResultsAwardsXpAndSavesHistory`
- **Screenshot/trace:** `artifacts/e2e/screenshots/multiplayer/quickmatch-forfeit-result-history/1366x900/light/bob-history-after-forfeit.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit uzivatele a otevrit libovolnou app stranku s hlavnim layoutem.
  2. Zkontrolovat levou navigaci na screenshotu.
- **Ocekavani:** Navigace pouziva ceske nazvy.
- **Skutecnost:** Sidebar ukazuje `Dashboard` a `Achievementy`.
- **Pravdepodobna pricina:** Resource hodnoty v `Resources/Layout/MainLayout.resx` a starsim `Resources/Shared/Navigation.resx` zustaly z casti v anglictine/loanwordech.
- **Oprava:** Resource hodnoty `Nav.Dashboard` a `Nav.Achievements` v `Resources/Layout/MainLayout.resx` a kompatibilni `Resources/Shared/Navigation.resx` byly prelozeny na `Přehled` a `Úspěchy`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~QuickMatch_Forfeit_ShowsPerspectiveResultsAwardsXpAndSavesHistory"` probehl uspesne po lokalizaci. Screenshot `artifacts/e2e/screenshots/multiplayer/quickmatch-forfeit-result-history/1366x900/light/bob-history-after-forfeit.png` ukazuje ceske polozky sidebaru.
- **Poznamky:** Oprava je resource-only a nemeni routy.

### E2E-BUG-0101: Historie multiplayeru zobrazuje anglicke texty ve filtrech a statistikach

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Multiplayer / Lokalizace / UX
- **Nalezeno v testu:** Screenshot review `MultiplayerE2ETests.QuickMatch_Forfeit_ShowsPerspectiveResultsAwardsXpAndSavesHistory`
- **Screenshot/trace:** `artifacts/e2e/screenshots/multiplayer/quickmatch-forfeit-result-history/1366x900/light/bob-history-after-forfeit.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Po Quick Match forfeit otevrit `/multiplayer/history`.
  2. Zkontrolovat screenshot historie se statistikami, filtry a typem zapasu.
- **Ocekavani:** Vsechny viditelne texty v historii multiplayeru jsou cesky.
- **Skutecnost:** Screenshot ukazuje `WIN RATE`, `Quick Match`, `Private Room` a `Quick`.
- **Pravdepodobna pricina:** Resource hodnoty v `Resources/Pages/Multiplayer.resx` zustaly z casti v anglictine.
- **Oprava:** Resource hodnoty historie multiplayeru byly lokalizovany na `Rychlý zápas`, `Soukromá místnost`, `Úspěšnost`, `Rychlý` a `Soukromý`. Aktualizovany byly i bUnit mocky.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~MatchHistoryPageTests|FullyQualifiedName~MatchResultTests"` probehl uspesne: 19/19. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~QuickMatch_Forfeit_ShowsPerspectiveResultsAwardsXpAndSavesHistory"` probehl uspesne a screenshot historie je cesky.
- **Poznamky:** Screenshot potvrzuje funkcni stav i lokalizaci history obsahu.

### E2E-BUG-0100: Quick Match po forfeit nema vysledek, XP ani ulozenou historii

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Multiplayer / Result / XP / Match history
- **Nalezeno v testu:** `MultiplayerE2ETests.QuickMatch_Forfeit_ShowsPerspectiveResultsAwardsXpAndSavesHistory`
- **Screenshot/trace:** `artifacts/e2e/failures/multiplayer/quickmatch-forfeit-result-history/...`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Registrovat a prihlasit dva hrace v samostatnych browser contexteh.
  2. Spustit Quick Match, pockat na realtime hru a u Alice kliknout na `data-testid="realtime-forfeit"`.
  3. Cekat na `/multiplayer/result/{matchId}`, vysledek pro Boba, XP/liga XP a zaznam v `/multiplayer/history`.
- **Ocekavani:** Forfeit ukonci zapas, soupeř vidi vyhru, vzdavajici hrac prohru, Quick Match prida osobni XP i league XP a historie/statistiky zobrazi odehrany zapas.
- **Skutecnost:** Realtime hra naviguje na `/multiplayer/result/{matchId}`, ale takova Blazor route neexistuje. Hub posila stejny `MatchResultDto` cele groupě z perspektivy player1, vysledek se neuklada do `MatchResults` a XP/league XP se nepersistuji.
- **Pravdepodobna pricina:** Multiplayer end-match tok konci pouze in-memory DTO z `MultiplayerGameService.EndMatchAsync`; `MatchHistoryService`, `IXpService`, `ILeagueService` ani result page nejsou napojene na SignalR ukonceni zapasu.
- **Oprava:** Doplněn endpoint `GET /api/v1/multiplayer/matches/{matchId}/result`, per-player mapping výsledku v `MatchHistoryService`, routovaná Blazor stránka `/multiplayer/result/{MatchId}`, stabilní result/history selektory a uložení match resultu z `MatchHub`. Hub po ukončení zápasu ukládá historii, přidává osobní XP i league XP jen při prvním uložení a posílá každému hráči výsledek z jeho perspektivy.
- **Overeni:** `dotnet build src/LexiQuest.Api/LexiQuest.Api.csproj` probehl uspesne. `dotnet test tests/LexiQuest.Api.Tests/LexiQuest.Api.Tests.csproj --filter "FullyQualifiedName~MatchHubTests|FullyQualifiedName~MatchHistoryEndpointsTests"` probehl uspesne: 11/11. `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~MatchHistoryPageTests|FullyQualifiedName~MatchResultTests"` probehl uspesne: 19/19. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~QuickMatch_Forfeit_ShowsPerspectiveResultsAwardsXpAndSavesHistory"` probehl uspesne. Screenshoty `bob-victory-result.png` a `bob-history-after-forfeit.png` byly zkontrolovany.
- **Poznamky:** Souvisi s checklist body `Forfeit da vyhru souperi`, `Result modal victory/defeat/draw`, `Quick Match dava osobni XP i league XP` a `Match history a stats`.

### E2E-BUG-0099: Realtime hra po Quick Match zobrazuje default `TEST` misto serveroveho kola

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Multiplayer / Realtime game / SignalR
- **Nalezeno v testu:** `MultiplayerE2ETests.QuickMatch_RealtimeCorrectAnswer_UpdatesOwnScoreAndOpponentProgress`
- **Screenshot/trace:** `artifacts/e2e/failures/multiplayer/quickmatch-realtime-score-progress-alice/...`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Spustit dva browser contexty, oba prihlasit a otevrit `/multiplayer/quick-match`.
  2. Pockat na prechod do `/multiplayer/game/{matchId}`.
  3. Precist `data-testid="realtime-scrambled-word"` a pokusit se mapovat scrambling na seedovane slovo.
- **Ocekavani:** Realtime hra po countdownu zobrazi serverem poslane aktualni kolo z `MultiplayerGameService`.
- **Skutecnost:** UI zustalo na defaultnim placeholderu `TEST`, ktery neni v E2E seedu; po odstraneni placeholderu se ukazalo `Ztraceno připojení`, protoze hub klient byl pri navigaci z matchmaking stranky zdisposovany.
- **Pravdepodobna pricina:** `RoundStarted` event muze odejit jeste pred tim, nez `RealtimeGame` po navigaci nasubscribuje handler. Nova realtime stranka se explicitne nepripojovala k existujicimu `match:{id}` groupu ani si nevyzadala aktualni kolo. Navic `QuickMatch.DisposeAsync()` a `RealtimeGame.DisposeAsync()` ručně disposovaly DI-owned `MatchHubClient`.
- **Oprava:** Doplněn hub contract `JoinMatch(Guid matchId)`, realtime stránka se po startu připojí k aktivnímu match groupu a server pošle aktuální kolo. `MatchHubClient` dostal `JoinMatchAsync` a `OnPlayerProgress`; hub po odpovědi posílá vlastní progress callerovi, stejný progress soupeři jako opponent update a následné kolo celé groupě. Odstraněn default `TEST` placeholder a komponenty už ručně nedisposují injektovaný hub klient. E2E helper pro scrambling lookup normalizuje velikost písmen.
- **Overeni:** `dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj --filter "FullyQualifiedName~MultiplayerGameService"` probehl uspesne: 39/39. `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~MatchmakingPageTests|FullyQualifiedName~RealtimeGamePageTests"` probehl uspesne: 6/6. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~QuickMatch_RealtimeCorrectAnswer_UpdatesOwnScoreAndOpponentProgress"` probehl uspesne: 1/1. Screenshoty `quickmatch-realtime-score-progress` pro Alici i Boba zkontrolovany.
- **Poznamky:** Tento fix pokryva vlastni score update i opponent progress. Match end, forfeit, timer expiry a historie zustavaji samostatne otevrene body.

### E2E-BUG-0098: Match-found countdown zobrazuje literalni parametry a nema aplikacni layout

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Multiplayer / Quick Match / UX
- **Nalezeno v testu:** `MultiplayerE2ETests.QuickMatch_TwoBrowserPlayers_CountdownAndNavigateToRealtimeGame`
- **Screenshot/trace:** `artifacts/e2e/screenshots/multiplayer/quickmatch-countdown-realtime/1366x900/light/match-found-countdown.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Registrovat a prihlasit dva E2E uzivatele na samostatnych browser context strankach.
  2. Otevrit `/multiplayer/quick-match` u obou hracu.
  3. Pockat na `data-testid="quick-match-found"` a ulozit screenshot countdown stavu.
- **Ocekavani:** Countdown obrazovka ukaze skutecne jmeno vlastniho hrace a soupere, citelne player karty, zretelny countdown 3-2-1 a zadne literalni backing-field texty.
- **Skutecnost:** Screenshot ukazuje `_username` a `_opponentUsername` misto hodnot, vlastni level pada na fallback `Level 1` a match-found child komponenta nema aplikacni CSS layout.
- **Pravdepodobna pricina:** String parametry `MatchFoundView` jsou v `QuickMatch.razor` predane bez `@`, takze Razor pouzije literalni hodnoty. Styly pro `MatchFoundView` zustaly v parent `QuickMatch.razor.css`, kde je kvuli CSS isolation child komponenta nedostane.
- **Oprava:** Opraveny string parametry v `QuickMatch.razor` na Razor bindingy s `@`, doplneno nacteni profilu pres `IUserService`, pridany resource `Matchmaking_Player_Level` a izolovane styly `MatchFoundView.razor.css`. Countdown timer startuje po 1 s, aby stav 3 zustal viditelny.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~MatchmakingPageTests|FullyQualifiedName~RealtimeGamePageTests"` probehl uspesne: 6/6. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~QuickMatch_TwoBrowserPlayers_CountdownAndNavigateToRealtimeGame"` probehl uspesne: 1/1. Opraveny screenshot `artifacts/e2e/screenshots/multiplayer/quickmatch-countdown-realtime/1366x900/light/match-found-countdown.png` zkontrolovan.
- **Poznamky:** Realtime screenshot je funkcne pouzitelny; horní header je hustsi kolem progress/tlacitka `Vzdát`, ale bez prekryvu nebo blokace scenare.

### E2E-BUG-0097: Quick Match nema E2E SignalR contract a multiplayer landing neni stabilne testovatelny

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Multiplayer / SignalR / Blazor UX
- **Nalezeno v testu:** `MultiplayerE2ETests.Multiplayer_LandingQuickMatch_SearchCancelAndScreenshot`, `MultiplayerE2ETests.QuickMatch_SignalR_JwtDuplicateCancelAndTwoPlayerMatch`, `MultiplayerE2ETests.QuickMatch_SignalR_PrefersSimilarLevelAndLeavesFarOpponentQueued`
- **Screenshot/trace:** Puvodni failure `artifacts/e2e/failures/multiplayer/landing-quickmatch-search-cancel/20260619-163536.png`, `artifacts/e2e/failures/multiplayer/landing-quickmatch-search-cancel/20260619-163536-console.log`; overovaci screenshoty `artifacts/e2e/screenshots/multiplayer/landing-quickmatch-search-cancel/1366x900/light/landing.png` a `artifacts/e2e/screenshots/multiplayer/landing-quickmatch-search-cancel/1366x900/light/searching.png`.
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, SignalR client proti `/hubs/match`
- **Reprodukce:**
  1. Registrovat a prihlasit E2E uzivatele.
  2. Otevrit `/multiplayer` a cekat na `data-testid="multiplayer-page"`.
  3. Pres SignalR client s JWT zavolat `JoinMatchmaking` dvakrat a potom `CancelMatchmaking`.
  4. Dva prihlaseni uzivatele pripojit na hub a oba zaradit do Quick Match fronty.
  5. Zaradit hrace level 5, potom level 20 a potom level 7; level 5 se ma sparovat s levelem 7 a level 20 zustat ve fronte.
- **Ocekavani:** Multiplayer landing ma stabilni selektory a citelne karty Quick Match / Private Room. Hub prijme JWT, join vrati `true`, duplicitni join vrati `false`, cancel vraci stav a dva hraci dostanou `MatchFound` se stejnym `MatchId`.
- **Skutecnost:** Landing nemel root/card/action `data-testid` selektory a screenshot ukazal nedoladeny layout. `JoinMatchmaking`/`CancelMatchmaking` nevracely vysledek, takze SignalR client nedokazal overit duplicate/cancel contract (`Error trying to deserialize result to Boolean`). Browser WebSocket navic vracel 401, protoze API nebralo SignalR JWT z `access_token` query parametru. Doplneny edge-case odhalil, ze background timer dokazal potichu sparovat level 5 s levelem 20 a odstranit je z fronty bez klientského `MatchFound`.
- **Pravdepodobna pricina:** Faze 5 multiplayer byl napsany pro komponentove testy a demo UI, ale nema Phase 9 E2E contract. Hub metody vracely pouze `Task`, match found flow spolehal na globalni eventy z hub instanci a produkcni JWT konfigurace neobsahovala SignalR query-token handler.
- **Oprava:** Doplněny stabilni multiplayer/quick-match `data-testid` selektory a komponentove CSS pro searching stav. `JoinMatchmaking` a `CancelMatchmaking` vraci `bool`, matchmaking service vraci okamzity `MatchmakingJoinResult`, hub posila obema hracum stejny `MatchId` pres jejich connection ID a API JWT konfigurace cte `access_token` pro `/hubs`. Background matching uz nesparuje hrace mimo level toleranci; vzdaleny soupeř zustava ve fronte do timeoutu.
- **Overeni:** `dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj --filter "FullyQualifiedName~MatchmakingServiceTests"` probehl uspesne: 10/10. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~MultiplayerE2ETests"` probehl uspesne: 3/3.
- **Poznamky:** Zbyvajici Quick Match body jako timeout UI, countdown, realtime hra, vysledek, XP/league XP a historie zustavaji otevrene ve Fazi 9.

### E2E-BUG-0096: Vlastni slovniky nemaji v1 API contract, premium gate ani E2E ovladatelne UI

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Dictionaries / Premium / Game
- **Nalezeno v testu:** `DictionariesE2ETests.Dictionaries_FreeUser_ShowsPremiumGateAndApiRejectsCreate`, `DictionariesE2ETests.Dictionaries_PremiumUser_CreatesAddsImportsPublicAndDeletes`, `DictionariesE2ETests.Dictionaries_ApiValidationOwnerImportAndCustomGame_WorkEndToEnd`, `DictionariesE2ETests.Dictionaries_ApiMaxTenDictionaries_IsEnforced`
- **Screenshot/trace:** Puvodni failure `artifacts/e2e/failures/dictionaries/premium-crud-import-public-delete/...`; overovaci screenshoty `artifacts/e2e/screenshots/dictionaries/free-user-premium-gate/1366x900/light/gate.png` a `artifacts/e2e/screenshots/dictionaries/premium-crud-import-public-delete/1366x900/light/public-visible.png`.
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Registrovat free nebo premium E2E uzivatele.
  2. Volat `POST /api/v1/dictionaries` nebo otevrit `/dictionaries`.
  3. Zkusit vytvořit slovník, přidat slovo, importovat, zobrazit veřejné slovníky a spustit hru s vlastním slovníkem.
- **Ocekavani:** API je dostupné pod `/api/v1/dictionaries`, free user dostane premium gate/403, premium user může vytvořit max 10 slovníků, nastavit public/private, přidat validní slova, odmítnout invalidní/duplicitní slova, importovat CSV/TXT/JSON a spustit hru nad vlastním slovníkem.
- **Skutecnost:** `api/v1/dictionaries` vrací 404, stránka `/dictionaries` nemá stabilní selektory ani premium gate, UI neposílá public toggle a chybí E2E pokrytí detailu/importů/owner pravidel/custom game.
- **Pravdepodobna pricina:** Slovníky byly implementované přes starší `/api/dictionaries` controller a základní stránku bez Phase 9 E2E contractu; service vrstva nehlídá premium, limit 10, duplicity ani max délku 20 podle UC.
- **Oprava:** Controller podporuje `/api/v1/dictionaries` i kompatibilni `/api/dictionaries`, service vrstva vynucuje premium ucet, limit 10 slovniku, limit 100 slov, duplicity a validaci slov 3-20 znaku. UI ma premium gate, stabilni `data-testid` selektory, create/add/import modaly, public/private toggle a klient vola v1 API. Importy podporuji CSV/TXT/JSON, owner pravidla vraci 403 a `StartGameRequest` umi `CustomDictionaryId` pro prvni custom round.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~DictionariesE2ETests"` probehl uspesne: 4/4. Souvisejici `dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj --filter "FullyQualifiedName~DictionaryWordTests|FullyQualifiedName~DictionaryServiceTests|FullyQualifiedName~DictionaryServiceEdgeCaseTests"` probehl 55/55 a `dotnet test tests/LexiQuest.Api.Tests/LexiQuest.Api.Tests.csproj --filter "FullyQualifiedName~Dictionary"` probehl 17/17. Screenshoty premium gate a public slovniku byly zkontrolovany bez prekryvu textu a s odpovidajicim stavem.
- **Poznamky:** Povinna screenshot coverage matice pro slovniky zustava samostatny ukol, protoze jeste nema schvalene baseline pro vsechny dilci stavy.

### E2E-BUG-0095: Herni flow nepripisuji coin odmeny za level, boss, daily ani achievement

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Shop / Coins / Game rewards
- **Nalezeno v testu:** `CoinEarningE2ETests.Coins_PathLevelComplete_EarnsTenCoinsOnce`, `CoinEarningE2ETests.Coins_BossVictory_EarnsFiftyCoins`, `CoinEarningE2ETests.Coins_DailyChallengeComplete_EarnsTwentyCoins`, `CoinEarningE2ETests.Coins_FirstWordAchievement_EarnsFiftyCoinsOnce`
- **Screenshot/trace:** API-only E2E scenare; vystup testu ukazuje balance `0` misto `10`, `20` nebo `50`.
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Registrovat E2E uzivatele s nulovym coin balance.
  2. Dokoncit path level, boss level, daily challenge nebo odemknout achievement `first_word`.
  3. Nacist `/api/v1/shop/coins`.
- **Ocekavani:** Level completion pripise 10 minci, boss 50 minci, daily challenge 20 minci a common achievement 50 minci; opakovany replay/duplicitni attempt nesmi pripsat odmenu podruhe.
- **Skutecnost:** Vsechny flow vraci po dokonceni balance `0`, protoze odmeny nejsou napojene na business flow.
- **Pravdepodobna pricina:** `CoinService` ma pravidla odmen, ale `GameSessionService`, `BossGameService`, `DailyChallengeService` a `AchievementService` ho nebo ekvivalentni doménovou transakci nevolaji pri dokončení.
- **Oprava:** Path level completion pridava idempotentni `LevelComplete` coin transakci jen pri prvnim dokonceni levelu; boss completion pridava `BossLevel` odmenu; daily challenge po prvni spravne completion vola `ICoinService` pro 20 minci; achievement unlock vola `ICoinService` s rarity mapovanou podle XP rewardu.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~CoinEarningE2ETests"` probehl uspesne: 4/4. Pred opravou stejny test reprodukoval balance `0` misto `10`, `20` a `50`.
- **Poznamky:** U path levelu se odmena ma pripsat jen za prvni dokonceni levelu, replay uz ne.

### E2E-BUG-0094: Shop nema E2E seed, klientskou sluzbu ani stabilni selektory

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Shop / Blazor / E2E data
- **Nalezeno v testu:** `Shop_Page_ShowsBalanceCategoriesItemsAndRarity`, `Shop_ConcurrentPurchase_OnlyOneSpendSucceedsAndBalanceNeverNegative`, `Shop_PurchaseOwnedEquipInsufficientAndDuplicate_WorkEndToEnd`, `Shop_PremiumOnlyItem_FreeUserShowsGateAndApiRejectsPurchase`
- **Screenshot/trace:** `artifacts/e2e/failures/shop/overview-balance-categories-rarity/20260619-153125.png`, `artifacts/e2e/failures/shop/overview-balance-categories-rarity/20260619-153125-console.log`; overovaci screenshoty `artifacts/e2e/screenshots/shop/overview-balance-categories-rarity/1366x900/light/loaded.png`, `artifacts/e2e/screenshots/shop/purchase-owned-equip-insufficient-duplicate/1366x900/light/after-equip.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Registrovat a prihlasit E2E uzivatele.
  2. Otevrit `/shop`.
  3. Cekat na root selector `shop-page` nebo hledat deterministicke shop itemy pres API.
- **Ocekavani:** Shop stranka se vykresli, zobrazi zustatek minci, ctyri kategorie, deterministicke itemy, rarity badge a premium gate.
- **Skutecnost:** Blazor pada na neregistrovane `IShopService`; API shop items je prazdne, protoze E2E seed shop itemy nevytvari; UI nema stabilni shop selektory.
- **Dalsi reprodukce po castecne oprave:** `ShopE2ETests` padaji na tom, ze realny resource resolver vraci `Rarity_Rare`/`Item_PremiumOnly` misto ceskych textu a `POST /api/v1/shop/purchase` konci 500 s `DbUpdateConcurrencyException` pri ukladani `CoinTransaction`.
- **Pravdepodobna pricina:** Shop je zatim pripraveny pro component testy, ale neni napojeny na realny HTTP klientsky service a E2E data. Navic resource soubor `ShopItemCard.resx` neni ve slozce odpovidajici namespace komponenty `Components.Shop` a `InventoryService` vola `Update(user)` nad trackovanou entitou s novou coin transakci, takze EF oznaci transakci jako modified misto added.
- **Oprava:** Doplněn deterministický E2E seed shop itemů, klientský `ShopService`, DI registrace, stabilní shop selektory, české resource soubory pro komponentu v namespace `Components.Shop`, bezpečné per-user/item zamykání nákupu a EF mapování `CoinTransaction` včetně migrace. UI podporuje balance, kategorie, purchase, owned/equipped stav, premium gate, rarity badge a insuficientní coins edge case.
- **Overeni:** `ShopE2ETests` prošly 4/4; související core coin/inventory testy prošly 55/55 a Blazor Shop component/page testy prošly 16/16. Screenshoty shop přehledu a after-equip flow byly zkontrolovány: české texty jsou správné, položky se nepřekrývají, premium gate je srozumitelný, balance/equipped/owned stavy odpovídají scénáři.
- **Poznamky:** Pri oprave zachovat ceske texty v `.resx` a nepouzivat externi assety pro shop item obrazky.

### E2E-BUG-0093: Stripe webhook scenare nemaji E2E fake adapter

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Premium / Stripe Webhook / API
- **Nalezeno v testu:** `Premium_StripeWebhookCheckoutCompleted_ActivatesSubscription`, `Premium_StripeWebhookInvoicePaid_ExtendsSubscription`, `Premium_StripeWebhookInvoiceFailed_MarksPastDue`, `Premium_StripeWebhookSubscriptionCancelled_MarksCancelled`
- **Screenshot/trace:** API-only E2E scenare bez screenshotu; vystup testu ukazuje `404 NotFound` na `POST /api/v1/webhooks/stripe/e2e`.
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, E2E API process
- **Reprodukce:**
  1. Registrovat E2E uzivatele.
  2. Pripravit Stripe customer/subscription id v testovaci DB.
  3. Poslat fake webhook payload na `/api/v1/webhooks/stripe/e2e` pro `checkout.session.completed`, `invoice.paid`, `invoice.payment_failed` nebo `customer.subscription.deleted`.
- **Ocekavani:** E2E fake adapter deterministicky zavola stejne business handlery jako Stripe webhook a vrati `200 OK`.
- **Skutecnost:** Endpoint neexistuje a vraci `404 NotFound`; pouze invalid signature test na realnem `/stripe` endpointu prochazi.
- **Pravdepodobna pricina:** Produkcni webhook controller umi jen realny Stripe event parsing/signature flow, ale nema testovy/fake adapter pro deterministicke E2E scenare.
- **Oprava:** Do `WebhookController` byl doplněn E2E-only endpoint `/api/v1/webhooks/stripe/e2e`, který přijímá deterministický fake payload a volá stejné subscription handlery pro completed, invoice paid, payment failed a subscription deleted. Invalid signature se dál testuje na reálném `/api/v1/webhooks/stripe` endpointu.
- **Overeni:** `Premium_StripeWebhook*` testy prošly 5/5 a celá `PremiumE2ETests` třída prošla 12/12.
- **Poznamky:** Fake endpoint musi byt dostupny pouze v `E2E` prostredi.

### E2E-BUG-0092: Premium stranka pada kvuli neregistrovanemu IToastService

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Premium / Blazor / DI
- **Nalezeno v testu:** `Premium_Page_ShowsPlansBestValueAndLockedFeaturesForFreeUser`, `Premium_FakeCheckoutSuccess_ActivatesPremiumAndShowsActiveBadge`, `Premium_CancelAndExpiredSubscription_UpdateDisplayedStatus`
- **Screenshot/trace:** `artifacts/e2e/failures/premium/overview-free-locked-features/20260619-150905.png`, `artifacts/e2e/failures/premium/overview-free-locked-features/20260619-150905-console.log`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit E2E uzivatele.
  2. Otevrit `/premium`.
  3. Cekat na root selector `data-testid="premium-page"`.
- **Ocekavani:** Premium stranka se vykresli a zobrazi plany, locked features nebo aktivni premium stav.
- **Skutecnost:** Blazor vyhodi runtime chybu `Cannot provide a value for property 'ToastService' ... There is no registered service of type 'LexiQuest.Blazor.Services.IToastService'` a stranka se nevykresli.
- **Pravdepodobna pricina:** DI registruje konkretni `ToastService`, ale ne interface `IToastService`, zatimco Premium a checkout success pouzivaji interface injekci.
- **Oprava:** Doplněn `TempoToastServiceAdapter`, který implementuje lokální `IToastService` nad `Tempo.Blazor.Services.ToastService`, a DI registruje interface bez rozbití existujících stránek injektujících konkrétní Tempo službu.
- **Overeni:** `PremiumE2ETests` po opravě prošly 7/7; `/premium` se vykreslí, neobsahuje console error a screenshoty Premium flow jsou vytvořené.
- **Poznamky:** Oprava ma zachovat existujici konkretni `ToastService` pro stare stranky a jen doplnit interface mapovani.

### E2E-BUG-0091: Premium checkout nema realny E2E fake success/cancel tok

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Premium / Checkout / API
- **Nalezeno v testu:** `Premium_FakeCheckoutSuccess_ActivatesPremiumAndShowsActiveBadge`
- **Screenshot/trace:** `artifacts/e2e/failures/premium/fake-checkout-success-activates/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit free E2E uzivatele.
  2. Otevrit `/premium`.
  3. Spustit roční checkout a cekat na `/premium/success`.
- **Ocekavani:** V E2E prostredi checkout vrati lokalni fake redirect, success stranka aktivuje premium a `/api/v1/premium/status` vraci aktivni roční plan.
- **Skutecnost:** Test zatim konci uz na chybejicim CTA selektoru; ctenim backendu je potvrzeno, ze `ISubscriptionService` je registrovany na placeholder `SubscriptionService`, jehoz checkout metoda hazi `NotImplementedException`, a success stranka premium neaktivuje.
- **Pravdepodobna pricina:** Infrastrukturni `StripeSubscriptionService` existuje, ale neni pouzit jako `ISubscriptionService`; E2E fake checkout a success confirmation kontrakt nejsou dokoncene.
- **Oprava:** `ISubscriptionService` je v API napojený na `StripeSubscriptionService`, E2E Stripe settings vrací lokální `/premium/success` URL, success stránka volá chráněný E2E fake-complete endpoint a subscription aktivace synchronizuje `Subscriptions` i `User.Premium`.
- **Overeni:** `PremiumE2ETests` prošly 7/7. Testy ověřují monthly/yearly/lifetime fake checkout URL, yearly success redirect, aktivní roční premium status, profilový premium badge, cancel bez aktivace a zrušení aktivní subscription.
- **Poznamky:** E2E fake nesmi volat externi Stripe a musi zustat deterministicky v testcontainer prostredi.

### E2E-BUG-0090: Premium page nema stabilni selektory, locked feature stav ani cancel CTA

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Premium / UX / Testability
- **Nalezeno v testu:** `Premium_Page_ShowsPlansBestValueAndLockedFeaturesForFreeUser`, `Premium_CancelAndExpiredSubscription_UpdateDisplayedStatus`, `Premium_CheckoutCancel_DoesNotActivatePremium`
- **Screenshot/trace:** `artifacts/e2e/failures/premium/overview-free-locked-features/`, `artifacts/e2e/failures/premium/cancel-and-expired-status/`, `artifacts/e2e/failures/premium/checkout-cancel-no-activation/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Otevrit `/premium` jako free uzivatel.
  2. Ocekavat root `data-testid`, tri plan cards, yearly best value badge a zamcene premium features.
  3. Otevrit `/premium` jako premium uzivatel a ocekavat active badge + cancel subscription akci.
  4. Otevrit `/premium/cancel` a ocekavat stabilni checkout cancel selektor.
- **Ocekavani:** Premium UI ma stabilni selektory, jasny locked/unlocked stav funkci, cancel subscription CTA a lokalizovane checkout success/cancel stavy.
- **Skutecnost:** Root `premium-page`, plan/CTA selektory, checkout cancel selector, feature availability section a cancel CTA chybi; UI se neda pokryt E2E bez krehkych selectoru.
- **Pravdepodobna pricina:** Premium page byla pokryta hlavne bUnit/css selektory a nema kompletni E2E/UX stavovou vrstvu.
- **Oprava:** Premium page má stabilní `data-testid` selektory, tři plan cards, best value badge, locked/unlocked feature availability s tooltipem, active badge, cancel CTA a lokalizované error/success toasty. Stránka dostala lokální CSS token fallbacky, aby screenshot působil jako hotové UI.
- **Overeni:** `PremiumE2ETests` prošly 7/7. Screenshoty `artifacts/e2e/screenshots/premium/overview-free-locked-features/1366x900/light/free.png`, `artifacts/e2e/screenshots/premium/fake-checkout-success-activates/1366x900/light/active.png` a `artifacts/e2e/screenshots/premium/checkout-cancel-no-activation/1366x900/light/cancel.png` byly zkontrolovány: texty jsou česky, karty se nepřekrývají, locked/unlocked stavy jsou čitelné a CTA hierarchie je srozumitelná.
- **Poznamky:** Pri oprave odstranit hardcodovane user-facing error texty v `Premium.razor`.

### E2E-BUG-0089: Settings screenshot obsahuje anglicky file input a nečesky format casu

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Settings / Lokalizace / UX
- **Nalezeno v testu:** UX review screenshotu `Settings_ProfileUsernameDuplicateAndAvatarValidation_WorkEndToEnd`, `Settings_PreferencesThemeLanguageNotificationsAndPrivacy_PersistToProfile`
- **Screenshot/trace:** `artifacts/e2e/screenshots/settings/profile-username-avatar-validation/1366x900/light/profile-saved.png`, `artifacts/e2e/screenshots/settings/preferences-theme-language-privacy/1366x900/light/saved.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Otevrit `/settings`.
  2. Nahrat avatar a ulozit profil.
  3. Nastavit cas pripominky streaku na `18:45` a ulozit preference.
- **Ocekavani:** Settings UI ma jen ceske user-facing texty a cas se zobrazuje ve 24hodinovem formatu `18:45`.
- **Skutecnost:** Nativni file input zobrazuje anglicke texty `Choose File`/`No file chosen` a nativni time input v Chromium screenshotu ukazuje `06:45 PM`.
- **Pravdepodobna pricina:** Page pouziva viditelny browser-native file input a `input type="time"`, jehoz zobrazeni se ridi browser locale, ne ceskymi resource texty aplikace.
- **Oprava:** Nativni file input zustava dostupny pro upload, ale je vizualne skryty za ceskym tlacitkem `Změnit avatar`; cas pripominky byl zmenen z browser-native `type=time` na textove pole `HH:mm`.
- **Overeni:** `SettingsE2ETests` prosly 4/4 po oprave. Screenshoty `profile-saved.png` a `saved.png` byly zkontrolovany: neobsahuji `Choose File`/`No file chosen` a cas se zobrazuje jako `18:45`.
- **Poznamky:** File input ma zustat dostupny pro Playwright `SetInputFilesAsync`, ale nemusi byt vizualne viditelny.

### E2E-BUG-0088: Nastaveni nema kompletni E2E/UX ovladani pro profil, preference a danger zone

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Settings / Profile / UX / Account
- **Nalezeno v testu:** `Settings_ProfileUsernameDuplicateAndAvatarValidation_WorkEndToEnd`, `Settings_DangerZone_LogoutDeactivateAndDeleteRequireConfirmation`
- **Screenshot/trace:** `artifacts/e2e/failures/settings/profile-username-avatar-validation/`, `artifacts/e2e/failures/settings/danger-zone-logout-deactivate-delete/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Registrovat E2E uzivatele a prihlasit ho do `/settings`.
  2. Zkontrolovat stabilni root selektor settings stranky a avatar upload s preview/validaci.
  3. Zkusit danger zone akce odhlaseni, deaktivaci a smazani uctu s potvrzenim.
- **Ocekavani:** Settings stranka ma stabilni E2E selektory, ceske popisky, avatar upload preview s validaci typu/velikosti, vsechny notifikacni/preferencni toggly vcetne reminder casu a realne danger zone akce.
- **Skutecnost:** Root `data-testid="settings-page"` chybi, avatar upload neni implementovan, cast togglu/reminderu/jazyka nema UI, danger zone nema stabilni selektory a smazani uctu je prazdna metoda bez backendove akce.
- **Pravdepodobna pricina:** Settings page byla pripravena jako zakladni formular pro component testy, ale nebyla dotazena pro plne E2E pokryti a realne account lifecycle scenare.
- **Oprava:** Settings page byla prepracovana na kompletni E2E-testovatelny formular se stabilnimi selektory, avatar upload preview/validaci, vsemi preferencemi, privacy volbami a danger zone confirm modalem. Backend doplnen o avatar ulozeni v profilu, deaktivaci uctu a smazani uctu pres realne autentizovane endpointy.
- **Overeni:** `SettingsE2ETests` prosly 4/4. Testy overuji username update, duplicitni username, avatar validaci typu/velikosti, zmenu hesla, spatne stare heslo, vsechny preference, privacy public/friends/private, logout, deaktivaci i delete s potvrzenim.
- **Poznamky:** Pri oprave je potreba zachovat ceske user-facing texty v `.resx` a nepouzivat hardcoded anglicke radio labely.

### E2E-BUG-0087: Marathon victory modal na desktopu urezava spodni tlacitko

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Boss / UX / Modal
- **Nalezeno v testu:** `Boss_Marathon_VictoryModal_ShowsPerfectAndSpeedBonuses`
- **Screenshot/trace:** `artifacts/e2e/screenshots/boss/marathon-victory-perfect-speed/1366x900/light/victory-modal.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Dokončit Marathon boss se zkrácenou E2E session na jedno kolo.
  2. Otevřít victory modal.
  3. Zkontrolovat spodní CTA `Zpět na přehled`.
- **Ocekavani:** Celý modal včetně primárního CTA je viditelný bez ořezu a bez nutnosti posouvat screenshot.
- **Skutecnost:** Spodní tlačítko je na screenshotu částečně uříznuté.
- **Pravdepodobna pricina:** Modal karta má příliš velké vnitřní rozestupy a výšku pro viewport 1366x900.
- **Oprava:** Victory modal byl zkompaktněn přes boss-page CSS, aby se primární CTA vešlo do desktop viewportu bez ořezu.
- **Overeni:** `Boss_Marathon_VictoryModal_ShowsPerfectAndSpeedBonuses` prošel po opravě; screenshot `artifacts/e2e/screenshots/boss/marathon-victory-perfect-speed/1366x900/light/victory-modal.png` ukazuje celé tlačítko `Zpět na přehled`.

### E2E-BUG-0086: Marathon perfect bonus se zobrazi, ale nepricte do celkovych XP

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Boss / XP / UI
- **Nalezeno v testu:** `Boss_Marathon_VictoryModal_ShowsPerfectAndSpeedBonuses`
- **Screenshot/trace:** `artifacts/e2e/screenshots/boss/marathon-victory-perfect-speed/1366x900/light/victory-modal.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Spustit Marathon boss session.
  2. V E2E setupu zkrátit session na jedno kolo.
  3. Odpovědět správně bez ztráty života.
  4. Zkontrolovat victory modal a rozpis XP.
- **Ocekavani:** Celkové XP obsahuje základní XP, speed bonus pod 5 minut a perfect bonus `+200 XP`; rozpis XP nesmí obsahovat záporný základ.
- **Skutecnost:** Modal ukazuje perfect bonus `+200 XP`, ale celkové XP je jen 65 a základní XP v rozpisu vychází `-185 XP`.
- **Pravdepodobna pricina:** `BossGameService` přičítá completion a speed bonus, ale `PerfectBonus` vyplní pouze do DTO a nepřičte ho do `session.TotalXP`.
- **Oprava:** `BossGameService` přičítá `PerfectBonus` do `totalBonus` spolu se speed/completion bonusem; E2E test navíc ověřuje, že dokončený boss state má alespoň bonusových 250 XP.
- **Overeni:** `Boss_Marathon_VictoryModal_ShowsPerfectAndSpeedBonuses` prošel po opravě; screenshot ukazuje `Celkem XP 265`, `Základní XP 15 XP`, `Bonus za perfektní hru +200 XP` a `Bonus za rychlost +50 XP`.

### E2E-BUG-0085: Boss obrazovky zobrazuji lokalizacni klice a nejsou vizualne hotove

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Boss / Lokalizace / UX
- **Nalezeno v testu:** `Boss_Marathon_StartsWithTwentyWordsThreeLivesAndNoRegen`
- **Screenshot/trace:** `artifacts/e2e/screenshots/boss/marathon-start-no-regen/1366x900/light/start.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Spustit Marathon boss session pres `POST /api/v1/boss/start`.
  2. Prihlasit uzivatele v UI a otevrit `/boss/marathon/{sessionId}`.
  3. Zkontrolovat screenshot startovni obrazovky.
- **Ocekavani:** Boss UI ma ceske texty z resource souboru, jasnou herni hierarchii, citelne staty a profesionalni spacing.
- **Skutecnost:** Stranka zobrazuje klice jako `MarathonBoss_Title`, `MarathonBoss_Subtitle`, `MarathonBoss_Submit`; vetsina Tailwind-like trid se neuplatni a obrazovka pusobi jako neostylovany formular.
- **Pravdepodobna pricina:** Resource soubory jsou ve slozce `Resources/Pages/Game`, ale komponenty jsou v namespace `Pages.BossGame`; zaroven boss markup spoleha na CSS tridy, ktere aplikace negeneruje.
- **Oprava:** Resource soubory boss stránek přesunuty pod `Resources/Pages/BossGame`, texty upraveny do češtiny a do globálního CSS doplněn boss-page styling pro hlavičky, staty, herní kartu, inputy a modaly.
- **Overeni:** `Boss_Marathon_StartsWithTwentyWordsThreeLivesAndNoRegen` prošel po opravě; screenshot `artifacts/e2e/screenshots/boss/marathon-start-no-regen/1366x900/light/start.png` byl zkontrolován a už nezobrazuje lokalizační klíče ani neostylovaný formulář.

### E2E-BUG-0084: Boss levely nemaji realny API start endpoint

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Boss / API / E2E flow
- **Nalezeno v testu:** `Boss_Marathon_StartsWithTwentyWordsThreeLivesAndNoRegen`
- **Screenshot/trace:** Zatim bez screenshotu; test konci na API `404` pred nactenim UI.
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Registrovat a prihlasit E2E uzivatele pres realny auth endpoint.
  2. Zavolat `POST /api/v1/boss/start` s `BossType=Marathon` a `Difficulty=Intermediate`.
  3. Ocekavat zalozeni boss session a navigaci na `/boss/marathon/{sessionId}`.
- **Ocekavani:** API vrati `201 Created` s boss session stavem, kde Marathon ma 20 kol, 3 zivoty a zadny regen.
- **Skutecnost:** API vraci `404 NotFound`, protoze boss controller/endpoint neni dostupny.
- **Pravdepodobna pricina:** Domenove boss entity a Blazor stranky existuji, ale chybi verejny HTTP kontrakt a klientsky `IBossService` napojeny na API.
- **Oprava:** Doplněn sdílený boss DTO kontrakt, `IBossGameService`, perzistentní `BossGameService`, `BossController` pro `/api/v1/boss`, Blazor `BossService` a propojení Marathon stránky s reálným session stavem.
- **Overeni:** `Boss_Marathon_StartsWithTwentyWordsThreeLivesAndNoRegen` prošel 2026-06-19; ověřuje `201 Created`, `BossType=Marathon`, 20 kol, 3 životy, absenci regen textu a UI checkpoint screenshot.

### E2E-BUG-0083: Splneny achievement se po spravne odpovedi neodemkne v UI

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Achievements / Game flow / UI
- **Nalezeno v testu:** `Achievements_FirstWordUnlock_ShowsModalAndDoesNotDuplicate`
- **Screenshot/trace:** `artifacts/e2e/failures/achievements/first-word-unlock-no-duplicate/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Registrovat noveho uzivatele.
  2. Spustit treninkovou hru.
  3. Odpovedet spravne na prvni slovo.
  4. Cekat na achievement unlock modal.
- **Ocekavani:** Prvni spravna odpoved odemkne achievement `first_word`, UI ukaze modal s nazvem a XP a dalsi splneni nevytvori duplicitni odemceni.
- **Skutecnost:** Modal `achievement-unlock-modal` se nezobrazi.
- **Pravdepodobna pricina:** `GameSessionService` nevola `AchievementService.CheckWordSolvedAsync` a `GameRoundResult` nenese informace o nove odemcenych achievementech do Blazoru.
- **Oprava:** `GameRoundResult` nese `UnlockedAchievements`, `GameSessionService` po spravne odpovedi vola `AchievementService.CheckWordSolvedAsync`, `AchievementService` pri odemceni nastavi progress na required value a `Game.razor` zobrazuje lokalizovany achievement unlock modal.
- **Overeni:** `Achievements_FirstWordUnlock_ShowsModalAndDoesNotDuplicate` a cela `AchievementsE2ETests` sada prosly 2026-06-19. Screenshot `artifacts/e2e/screenshots/achievements/first-word-unlock-no-duplicate/1366x900/light/unlock-modal.png` ukazuje odemceni `První slovo` a test overuje, ze dalsi splneni nevytvori druhy unlock zaznam.

### E2E-BUG-0082: Achievement page nema stabilni selektory a vypada jako neostylovany seznam

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Achievements / UI / UX / Testability
- **Nalezeno v testu:** `Achievements_Page_ShowsProgressFiltersAndCardStates`
- **Screenshot/trace:** `artifacts/e2e/failures/achievements/overview-filter-card-states/20260619-133528.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Seednout uzivateli jeden odemceny a jeden rozpracovany achievement.
  2. Otevrit `/achievements`.
  3. Zkontrolovat DOM selektory, filtry a vizualni stav karet.
- **Ocekavani:** Stranka ma stabilni `data-testid`, citelne karty pro locked/in-progress/unlocked stav, progress header a profesionálně stylovane filter taby.
- **Skutecnost:** Chybi `data-testid="achievements-page"` a dalsi selektory; screenshot ukazuje neostylovana nativni tlacitka a dlouhy jednosloupcovy seznam bez jasneho vizualniho rozliseni stavu.
- **Pravdepodobna pricina:** `Achievements.razor` byl pripraven pro component testy pres CSS tridy, ale nema E2E selektory ani page-specific CSS.
- **Oprava:** `Achievements.razor` ma stabilni `data-testid`, `data-state` a `data-category` atributy pro progress, taby a karty; `Achievements.razor.css` definuje grid karet, aktivni taby a vizualni rozliseni locked/in-progress/unlocked stavu.
- **Overeni:** `Achievements_Page_ShowsProgressFiltersAndCardStates` a cela `AchievementsE2ETests` sada prosly 2026-06-19. Screenshot `artifacts/e2e/screenshots/achievements/overview-filter-card-states/1366x900/light/streak-filter.png` byl zkontrolovan z pohledu UX.

### E2E-BUG-0081: E2E databaze nema seednuty achievement katalog

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Achievements / Test data / API
- **Nalezeno v testu:** `Achievements_ApiAuthenticated_ReturnsSeededCatalog`
- **Screenshot/trace:** `artifacts/e2e/failures/achievements/api-authenticated-catalog/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, authenticated HTTP client
- **Reprodukce:**
  1. Opravit auth claim mapping pro achievements endpoint.
  2. Zavolat `GET /api/v1/achievements` v ciste E2E databazi.
  3. Zkontrolovat obsah odpovedi.
- **Ocekavani:** E2E databaze obsahuje deterministicky katalog achievementu pro UI/API testy.
- **Skutecnost:** Endpoint vraci `200 OK`, ale pole achievementu je prazdne.
- **Pravdepodobna pricina:** `E2ETestDataSeeder` seeduje jen slova a learning paths; `SeedData` nema katalog achievementu.
- **Oprava:** `SeedData.GetAchievements()` definuje deterministicky cesky achievement katalog a `E2ETestDataSeeder` ho seeduje po migracich i po resetu databaze.
- **Overeni:** `Achievements_ApiAuthenticated_ReturnsSeededCatalog` prosel 2026-06-19 a vraci katalog s `first_word` a `streak_7`.

### E2E-BUG-0080: Authenticated achievements endpoint vraci 401 kvuli claim mappingu

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Achievements / API / Auth
- **Nalezeno v testu:** `Achievements_ApiAuthenticated_ReturnsSeededCatalog`
- **Screenshot/trace:** `artifacts/e2e/failures/achievements/api-authenticated-catalog/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, authenticated HTTP client s realnym login tokenem
- **Reprodukce:**
  1. Registrovat a prihlasit uzivatele pres realny `/api/v1/users/login`.
  2. Zavolat `GET /api/v1/achievements` s bearer tokenem.
  3. Zkontrolovat status code.
- **Ocekavani:** Endpoint vrati `200 OK` a achievement katalog pro aktualniho uzivatele.
- **Skutecnost:** Endpoint vraci `401 Unauthorized`.
- **Pravdepodobna pricina:** `AchievementsController` a `UserAchievementsController` ctou pouze claim `"sub"`, ale JWT middleware mapuje identitu na `ClaimTypes.NameIdentifier`.
- **Oprava:** `AchievementsController` a `UserAchievementsController` pouzivaji sdileny `ClaimsPrincipalExtensions.GetUserId()`.
- **Overeni:** `Achievements_ApiAuthenticated_ReturnsSeededCatalog` prosel 2026-06-19 s `200 OK` a seednutym katalogem.

### E2E-BUG-0079: Primarni tlacitka denni vyzvy vypadaji jako neaktivni

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Daily Challenge / UX
- **Nalezeno v testu:** UX review screenshotu `DailyChallenge_TodayChallenge_DisplaysExpectedModifierAndStarts`, `DailyChallenge_NextDayReset_AllowsNewChallenge`
- **Screenshot/trace:** `artifacts/e2e/screenshots/daily/today-start/1366x900/light/ready.png`, `artifacts/e2e/screenshots/daily/next-day-reset/1366x900/light/available.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900, light theme
- **Reprodukce:**
  1. Otevrit `/daily-challenge`.
  2. Zkontrolovat tlacitko "Zacit vyzvu".
  3. Spustit vyzvu a zkontrolovat tlacitko "Odeslat odpoved".
- **Ocekavani:** Primarni akce jsou jasne rozpoznatelne jako aktivni tlacitka.
- **Skutecnost:** Tlacitka vypadaji jako maly sedy nativni button, u submitu dokonce jako neaktivni pruh.
- **Pravdepodobna pricina:** Stranka pouziva nativni `button` s tridami `tm-btn tm-btn--primary`, ale bez garantovaneho lokalniho stylu pro tento markup.
- **Oprava:** `DailyChallenge.razor.css` definuje lokalni styl pro `button.tm-btn--primary` na strance denni vyzvy vcetne hover/focus/disabled stavu a full-width submitu v hernim panelu.
- **Overeni:** `DailyChallengeE2ETests` prosly 2026-06-19 3/3. Screenshoty `ready.png` a `available.png` ukazuji jasne modra primarni tlacitka bez prekryvu textu.

### E2E-BUG-0078: Prazdny denni leaderboard nema viditelny empty stav

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Daily Challenge / UX
- **Nalezeno v testu:** `DailyChallenge_TodayChallenge_DisplaysExpectedModifierAndStarts`
- **Screenshot/trace:** `artifacts/e2e/failures/daily/today-start/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Registrovat noveho uzivatele bez denniho dokonceni.
  2. Otevrit `/daily-challenge`.
  3. Zkontrolovat sekci "Dnesni zebricek".
- **Ocekavani:** Prazdny leaderboard ma viditelny a srozumitelny empty stav v cestine.
- **Skutecnost:** Vykresli se prazdny `data-testid="daily-leaderboard"` kontejner bez vysky, ktery neni viditelny ani uzivatelsky informativni.
- **Pravdepodobna pricina:** `DailyChallenge.razor` iteruje jen pres polozky leaderboardu a pro prazdny seznam nema fallback obsah.
- **Oprava:** `DailyChallenge.razor` vykresluje lokalizovany `daily-leaderboard-empty` stav, kdyz je leaderboard prazdny; `DailyChallenge.resx` obsahuje cesky text a CSS ho zobrazuje jako citelny neutrální blok.
- **Overeni:** `DailyChallengeE2ETests` prosly 2026-06-19 3/3. Screenshoty `today-start/ready.png` a `next-day-reset/available.png` ukazuji viditelny empty stav.

### E2E-BUG-0070: Novy uzivatel neni automaticky zarazen do Bronze ligy

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Leagues / Registration / Dashboard navigation
- **Nalezeno v testu:** `Leagues_NewUser_IsAssignedToBronzeLeague`
- **Screenshot/trace:** `artifacts/e2e/failures/leagues/new-user-bronze/20260619-122342.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Registrovat noveho uzivatele.
  2. Prihlasit ho a otevrit `/leagues`.
  3. Zkontrolovat aktualni ligu.
- **Ocekavani:** Novy uzivatel je v Bronzove lize, vidi svuj radek v zebricku s 0 XP a aktualni rank.
- **Skutecnost:** Stranka ukazala prazdny stav "Zatim nejste v zadne lize".
- **Pravdepodobna pricina:** Registrace nevytvarela league participant zaznam a ligovy endpoint nepouzival konzistentni user id claim.
- **Oprava:** `UserService.RegisterAsync` po vytvoreni uzivatele vola `AssignUserToLeagueAsync` pro aktualni tyden, `LeaguesController` pouziva sdileny `ClaimsPrincipalExtensions.GetUserId()` a leaderboard doplnuje username pres `IUserRepository`.
- **Overeni:** `Leagues_NewUser_IsAssignedToBronzeLeague` prosel 2026-06-19. Screenshot `artifacts/e2e/screenshots/leagues/new-user-bronze/1366x900/light/loaded.png` ukazuje Bronzovou ligu, aktualniho uzivatele v zebricku a odmeny tieru.

### E2E-BUG-0071: Jednočlenná liga ukazuje záporný sestupový práh

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Leagues / Progress thresholds / UX
- **Nalezeno v testu:** `Leagues_NewUser_IsAssignedToBronzeLeague`
- **Screenshot/trace:** `artifacts/e2e/screenshots/leagues/new-user-bronze/1366x900/light/loaded.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Registrovat prvniho uzivatele v ciste E2E databazi.
  2. Otevrit `/leagues`.
  3. Zkontrolovat sekci "Postup a sestup".
- **Ocekavani:** Pri jednom ucastnikovi neni zobrazena sestupova zona ani zaporny rank.
- **Skutecnost:** UI ukazovalo "Od #-3" a badge "V sestupove zone".
- **Pravdepodobna pricina:** League threshold vypocet vzdy vracel 5 sestupujicich bez ohledu na pocet ucastniku.
- **Oprava:** `LeagueService.GetPromotionDemotionCounts` omezuje promotion/demotion poctem ucastniku a nedovoli prekryv zón; UI pri absenci sestupu zobrazi "Bez sestupu" a neskresli demotion marker.
- **Overeni:** `LeagueServiceEdgeCaseTests` pokryvaji jednočlennou ligu a `Leagues_NewUser_IsAssignedToBronzeLeague` screenshot po oprave ukazuje "Bez sestupu" bez cerveneho varovani.

### E2E-BUG-0072: Registrace druheho uzivatele do existujici ligy pada na EF concurrency

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Leagues / Registration / Persistence
- **Nalezeno v testu:** `Leagues_Leaderboard_SortsHighlightsAndMarksPromotionDemotionZones`
- **Screenshot/trace:** `artifacts/e2e/failures/leagues/leaderboard-zones/20260619-123612.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer
- **Reprodukce:**
  1. V ciste E2E databazi registrovat prvniho uzivatele.
  2. Registrovat druheho uzivatele.
  3. Automaticke zarazeni do existujici Bronze ligy spadne.
- **Ocekavani:** Druhy a dalsi uzivatele se pripoji do existujici aktivni Bronze ligy bez prepisu jiz existujicich participantu.
- **Skutecnost:** `AssignUserToLeagueAsync` pri `SaveChangesAsync` vyvola `DbUpdateConcurrencyException` nad existujicim `LeagueParticipant`.
- **Pravdepodobna pricina:** Pridani noveho participanta pres nactenou agregaci ligy zpusobi, ze EF pri ukladani zkousi aktualizovat existujici participant radek.
- **Oprava:** `AssignUserToLeagueAsync` pro existujici neplnou ligu pouziva `ILeagueRepository.AddParticipantAsync`, tedy vklada jen novy `LeagueParticipant` bez prepisu jiz nactenych participantu; vytvoreni nove ligy zustava pres aggregate.
- **Overeni:** `LeagueService_AssignUser_ExistingBronzeLeague_AddsOnlyNewParticipant`, `LeagueServiceTests|LeagueServiceEdgeCaseTests` a `Leagues_Leaderboard_SortsHighlightsAndMarksPromotionDemotionZones` prosly. Screenshot `artifacts/e2e/screenshots/leagues/leaderboard-zones/1366x900/light/ranked.png` potvrzuje 12 ucastniku v jedne Bronze lize.

### E2E-BUG-0073: League countdown nema vizualni stav pod 24h a pod 6h

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Leagues / Countdown / UX
- **Nalezeno v testu:** `Leagues_Countdown_UnderThresholds_UsesVisualState`
- **Screenshot/trace:** `artifacts/e2e/failures/leagues/countdown-warning/20260619-124504.png`, `artifacts/e2e/failures/leagues/countdown-critical/20260619-124517.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Registrovat uzivatele a nastavit `WeekEnd` aktivni ligy na mene nez 24 hodin.
  2. Otevrit `/leagues`.
  3. Opakovat pro mene nez 6 hodin.
- **Ocekavani:** Countdown rozlisi stav `warning` pod 24h a `critical` pod 6h pres testovatelny DOM atribut a jasny vizualni styl.
- **Skutecnost:** Countdown ukazuje pouze text, nema `data-state` ani vizualni tridu pro hranice.
- **Pravdepodobna pricina:** `Leagues.razor` pouze formatuje zbyvajici cas, ale nevypocitava UX stav.
- **Oprava:** `Leagues.razor` pocita countdown stav `normal|warning|critical|ended`, vystavuje ho jako `data-state` a prirazuje vizualni tridy; CSS rozlisuje warning a critical barvou pozadi, ramecku a textu.
- **Overeni:** `Leagues_Countdown_UnderThresholds_UsesVisualState` prosel pro 23h i 5h do resetu. Screenshoty `artifacts/e2e/screenshots/leagues/countdown-warning/1366x900/light/warning.png` a `artifacts/e2e/screenshots/leagues/countdown-critical/1366x900/light/critical.png` byly zkontrolovane z pohledu UX.

### E2E-BUG-0074: Weekly reset neposila promoted/demoted hrace do cilove ligy

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Leagues / Weekly reset / Progression
- **Nalezeno v testu:** `LeagueResetJob_Execute_MovesPromotedUsersUp`, `LeagueResetJob_Execute_MovesDemotedUsersDown`, `LeagueResetJob_Execute_LegendTier_StayersRemainInLegend`
- **Screenshot/trace:** `artifacts/e2e/screenshots/leagues/weekly-reset-tier-moves/1366x900/light/promoted-silver.png`, `artifacts/e2e/screenshots/leagues/weekly-reset-tier-moves/1366x900/light/demoted-bronze.png`
- **Prostredi:** Core xUnit, nasledne SQL Server Testcontainer v E2E
- **Reprodukce:**
  1. Pripravit ligu s oznacenymi promoted/demoted ucastniky.
  2. Spustit `LeagueResetJob.ExecuteAsync`.
  3. Zkontrolovat, do jakeho tieru job uzivatele prirazuje pro novy tyden.
- **Ocekavani:** Promoted Bronze jde do Silver, demoted Silver jde do Bronze, Legend promoted zustava v Legend.
- **Skutecnost:** Job vola overload bez tieru, ktery prirazuje vzdy Bronze.
- **Pravdepodobna pricina:** `GetNextTier` a `GetPreviousTier` se vypocitaji jen pro logovani, ale nejsou predane do `AssignUserToLeagueAsync`.
- **Oprava:** `ILeagueService` ma overload s cilovym `LeagueTier`, `LeagueService` hleda aktivni ligu podle tieru a tydne, `LeagueResetJob` predava `nextTier`, `previousTier` nebo puvodni tier podle vysledku resetu.
- **Overeni:** `LeagueServiceTests|LeagueServiceEdgeCaseTests|LeagueResetJobTests` prosly a `Leagues_WeeklyReset_MovesPromotedAndDemotedUsers` overil Bronze -> Silver i Silver -> Bronze pres SQL Server Testcontainer a UI screenshoty.

### E2E-BUG-0075: Aktualni uzivatel v promo/demo zone neni vizualne odlisitelny

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Leagues / Leaderboard / UX
- **Nalezeno v testu:** UX review screenshotu `Leagues_WeeklyReset_MovesPromotedAndDemotedUsers`
- **Screenshot/trace:** `artifacts/e2e/screenshots/leagues/weekly-reset-tier-moves/1366x900/light/promoted-silver.png`, `artifacts/e2e/screenshots/leagues/weekly-reset-tier-moves/1366x900/light/demoted-bronze.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Dostat aktualniho uzivatele do promotion nebo demotion zony.
  2. Otevrit `/leagues`.
  3. Porovnat jeho radek s ostatnimi radky ve stejne zone.
- **Ocekavani:** Aktualni uzivatel je stale jasne odlisitelny i uvnitr promo/demo zony.
- **Skutecnost:** Zeleny/cerveny zone styl prebije modre current-user zvyrazneni a radek vypada jako bezny radek v zone.
- **Pravdepodobna pricina:** CSS pravidla `.promotion-zone` a `.demotion-zone` jsou aplikovana po `.is-current-user` a neexistuje kombinovany current-user styl.
- **Oprava:** CSS pro `.leaderboard-row.is-current-user` pridava modry levy rail a outline i pri kombinaci s `.promotion-zone` nebo `.demotion-zone`; E2E kontroluje computed `box-shadow`.
- **Overeni:** `Leagues_WeeklyReset_MovesPromotedAndDemotedUsers` prosel s current-user outline checkem. Screenshoty `promoted-silver.png` a `demoted-bronze.png` ukazuji aktualniho uzivatele jasne odliseneho uvnitr promo/demo zony.

### E2E-BUG-0076: Historie lig se nikdy nezobrazi

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Leagues / History
- **Nalezeno v testu:** `LeagueService_GetLeagueHistory_ReturnsPastLeagueResult`
- **Screenshot/trace:** `artifacts/e2e/screenshots/leagues/weekly-reset-tier-moves/1366x900/light/promoted-silver.png`, `artifacts/e2e/screenshots/leagues/weekly-reset-tier-moves/1366x900/light/demoted-bronze.png`
- **Prostredi:** Core xUnit, nasledne SQL Server Testcontainer v E2E
- **Reprodukce:**
  1. Pripravit deaktivovanou ligu s participantem uzivatele a vyslednym promoted/demoted stavem.
  2. Zavolat `GetLeagueHistoryAsync`.
  3. Otevrit `/leagues` po weekly resetu.
- **Ocekavani:** Historie vrati minule ligy s tierem, final rankem, weekly XP a status badge.
- **Skutecnost:** `LeagueService.GetLeagueHistoryAsync` vraci vzdy prazdny seznam.
- **Pravdepodobna pricina:** Metoda je zatim stub bez repository dotazu.
- **Oprava:** `LeagueService.GetLeagueHistoryAsync` nacita deaktivovane ligy pres `ILeagueRepository.GetLeagueHistoryForUserAsync` a mapuje participant vysledek na `LeagueHistoryDto`.
- **Overeni:** `LeagueService_GetLeagueHistory_ReturnsPastLeagueResult` a `Leagues_WeeklyReset_MovesPromotedAndDemotedUsers` prosly; screenshoty ukazuji historii Bronzove ligy s Postup a Stribrne ligy se Sestup.

### E2E-BUG-0077: Weekly reset maze historicke ranky a XP

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Leagues / Weekly reset / History
- **Nalezeno v testu:** `Leagues_WeeklyReset_MovesPromotedAndDemotedUsers`
- **Screenshot/trace:** `artifacts/e2e/failures/leagues/weekly-reset-tier-moves/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Pripravit ligu s weekly XP a ranky.
  2. Spustit weekly reset.
  3. Otevrit historii lig.
- **Ocekavani:** Historie zachova final rank, weekly XP a promoted/demoted status minuleho tydne.
- **Skutecnost:** Historie ukazuje `#0`, `0 XP`, `Udrženo`.
- **Pravdepodobna pricina:** `LeagueResetJob` po prirazeni do noveho tydne vola `ResetWeeklyXP()` nad participanty deaktivovane ligy a nasledujici `SaveChanges` je muze persistovat.
- **Oprava:** `LeagueResetJob` uz nenuluje participanty deaktivovane ligy; historicke ranky, weekly XP a promoted/demoted priznaky zustavaji zachovane pro historii.
- **Overeni:** `LeagueResetJob_Execute_PreservesPastWeekParticipantResults`, ligove core testy a `Leagues_WeeklyReset_MovesPromotedAndDemotedUsers` prosly. E2E historie ukazuje `#1 1200 XP Postup` a `#12 100 XP Sestup`.

### E2E-BUG-0069: Missed streak ignoruje aktivni shield i premium auto-freeze

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Game completion / Streak Shield / Premium Freeze
- **Nalezeno v testu:** `Streak_ActiveShield_MissedGracePeriod_PreservesStreakAndConsumesShield`, `Streak_PremiumAutoFreeze_MissedGracePeriod_PreservesStreak`
- **Screenshot/trace:** `artifacts/e2e/failures/streak/active-shield-preserves-missed-streak/`, `artifacts/e2e/failures/streak/premium-auto-freeze-preserves-streak/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer
- **Reprodukce:**
  1. Nastavit uzivateli streak 5 dni s posledni aktivitou 73 hodin zpet.
  2. Varianta A: aktivovat shield.
  3. Varianta B: premium uzivatel s dostupnym freeze.
  4. Dokoncit session.
- **Ocekavani:** Aktivni shield nebo premium freeze streak zachova a posune na 6; shield se spotrebuje nebo freeze oznaci jako pouzity.
- **Skutecnost:** Dokonceni session volalo jen beznou streak aktivitu a resetovalo streak na 1.
- **Pravdepodobna pricina:** `GameSessionService` pri dokončení hry nevyhodnocoval `StreakProtections`.
- **Oprava:** `GameSessionService` pri dokonceni session vola `RecordCompletionStreakAsync`, pro aktivni shield provede `DeactivateShield()` a pro premium uzivatele s dostupnym freeze `UseFreeze()`; v obou ochrannych pripadech `RecordProtectedActivity()` zachova a posune streak.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Streak_ActiveShield_MissedGracePeriod_PreservesStreakAndConsumesShield|FullyQualifiedName~Streak_PremiumAutoFreeze_MissedGracePeriod_PreservesStreak"` prosel 2026-06-19.

### E2E-BUG-0068: Nakup streak shieldu neodecita mince

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** API / Streak Shield / Coins
- **Nalezeno v testu:** `StreakE2ETests.Streak_PurchaseShields_DeductsCoinsAndAddsShields`
- **Screenshot/trace:** `artifacts/e2e/failures/streak/shield-purchase-coins/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer
- **Reprodukce:**
  1. Nastavit uzivateli 500 minci.
  2. Zavolat `POST /api/v1/streak/shield/purchase` s `Quantity = 3`.
  3. Nacist `/api/v1/shop/coins`.
- **Ocekavani:** Nákup 3 shieldu stoji 500 minci a zustatek je 0.
- **Skutecnost:** `StreakProtectionService.PurchaseShieldsAsync` pouze prida shieldy; coin balance neresi.
- **Pravdepodobna pricina:** Service nema pristup k uzivateli/coin transakci a controller vraci placeholder `RemainingCoins = 0`.
- **Oprava:** `PurchaseShieldsAsync` kontroluje zůstatek uživatele, odečte mince přes `SpendCoins`, přidá shieldy a controller vrací skutečný `RemainingCoins`.
- **Overeni:** `Streak_PurchaseShields_DeductsCoinsAndAddsShields` probehl uspesne; `/api/v1/shop/coins` po nakupu vraci zustatek 0.

### E2E-BUG-0067: Nakup shieldu pada na EF concurrency chybe pri novem protection zaznamu

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** API / EF Core / Streak Shield
- **Nalezeno v testu:** `StreakE2ETests.Streak_PurchaseShields_DeductsCoinsAndAddsShields`
- **Screenshot/trace:** `artifacts/e2e/logs/LexiQuest.Api-streak-shield-purchase-coins-stdout.log`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer
- **Reprodukce:**
  1. Uzivatel bez `StreakProtections` zaznamu koupi shieldy.
  2. Service vytvori novy `StreakProtection` a pred ulozenim zavola `Update`.
- **Ocekavani:** Novy protection zaznam se vlozi s nakoupenymi shieldy.
- **Skutecnost:** EF Core provede `UPDATE` neexistujiciho radku a vrati `DbUpdateConcurrencyException`.
- **Pravdepodobna pricina:** Purchase vetev nerozlisuje novy a existujici protection zaznam.
- **Oprava:** Purchase vetev rozlisuje novy protection zaznam a `Update` vola jen pro existujici radky.
- **Overeni:** `Streak_PurchaseShields_DeductsCoinsAndAddsShields` probehl uspesne.

### E2E-BUG-0066: Aktivace free shieldu pada na EF concurrency chybe pri novem protection zaznamu

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** API / EF Core / Streak Shield
- **Nalezeno v testu:** `StreakE2ETests.Streak_DashboardFreeShield_CanActivate`
- **Screenshot/trace:** `artifacts/e2e/logs/LexiQuest.Api-streak-dashboard-free-shield-activate-stdout.log`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless
- **Reprodukce:**
  1. Free uzivatel bez `StreakProtections` zaznamu klikne na aktivaci shieldu.
  2. API vytvori novy `StreakProtection` a pred `SaveChanges` zavola repository `Update`.
- **Ocekavani:** Novy protection zaznam se vlozi a aktivuje.
- **Skutecnost:** EF Core provede `UPDATE` neexistujiciho radku a vrati `DbUpdateConcurrencyException`.
- **Pravdepodobna pricina:** Service nerozlisuje novy a existujici protection zaznam pri volani `Update`.
- **Oprava:** `StreakProtectionService` pri novem protection zaznamu nevola `Update`, ponecha entitu ve stavu `Added` a ulozi ji pres Unit of Work.
- **Overeni:** `Streak_DashboardFreeShield_CanActivate` probehl uspesne.

### E2E-BUG-0065: Free shield aktivace selze pro uzivatele bez protection zaznamu

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** API / Streak Shield
- **Nalezeno v testu:** Pripraveny tok `StreakE2ETests.Streak_DashboardFreeShield_CanActivate`, potvrzeno kontrolou `StreakProtectionService.ActivateShieldAsync`.
- **Screenshot/trace:** N/A
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer
- **Reprodukce:**
  1. Vytvorit noveho free uzivatele bez zaznamu ve `StreakProtections`.
  2. Zavolat `POST /api/v1/streak/shield/activate`.
- **Ocekavani:** Pokud ma uzivatel narok na free shield, service vytvori protection zaznam, aktivuje shield a nastavi cooldown.
- **Skutecnost:** `ActivateShieldAsync` pri `protection == null` vyhodi `InvalidOperationException` / aktivace neni uspesna.
- **Pravdepodobna pricina:** Service podporuje jen aktivaci uz zakoupenych/inventarovych shieldu.
- **Oprava:** `ActivateShieldAsync` umi vytvorit protection zaznam, pridat jednorazovy free shield podle cooldownu a aktivovat ho; controller predava premium stav pro spravny limit.
- **Overeni:** `Streak_DashboardFreeShield_CanActivate` probehl uspesne a core subset `StreakProtectionServiceEdgeCaseTests|StreakProtectionTests` probehl uspesne: 48/48.

### E2E-BUG-0064: Dashboard neumoznuje spolehlive aktivovat streak shield

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Dashboard / Streak Shield / UX
- **Nalezeno v testu:** `StreakE2ETests.Streak_DashboardFreeShield_CanActivate`
- **Screenshot/trace:** `artifacts/e2e/failures/streak/dashboard-free-shield-activate/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Nastavit free uzivatele do at-risk streak stavu.
  2. Otevrit `/dashboard`.
  3. Pokusit se najit a aktivovat shield.
- **Ocekavani:** Tlačítko pro aktivaci má stabilní selector, zavola API a po uspechu dashboard ukaze aktivni shield.
- **Skutecnost:** E2E nenajde stabilni shield activation selector; dashboard zatim nema API wiring pro aktivaci.
- **Pravdepodobna pricina:** `StreakIndicator` nema test id pro shield akce a `Dashboard.razor` nepredava `OnActivateShield` handler.
- **Oprava:** `StreakIndicator` ma test ID pro shield akce a aktivni stav; dashboard predava `OnActivateShield`; Blazor klient vola `POST /api/v1/streak/shield/activate` a po uspechu reloaduje stats.
- **Overeni:** `Streak_DashboardFreeShield_CanActivate` probehl uspesne. Screenshot `artifacts/e2e/screenshots/streak/dashboard-free-shield-activate/1366x900/light/shield-active.png` ukazuje citelny aktivni shield bez overflow.

### E2E-BUG-0063: Leve menu na dashboardu vypada jako neostylovane odkazy

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Dashboard / Layout / UX
- **Nalezeno v testu:** `StreakE2ETests.Streak_DashboardAtRisk_ShowsCountdown`
- **Screenshot/trace:** `artifacts/e2e/screenshots/streak/dashboard-at-risk-countdown/1366x900/light/at-risk.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit uzivatele.
  2. Otevrit `/dashboard`.
  3. Zkontrolovat levou navigaci.
- **Ocekavani:** Navigace je vizualne integrovana s aplikaci, odkazy nejsou syrove modre/podtrzene browser defaulty.
- **Skutecnost:** Leve menu zobrazovalo polozky jako neostylovane modre odkazy, coz pusobilo nedodelane vedle zbytku dashboardu.
- **Pravdepodobna pricina:** `TmSidebar` renderuje navigacni odkazy jednodussim anchor markupem, na ktery se neuplatnily ocekavane `.tm-sidebar-nav-item` styly; host zaroven nacital importni Tempo CSS misto bundlovaneho stylesheetu.
- **Oprava:** `MainLayout` obaluje sidebar stabilnim `data-testid="app-sidebar"` shellem, host nacita `_content/Tempo.Blazor/css/tempo-blazor.bundled.css`, service worker cachuje stejny asset a `app.css` doplnuje fallback styly pro odkazy v `.tm-sidebar`.
- **Overeni:** `Layout_DashboardSidebar_IsStyledAsNavigation` prosel 2026-06-19. Screenshot `artifacts/e2e/screenshots/layout/dashboard-sidebar-styled/1366x900/light/desktop.png` ukazuje polozky menu bez defaultniho modreho underline, s ikonami, aktivnim stavem a konzistentnim odsazenim.

### E2E-BUG-0062: StreakIndicator zobrazuje lokalizacni klice misto ceskych textu

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Dashboard / Streak / Lokalizace
- **Nalezeno v testu:** `StreakE2ETests.Streak_DashboardAtRisk_ShowsCountdown`
- **Screenshot/trace:** `artifacts/e2e/failures/streak/dashboard-at-risk-countdown/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Otevrit dashboard se zobrazenym streak indikátorem.
  2. Zkontrolovat countdown text.
- **Ocekavani:** Komponenta zobrazuje ceske texty, napriklad `Zbývá`.
- **Skutecnost:** Komponenta zobrazuje resource klic `TimeRemaining`.
- **Pravdepodobna pricina:** `.resx` soubor je v `Resources/Components/StreakIndicator.resx`, ale komponenta ma namespace `LexiQuest.Blazor.Components.Stats` a runtime resource hleda pod `Resources/Components/Stats/StreakIndicator.resx`.
- **Oprava:** Resource byl doplnen na runtime cestu `Resources/Components/Stats/StreakIndicator.resx`.
- **Overeni:** `Streak_DashboardAtRisk_ShowsCountdown` probehl uspesne a screenshot `artifacts/e2e/screenshots/streak/dashboard-at-risk-countdown/1366x900/light/at-risk.png` zobrazuje ceske texty `Ve hrozbě` a `Zbývá`.

### E2E-BUG-0061: Dashboard nezobrazuje at-risk streak ani countdown

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Dashboard / Streak / UX
- **Nalezeno v testu:** `StreakE2ETests.Streak_DashboardAtRisk_ShowsCountdown`
- **Screenshot/trace:** `artifacts/e2e/failures/streak/dashboard-at-risk-countdown/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Nastavit uzivateli `CurrentStreak = 4` a posledni aktivitu 26 hodin zpet.
  2. Prihlasit uzivatele a otevrit `/dashboard`.
  3. Hledat streak indikator, at-risk stav a countdown.
- **Ocekavani:** Dashboard ukaze streak komponentu s upozornenim a zbyvajicim casem do propadnuti.
- **Skutecnost:** Dashboard zobrazuje pouze obecnou stat card bez countdownu a bez ochrannych akci.
- **Pravdepodobna pricina:** `UserStatsSummaryDto` ani `Dashboard.razor` nenesou detailni read-only `StreakStatus`.
- **Oprava:** `UserStatsSummaryDto` nese read-only `StreakStatus`, `StreakProtection` a `IsPremium`; `StatsController` status pocita bez zapisu streak aktivity; dashboard pouziva `StreakIndicator` se stabilnimi test ID.
- **Overeni:** `Streak_DashboardAtRisk_ShowsCountdown` probehl uspesne. Screenshot review: streak panel je citelny, countdown nepreteka a stav je jednoznacny.

### E2E-BUG-0060: Streak grace period nepocita presnych 48 hodin

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Streak / Grace period
- **Nalezeno v testu:** `StreakE2ETests.Streak_GracePeriodWithinFortyEightHours_KeepsAndIncrementsStreak`
- **Screenshot/trace:** `artifacts/e2e/failures/streak/grace-period-47-hours/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Nastavit uzivateli `CurrentStreak = 5` a `LastActivityDate = DateTime.UtcNow.AddHours(-47)`.
  2. Dokoncit dalsi session.
  3. Nacist `GET /api/v1/stats/user`.
- **Ocekavani:** Aktivita v ramci 48 hodin streak udrzi a zvysi na 6.
- **Skutecnost:** Streak se resetuje na 1.
- **Pravdepodobna pricina:** `Streak.RecordActivity` porovnava jen `Date`, takze aktivitu z predvcerejsiho pozdniho casu bere jako propadlou i v ramci 48 hodin.
- **Oprava:** `Streak.RecordActivity` uchovava presny cas posledni aktivity a navazuje streak, pokud rozdil od posledni aktivity nepresahl 48 hodin; stejny den stale nepridava dalsi den.
- **Overeni:** `dotnet test tests/LexiQuest.Core.Tests/LexiQuest.Core.Tests.csproj --filter "FullyQualifiedName~StreakServiceTests"` probehl uspesne: 20/20. `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~StreakE2ETests"` probehl uspesne: 5/5.

### E2E-BUG-0059: Dokonceni hry nezapisuje denni streak

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Game / Streak / Stats
- **Nalezeno v testu:** `StreakE2ETests.Streak_FirstCompletedSession_SetsCurrentStreakToOne`
- **Screenshot/trace:** `artifacts/e2e/failures/streak/first-completed-session/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Zaregistrovat noveho uzivatele.
  2. Spustit training session a testove ji zkratit na 1 kolo.
  3. Odpovedet spravne a nacist `GET /api/v1/stats/user`.
- **Ocekavani:** Prvni dokoncena session nastavi `CurrentStreak = 1` a `LongestStreak = 1`.
- **Skutecnost:** `CurrentStreak` i `LongestStreak` zustanou 0.
- **Pravdepodobna pricina:** `GameSessionService` pri `session.Complete()` aktualizuje XP/statistiky, ale nevola zadnou streak logiku.
- **Oprava:** `GameSessionService` pri skutecnem dokonceni session vola `user.Streak.RecordActivity(DateTime.UtcNow)` a stale ulozi i path progress.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Streak_FirstCompletedSession_SetsCurrentStreakToOne"` probehl uspesne.

### E2E-BUG-0058: Path hra po kazde odpovedi vola offline-training-seed a generuje 404 v konzoli

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Game / Paths / PWA offline cache / Console hygiene
- **Nalezeno v testu:** `PathsE2ETests.Paths_CompleteLevel_UpdatesProgressAndShowsPerfectState`
- **Screenshot/trace:** `artifacts/e2e/failures/paths/complete-level-perfect-progress/20260619-112008-console.log`, `artifacts/e2e/logs/LexiQuest.Api-paths-complete-level-perfect-progress-stdout.log`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Spustit path level z `/paths/{pathId}`.
  2. Odeslat spravnou odpoved v path hre.
  3. Zkontrolovat konzoli a API log.
- **Ocekavani:** Offline training seed se cacheuje jen pro training session; path hra negeneruje 404 requesty.
- **Skutecnost:** Po kazde spravne odpovedi path session klient vola `/api/v1/game/{sessionId}/offline-training-seed`, API vraci 404 a Chromium zapise `Failed to load resource`.
- **Pravdepodobna pricina:** `GameService.SubmitAnswerAsync` cacheuje offline seed pro kazdou uspesnou odpoved bez znalosti herniho modu.
- **Oprava:** `GameService` si pamatuje session zalozene jako `GameMode.Training` a offline training seed obnovuje jen pro tyto session. Path session uz po spravne odpovedi offline seed endpoint nevola.
- **Overeni:** `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~GameServiceTests"` probehl uspesne: 9/9. `Paths_CompleteLevel_UpdatesProgressAndShowsPerfectState` po oprave probehl uspesne bez console errors.

### E2E-BUG-0057: Dokonceni path levelu neulozi progress ani perfect stav

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Paths / Progress / Game completion
- **Nalezeno v testu:** `PathsE2ETests.Paths_CompleteLevel_UpdatesProgressAndShowsPerfectState`
- **Screenshot/trace:** `artifacts/e2e/failures/paths/complete-level-perfect-progress/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit noveho uzivatele.
  2. Otevrit beginner path, spustit level 1 a dohrat vsech 10 kol spravne.
  3. Vratit se na detail cesty.
- **Ocekavani:** Level 1 ma stav perfect/completed a level 2 je aktualni; progress je ulozen pro konkretniho uzivatele.
- **Skutecnost:** Level 1 zustane `level-current`, protoze dokončení hry neuklada path progress a `PathService` pocita stav z globalnich `PathLevel` hodnot.
- **Pravdepodobna pricina:** Chybi per-user path progress model a update pri `GameSession.Complete()`.
- **Oprava:** Pridana per-user tabulka `UserPathLevelProgresses` vcetne EF migrace. `GameSessionService` pri dokonceni path levelu upsertuje completed/perfect progress a `PathService` pocita progress, current level a odemykani navazujicich cest z uzivatelskych dat.
- **Overeni:** RED: `Paths_CompleteLevel_UpdatesProgressAndShowsPerfectState` spadl na tom, ze level 1 zustal `level-current`. GREEN: stejny test probehl uspesne a screenshot `artifacts/e2e/screenshots/paths/complete-level-perfect-progress/1366x900/light/perfect-progress.png` potvrzuje perfect stav a dalsi current level. `PathsE2ETests` probehly uspesne 5/5.

### E2E-BUG-0056: Detail levelu cesty nezobrazuje herni parametry ani odmeny

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Paths / Level detail / UX screenshot review
- **Nalezeno v testu:** Screenshot review po `PathsE2ETests.Paths_BeginnerDetail_ShowsMapLevelModalAndStartsPathGame`
- **Screenshot/trace:** `artifacts/e2e/screenshots/paths/beginner-detail-starts-path-game/1366x900/light/level-modal.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit noveho uzivatele.
  2. Otevrit `/paths`, vybrat beginner cestu a kliknout na level 1.
  3. Zkontrolovat modal detailu levelu.
- **Ocekavani:** Detail levelu ukaze konkretni pocet slov, cas, hinty, zivoty a XP odmenu, aby hrac pred startem rozumel pravidlum levelu.
- **Skutecnost:** Modal ukazuje jen stav levelu a obecny popis, takze neplni pozadavek z `todo/Faze-9-E2E-Playwright-Testy.md`.
- **Pravdepodobna pricina:** `PathLevelDto` neobsahuje preview parametry a `LevelDetailModal` renderuje pouze minimalni stavove informace.
- **Oprava:** `PathLevelDto` nese preview parametry levelu, backend `PathService` je plni podle cesty/boss pravidel a `LevelDetailModal` je zobrazuje ve stabilni informacni mrizce. Path start v `GameSessionService` pouziva pravidla vybraneho levelu pro pocet kol, cas a zivoty.
- **Overeni:** RED: `Paths_BeginnerDetail_ShowsMapLevelModalAndStartsPathGame` spadl na chybejicim `path-level-detail-word-count`. GREEN: stejny test probehl uspesne; `PathsE2ETests` probehly uspesne 3/3. Screenshot `level-modal.png` zkontrolovan a modal je citelny bez pretekani.

### E2E-BUG-0055: Screenshot cest ukazuje neostylovany seznam misto citelnych karet

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Paths / UX screenshot review
- **Nalezeno v testu:** Screenshot review po `PathsE2ETests`
- **Screenshot/trace:** `artifacts/e2e/screenshots/paths/new-user-lock-state/1366x900/light/loaded.png`, `artifacts/e2e/screenshots/paths/level-five-unlocks-intermediate/1366x900/light/loaded.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Spustit `PathsE2ETests`.
  2. Otevrit screenshot cest.
  3. Zkontrolovat rozlozeni seznamu cest.
- **Ocekavani:** Cesty jsou zobrazene jako citelne karty v responsivnim gridu s jasnym locked/unlocked stavem a progress barem.
- **Skutecnost:** Obsah je vykreslen jako neostylovany textovy sloupec, tlacitka jsou default HTML a locked stavy nejsou vizualne jasne.
- **Pravdepodobna pricina:** `Paths.razor` nema scoped CSS pro layout a stavove prvky.
- **Oprava:** `Paths.razor` dostala scoped CSS pro responsivni grid, karty, locked badge, progress bar a primarni akce.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PathsE2ETests"` probehl uspesne: 2/2. Screenshoty `new-user-lock-state` a `level-five-unlocks-intermediate` byly zkontrolovany a jsou pouzitelne pro UX review.

### E2E-BUG-0054: Stranka Cesty nema funkcni E2E tok, API data ani stabilni selektory

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Paths / Learning paths / API / UX
- **Nalezeno v testu:** `PathsE2ETests.Paths_NewUser_ShowsFourPathsAndOnlyBeginnerUnlocked`, `PathsE2ETests.Paths_LevelFiveUser_UnlocksIntermediatePath`
- **Screenshot/trace:** `artifacts/e2e/failures/paths/new-user-lock-state/`, `artifacts/e2e/failures/paths/level-five-unlocks-intermediate/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit noveho uzivatele.
  2. Otevrit `/paths`.
  3. Cekat na `paths-page` a karty cest.
- **Ocekavani:** Stranka zobrazi 4 cesty, Beginner je odemcena a vyssi cesty jsou zamcene; level 5 uzivatel vidi Intermediate odemcenou.
- **Skutecnost:** `paths-page` ani karty nejsou dostupne stabilnim selektorem; frontend nema realnou HTTP implementaci `IPathService`, API nema paths controller a E2E DB neseeduje cesty.
- **Pravdepodobna pricina:** Faze 1 mela bUnit/core pripravu, ale chybi integracni propojeni API -> Blazor -> E2E seed a UI stale obsahuje cast hardcoded labelu.
- **Oprava:** Doplnene seedovani 4 learning paths v E2E DB, `PathsController` s `GET /api/v1/paths`, Blazor `PathService`, DI registrace, stabilni `data-testid` selektory a lokalizovane labely bez hardcoded anglictiny.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PathsE2ETests"` probehl uspesne: 2/2. `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~PathsPageTests"` probehl uspesne: 5/5.

### E2E-BUG-0053: XP bar na dashboardu zobrazuje resource klice misto ceskych hodnot

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Dashboard / XP / Lokalizace / UX
- **Nalezeno v testu:** `GameFlowE2ETests.Dashboard_XpBar_MatchesStatsApiProgress`
- **Screenshot/trace:** `artifacts/e2e/screenshots/dashboard/xp-bar-api-progress/1366x900/light/loaded.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Nastavit uzivateli 375 XP a otevrit dashboard.
  2. Zobrazit `xp-bar`.
  3. Zkontrolovat text levelu.
- **Ocekavani:** XP bar zobrazi lokalizovany text s aktualnim levelem, napr. `Úroveň 3`, a pomer XP podle API.
- **Skutecnost:** XP bar zobrazi pouze klic `Level`.
- **Pravdepodobna pricina:** Resource soubor existuje jen jako `Resources/Components/XpBar.resx`, ale komponenta je v namespace `LexiQuest.Blazor.Components.Game.XpBar` a potrebuje `Resources/Components/Game/XpBar.resx`.
- **Oprava:** Doplnene `Resources/Components/Game/XpBar.resx`; `StatsController` vraci `XPProgress`, dashboard zobrazuje `XpBar` se stabilnimi test id a komponenta ma lokalizovany text i scoped styling.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Dashboard_XpBar_MatchesStatsApiProgress"` probehl uspesne: 1/1. Screenshot potvrzuje level, XP pomer i progress bar podle API.

### E2E-BUG-0052: Po prekroceni XP hranice se ve hre nezobrazi level-up modal

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Game / XP / Level-up / UX
- **Nalezeno v testu:** `GameFlowE2ETests.Game_LevelUp_ShowsModalWhenXpCrossesThreshold`
- **Screenshot/trace:** `artifacts/e2e/screenshots/game/level-up-modal/1366x900/light/visible.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Nastavit uzivatele na 95 XP a level 1.
  2. Spustit training hru a odeslat spravnou odpoved.
  3. Cekat na `level-up-modal`.
- **Ocekavani:** Po prekroceni hranice pro level 2 se zobrazi lokalizovany level-up modal s novym levelem.
- **Skutecnost:** XP se pripisou, ale UI nezobrazi zadny `level-up-modal`.
- **Pravdepodobna pricina:** `GameRoundResult` nenese `XPGainedEvent` a `Game.razor` komponentu `LevelUpModal` v hernim toku nepouziva.
- **Oprava:** `GameRoundResult` nese `XPGainedEvent`, `GameSessionService` generuje level-up event pri pripsani XP do `User.Stats`, herni stranka zobrazuje `LevelUpModal` se stabilnim `data-testid`, ceskymi resource texty a odolnou lokalizaci unlocku. `UserStats` pouziva stejnou XP krivku jako `LevelCalculator`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Game_LevelUp_ShowsModalWhenXpCrossesThreshold"` probehl uspesne: 1/1. Screenshot byl vizualne zkontrolovan; modal je citelny, cesky a ve viewportu neprekryva obsah.

### E2E-BUG-0051: ForceUserStatsAsync pocita se spatnymi nazvy owned stats sloupcu

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** E2E infra / Test data setup / DB schema
- **Nalezeno v testu:** `GameFlowE2ETests.Game_LevelUp_ShowsModalWhenXpCrossesThreshold`
- **Screenshot/trace:** `artifacts/e2e/failures/game/level-up-modal/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer
- **Reprodukce:**
  1. V E2E testu zavolat `ForceUserStatsAsync`.
  2. Helper spusti `UPDATE Users SET TotalXP = ...`.
- **Ocekavani:** Helper nastavi owned `UserStats` sloupce bez vazby na historicky prefix sloupcu.
- **Skutecnost:** SQL Server vrati `Invalid column name 'TotalXP'`, `Level`, `TotalWordsSolved`, `Accuracy`, `AverageResponseTime`.
- **Pravdepodobna pricina:** E2E DB schema muze mit owned stats sloupce s prefixem `Stats_`, zatimco helper pouzil neprefixovanou variantu.
- **Oprava:** `ForceUserStatsAsync` introspektuje skutecne sloupce tabulky `Users` pres `INFORMATION_SCHEMA.COLUMNS` a pouzije bud neprefixovane, nebo `Stats_` nazvy owned sloupcu.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Game_LevelUp_ShowsModalWhenXpCrossesThreshold"` probehl uspesne: 1/1; helper nastavil XP/level bez SQL chyby.

### E2E-BUG-0050: Spravna odpoved nepripise XP a statistiky do uzivatelskeho profilu

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Game / XP / User stats / Dashboard
- **Nalezeno v testu:** `GameFlowE2ETests.Game_CorrectAnswer_UpdatesUserStatsXpAndDashboardValues`
- **Screenshot/trace:** `artifacts/e2e/failures/game/xp-updates-user-stats/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit uzivatele a spustit training.
  2. Odeslat spravnou odpoved.
  3. Precist `/api/v1/stats/user`.
- **Ocekavani:** `TotalXP` je vetsi nez 0, `TotalWordsSolved` je 1 a `Accuracy` je 100 %; dashboard ukaze stejne hodnoty.
- **Skutecnost:** Herni feedback ukaze spravnou odpoved, ale `stats.TotalXP` zustava 0.
- **Pravdepodobna pricina:** `GameSessionService.SubmitAnswerAsync` uklada XP jen do `GameSession.TotalXP`, ale nevola `User.Stats.AddXP`, `UpdateAccuracy` ani `UpdateAverageResponseTime`.
- **Oprava:** `GameSessionService.SubmitAnswerAsync` aktualizuje pri pokusu uzivatelske statistiky: pri spravne odpovedi pripise XP do `User.Stats`, pri kazdem pokusu aktualizuje presnost, pocet slov a prumerny cas. Pro stare service testy bez seedovaneho uzivatele je update statistiky podmineny existenci uzivatele.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Game_XpSpeedBonusThresholds_AreApplied|FullyQualifiedName~Game_XpComboAndStreakBonuses_AreAppliedAcrossSession|FullyQualifiedName~Game_CorrectAnswer_UpdatesUserStatsXpAndDashboardValues"` probehl uspesne: 6/6. `dotnet test tests/LexiQuest.Infrastructure.Tests/LexiQuest.Infrastructure.Tests.csproj --filter "FullyQualifiedName~GameSessionServiceTests"` probehl uspesne: 20/20.

### E2E-BUG-0049: Low-lives screenshot ma nalepenou lives listu a hur citelny regen stav

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Game / Lives / UX screenshot review
- **Nalezeno v testu:** Screenshot review po `GameFlowE2ETests.Game_LowLives_ShowsWarningAndRegenTimer`
- **Screenshot/trace:** `artifacts/e2e/screenshots/game/low-lives-warning-regen/1366x900/light/warning.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Spustit low-lives E2E scenar.
  2. Otevrit screenshot `warning.png`.
  3. Zkontrolovat header herni areny.
- **Ocekavani:** Pocet zivotu, warning a regen timer jsou citelne oddelene a profesionalne zarovnane.
- **Skutecnost:** Text `Životy`, `1/5`, srdce, warning a regen timer pusobi nalepene v jedne lince.
- **Pravdepodobna pricina:** `LivesIndicator` nema vlastni scoped CSS a spoleha na parent styling.
- **Oprava:** `LivesIndicator` dostal vlastni scoped CSS pro rozestupy, warning badge a regen timer; resource label byl upraven na `Životy:`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Game_LowLives_ShowsWarningAndRegenTimer"` probehl uspesne: 1/1. Screenshot `artifacts/e2e/screenshots/game/low-lives-warning-regen/1366x900/light/warning.png` byl zkontrolovan vizualne.

### E2E-BUG-0048: Low-lives warning a regen timer se v herni arene nezobrazi

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Game / Lives / UX
- **Nalezeno v testu:** `GameFlowE2ETests.Game_LowLives_ShowsWarningAndRegenTimer`
- **Screenshot/trace:** `artifacts/e2e/failures/game/low-lives-warning-regen/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Pres API spustit TimeAttack hru.
  2. V test DB nastavit session na 1/5 zivotu a uzivateli `NextLifeRegenAt`.
  3. Otevrit `/game/{sessionId}`.
- **Ocekavani:** UI ukaze `game-low-lives-warning`, `game-lives-regen` a cesky text `Další život za`.
- **Skutecnost:** Pocet `1/5` je videt, ale warning ani regen timer nejsou dostupne stabilnim selektorem.
- **Pravdepodobna pricina:** `LivesIndicator` mel pouze obecny timer bez `data-testid`, chybela namespace-presna resource cesta `Resources/Components/Game/LivesIndicator.resx` a `GameSessionService.GetSessionStateAsync` nevracel `User.NextLifeRegenAt`.
- **Oprava:** `LivesIndicator` zobrazuje low-lives warning a regen timer se stabilnimi `data-testid`; doplnena namespace resource cesta; `GameSessionService` vraci `NextLifeRegenAt` a pri ztrate zivota planuje regeneraci.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Game_LowLives_ShowsWarningAndRegenTimer"` probehl uspesne: 1/1.

### E2E-BUG-0047: Po ztrate posledniho zivota chybi game-over stav v UI

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Game / Lives / Game over / UX
- **Nalezeno v testu:** `GameFlowE2ETests.Game_LastLifeLost_ShowsGameOverAndDisablesInput`
- **Screenshot/trace:** `artifacts/e2e/failures/game/last-life-game-over/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Pres API spustit TimeAttack hru.
  2. V test DB nastavit session na 1/5 zivotu.
  3. Otevrit `/game/{sessionId}` a odeslat spatnou odpoved.
- **Ocekavani:** UI ukaze `game-over`, vstup pro odpoved se deaktivuje a pocet zivotu je `0/5`.
- **Skutecnost:** Server vrati game-over vysledek, ale herni arena zustane bez game-over stavu.
- **Pravdepodobna pricina:** `GameArena.ShowResult` si neuklada `IsGameOver` a komponenta nema game-over overlay ani disabling logiku.
- **Oprava:** `GameArena` si uklada `IsGameOver`, zobrazuje lokalizovany `game-over` overlay a po konci hry deaktivuje input i akce.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~Game_LastLifeLost_ShowsGameOverAndDisablesInput|FullyQualifiedName~Game_LowLives_ShowsWarningAndRegenTimer"` probehl uspesne pro game-over scenar v behu 2/2; nasledne cely `GameFlowE2ETests` probehl uspesne: 22/22.

### E2E-BUG-0046: WaitForNoBusyIndicators pada na strict mode pri vice loading elementech

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** E2E infra / Stabilita testu
- **Nalezeno v testu:** `GameFlowE2ETests.Game_TimeAttackDifficulty_StartsWithExpectedLives`, `GameFlowE2ETests.Game_LastLifeLost_ShowsGameOverAndDisablesInput`, `GameFlowE2ETests.Game_LowLives_ShowsWarningAndRegenTimer`
- **Screenshot/trace:** `artifacts/e2e/failures/game/time-attack-lives-beginner/`, `artifacts/e2e/failures/game/last-life-game-over/`, `artifacts/e2e/failures/game/low-lives-warning-regen/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Otevrit `/game/{sessionId}` ve scenari, kde stranka zobrazi `.loading-state` s vnorenym `.spinner`.
  2. `GoToAndWaitForAppReadyAsync` zavola `WaitForNoBusyIndicatorsAsync`.
  3. Playwright locator pro `.loading-state, .loading-container, .spinner, .loading-skeleton, [aria-busy='true']` najde vice prvku.
- **Ocekavani:** Helper pocka, az vsechny busy prvky zmizi nebo se skryji.
- **Skutecnost:** Helper spadne na Playwright strict mode violation driv, nez test dojde k vlastni aserci.
- **Pravdepodobna pricina:** `Locator.WaitForAsync(State=Hidden)` se pouziva nad multi-match locatorem.
- **Oprava:** `WaitForNoBusyIndicatorsAsync` pouziva DOM predicate pres `WaitForFunctionAsync`, ktery overi vsechny busy prvky bez strict multi-match locatoru.
- **Overeni:** `Game_TimeAttackDifficulty_StartsWithExpectedLives` probehl uspesne: 4/4; nasledne cely `GameFlowE2ETests` probehl uspesne: 22/22.

### E2E-BUG-0045: Herni arena nezobrazuje zivoty a training nema nekonecne zivoty

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Game / Lives / UX
- **Nalezeno v testu:** `GameFlowE2ETests.Game_TrainingMode_ShowsInfiniteLivesAndWrongAnswerDoesNotDecrease`, `GameFlowE2ETests.Game_TimeAttackWrongAnswer_DecreasesLives`
- **Screenshot/trace:** `artifacts/e2e/failures/game/training-infinite-lives/`, `artifacts/e2e/failures/game/time-attack-wrong-answer-loses-life/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Spustit training nebo TimeAttack.
  2. Hledat lives indikátor v herní aréně.
  3. V TimeAttack odeslat spatnou odpoved.
- **Ocekavani:** Training ukazuje `∞` a spatna odpoved ho nesnizi; TimeAttack ukazuje `5/5` a po spatne odpovedi `4/5`.
- **Skutecnost:** UI nema `game-lives` ani `game-lives-count`; server navic startuje training s beznymi 5 zivoty.
- **Pravdepodobna pricina:** `LivesIndicator` komponenta existuje, ale neni napojena do `GameArena`; `ScrambledWordDto` nenese max/infinite metadata a `GameSessionService` nerozlisuje training lives.
- **Oprava:** `ScrambledWordDto` nese metadata zivotu vcetne maxima a nekonecneho training rezimu, `GameSessionService` startuje training s nekonecnymi zivoty a bez ztraty zivota pri spatne odpovedi, timed rezimy ubiraji zivoty podle obtiznosti, `GameArena` zobrazuje `LivesIndicator` se stabilnimi `data-testid` selektory a Blazor testy registruji potrebny localizer.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GameFlowE2ETests"` probehl uspesne: 22/22 testu. `dotnet test tests/LexiQuest.Blazor.Tests/LexiQuest.Blazor.Tests.csproj --filter "FullyQualifiedName~GameArenaTests|FullyQualifiedName~LivesIndicatorTests|FullyQualifiedName~GamePageTests|FullyQualifiedName~GameServiceTests"` probehl uspesne: 49/49. `dotnet test tests/LexiQuest.Infrastructure.Tests/LexiQuest.Infrastructure.Tests.csproj --filter "FullyQualifiedName~GameSessionServiceTests"` probehl uspesne: 20/20.

### E2E-BUG-0044: Klient po spatne/skip odpovedi obnovuje offline seed a generuje 404 console error

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Game / PWA offline cache / Console errors
- **Nalezeno v testu:** `GameFlowE2ETests.Game_WrongAnswer_ShowsCorrectAnswerAndStaysOnRound`, `GameFlowE2ETests.Game_SkipTreatsRoundAsWrongAndShowsCorrectAnswer`, `GameFlowE2ETests.Game_DiacriticsMustMatch`
- **Screenshot/trace:** `artifacts/e2e/failures/game/wrong-answer-feedback/20260619-093040-console.log`, `artifacts/e2e/failures/game/skip-round-feedback/20260619-093056-console.log`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Spustit training hru.
  2. Odeslat spatnou odpoved nebo skip.
  3. Zkontrolovat console errors.
- **Ocekavani:** UI ukaze spatnou odpoved bez browser console erroru a bez zbytecneho 404 requestu.
- **Skutecnost:** Po odpovedi klient vola `/offline-training-seed`, ale server po dokoncenem spatnem kole nema aktivni nove kolo a vraci 404; browser to zapise jako console error.
- **Pravdepodobna pricina:** `GameService.SubmitAnswerAsync` obnovuje offline seed po kazde odpovedi, ne jen po spravne odpovedi s dalsim kolem.
- **Oprava:** `GameService.SubmitAnswerAsync` obnovuje offline training seed pouze po spravne odpovedi, ktera skutecne vraci dalsi kolo.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GameFlowE2ETests"` probehl uspesne: 22/22 testu.

### E2E-BUG-0043: Session jineho uzivatele je dostupna pres URL

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Game / Authorization / Data isolation
- **Nalezeno v testu:** `GameFlowE2ETests.Game_OtherUsersSession_IsNotAccessible`
- **Screenshot/trace:** `artifacts/e2e/failures/game/other-user-session-forbidden/20260619-092953.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Uzivatel A spusti training a ziska URL `/game/{sessionId}`.
  2. Uzivatel B se prihlasi v tomtez browser contextu.
  3. Uzivatel B otevre URL session uzivatele A.
- **Ocekavani:** API vrati `404/403` a UI zobrazi error state; herni arena cizi session se nezobrazi.
- **Skutecnost:** Arena cizi session se nacte.
- **Pravdepodobna pricina:** `GET /api/v1/game/{id}` kontroluje jen existenci session, ale neposila do service aktualni `userId`.
- **Oprava:** `GET /api/v1/game/{id}` nacita aktualni `userId` z tokenu a `GetSessionStateAsync` filtruje session podle vlastnika.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GameFlowE2ETests"` probehl uspesne: 22/22 testu.

### E2E-BUG-0042: Spravna odpoved neprejde na dalsi kolo

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Game / Session progression
- **Nalezeno v testu:** `GameFlowE2ETests.Game_CorrectAnswer_IncreasesXpComboAndMovesToNextRound`
- **Screenshot/trace:** `artifacts/e2e/failures/game/correct-answer-next-round/20260619-093032.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit uzivatele a spustit training.
  2. Odeslat spravnou odpoved.
  3. Cekat na `Kolo 2`.
- **Ocekavani:** UI ukaze spravny feedback, XP/combo a posune session na dalsi kolo.
- **Skutecnost:** Feedback se zobrazi, ale `Kolo 2` neprijde.
- **Pravdepodobna pricina:** Seznam word IDs v `GameSession` je ulozen jen v private fieldu `_wordIds`, ktery se po novem API requestu neobnovi z DB.
- **Oprava:** `GenerateNextRoundAsync` pri chybejicim nepersistovanem `_wordIds` vybere dalsi slovo z repozitare podle obtiznosti misto ukonceni hry po prvnim kole.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GameFlowE2ETests"` probehl uspesne: 22/22 testu.

### E2E-BUG-0041: Offline training neuklada seed data a nema replay queue pro odpovedi

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** PWA / Offline / Game
- **Nalezeno v testu:** `PwaE2ETests.Pwa_OfflineTraining_UsesCachedSeedAndReplaysQueuedAnswer`
- **Screenshot/trace:** `artifacts/e2e/failures/pwa/offline-training-queue/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit uzivatele a online spustit training hru.
  2. Pockat na cache offline seed dat v `localStorage`.
  3. Prepnut prohlizec offline a znovu spustit training.
  4. Odeslat odpoved a po navratu online cekat na replay fronty.
- **Ocekavani:** Online training ulozi seed pro offline training; offline training startuje jen z povolene cache, odpoved se ulozi do fronty a po navratu online se fronta prehraje.
- **Skutecnost:** `lexiquest_offline_training_seed` nikdy nevznikne, protoze klient nema offline seed endpoint ani localStorage cache/replay logiku.
- **Pravdepodobna pricina:** PWA offline cast byla deklarovana v todo, ale GameService dela pouze prime HTTP volani.
- **Oprava:** Doplněn autorizovaný endpoint offline training seed pro aktuální training session, klient ukládá seed do `localStorage`, offline training startuje z cache, offline odpovědi se ukládají do `lexiquest_offline_game_queue` a po `online` eventu se přehrají.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PwaE2ETests"` probehl uspesne: 4/4 testy.

### E2E-BUG-0040: PWA install prompt neni dostupny na landing strance a chybi stabilni PWA selektory

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** PWA / Public web / UX / Testability
- **Nalezeno v testu:** `PwaE2ETests.Pwa_InstallPrompt_CanBeAcceptedAndDismissed`, `PwaE2ETests.Pwa_OfflineBanner_AppearsAndDisappearsWithConnectivity`
- **Screenshot/trace:** `artifacts/e2e/failures/pwa/install-prompt/20260619-090753.png`, `artifacts/e2e/failures/pwa/offline-banner/20260619-090812.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Otevrit landing page `/`.
  2. V prohlizeci vyvolat `beforeinstallprompt`.
  3. Zkontrolovat viditelnost install promptu.
  4. Prihlaseneho uzivatele prepnout offline a zkontrolovat offline banner.
- **Ocekavani:** Install prompt je dostupny i na public landing page; install prompt a offline banner maji stabilni `data-testid` selektory pro E2E a screenshot kontroly.
- **Skutecnost:** Install prompt je vlozen pouze v `MainLayout`, takze anonymni landing ho nezobrazi; offline banner sice viditelny je, ale nema stabilni `data-testid`.
- **Pravdepodobna pricina:** PWA komponenty byly pripojene pouze k prihlasenemu layoutu a bez testovacich selektoru.
- **Oprava:** `InstallPrompt` a `OfflineBanner` maji stabilni `data-testid`; landing layout zobrazuje PWA offline banner i install prompt, stejne jako prihlaseny layout.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PwaE2ETests"` probehl uspesne: 3/3 testy.

### E2E-BUG-0039: Manifest a service worker odkazuji na neexistujici PWA assety

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** PWA / Static assets / Offline
- **Nalezeno v testu:** `PwaE2ETests.Pwa_ManifestAndServiceWorker_AreInstallable`
- **Screenshot/trace:** `artifacts/e2e/failures/pwa/manifest-service-worker/20260619-090758.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Otevrit landing page `/`.
  2. Nacist `/manifest.json` a vsechny manifest ikony.
  3. Pockat na `navigator.serviceWorker.ready`.
- **Ocekavani:** Manifest vraci validni metadata vcetne 192x192 a 512x512 ikon; service worker se nainstaluje a aktivuje bez 404.
- **Skutecnost:** Prohlizec hlasi 404 pri PWA assetech; `icon-512.png` chybi a `service-worker.js` precachuje neexistujici `/index.html` a spatny CSS bundle.
- **Pravdepodobna pricina:** PWA asset manifest nebyl zarovnan s Blazor Web App vystupem.
- **Oprava:** Doplnena realna `icon-512.png`; `service-worker.js` precachuje existujici Blazor CSS bundle a uz neodkazuje na neexistujici `/index.html`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~PwaE2ETests"` probehl uspesne: 3/3 testy.

### E2E-BUG-0038: Footer public odkazy konci na loginu misto verejnych stranek

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Public web / Routing / Lokalizace / UX
- **Nalezeno v testu:** `LandingE2ETests.LandingPage_FooterLink_NavigatesToExpectedRoute`
- **Screenshot/trace:** `artifacts/e2e/failures/landing/footer-footer-about/20260619-085845.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Otevrit landing page `/`.
  2. Kliknout ve footeru na `O nás`.
  3. Pockat na navigaci na `/about`.
- **Ocekavani:** Footer odkazy `/about`, `/terms`, `/privacy` a `/contact` zustanou verejne dostupne, zobrazi ceske nadpisy a nevypisuji lokalizacni klice.
- **Skutecnost:** `/about` se vyrenderuje pres prihlaseny `MainLayout`, ten anonymniho uzivatele presmeruje na `/login`; stranky navic nemaji vlastni `.resx` soubory.
- **Pravdepodobna pricina:** Public informacni stranky nemaji explicitni `LandingLayout` a chybi resources pro `IStringLocalizer<About|Terms|Privacy|Contact>`.
- **Oprava:** Stranky `/about`, `/terms`, `/privacy` a `/contact` dostaly explicitni `LandingLayout`; doplneny byly ceske `.resx` resources pro vsechny pouzite lokalizacni klice.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~LandingE2ETests.LandingPage_FooterLink_NavigatesToExpectedRoute"` probehl uspesne: 4/4 testy.

### E2E-BUG-0037: Neexistujici public URL vraci prazdnou 404 misto lokalizovane NotFound stranky

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Routing / Error pages / UX
- **Nalezeno v testu:** `LandingE2ETests.UnknownPublicRoute_ShowsLocalizedNotFoundPage`
- **Screenshot/trace:** `artifacts/e2e/failures/routing/not-found/20260619-085313.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Bez prihlaseni otevrit libovolnou neexistujici URL, napr. `/neexistujici-stranka-*`.
  2. Pockat na aplikaci.
- **Ocekavani:** Zobrazi se lokalizovana ceska NotFound stranka s nadpisem `Stránka nenalezena` a odkazem zpet na uvod.
- **Skutecnost:** Browser dostane prazdnou 404 stranku; Blazor NotFound fallback v `Routes.razor` se pri primem requestu nevyrenderuje.
- **Pravdepodobna pricina:** Chybi routovana catch-all NotFound komponenta pro public URL; inline `<NotFound>` fallback navic pouziva `MainLayout`.
- **Oprava:** Pridat routovanou catch-all `NotFound.razor` s public `LandingLayout` a sdilenou `NotFoundContent` komponentu pouzitou i v router fallbacku.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~LandingE2ETests.UnknownPublicRoute_ShowsLocalizedNotFoundPage"` probehl uspesne: 1/1 test.

### E2E-BUG-0036: Guest limit reset po 24h nema deterministicky E2E time hook

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Guest / Limit / E2E infra
- **Nalezeno v testu:** `GuestE2ETests.GuestLimit_After24Hours_AllowsNewGameAgain`
- **Screenshot/trace:** `artifacts/e2e/failures/guest/daily-limit-reset-after-24h/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Pres realne API vycerpat 5 guest her.
  2. Overit, ze dalsi start vrati `429 Too Many Requests`.
  3. Pokusit se v E2E posunout cas o 24 hodin pres `/api/v1/e2e/time/advance`.
- **Ocekavani:** E2E prostredi umi deterministicky posunout cas, aby slo overit reset denniho limitu bez cekani 24 hodin.
- **Skutecnost:** API vraci `404 Not Found`; `GuestLimiter` pouziva `DateTime.UtcNow` primo.
- **Pravdepodobna pricina:** Chybi injektovatelny `TimeProvider` a E2E-only endpoint pro posun/reset casu.
- **Oprava:** `GuestLimiter` pouziva injektovany `TimeProvider`; E2E prostredi registruje `AdjustableTimeProvider` a endpointy `/api/v1/e2e/time/advance` a `/api/v1/e2e/state/reset`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GuestE2ETests.GuestLimit_After24Hours_AllowsNewGameAgain"` probehl uspesne: 1/1 test.

### E2E-BUG-0035: Guest daily limit zobrazuje prihlasovaci text misto registracni CTA

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Guest / Limit / UX
- **Nalezeno v testu:** `GuestE2ETests.GuestLimit_FifthGameAllowedAndSixthGameShowsRegistrationCta`
- **Screenshot/trace:** `artifacts/e2e/failures/guest/daily-limit-registration-cta/20260619-083710.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Pres realne API spustit 5 guest her ze stejneho klienta.
  2. Overit, ze 6. start vrati `429 Too Many Requests`.
  3. Otevrit `/play`, kliknout `Začít hrát` a zkontrolovat limitni stav.
- **Ocekavani:** Limitni stav ma primarni CTA `Zaregistrovat se`, ktera vede na `/register`.
- **Skutecnost:** Limitni karta sice rika, ze registrace odemkne neomezeny pristup, ale primarni tlacitko ma text `Přihlásit se`.
- **Pravdepodobna pricina:** `GuestGame.razor` pouziva resource `Login` i pro registracni limitni CTA.
- **Oprava:** V limitnim stavu pouzit lokalizovany registracni text `RegisterNow`; navigace na `/register` zustala stejna.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GuestE2ETests.GuestLimit_FifthGameAllowedAndSixthGameShowsRegistrationCta"` probehl uspesne: 1/1 test.

### E2E-BUG-0034: Guest conversion neulozi progress a registrace ho neprenese do uctu

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Guest / Registrace / Progress transfer
- **Nalezeno v testu:** `GuestE2ETests.GuestConversion_RegisterFromCta_TransfersProgressToDashboard`
- **Screenshot/trace:** `artifacts/e2e/failures/guest/conversion-transfers-progress/20260619-082510.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Otevrit `/play` jako anonymni navstevnik.
  2. Spustit guest hru a spravne vyresit vsech 5 slov.
  3. V konverznim modalu kliknout na primarni CTA pro registraci.
- **Ocekavani:** UI zavola guest convert flow, ulozi progress do `localStorage`, registrace zobrazi cesky banner s XP/slovy a po vytvoreni uctu dashboard ukaze prenesene XP/statistiky.
- **Skutecnost:** UI naviguje na beznou `/register` stranku bez progress banneru, bez `guest_progress` v `localStorage` a bez backendoveho transfer tokenu.
- **Pravdepodobna pricina:** `NavigateToRegisterWithProgress` jen naviguje na `/register?guestSession=...`; Blazor nema `ConvertAsync`, registrace necte guest progress a `RegisterRequest` neumi predat transfer do `UserService`.
- **Oprava:** Doplnit jednorazovy guest transfer token, endpoint `GuestGameService.ConvertAsync`, ulozeni `guest_progress` do localStorage, banner na registraci a spotrebovani tokenu v `UserService.RegisterAsync`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GuestE2ETests.GuestConversion_RegisterFromCta_TransfersProgressToDashboard"` probehl uspesne: 1/1 test.

### E2E-BUG-0033: Guest route `/play` po auth guardu presmeruje anonymniho hrace na login

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Guest / Routing / Auth guard
- **Nalezeno v testu:** `GuestE2ETests.GuestPlay_LoadsWithoutAccount_AndStoresWelcomeScreenshot`
- **Screenshot/trace:** `artifacts/e2e/failures/guest/welcome/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Bez tokenu otevrit `/play`.
  2. Pockat na app ready.
- **Ocekavani:** Guest route je verejna a zobrazi guest welcome obrazovku.
- **Skutecnost:** `MainLayout` auth guard presmeruje anonymniho navstevnika na `/login`.
- **Pravdepodobna pricina:** `GuestGame.razor` pouziva protected `MainLayout`, ktery je urceny pro prihlasenou cast aplikace.
- **Oprava:** Prepnout guest page na public `LandingLayout`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GuestE2ETests.GuestPlay_LoadsWithoutAccount"` probehl uspesne: 1/1 test.

### E2E-BUG-0032: Registrace neposila welcome email do smtp4dev

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Auth / Registration / Email
- **Nalezeno v testu:** `EmailE2ETests.Register_NewUser_SendsCzechWelcomeEmailToSmtp4Dev`
- **Screenshot/trace:** `artifacts/e2e/failures/email/register-welcome-email/`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Registrovat noveho uzivatele pres `/register`.
  2. Pockat na dashboard.
  3. Cist zpravy ze smtp4dev.
- **Ocekavani:** Do smtp4dev dorazi cesky welcome email se subjectem `Vítej v LexiQuestu!`.
- **Skutecnost:** smtp4dev vraci prazdny seznam zprav; registracni flow welcome email neodesila.
- **Pravdepodobna pricina:** `EmailService.SendWelcomeEmailAsync` existuje, ale `UserService.RegisterAsync` ho nevola.
- **Oprava:** Po uspesnem ulozeni noveho uzivatele odeslat welcome email pres `IEmailService`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~EmailE2ETests.Register_NewUser_SendsCzechWelcomeEmailToSmtp4Dev"` probehl uspesne: 1/1 test.

### E2E-BUG-0031: Dlouhy email pri registraci projde validaci a spadne az na DB truncation

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Auth / Registration / Validation
- **Nalezeno v testu:** `AuthE2ETests.Register_EmailValidationEdgeCases_ShowSpecificLocalizedMessage`
- **Screenshot/trace:** `artifacts/e2e/failures/auth/register-email-validation-email-m-e-m-t-maxim-ln-256-znak/20260619-080300.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Otevrit `/register`.
  2. Zadat strukturou validni email delsi nez 256 znaku.
  3. Odeslat formular.
- **Ocekavani:** UI zobrazi lokalizovanou validaci `Email může mít maximálně 256 znaků`.
- **Skutecnost:** Email pole je oznacene jako validni, API zkusi ulozit uzivatele a SQL Server vrati truncation chybu pro sloupec `Users.Email`; UI ukaze obecnou chybu registrace.
- **Pravdepodobna pricina:** Frontend ani backend validator nema max length pravidlo pro email, i kdyz EF konfigurace limituje email na 256 znaku.
- **Oprava:** Doplnit `MaximumLength(256)` a lokalizovany text do Blazor i Core/API registracni validace.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~AuthE2ETests.Register_EmailValidationEdgeCases"` probehl uspesne: 3/3 testy.

### E2E-BUG-0030: Chybi refresh token endpoint a neplatny refresh neodhlasi UI

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Auth / Session / Token refresh
- **Nalezeno v testech:** `AuthE2ETests.Session_ExpiredAccessToken_RefreshesWithRefreshTokenAndLoadsDashboard`, `AuthE2ETests.Session_InvalidRefreshToken_LogsOutAndClearsStoredTokens`
- **Screenshot/trace:** `artifacts/e2e/failures/auth/session-refresh-expired-access-token/20260619-075404.png`, `artifacts/e2e/failures/auth/session-invalid-refresh-token/20260619-075347.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit uzivatele a ulozit do browseru prosly access token.
  2. Nechat platny refresh token, nebo ho nahradit neplatnou hodnotou.
  3. Otevrit `/dashboard`.
- **Ocekavani:** Platny refresh token obnovi session a dashboard nacte statistiky; neplatny refresh token smaze ulozene tokeny a presmeruje na login.
- **Skutecnost:** API vraci `404 Not Found` pro `POST /api/v1/users/refresh`; dashboard skonci v error stavu a pri neplatnem refreshi zustane na protected route.
- **Pravdepodobna pricina:** Login/register vraci refresh token, ale token se neuklada do DB a API nema refresh endpoint; Blazor stats flow po neuspesnem refreshi nenaviguje na login.
- **Oprava:** Doplnit ulozeni/rotaci refresh tokenu, endpoint `POST /api/v1/users/refresh` a klientsky logout/navigaci pri neobnovitelne `401`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~AuthE2ETests.Session_"` probehl uspesne: 2/2 testy.

### E2E-BUG-0029: Dashboard po loginu nenacte statistiky kvuli auth handleru mimo Blazor scope

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Auth / Dashboard / Interactive rendering
- **Nalezeno v testu:** `AuthE2ETests.Login_RememberMe_StaysAuthenticatedAfterDashboardReload`
- **Screenshot/trace:** `artifacts/e2e/failures/auth/login-remember-me-reload/20260619-074306.png`, `artifacts/e2e/traces/auth-login-remember-me-reload-20260619-074306.zip`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit uzivatele pres `/login`.
  2. Po navigaci na `/dashboard` cekat na statistiky.
  3. Dashboard zobrazi error stav misto stat karet.
- **Ocekavani:** Dashboard po loginu i po reloadu zustane prihlaseny a zobrazi `Celkové XP`.
- **Skutecnost:** `ApiClient` zacne zpracovavat `GET /api/v1/stats/user`, ale request se neodesle; dashboard zachyti vyjimku a zobrazi `Nepodařilo se načíst data`.
- **Pravdepodobna pricina:** Protected layout/dashboard nacitaji auth/data prilis brzo a autorizacni `HttpClientFactory` handler bezi mimo bezny Blazor component/circuit scope, takze nedokaze spolehlive cist token z `localStorage` pres `IJSRuntime`.
- **Oprava:** Presunout auth gate v `MainLayout` a nacitani statistik v `Dashboard` do `OnAfterRenderAsync(firstRender)`; `StatsService` pouziva scoped `AuthService`, nastavuje Bearer header primo a pri `401` zkusi refresh.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~AuthE2ETests.Login_RememberMe_StaysAuthenticatedAfterDashboardReload"` probehl uspesne: 1/1 test.

### E2E-BUG-0028: Topbar nema funkcni pristupne odhlaseni a protected layout nepřesměruje bez tokenu

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Auth / Logout / Protected routes / UX
- **Nalezeno v testu:** `AuthE2ETests.Logout_FromTopBarClearsTokensAndProtectedRouteReturnsToLogin`
- **Screenshot/trace:** `artifacts/e2e/failures/auth/logout-clears-session/20260619-073639.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit uzivatele a otevrit `/dashboard`.
  2. Pokusit se najit/clicknout topbar tlacitko `Odhlásit se`.
  3. Po odhlaseni zkusit otevrit `/dashboard` bez tokenu.
- **Ocekavani:** Topbar ma pristupne odhlaseni, logout smaze tokeny a protected route vrati uzivatele na `/login`.
- **Skutecnost:** Logout tlacitko neni dostupne podle role/name; layout jen navigoval na login a nemazal tokeny.
- **Pravdepodobna pricina:** `MainLayout` nepouziva `AuthService.LogoutAsync` a nema explicitni lokalizovane logout tlacitko; protected layout nema guard pro chybejici token.
- **Oprava:** Doplnit icon-only logout button s `aria-label`, volat `AuthService.LogoutAsync` a v `MainLayout.OnAfterRenderAsync` presmerovat anonymniho uzivatele na login po prvnim renderu.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~AuthE2ETests.Logout_FromTopBar"` probehl uspesne: 1/1 test.

### E2E-BUG-0027: Login lockout se v UI zobrazi jako obecne selhani prihlaseni

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Auth / Login / Lockout / Error handling
- **Nalezeno v testu:** `AuthE2ETests.Login_FiveWrongAttempts_LocksAccountAndShowsLocalizedLockout`
- **Screenshot/trace:** `artifacts/e2e/failures/auth/login-lockout/20260619-073241.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Registrovat uzivatele.
  2. Pětkrat zadat spatne heslo.
  3. Zkusit se prihlasit spravnym heslem.
- **Ocekavani:** UI zobrazi cesky lockout stav `Účet je dočasně zablokován. Zkuste to znovu za 15 minut.`
- **Skutecnost:** API vrati 423, ale UI zobrazi obecnou hlasku `Přihlášení se nezdařilo. Zkuste to prosím později.`
- **Pravdepodobna pricina:** `AuthService.LoginAsync` nerozlisuje HTTP 423 a necte `ProblemDetails.detail`.
- **Oprava:** Doplnit mapovani `HttpStatusCode.Locked` na detail z error odpovedi nebo lokalizovany fallback.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~AuthE2ETests.Login_FiveWrongAttempts"` probehl uspesne: 1/1 test.

### E2E-BUG-0026: Password reset confirm pouziva app layout se sidebar navigaci

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Auth / Password reset / UX layout
- **Nalezeno v testu:** `EmailE2ETests.PasswordReset_NewPasswordSameAsOld_ShowsSpecificLocalizedError`
- **Screenshot/trace:** `artifacts/e2e/failures/email/password-reset-same-as-old/20260619-072341.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Otevrit `/password-reset/{token}`.
  2. Zkontrolovat screenshot confirm stranky.
- **Ocekavani:** Reset hesla je auth flow bez prihlasene sidebar navigace, stejne jako login/register.
- **Skutecnost:** Stranka zobrazuje hlavni app layout se sidebar odkazy `Dashboard`, `Hra`, `Cesty`, ...
- **Pravdepodobna pricina:** `PasswordResetRequest` a `PasswordResetConfirm` nemaji explicitni `LandingLayout`.
- **Oprava:** Doplnit `@layout LandingLayout` na password reset request i confirm stranku.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~EmailE2ETests.PasswordReset_UsedToken|FullyQualifiedName~EmailE2ETests.PasswordReset_InvalidToken|FullyQualifiedName~EmailE2ETests.PasswordReset_NewPasswordSameAsOld|FullyQualifiedName~AuthE2ETests.AuthPages_RenderFocusedLayout"` probehl uspesne: 7/7 testu.

### E2E-BUG-0025: Password reset UI nerozlisuje pouzity token a stejne nove heslo

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Auth / Password reset / Error handling / Lokalizace
- **Nalezeno v testech:** `EmailE2ETests.PasswordReset_UsedToken_ShowsLocalizedError`, `EmailE2ETests.PasswordReset_NewPasswordSameAsOld_ShowsSpecificLocalizedError`
- **Screenshot/trace:** `artifacts/e2e/failures/email/password-reset-used-token/20260619-072359.png`, `artifacts/e2e/failures/email/password-reset-same-as-old/20260619-072341.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Pouzit reset token podruhe, nebo zadat nove heslo shodne se starym.
  2. Odeslat confirm formular.
- **Ocekavani:** UI zobrazi konkretni ceskou hlasku `Odkaz pro obnovení hesla již byl použit.` nebo `Nové heslo nesmí být stejné jako staré.`
- **Skutecnost:** UI zobrazi obecnou hlasku `Odkaz pro obnovení hesla je neplatný nebo vypršel.`
- **Pravdepodobna pricina:** `PasswordResetService` nema resource soubor pro typed localizer a Blazor `AuthService` cte jen pole `message`, ne standardni `ProblemDetails.detail`.
- **Oprava:** Doplnit Core resource pro `PasswordResetService` a v Blazor `AuthService` pouzivat `detail/message/error/title` z error odpovedi.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~EmailE2ETests.PasswordReset_UsedToken|FullyQualifiedName~EmailE2ETests.PasswordReset_InvalidToken|FullyQualifiedName~EmailE2ETests.PasswordReset_NewPasswordSameAsOld|FullyQualifiedName~AuthE2ETests.AuthPages_RenderFocusedLayout"` probehl uspesne: 7/7 testu.

### E2E-BUG-0024: Dashboard pada pri renderu `TmStatCard` kvuli neexistujicimu parametru `Icon`

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Dashboard / Tempo.Blazor / Runtime render
- **Nalezeno v testu:** `EmailE2ETests.PasswordReset_LinkChangesPasswordAndOldPasswordStopsWorking`
- **Screenshot/trace:** `artifacts/e2e/failures/email/password-reset-complete-flow/20260619-071724.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit se a otevrit `/dashboard`.
  2. Nechat nacist statistiky.
  3. Sledovat Blazor console/error boundary.
- **Ocekavani:** Stat karty se vyrenderuji bez runtime vyjimky.
- **Skutecnost:** Blazor hlasi `TmStatCard does not have a property matching the name 'Icon'`.
- **Pravdepodobna pricina:** Dashboard a admin dashboard pouzivaly starsi/neplatny API povrch `TmStatCard`.
- **Oprava:** Odstranit parametr `Icon` z `TmStatCard` na dashboardu i admin dashboardu podle skutecnych public parametru komponenty.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~EmailE2ETests.PasswordReset_LinkChangesPassword"` probehl uspesne: 1/1 test.

### E2E-BUG-0023: Dashboard po loginu vola chybejici `/api/v1/stats/user`

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Dashboard / API / Authenticated flow
- **Nalezeno v testu:** `EmailE2ETests.PasswordReset_LinkChangesPasswordAndOldPasswordStopsWorking`
- **Screenshot/trace:** `artifacts/e2e/screenshots/email/password-reset-complete-flow/1366x900/light/dashboard-after-reset.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Dokoncit reset hesla.
  2. Prihlasit se novym heslem.
  3. Pockat na dashboard a zkontrolovat konzoli/API logy.
- **Ocekavani:** Dashboard nacte statistiky prihlaseneho uzivatele bez console erroru a bez error empty state.
- **Skutecnost:** API vraci `404 Not Found` pro `GET /api/v1/stats/user`, dashboard zobrazi `Nepodařilo se načíst data` a browser zaloguje failed resource.
- **Pravdepodobna pricina:** Blazor `StatsService` vola endpoint, ktery v API nebyl implementovan.
- **Oprava:** Pridat shared `UserStatsSummaryDto`, autorizovany `StatsController` a napojit Blazor `IStatsService` na shared DTO.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~EmailE2ETests.PasswordReset_LinkChangesPassword"` probehl uspesne: 1/1 test.

### E2E-BUG-0022: Register validace zobrazuje `{0}` misto konkretni delky

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Auth / Registrace / Lokalizace validace
- **Nalezeno v testu:** `AuthE2ETests.Register_InvalidInputs_ShowLocalizedValidationMessagesWithoutRawKeys`
- **Screenshot/trace:** `artifacts/e2e/failures/auth/register-validation-errors/20260619-070555.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Otevrit `/register`.
  2. Vyplnit kratke uzivatelske jmeno a kratke heslo.
  3. Odeslat formular.
- **Ocekavani:** Validacni hlasky obsahuji konkretni limity, napr. `alespoň 3 znaky` a `alespoň 8 znaků`.
- **Skutecnost:** UI zobrazilo `Uživatelské jméno musí mít alespoň {0} znaky` a `Heslo musí mít alespoň {0} znaků`.
- **Pravdepodobna pricina:** `RegisterModelValidator` predaval lokalizovanou sablonu do `WithMessage` bez formatovacich argumentu.
- **Oprava:** `RegisterModelValidator` predava do lokalizovanych hlasek konkretni hodnoty limitu 3, 30 a 8.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~AuthE2ETests.Register_InvalidInputs|FullyQualifiedName~AuthE2ETests.Register_FieldValidationEdgeCases"` probehl uspesne: 8/8 testu.

### E2E-BUG-0021: Register formulář zobrazuje anglickou native browser validaci místo českých FluentValidation hlášek

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Auth / Registrace / Validace / Lokalizace
- **Nalezeno v testu:** `AuthE2ETests.Register_InvalidInputs_ShowLocalizedValidationMessagesWithoutRawKeys`
- **Screenshot/trace:** `artifacts/e2e/failures/auth/register-validation-errors/20260619-070046.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Otevrit `/register`.
  2. Zadat nevalidni email bez `@` a dalsi nevalidni pole.
  3. Odeslat formular.
- **Ocekavani:** Formular zobrazi ceske aplikacni validacni hlasky z FluentValidation/resources.
- **Skutecnost:** Browser zobrazi native anglickou bublinu `Please include an '@' in the email address...` a aplikacni validace se nespusti.
- **Pravdepodobna pricina:** `EditForm` neobsahuje `novalidate`, tak HTML5 constraint validation blokuje submit pred Blazor validaci.
- **Oprava:** Doplnit `novalidate` na auth/reset/contact/multiplayer formulare a pridat resource soubory pro typove Blazor validatory pod `Resources/Validators`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~AuthE2ETests.Register_InvalidInputs|FullyQualifiedName~AuthE2ETests.Register_FieldValidationEdgeCases"` probehl uspesne: 8/8 testu.
- **Poznamky:** Stejny pattern je potreba hlidat i u login/password-reset formularu.

### E2E-BUG-0020: E2E databaze se vytvari pres EnsureCreated a nepres EF migrace

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** E2E infrastruktura / Databaze / Migrace
- **Nalezeno v testu:** `InfrastructureE2ETests.E2EEnvironment_AppliesEfMigrationsAndReadyHealth`
- **Screenshot/trace:** `artifacts/e2e/logs`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer
- **Reprodukce:**
  1. Spustit `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~InfrastructureE2ETests.E2EEnvironment_AppliesEfMigrationsAndReadyHealth"`.
  2. Test se pripoji do SQL Server Testcontaineru.
  3. Test se pokusi precist `__EFMigrationsHistory`.
- **Ocekavani:** E2E DB vznikne aplikovanim EF migraci a obsahuje historii migraci i tabulky aktualniho modelu.
- **Skutecnost:** SQL Server vraci `Invalid object name '__EFMigrationsHistory'`.
- **Pravdepodobna pricina:** `E2ETestDataSeeder.EnsureDatabaseAsync` pouziva `EnsureCreatedAsync`, protoze historicky migration snapshot nepokryval nove domenove tabulky.
- **Oprava:** Doplněna migrace `AddPhase9DomainTables`, E2E seed přepnut z `EnsureCreatedAsync` na `MigrateAsync` a Respawn reset ignoruje `__EFMigrationsHistory`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~InfrastructureE2ETests"` probehl uspesne: 2/2 testy.
- **Poznamky:** Tohle blokuje vernejsi E2E prostredi a muze maskovat problemy v produkcnich migracich.

### E2E-BUG-0019: Password reset email pouziva fallback resource klice misto ceskeho subjectu a obsahu

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Email / Password reset / Lokalizace
- **Nalezeno v testu:** `EmailE2ETests.PasswordReset_ExistingUser_SendsCzechResetEmailToSmtp4Dev`
- **Screenshot/trace:** `artifacts/e2e/screenshots/email/password-reset-existing-user/1366x900/light/success.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Registrovat testovaciho uzivatele.
  2. Otevrit `/password-reset`, zadat jeho email a odeslat zadost.
  3. Precist zpravu zachycenou ve smtp4dev.
- **Ocekavani:** Email ma cesky subject `Obnovení hesla - LexiQuest`, ceske HTML/plaintext telo, spravny reset odkaz a odesilatele `noreply@lexiquest.test`.
- **Skutecnost:** Prvni E2E beh zachytil anglicky subject `LexiQuest - Password Reset`; po prepnuti na typed localizer se projevilo dalsi spatne mapovani resources jako subject `Subject` a telo `Greeting`, `Body`, `Action`.
- **Pravdepodobna pricina:** SMTP `EmailService` skládal emaily ručne anglicky a nasledne typed localizer v Infrastructure assembly nenasel vlozene resource soubory spolehlive.
- **Oprava:** EmailService pouziva embedded `.resx` pres explicitni `ResourceManager`, posila HTML i plaintext telo a preskakuje SMTP autentizaci, pokud nejsou credentials nastavene pro smtp4dev.
- **Overeni:** `dotnet build src/LexiQuest.Api/LexiQuest.Api.csproj && dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~EmailE2ETests"` probehl uspesne: 1/1 test.
- **Poznamky:** Test cte smtp4dev summary, detail a body endpointy, aby overil subject i realny obsah zpravy.

### E2E-BUG-0018: Mobile landing ma slepene footer odkazy, rusivy focus outline a prazdny feature placeholder

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Landing / Responsive / UX
- **Nalezeno v testu:** Screenshot review po `ResponsiveE2ETests.LandingPage_Viewport_HasNoHorizontalOverflow`
- **Screenshot/trace:** `artifacts/e2e/screenshots/responsive/landing/375x812/light/loaded.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, mobile 375x812
- **Reprodukce:**
  1. Spustit responsive E2E sadu.
  2. Otevrit mobile landing screenshot.
  3. Zkontrolovat hero, features a footer.
- **Ocekavani:** Programove focusovany neinteraktivni nadpis nema rusivy ramecek, feature placeholder ma viditelny obsah a footer odkazy maji citelne rozestupy.
- **Skutecnost:** Hero nadpis ma modry focus outline, feature placeholder je bily prazdny blok a footer odkazy se slepuji.
- **Pravdepodobna pricina:** CSS pouziva Tempo tokeny bez fallbacku; focus outline zustava na h1 po `FocusOnNavigate`.
- **Oprava:** Doplneny CSS fallbacky pro landing hero/footer/features a odstranen outline z neinteraktivniho hero nadpisu.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~ResponsiveE2ETests"` probehl uspesne: 18/18 testu. Screenshot `artifacts/e2e/screenshots/responsive/landing/375x812/light/loaded.png` zkontrolovan vizualne.
- **Poznamky:** Funkcni responsive test bez screenshot review by tento polish neoznacil.

### E2E-BUG-0017: Landing page na mobile screenshotu zobrazuje raw resource klice

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Landing / Lokalizace / UX / Responsive
- **Nalezeno v testu:** Screenshot review po `ResponsiveE2ETests.LandingPage_Viewport_HasNoHorizontalOverflow`
- **Screenshot/trace:** `artifacts/e2e/screenshots/responsive/landing/375x812/light/loaded.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, mobile 375x812
- **Reprodukce:**
  1. Spustit responsive E2E sadu.
  2. Otevrit mobile landing screenshot.
  3. Zkontrolovat texty hero, HowItWorks, Features, Paths, Testimonials, CTA a Footer.
- **Ocekavani:** Landing zobrazuje ceske texty z `Resources/Pages/Index.resx`.
- **Skutecnost:** UI zobrazuje klice jako `Hero.Tagline`, `HowItWorks.Title`, `CTA.Button`, `Footer.About`.
- **Pravdepodobna pricina:** Landing komponenty injectuji lokalizer pro vlastni typy komponent, ale sdilene texty jsou ulozene v `Resources/Pages/Index.resx`.
- **Oprava:** Zaveden marker `LexiQuest.Blazor.Pages.Index` a landing komponenty prepnute na `IStringLocalizer<Index>`.
- **Overeni:** `LandingE2ETests` probehly uspesne 8/8 a `ResponsiveE2ETests` 18/18; mobile landing screenshot uz zobrazuje ceske texty.
- **Poznamky:** Tohle je priklad, proc screenshot review neni jen duplicitni ke klasickym asertum.

### E2E-BUG-0016: Game arena zobrazuje literal `CurrentState.ScrambledWord` misto aktualniho slova

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Game / Blazor binding / UX
- **Nalezeno v testu:** Screenshot review po `GameFlowE2ETests.StartGame_LoggedInUser_CanStartTrainingSession`
- **Screenshot/trace:** `artifacts/e2e/screenshots/game/start-training/1366x900/light/active-game.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit uzivatele.
  2. Otevrit `/game` a spustit training.
  3. Zkontrolovat pismena v arene.
- **Ocekavani:** Arena zobrazi realne zamichane slovo ze session.
- **Skutecnost:** Arena sklada literal `CurrentState.ScrambledWord`.
- **Pravdepodobna pricina:** String parametr komponenty je predan jako `ScrambledWord="CurrentState.ScrambledWord"` bez `@`.
- **Oprava:** Binding zmenen na `ScrambledWord="@CurrentState.ScrambledWord"` a doplnen E2E assert proti literal textu.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GameFlowE2ETests"` probehl uspesne: 2/2 testy. Screenshot `artifacts/e2e/screenshots/game/start-training/1366x900/light/active-game.png` zkontrolovan vizualne.
- **Poznamky:** Funkcni test by bez screenshotu tenhle problem minul.

### E2E-BUG-0015: Game arena a timer vypisuji resource klice misto ceskych textu

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Game / Lokalizace / UX
- **Nalezeno v testu:** Screenshot review po `GameFlowE2ETests.StartGame_LoggedInUser_CanStartTrainingSession`
- **Screenshot/trace:** `artifacts/e2e/failures/game/start-training/20260619-055405.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit uzivatele.
  2. Otevrit `/game`, spustit training.
  3. Zkontrolovat game arena screenshot.
- **Ocekavani:** Game arena zobrazuje ceske texty `Zpet`, `Kolo 1`, `Napis slovo...`, `Potvrdit`, `Preskocit`, timer zobrazuje cesky popisek.
- **Skutecnost:** UI zobrazuje `Button_Back`, `Level_Name`, `Answer_Placeholder`, `Button_Submit`, `TimeRemaining`.
- **Pravdepodobna pricina:** `GameArena.razor` pouziva klice s podtrzitky, ale resx ma teckove klice; `GameTimer` nema odpovidajici resource file.
- **Oprava:** Sjednoceny klice v `GameArena.razor`, doplneny `GameTimer.resx` na namespace ceste komponent a pridany E2E asserty na ceske texty.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GameFlowE2ETests"` probehl uspesne: 2/2 testy.
- **Poznamky:** Screenshot test zde splnil druhou roli: funkcni asserty prosly az k arene, ale vizualni review zachytilo spatny stav.

### E2E-BUG-0014: WASM klient v E2E pouziva staticky `https://localhost:5000` misto dynamickeho API portu

- **Stav:** Overeno
- **Severity:** P0
- **Oblast:** Game / E2E konfigurace / Blazor WASM
- **Nalezeno v testu:** `GameFlowE2ETests.StartGame_LoggedInUser_CanStartTrainingSession`, `GameFlowE2ETests.Game_NonExistingSession_ShowsErrorState`
- **Screenshot/trace:** `artifacts/e2e/failures/game/start-training/20260619-055147-console.log`, `artifacts/e2e/failures/game/missing-session/20260619-055154-console.log`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit uzivatele a otevrit `/game`.
  2. V interaktivni WASM casti kliknout na start hry nebo otevrit neexistujici session.
  3. Sledovat failed request.
- **Ocekavani:** WASM klient vola API na dynamickem `http://127.0.0.1:{apiPort}` z E2E fixture.
- **Skutecnost:** Browser vola `https://localhost:5000/api/v1/game/...` a pada na `net::ERR_SSL_PROTOCOL_ERROR`.
- **Pravdepodobna pricina:** Serverovy env `ApiBaseUrl` se nepromitne do statickeho `wwwroot/appsettings.json`, ktery cte WASM klient.
- **Oprava:** `LexiQuest.Web` v E2E prostredi servíruje dynamicky klientsky appsettings JSON pred static files.
- **Overeni:** `GameFlowE2ETests` uz vola API na dynamickem `127.0.0.1` portu a probehl uspesne: 2/2 testy.
- **Poznamky:** Auth flow tento problem schoval, protoze cast volani probehla pri server-side interaktivite.

### E2E-BUG-0013: Game page nema registrovany `IGameService` v Blazor client DI

- **Stav:** Overeno
- **Severity:** P0
- **Oblast:** Game / Blazor client DI
- **Nalezeno v testu:** `GameFlowE2ETests.StartGame_LoggedInUser_CanStartTrainingSession`, `GameFlowE2ETests.Game_NonExistingSession_ShowsErrorState`
- **Screenshot/trace:** `artifacts/e2e/failures/game/start-training/20260619-054946-console.log`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Prihlasit testovaciho uzivatele.
  2. Otevrit `/game`.
  3. Sledovat konzoli.
- **Ocekavani:** `Game.razor` dostane `IGameService` z DI a zobrazi start screen.
- **Skutecnost:** Blazor vyhodi `There is no registered service of type 'LexiQuest.Blazor.Services.IGameService'`.
- **Pravdepodobna pricina:** `GameService` existuje, ale neni registrovany v `AddLexiQuestClientServices`.
- **Oprava:** Pridano `services.AddScoped<IGameService, GameService>()`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GameFlowE2ETests"` probehl uspesne: 2/2 testy.
- **Poznamky:** Chyba se ukazala az po oprave globalization dat.

### E2E-BUG-0012: Game route shodi Blazor WASM kvuli chybejicim globalization datum pro ceskou kulturu

- **Stav:** Overeno
- **Severity:** P0
- **Oblast:** Game / Blazor WASM / Lokalizace
- **Nalezeno v testu:** `GameFlowE2ETests.StartGame_LoggedInUser_CanStartTrainingSession`, `GameFlowE2ETests.Game_NonExistingSession_ShowsErrorState`
- **Screenshot/trace:** `artifacts/e2e/failures/game/start-training/20260619-054737.png`, `artifacts/e2e/failures/game/start-training/20260619-054737-console.log`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Registrovat a prihlasit testovaciho uzivatele.
  2. Otevrit `/game` nebo `/game/{neexistujici-guid}`.
  3. Sledovat Blazor error bar a konzoli.
- **Ocekavani:** Game route se vyrenderuje v ceske kulture bez runtime chyby.
- **Skutecnost:** Blazor hlasi `change in the application's culture that is not supported` a stranka spadne na global error bar.
- **Pravdepodobna pricina:** Klient nastavuje `CultureInfo("cs")`, ale WASM projekt nema zapnute nacteni globalization dat.
- **Oprava:** Zapnuto `<BlazorWebAssemblyLoadAllGlobalizationData>true</BlazorWebAssemblyLoadAllGlobalizationData>` v Blazor client csproj.
- **Overeni:** Nasledujici `GameFlowE2ETests` uz neobsahovaly culture runtime chybu a finalni beh probehl uspesne: 2/2 testy.
- **Poznamky:** Problem se projevil az na interaktivni WASM casti protected route.

### E2E-BUG-0011: Auth screenshoty ukazuji nevyrovnanou brand/heading semantiku a prompt linky bez mezery

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Auth / UX / Visual
- **Nalezeno v testu:** Screenshot review po `AuthE2ETests.AuthPages_RenderFocusedLayoutWithoutAppSidebar`
- **Screenshot/trace:** `artifacts/e2e/screenshots/auth/login-focused-layout/1366x900/light/loaded.png`, `artifacts/e2e/screenshots/auth/register-password-strength-localized/1366x900/light/filled-password.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Spustit auth E2E sadu se screenshot checkpointy.
  2. Otevrit auth screenshoty.
  3. Zkontrolovat brand/title a spodní prompt linky.
- **Ocekavani:** Brand `LexiQuest`, nadpis formulare a prompt odkazy maji konzistentni hierarchy a citelne mezery.
- **Skutecnost:** Login pouziva brand jako `h1`, register ma nestylovany `register-logo` a prompt text s odkazem vizualne splýva bez mezery.
- **Pravdepodobna pricina:** Nesoulad trid `register-logo` vs. `register-brand`, auth title neni primarni `h1` a CSS gap zavisi na tokenu bez fallbacku.
- **Oprava:** Sjednocen auth heading markup, doplneny stabilni gap fallbacky a focus outline odstraneny z neinteraktivniho nadpisu.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~AuthE2ETests"` probehl uspesne: 7/7 testu. Screenshot `artifacts/e2e/screenshots/auth/login-focused-layout/1366x900/light/loaded.png` zkontrolovan vizualne.
- **Poznamky:** Funkcne jde o drobnost, ale screenshot baseline by tento stav nemel schvalit.

### E2E-BUG-0010: Register password strength zobrazuje lokalizacni klice misto ceskych textu

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Auth / Registrace / Lokalizace / UX
- **Nalezeno v testu:** `AuthE2ETests.Register_PasswordStrength_UsesLocalizedCzechTextWithoutKeys`
- **Screenshot/trace:** `artifacts/e2e/failures/auth/register-password-strength-localized/...`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Otevrit `/register`.
  2. Vyplnit silne heslo `TestPass123!`.
  3. Zkontrolovat indikator sily hesla.
- **Ocekavani:** Indikator zobrazi ceske texty jako `Sila hesla` a `Silne`, bez raw resource klicu.
- **Skutecnost:** Screenshot ukazuje `TmPasswordStrength_VeryStrongTmPasswordStrength_HintAvoidCommonPatterns`.
- **Pravdepodobna pricina:** Komponenta `TmPasswordStrengthIndicator` nepouziva lokalizaci aplikace nebo nema dostupne sve resource soubory.
- **Oprava:** Nahrazena lokalizovanym lightweight indikatorem primo na register strance.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~AuthE2ETests"` probehl uspesne: 7/7 testu.
- **Poznamky:** Jde i o vizualni bug, text se nevejde dobre do formulare.

### E2E-BUG-0009: Login a register se renderuji v hlavnim app layoutu se sidebarem

- **Stav:** Overeno
- **Severity:** P2
- **Oblast:** Auth / UX / Layout
- **Nalezeno v testu:** `AuthE2ETests.AuthPages_RenderFocusedLayoutWithoutAppSidebar`
- **Screenshot/trace:** `artifacts/e2e/failures/auth/login-focused-layout/...`, `artifacts/e2e/failures/auth/register-focused-layout/...`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Otevrit `/login` nebo `/register`.
  2. Zkontrolovat, zda se zobrazuje app navigace `Dashboard`, `Hra`, `Cesty`.
- **Ocekavani:** Auth stranky maji soustredeny layout bez prihlasene app navigace.
- **Skutecnost:** Auth formular je vykreslen uvnitr `MainLayout` a vlevo je videt sidebar s protected navigaci.
- **Pravdepodobna pricina:** Login/Register nemaji explicitni `@layout`, tak spadnou do defaultniho `MainLayout`.
- **Oprava:** Pro login a register nastaven `LandingLayout`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~AuthE2ETests"` probehl uspesne: 7/7 testu.
- **Poznamky:** Screenshot ukazuje i zbytecne velkou prazdnou plochu a vychyleny formular.

### E2E-BUG-0008: Invalid login hlaska je nekonzistentni mezi Login page a AuthService resources

- **Stav:** Overeno
- **Severity:** P3
- **Oblast:** Auth / Lokalizace
- **Nalezeno v testu:** `AuthE2ETests.Login_InvalidCredentials_ShowsGenericError`
- **Screenshot/trace:** `artifacts/e2e/failures/auth/login-invalid-credentials/20260619-053332.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Otevrit `/login`.
  2. Zadavat neexistujici email a spatne heslo.
  3. Odeslat formular.
- **Ocekavani:** UI pouzije sjednoceny cesky text `Nespravny email nebo heslo` podle login resource.
- **Skutecnost:** Service resource vraci `Neplatny email nebo heslo.`, tedy jinou formulaci nez login page fallback.
- **Pravdepodobna pricina:** `Login.resx` a `Services/AuthService.resx` obsahuji ruzne texty pro stejny stav.
- **Oprava:** Text v `AuthService.resx` sjednocen s login page resource.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~AuthE2ETests"` probehl uspesne: 7/7 testu.
- **Poznamky:** Nejde o bezpecnostni problem, obe formulace jsou obecne, ale E2E ma hlidat konzistenci UI textu.

### E2E-BUG-0007: Register terms checkbox nejde zaškrtnout přes pristupny label

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Auth / Registrace / A11y
- **Nalezeno v testu:** `AuthE2ETests.Register_ValidNewUser_RedirectsToDashboard`, `AuthE2ETests.Register_DuplicateEmail_ShowsLocalizedError`
- **Screenshot/trace:** `artifacts/e2e/failures/auth/register-success/20260619-053409.png`, `artifacts/e2e/failures/auth/register-duplicate-email/20260619-053442.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Otevrit `/register`.
  2. Vyplnit validni formular.
  3. Zavolat Playwright `GetByLabel("Souhlasim s podminkami pouziti").CheckAsync()`.
- **Ocekavani:** Checkbox souhlasu je viditelny, pristupny a lze ho zaskrtnout stejne jako realny uzivatel.
- **Skutecnost:** Locator najde native input, ale ten je neviditelny, takze Playwright ceka a test timeoutuje.
- **Pravdepodobna pricina:** `TmCheckbox` renderuje skryty native input bez vhodneho stabilniho verejneho hit targetu pro tento formular.
- **Oprava:** Pro terms souhlas pouzit viditelny `InputCheckbox` ve vlastnim labelu a zachovat FluentValidation.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~AuthE2ETests"` probehl uspesne: 7/7 testu.
- **Poznamky:** Souhlas je blokujici registracni krok, proto P1.

### E2E-BUG-0006: Guest hra po spatne odpovedi nezobrazi feedback se spravnou odpovedi

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Guest / Game / UX
- **Nalezeno v testu:** `GuestE2ETests.GuestPlay_WrongAnswer_ShowsFeedbackWithCorrectAnswer`
- **Screenshot/trace:** `artifacts/e2e/failures/guest/wrong-answer-feedback/20260619-052836.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Spustit guest E2E sadu.
  2. Na `/play` zalozit guest hru.
  3. Odeslat zjevne spatnou odpoved `neexistujiciodpoved`.
- **Ocekavani:** UI zobrazi viditelny feedback `Spatne` vcetne spravne odpovedi, aby hrac vedel, co se stalo.
- **Skutecnost:** Komponenta feedback nastavi a ve stejnem handleru ho okamzite skryje, takze Playwright ani uzivatel stav neuvidi.
- **Pravdepodobna pricina:** `GuestGame.razor` po vyhodnoceni odpovedi vzdy nastavi `_showingFeedback = false`; u spravne odpovedi navic posouva `_currentWordIndex` pred modalem a `ContinueGame()` ho posune podruhe.
- **Oprava:** Upraven tok odeslani odpovedi tak, aby feedback zustal viditelny, spatna odpoved se po kratke dobe sama odemkla a spravna odpoved posunula slovo jen pri pokracovani. Doplněn stabilni `data-testid="answer-feedback"`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~GuestE2ETests"` probehl uspesne: 3/3 testy.
- **Poznamky:** Pri oprave doplnit stabilni `data-testid` pro feedback.

### E2E-BUG-0005: Register/Login UI vypisuje lokalizacni klice misto ceskych textu

- **Stav:** Overeno
- **Severity:** P1
- **Oblast:** Auth / Lokalizace / UX
- **Nalezeno v testu:** `LandingE2ETests.LandingPage_RegisterCta_NavigatesToRegister`
- **Screenshot/trace:** `artifacts/e2e/failures/landing/register-cta/20260619-052429.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Spustit landing E2E sadu.
  2. Kliknout na registracni CTA.
  3. Zkontrolovat register formular.
- **Ocekavani:** Register page zobrazi ceske texty z `Resources/Pages/Register.resx`.
- **Skutecnost:** UI zobrazuje klice `Title`, `Input.Email.Label`, `Button.Submit` atd.
- **Pravdepodobna pricina:** Assembly name `LexiQuest.Blazor.Client` a root namespace `LexiQuest.Blazor` nejsou pro lokalizaci explicitne svazane pres assembly atributy.
- **Oprava:** Doplnit do Blazor klienta assembly atributy `RootNamespace("LexiQuest.Blazor")` a `ResourceLocation("Resources")`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~LandingE2ETests.LandingPage_RegisterCta"` probehl uspesne.
- **Poznamky:** Screenshot zachytil i velmi spatny UX stav auth formulare.

### E2E-BUG-0004: Auth a guest route shodi Blazor circuit kvuli rekurzi `ApiClient` auth handleru

- **Stav:** Overeno
- **Severity:** P0
- **Oblast:** Auth / Guest / Client DI
- **Nalezeno v testu:** `LandingE2ETests.LandingPage_RegisterCta_NavigatesToRegister`, `LandingE2ETests.LandingPage_GuestCta_NavigatesToGuestPlay`
- **Screenshot/trace:** `artifacts/e2e/failures/landing/register-cta/20260619-052208-console.log`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless
- **Reprodukce:**
  1. Spustit landing E2E sadu.
  2. Kliknout na CTA registrace nebo guest.
  3. Cílová stránka shodí Blazor server circuit.
- **Ocekavani:** Register a guest route se vyrenderuji a jejich služby umí volat API.
- **Skutecnost:** Server log hlasi `ValueFactory attempted to access the Value property of this instance` pri vytvareni `ApiClient`; `AuthService` vytvari `ApiClient`, jeho handler injectuje `IAuthService`, a vznikne rekurze.
- **Pravdepodobna pricina:** Jeden pojmenovany HTTP client `ApiClient` je pouzit jak pro autentizacni sluzbu, tak pro auth handler zavisly na teto sluzbe.
- **Oprava:** Oddelit `PublicApiClient` bez auth handleru pro auth/guest flow, ponechat `ApiClient` pro protected flow a doplnit `LexiQuestApi` alias pouzivany herni sluzbou.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~LandingE2ETests"` uz nema circuit chybu; guest CTA prosla.
- **Poznamky:** Stejny audit odhalil, ze `GameService` pouziva neregistrovany klient `LexiQuestApi`.

### E2E-BUG-0003: Web v E2E neserviruje Blazor static web assets a landing je prazdny

- **Stav:** Overeno
- **Severity:** P0
- **Oblast:** Public web / Web startup / Static assets
- **Nalezeno v testu:** `LandingE2ETests.LandingPage_LoadsAllPrimarySections_AndStoresUxCheckpoint`
- **Screenshot/trace:** `artifacts/e2e/failures/landing/primary-sections/20260619-051350.png`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless, desktop 1366x900
- **Reprodukce:**
  1. Spustit `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~LandingE2ETests"`.
  2. Otevrit landing route `/`.
  3. Zkontrolovat konzoli a screenshot.
- **Ocekavani:** Landing page zobrazi hero, sekce, footer a nacte Blazor runtime i CSS assety.
- **Skutecnost:** Screenshot je prazdna bila stranka; requesty na `_framework/blazor.web.js`, `LexiQuest.Blazor.Client.styles.css` a `_content/Tempo.Blazor/css/tempo-blazor.css` vraci 404.
- **Pravdepodobna pricina:** `LexiQuest.Web` nepřipojuje static web assets endpointy pro Blazor Web App / .NET 10.
- **Oprava:** Doplnit `MapStaticAssets()` a pro prostredi `E2E` povolit static web assets pres `StaticWebAssetsLoader.UseStaticWebAssets(...)`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~LandingE2ETests.LandingPage_LoadsAllPrimarySections"` probehl uspesne.
- **Poznamky:** Infra health check samotny tento problem nezachytil, protoze root endpoint vracel HTTP 200.

## Overene opravy

### E2E-BUG-0002: smtp4dev Testcontainer nejde spustit s port bindingem `127.0.0.1:0`

- **Stav:** Overeno
- **Severity:** P0
- **Oblast:** Infra / smtp4dev
- **Nalezeno v testu:** `InfrastructureE2ETests.E2EEnvironment_StartsApiWebDatabaseAndSmtp4Dev`
- **Screenshot/trace:** Docker API chyba v `dotnet test`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless
- **Reprodukce:**
  1. Spustit `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~InfrastructureE2ETests"`.
  2. Fixture se pokusi spustit smtp4dev container s `WithPortBinding("127.0.0.1:0", "25/tcp")`.
  3. Docker vrati `invalid port specification: "127.0.0.1:0"`.
- **Ocekavani:** smtp4dev expose porty jsou nahodne a dostupne pouze pres localhost.
- **Skutecnost:** Docker container se nevytvori, protoze zapis host IP + nahodny port neni validni v pouzitem Testcontainers API.
- **Pravdepodobna pricina:** `WithPortBinding(string, string)` neakceptuje host binding ve tvaru `hostIp:0`.
- **Oprava:** Vratit `WithPortBinding(25, true)` / `WithPortBinding(80, true)` a pres `WithCreateParameterModifier` nastavit `HostIP=127.0.0.1` pro vsechny Docker port bindingy.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~InfrastructureE2ETests"` probehl uspesne.
- **Poznamky:** Pozadavek na localhost binding zustava zachovan nizkourovnovou Docker konfiguraci.

### E2E-BUG-0001: Web proces pri E2E startu nenajde Tempo.Blazor.Abstractions

- **Stav:** Overeno
- **Severity:** P0
- **Oblast:** Infra / Web startup
- **Nalezeno v testu:** `InfrastructureE2ETests.E2EEnvironment_StartsApiWebDatabaseAndSmtp4Dev`
- **Screenshot/trace:** stdout/stderr z `AppProcessRunner`
- **Prostredi:** SQL Server Testcontainer, smtp4dev Testcontainer, Chromium headless
- **Reprodukce:**
  1. Spustit `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~InfrastructureE2ETests"`.
  2. Fixture nastartuje API a pote Web proces.
  3. Web proces skonci pred health checkem.
- **Ocekavani:** `LexiQuest.Web` nabehne na dynamickem E2E portu.
- **Skutecnost:** Runtime vyhodi `FileNotFoundException` pro `Tempo.Blazor.Abstractions, Version=1.1.0.0`.
- **Pravdepodobna pricina:** Tempo balicky ve Web/Client projektu nebyly zarovnane na verzi, ktera ma konzistentni assembly identity.
- **Oprava:** Zarovnat `Tempo.Blazor`, `Tempo.Blazor.Abstractions` a `Tempo.Blazor.FluentValidation` ve Web/Client projektu na `1.1.15`.
- **Overeni:** `dotnet test tests/LexiQuest.E2E.Tests/LexiQuest.E2E.Tests.csproj --filter "FullyQualifiedName~InfrastructureE2ETests"` probehl uspesne.
- **Poznamky:** Nalez vznikl az pri realnem process startu, unit/bUnit testy ho nezachytily.

---

## Technicke poznamky k evidenci

- Kazdy bug musi odkazovat na konkretni E2E test nebo screenshot checkpoint.
- Pokud test spadne kvuli testovaci infrastrukture, zapisuje se take sem s oblasti `Infra`.
- Pokud screenshot vypada spatne, ale funkcni asserty projdou, stale jde o bug typu `UX` nebo `Visual`.
- Pokud je chyba opravena, musi se zapsat, ktery test ji overil.
- Pokud se rozhodne, ze chovani je spravne a test byl spatne, stav bude `Neni chyba` a do poznamek se doplni uprava testu.
