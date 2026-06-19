# Fáze 8 – Oprava všech problémů z review aplikace

> **Celkem: 118 failing testů (114 Blazor + 4 API) + 12 produkčních problémů**
> **Odhad: ~35-45 hodin**

---

## 1. KRITICKÉ – Oprava testů (118 failures)

### 1.1 Blazor testy – Chybějící `ITmLocalizer` v DI (ovlivňuje ~70 testů)

**Příčina:** Tempo.Blazor komponenty (`TmModal`, `TmCard`, `TmAvatar`, `TmCopyButton`, `TmIcon`, `TmAlert`) vyžadují `ITmLocalizer` přes `@inject`, ale žádný test ho neregistruje.

**Řešení:** Vytvořit sdílený test helper / base class, který registruje mock `ITmLocalizer` do bUnit `TestContext.Services`. Aplikovat na VŠECHNY test soubory.

**Ovlivněné test soubory:**
- [x] `Components/CreateRoomModalTests.cs` (5 testů) – TmModal
- [x] `Components/JoinRoomModalTests.cs` (12 testů) – TmModal
- [x] `Components/RoomLobbyTests.cs` (8 testů) – TmCopyButton, TmAvatar
- [x] `Components/PlayerResultCardTests.cs` (4 testy) – TmAvatar
- [x] `Components/SeriesScoreTests.cs` (5 testů) – TmAvatar
- [x] `Pages/RegisterPageTests.cs` (7 testů) – TmCard
- [x] `Pages/LoginPageTests.cs` (5 testů) – TmCard
- [x] `Pages/PremiumPageTests.cs` (12 testů) – TmCard + TmIcon (+ viz 1.2)
- [x] `Pages/MatchHistoryPageTests.cs` (8 testů) – TmAvatar (+ viz 1.3)
- [x] `Pages/MultiplayerLandingPageTests.cs` (8 testů) – TmModal (+ viz 1.4)

**Implementace:**
```
1. Vytvořit `tests/LexiQuest.Blazor.Tests/Helpers/TempoTestHelper.cs`
   - Statická metoda `RegisterTempoServices(TestServiceProvider services)`
   - Registruje mock ITmLocalizer (NSubstitute)
   - Registruje případné další Tempo DI závislosti
2. Volat helper v konstruktoru každého test souboru
3. Spustit testy a ověřit opravu
```

**Odhad:** 3-4 hodiny

---

### 1.2 Blazor testy – Chybějící `IToastService` (ovlivňuje ~12 testů)

**Příčina:** `Premium.razor` injektuje `IToastService`, ale `PremiumPageTests` ho neregistruje.

**Ovlivněné test soubory:**
- [x] `Pages/PremiumPageTests.cs` – 12 testů (kombinace s 1.1)

**Řešení:** Přidat `Services.AddSingleton(Substitute.For<IToastService>())` v konstruktoru testu.

**Odhad:** 30 minut

---

### 1.3 Blazor testy – Chybějící `IMatchHistoryClient` (ovlivňuje ~8 testů)

**Příčina:** `MatchHistory.razor` injektuje `IMatchHistoryClient`, ale `MatchHistoryPageTests` ho neregistruje.

**Ovlivněné test soubory:**
- [x] `Pages/MatchHistoryPageTests.cs` – 8 testů (kombinace s 1.1)

**Řešení:** Přidat mock `IMatchHistoryClient` do DI v konstruktoru testu.

**Odhad:** 30 minut

---

### 1.4 Blazor testy – Chybějící `IMatchHubClient` (ovlivňuje ~8 testů)

**Příčina:** `Multiplayer.razor` injektuje `IMatchHubClient`, ale `MultiplayerLandingPageTests` ho neregistruje.

**Ovlivněné test soubory:**
- [x] `Pages/MultiplayerLandingPageTests.cs` – 8 testů (kombinace s 1.1)

**Řešení:** Přidat mock `IMatchHubClient` do DI v konstruktoru testu.

**Odhad:** 30 minut

---

### 1.5 Blazor testy – Chybějící `IAuthService` (ovlivňuje 3 testy)

**Příčina:** `QuickMatch.razor` injektuje `IAuthService`, ale `MatchmakingPageTests` ho neregistruje.

**Ovlivněné test soubory:**
- [x] `Pages/MatchmakingPageTests.cs` – 3 testy

**Řešení:** Přidat mock `IAuthService` do DI v konstruktoru testu.

