# Fáze 4: Premium Features (Týden 6-7)

> **Cíl:** Prémiový účet (Stripe), streak ochrana, obchod s kosmickými předměty, vlastní slovníky
> **Závislost:** Fáze 2 (achievementy, streak), Fáze 3 (guest conversion)
> **Tempo.Blazor komponenty:** TmCard, TmButton, TmBadge, TmModal, TmIcon, TmProgressBar, TmToggle, TmTabs, TmTabPanel, TmChip, TmChipGroup, TmTextInput, TmTextArea, TmSelect, TmFormField, TmFormSection, TmAlert, TmEmptyState, TmFileDropZone, TmDataTable, TmTooltip, FluentValidationValidator, ToastService
> **Status:** ✅ 100% hotovo - všechny úkoly implementovány včetně Stripe integrace, Shield Management UI, API Controller Tests, Hangfire Jobs

---

## ⚠️ KRITICKÁ PRAVIDLA

- **TDD:** Test FIRST → RED → GREEN → REFACTOR
- **Žádné hardcoded texty** → vše z `.resx`
- **FluentValidation** na FE i BE s lokalizací
- **DTOs** v `LexiQuest.Shared`
- **HTTP status kódy** místo wrapper tříd
- **Produkční kód** od prvního řádku
- **Payment security:** PCI compliance, žádné ukládání karet na serveru

---

## T-400: UC-018 Premium účet - Backend

### T-400.1: Domain Entities (TDD) - ✅ HOTOVÉ
- [x] **TEST:** `Subscription_Create_SetsDefaultValues` → RED → GREEN
- [x] **TEST:** `Subscription_IsActive_ReturnsTrueBeforeExpiry` → RED → GREEN
- [x] **TEST:** `Subscription_IsActive_ReturnsFalseAfterExpiry` → RED → GREEN
- [x] **TEST:** `Subscription_Cancel_SetsCancelledAt` → RED → GREEN
- [x] Vytvořit `Subscription` entitu (Id, UserId, Plan, StartedAt, ExpiresAt, CancelledAt, StripeSubscriptionId, Status)
- [x] Vytvořit `SubscriptionPlan` enum (Monthly, Yearly, Lifetime)
- [x] Vytvořit `SubscriptionStatus` enum (Active, Cancelled, Expired, PastDue)
- [x] EF Core konfigurace + migrace
- [x] **GREEN:** Testy prochází

### T-400.2: Stripe Integration Setup - ✅ HOTOVÉ
- [x] Přidat Stripe NuGet balíček (`Stripe.net` v47.4.0) do `LexiQuest.Infrastructure`
- [x] Vytvořit `StripeSettings` POCO (ApiKey, WebhookSecret, MonthlyPriceId, YearlyPriceId, LifetimePriceId)
- [x] Nastavit Stripe API key v appsettings.json (environment variables v produkci)
- [x] Konfigurovat StripeSettings v Program.cs (`services.Configure<StripeSettings>`)
- [x] Implementovat `StripeSubscriptionService` s `CreateCheckoutSessionAsync()`
- [x] Implementovat webhook handlery pro Stripe eventy
- [x] Připravit ceny pro Stripe Dashboard:
  - Monthly: 99 CZK/měsíc
  - Yearly: 899 CZK/rok (25% sleva)
  - Lifetime: 2 499 CZK jednorázově

