# Plán refaktoringu LexiQuest pro správné používání Tempo.Blazor

## Současný stav

### ✅ Co již funguje
- Tempo.Blazor je nainstalován a nakonfigurován (`AddTempoBlazor()`, `AddTempoFluentValidation()`)
- CSS Tempo.Blazor je načteno (`_content/Tempo.Blazor/css/tempo-blazor.css`)
- Některé stránky již používají Tempo.Blazor komponenty (Settings.razor)
- MainLayout používá `TmTopBar`, `TmSidebar`, `TmToastContainer`

### ❌ Problémy identifikovány
1. **Login.razor** a **Register.razor** používají standardní Blazor komponenty (`InputText`, `InputCheckbox`) místo Tempo.Blazor
2. **Dashboard.razor** má vlastní statické karty místo `TmStatCard`
3. **GameArena.razor** používá vlastní HTML inputy a tlačítka místo `TmTextInput` a `TmButton`
4. Mix globálního CSS (`wwwroot/css/app.css`) a scoped CSS (`.razor.css`)
5. Hardcoded texty v některých komponentách (např. `// Placeholder` komentáře v kódu)

---

## Cíle refaktoringu

1. **VŠECHNY formuláře** používají Tempo.Blazor komponenty (`TmTextInput`, `TmButton`, `TmValidatedField`, `TmFormSection`)
2. **ŽÁDNÉ hardcoded texty** - vše z `.resx` souborů
3. **Scoped CSS** - styly jen tam, kde je potřeba custom vizuál (herní prvky)
4. **Design tokeny** - používání Tempo.Blazor CSS proměnných
5. **Správná validace** - `TmValidatedField` s `FluentValidationValidator`
6. **Konzistentní UX** - stejné chování všech komponent

---

## Podrobný plán

### Fáze 1: Refaktoring Login a Register stránek

#### 1.1 Login.razor
**Současný stav:**
```razor
<div class="form-field">
    <label for="email">@L["Input.Email.Label"]</label>
    <InputText id="email" type="email" @bind-Value="model.Email" ... />
    <ValidationMessage For="@(() => model.Email)" />
</div>
<button type="submit" class="btn btn-primary btn-block">...</button>
```

**Cílový stav:**
```razor
<TmValidatedField 
    Label="@L["Input.Email.Label"]"
    @bind-Value="model.Email"
    Type="email"
    Placeholder="@L["Input.Email.Placeholder"]"
    Required="true"
    AutoComplete="email" />
<TmButton Type="ButtonType.Submit" 
    Variant="ButtonVariant.Primary" 
    Block="true"
    IsLoading="isLoading">
    @L["Button.Submit"]
</TmButton>
```

**Úkoly:**
- [ ] Nahradit `InputText` → `TmValidatedField`
- [ ] Nahradit `InputCheckbox` → `TmCheckbox`  
- [ ] Nahradit `<button>` → `TmButton`
- [ ] Nahradit alert div → `TmAlert`
- [ ] Přidat `TmCard` pro obalení formuláře
- [ ] Vytvořit scoped CSS jen pro layout (ne pro komponenty)

#### 1.2 Register.razor
- [ ] Stejné změny jako u Login
- [ ] Přidat `TmPasswordStrengthIndicator` pro heslo
- [ ] Použít `TmFormSection` pro seskupení polí

---

### Fáze 2: Refaktoring Dashboard

#### 2.1 Statistiky
**Současný stav:**
```razor
<div class="stat-card stat-xp">
    <span class="stat-icon">⭐</span>
    <div class="stat-info">
        <span class="stat-value">@stats.TotalXP</span>
        <span class="stat-label">@L["Stat.XP"]</span>
    </div>
</div>
```

**Cílový stav:**
```razor
<TmStatCard 
    Title="@L["Stat.XP"]" 
    Value="@stats.TotalXP.ToString()"
    Icon="@IconNames.Star" />
```

#### 2.2 Akční tlačítka
- [ ] Nahradit `<button class="btn btn-primary">` → `TmButton`
- [ ] Použít `TmCard` pro sekce

#### 2.3 Loading stav
- [ ] Použít `TmSkeleton` místo custom loading div

---

### Fáze 3: Refaktoring Game komponent

#### 3.1 GameArena.razor
**Zachovat custom styly pro:**
- Scrambled letter cards (herní design)
- Animace (dealIn, pulse)
- Feedback animace

**Nahradit Tempo.Blazor:**
- [ ] `<input class="answer-input">` → `TmTextInput`
- [ ] `<button class="btn-submit">` → `TmButton`
- [ ] Alert divy → `TmAlert` nebo `TmToastService`

#### 3.2 GameTimer.razor
- [ ] Zvážit použití `TmProgressBar` místo custom progress

#### 3.3 LivesIndicator, XpBar
- [ ] Zachovat custom vizuál (game-like)
- [ ] Použít CSS design tokeny pro barvy

---

### Fáze 4: CSS Refaktoring

#### 4.1 Globální CSS (wwwroot/css/app.css)
**Zachovat:**
- Blazor error UI styly
- Loading progress styly
- Validation základní styly (pro fallback)

**Odstranit:**
- Custom button styly (`.btn`, `.btn-primary`)
- Custom form styly (`.form-field`, `.form-input`)
- Custom card styly (`.stat-card`)

