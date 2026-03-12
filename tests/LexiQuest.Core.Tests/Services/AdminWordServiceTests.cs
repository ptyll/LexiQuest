using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Services;
using LexiQuest.Shared.DTOs.Admin;
using LexiQuest.Shared.Enums;
using NSubstitute;

namespace LexiQuest.Core.Tests.Services;

public class AdminWordServiceTests
{
    private readonly IWordRepository _wordRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AdminWordService _sut;

    public AdminWordServiceTests()
    {
        _wordRepository = Substitute.For<IWordRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new AdminWordService(_wordRepository, _unitOfWork);
    }

    [Fact]
    public async Task AdminWordService_GetWords_ReturnsPaginatedList()
    {
        // Arrange
        var words = new List<Word>
        {
            Word.Create("hello", DifficultyLevel.Beginner, WordCategory.Everyday),
            Word.Create("world", DifficultyLevel.Beginner, WordCategory.Everyday)
        };

        _wordRepository.GetPaginatedAsync(
            null, null, null, null, null, 1, 25, Arg.Any<CancellationToken>())
            .Returns((words.AsReadOnly() as IReadOnlyList<Word>, 2));

        var request = new AdminWordListRequest(null, null, null, null, null, 1, 25);

        // Act
        var result = await _sut.GetWordsAsync(request);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(25);
    }

    [Fact]
    public async Task AdminWordService_CreateWord_AddsToDb()
    {
        // Arrange
        var request = new AdminWordCreateRequest("test", "Beginner", "Animals");

        // Act
        var result = await _sut.CreateWordAsync(request);

        // Assert
        result.Word.Should().Be("test");
        result.Difficulty.Should().Be("Beginner");
        result.Category.Should().Be("Animals");
        result.Length.Should().Be(4);
        await _wordRepository.Received(1).AddAsync(Arg.Any<Word>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AdminWordService_UpdateWord_ModifiesExisting()
    {
        // Arrange
        var wordId = Guid.NewGuid();
        var existingWord = Word.Create("old", DifficultyLevel.Beginner, WordCategory.Everyday);

        _wordRepository.GetByIdAsync(wordId, Arg.Any<CancellationToken>())
            .Returns(existingWord);

        var request = new AdminWordUpdateRequest("new", "Advanced", "Science");

        // Act
        var result = await _sut.UpdateWordAsync(wordId, request);

        // Assert
        result.Should().NotBeNull();
        result!.Word.Should().Be("new");
        result.Difficulty.Should().Be("Advanced");
        result.Category.Should().Be("Science");
        _wordRepository.Received(1).Remove(existingWord);
        await _wordRepository.Received(1).AddAsync(Arg.Any<Word>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AdminWordService_DeleteWord_RemovesFromDb()
    {
        // Arrange
        var wordId = Guid.NewGuid();
        var word = Word.Create("delete", DifficultyLevel.Beginner, WordCategory.Everyday);

        _wordRepository.GetByIdAsync(wordId, Arg.Any<CancellationToken>())
            .Returns(word);

        // Act
        var result = await _sut.DeleteWordAsync(wordId);

        // Assert
        result.Should().BeTrue();
        _wordRepository.Received(1).Remove(word);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AdminWordService_BulkImport_CSV_AddsMultiple()
    {
        // Arrange
        var csv = "hello,Beginner,Animals\nworld,Intermediate,Science";

        _wordRepository.GetByNormalizedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Word?)null);

        // Act
        var result = await _sut.BulkImportAsync(csv);

        // Assert
        result.Imported.Should().Be(2);
        result.Skipped.Should().Be(0);
        result.Errors.Should().Be(0);
        await _wordRepository.Received(1).AddRangeAsync(
            Arg.Is<IEnumerable<Word>>(w => w.Count() == 2),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AdminWordService_BulkImport_DuplicatesSkipped()
    {
        // Arrange
        var csv = "hello,Beginner,Animals\nworld,Intermediate,Science";
        var existingWord = Word.Create("hello", DifficultyLevel.Beginner, WordCategory.Animals);

        _wordRepository.GetByNormalizedAsync("hello", Arg.Any<CancellationToken>())
            .Returns(existingWord);
        _wordRepository.GetByNormalizedAsync("world", Arg.Any<CancellationToken>())
            .Returns((Word?)null);

        // Act
        var result = await _sut.BulkImportAsync(csv);

        // Assert
        result.Imported.Should().Be(1);
        result.Skipped.Should().Be(1);
        result.Errors.Should().Be(0);
    }

    [Fact]
    public async Task AdminWordService_Export_ReturnsCSV()
    {
        // Arrange
        var words = new List<Word>
        {
            Word.Create("hello", DifficultyLevel.Beginner, WordCategory.Animals),
            Word.Create("world", DifficultyLevel.Advanced, WordCategory.Science)
        };

        _wordRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(words.AsReadOnly());

        // Act
        var result = await _sut.ExportAsync();

        // Assert
        result.Should().Contain("hello,Beginner,Animals");
        result.Should().Contain("world,Advanced,Science");
    }

    [Fact]
    public async Task AdminWordService_GetStats_ReturnsDifficultyDistribution()
    {
        // Arrange
        _wordRepository.CountByDifficultyAsync(DifficultyLevel.Beginner, Arg.Any<CancellationToken>())
            .Returns(10);
        _wordRepository.CountByDifficultyAsync(DifficultyLevel.Intermediate, Arg.Any<CancellationToken>())
            .Returns(20);
        _wordRepository.CountByDifficultyAsync(DifficultyLevel.Advanced, Arg.Any<CancellationToken>())
            .Returns(15);
        _wordRepository.CountByDifficultyAsync(DifficultyLevel.Expert, Arg.Any<CancellationToken>())
            .Returns(5);

        // Act
        var result = await _sut.GetStatsAsync();

        // Assert
        result.TotalWords.Should().Be(50);
        result.DifficultyDistribution["Beginner"].Should().Be(10);
        result.DifficultyDistribution["Intermediate"].Should().Be(20);
        result.DifficultyDistribution["Advanced"].Should().Be(15);
        result.DifficultyDistribution["Expert"].Should().Be(5);
    }
}