### T-400.3: SubscriptionService (TDD) - ✅ HOTOVÉ
- [x] **TEST:** `SubscriptionService_CreateCheckout_Monthly_ReturnsStripeUrl` → RED → GREEN
- [x] **TEST:** `SubscriptionService_CreateCheckout_Yearly_ReturnsStripeUrl` → RED → GREEN
- [x] **TEST:** `SubscriptionService_CreateCheckout_Lifetime_ReturnsStripeUrl` → RED → GREEN
- [x] **TEST:** `SubscriptionService_ActivateSubscription_SetsUserPremium` → RED → GREEN
- [x] **TEST:** `SubscriptionService_CancelSubscription_SetsCancelledAt` → RED → GREEN
- [x] **TEST:** `SubscriptionService_CheckExpired_DeactivatesPremium` → RED → GREEN
- [x] **TEST:** `SubscriptionService_GetStatus_ReturnsCurrentPlan` → RED → GREEN
- [x] Vytvořit `ISubscriptionService` interface
- [x] Vytvořit DTOs: `CreateCheckoutRequest` (Plan), `CheckoutResponse` (StripeCheckoutUrl), `SubscriptionStatusDto`
- [x] Implementovat `SubscriptionService.CreateCheckoutSession()` → Stripe Checkout Session s test/produkčním režimem
- [x] Implementovat `SubscriptionService.ActivateSubscription()` → po úspěšné platbě
- [x] Implementovat `SubscriptionService.CancelSubscription()` → zruší na konci období
- [x] Implementovat `SubscriptionService.CheckExpiredSubscriptions()` → Hangfire job
- [x] **GREEN:** Všechny testy prochází (8 testů)

### T-400.4: Stripe Webhook Handler (TDD) - ✅ HOTOVÉ
- [x] **TEST:** `WebhookHandler_CheckoutCompleted_ActivatesSubscription` → RED → GREEN
- [x] **TEST:** `WebhookHandler_InvoicePaid_ExtendsSubscription` → RED → GREEN
- [x] **TEST:** `WebhookHandler_InvoiceFailed_MarksAsPastDue` → RED → GREEN
- [x] **TEST:** `WebhookHandler_SubscriptionCancelled_DeactivatesPremium` → RED → GREEN
- [x] **TEST:** `WebhookHandler_InvalidSignature_Returns400` → RED → GREEN
- [x] Vytvořit `POST /api/v1/webhooks/stripe` endpoint (`WebhookController`)
- [x] Implementovat zpracování webhooků pomocí Stripe SDK (`EventUtility.ConstructEvent`)
- [x] Ověřovat Stripe webhook signature v produkčním režimu
- [x] Zpracovat eventy: `checkout.session.completed`, `invoice.paid`, `invoice.payment_failed`, `customer.subscription.deleted`
- [x] **GREEN:** Testy prochází

### T-400.5: Premium Feature Flags (TDD) - ✅ HOTOVÉ
- [x] **TEST:** `PremiumFeatureService_IsPremium_ReturnsTrueForActiveSubscription` → RED → GREEN
- [x] **TEST:** `PremiumFeatureService_HasFeature_ReturnsTrueForPremiumFeatures` → RED → GREEN
- [x] **TEST:** `PremiumFeatureService_HasFeature_ReturnsFalseForFreeUser` → RED → GREEN
- [x] Vytvořit `IPremiumFeatureService` interface
- [x] Vytvořit `PremiumFeature` enum (NoAds, StreakFreeze, StreakShield, DoubleXPWeekends, ExclusivePaths, CustomDictionaries, DetailedStats, CustomAvatar, DiamondLeague, TeamCreation)
- [x] Implementovat feature checking per user
- [x] **GREEN:** Testy prochází (22 testů)

### T-400.6: Subscription Expiration Job - ✅ HOTOVÉ
- [x] **TEST:** `SubscriptionExpirationJob_NoExpired_DoesNothing` → RED → GREEN
- [x] **TEST:** `SubscriptionExpirationJob_Expired_MarksAsExpired` → RED → GREEN
- [x] **TEST:** `SubscriptionExpirationJob_Expired_DisablesUserPremium` → RED → GREEN
- [x] Vytvořit `ISubscriptionExpirationJob` interface
- [x] Implementovat `SubscriptionExpirationJob.CheckExpiredSubscriptionsAsync()`
- [x] Přidat `GetExpiredActiveSubscriptionsAsync` do repository
- [x] **GREEN:** Testy prochází (6 testů)

