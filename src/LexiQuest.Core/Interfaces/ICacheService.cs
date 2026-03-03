namespace LexiQuest.Core.Interfaces;

public interface ICacheService
{
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan absoluteExpiration);
    void Remove(string key);
}
