# Fáze 4: Premium Features (Týden 6-7)

> **Cíl:** Prémiový účet (Stripe), streak ochrana, obchod s kosmickými předměty, vlastní slovníky
> **Závislost:** Fáze 2 (achievementy, streak), Fáze 3 (guest conversion)
> **Tempo.Blazor komponenty:** TmCard, TmButton, TmBadge, TmModal, TmIcon, TmProgressBar, TmToggle, TmTabs, TmTabPanel, TmChip, TmChipGroup, TmTextInput, TmTextArea, TmSelect, TmFormField, TmFormSection, TmAlert, TmEmptyState, TmFileDropZone, TmDataTable, TmTooltip, FluentValidationValidator, ToastService

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

### T-400.1: Domain Entities (TDD)
- [ ] **TEST:** `Subscription_Create_SetsDefaultValues` → RED
- [ ] **TEST:** `Subscription_IsActive_ReturnsTrueBeforeExpiry` → RED
- [ ] **TEST:** `Subscription_IsActive_ReturnsFalseAfterExpiry` → RED
- [ ] **TEST:** `Subscription_Cancel_SetsCancelledAt` → RED
- [ ] Vytvořit `Subscription` entitu (Id, UserId, Plan, StartedAt, ExpiresAt, CancelledAt, StripeSubscriptionId, Status)
- [ ] Vytvořit `SubscriptionPlan` enum (Monthly, Yearly, Lifetime)
- [ ] Vytvořit `SubscriptionStatus` enum (Active, Cancelled, Expired, PastDue)
- [ ] EF Core konfigurace + migrace
- [ ] **GREEN:** Testy prochází

### T-400.2: Stripe Integration Setup
- [ ] Přidat Stripe NuGet balíček do `LexiQuest.Infrastructure`
- [ ] Vytvořit `StripeSettings` POCO (ApiKey, WebhookSecret, Prices: Monthly/Yearly/Lifetime)
- [ ] Nastavit Stripe API key v appsettings.json (z environment variable v produkci)
- [ ] Vytvořit Stripe produkty a ceny v Stripe Dashboard:
  - Monthly: 99 CZK/měsíc
  - Yearly: 899 CZK/rok (25% sleva)
  - Lifetime: 2 499 CZK jednorázově

### T-400.3: SubscriptionService (TDD)
- [ ] **TEST:** `SubscriptionService_CreateCheckout_Monthly_ReturnsStripeUrl` → RED
- [ ] **TEST:** `SubscriptionService_CreateCheckout_Yearly_ReturnsStripeUrl` → RED
- [ ] **TEST:** `SubscriptionService_CreateCheckout_Lifetime_ReturnsStripeUrl` → RED
- [ ] **TEST:** `SubscriptionService_ActivateSubscription_SetsUserPremium` → RED
- [ ] **TEST:** `SubscriptionService_CancelSubscription_SetsCancelledAt` → RED
- [ ] **TEST:** `SubscriptionService_CheckExpired_DeactivatesPremium` → RED
- [ ] **TEST:** `SubscriptionService_GetStatus_ReturnsCurrentPlan` → RED
- [ ] Vytvořit `ISubscriptionService` interface
- [ ] Vytvořit DTOs: `CreateCheckoutRequest` (Plan), `CheckoutResponse` (StripeCheckoutUrl), `SubscriptionStatusDto`
- [ ] Implementovat `SubscriptionService`:
  - `CreateCheckoutSession()` → Stripe Checkout Session
  - `ActivateSubscription()` → po úspěšné platbě
  - `CancelSubscription()` → zruší na konci období
  - `CheckExpiredSubscriptions()` → Hangfire job
- [ ] **GREEN:** Všechny testy prochází