### T-400.6: Premium Endpoints - ✅ HOTOVÉ
- [x] Vytvořit `POST /api/v1/premium/checkout` (vrací Stripe checkout URL)
- [x] Vytvořit `GET /api/v1/premium/status` (vrací subscription status)
- [x] Vytvořit `POST /api/v1/premium/cancel` (zruší subscription)
- [x] Vytvořit `GET /api/v1/premium/features` (vrací seznam premium features)
- [x] Vytvořit `PremiumController` s dependency injection
- [x] Implementovat autentizaci a autorizaci endpointů
- [x] **GREEN:** Testy prochází (6 testů)

---

## T-401: UC-018 Premium účet - Frontend

### T-401.1: Premium Landing Page (Tempo.Blazor) - ✅ HOTOVÉ
- [x] **TEST (bUnit):** `PremiumPage_Renders_3PricingCards` → RED → GREEN
- [x] **TEST (bUnit):** `PremiumPage_BestValue_HasHighlightBadge` → RED → GREEN
- [x] **TEST (bUnit):** `PremiumPage_ClickCheckout_RedirectsToStripe` → RED → GREEN
- [x] Vytvořit `Premium.razor` (`@page "/premium"`)
- [x] `@inject IStringLocalizer<Premium> L`
- [x] **Hero sekce**: Crown animace, titulek "LexiQuest Premium"
- [x] **Feature groups** (4× `TmCard`):
  1. Game Features: No ads, Streak Freeze, Shield, 2x XP weekends, exclusive paths, custom dictionaries
  2. Analytics: Detailed stats, export (CSV/JSON), history, friend comparison
  3. Personalization: Custom avatar, exclusive avatars/frames/themes, colors
  4. Multiplayer: Diamond league, tournaments, team creation
  - Každý feature s `TmIcon` (check) a popisem z .resx
- [x] **Pricing Cards** (3× `TmCard`):
  - Monthly: `TmCard` - "99 Kč/měsíc", `TmButton` "Předplatit"
  - Yearly: `TmCard` (Elevated, gold border) - `TmBadge` "⭐ BEST VALUE", "899 Kč/rok", přeškrtnutá cena "1 188 Kč", `TmButton Variant="Primary"` "Předplatit"
  - Lifetime: `TmCard` - "2 499 Kč jednorázově", `TmButton` "Koupit"
- [x] Payment methods info: Stripe/PayPal ikony
- [x] Cancel anytime notice
- [x] Checkout flow: klik → volání API → redirect na Stripe Checkout → callback
- [x] **GREEN:** Testy prochází

### T-401.2: Premium Badge v profilu - ✅ HOTOVÉ
- [x] Vytvořit `UserProfileBadge.razor` komponentu
- [x] Přidat `TmBadge` "⭐ Premium" do profilu a navigace pro premium uživatele
- [x] Podmíněné zobrazení premium features v celé aplikaci
- [x] Free user: `TmTooltip` "Premium funkce" s `TmIcon` (lock) na zamčených features

### T-401.3: Checkout Success/Cancel Pages - ✅ HOTOVÉ
- [x] Vytvořit `CheckoutSuccess.razor` (`@page "/premium/success"`)
  - `TmCard` s checkmark ikonou, "Děkujeme za Premium!", přehled features
  - `TmButton` "Zpět na dashboard" a "Nastavení"
- [x] Vytvořit `CheckoutCancel.razor` (`@page "/premium/cancel"`)
  - `TmCard` s X ikonou, "Platba zrušena", info alert
  - `TmButton` "Zkusit znovu" a "Zpět na dashboard"
- [x] Přidat resource klíče do `CheckoutSuccess.resx` a `CheckoutCancel.resx`

---

## T-402: UC-012 Streak Shield a Freeze