#### 4.2 Scoped CSS
**Vytvořit nové:**
- `Login.razor.css` - jen layout pozicování
- `Register.razor.css` - jen layout pozicování
- `GameArena.razor.css` - herní animace a specifické prvky

**Struktura scoped CSS:**
```css
/* Jen layout - používáme Tempo.Blazor komponenty */
.login-container {
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 100vh;
    background: var(--tm-color-bg-secondary);
}

.login-card {
    width: 100%;
    max-width: 400px;
    padding: var(--tm-spacing-6);
}
```

#### 4.3 Design Tokeny
**Používat CSS proměnné z Tempo.Blazor:**
```css
/* Místo hardcoded barev */
color: var(--tm-color-primary);
background: var(--tm-color-bg-secondary);
padding: var(--tm-spacing-4);
border-radius: var(--tm-radius-lg);
```

---

### Fáze 5: Resource soubory

#### 5.1 Přidat chybějící klíče
**Login.resx:**
```xml
<data name="Alert.ErrorTitle" xml:space="preserve">
  <value>Přihlášení selhalo</value>
</data>
<data name="Button.Loading" xml:space="preserve">
  <value>Přihlašuji...</value>
</data>
```

**Register.resx:**
```xml
<data name="PasswordStrength.Weak" xml:space="preserve">
  <value>Slabé heslo</value>
</data>
<data name="PasswordStrength.Strong" xml:space="preserve">
  <value>Silné heslo</value>
</data>
```

---

### Fáze 6: Validace

#### 6.1 Validátory
**Zajistit, že všechny validátory používají resources:**
```csharp
public class LoginModelValidator : AbstractValidator<LoginModel>
{
    public LoginModelValidator(IStringLocalizer<ValidationMessages> L)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(L["Validation.Email.Required"])
            .EmailAddress()
            .WithMessage(L["Validation.Email.Invalid"]);
    }
}
```

---

## Komponenty Tempo.Blazor - Přehled použití

### Formuláře
| Komponenta | Použití | Nahrazuje |
|------------|---------|-----------|
| `TmValidatedField` | Všechny textové inputy | `InputText` |
| `TmCheckbox` | Checkboxy | `InputCheckbox` |
| `TmSelect` | Dropdowny | `<select>` |
| `TmButton` | Všechna tlačítka | `<button>` |
| `TmFormSection` | Sekce formuláře | `<div class="section">` |
| `TmAlert` | Chybové zprávy | `<div class="alert">` |

### Layout
| Komponenta | Použití | Nahrazuje |
|------------|---------|-----------|
| `TmCard` | Karty obsahu | Custom `.card` |
| `TmStatCard` | Statistiky | Custom `.stat-card` |
| `TmSkeleton` | Loading stavy | Custom loading |
| `TmEmptyState` | Prázdné stavy | Custom empty |

### Feedback
| Komponenta | Použití | Nahrazuje |
|------------|---------|-----------|
| `TmToastService` | Notifikace | Alert divy |
| `TmSpinner` | Loading indikátor | Text "Loading..." |
| `TmProgressBar` | Progress | Custom progress |

---

## Checklist pro každou stránku

### Login.razor
- [ ] `TmCard` jako obal
- [ ] `TmValidatedField` pro email
- [ ] `TmValidatedField` pro heslo
- [ ] `TmCheckbox` pro "Zapamatovat si mě"
- [ ] `TmButton` pro submit
- [ ] `TmButton` pro Google login
- [ ] `TmAlert` pro chybové zprávy
- [ ] Scoped CSS jen pro layout
- [ ] Všechny texty z resources

### Register.razor
- [ ] `TmCard` jako obal
- [ ] `TmValidatedField` pro všechna pole
- [ ] `TmPasswordStrengthIndicator` pro heslo
- [ ] `TmCheckbox` pro podmínky
- [ ] `TmButton` pro submit
- [ ] Scoped CSS jen pro layout
- [ ] Všechny texty z resources

### Dashboard.razor
- [ ] `TmStatCard` pro statistiky
- [ ] `TmButton` pro akce
- [ ] `TmCard` pro sekce
- [ ] `TmSkeleton` pro loading

### GameArena.razor
- [ ] `TmTextInput` pro odpověď
- [ ] `TmButton` pro odeslání/přeskočení
- [ ] `TmAlert` nebo Toast pro feedback
- [ ] Zachovat custom styly pro letter cards

---

## Testování

### Po refaktoringu ověřit:
1. **Validace** - zobrazují se chyby správně?
2. **Loading stavy** - tlačítka se disabled při načítání?
3. **Responsivita** - vše funguje na mobilech?
4. **Tmavý režim** - všechny komponenty respektují téma?
5. **Přístupnost** - ARIA atributy správně nastaveny?

---

## Poznámky

### Co NEMĚNIT
- Herní vizuál (letter cards, combo badge, animace)
- Barevné schéma hry (gradienty, efekty)
- Animace (dealIn, pulse, shake)

### Používat Tempo.Blazor pro:
- Všechny formuláře
- Všechna tlačítka
- Všechny karty (kromě herních)
- Všechny inputy
- Feedback uživateli