**Odhad:** 15 minut

---

### 1.6 Blazor testy – `ArgumentNullException` v `GameArena` lokalizaci (ovlivňuje ~19 testů)

**Příčina:** Testy poskytují lokalizační klíče s podtržítkem (`Level_Name`), ale komponenta `GameArena.razor` používá tečku (`Level.Name`). Localizer vrátí null → `string.Format(null, ...)` vyhodí `ArgumentNullException`.

**Ovlivněné test soubory:**
- [x] `Components/GameArenaTests.cs` – 16 testů
- [x] `Pages/GamePageTests.cs` – 2 testy
- [x] `Pages/RealtimeGamePageTests.cs` – 1 test

**Řešení (opravit testy – komponenta má správný formát s tečkou):**
```
V mock localizerech opravit klíče:
  "Level_Name"       → "Level.Name"
  "Level_Progress"   → "Level.Progress"
  "Combo_Multiplier" → "Combo.Multiplier"
  "Feedback_Correct" → "Feedback.Correct"
  "Feedback_Wrong"   → "Feedback.Wrong"
  ... a všechny další klíče s podtržítkem → tečka
```

**Odhad:** 1-2 hodiny

---

### 1.7 Blazor testy – `NavigationManagerProxy` neinicializován (ovlivňuje ~7 testů)

**Příčina:** `GuestGame.razor` a další stránky používají `NavigationManager`, ale testy neinicializují bUnit `FakeNavigationManager` nebo nepoužívají `ctx.Services.GetRequiredService<NavigationManager>()`.

**Ovlivněné test soubory:**
- [x] `Pages/GuestGamePageTests.cs` – 7 testů
- [x] `Pages/MultiplayerLandingPageTests.cs` – 1 test (navigace)

**Řešení:** Přidat `ctx.Services.AddSingleton<NavigationManager>(new FakeNavigationManager(...))` nebo použít bUnit's vestavěný `TestContext` který NavigationManager registruje automaticky – ověřit, zda test dědí z `TestContext`.

**Odhad:** 1 hodina

---

### 1.8 Blazor testy – `TmButton.CssClass` není `[Parameter]` (ovlivňuje 3 testy)

**Příčina:** `NotificationBellTests` předává `CssClass` parametr do `TmButton`, ale v aktuální verzi Tempo.Blazor tento parametr buď neexistuje nebo se jmenuje jinak (pravděpodobně `Class` nebo `AdditionalClasses`).

**Ovlivněné test soubory:**
- [x] `Components/NotificationBellTests.cs` – 3 testy

**Řešení:**
```
1. Ověřit skutečný název CSS parametru v TmButton (přečíst zdrojový kód Tempo.Blazor)
2. Opravit NotificationBell.razor – použít správný název parametru
3. Případně opravit test pokud problém je v testu
```

**Odhad:** 30-45 minut

---

### 1.9 Blazor testy – `SupplyParameterFromQuery` bez NavigationManager (1 test)

**Příčina:** Test renderuje komponentu s `[SupplyParameterFromQuery]` parametrem, ale neposkytuje ho přes `NavigationManager`.

**Ovlivněné test soubory:**
- [x] `Pages/GuestGamePageTests.cs` – 1 test (překrývá se s 1.7)

**Řešení:** Použít bUnit's `SetParameterFromQuery` nebo navigovat na URL s query parametrem.

**Odhad:** zahrnuto v 1.7

---

### 1.10 Blazor testy – ErrorBoundary testy (2 testy)

**Příčina:** `ErrorBoundary` komponenta nedědí z `ErrorBoundaryBase` nebo neimplementuje správné zachycení výjimek z child komponent v bUnit kontextu.

**Ovlivněné test soubory:**
- [x] `Components/ErrorBoundaryTests.cs` – 2 testy

**Řešení:**
```
1. Zkontrolovat, zda ErrorBoundary.razor dědí z ErrorBoundaryBase
2. Pokud ne → opravit komponentu aby správně dědila a overridovala OnErrorAsync
3. Pokud ano → upravit test aby správně simuloval výjimky v bUnit
```

**Odhad:** 1 hodina

---

### 1.11 API testy – `UserSettingsEndpointsTests` 401 Unauthorized (4 testy)

