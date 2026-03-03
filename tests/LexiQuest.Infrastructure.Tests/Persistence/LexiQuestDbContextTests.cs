using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Tests.Persistence;

public class LexiQuestDbContextTests
{
    private static LexiQuestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LexiQuestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new LexiQuestDbContext(options);
    }

    [Fact]
    public void Constructor_CreatesInstance()
    {
        using var context = CreateContext();

        context.Should().NotBeNull();
        context.Users.Should().NotBeNull();
        context.Words.Should().NotBeNull();
        context.GameSessions.Should().NotBeNull();
        context.GameRounds.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveChanges_PersistsWord()
    {
        using var context = CreateContext();
        var word = Word.Create("test", DifficultyLevel.Beginner, WordCategory.Animals);

        context.Words.Add(word);
        await context.SaveChangesAsync();

        var saved = await context.Words.FirstOrDefaultAsync();
        saved.Should().NotBeNull();
        saved!.Original.Should().Be("test");
    }
}
