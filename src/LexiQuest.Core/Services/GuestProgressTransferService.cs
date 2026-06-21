using System.Security.Cryptography;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Game;
using Microsoft.Extensions.Caching.Memory;

namespace LexiQuest.Core.Services;

public class GuestProgressTransferService : IGuestProgressTransferService
{
    private const string CacheKeyPrefix = "guest_progress_transfer:";
    private static readonly TimeSpan TransferExpiration = TimeSpan.FromMinutes(30);
    private readonly IMemoryCache _cache;

    public GuestProgressTransferService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public string CreateTransferToken(GuestSessionProgress progress)
    {
        if (progress.TotalXp < 0 || progress.WordsSolved < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(progress), "Guest progress values cannot be negative.");
        }

        var token = CreateUrlSafeToken();
        _cache.Set(GetCacheKey(token), progress, TransferExpiration);
        return token;
    }

    public GuestSessionProgress? ConsumeTransferToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var cacheKey = GetCacheKey(token);
        if (!_cache.TryGetValue(cacheKey, out GuestSessionProgress? progress) || progress is null)
        {
            return null;
        }

        _cache.Remove(cacheKey);
        return progress;
    }

    private static string CreateUrlSafeToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string GetCacheKey(string token) => $"{CacheKeyPrefix}{token}";
}