**Příčina:** Testy autentizují přes login endpoint, ale JWT token není akceptován validačním middleware. Pravděpodobná příčina: konfigurace JWT settings se nenačítá správně do `TokenService` a/nebo validačního middleware v test factory. Test nekontroluje status code login odpovědi.

**Ovlivněné test soubory:**
- [x] `Endpoints/UserSettingsEndpointsTests.cs` – 4 testy:
  - `GetUserProfile_Returns200WithProfile`
  - `UpdateProfile_ValidData_Returns200`
  - `UpdatePreferences_ValidData_Returns200`
  - `UpdatePrivacySettings_ValidData_Returns200`

**Řešení:**
```
1. Přidat kontrolu loginResponse.StatusCode v AuthenticateAsync()
2. Porovnat JWT konfiguraci v CustomWebApplicationFactory vs. Program.cs
3. Ověřit, zda ConfigureAppConfiguration je voláno PŘED čtením JwtSettings
4. Pokud login selhává → diagnostikovat proč (chybějící DB seed, špatná konfigurace)
5. Alternativně: použít GenerateTestToken() jako v MatchHistoryEndpointsTests
```

**Odhad:** 2-3 hodiny

---

## 2. KRITICKÉ – Produkční problémy

### 2.1 Nebezpečné `Guid.Parse()` ve 4 kontrolerech

**Příčina:** Kontrolery mají privátní `GetUserId()` metodu s `Guid.Parse(userIdClaim!)` místo používání existující bezpečné extension metody `ClaimsPrincipalExtensions.GetUserId()` která používá `Guid.TryParse()`.

**Soubory k opravě:**
- [x] `src/LexiQuest.Api/Controllers/ShopController.cs` – nahradit privátní `GetUserId()` → `User.GetUserId()`
- [x] `src/LexiQuest.Api/Controllers/NotificationsController.cs` – stejně
- [x] `src/LexiQuest.Api/Controllers/PremiumController.cs` – stejně
- [x] `src/LexiQuest.Api/Controllers/StreakProtectionController.cs` – stejně

**Postup:**
```
1. Přidat using na ClaimsPrincipalExtensions namespace
2. Smazat privátní GetUserId() metodu
3. Nahradit všechna volání → User.GetUserId()
4. Spustit existující testy
```

**Odhad:** 30 minut

---

### 2.2 Výkon `OrderBy(Guid.NewGuid())` v WordRepository

**Příčina:** `OrderBy(w => Guid.NewGuid())` generuje GUID pro KAŽDÝ řádek v DB a pak třídí – extrémně pomalé na velkém slovníku.

**Soubor:** `src/LexiQuest.Infrastructure/Persistence/Repositories/WordRepository.cs` (řádek ~72)

**Řešení:**
```
1. Načíst jen ID slov matching filtru
2. Randomizovat ID v paměti (Fisher-Yates na poli ID)
3. Vzít požadovaný počet ID
4. Načíst plné entity pro vybraná ID
5. Alternativně: použít SQL Server TABLESAMPLE nebo ORDER BY NEWID() pro menší datasety
```

**Odhad:** 1-2 hodiny

---

### 2.3 EmailService – kompletní implementace

**Příčina:** Všechny 3 email metody jsou TODO/mock – `SendPasswordResetEmailAsync()`, `SendWelcomeEmailAsync()`, `SendNotificationEmailAsync()` jen logují a vrací `Task.CompletedTask`.

**Soubor:** `src/LexiQuest.Core/Services/EmailService.cs`

**Řešení:**
```
1. Přidat NuGet balíček SendGrid nebo MailKit/SMTP
2. Vytvořit EmailSettings konfiguraci (appsettings.json)
3. Implementovat SendPasswordResetEmailAsync() – HTML šablona + token URL
4. Implementovat SendWelcomeEmailAsync() – uvítací email
5. Implementovat SendNotificationEmailAsync() – obecné notifikace
6. Přidat unit testy pro email service
7. Přidat health check pro email connectivity
```

**Odhad:** 4-6 hodin

---

## 3. VYSOKÁ PRIORITA

### 3.1 MatchHub – statické Dictionary → ConcurrentDictionary

**Příčina:** `_connectionToMatch` a `_connectionToRoom` jsou `Dictionary<>` modifikované z více SignalR spojení současně → race conditions, potenciální corrupted state.

**Soubor:** `src/LexiQuest.Api/Hubs/MatchHub.cs` (řádky 20-21)

