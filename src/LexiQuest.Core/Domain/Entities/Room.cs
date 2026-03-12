using System.Text;
using LexiQuest.Core.Domain.ValueObjects;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Domain.Entities;

/// <summary>
/// Represents a private multiplayer room.
/// </summary>
public class Room
{
    private static readonly Random Random = new();

    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public Guid Player1UserId { get; private set; }
    public string Player1Username { get; private set; } = string.Empty;
    public Guid? Player2UserId { get; private set; }
    public string? Player2Username { get; private set; }
    
    public RoomStatus Status { get; private set; }
    public RoomSettings Settings { get; private set; } = null!;
    
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    
    public string? Player1ConnectionId { get; set; }
    public string? Player2ConnectionId { get; set; }
    
    public bool Player1Ready { get; private set; }
    public bool Player2Ready { get; private set; }
    
    public Guid? CurrentMatchId { get; set; }
    public int CurrentGameIndex { get; private set; }
    public int Player1Wins { get; private set; }
    public int Player2Wins { get; private set; }

    private Room() { } // EF Core constructor

    /// <summary>
    /// Creates a new room with generated code.
    /// </summary>
    public static Room Create(Guid hostUserId, string hostUsername, RoomSettings settings)
    {
        return new Room
        {
            Id = Guid.NewGuid(),
            Code = GenerateRoomCode(),
            Player1UserId = hostUserId,
            Player1Username = hostUsername,
            Settings = settings,
            Status = RoomStatus.WaitingForOpponent,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            CurrentGameIndex = 1
        };
    }

    /// <summary>
    /// Generates a unique room code in format "LEXIQ-XXXX" where X is alphanumeric uppercase.
    /// </summary>
    private static string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var codeBuilder = new StringBuilder("LEXIQ-");
        
        for (int i = 0; i < 4; i++)
        {
            codeBuilder.Append(chars[Random.Next(chars.Length)]);
        }
        
        return codeBuilder.ToString();
    }

    /// <summary>
    /// Checks if the room has expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    /// <summary>
    /// Checks if both players are ready.
    /// </summary>
    public bool BothReady => Player1Ready && Player2Ready && Player2UserId.HasValue;

    /// <summary>
    /// Checks if the series is complete (Best of X).
    /// </summary>
    public bool IsSeriesComplete
    {
        get
        {
            int winsNeeded = (Settings.BestOf / 2) + 1;
            return Player1Wins >= winsNeeded || Player2Wins >= winsNeeded;
        }
    }

    /// <summary>
    /// Adds player 2 to the room.
    /// </summary>
    public void JoinRoom(Guid player2UserId, string player2Username)
    {
        if (Player2UserId.HasValue)
            throw new InvalidOperationException("Room is already full");
        
        if (player2UserId == Player1UserId)
            throw new InvalidOperationException("Cannot join your own room");

        Player2UserId = player2UserId;
        Player2Username = player2Username;
        Status = RoomStatus.Lobby;
    }

    /// <summary>
    /// Sets a player's ready status.
    /// </summary>
    public void SetReady(Guid userId)
    {
        if (userId == Player1UserId)
            Player1Ready = true;
        else if (userId == Player2UserId)
            Player2Ready = true;
        else
            throw new InvalidOperationException("User is not in this room");
    }

    /// <summary>
    /// Sets a player's not ready status.
    /// </summary>
    public void SetNotReady(Guid userId)
    {
        if (userId == Player1UserId)
            Player1Ready = false;
        else if (userId == Player2UserId)
            Player2Ready = false;
        else
            throw new InvalidOperationException("User is not in this room");
    }

    /// <summary>
    /// Starts the game when both players are ready.
    /// </summary>
    public void StartGame()
    {
        if (!BothReady)
            throw new InvalidOperationException("Both players must be ready");

        Status = RoomStatus.Playing;
        CurrentMatchId = Guid.NewGuid();
    }

    /// <summary>
    /// Records the result of a game in the series.
    /// </summary>
    public void RecordGameResult(Guid winnerId)
    {
        if (winnerId == Player1UserId)
            Player1Wins++;
        else if (winnerId == Player2UserId)
            Player2Wins++;
        else
            throw new InvalidOperationException("Invalid winner ID");

        CurrentGameIndex++;

        if (IsSeriesComplete)
        {
            Status = RoomStatus.Completed;
        }
        else
        {
            Status = RoomStatus.BetweenGames;
            Player1Ready = false;
            Player2Ready = false;
        }
    }

    /// <summary>
    /// Resets the room for a rematch (new series).
    /// </summary>
    public void ResetForRematch()
    {
        Player1Ready = false;
        Player2Ready = false;
        Player1Wins = 0;
        Player2Wins = 0;
        CurrentGameIndex = 1;
        CurrentMatchId = null;
        Status = RoomStatus.Lobby;
    }

    /// <summary>
    /// Removes a player from the room.
    /// </summary>
    public void LeaveRoom(Guid userId)
    {
        if (userId == Player1UserId)
        {
            // Host leaving cancels the room
            Status = RoomStatus.Cancelled;
        }
        else if (userId == Player2UserId)
        {
            // Guest leaving just removes them
            Player2UserId = null;
            Player2Username = null;
            Player2Ready = false;
            Player2ConnectionId = null;
            Status = RoomStatus.WaitingForOpponent;
        }
        else
        {
            throw new InvalidOperationException("User is not in this room");
        }
    }

    /// <summary>
    /// Checks if a user is a participant in this room.
    /// </summary>
    public bool HasPlayer(Guid userId)
    {
        return userId == Player1UserId || userId == Player2UserId;
    }

    /// <summary>
    /// Marks the room as expired.
    /// </summary>
    public void Expire()
    {
        Status = RoomStatus.Expired;
    }
}