### T-400.4: Stripe Webhook Handler (TDD)
- [ ] **TEST:** `WebhookHandler_CheckoutCompleted_ActivatesSubscription` → RED
- [ ] **TEST:** `WebhookHandler_InvoicePaid_ExtendsSubscription` → RED
- [ ] **TEST:** `WebhookHandler_InvoiceFailed_MarksAsPastDue` → RED
- [ ] **TEST:** `WebhookHandler_SubscriptionCancelled_DeactivatesPremium` → RED
- [ ] **TEST:** `WebhookHandler_InvalidSignature_Returns400` → RED
- [ ] Vytvořit `POST /api/v1/webhooks/stripe` endpoint
- [ ] Ověřovat Stripe webhook signature
- [ ] Zpracovat eventy: checkout.session.completed, invoice.paid, invoice.payment_failed, customer.subscription.deleted
- [ ] **GREEN:** Testy prochází

### T-400.5: Premium Feature Flags (TDD)
- [ ] **TEST:** `PremiumFeatureService_IsPremium_ReturnsTrueForActiveSubscription` → RED
- [ ] **TEST:** `PremiumFeatureService_HasFeature_ReturnsTrueForPremiumFeatures` → RED
- [ ] **TEST:** `PremiumFeatureService_HasFeature_ReturnsFalseForFreeUser` → RED
- [ ] Vytvořit `IPremiumFeatureService` interface
- [ ] Vytvořit `PremiumFeature` enum (NoAds, StreakFreeze, StreakShield, DoubleXPWeekends, ExclusivePaths, CustomDictionaries, DetailedStats, CustomAvatar, DiamondLeague, TeamCreation)
- [ ] Implementovat feature checking per user
- [ ] **GREEN:** Testy prochází

### T-400.6: Premium Endpoints
- [ ] Vytvořit `POST /api/v1/premium/checkout` (vrací Stripe checkout URL)
- [ ] Vytvořit `GET /api/v1/premium/status` (vrací subscription status)
- [ ] Vytvořit `POST /api/v1/premium/cancel` (zruší subscription)
- [ ] Vytvořit `GET /api/v1/premium/features` (vrací seznam premium features)

---

## T-401: UC-018 Premium účet - Frontend

### T-401.1: Premium Landing Page (Tempo.Blazor)
- [ ] **TEST (bUnit):** `PremiumPage_Renders_3PricingCards` → RED
- [ ] **TEST (bUnit):** `PremiumPage_BestValue_HasHighlightBadge` → RED
- [ ] **TEST (bUnit):** `PremiumPage_ClickCheckout_RedirectsToStripe` → RED
- [ ] Vytvořit `Premium.razor` (`@page "/premium"`)
- [ ] `@inject IStringLocalizer<Premium> L`
- [ ] **Hero sekce**: Crown animace, titulek "LexiQuest Premium"
- [ ] **Feature groups** (4× `TmCard`):
  1. Game Features: No ads, Streak Freeze, Shield, 2x XP weekends, exclusive paths, custom dictionaries
  2. Analytics: Detailed stats, export (CSV/JSON), history, friend comparison
  3. Personalization: Custom avatar, exclusive avatars/frames/themes, colors
  4. Multiplayer: Diamond league, tournaments, team creation
  - Každý feature s `TmIcon` (check) a popisem z .resx
- [ ] **Pricing Cards** (3× `TmCard`):
  - Monthly: `TmCard` - "99 Kč/měsíc", `TmButton` "Předplatit"
  - Yearly: `TmCard` (Elevated, gold border) - `TmBadge` "⭐ BEST VALUE", "899 Kč/rok", přeškrtnutá cena "1 188 Kč", `TmButton Variant="Primary"` "Předplatit"
  - Lifetime: `TmCard` - "2 499 Kč jednorázově", `TmButton` "Koupit"
- [ ] Payment methods info: Stripe/PayPal ikony
- [ ] Cancel anytime notice
- [ ] Checkout flow: klik → volání API → redirect na Stripe Checkout → callback
- [ ] **GREEN:** Testy prochází

### T-401.2: Premium Badge v profilu
- [ ] Přidat `TmBadge` "⭐ Premium" do profilu a navigace pro premium uživatele
- [ ] Podmíněné zobrazení premium features v celé aplikaci
- [ ] Free user: `TmTooltip` "Premium funkce" s `TmIcon` (lock) na zamčených features

