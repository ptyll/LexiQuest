using LexiQuest.Shared.Enums;

namespace LexiQuest.Shared.DTOs.Multiplayer;

/// <summary>
/// DTO reprezentující kompletní stav místnosti pro SignalR synchronizaci.
/// </summary>
public record RoomStateDto(
    string Code,
    RoomStatus Status,
    PlayerDto? Player1,
    PlayerDto? Player2,
    RoomSettingsDto Settings,
    Guid? CurrentMatchId,
    DateTime ExpiresAt);

/// <summary>
/// DTO reprezentující hráče v místnosti.
/// </summary>
public record PlayerDto(
    Guid UserId,
    string Username,
    bool IsReady);

/// <summary>
/// DTO reprezentující stav připravenosti hráče.
/// </summary>
public record PlayerReadyStateDto(
    Guid UserId,
    string Username,
    bool IsReady);
