using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Infrastructure.Persistence.Repositories;
using LexiQuest.Shared.Enums;
using NSubstitute;
using Xunit;

namespace LexiQuest.Infrastructure.Tests.Repositories;

public class CachedWordRepositoryTests
{
    private readonly IWordRepository _innerRepository;
    private readonly ICacheService _cacheService;
    private readonly CachedWordRepository _sut;

    public CachedWordRepositoryTests()
    {
        _innerRepository = Substitute.For<IWordRepository>();
        _cacheService = Substitute.For<ICacheService>();
        _sut = new CachedWordRepository(_innerRepository, _cacheService);
    }

    [Fact]
    public async Task CachedWordRepository_GetByDifficulty_UsesCacheFirst()
    {
        // Arrange
        var cachedWords = new List<Word> { Word.Create("test", DifficultyLevel.Beginner, WordCategory.Animals) };
        _cacheService.GetOrCreateAsync(
                Arg.Is<string>(k => k.Contains("words:difficulty")),
                Arg.Any<Func<Task<IReadOnlyList<Word>>>>(),
                Arg.Any<TimeSpan>())
            .Returns(Task.FromResult<IReadOnlyList<Word>>(cachedWords.AsReadOnly()));

        // Act
        var result = await _sut.GetByDifficultyAsync(DifficultyLevel.Beginner);

        // Assert
        result.Should().HaveCount(1);
        result[0].Original.Should().Be("test");
        await _innerRepository.DidNotReceive().GetByDifficultyAsync(
            Arg.Any<DifficultyLevel>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CachedWordRepository_CacheExpired_RefreshesFromDb()
    {
        // Arrange
        var dbWords = new List<Word> { Word.Create("fresh", DifficultyLevel.Intermediate, WordCategory.Animals) };

        _cacheService.GetOrCreateAsync(
                Arg.Any<string>(),
                Arg.Any<Func<Task<IReadOnlyList<Word>>>>(),
                Arg.Any<TimeSpan>())
            .Returns(callInfo =>
            {
                var factory = callInfo.ArgAt<Func<Task<IReadOnlyList<Word>>>>(1);
                return factory();
            });

        _innerRepository.GetByDifficultyAsync(DifficultyLevel.Intermediate, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<Word>>(dbWords.AsReadOnly()));

        // Act
        var result = await _sut.GetByDifficultyAsync(DifficultyLevel.Intermediate);

        // Assert
        result.Should().HaveCount(1);
        result[0].Original.Should().Be("fresh");
        await _innerRepository.Received(1).GetByDifficultyAsync(
            DifficultyLevel.Intermediate, Arg.Any<CancellationToken>());
    }
}