### T-402.1: Backend - Shield/Freeze Service (TDD) - ✅ HOTOVÉ
- [x] **TEST:** `StreakProtectionService_ActivateShield_Free_1PerMonth` → RED → GREEN
- [x] **TEST:** `StreakProtectionService_ActivateShield_Premium_1PerWeek` → RED → GREEN
- [x] **TEST:** `StreakProtectionService_ActivateShield_AlreadyActive_ReturnsForbidden` → RED → GREEN
- [x] **TEST:** `StreakProtectionService_ActivateShield_NoShieldsRemaining_Returns400` → RED → GREEN
- [x] **TEST:** `StreakProtectionService_AutoFreeze_Premium_ProtectsStreak` → RED → GREEN
- [x] **TEST:** `StreakProtectionService_AutoFreeze_Free_DoesNotProtect` → RED → GREEN
- [x] **TEST:** `StreakProtectionService_AutoFreeze_AlreadyUsedThisWeek_DoesNotProtect` → RED → GREEN
- [x] **TEST:** `StreakProtectionService_PurchaseShield_3For500Coins_DeductsCoins` → RED → GREEN
- [x] **TEST:** `StreakProtectionService_EmergencyShield_Premium_300Coins` → RED → GREEN
- [x] Vytvořit `IStreakProtectionService` interface
- [x] Vytvořit `StreakProtection` entitu (UserId, ShieldsRemaining, FreezeUsedThisWeek, LastShieldActivatedAt)
- [x] Vytvořit DTOs: `StreakProtectionDto`, `ActivateShieldRequest`, `PurchaseShieldsRequest`
- [x] EF Core konfigurace + migrace
- [x] Rozšířit `StreakService` o Shield/Freeze logic
- [x] Implementovat auto-freeze v streak check (pokud premium a miss)
- [x] **GREEN:** Všechny testy prochází

### T-402.2: Backend - Shield Endpoints - ✅ HOTOVÉ

## T-402.3: Shield Management UI (Tempo.Blazor) - ✅ HOTOVÉ
- [x] **TEST (bUnit):** `StreakIndicator_WithShieldActive_ShowsShieldIcon` → RED → GREEN
- [x] **TEST (bUnit):** `StreakIndicator_ClickActivateShield_InvokesCallback` → RED → GREEN
- [x] **TEST (bUnit):** `StreakIndicator_WithFreezeAvailable_ShowsFreezeBadge` → RED → GREEN
- [x] Rozšířit `StreakIndicator.razor` o Shield Management UI
- [x] Přidat parametry: `ShieldProtection`, `IsPremium`, `OnActivateShield`, `OnBuyShields`
- [x] Zobrazit aktivní štít, dostupné štíty, countdown do dalšího štítu
- [x] Přidat Freeze badge pro premium uživatele
- [x] Vytvořit `StreakProtectionDto` v Shared projektu
- [x] Přidat resource klíče do `StreakIndicator.resx`
- [x] **GREEN:** Testy prochází
- [x] Vytvořit `POST /api/v1/streak/shield/activate` (aktivuje shield)
- [x] Vytvořit `POST /api/v1/streak/shield/purchase` (koupí shieldy za coiny)
- [x] Vytvořit `GET /api/v1/streak/protection` (vrací stav shieldů/freeze)

### T-402.3: Frontend - Shield Management UI (Tempo.Blazor) - ✅ HOTOVÉ
- [x] **TEST (bUnit):** `ShieldManagement_Renders_ShieldCount` → RED → GREEN
- [x] **TEST (bUnit):** `ShieldManagement_ActivateShield_CallsApi` → RED → GREEN
- [x] **TEST (bUnit):** `ShieldManagement_PurchaseShield_ShowsConfirmDialog` → RED → GREEN
- [x] Rozšířit `StreakIndicator.razor` o shield info:
  - `TmBadge` "🛡️ Shield k dispozici" / "🛡️ Shield použit"
  - `TmTooltip` s detaily
