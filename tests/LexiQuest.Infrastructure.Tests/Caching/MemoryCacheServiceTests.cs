using FluentAssertions;
using LexiQuest.Core.Interfaces;
using LexiQuest.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace LexiQuest.Infrastructure.Tests.Caching;

public class MemoryCacheServiceTests
{
    private readonly ICacheService _sut;

    public MemoryCacheServiceTests()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        _sut = new MemoryCacheService(memoryCache);
    }

    [Fact]
    public async Task GetOrCreate_ReturnsCachedValue()
    {
        var callCount = 0;
        var result1 = await _sut.GetOrCreateAsync("key1", () =>
        {
            callCount++;
            return Task.FromResult("value1");
        }, TimeSpan.FromMinutes(5));

        var result2 = await _sut.GetOrCreateAsync("key1", () =>
        {
            callCount++;
            return Task.FromResult("value2");
        }, TimeSpan.FromMinutes(5));

        result1.Should().Be("value1");
        result2.Should().Be("value1");
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task GetOrCreate_CallsFactoryOnMiss()
    {
        var factoryCalled = false;
        await _sut.GetOrCreateAsync("miss-key", () =>
        {
            factoryCalled = true;
            return Task.FromResult(42);
        }, TimeSpan.FromMinutes(5));

        factoryCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Remove_InvalidatesCache()
    {
        await _sut.GetOrCreateAsync("remove-key", () => Task.FromResult("original"), TimeSpan.FromMinutes(5));

        _sut.Remove("remove-key");

        var result = await _sut.GetOrCreateAsync("remove-key", () => Task.FromResult("new-value"), TimeSpan.FromMinutes(5));
        result.Should().Be("new-value");
    }

    [Fact]
    public async Task GetOrCreate_RespectsExpiration()
    {
        var shortCache = new MemoryCache(new MemoryCacheOptions());
        var service = new MemoryCacheService(shortCache);

        await service.GetOrCreateAsync("expire-key", () => Task.FromResult("value"), TimeSpan.FromMilliseconds(1));

        await Task.Delay(50);

        var callCount = 0;
        await service.GetOrCreateAsync("expire-key", () =>
        {
            callCount++;
            return Task.FromResult("new-value");
        }, TimeSpan.FromMinutes(5));

        callCount.Should().Be(1);
    }
}
