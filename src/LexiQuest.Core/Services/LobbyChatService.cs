using System.Collections.Concurrent;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs;

namespace LexiQuest.Core.Services;

/// <summary>
/// Service pro správu lobby chatu v místnostech
/// </summary>
public class LobbyChatService : ILobbyChatService
{
    private readonly IRoomService _roomService;
    private readonly ConcurrentDictionary<string, List<ChatMessage>> _roomMessages = new();
    private readonly ConcurrentDictionary<Guid, List<DateTime>> _userRateLimits = new();

    private const int MaxMessageLength = 200;
    private const int MaxMessagesPerWindow = 5;
    private const int RateLimitWindowSeconds = 10;
    private const int MaxChatHistory = 100;

    public LobbyChatService(IRoomService roomService)
    {
        _roomService = roomService;
    }

    public async Task<(bool Success, string? Error)> SendMessageAsync(
        string roomCode,
        Guid userId,
        string username,
        string content,
        CancellationToken cancellationToken = default)
    {
        // Kontrola existence místnosti
        var room = await _roomService.GetRoomAsync(roomCode, cancellationToken);
        if (room == null)
            return (false, "Room not found");

        // Kontrola, že uživatel je účastník místnosti
        if (room.Player1UserId != userId && room.Player2UserId != userId)
            return (false, "Only room participants can send messages");

        // Validace obsahu zprávy
        if (string.IsNullOrWhiteSpace(content))
            return (false, "Message cannot be empty");

        if (content.Length > MaxMessageLength)
            return (false, $"Message exceeds maximum length of {MaxMessageLength} characters");

        // Rate limiting
        if (IsRateLimited(userId))
            return (false, "Rate limit exceeded. Please wait before sending more messages.");

        // Uložení času zprávy pro rate limiting
        RecordMessageTime(userId);

        // Vytvoření a uložení zprávy
        var message = new ChatMessage
        {
            Id = Guid.NewGuid(),
            RoomCode = roomCode,
            UserId = userId,
            Username = username,
            Content = content.Trim(),
            Timestamp = DateTime.UtcNow
        };

        var messages = _roomMessages.GetOrAdd(roomCode, _ => new List<ChatMessage>());
        lock (messages)
        {
            messages.Add(message);
            // Keep only last MaxChatHistory messages
            if (messages.Count > MaxChatHistory)
            {
                messages.RemoveAt(0);
            }
        }

        return (true, null);
    }

    public Task<IReadOnlyList<LobbyChatMessageDto>> GetChatHistoryAsync(
        string roomCode,
        CancellationToken cancellationToken = default)
    {
        if (!_roomMessages.TryGetValue(roomCode, out var messages))
            return Task.FromResult<IReadOnlyList<LobbyChatMessageDto>>(Array.Empty<LobbyChatMessageDto>());

        lock (messages)
        {
            var dtos = messages
                .Select(m => new LobbyChatMessageDto(
                    m.Id,
                    m.RoomCode,
                    m.UserId,
                    m.Username,
                    m.Content,
                    m.Timestamp))
                .ToList();

            return Task.FromResult<IReadOnlyList<LobbyChatMessageDto>>(dtos);
        }
    }

    public Task ClearChatAsync(string roomCode, CancellationToken cancellationToken = default)
    {
        _roomMessages.TryRemove(roomCode, out _);
        return Task.CompletedTask;
    }

    private bool IsRateLimited(Guid userId)
    {
        if (!_userRateLimits.TryGetValue(userId, out var messageTimes))
            return false;

        var cutoff = DateTime.UtcNow.AddSeconds(-RateLimitWindowSeconds);
        lock (messageTimes)
        {
            // Remove old entries
            messageTimes.RemoveAll(t => t < cutoff);
            return messageTimes.Count >= MaxMessagesPerWindow;
        }
    }

    private void RecordMessageTime(Guid userId)
    {
        var messageTimes = _userRateLimits.GetOrAdd(userId, _ => new List<DateTime>());
        lock (messageTimes)
        {
            messageTimes.Add(DateTime.UtcNow);
        }
    }

    /// <summary>
    /// Interní reprezentace chat zprávy
    /// </summary>
    private class ChatMessage
    {
        public Guid Id { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
