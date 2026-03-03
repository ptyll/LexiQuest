namespace LexiQuest.Infrastructure.Caching;

public record CachePolicy(TimeSpan AbsoluteExpiration, TimeSpan? SlidingExpiration = null)
{
    public static readonly CachePolicy ShortLived = new(TimeSpan.FromMinutes(5));
    public static readonly CachePolicy MediumLived = new(TimeSpan.FromMinutes(30));
    public static readonly CachePolicy LongLived = new(TimeSpan.FromHours(2));
    public static readonly CachePolicy Daily = new(TimeSpan.FromHours(24));
}