**Řešení:**
```
1. Změnit Dictionary<string, Guid> → ConcurrentDictionary<string, Guid>
2. Změnit Dictionary<string, string> → ConcurrentDictionary<string, string>
3. Nahradit .Add() → .TryAdd(), .Remove() → .TryRemove()
4. Přidat periodický cleanup pro disconnected sessions
5. Přidat testy
```

**Odhad:** 1-2 hodiny

---

### 3.2 InventoryService – implementace coin systému

**Příčina:** `GetCoinBalanceAsync()` vrací hardcoded 1000, `AddCoinsAsync()` nic nedělá, `SpendCoinsAsync()` vždy vrací true bez validace.

**Soubor:** `src/LexiQuest.Core/Services/InventoryService.cs`

**Řešení:**
```
1. Přidat separátní UserWallet entity
2. Vytvořit EF migraci
3. Implementovat GetCoinBalanceAsync() – čtení z DB
4. Implementovat AddCoinsAsync() – atomická operace přidání coinů
5. Implementovat SpendCoinsAsync() – atomická operace s kontrolou zůstatku
6. Přidat unit testy
7. Přidat integrační testy
```

**Odhad:** 3-4 hodiny

---

### 3.3 Přidat `app.UseHsts()` pro produkci

**Soubor:** `src/LexiQuest.Api/Program.cs`

**Řešení:**
```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();
```

**Odhad:** 5 minut

---

### 3.4 In-memory session cleanup

**Příčina:** `GuestSessionService` a `MultiplayerGameService` ukládají sessions pouze v paměti bez cleanup mechanismu → memory leak při dlouhém běhu.

**Soubory:**
- [x] `src/LexiQuest.Core/Services/GuestSessionService.cs`
- [x] `src/LexiQuest.Core/Services/MultiplayerGameService.cs`

**Řešení:**
```
1. Přidat TTL (Time-To-Live) na session záznamy
2. Implementovat background job (Hangfire) pro periodický cleanup expirovaných sessions
3. Alternativně: použít IMemoryCache s absolutní expirací místo ConcurrentDictionary
4. Přidat testy
```

**Odhad:** 2-3 hodiny

---

## 4. STŘEDNÍ PRIORITA

### 4.1 Chat validace v SignalR hubu

**Soubor:** `src/LexiQuest.Api/Hubs/MatchHub.cs` (metoda `SendLobbyMessage`)

**Řešení:**
```
1. Přidat validaci prázdných/whitespace zpráv
2. Přidat HTML/XSS sanitizaci (HtmlEncoder.Default.Encode)
3. Přidat rate limiting na zprávy (max 10 zpráv za 10 sekund)
4. Přidat testy
```

**Odhad:** 1-2 hodiny

---

### 4.2 Accessibility (a11y) na frontend komponentách

**Příčina:** Pouze `Footer.razor` má `aria-label` atributy. Ostatní interaktivní komponenty nemají přístupnostní atributy.

**Řešení:**
```
1. Audit všech interaktivních komponent
2. Přidat aria-label na tlačítka bez textu (ikony)
3. Přidat aria-live regions pro dynamický obsah (skóre, timer, feedback)
4. Přidat role atributy na herní komponenty
5. Ověřit focus management v modálech
6. Přidat aria-describedby na form inputs s chybovými zprávami
7. Ověřit keyboard navigaci
```

**Komponenty k opravě:**
- [x] `Components/Game/GameArena.razor` – aria-live na skóre, feedback
- [x] `Components/Game/GameTimer.razor` – aria-live na časovač
- [x] `Components/Multiplayer/RoomLobby.razor` – focus trap v lobby
- [x] `Pages/QuickMatch.razor` – aria-live na stav matchmakingu
- [x] `Pages/RealtimeGame.razor` – aria pro multiplayer state
- [x] `Layout/MainLayout.razor` – landmark roles
- [x] Všechny modály – focus trap, aria-modal

**Odhad:** 4-6 hodin

---

### 4.3 Centrální error logging na frontendu

**Řešení:**
```
1. Vytvořit IErrorLoggingService interface + implementaci
2. Implementovat global error handler (ErrorBoundary na App úrovni)
3. Logovat chyby na server endpoint (POST /api/v1/client-errors)
4. Přidat API endpoint pro příjem client-side chyb
5. Zahrnout kontext (user ID, stránka, stack trace)
6. Přidat testy
```

**Odhad:** 3-4 hodiny

---