- [x] Vytvořit `ShieldManagement.razor` modal/drawer:
  - `TmDrawer` (Right) s shield management
  - Aktuální shieldy: count s `TmIcon` (shield)
  - `TmButton` "Aktivovat Shield" (Primary)
  - Nákup: `TmCard` "3 shieldy za 500 mincí", `TmButton` "Koupit"
  - Emergency: `TmCard` "Okamžitý shield za 300 mincí" (Premium only)
  - Freeze status: `TmBadge` "❄️ Freeze aktivní" / "❄️ Freeze dostupný"
- [x] Přidat resource klíče pro shield management
- [x] **GREEN:** Testy prochází (10 testů)

---

## T-403: UC-019 Obchod

### T-403.1: Domain Entities (TDD) - ✅ HOTOVÉ
- [x] **TEST:** `ShopItem_Create_SetsProperties` → RED → GREEN
- [x] **TEST:** `UserInventory_AddItem_UpdatesInventory` → RED → GREEN
- [x] **TEST:** `UserInventory_EquipItem_SetsAsActive` → RED → GREEN
- [x] **TEST:** `UserInventory_HasItem_ReturnsTrueForOwned` → RED → GREEN
- [x] Vytvořit `ShopItem` entitu (Id, Name, Category, Price, Rarity, ImageUrl, IsPremiumOnly, IsLimited, AvailableUntil)
- [x] Vytvořit `UserInventoryItem` entitu (UserId, ShopItemId, PurchasedAt, IsEquipped)
- [x] Vytvořit `ShopCategory` enum (Avatar, Frame, Theme, Boost)
- [x] Vytvořit `ItemRarity` enum (Common, Rare, Epic, Legendary)
- [x] EF Core konfigurace + migrace
- [x] Seed data: avatary, frames, themes, boosty s cenami
- [x] **GREEN:** Testy prochází

### T-403.2: InventoryService (TDD) - ✅ HOTOVÉ
- [x] **TEST:** `InventoryService_Purchase_DeductsCoins` → RED → GREEN
- [x] **TEST:** `InventoryService_Purchase_InsufficientCoins_Returns400` → RED → GREEN
- [x] **TEST:** `InventoryService_Purchase_AlreadyOwned_Returns409` → RED → GREEN
- [x] **TEST:** `InventoryService_Purchase_PremiumOnly_FreeUser_Returns403` → RED → GREEN
- [x] **TEST:** `InventoryService_Equip_SetsItemAsActive` → RED → GREEN
- [x] **TEST:** `InventoryService_Equip_UnequipsPreviousInCategory` → RED → GREEN
- [x] **TEST:** `InventoryService_GetInventory_ReturnsOwnedItems` → RED → GREEN
- [x] **TEST:** `InventoryService_GetShopItems_ReturnsAllWithOwnedStatus` → RED → GREEN
- [x] Vytvořit `IInventoryService` interface
- [x] Vytvořit DTOs: `ShopItemDto`, `UserInventoryDto`, `PurchaseRequest`, `PurchaseResult`, `EquipItemRequest`
- [x] Implementovat `InventoryService` s atomickou transakcí (UoW)
- [x] **GREEN:** Všechny testy prochází

### T-403.3: CoinService (TDD) - ✅ HOTOVÉ
- [x] **TEST:** `CoinService_EarnCoins_LevelComplete_10Coins` → RED → GREEN
- [x] **TEST:** `CoinService_EarnCoins_BossLevel_50Coins` → RED → GREEN
- [x] **TEST:** `CoinService_EarnCoins_DailyChallenge_20Coins` → RED → GREEN
- [x] **TEST:** `CoinService_EarnCoins_Achievement_50to200Coins` → RED → GREEN
- [x] **TEST:** `CoinService_SpendCoins_DeductsBalance` → RED → GREEN
- [x] **TEST:** `CoinService_SpendCoins_InsufficientBalance_Returns400` → RED → GREEN
- [x] Vytvořit `ICoinService` interface (`EarnCoinsAsync`, `SpendCoinsAsync`, `GetBalanceAsync`, `GetTransactionHistoryAsync`)
- [x] Implementovat `CoinService` s `CoinTransactionType` enum
- [x] Rozšířit `User` entitu o `CoinBalance`, `StripeCustomerId`, `CoinTransactions`
- [x] **GREEN:** Všechny testy prochází (12 testů)

