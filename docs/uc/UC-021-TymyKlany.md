# UC-021: Týmy a Klany

## Popis
Hráči mohou tvořit týmy, soutěžit společně v týdenních výzvách.

## Pravidla týmů

| Parametr | Hodnota |
|----------|---------|
| Min členové | 3 |
| Max členové | 20 |
| Vytvoření | Premium nebo 1000 mincí |
| Název | Unikátní, 3-30 znaků |
| Tag | 2-4 znaky (např. LQG) |

## Týdenní týmová výzva

```
Týmy soutěží v celkovém XP:
- Součet XP všech členů = Týmové XP
- Žebříček týmů
- TOP 3 týmy dostanou odměny
```

## Role v týmu

| Role | Oprávnění |
|------|-----------|
| Leader | Vše + předat vedení + rozpustit |
| Officer | Pozvat/kick + editace info |
| Member | Hrát, chatovat |

## Hlavní tok

| Krok | Akce | Popis |
|------|------|-------|
| 1 | Uživatel otevře Týmy | - |
| 2 | Volba: Vytvořit / Najít / Pozvánka | - |
| 3a | Vytvořit | Zadat název, tag, popis |
| 3b | Najít | Seznam týmů, filtrování |
| 4 | Připojení k týmu | Žádost nebo přímé připojení |
| 5 | Dashboard týmu | Statistiky, členové, chat |
| 6 | Týdenní výzva | Týmový cíl |
| 7 | Přispívání XP | Automaticky z her |

## DTOs

```csharp
public record Team(
    Guid Id,
    string Name,
    string Tag,
    string Description,
    string? LogoUrl,
    DateTime CreatedAt,
    TeamStats Stats,
    List<TeamMember> Members,
    TeamWeeklyChallenge? CurrentChallenge
);

public record TeamMember(
    Guid UserId,
    string Username,
    string AvatarUrl,
    TeamRole Role,
    DateTime JoinedAt,
    int WeeklyXPContribution,
    int TotalXPContribution
);

public record TeamStats(
    int TotalMembers,
    int WeeklyXP,
    int AllTimeXP,
    int Rank,
    int Wins
);

public record CreateTeamRequest(
    string Name,
    string Tag,
    string Description
);

public enum TeamRole
{
    Leader,
    Officer,
    Member
}
```

## Resource klíče

```
Team.Title
Team.Create.Button
Team.Find.Button
Team.Join.Button
Team.Leave.Button
Team.Members.Title
Team.Stats.WeeklyXP
Team.Stats.TotalXP
Team.Stats.Rank
Team.Role.Leader
Team.Role.Officer
Team.Role.Member
Team.Challenge.Progress
Team.Challenge.Goal
Team.Chat.Title
Team.Error.AlreadyInTeam
Team.Error.Full
Team.Error.NameTaken
```

## Odhad: 16h