### T-401.3: Checkout Success/Cancel Pages
- [ ] Vytvořit `CheckoutSuccess.razor` (`@page "/premium/success"`)
  - `TmCard` s confetti, "Děkujeme za Premium!", přehled features
  - `TmButton` "Zpět na dashboard"
- [ ] Vytvořit `CheckoutCancel.razor` (`@page "/premium/cancel"`)
  - `TmCard` s "Platba zrušena", `TmButton` "Zkusit znovu"

---

## T-402: UC-012 Streak Shield a Freeze

### T-402.1: Backend - Shield/Freeze Service (TDD)
- [ ] **TEST:** `StreakProtectionService_ActivateShield_Free_1PerMonth` → RED
- [ ] **TEST:** `StreakProtectionService_ActivateShield_Premium_1PerWeek` → RED
- [ ] **TEST:** `StreakProtectionService_ActivateShield_AlreadyActive_ReturnsForbidden` → RED
- [ ] **TEST:** `StreakProtectionService_ActivateShield_NoShieldsRemaining_Returns400` → RED
- [ ] **TEST:** `StreakProtectionService_AutoFreeze_Premium_ProtectsStreak` → RED
- [ ] **TEST:** `StreakProtectionService_AutoFreeze_Free_DoesNotProtect` → RED
- [ ] **TEST:** `StreakProtectionService_AutoFreeze_AlreadyUsedThisWeek_DoesNotProtect` → RED
- [ ] **TEST:** `StreakProtectionService_PurchaseShield_3For500Coins_DeductsCoins` → RED
- [ ] **TEST:** `StreakProtectionService_EmergencyShield_Premium_300Coins` → RED
- [ ] Vytvořit `IStreakProtectionService` interface
- [ ] Vytvořit `StreakProtection` entitu (UserId, ShieldsRemaining, FreezeUsedThisWeek, LastShieldActivatedAt)
- [ ] Vytvořit DTOs: `StreakProtectionDto`, `ActivateShieldRequest`, `PurchaseShieldsRequest`
- [ ] EF Core konfigurace + migrace
- [ ] Rozšířit `StreakService` o Shield/Freeze logic
- [ ] Implementovat auto-freeze v streak check (pokud premium a miss)
- [ ] **GREEN:** Všechny testy prochází

### T-402.2: Backend - Shield Endpoints
- [ ] Vytvořit `POST /api/v1/streak/shield/activate` (aktivuje shield)
- [ ] Vytvořit `POST /api/v1/streak/shield/purchase` (koupí shieldy za coiny)
- [ ] Vytvořit `GET /api/v1/streak/protection` (vrací stav shieldů/freeze)

### T-402.3: Frontend - Shield Management UI (Tempo.Blazor)
- [ ] **TEST (bUnit):** `ShieldManagement_Renders_ShieldCount` → RED
- [ ] **TEST (bUnit):** `ShieldManagement_ActivateShield_CallsApi` → RED
- [ ] **TEST (bUnit):** `ShieldManagement_PurchaseShield_ShowsConfirmDialog` → RED
- [ ] Rozšířit `StreakIndicator.razor` o shield info:
  - `TmBadge` "🛡️ Shield k dispozici" / "🛡️ Shield použit"
  - `TmTooltip` s detaily
- [ ] Vytvořit `ShieldManagement.razor` modal/drawer:
  - `TmDrawer` (Right) s shield management
  - Aktuální shieldy: count s `TmIcon` (shield)
  - `TmButton` "Aktivovat Shield" (Primary)
  - Nákup: `TmCard` "3 shieldy za 500 mincí", `TmButton` "Koupit"
  - Emergency: `TmCard` "Okamžitý shield za 300 mincí" (Premium only)
  - Freeze status: `TmBadge` "❄️ Freeze aktivní" / "❄️ Freeze dostupný"
- [ ] **GREEN:** Testy prochází

---

## T-403: UC-019 Obchod