### T-403.4: Shop Endpoints - ✅ HOTOVÉ
- [x] Vytvořit `GET /api/v1/shop/items` (s filtrem dle category)
- [x] Vytvořit `GET /api/v1/shop/items/{id}` (detail položky)
- [x] Vytvořit `POST /api/v1/shop/purchase` (koupí položku)
- [x] Vytvořit `POST /api/v1/shop/equip` (nasadí položku)
- [x] Vytvořit `GET /api/v1/users/me/inventory` (inventář uživatele)
- [x] Vytvořit `GET /api/v1/users/me/coins` (balance mincí)

### T-403.5: Frontend - Shop Page (Tempo.Blazor) - ✅ HOTOVÉ
- [x] **TEST (bUnit):** `ShopPage_Renders_CategoryTabs` → RED → GREEN
- [x] **TEST (bUnit):** `ShopPage_Renders_ItemCards` → RED → GREEN
- [x] **TEST (bUnit):** `ShopPage_Purchase_DeductsCoins` → RED → GREEN
- [x] Vytvořit `Shop.razor` (`@page "/shop"`)
- [x] `@inject IStringLocalizer<Shop> L`
- [x] **Header**: Balance mincí (`TmBadge` ⭐ + počet), `TmButton` "Koupit mince"
- [x] **Category tabs**: `TmTabs` + `TmTabPanel` (Avatary, Rámečky, Témata, Boosty)
- [x] **Item grid**: responsivní grid (4 col desktop, 2 col tablet, 1 col mobile)
- [x] **GREEN:** Testy prochází

### T-403.6: Frontend - ShopItemCard komponenta (Tempo.Blazor) - ✅ HOTOVÉ
- [x] **TEST (bUnit):** `ShopItemCard_Owned_ShowsCheckmark` → RED → GREEN
- [x] **TEST (bUnit):** `ShopItemCard_Available_ShowsPrice` → RED → GREEN
- [x] **TEST (bUnit):** `ShopItemCard_PremiumOnly_ShowsLock` → RED → GREEN
- [x] Vytvořit `ShopItemCard.razor`
- [x] Owned state: `TmCard` se zelený checkmark `TmIcon`, `TmButton` "Nasadit" / "Nasazeno"
- [x] Available state: `TmCard` s cenou (⭐ coins), `TmButton Variant="Primary"` "Koupit"
- [x] Premium Only state: `TmCard` s greyscale, `TmIcon` (lock) overlay, `TmBadge` "🔒 Premium"
- [x] Rarity indikátor: `TmBadge` s barvou dle rarity (Common=šedá, Rare=modrá, Epic=fialová, Legendary=zlatá)
- [x] Hover: scale 1.02, shadow
- [x] **Purchase confirm**: `TmModal` s potvrzením ("Koupit {item} za {price} mincí?")
- [x] **GREEN:** Testy prochází

---

## T-404: UC-022 Vlastní slovníky (Premium)

### T-404.1: Domain Entities (TDD) - ✅ HOTOVÉ
- [x] **TEST:** `CustomDictionary_Create_SetsOwner` → RED → GREEN
- [x] **TEST:** `CustomDictionary_AddWord_IncreasesCount` → RED → GREEN
- [x] **TEST:** `CustomDictionary_AddWord_Max100_ThrowsOverLimit` → RED → GREEN
- [x] **TEST:** `DictionaryWord_Validate_Length3to20` → RED → GREEN
- [x] **TEST:** `DictionaryWord_Validate_InvalidChars_ThrowsError` → RED → GREEN
- [x] Vytvořit `CustomDictionary` entitu (Id, UserId, Name, Description, IsPublic, WordCount, DownloadCount, CreatedAt)
- [x] Vytvořit `DictionaryWord` entitu (Id, DictionaryId, Word, Difficulty)
- [x] EF Core konfigurace + migrace
- [x] **GREEN:** Testy prochází

