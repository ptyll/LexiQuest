# LexiQuest - Struktura Resource Souborů

## Pravidla organizace

1. **Žádné hardcoded texty** - Všechny uživatelské texty musí být v resource souborech
2. **Jedna stránka = jeden resource soubor** - Každá stránka má svůj vlastní .resx
3. **Hierarchická struktura** - Rozděleno podle komponent/pages/shared
4. **Konzistentní klíče** - Používáme PascalCase s tečkovou notací

## Struktura adresářů

```
LexiQuest.Blazor/
└── Resources/
    ├── Components/           # Komponenty používané na více místech
    ├── Pages/               # Jednotlivé stránky
    ├── Validation/          # Validace
    └── Shared/              # Layout, navigace, společné části

LexiQuest.Api/
└── Resources/
    ├── Validation/          # API validace
    ├── Email/              # Emailové šablony
    └── Errors/             # Error messages
```

## Konvence pojmenování klíčů

### Stránky
```
PageName.Element.Type.Detail

Příklady:
- Login.Title                    // "Přihlášení"
- Login.Button.Submit           // "Přihlásit se"
- Login.Input.Email.Placeholder // "Zadejte email"
- Login.Error.InvalidCredentials // "Nesprávné přihlašovací údaje"
```

### Komponenty
```
ComponentName.Context.Type

Příklady:
- GameArena.Timer.Remaining     // "Zbývá {0} sekund"
- StreakIndicator.Days.Count    // "{0} dní"
- StreakIndicator.Status.OnFire // "🔥 Držíš oheň!"
```

### Validace
```
Validation.FieldName.Rule

Příklady:
- Validation.Email.Required      // "Email je povinný"
- Validation.Email.Invalid       // "Zadejte platný email"
- Validation.Password.MinLength  // "Heslo musí mít alespoň {0} znaků"
```

### Notifikace/Toasty
```
Notification.Type.Action

Příklady:
- Notification.Success.XpEarned          // "Získal jsi {0} XP!"
- Notification.Warning.StreakEnding      // "Pozor! Streak končí za {0} hodin"
- Notification.Error.ConnectionLost      // "Spojení ztraceno"
```

## Přehled resource souborů

### LexiQuest.Blazor/Resources/Pages/

| Soubor | Popis | Počet klíčů (odhad) |
|--------|-------|---------------------|
| Index.resx | Landing page | 25 |
| Login.resx | Přihlášení | 20 |
| Register.resx | Registrace | 25 |
| Dashboard.resx | Hlavní dashboard | 40 |
| Game.resx | Herní obrazovka | 50 |
| Paths.resx | Cesty a levely | 35 |
| BossLevel.resx | Boss level speciální | 30 |
| Leagues.resx | Ligy a žebříček | 40 |
| Statistics.resx | Statistiky | 45 |
| Achievements.resx | Achievementy | 35 |
| Profile.resx | Profil uživatele | 30 |
| Premium.resx | Prémiový účet | 40 |
| Shop.resx | Obchod | 35 |
| Multiplayer.resx | Multiplayer | 45 |
| Settings.resx | Nastavení | 50 |
| DailyChallenge.resx | Denní výzva | 30 |
| PasswordReset.resx | Obnova hesla | 20 |
| NotFound.resx | 404 stránka | 10 |

### LexiQuest.Blazor/Resources/Components/

| Soubor | Popis | Počet klíčů (odhad) |
|--------|-------|---------------------|
| GameArena.resx | Herní aréna | 30 |
| StreakIndicator.resx | Streak ukazatel | 15 |
| XpBar.resx | XP lišta | 10 |
| LivesIndicator.resx | Životy | 15 |
| Leaderboard.resx | Žebříček | 20 |
| Heatmap.resx | Aktivitní heatmapa | 10 |
| Timer.resx | Časovač | 15 |
| HintButton.resx | Tlačítko nápovědy | 10 |
| WordDisplay.resx | Zobrazení slova | 15 |
| AchievementCard.resx | Karta achievementu | 15 |
| LeagueCard.resx | Karta ligy | 15 |
| PathNode.resx | Uzel cesty | 15 |
| BossModifiers.resx | Boss modifikátory | 20 |
| ShopItem.resx | Položka obchodu | 15 |
| AvatarSelector.resx | Výběr avatara | 15 |
| ComboDisplay.resx | Combo zobrazení | 10 |
| LevelComplete.resx | Dokončení levelu | 15 |
| GameOver.resx | Konec hry | 15 |
| Notifications.resx | Toast notifikace | 30 |

### LexiQuest.Blazor/Resources/Shared/

| Soubor | Popis | Počet klíčů (odhad) |
|--------|-------|---------------------|
| Navigation.resx | Navigace | 25 |
| Footer.resx | Patička | 10 |
| Loading.resx | Loading stavy | 15 |
| ErrorBoundary.resx | Error handling | 10 |
| ConfirmDialog.resx | Potvrzovací dialogy | 15 |

### LexiQuest.Blazor/Resources/Validation/

| Soubor | Popis | Počet klíčů (odhad) |
|--------|-------|---------------------|
| ValidationMessages.resx | Všechny validace | 60 |

### LexiQuest.Api/Resources/

| Soubor | Popis | Počet klíčů (odhad) |
|--------|-------|---------------------|
| Validation/ValidationMessages.resx | API validace | 40 |
| Email/WelcomeEmail.resx | Vítací email | 15 |
| Email/PasswordResetEmail.resx | Reset hesla | 10 |
| Email/StreakWarningEmail.resx | Varování streak | 10 |
| Email/LeagueResultsEmail.resx | Výsledky ligy | 15 |
| Errors/ErrorMessages.resx | Error kódy | 30 |

## Příklady obsahu resource souborů

