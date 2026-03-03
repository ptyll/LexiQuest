# UC-022: Vlastní slovníky

## Popis
Premium funkce - uživatelé mohou vytvářet vlastní sady slov pro trénink.

## Pravidla

| Parametr | Hodnota |
|----------|---------|
| Dostupnost | Premium only |
| Max slovníků | 10 |
| Max slov na slovník | 100 |
| Min slovo | 3 písmena |
| Max slovo | 20 písmen |
| Veřejné sdílení | Volitelné |

## Hlavní tok

| Krok | Akce | Popis |
|------|------|-------|
| 1 | Uživatel otevře "Moje slovníky" | - |
| 2 | Zobrazení existujících slovníků | - |
| 3 | Klikne "Vytvořit slovník" | - |
| 4 | Zadá název a popis | Validace |
| 5 | Přidává slova | Jedno po druhém nebo import |
| 6 | Kontrola validity | Existence v hlavním slovníku |
| 7 | Uložení slovníku | - |
| 8 | Možnost trénovat | Spustit s vlastními slovy |
| 9 | Možnost sdílet | Veřejný/privátní |

## Import slov

```csharp
public class DictionaryImportService
{
    public async Task<ImportResult> ImportWordsAsync(Guid userId, string content)
    {
        // Podporované formáty:
        // - CSV: slovo,difficulty
        // - TXT: jedno slovo na řádek
        // - JSON: [{"word": "...", "difficulty": 1}]
        
        // Validace:
        // - Min délka 3
        // - Max délka 20
        // - Povolené znaky
        // - Existuje v hlavní DB nebo je validní
    }
}
```

## DTOs

```csharp
public record CustomDictionary(
    Guid Id,
    Guid UserId,
    string Name,
    string Description,
    bool IsPublic,
    DateTime CreatedAt,
    int WordCount,
    List<DictionaryWord> Words,
    int? Downloads  // pokud veřejný
);

public record DictionaryWord(
    Guid Id,
    string Original,
    string? Hint,
    DifficultyLevel Difficulty
);

public record CreateDictionaryRequest(
    string Name,
    string Description,
    bool IsPublic,
    List<string> Words
);

public record ImportWordsRequest(
    string Content,
    string Format  // csv, txt, json
);
```

## Resource klíče

```
Dictionary.Title
Dictionary.Create.Button
Dictionary.Import.Button
Dictionary.Word.Add
Dictionary.Word.Remove
Dictionary.Word.Hint
Dictionary.Public.Label
Dictionary.Private.Label
Dictionary.Share.Button
Dictionary.Train.Button
Dictionary.Count.Words
Dictionary.Error.TooMany
Dictionary.Error.WordTooShort
Dictionary.Error.InvalidChars
Dictionary.Import.Success
Dictionary.Import.Errors
```

## Odhad: 10h