### T-404.2: DictionaryService (TDD) - ✅ HOTOVÉ
- [x] **TEST:** `DictionaryService_Create_PremiumUser_Success` → RED → GREEN
- [x] **TEST:** `DictionaryService_Create_FreeUser_Returns403` → RED → GREEN
- [x] **TEST:** `DictionaryService_Create_Max10Dictionaries_Returns400` → RED → GREEN
- [x] **TEST:** `DictionaryService_AddWord_ValidWord_Success` → RED → GREEN
- [x] **TEST:** `DictionaryService_AddWord_InvalidLength_Returns400` → RED → GREEN
- [x] **TEST:** `DictionaryService_AddWord_DuplicateInDictionary_Returns409` → RED → GREEN
- [x] **TEST:** `DictionaryService_ImportCSV_ParsesCorrectly` → RED → GREEN
- [x] **TEST:** `DictionaryService_ImportTXT_ParsesCorrectly` → RED → GREEN
- [x] **TEST:** `DictionaryService_ImportJSON_ParsesCorrectly` → RED → GREEN
- [x] **TEST:** `DictionaryService_Import_ExceedsMax100_Returns400` → RED → GREEN
- [x] **TEST:** `DictionaryService_Delete_OwnerOnly` → RED → GREEN
- [x] **TEST:** `DictionaryService_GetPublic_ReturnsPublicDictionaries` → RED → GREEN
- [x] **TEST:** `DictionaryService_StartGameWithCustom_UsesCustomWords` → RED → GREEN
- [x] Vytvořit `IDictionaryService` interface
- [x] Vytvořit DTOs: `CustomDictionaryDto`, `DictionaryWordDto`, `CreateDictionaryRequest`, `ImportWordsRequest`, `ImportResult`
- [x] Vytvořit validátory: `CreateDictionaryValidator`, `AddWordValidator` s lokalizací
- [x] Implementovat `DictionaryService` s import parsery (CSV, TXT, JSON)
- [x] **GREEN:** Všechny testy prochází

### T-404.3: Dictionary Endpoints - ✅ HOTOVÉ
- [x] Vytvořit `GET /api/v1/dictionaries` (moje slovníky)
- [x] Vytvořit `POST /api/v1/dictionaries` (vytvoří slovník)
- [x] Vytvořit `GET /api/v1/dictionaries/{id}` (detail s words)
- [x] Vytvořit `PUT /api/v1/dictionaries/{id}` (upraví slovník)
- [x] Vytvořit `DELETE /api/v1/dictionaries/{id}` (smaže slovník)
- [x] Vytvořit `POST /api/v1/dictionaries/{id}/words` (přidá slovo)
- [x] Vytvořit `DELETE /api/v1/dictionaries/{id}/words/{wordId}` (smaže slovo)
- [x] Vytvořit `POST /api/v1/dictionaries/{id}/import` (import CSV/TXT/JSON)
- [x] Vytvořit `GET /api/v1/dictionaries/public` (veřejné slovníky)
- [x] Vytvořit `POST /api/v1/game/start` rozšířit o CustomDictionaryId parametr

### T-404.4: Frontend - DictionaryBuilder Page (Tempo.Blazor) - ✅ HOTOVÉ
- [x] **TEST (bUnit):** `DictionaryBuilder_Renders_DictionaryList` → RED → GREEN
- [x] **TEST (bUnit):** `DictionaryBuilder_CreateDictionary_ShowsModal` → RED → GREEN
- [x] **TEST (bUnit):** `DictionaryBuilder_ImportFile_ParsesAndAdds` → RED → GREEN
- [x] Vytvořit `Dictionaries.razor` (`@page "/dictionaries"`)
- [x] `@inject IStringLocalizer<Dictionaries> L`
- [x] **Dictionary list**: `TmCard` pro každý slovník s:
  - Název, popis, počet slov, `TmBadge` (Public/Private)
  - `TmButton` "Upravit" / "Smazat" / "Hrát"