### Pages/Login.resx
```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="Title" xml:space="preserve">
    <value>Přihlášení</value>
  </data>
  <data name="Subtitle" xml:space="preserve">
    <value>Vítej zpět! Pokračuj ve svém streaku.</value>
  </data>
  <data name="Input.Email.Label" xml:space="preserve">
    <value>Email</value>
  </data>
  <data name="Input.Email.Placeholder" xml:space="preserve">
    <value>tvuj@email.cz</value>
  </data>
  <data name="Input.Password.Label" xml:space="preserve">
    <value>Heslo</value>
  </data>
  <data name="Input.Password.Placeholder" xml:space="preserve">
    <value>••••••••</value>
  </data>
  <data name="Link.ForgotPassword" xml:space="preserve">
    <value>Zapomněl jsi heslo?</value>
  </data>
  <data name="Button.Submit" xml:space="preserve">
    <value>Přihlásit se</value>
  </data>
  <data name="Button.Google" xml:space="preserve">
    <value>Přihlásit přes Google</value>
  </data>
  <data name="Register.Prompt" xml:space="preserve">
    <value>Ještě nemáš účet?</value>
  </data>
  <data name="Register.Link" xml:space="preserve">
    <value>Zaregistruj se</value>
  </data>
  <data name="Error.InvalidCredentials" xml:space="preserve">
    <value>Nesprávný email nebo heslo</value>
  </data>
  <data name="Error.AccountLocked" xml:space="preserve">
    <value>Účet je dočasně zablokován. Zkus to za {0} minut.</value>
  </data>
  <data name="Loading.Text" xml:space="preserve">
    <value>Přihlašuji...</value>
  </data>
</root>
```

### Components/GameArena.resx
```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="Timer.Label" xml:space="preserve">
    <value>Čas</value>
  </data>
  <data name="Timer.Format" xml:space="preserve">
    <value>{0:D2}:{1:D2}</value>
  </data>
  <data name="Lives.Label" xml:space="preserve">
    <value>Životy</value>
  </data>
  <data name="Hint.Button" xml:space="preserve">
    <value>Nápověda</value>
  </data>
  <data name="Hint.Cost" xml:space="preserve">
    <value>-{0} XP</value>
  </data>
  <data name="Hint.NotAvailable" xml:space="preserve">
    <value>Nápověda není dostupná</value>
  </data>
  <data name="Answer.Placeholder" xml:space="preserve">
    <value>Napiš slovo...</value>
  </data>
  <data name="Answer.Submit" xml:space="preserve">
    <value>Potvrdit</value>
  </data>
  <data name="Answer.Skip" xml:space="preserve">
    <value>Přeskočit (-1 život)</value>
  </data>
  <data name="Combo.Multiplier" xml:space="preserve">
    <value>x{0} COMBO!</value>
  </data>
  <data name="Speed.Bonus" xml:space="preserve">
    <value>+{0} XP rychlost!</value>
  </data>
  <data name="Level.Progress" xml:space="preserve">
    <value>Slovo {0} z {1}</value>
  </data>
</root>
```

### Components/StreakIndicator.resx
```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <data name="Days.Singular" xml:space="preserve">
    <value>{0} den</value>
  </data>
  <data name="Days.Plural2-4" xml:space="preserve">
    <value>{0} dny</value>
  </data>
  <data name="Days.Plural5Plus" xml:space="preserve">
    <value>{0} dní</value>
  </data>
  <data name="Status.OnFire" xml:space="preserve">
    <value>🔥 Držíš oheň!</value>
  </data>
  <data name="Status.AtRisk" xml:space="preserve">
    <value>⚠️ Streak končí za {0}h</value>
  </data>
  <data name="Status.Frozen" xml:space="preserve">
    <value>❄️ Zamrzlé</value>
  </data>
  <data name="Tooltip.Description" xml:space="preserve">
    <value>Splň alespoň 1 level denně pro udržení streaku!</value>
  </data>
  <data name="Shield.Available" xml:space="preserve">
    <value>🛡️ Streak Shield k dispozici</value>
  </data>
  <data name="Shield.Used" xml:space="preserve">
    <value>🛡️ Shield použit tento týden</value>
  </data>
</root>
```

## Použití v kódu

### Blazor komponenta
```razor
@inject IStringLocalizer<Login> L

<h1>@L["Title"]</h1>
<p>@L["Subtitle"]</p>

<input placeholder="@L["Input.Email.Placeholder"]" />
<button>@L["Button.Submit"]</button>

@if (hasError)
{
    <p class="error">@L["Error.InvalidCredentials"]</p>
}
```

### Interpolace hodnot
```razor
<p>@string.Format(L["Days.Plural5Plus"], streakDays)</p>
```

### Pluralizace (volitelně s Humanizer nebo vlastním helperem)
```csharp
public string GetStreakText(int days)
{
    var key = days switch
    {
        1 => "Days.Singular",
        >= 2 and <= 4 => "Days.Plural2-4",
        _ => "Days.Plural5Plus"
    };
    return string.Format(L[key], days);
}
```

## Lokalizace (budoucí rozšíření)

Resource soubory jsou připraveny pro vícejazyčnost:
- `Login.resx` - výchozí (čeština)
- `Login.en.resx` - angličtina
- `Login.de.resx` - němčina

Přepínání jazyka v URL: `/cs/dashboard` nebo `/en/dashboard`

## Kontrolní seznam při vytváření nové stránky

- [ ] Vytvořit `Pages/NazevStranky.resx`
- [ ] Všechny texty použít přes `@inject IStringLocalizer`
- [ ] Žádné string literály v .razor souborech
- [ ] Otestovat pluralizaci pokud používá čísla
- [ ] Zkontrolovat překlepy v resource souboru