### T-403.1: Domain Entities (TDD)
- [ ] **TEST:** `ShopItem_Create_SetsProperties` → RED
- [ ] **TEST:** `UserInventory_AddItem_UpdatesInventory` → RED
- [ ] **TEST:** `UserInventory_EquipItem_SetsAsActive` → RED
- [ ] **TEST:** `UserInventory_HasItem_ReturnsTrueForOwned` → RED
- [ ] Vytvořit `ShopItem` entitu (Id, Name, Category, Price, Rarity, ImageUrl, IsPremiumOnly, IsLimited, AvailableUntil)
- [ ] Vytvořit `UserInventoryItem` entitu (UserId, ShopItemId, PurchasedAt, IsEquipped)
- [ ] Vytvořit `ShopCategory` enum (Avatar, Frame, Theme, Boost)
- [ ] Vytvořit `ItemRarity` enum (Common, Rare, Epic, Legendary)
- [ ] Vytvořit `UserCoins` value object v User entitě (Balance)
- [ ] EF Core konfigurace + migrace
- [ ] Seed data: avatary, frames, themes, boosty s cenami
- [ ] **GREEN:** Testy prochází

### T-403.2: InventoryService (TDD)
- [ ] **TEST:** `InventoryService_Purchase_DeductsCoins` → RED
- [ ] **TEST:** `InventoryService_Purchase_InsufficientCoins_Returns400` → RED
- [ ] **TEST:** `InventoryService_Purchase_AlreadyOwned_Returns409` → RED
- [ ] **TEST:** `InventoryService_Purchase_PremiumOnly_FreeUser_Returns403` → RED
- [ ] **TEST:** `InventoryService_Equip_SetsItemAsActive` → RED
- [ ] **TEST:** `InventoryService_Equip_UnequipsPreviousInCategory` → RED
- [ ] **TEST:** `InventoryService_GetInventory_ReturnsOwnedItems` → RED
- [ ] **TEST:** `InventoryService_GetShopItems_ReturnsAllWithOwnedStatus` → RED
- [ ] Vytvořit `IInventoryService` interface
- [ ] Vytvořit DTOs: `ShopItemDto`, `UserInventoryDto`, `PurchaseRequest`, `PurchaseResult`, `EquipItemRequest`
- [ ] Implementovat `InventoryService` s atomickou transakcí (UoW)
- [ ] **GREEN:** Všechny testy prochází

### T-403.3: CoinService (TDD)
- [ ] **TEST:** `CoinService_EarnCoins_LevelComplete_10Coins` → RED
- [ ] **TEST:** `CoinService_EarnCoins_BossLevel_50Coins` → RED
- [ ] **TEST:** `CoinService_EarnCoins_DailyChallenge_20Coins` → RED
- [ ] **TEST:** `CoinService_EarnCoins_Achievement_50to200Coins` → RED
- [ ] **TEST:** `CoinService_SpendCoins_DeductsBalance` → RED
- [ ] **TEST:** `CoinService_SpendCoins_InsufficientBalance_Returns400` → RED
- [ ] Vytvořit `ICoinService` interface
- [ ] Implementovat `CoinService`
- [ ] Integrovat do game session flow (přidat coiny při level complete, boss, daily, achievement)
- [ ] **GREEN:** Testy prochází

### T-403.4: Shop Endpoints
- [ ] Vytvořit `GET /api/v1/shop/items` (s filtrem dle category)
- [ ] Vytvořit `GET /api/v1/shop/items/{id}` (detail položky)
- [ ] Vytvořit `POST /api/v1/shop/purchase` (koupí položku)
- [ ] Vytvořit `POST /api/v1/shop/equip` (nasadí položku)
- [ ] Vytvořit `GET /api/v1/users/me/inventory` (inventář uživatele)
- [ ] Vytvořit `GET /api/v1/users/me/coins` (balance mincí)

