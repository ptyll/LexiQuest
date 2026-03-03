# UC-003: Obnova hesla

## Popis
Umožňuje uživateli obnovit zapomenuté heslo pomocí emailu s resetovacím odkazem.

## Aktéři
- **Primary Actor:** Uživatel se zapomenutým heslem
- **Secondary Actor:** Email service

## Post-conditions
**Úspěch:**
- Uživatel obdrží email s odkazem
- Staré heslo je neplatné po vytvoření nového

## Hlavní tok

| Krok | Akce | Data |
|------|------|------|
| 1 | Uživatel klikne "Zapomněl jsi heslo?" | - |
| 2 | Systém zobrazí formulář pro zadání emailu | - |
| 3 | Uživatel zadá email | email: string |
| 4 | Uživatel klikne "Odeslat odkaz" | - |
| 5 | FE odešle POST /api/v1/users/forgot-password | ForgotPasswordRequest |
| 6 | BE vygeneruje unikátní token (expirace 1 hodina) | token: GUID |
| 7 | BE uloží token do PasswordResets tabulky | - |
| 8 | BE odešle email s odkazem | - |
| 9 | BE vrátí 200 OK (i když email neexistuje - bezpečnost) | - |
| 10 | FE zobrazí zprávu "Pokud účet existuje, byl odeslán email" | - |
| 11 | Uživatel klikne odkaz v emailu | - |
| 12 | Systém zobrazí formulář pro nové heslo | - |
| 13 | Uživatel zadá nové heslo 2x | password, confirm |
| 14 | FE odešle POST /api/v1/users/reset-password | ResetPasswordRequest |
| 15 | BE validuje token | - |
| 16 | BE aktualizuje heslo | - |
| 17 | BE invaliduje token | - |
| 18 | BE vrátí úspěch | - |
| 19 | FE přesměruje na login se zprávou "Heslo bylo změněno" | - |

## Business pravidla
- Token expiruje po 1 hodině
- Token je jednorázový
- Nové heslo nesmí být stejné jako posledních 5 hesel (volitelně)

## Resource klíče
```
PasswordReset.Title
PasswordReset.Description
PasswordReset.Input.Email.Label
PasswordReset.Button.Submit
PasswordReset.Success.Message
PasswordReset.NewPassword.Title
PasswordReset.NewPassword.Input.Label
PasswordReset.NewPassword.Confirm.Label
PasswordReset.NewPassword.Button.Submit
PasswordReset.Error.TokenExpired
PasswordReset.Error.TokenInvalid
```

## Odhad: 6h
