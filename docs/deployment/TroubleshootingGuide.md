# LexiQuest - Troubleshooting Guide

> Pruvodce resenim nejcastejsich problemu pri vyvoji a provozu aplikace LexiQuest.

## Obsah

1. [Problemy s databazi](#problemy-s-databazi)
2. [JWT a autentizace](#jwt-a-autentizace)
3. [CORS chyby](#cors-chyby)
4. [SignalR a WebSocket problemy](#signalr-a-websocket-problemy)
5. [Stripe webhook selhani](#stripe-webhook-selhani)
6. [Push notifikace nefunguji](#push-notifikace-nefunguji)
7. [Vykonnostni problemy](#vykonnostni-problemy)
8. [Analyza logu](#analyza-logu)
9. [Blazor WASM specificke problemy](#blazor-wasm-specificke-problemy)

---

## Problemy s databazi

### Chyba: "Cannot open database" / "Login failed"

**Pricina:** Nespravny connection string nebo chybejici pristupova prava.

**Reseni:**
1. Overit connection string v `appsettings.json` nebo promennych prostredi:
   ```
   ConnectionStrings__DefaultConnection
   ```
2. Zkontrolovat, ze SQL Server bezi:
   ```powershell
   Get-Service MSSQLSERVER
   # nebo pro SQL Express:
   Get-Service 'MSSQL$SQLEXPRESS'
   ```
3. Overit pristupova prava uzivatele k databazi
4. Pro Azure SQL zkontrolovat firewall pravidla:
   ```bash
   az sql server firewall-rule list --resource-group rg-lexiquest --server lexiquest-sql
   ```

### Chyba: "A connection was successfully established with the server, but then an error occurred"

**Pricina:** Problem s SSL/TLS certifikatem databaze.

**Reseni:** Pridat `TrustServerCertificate=True` do connection stringu (pouze pro vyvoj) nebo nainstalovat spravny certifikat.

### Chyba: Migrace selhava

**Pricina:** Nekonzistentni stav migraci nebo chybejici zavislosti.

**Reseni:**
1. Zkontrolovat stav migraci:
   ```bash
   dotnet ef migrations list \
     --project src/LexiQuest.Infrastructure \
     --startup-project src/LexiQuest.Api
   ```
2. Pro uplne novou databazi:
   ```bash
   dotnet ef database update \
     --project src/LexiQuest.Infrastructure \
     --startup-project src/LexiQuest.Api
   ```
3. Pokud migrace selhava na existujici databazi, vygenerujte SQL script a aplikujte rucne:
   ```bash
   dotnet ef migrations script <posledni-aplikovana-migrace> \
     --project src/LexiQuest.Infrastructure \
     --startup-project src/LexiQuest.Api \
     --output fix.sql
   ```

### Chyba: "Retrying execution due to transient failure"

**Pricina:** Docasna nedostupnost SQL Serveru. Aplikace pouziva retry policy (max 5 pokusu, 30s delay).

**Reseni:** Pokud se opakuje, zkontrolovat:
- Zatizeni SQL Serveru
- Sitove pripojeni
- Azure SQL DTU limity

---

## JWT a autentizace

### Chyba: 401 Unauthorized na vsech endpointech

**Pricina:** Chybejici nebo nespravny JWT token.

**Reseni:**
1. Overit, ze `JwtSettings__SecretKey` je nastaveny a ma min. 64 znaku
2. Zkontrolovat `JwtSettings__Issuer` a `JwtSettings__Audience` - musi souhlasit s hodnotami pouzitymi pri generovani tokenu
3. Zkontrolovat, ze token neni expirovany (vychozi expirace: 30 minut)

### Chyba: "IDX10223: Lifetime validation failed. The token is expired"

**Pricina:** Token vyprsel.

**Reseni:**
- Klient by mel automaticky pouzit refresh token pro ziskani noveho access tokenu
- Zkontrolovat synchronizaci hodin serveru (aplikace pouziva `ClockSkew = TimeSpan.Zero`)
- Overit, ze server ma spravny cas (NTP)

### Chyba: "IDX10501: Signature validation failed"

**Pricina:** Nesoulad tajneho klice mezi generovanim a validaci tokenu.

**Reseni:**
- Overit, ze `JwtSettings__SecretKey` je stejny na vsech instancich API serveru
- Po zmene klice se vsechny existujici tokeny stavaji neplatnymi - uzivatele se musi znovu prihlasit

### Chyba: "InvalidOperationException: JWT SecretKey is not configured"

**Pricina:** Promenna `JwtSettings__SecretKey` neni nastavena.

**Reseni:** Nastavte promennou prostredi nebo v `appsettings.json`:
```json
{
  "JwtSettings": {
    "SecretKey": "<min-64-znaku-dlouhy-nahodny-retezec>"
  }
}
```

---

## CORS chyby

### Chyba: "Access to fetch has been blocked by CORS policy"

**Pricina:** Frontend se pokusi pristoupit k API z jine domeny, nez je nakonfigurovana v CORS policy.

**Reseni:**
1. Overit `BlazorClient__Url` v konfiguraci API:
   ```json
   {
     "BlazorClient": {
       "Url": "https://app.lexiquest.cz"
     }
   }
   ```
2. URL musi presne souhlasit (vcetne protokolu, bez trailing slash)
3. Pro vyvoj: `https://localhost:5001`
4. Pro produkci: domena Blazor frontendu

### Chyba: CORS s credentials

**Pricina:** SignalR vyzaduje `AllowCredentials()`, coz je nekompatibilni s `AllowAnyOrigin()`.

**Reseni:** Vzdy specifikujte konkretni origin misto `AllowAnyOrigin()`. Aktualni konfigurace v `Program.cs` to spravne resi pres `WithOrigins()` + `AllowCredentials()`.

---

## SignalR a WebSocket problemy

### Chyba: SignalR se nepripoji / neustale reconnect

**Pricina:** WebSocket neni povoleny na serveru nebo proxy.

**Reseni:**

**IIS:**
1. Zkontrolovat, ze **WebSocket Protocol** je nainstalovan:
   - Server Manager > Add Roles and Features > Web Server > Application Development > WebSocket Protocol
2. Restartovat IIS

**Azure App Service:**
1. Zapnout WebSockets v konfiguraci:
   ```bash
   az webapp config set --name lexiquest-api --resource-group rg-lexiquest --web-sockets-enabled true
   ```

**Za reverse proxy (nginx, Apache):**
- Nakonfigurovat proxy pro WebSocket upgrade hlavicky
- Zvysit timeout na min. 120s

### Chyba: SignalR odpojuje po 30s

**Pricina:** Load balancer nebo proxy prerusuje idle pripojeni.

**Reseni:**
- Nastavit KeepAlive interval na SignalR klientovi
- Nakonfigurovat sticky sessions na load balanceru (Azure: ARR Affinity)

### Chyba: "Failed to start the connection: Error: WebSocket failed to connect"

**Pricina:** SignalR hub neni dostupny na ocekavane URL.

**Reseni:**
- Overit, ze hub je namapovany: `/hubs/match`
- Zkontrolovat, ze CORS povoli pripojeni z frontendu
- Overit, ze authentication middleware je pred mapovanim hubu

---

## Stripe webhook selhani

### Chyba: "No signatures found matching the expected signature for payload"

**Pricina:** Nespravny webhook signing secret.

**Reseni:**
1. Zkopirovat spravny signing secret ze Stripe Dashboard > Webhooks > endpoint > Signing secret
2. Nastavit jako `StripeSettings__WebhookSecret`
3. Overit, ze webhook endpoint URL je spravna: `https://api.lexiquest.cz/api/webhook/stripe`

### Chyba: Webhook neni dorucen

**Pricina:** Stripe nemuze dosahnout na endpoint.

**Reseni:**
1. Overit, ze endpoint je verejne dostupny (ne za VPN/firewall)
2. Zkontrolovat ve Stripe Dashboard > Webhooks > Recent events
3. Pro lokalni vyvoj pouzit [Stripe CLI](https://stripe.com/docs/stripe-cli):
   ```bash
   stripe listen --forward-to localhost:5000/api/webhook/stripe
   ```

### Chyba: Duplicitni zpracovani udalosti

**Pricina:** Stripe muze poslat stejny webhook vicekrat.

**Reseni:** Implementovat idempotentni zpracovani - kontrolovat `event.Id` proti jiz zpracovanym udalostem.

---

## Push notifikace nefunguji

### Uzivatele nedostavaji notifikace

**Pricina:** Vicero moznych pricin.

**Kontrolni seznam:**
1. **VAPID klice** - overit, ze `VapidSettings` je spravne nakonfigurovany (PublicKey, PrivateKey, Subject)
2. **Browser permissions** - uzivatel musi povolit notifikace v prohlizeci
3. **Service Worker** - overit, ze service worker je registrovany:
   ```javascript
   // V browser konzoli
   navigator.serviceWorker.getRegistrations().then(r => console.log(r))
   ```
4. **HTTPS** - push notifikace vyzaduji HTTPS (krome localhost)
5. **Subscription** - overit, ze PushSubscription je ulozena v databazi

### Chyba: "UnauthorizedRegistration" nebo "ExpiredSubscription"

**Pricina:** Push subscription vyprsela nebo je neplatna.

**Reseni:** Klient by mel znovu zaregistrovat subscription pri dalsim pristupu do aplikace.

---

## Vykonnostni problemy

### Kontrolni seznam pro degradaci vykonu

1. **Databazove indexy**
   - Zkontrolovat, ze EF Core migrace vytvorily vsechny indexy
   - Overit execution plany pomalych dotazu v SQL Server Management Studio
   - Hledat table scany na velkych tabulkach

2. **N+1 dotazy**
   - Zkontrolovat logy pro opakujici se SQL dotazy
   - Pouzivat `Include()` pro eager loading relaci
   - Overit, ze repository metody pouzivaji spravne `.AsNoTracking()` pro read-only dotazy

3. **Caching**
   - Overit, ze `MemoryCache` funguje spravne
   - Zkontrolovat cache hit ratio v logach
   - Pro velke nasazeni zvazit distribuovanou cache (Redis)

4. **SignalR pripojeni**
   - Monitorovat pocet aktivnich pripojeni
   - Overit `MaximumReceiveMessageSize` (vychozi: 64KB)

5. **Blazor WASM velikost**
   - Overit, ze IL linking je zapnuty (`BlazorWebAssemblyEnableLinking`)
   - Overit, ze komprese je zapnuta (`BlazorEnableCompression`)
   - Sledovat velikost stazenych DLL

6. **Databaze**
   - Monitorovat DTU/CPU vyuziti (Azure SQL)
   - Zkontrolovat deadlocky a blocking queries
   - Overit retry policy nastaveni (max 5 pokusu, 30s delay)

---

## Analyza logu

### Struktura logu

Aplikace pouziva **Serilog** s nasledujici konfiguraci:
- **Console sink** - vystup do konzole
- **File sink** - soubory v `logs/lexiquest-YYYYMMDD.log` (rotace denne, max 7 souboru)

### Filtrovani logu

```bash
# Hledani chyb
grep -i "error\|exception\|fatal" logs/lexiquest-*.log

# Hledani pomalych dotazu (EF Core)
grep "Executed DbCommand" logs/lexiquest-*.log | grep -v "1ms\|2ms\|3ms"

# Hledani autentizacnich problemu
grep -i "401\|unauthorized\|jwt\|token" logs/lexiquest-*.log

# Hledani SignalR problemu
grep -i "signalr\|hub\|websocket" logs/lexiquest-*.log
```

### Serilog urovne

| Uroven | Popis |
|---|---|
| `Verbose` | Nejvice detailni (ve vychozim stavu vypnuto) |
| `Debug` | Diagnosticke informace (zapnuto v Development) |
| `Information` | Bezne provozni udalosti |
| `Warning` | Neocekavane, ale ne kriticke udalosti |
| `Error` | Chyby, ktere ovlivnuji funkcionalitu |
| `Fatal` | Kriticke chyby, aplikace se nespusti |

### Zmena urovne logu za behu

Upravit `appsettings.json` a restartovat aplikaci:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft.EntityFrameworkCore": "Information"
      }
    }
  }
}
```

---

## Blazor WASM specificke problemy

### Aplikace se nenacita / bila obrazovka

**Pricina:** Chyba pri stahovani WASM nebo DLL souboru.

**Reseni:**
1. Otevrit browser konzoli (F12) a zkontrolovat chyby
2. Overit, ze server spravne servuje `.wasm` a `.dll` soubory s MIME typy:
   - `.wasm` -> `application/wasm`
   - `.dll` -> `application/octet-stream`
3. Na IIS zkontrolovat `web.config` pro staticContent MIME typy
4. Overit, ze `index.html` referuje spravny `blazor.webassembly.js`

### Chyba: "Failed to find a valid digest in the 'integrity' attribute"

**Pricina:** Cachovana verze souboru nesouhlasi s novou verzi.

**Reseni:**
1. Hard refresh prohlizece: `Ctrl+Shift+R`
2. Vymazat cache prohlizece
3. Unregistrovat service worker:
   ```javascript
   navigator.serviceWorker.getRegistrations().then(regs => regs.forEach(r => r.unregister()))
   ```

### Chyba: Stara verze aplikace po deployi

**Pricina:** Service worker nebo browser cache servuje starou verzi.

**Reseni:**
1. Inkrementovat verzi v `index.html` nebo service worker
2. Pridat cache-busting hlavicky na serveru:
   ```
   Cache-Control: no-cache, no-store, must-revalidate
   ```
   (pouze pro `index.html` a `blazor.boot.json`)

### Chyba: HttpClient vola spatnou URL

**Pricina:** Base address HttpClient neni spravne nakonfigurovana.

**Reseni:** Overit konfiguraci v `Program.cs` Blazor projektu:
```csharp
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri("https://api.lexiquest.cz") });
```

### Chyba: Lokalizace nefunguje / texty se zobrazuji jako klice

**Pricina:** Chybejici `.resx` soubory nebo spatna konfigurace lokalizace.

**Reseni:**
1. Overit, ze `.resx` soubory existuji v `Resources/` adresari
2. Zkontrolovat, ze nazev resource souboru odpovida namespace komponenty
3. Overit registraci lokalizace v `Program.cs`:
   ```csharp
   builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
   ```

---

## Kontaktni informace pro eskalaci

Pokud problem nemuze byt vyresen pomoci tohoto pruvodce:

1. Zkontrolujte [GitHub Issues](https://github.com/your-org/lexiquest/issues) pro zname problemy
2. Vytvorte novy issue s:
   - Popisem problemu
   - Kroky k reprodukci
   - Relevatnimi logy
   - Verzi aplikace a prostredi
