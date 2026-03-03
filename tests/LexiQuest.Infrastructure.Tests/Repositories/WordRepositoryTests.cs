using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Infrastructure.Persistence.Repositories;
using LexiQuest.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Tests.Repositories;

public class WordRepositoryTests : IDisposable
{
    private readonly LexiQuestDbContext _context;
    private readonly WordRepository _repository;

    public WordRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<LexiQuestDbContext>()
            .UseInMemoryDatabase(databaseName: $"WordTestDb_{Guid.NewGuid()}")
            .Options;
        
        _context = new LexiQuestDbContext(options);
        _repository = new WordRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task WordRepository_GetByDifficulty_ReturnsCorrectWords()
    {
        // Arrange
        var beginnerWords = new[]
        {
            Word.Create("JABLKO", DifficultyLevel.Beginner, WordCategory.Food, 1),
            Word.Create("BANÁN", DifficultyLevel.Beginner, WordCategory.Food, 2),
            Word.Create("POMERANČ", DifficultyLevel.Intermediate, WordCategory.Food, 3)
        };
        
        foreach (var word in beginnerWords)
            await _repository.AddAsync(word);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByDifficultyAsync(DifficultyLevel.Beginner);

        // Assert
        result.Should().HaveCount(2);
        result.All(w => w.Difficulty == DifficultyLevel.Beginner).Should().BeTrue();
    }

    [Fact]
    public async Task WordRepository_GetByCategory_ReturnsCorrectWords()
    {
        // Arrange
        var words = new[]
        {
            Word.Create("JABLKO", DifficultyLevel.Beginner, WordCategory.Food, 1),
            Word.Create("LEV", DifficultyLevel.Beginner, WordCategory.Animals, 4),
            Word.Create("BANÁN", DifficultyLevel.Beginner, WordCategory.Food, 2)
        };
        
        foreach (var word in words)
            await _repository.AddAsync(word);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCategoryAsync(WordCategory.Food);

        // Assert
        result.Should().HaveCount(2);
        result.All(w => w.Category == WordCategory.Food).Should().BeTrue();
    }

    [Fact]
    public async Task WordRepository_GetRandom_ReturnsRandomWord()
    {
        // Arrange
        var words = new[]
        {
            Word.Create("JABLKO", DifficultyLevel.Beginner, WordCategory.Food, 1),
            Word.Create("BANÁN", DifficultyLevel.Beginner, WordCategory.Food, 2)
        };
        
        foreach (var word in words)
            await _repository.AddAsync(word);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetRandomAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Original.Should().BeOneOf("JABLKO", "BANÁN");
    }

    [Fact]
    public async Task WordRepository_GetRandom_WithDifficultyFilter_ReturnsCorrectDifficulty()
    {
        // Arrange
        var words = new[]
        {
            Word.Create("JABLKO", DifficultyLevel.Beginner, WordCategory.Food, 1),
            Word.Create("EXPERTWORD", DifficultyLevel.Expert, WordCategory.Food, 2)
        };
        
        foreach (var word in words)
            await _repository.AddAsync(word);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetRandomAsync(DifficultyLevel.Expert);

        // Assert
        result.Should().NotBeNull();
        result!.Difficulty.Should().Be(DifficultyLevel.Expert);
        result.Original.Should().Be("EXPERTWORD");
    }

    [Fact]
    public async Task WordRepository_GetRandomBatch_ReturnsNonRepeating()
    {
        // Arrange
        var words = new[]
        {
            Word.Create("JABLKO", DifficultyLevel.Beginner, WordCategory.Food, 1),
            Word.Create("BANÁN", DifficultyLevel.Beginner, WordCategory.Food, 2),
            Word.Create("POMERANČ", DifficultyLevel.Beginner, WordCategory.Food, 3),
            Word.Create("HRUŠKA", DifficultyLevel.Beginner, WordCategory.Food, 4),
            Word.Create("ŠVESTKA", DifficultyLevel.Beginner, WordCategory.Food, 5)
        };
        
        foreach (var word in words)
            await _repository.AddAsync(word);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetRandomBatchAsync(5);

        // Assert
        result.Should().HaveCount(5);
        result.Select(w => w.Original).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task WordRepository_GetRandomBatch_WithDifficultyFilter_ReturnsCorrectWords()
    {
        // Arrange
        var words = new[]
        {
            Word.Create("JABLKO", DifficultyLevel.Beginner, WordCategory.Food, 1),
            Word.Create("BANÁN", DifficultyLevel.Beginner, WordCategory.Food, 2),
            Word.Create("POMERANČ", DifficultyLevel.Intermediate, WordCategory.Food, 3),
            Word.Create("HRUŠKA", DifficultyLevel.Intermediate, WordCategory.Food, 4),
            Word.Create("ŠVESTKA", DifficultyLevel.Expert, WordCategory.Food, 5)
        };
        
        foreach (var word in words)
            await _repository.AddAsync(word);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetRandomBatchAsync(2, DifficultyLevel.Beginner);

        // Assert
        result.Should().HaveCount(2);
        result.All(w => w.Difficulty == DifficultyLevel.Beginner).Should().BeTrue();
    }
}