- [x] **Create modal**: `TmModal` s `TmFormField` + `TmTextInput` pro název, `TmTextArea` pro popis, `TmToggle` pro veřejnost
- [x] **Dictionary detail page** (`@page "/dictionaries/{id}"`):
  - Word list: `TmDataTable` se slovy (Word, Difficulty, Actions)
  - Add word: `TmFormField` + `TmTextInput` + `TmSelect` (difficulty) + `TmButton` "Přidat"
  - Import: `TmFileDropZone` pro CSV/TXT/JSON upload
  - `<FluentValidationValidator />`
- [x] **Empty state**: `TmEmptyState` "Zatím žádné slovníky" s CTA "Vytvořit první"
- [x] Free user view: `TmAlert` "Premium funkce" s link na `/premium`
- [x] **GREEN:** Testy prochází

---

## T-405: API Controller Tests - ✅ HOTOVÉ
- [x] `PremiumControllerTests` - Testy pro PremiumController (6 testů)
- [x] `ShopControllerTests` - Testy pro ShopController
- [x] `StreakProtectionControllerTests` - Testy pro StreakProtectionController
- [x] `DictionaryControllerTests` - Rozšíření existujících testů

---

## T-406: Infrastructure & Jobs - ✅ HOTOVÉ
- [x] Hangfire job pro kontrolu expirovaných subscription (`SubscriptionExpirationJob`)
- [x] Stripe webhook endpoint security (signature verification)
- [x] Customer-Stripe ID mapping (Uživatel.StripeCustomerId)
- [x] Implementace webhook handlerů v `WebhookController`
- [x] `StripeSubscriptionService` pro komunikaci se Stripe API

---

## Shrnutí implementace

### ✅ Hotovo (100%)
- ✅ Všechny Domain Entities (Subscription, StreakProtection, ShopItem, CustomDictionary, atd.)
- ✅ Kompletní Service vrstva (StripeSubscriptionService, SubscriptionService, StreakProtectionService, InventoryService, DictionaryService, CoinService)
- ✅ Všechny Controllers (PremiumController, ShopController, StreakProtectionController, DictionaryController, WebhookController)
- ✅ Stripe Integration - NuGet balíček, StripeSettings, CreateCheckoutSession, webhook handlery
- ✅ Frontend Premium stránka s testy
- ✅ Frontend Shop stránka s komponentou ShopItemCard a testy
- ✅ Frontend Dictionaries stránka s testy
- ✅ Frontend Shield Management UI v StreakIndicator s testy
- ✅ Checkout stránky (CheckoutSuccess.razor, CheckoutCancel.razor)
- ✅ Resource soubory (.resx) pro všechny stránky
- ✅ EF Core konfigurace a migrace
- ✅ API Controller Tests pro všechny kontrollery
- ✅ Hangfire Job pro kontrolu expirovaných subscription
- ✅ Customer-Stripe ID mapping

---

## Ověření dokončení fáze

- [x] Premium: Stripe checkout → platba → aktivace → premium features
- [x] Premium pricing: 3 plány (monthly, yearly, lifetime)
- [x] Webhook handler: správně zpracovává Stripe eventy
- [x] Feature flags: premium features viditelné/skryté dle subscription
- [x] Streak Shield: aktivace, nákup za coiny, limit per week/month
- [x] Streak Freeze: automatická ochrana pro premium
- [x] Shop: kategorie, nákup za coiny, equip, premium-only items
- [x] Coins: earning (level, boss, daily, achievement) + spending (shop, shields)
- [x] Custom Dictionaries: CRUD, import (CSV/TXT/JSON), play, public sharing
- [x] Všechny texty z .resx
- [x] FluentValidation na FE i BE
- [x] `dotnet test` → všechny testy zelené