### T-403.5: Frontend - Shop Page (Tempo.Blazor)
- [ ] **TEST (bUnit):** `ShopPage_Renders_CategoryTabs` → RED
- [ ] **TEST (bUnit):** `ShopPage_Renders_ItemCards` → RED
- [ ] **TEST (bUnit):** `ShopPage_Purchase_DeductsCoins` → RED
- [ ] Vytvořit `Shop.razor` (`@page "/shop"`)
- [ ] `@inject IStringLocalizer<Shop> L`
- [ ] **Header**: Balance mincí (`TmBadge` ⭐ + počet), `TmButton` "Koupit mince"
- [ ] **Category tabs**: `TmTabs` + `TmTabPanel` (Avatary, Rámečky, Témata, Boosty)
- [ ] **Item grid**: responsivní grid (4 col desktop, 2 col tablet, 1 col mobile)

### T-403.6: Frontend - ShopItemCard komponenta (Tempo.Blazor)
- [ ] **TEST (bUnit):** `ShopItemCard_Owned_ShowsCheckmark` → RED
- [ ] **TEST (bUnit):** `ShopItemCard_Available_ShowsPrice` → RED
- [ ] **TEST (bUnit):** `ShopItemCard_PremiumOnly_ShowsLock` → RED
- [ ] Vytvořit `ShopItemCard.razor`
- [ ] Owned state: `TmCard` se zelený checkmark `TmIcon`, `TmButton` "Nasadit" / "Nasazeno"
- [ ] Available state: `TmCard` s cenou (⭐ coins), `TmButton Variant="Primary"` "Koupit"
- [ ] Premium Only state: `TmCard` s greyscale, `TmIcon` (lock) overlay, `TmBadge` "🔒 Premium"
- [ ] Rarity indikátor: `TmBadge` s barvou dle rarity (Common=šedá, Rare=modrá, Epic=fialová, Legendary=zlatá)
- [ ] Hover: scale 1.02, shadow
- [ ] **Purchase confirm**: `TmModal` s potvrzením ("Koupit {item} za {price} mincí?")
- [ ] **GREEN:** Testy prochází

---

## T-404: UC-022 Vlastní slovníky (Premium)

### T-404.1: Domain Entities (TDD)
- [ ] **TEST:** `CustomDictionary_Create_SetsOwner` → RED
- [ ] **TEST:** `CustomDictionary_AddWord_IncreasesCount` → RED
- [ ] **TEST:** `CustomDictionary_AddWord_Max100_ThrowsOverLimit` → RED
- [ ] **TEST:** `DictionaryWord_Validate_Length3to20` → RED
- [ ] **TEST:** `DictionaryWord_Validate_InvalidChars_ThrowsError` → RED
- [ ] Vytvořit `CustomDictionary` entitu (Id, UserId, Name, Description, IsPublic, WordCount, DownloadCount, CreatedAt)
- [ ] Vytvořit `DictionaryWord` entitu (Id, DictionaryId, Word, Difficulty)
- [ ] EF Core konfigurace + migrace
- [ ] **GREEN:** Testy prochází

### T-404.2: DictionaryService (TDD)
- [ ] **TEST:** `DictionaryService_Create_PremiumUser_Success` → RED
- [ ] **TEST:** `DictionaryService_Create_FreeUser_Returns403` → RED
- [ ] **TEST:** `DictionaryService_Create_Max10Dictionaries_Returns400` → RED
- [ ] **TEST:** `DictionaryService_AddWord_ValidWord_Success` → RED
- [ ] **TEST:** `DictionaryService_AddWord_InvalidLength_Returns400` → RED
- [ ] **TEST:** `DictionaryService_AddWord_DuplicateInDictionary_Returns409` → RED
- [ ] **TEST:** `DictionaryService_ImportCSV_ParsesCorrectly` → RED
- [ ] **TEST:** `DictionaryService_ImportTXT_ParsesCorrectly` → RED
- [ ] **TEST:** `DictionaryService_ImportJSON_ParsesCorrectly` → RED
- [ ] **TEST:** `DictionaryService_Import_ExceedsMax100_Returns400` → RED
- [ ] **TEST:** `DictionaryService_Delete_OwnerOnly` → RED
- [ ] **TEST:** `DictionaryService_GetPublic_ReturnsPublicDictionaries` → RED
- [ ] **TEST:** `DictionaryService_StartGameWithCustom_UsesCustomWords` → RED
- [ ] Vytvořit `IDictionaryService` interface
- [ ] Vytvořit DTOs: `CustomDictionaryDto`, `DictionaryWordDto`, `CreateDictionaryRequest`, `ImportWordsRequest`, `ImportResult`
- [ ] Vytvořit validátory: `CreateDictionaryValidator`, `AddWordValidator` s lokalizací
- [ ] Implementovat `DictionaryService` s import parsery (CSV, TXT, JSON)
- [ ] **GREEN:** Všechny testy prochází

