# UC-027: Testovací hra bez registrace (Guest Mode)

## Popis
Umožnit návštěvníkům vyzkoušet hru okamžitě bez nutnosti registrace. Slouží jako "try before you buy" - uživatel zažije core gameplay a chce se pak registrovat pro ukládání progressu.

## Aktéři
- **Primary Actor:** Host (nepřihlášený návštěvník)
- **Secondary Actor:** Systém (IP tracking, Rate limiting)

## Předpoklady
- Landing page je dostupná
- Hra je funkční (UC-004)

## Post-conditions
**Úspěch:**
- Host si zahraje 1-5 kol
- Zobrazí se CTA pro registraci
- Host se zaregistruje a převede si progress (volitelné)

**Neúspěch:**
- Host dosáhne limitu her
- Host odejde bez registrace

## Omezení Guest módu

| Funkce | Registrovaný | Guest |
|--------|--------------|-------|
| Ukládání progressu | ✅ Ano | ❌ Ne |
| Počet her | ∞ | Max 5 za den |
| XP zisk | ✅ Ano | ❌ Ne (ukázka jen) |
| Streak | ✅ Ano | ❌ Ne |
| Ligy | ✅ Ano | ❌ Ne |
| Achievementy | ✅ Ano | ❌ Ne |
| Cesty | Všechny | Pouze 5 levelů z Cesty 1 |
| Leaderboard | ✅ Ano | ❌ Ne (vidí ale neukládá) |
| Historie her | ✅ Ano | ❌ Ne |
| Multiplayer | ✅ Ano | ❌ Ne |

## Hlavní tok - Zahájení Guest hry

| Krok | Akce | Data | Popis |
|------|------|------|-------|
| 1 | Návštěvník otevře Landing page | - | - |
| 2 | Klikne "🎮 Zkusit zdarma" | - | CTA tlačítko |
| 3 | Systém kontroluje Guest limit | IP/Cookie | Max 5 her za 24h |
| 3a | Limit dosažen | - | Zobrazit "Pro více her se registruj" |
| 3b | Limit OK | - | Pokračovat |
| 4 | Systém vytvoří GuestSession | sessionId | Dočasná session (In-Memory nebo LocalStorage) |
| 5 | Zahájí se hra (Cesta 1, Level 1) | - | Plnohodnotná herní zkušenost |
| 6 | Host hraje | - | Stejná mechanika jako registrovaný |
| 7 | Po 5 slovech / GameOver / LevelComplete | - | Konec Guest session |
| 8 | Zobrazení výsledků | - | "Skvělé! Zaregistruj se pro ukládání" |
| 9 | CTA pro registraci | - | Modal s benefity registrace |

## Guest Session Types

### 1. Pure Frontend (doporučeno pro MVP)
```
- Bez volání API
- Slova zabudovaná v kódu (5-10 statických slov)
- Scramble probíhá v JavaScriptu
- Statistiky se neukládají nikde
- Jednoduché, žádný backend kód potřeba
```

### 2. API with Anonymous Session (pro pokročilé)
```
- Volání API s anonymním tokenem
- SessionId v URL nebo Header
- Rate limiting podle IP
- Dočasná session v DB (expires za 24h)
- Slova se načítají z DB
```

**Pro MVP doporučuji Varianta 1 (Pure Frontend)** - rychlejší implementace, žádná zátěž na server.

## Guest Limitování

### Strategie A: Cookie-based (jednodušší)
```javascript
// Uložit do cookie/localStorage
guestGamesPlayed: 2
guestLastPlayDate: 2024-03-15

// Kontrola při načtení
if (gamesPlayed >= 5 && lastDate === today) {
    showRegistrationWall();
}
```

### Strategie B: IP-based (bezpečnější)
```csharp
// Backend tracking
public class GuestLimiter {
    // In-Memory cache: IP -> PlayCount
    // Reset každých 24h
}
```

**Pro MVP: Strategie A (Frontend only)**

## Flow Diagram

```
[Landing Page]
      ↓
[Click "Zkusit zdarma"]
      ↓
[Check Cookie: guestGamesToday < 5?]
      ↓
   ┌──┴──┐
   │     │
  YES    NO
   │     │
   ↓     ↓
[Start   [Show Modal:
 Game]    "Limit dosažen
           - Registruj se"]
   │
   ↓
[Play 5 words]
   │
   ↓
[Show Results]
   │
   ↓
[CTA: "Registruj se
       a ukládej
       progress!"]
```

