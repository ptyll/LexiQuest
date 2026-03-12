using System.Collections.Concurrent;
using LexiQuest.Core.Interfaces.Services;

namespace LexiQuest.Core.Services;

/// <summary>
/// Service for managing matchmaking queue and creating matches between players.
/// </summary>
public class MatchmakingService : IMatchmakingService
{
    private readonly ConcurrentDictionary<Guid, QueuedPlayer> _queue = new();
    private readonly TimeSpan _matchmakingTimeout;
    private readonly Timer _matchingTimer;
    private readonly Timer _timeoutTimer;
    private const int LevelTolerance = 3;

    public event EventHandler<MatchFoundEventArgs>? OnMatchFound;
    public event EventHandler<MatchmakingTimeoutEventArgs>? OnMatchmakingTimeout;

    public MatchmakingService() : this(TimeSpan.FromSeconds(30))
    {
    }

    public MatchmakingService(TimeSpan matchmakingTimeout)
    {
        _matchmakingTimeout = matchmakingTimeout;
        // Run matching algorithm every 100ms
        _matchingTimer = new Timer(_ => TryMatchPlayers(), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
        // Check for timeouts - use shorter interval for tests
        var timeoutCheckInterval = matchmakingTimeout < TimeSpan.FromSeconds(1) 
            ? TimeSpan.FromMilliseconds(50) 
            : TimeSpan.FromSeconds(1);
        _timeoutTimer = new Timer(_ => CheckTimeouts(), null, timeoutCheckInterval, timeoutCheckInterval);
    }

    public Task<bool> JoinQueueAsync(Guid userId, int level, string username, string? avatar, CancellationToken cancellationToken = default)
    {
        if (_queue.ContainsKey(userId))
        {
            return Task.FromResult(false);
        }

        var player = new QueuedPlayer
        {
            UserId = userId,
            Level = level,
            Username = username,
            Avatar = avatar,
            JoinedAt = DateTime.UtcNow
        };

        _queue.TryAdd(userId, player);
        return Task.FromResult(true);
    }

    public Task<bool> CancelQueueAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var removed = _queue.TryRemove(userId, out _);
        return Task.FromResult(removed);
    }

    public Task<bool> IsInQueueAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_queue.ContainsKey(userId));
    }

    public Task<int> GetQueueCountAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_queue.Count);
    }

    private void TryMatchPlayers()
    {
        if (_queue.Count < 2)
        {
            return;
        }

        var players = _queue.Values.OrderBy(p => p.JoinedAt).ToList();
        var matchedPlayers = new HashSet<Guid>();

        for (int i = 0; i < players.Count; i++)
        {
            var player1 = players[i];
            
            if (matchedPlayers.Contains(player1.UserId))
                continue;

            // Find best match (prefer similar level, then by wait time)
            QueuedPlayer? bestMatch = null;
            var bestMatchScore = int.MaxValue;

            for (int j = i + 1; j < players.Count; j++)
            {
                var player2 = players[j];
                
                if (matchedPlayers.Contains(player2.UserId))
                    continue;

                var levelDiff = Math.Abs(player1.Level - player2.Level);
                
                // Prefer players within level tolerance
                if (levelDiff <= LevelTolerance)
                {
                    // Lower score is better (level diff is primary, wait time secondary)
                    var score = levelDiff * 1000 + (int)(DateTime.UtcNow - player2.JoinedAt).TotalSeconds;
                    
                    if (score < bestMatchScore)
                    {
                        bestMatchScore = score;
                        bestMatch = player2;
                    }
                }
                // If no match within tolerance found yet, consider anyone
                else if (bestMatch == null)
                {
                    var score = levelDiff * 1000 + (int)(DateTime.UtcNow - player2.JoinedAt).TotalSeconds;
                    
                    if (score < bestMatchScore)
                    {
                        bestMatchScore = score;
                        bestMatch = player2;
                    }
                }
            }

            if (bestMatch != null)
            {
                // Create match
                CreateMatch(player1, bestMatch);
                matchedPlayers.Add(player1.UserId);
                matchedPlayers.Add(bestMatch.UserId);
                
                // Remove from queue
                _queue.TryRemove(player1.UserId, out _);
                _queue.TryRemove(bestMatch.UserId, out _);
            }
        }
    }

    private void CreateMatch(QueuedPlayer player1, QueuedPlayer player2)
    {
        var matchId = Guid.NewGuid();
        
        OnMatchFound?.Invoke(this, new MatchFoundEventArgs
        {
            MatchId = matchId,
            Player1Id = player1.UserId,
            Player2Id = player2.UserId,
            Player1Username = player1.Username,
            Player2Username = player2.Username,
            Player1Level = player1.Level,
            Player2Level = player2.Level,
            Player1Avatar = player1.Avatar,
            Player2Avatar = player2.Avatar
        });
    }

    private void CheckTimeouts()
    {
        var now = DateTime.UtcNow;
        var timedOutPlayers = _queue
            .Where(p => now - p.Value.JoinedAt > _matchmakingTimeout)
            .Select(p => p.Value)
            .ToList();

        foreach (var player in timedOutPlayers)
        {
            _queue.TryRemove(player.UserId, out _);
            OnMatchmakingTimeout?.Invoke(this, new MatchmakingTimeoutEventArgs { UserId = player.UserId });
        }
    }

    private class QueuedPlayer
    {
        public Guid UserId { get; set; }
        public int Level { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
