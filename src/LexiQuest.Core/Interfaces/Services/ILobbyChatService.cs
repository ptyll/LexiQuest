using LexiQuest.Shared.DTOs;

namespace LexiQuest.Core.Interfaces.Services;

/// <summary>
/// Service pro správu lobby chatu v místnostech
/// </summary>
public interface ILobbyChatService
{
    /// <summary>
    /// Odešle zprávu do chatu místnosti
    /// </summary>
    /// <param name="roomCode">Kód místnosti</param>
    /// <param name="userId">ID uživatele</param>
    /// <param name="username">Uživatelské jméno</param>
    /// <param name="content">Obsah zprávy</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>(Success, ErrorMessage)</returns>
    Task<(bool Success, string? Error)> SendMessageAsync(
        string roomCode,
        Guid userId,
        string username,
        string content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Získá historii zpráv chatu (posledních 100 zpráv)
    /// </summary>
    /// <param name="roomCode">Kód místnosti</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Seznam zpráv</returns>
    Task<IReadOnlyList<LobbyChatMessageDto>> GetChatHistoryAsync(
        string roomCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Smaže všechny zprávy z chatu místnosti
    /// </summary>
    /// <param name="roomCode">Kód místnosti</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ClearChatAsync(
        string roomCode,
        CancellationToken cancellationToken = default);
}