## UI/UX Flow

### 1. Entry Point (Landing Page)
```
┌─────────────────────────────────────┐
│                                     │
│         🔥 LOGO 🔥                  │
│                                     │
│    „Rozlušti slova..."             │
│                                     │
│   [🎮 ZKUSIT ZDARMA]               │  ← Secondary CTA
│                                     │
│   [✨ VYTVOŘIT ÚČET]               │  ← Primary CTA
│                                     │
└─────────────────────────────────────┘
```

### 2. Guest Game In-Progress
```
Identické s normální hrou, ale:
- V headeru: "Režim hosta - [Registrovat se]"
- Po každém slově: Subtle reminder "Pro ukládání XP se registruj"
```

### 3. Post-Game CTA Modal
```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│                        🎉                                   │
│                   SKVĚLÉ VÝSLEDKY!                         │
│                                                             │
│              Správně: 4/5 slov                             │
│                                                             │
│         [Confetti animation]                                │
│                                                             │
│   ═══════════════════════════════════════════════════       │
│                                                             │
│   Chceš svůj progress ukládat?                             │
│                                                             │
│   ✅ Ukládej XP a leveluj                                   │
│   ✅ Soutěž v ligách                                        │
│   ✅ Buduj streak                                           │
│   ✅ Odemkni achievementy                                   │
│                                                             │
│   [🚀 VYTVOŘIT ÚČET ZDARMA]                                │
│                                                             │
│   [❌ Ne, díky - hrát jako host]                           │  ← zpět na landing
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 4. Limit Reached Wall
```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│                        🚫                                   │
│               DOSÁHL JSI DNEŠNÍHO LIMITU                   │
│                                                             │
│         Jako host můžeš hrát max 5 her za den.             │
│                                                             │
│         Registrací získáš:                                 │
│         • Neomezený počet her                              │
│         • Ukládání progressu                               │
│         • Soutěžení v ligách                               │
│                                                             │
│         [🚀 REGISTROVAT SE]                                │
│                                                             │
│         [🔔 Připomenout zítra]                             │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## Implementace (Pure Frontend)

### GuestWordsService.cs (Blazor)
```csharp
public class GuestWordsService
{
    // Statický seznam 10 slov pro guest mód
    private static readonly List<Word> GuestWords = new()
    {
        new Word("KOCKA", "KACKO"),
        new Word("PES", "SPE"),
        new Word("DUM", "MUD"),
        new Word("AUTO", "UTAO"),
        new Word("STROM", "MORTS"),
        // ... další
    };
    
    public Word GetWord(int index) => GuestWords[index % GuestWords.Count];
    
    public string Scramble(string word)
    {
        // Fisher-Yates shuffle
        var chars = word.ToCharArray();
        var rng = new Random();
        // ...
        return new string(chars);
    }
}
```

### GuestSessionStorage.cs
```csharp
// LocalStorage wrapper pro guest data
public class GuestSessionStorage
{
    private readonly ILocalStorageService _localStorage;
    
    public async Task<int> GetGamesPlayedToday()
    {
        var lastDate = await _localStorage.GetItemAsync<string>("guestLastDate");
        var today = DateTime.Now.ToString("yyyy-MM-dd");
        
        if (lastDate != today)
        {
            await _localStorage.SetItemAsync("guestGamesToday", 0);
            await _localStorage.SetItemAsync("guestLastDate", today);
            return 0;
        }
        
        return await _localStorage.GetItemAsync<int>("guestGamesToday");
    }
    
    public async Task IncrementGamesPlayed()
    {
        var current = await GetGamesPlayedToday();
        await _localStorage.SetItemAsync("guestGamesToday", current + 1);
    }
    
    public async Task<bool> CanPlayAsGuest()
    {
        return await GetGamesPlayedToday() < 5;
    }
}
```

### GuestGameFlow
```
1. Kontrola limitu (LocalStorage)
2. Načtení 5 slov (GuestWordsService)
3. Hraní (GameArena komponenta v Guest módu)
4. Uložení výsledků do LocalStorage (dočasně)
5. Zobrazení CTA modalu
6. Při registraci: Převést LocalStorage data do API
```

## Převod Guest → Registered

