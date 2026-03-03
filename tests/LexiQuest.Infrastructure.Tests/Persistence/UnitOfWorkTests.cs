using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Tests.Persistence;

public class UnitOfWorkTests
{
    private static (LexiQuestDbContext, UnitOfWork) CreateUoW()
    {
        var options = new DbContextOptionsBuilder<LexiQuestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new LexiQuestDbContext(options);
        var uow = new UnitOfWork(context);
        return (context, uow);
    }

    [Fact]
    public async Task SaveChanges_PersistsAllChanges()
    {
        var (context, uow) = CreateUoW();

        var word1 = Word.Create("pes", DifficultyLevel.Beginner, WordCategory.Animals);
        var word2 = Word.Create("kocka", DifficultyLevel.Beginner, WordCategory.Animals);

        context.Words.Add(word1);
        context.Words.Add(word2);

        await uow.SaveChangesAsync();

        var count = await context.Words.CountAsync();
        count.Should().Be(2);

        await context.DisposeAsync();
    }
}