### T-404.3: Dictionary Endpoints
- [ ] Vytvořit `GET /api/v1/dictionaries` (moje slovníky)
- [ ] Vytvořit `POST /api/v1/dictionaries` (vytvoří slovník)
- [ ] Vytvořit `GET /api/v1/dictionaries/{id}` (detail s words)
- [ ] Vytvořit `PUT /api/v1/dictionaries/{id}` (upraví slovník)
- [ ] Vytvořit `DELETE /api/v1/dictionaries/{id}` (smaže slovník)
- [ ] Vytvořit `POST /api/v1/dictionaries/{id}/words` (přidá slovo)
- [ ] Vytvořit `DELETE /api/v1/dictionaries/{id}/words/{wordId}` (smaže slovo)
- [ ] Vytvořit `POST /api/v1/dictionaries/{id}/import` (import CSV/TXT/JSON)
- [ ] Vytvořit `GET /api/v1/dictionaries/public` (veřejné slovníky)
- [ ] Vytvořit `POST /api/v1/game/start` rozšířit o CustomDictionaryId parametr

### T-404.4: Frontend - DictionaryBuilder Page (Tempo.Blazor)
- [ ] **TEST (bUnit):** `DictionaryBuilder_Renders_DictionaryList` → RED
- [ ] **TEST (bUnit):** `DictionaryBuilder_CreateDictionary_ShowsModal` → RED
- [ ] **TEST (bUnit):** `DictionaryBuilder_ImportFile_ParsesAndAdds` → RED
- [ ] Vytvořit `Dictionaries.razor` (`@page "/dictionaries"`)
- [ ] `@inject IStringLocalizer<Dictionaries> L`
- [ ] **Dictionary list**: `TmCard` pro každý slovník s:
  - Název, popis, počet slov, `TmBadge` (Public/Private)
  - `TmButton` "Upravit" / "Smazat" / "Hrát"
- [ ] **Create modal**: `TmModal` s `TmFormField` + `TmTextInput` pro název, `TmTextArea` pro popis, `TmToggle` pro veřejnost
- [ ] **Dictionary detail page** (`@page "/dictionaries/{id}"`):
  - Word list: `TmDataTable` se slovy (Word, Difficulty, Actions)
  - Add word: `TmFormField` + `TmTextInput` + `TmSelect` (difficulty) + `TmButton` "Přidat"
  - Import: `TmFileDropZone` pro CSV/TXT/JSON upload
  - `<FluentValidationValidator />`
- [ ] **Empty state**: `TmEmptyState` "Zatím žádné slovníky" s CTA "Vytvořit první"
- [ ] Free user view: `TmAlert` "Premium funkce" s link na `/premium`
- [ ] **GREEN:** Testy prochází

---

## Ověření dokončení fáze

- [ ] Premium: Stripe checkout → platba → aktivace → premium features
- [ ] Premium pricing: 3 plány (monthly, yearly, lifetime)
- [ ] Webhook handler: správně zpracovává Stripe eventy
- [ ] Feature flags: premium features viditelné/skryté dle subscription
- [ ] Streak Shield: aktivace, nákup za coiny, limit per week/month
- [ ] Streak Freeze: automatická ochrana pro premium
- [ ] Shop: kategorie, nákup za coiny, equip, premium-only items
- [ ] Coins: earning (level, boss, daily, achievement) + spending (shop, shields)
- [ ] Custom Dictionaries: CRUD, import (CSV/TXT/JSON), play, public sharing
- [ ] Všechny texty z .resx
- [ ] FluentValidation na FE i BE
- [ ] `dotnet test` → všechny testy zelené