### 4.4 Race conditions v multiplayer state

**Příčina:** Lokální skóre se aktualizuje před serverovou konfirmací. Replay opponent progress eventů může duplikovat skóre.

**Řešení:**
```
1. Přidat sequence numbers na multiplayer eventy
2. Deduplikovat eventy na klientu podle sequence number
3. Server-authoritative score – klient zobrazuje server hodnoty
4. Přidat optimistické UI update s rollback při neshodě
5. Přidat testy
```

**Odhad:** 3-4 hodiny

---

## 5. NÍZKÁ PRIORITA

### 5.1 Nekonzistentní type safety

**Příčina:** Některá místa používají `string` literal kde by měl být `enum`.

**Řešení:**
```
1. Najít všechna místa s RadioOption<string> a nahradit za enum
2. Sjednotit přístup k type-safe options
```

**Odhad:** 1 hodina

---

### 5.2 Mrtvý kód – nedostupné stránky

**Příčina:** Boss level stránky (`ConditionBoss`, `MarathonBoss`, `TwistBoss`) a další existují ale nejsou napojené na navigaci.

**Řešení:**
```
1. Audit všech @page direktiv vs. navigační linky
2. Buď přidat navigaci, nebo označit jako [WIP] s redirectem
3. Uklidit nepoužívané komponenty
```

**Odhad:** 1-2 hodiny

---

### 5.3 Neúplné stránky – placeholder implementace

**Příčina:** Některé stránky mají minimální implementaci.

**Stránky:**
- [x] `Pages/DailyChallenge.razor` – StartChallenge navigace, result z leaderboard
- [x] `Pages/Leagues.razor` – historie, rewards, progress bar, time remaining
- [x] Boss level stránky (plně implementováno – ConditionBoss, MarathonBoss, TwistBoss)
- [x] `Pages/Profile.razor` – nová stránka s profilem, statistikami, achievementy
- [x] `Pages/About.razor` – nová stránka o aplikaci
- [x] `Pages/Terms.razor` – nová stránka s podmínkami použití
- [x] `Pages/Privacy.razor` – nová stránka s ochranou soukromí
- [x] `Pages/Contact.razor` – nová stránka s kontaktním formulářem
- [x] `Pages/Dictionaries.razor` – napojení na IDictionaryService, lokalizace

**Řešení:** Dopsat implementaci.

**Odhad:** záleží na scope – 2-8 hodin per stránka

---

## Souhrn odhadů

| Kategorie | Položky | Odhad |
|-----------|---------|-------|
| **1. Oprava testů** | 1.1–1.11 | 10-14 hodin |
| **2. Kritické produkční** | 2.1–2.3 | 6-9 hodin |
| **3. Vysoká priorita** | 3.1–3.4 | 6-10 hodin |
| **4. Střední priorita** | 4.1–4.4 | 11-16 hodin |
| **5. Nízká priorita** | 5.1–5.3 | 4-12 hodin |
| **CELKEM** | | **~37-61 hodin** |

---

## Doporučené pořadí implementace

```
Sprint 1 (den 1-2): Kritické opravy
  → 2.1 Guid.Parse (30 min)
  → 2.3 EmailService stub → skutečná implementace (4-6h)
  → 3.3 UseHsts (5 min)
  → 1.1 TempoTestHelper + ITmLocalizer fix (~70 testů) (3-4h)

Sprint 2 (den 3-4): Oprava zbytku testů
  → 1.2-1.5 Chybějící DI services v testech (1.5h)
  → 1.6 Lokalizační klíče GameArena (1-2h)
  → 1.7 NavigationManager v testech (1h)
  → 1.8-1.10 Ostatní test fixes (2h)
  → 1.11 API UserSettings testy (2-3h)

Sprint 3 (den 5-6): Produkční kvalita
  → 2.2 WordRepository výkon (1-2h)
  → 3.1 ConcurrentDictionary v MatchHub (1-2h)
  → 3.2 InventoryService coins (3-4h)
  → 3.4 Session cleanup (2-3h)

Sprint 4 (den 7-8): UX a bezpečnost
  → 4.1 Chat validace (1-2h)
  → 4.2 Accessibility (4-6h)
  → 4.3 Error logging (3-4h)
  → 4.4 Multiplayer state (3-4h)

Sprint 5 (den 9): Cleanup
  → 5.1-5.3 Nízká priorita (4-12h)
```
