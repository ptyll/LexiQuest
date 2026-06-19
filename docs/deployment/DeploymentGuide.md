# LexiQuest - Deployment Guide

> Kompletni pruvodce nasazenim aplikace LexiQuest do produkce.

## Obsah

1. [Predpoklady](#predpoklady)
2. [Promenne prostredi](#promenne-prostredi)
3. [Databazove migrace](#databazove-migrace)
4. [Nasazeni na Azure](#nasazeni-na-azure)
5. [Nasazeni na IIS](#nasazeni-na-iis)
6. [SSL certifikat](#ssl-certifikat)
7. [Prvni admin ucet](#prvni-admin-ucet)

---

## Predpoklady

### Software

| Komponenta | Minimalni verze | Poznamka |
|---|---|---|
| .NET SDK | 10.0 | Runtime i SDK |
| SQL Server | 2019+ | Nebo Azure SQL Database |
| IIS | 10+ | Pro on-premise nasazeni |
| Node.js | 18+ | Pouze pro E2E testy (Playwright) |

### Azure alternativa

- Azure App Service (API)
- Azure Static Web Apps (Blazor WASM)
- Azure SQL Database

---

## Promenne prostredi

Vsechny promenne musi byt nastaveny pred spustenim aplikace. V Azure se nastavuji jako Application Settings, na IIS jako environment variables nebo v `appsettings.Production.json`.

| Promenna | Popis | Priklad |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | Connection string pro SQL Server | `Server=tcp:lexiquest.database.windows.net;Database=LexiQuest;User ID=admin;Password=***;Encrypt=True;` |
| `JwtSettings__SecretKey` | Tajny klic pro podepisovani JWT tokenu (min. 64 znaku) | Vygenerujte nahodny retezec |
| `JwtSettings__Issuer` | Vydavatel JWT tokenu | `LexiQuest` |
| `JwtSettings__Audience` | Prijemce JWT tokenu | `LexiQuestClient` |
| `JwtSettings__AccessTokenExpiryMinutes` | Expirace access tokenu v minutach | `30` |
| `JwtSettings__RefreshTokenExpiryDays` | Expirace refresh tokenu ve dnech | `7` |
| `StripeSettings__ApiKey` | Stripe API klic (sk_live_...) | `sk_live_...` |
| `StripeSettings__WebhookSecret` | Stripe webhook signing secret | `whsec_...` |
| `StripeSettings__MonthlyPriceId` | Stripe Price ID pro mesicni plan | `price_...` |
| `StripeSettings__YearlyPriceId` | Stripe Price ID pro rocni plan | `price_...` |
| `StripeSettings__LifetimePriceId` | Stripe Price ID pro dozivotni plan | `price_...` |
| `VapidSettings__PublicKey` | VAPID verejny klic pro push notifikace | Vygenerujte pres `web-push generate-vapid-keys` |
| `VapidSettings__PrivateKey` | VAPID soukromy klic | Viz vyse |
| `VapidSettings__Subject` | VAPID subject (email nebo URL) | `mailto:admin@lexiquest.cz` |
| `Email__ApiKey` | API klic emailove sluzby | Zavisí na poskytovateli |
| `BlazorClient__Url` | URL Blazor frontendu (pro CORS) | `https://app.lexiquest.cz` |

> **DULEZITE:** Nikdy neukladejte tajne klice do `appsettings.json` v repozitari. Pouzivejte environment variables, Azure Key Vault nebo user secrets.

---

## Databazove migrace

### Spusteni migraci

```bash
dotnet ef database update \
  --project src/LexiQuest.Infrastructure \
  --startup-project src/LexiQuest.Api
```

### Vytvoreni nove migrace

```bash
dotnet ef migrations add NazevMigrace \
  --project src/LexiQuest.Infrastructure \
  --startup-project src/LexiQuest.Api
```

### Generovani SQL scriptu (pro produkcni nasazeni)

```bash
dotnet ef migrations script \
  --project src/LexiQuest.Infrastructure \
  --startup-project src/LexiQuest.Api \
  --idempotent \
  --output migration.sql
```

> **Doporuceni:** Pro produkcni databaze pouzivejte vzdy SQL script misto primeho `database update`. Umoznuje to review zmen pred aplikaci.

---

## Nasazeni na Azure

### 1. Vytvoreni Azure SQL Database

```bash
# Vytvoreni resource group
az group create --name rg-lexiquest --location westeurope

# Vytvoreni SQL serveru
az sql server create \
  --name lexiquest-sql \
  --resource-group rg-lexiquest \
  --location westeurope \
  --admin-user sqladmin \
  --admin-password '<silne-heslo>'

# Vytvoreni databaze
az sql db create \
  --resource-group rg-lexiquest \
  --server lexiquest-sql \
  --name LexiQuest \
  --service-objective S1

# Povoleni pristupu z Azure sluzeb
az sql server firewall-rule create \
  --resource-group rg-lexiquest \
  --server lexiquest-sql \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

### 2. Vytvoreni Azure App Service (API)

```bash
# Vytvoreni App Service planu
az appservice plan create \
  --name plan-lexiquest \
  --resource-group rg-lexiquest \
  --sku B1 \
  --is-linux false

# Vytvoreni Web App
az webapp create \
  --name lexiquest-api \
  --resource-group rg-lexiquest \
  --plan plan-lexiquest \
  --runtime "dotnet:10"
```

### 3. Konfigurace promennych prostredi (Azure App Service)

```bash
az webapp config appsettings set \
  --name lexiquest-api \
  --resource-group rg-lexiquest \
  --settings \
    ConnectionStrings__DefaultConnection="Server=tcp:lexiquest-sql.database.windows.net,1433;Database=LexiQuest;User ID=sqladmin;Password=<heslo>;Encrypt=True;TrustServerCertificate=False;" \
    JwtSettings__SecretKey="<vygenerovany-tajny-klic-min-64-znaku>" \
    JwtSettings__Issuer="LexiQuest" \
    JwtSettings__Audience="LexiQuestClient" \
    StripeSettings__ApiKey="sk_live_..." \
    StripeSettings__WebhookSecret="whsec_..." \
    VapidSettings__PublicKey="<vapid-public-key>" \
    VapidSettings__PrivateKey="<vapid-private-key>" \
    VapidSettings__Subject="mailto:admin@lexiquest.cz" \
    BlazorClient__Url="https://lexiquest-app.azurestaticapps.net"
```

### 4. Deploy API

```bash
# Publikovani API
dotnet publish src/LexiQuest.Api -c Release -o ./publish/api

# Nasazeni na Azure (pres zip deploy)
cd ./publish/api
zip -r ../api.zip .
az webapp deploy \
  --name lexiquest-api \
  --resource-group rg-lexiquest \
  --src-path ../api.zip \
  --type zip
```

### 5. Vytvoreni Azure Static Web Apps (Blazor WASM)

```bash
# Publikovani Blazor WASM
dotnet publish src/LexiQuest.Blazor -c Release -o ./publish/blazor

# Vytvoreni Static Web App
az staticwebapp create \
  --name lexiquest-app \
  --resource-group rg-lexiquest \
  --location westeurope

# Deploy pres Azure CLI nebo GitHub Actions
# Doporuceny postup: propojit s GitHub repo pro automaticky deploy
```

### 6. Spusteni migraci na produkcni databazi

```bash
# Vygenerujte SQL script
dotnet ef migrations script \
  --project src/LexiQuest.Infrastructure \
  --startup-project src/LexiQuest.Api \
  --idempotent \
  --output migration.sql

# Aplikujte script na Azure SQL
sqlcmd -S lexiquest-sql.database.windows.net \
  -d LexiQuest \
  -U sqladmin \
  -P '<heslo>' \
  -i migration.sql
```

### 7. Konfigurace Stripe webhooku

1. Prejdete do [Stripe Dashboard](https://dashboard.stripe.com/webhooks)
2. Kliknete na **Add endpoint**
3. Zadejte URL: `https://lexiquest-api.azurewebsites.net/api/webhook/stripe`
4. Vyberte udalosti:
   - `checkout.session.completed`
   - `customer.subscription.updated`
   - `customer.subscription.deleted`
   - `invoice.payment_succeeded`
   - `invoice.payment_failed`
5. Zkopirujte **Signing secret** a nastavte jako `StripeSettings__WebhookSecret`

### 8. Konfigurace VAPID klicu pro push notifikace

```bash
# Instalace nastroje pro generovani klicu
npm install -g web-push

# Generovani VAPID klicu
web-push generate-vapid-keys
```

Vystup obsahuje `publicKey` a `privateKey`. Nastavte je jako:
- `VapidSettings__PublicKey` - verejny klic
- `VapidSettings__PrivateKey` - soukromy klic
- `VapidSettings__Subject` - `mailto:admin@lexiquest.cz`

### 9. SSL certifikat

Azure App Service a Static Web Apps poskytuji SSL certifikaty automaticky. Pro vlastni domenu:

```bash
# Pridani vlastni domeny
az webapp config hostname add \
  --webapp-name lexiquest-api \
  --resource-group rg-lexiquest \
  --hostname api.lexiquest.cz

# Vytvoreni managed certifikatu
az webapp config ssl create \
  --name lexiquest-api \
  --resource-group rg-lexiquest \
  --hostname api.lexiquest.cz

# Svazani certifikatu s domenou
az webapp config ssl bind \
  --name lexiquest-api \
  --resource-group rg-lexiquest \
  --certificate-thumbprint <thumbprint> \
  --ssl-type SNI
```

### 10. Prvni admin ucet

Po nasazeni vytvorte prvniho administratora:

1. Zaregistrujte se normalne pres UI
2. Pripojte se k databazi a nastavte roli:

```sql
-- Nastaveni admin role pro prvniho uzivatele
UPDATE Users SET Role = 'Admin' WHERE Email = 'admin@lexiquest.cz';

-- Alternativne pres AdminRoleAssignments tabulku
INSERT INTO AdminRoleAssignments (UserId, Role, AssignedAt, AssignedBy)
SELECT Id, 'SuperAdmin', GETUTCDATE(), Id FROM Users WHERE Email = 'admin@lexiquest.cz';
```

---

## Nasazeni na IIS

### 1. Instalace .NET 10 Hosting Bundle

1. Stahnete [.NET 10 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/10.0)
2. Nainstalujte na server
3. Restartujte IIS: `iisreset`

### 2. Publikovani aplikace

```bash
# API
dotnet publish src/LexiQuest.Api -c Release -o C:\inetpub\lexiquest-api

# Blazor WASM
dotnet publish src/LexiQuest.Blazor -c Release -o C:\inetpub\lexiquest-blazor
```

### 3. Konfigurace IIS situ - API

1. Otevrete **IIS Manager**
2. Pridejte novy **Application Pool**:
   - Nazev: `LexiQuestApiPool`
   - .NET CLR Version: **No Managed Code**
   - Managed Pipeline Mode: **Integrated**
3. Pridejte novy **Web Site**:
   - Nazev: `LexiQuest-API`
   - Physical Path: `C:\inetpub\lexiquest-api`
   - Application Pool: `LexiQuestApiPool`
   - Binding: `https`, port `443`, hostname `api.lexiquest.cz`
   - SSL Certificate: vyberte certifikat

### 4. Konfigurace IIS situ - Blazor WASM

1. Pridejte novy **Application Pool**:
   - Nazev: `LexiQuestBlazorPool`
   - .NET CLR Version: **No Managed Code**
2. Pridejte novy **Web Site**:
   - Nazev: `LexiQuest-Blazor`
   - Physical Path: `C:\inetpub\lexiquest-blazor\wwwroot`
   - Application Pool: `LexiQuestBlazorPool`
   - Binding: `https`, port `443`, hostname `app.lexiquest.cz`

### 5. web.config pro Blazor WASM

Vytvorte `web.config` v `C:\inetpub\lexiquest-blazor\wwwroot`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <staticContent>
      <remove fileExtension=".blat" />
      <remove fileExtension=".dat" />
      <remove fileExtension=".dll" />
      <remove fileExtension=".json" />
      <remove fileExtension=".wasm" />
      <remove fileExtension=".woff" />
      <remove fileExtension=".woff2" />
      <mimeMap fileExtension=".blat" mimeType="application/octet-stream" />
      <mimeMap fileExtension=".dat" mimeType="application/octet-stream" />
      <mimeMap fileExtension=".dll" mimeType="application/octet-stream" />
      <mimeMap fileExtension=".json" mimeType="application/json" />
      <mimeMap fileExtension=".wasm" mimeType="application/wasm" />
      <mimeMap fileExtension=".woff" mimeType="application/font-woff" />
      <mimeMap fileExtension=".woff2" mimeType="application/font-woff" />
    </staticContent>
    <httpCompression>
      <dynamicTypes>
        <add mimeType="application/wasm" enabled="true" />
      </dynamicTypes>
    </httpCompression>
    <rewrite>
      <rules>
        <rule name="Serve subdir">
          <match url=".*" />
          <action type="Rewrite" url="wwwroot\{R:0}" />
        </rule>
        <rule name="SPA fallback routing" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
          </conditions>
          <action type="Rewrite" url="index.html" />
        </rule>
      </rules>
    </rewrite>
  </system.webServer>
</configuration>
```

### 6. Nastaveni reverse proxy (volitelne)

Pokud chcete mit API i Blazor na jedne domene, nainstalujte **URL Rewrite** a **Application Request Routing** moduly pro IIS:

```xml
<!-- Pridejte do web.config Blazor situ -->
<rule name="API Proxy" stopProcessing="true">
  <match url="^api/(.*)" />
  <action type="Rewrite" url="https://localhost:5000/api/{R:1}" />
</rule>
```

### 7. Konfigurace HTTPS

1. Ziskejte SSL certifikat (Let's Encrypt, nakoupeny, nebo self-signed pro interni pouziti)
2. Importujte certifikat do IIS:
   - Otevrete **Server Certificates** v IIS Manager
   - Kliknete **Import** a vyberte PFX soubor
3. Nastavte HTTPS binding na obou sitech

Pro automatickou obnovu Let's Encrypt certifikatu pouzijte [win-acme](https://www.win-acme.com/).

### 8. Nastaveni databaze

```bash
# Spusteni migraci z prikazove radky
dotnet ef database update \
  --project src/LexiQuest.Infrastructure \
  --startup-project src/LexiQuest.Api \
  --connection "Server=<server>;Database=LexiQuest;User ID=<user>;Password=<heslo>;TrustServerCertificate=True"
```

### 9. Promenne prostredi na IIS

Nastavte promenne prostredi jednim z nasledujicich zpusobu:

**a) Systemove promenne prostredi (doporuceno):**
```powershell
[System.Environment]::SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Server=...;", "Machine")
[System.Environment]::SetEnvironmentVariable("JwtSettings__SecretKey", "...", "Machine")
# ... dalsi promenne
iisreset
```

**b) V souboru `appsettings.Production.json`:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=LexiQuest;..."
  },
  "JwtSettings": {
    "SecretKey": "..."
  }
}
```

> **Upozorneni:** Soubor `appsettings.Production.json` nesmi byt v git repozitari. Pridejte ho do `.gitignore`.

---

## Kontrolni seznam pred nasazenim

- [ ] Vsechny testy prochazi (`dotnet test`)
- [ ] Promenne prostredi jsou nastaveny
- [ ] Databazove migrace jsou aplikovany
- [ ] SSL certifikat je nakonfigurovany
- [ ] CORS je nakonfigurovany na spravnou URL frontendu
- [ ] Stripe webhook je nakonfigurovany a otestovany
- [ ] VAPID klice jsou vygenerovany a nastaveny
- [ ] Health check endpoint `/health` odpovida 200
- [ ] Swagger UI je pristupny (pouze v Development prostredi)
- [ ] SignalR hub `/hubs/match` je dostupny
- [ ] Prvni admin ucet je vytvoren
- [ ] Logy se zapisuji do `logs/` adresare
- [ ] Zalohovani databaze je nastaveno