### Scénář: Host se registruje během session
```
1. Host odehraje 3 slova (skóre: 3/3)
2. Klikne "Registrovat se"
3. Vyplní registraci
4. Systém detekuje guest progress v LocalStorage
5. Zeptá se: "Chceš převést svých 3 splněná slova?"
6. Po registraci: XP se přičte k novému účtu
```

### Implementace převodu
```csharp
// Po úspěšné registraci
public async Task ConvertGuestProgress(Guid newUserId)
{
    var guestProgress = await _localStorage.GetItemAsync<GuestProgress>("guestProgress");
    
    if (guestProgress != null)
    {
        // Přidat XP novému uživateli
        await _userService.AddXP(newUserId, guestProgress.XP);
        
        // Označit převedené levely
        foreach (var level in guestProgress.CompletedLevels)
        {
            await _userService.MarkLevelCompleted(newUserId, level);
        }
        
        // Vyčistit LocalStorage
        await _localStorage.RemoveItemAsync("guestProgress");
    }
}
```

## Resource klíče

```
Guest.CTA.Button
Guest.Header.Label
Guest.Header.Register
Guest.Game.Reminder
Guest.Results.Title
Guest.Results.Score
Guest.Results.CTA.Title
Guest.Results.CTA.Benefits.XP
Guest.Results.CTA.Benefits.Leagues
Guest.Results.CTA.Benefits.Streak
Guest.Results.CTA.Benefits.Achievements
Guest.Results.CTA.Button
Guest.Results.CTA.Skip
Guest.Limit.Title
Guest.Limit.Message
Guest.Limit.Benefits
Guest.Limit.RegisterButton
Guest.Limit.RemindLater
Guest.Convert.Title
Guest.Convert.Message
Guest.Convert.Yes
Guest.Convert.No
```

## Test Cases

```csharp
[Fact]
public async Task Guest_CanPlay_IfUnderLimit()
{
    // Arrange
    await _localStorage.SetItemAsync("guestGamesToday", 2);
    
    // Act
    var canPlay = await _guestService.CanPlayAsGuest();
    
    // Assert
    Assert.True(canPlay);
}

[Fact]
public async Task Guest_CannotPlay_IfLimitReached()
{
    // Arrange
    await _localStorage.SetItemAsync("guestGamesToday", 5);
    
    // Act
    var canPlay = await _guestService.CanPlayAsGuest();
    
    // Assert
    Assert.False(canPlay);
}

[Fact]
public async Task Guest_CounterResets_NextDay()
{
    // Arrange
    await _localStorage.SetItemAsync("guestGamesToday", 5);
    await _localStorage.SetItemAsync("guestLastDate", DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"));
    
    // Act
    var count = await _guestService.GetGamesPlayedToday();
    
    // Assert
    Assert.Equal(0, count);
}
```

## Analytics & Tracking

### Tracked Events
- `guest_game_started` - Když host začne hru
- `guest_game_completed` - Když host dokončí hru (5 slov)
- `guest_registration_cta_shown` - Zobrazení CTA modalu
- `guest_registration_cta_clicked` - Kliknutí na registraci z guest módu
- `guest_limit_reached` - Dosažení denního limitu
- `guest_convert_yes` - Převod progressu při registraci
- `guest_convert_no` - Odmítnutí převodu

### Konverze metriky
```
Guest Conversion Rate = 
    (Počet registrací z Guest módu / Počet začatých Guest her) * 100

Cíl: > 15% conversion rate
```

## Odhad implementace

| Část | Hodiny |
|------|--------|
| GuestWordsService | 2h |
| GuestSessionStorage | 2h |
| Limit checking | 1h |
| CTA Modals | 2h |
| Převod progressu | 2h |
| Testování | 2h |
| **Celkem** | **11h** |

## Rozdělení na FE a BE

### Pro MVP (Pure Frontend):
- **Backend:** 0h (žádný backend kód)
- **Frontend:** 11h (vše v Blazor + LocalStorage)

### Pro produkci (s API):
- **Backend:** 6h (GuestController, Rate limiting, Anonymous sessions)
- **Frontend:** 5h (adaptace pro API calls)

## Doporučení

Pro rychlý start doporučuji **Pure Frontend** přístup:
1. Rychlejší implementace (žádné BE změny)
2. Žádná zátěž na server
3. Okamžité načtení (žádné API volání)
4. Snadné testování

Limity:
- Zkušený uživatel může smazat LocalStorage a hrát znovu
- Ale to je OK - cílem je konverze, ne bezpečnost
