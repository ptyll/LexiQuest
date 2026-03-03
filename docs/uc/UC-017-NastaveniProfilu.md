# UC-017: Nastavení profilu

## Popis
Správa uživatelského profilu, preferencí a účtu.

## Sekce nastavení

### 1. Profilová fotka/Avatar
- Výběr z předdefinovaných avatarů
- Nahrání vlastní fotky (Premium)
- Avatar frame podle achievementů

### 2. Uživatelské jméno
- Zobrazení aktuálního
- Možnost změny (1x za měsíc zdarma)
- Kontrola unikátnosti

### 3. Email a heslo
- Zobrazení emailu (maskovaného)
- Změna hesla
- Změna emailu (s verifikací)

### 4. Notifikace
- Push notifikace (ANO/NE)
- Email notifikace (ANO/NE)
- Streak upozornění (čas)
- Liga aktualizace
- Denní výzva

### 5. Zobrazení
- Téma (Světlé/Tmavé/Auto)
- Jazyk (CS/EN/DE)
- Animace (ANO/NE)
- Zvuky (ANO/NE)

### 6. Soukromí
- Viditelnost profilu (Veřejný/Přátelé/Soukromý)
- Viditelnost statistik
- Ukazovat v žebříčcích

### 7. Účet
- Deaktivace účtu
- Smazání účtu a dat (GDPR)
- Export dat

## DTOs

```csharp
public record UserProfile(
    Guid Id,
    string Username,
    string Email,
    string AvatarUrl,
    string? FrameUrl,
    DateTime CreatedAt,
    UserPreferences Preferences
);

public record UpdateProfileRequest(
    string? Username,
    string? AvatarUrl,
    UserPreferences? Preferences
);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
);

public record UserPreferences(
    bool PushNotifications,
    bool EmailNotifications,
    TimeSpan? StreakReminderTime,
    ThemePreference Theme,
    LanguagePreference Language,
    bool EnableAnimations,
    bool EnableSounds,
    PrivacySettings Privacy
);

public record PrivacySettings(
    ProfileVisibility ProfileVisibility,
    bool ShowStatsPublic,
    bool AppearInLeaderboards
);
```

## Resource klíče

```
Settings.Title
Settings.Profile.Title
Settings.Profile.Avatar
Settings.Profile.Username
Settings.Profile.Email
Settings.Profile.ChangePassword
Settings.Notifications.Title
Settings.Notifications.Push
Settings.Notifications.Email
Settings.Notifications.StreakTime
Settings.Display.Title
Settings.Display.Theme
Settings.Display.Language
Settings.Display.Animations
Settings.Display.Sounds
Settings.Privacy.Title
Settings.Privacy.ProfileVisibility
Settings.Privacy.ShowStats
Settings.Privacy.Leaderboards
Settings.Account.Title
Settings.Account.Deactivate
Settings.Account.Delete
Settings.Account.ExportData
Settings.Save.Button
Settings.Success.Saved
```

## Odhad: 10h
