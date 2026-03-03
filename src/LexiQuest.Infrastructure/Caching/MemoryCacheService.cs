using LexiQuest.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace LexiQuest.Infrastructure.Caching;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan absoluteExpiration)
    {
        if (_cache.TryGetValue(key, out T? cachedValue) && cachedValue is not null)
        {
            return cachedValue;
        }

        var value = await factory();
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration
        };

        _cache.Set(key, value, options);
        return value;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }
}
