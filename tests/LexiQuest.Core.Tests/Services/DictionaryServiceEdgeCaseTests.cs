using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Services;
using LexiQuest.Shared.Enums;
using NSubstitute;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

public class DictionaryServiceEdgeCaseTests
{
    private readonly ICustomDictionaryRepository _dictionaryRepo = Substitute.For<ICustomDictionaryRepository>();
    private readonly IDictionaryWordRepository _wordRepo = Substitute.For<IDictionaryWordRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DictionaryService _sut;

    public DictionaryServiceEdgeCaseTests()
    {
        _sut = new DictionaryService(_dictionaryRepo, _wordRepo, _unitOfWork);
    }

    private CustomDictionary CreateDictionaryForUser(Guid userId)
    {
        var dict = CustomDictionary.Create(userId, "Test Dict", "Description");
        _dictionaryRepo.GetByIdAsync(dict.Id).Returns(dict);
        return dict;
    }

    // --- Malformed CSV (no commas, extra commas) ---

    [Fact]
    public async Task ImportCsv_NoCommas_TreatsEntireLineAsWord()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dict = CreateDictionaryForUser(userId);
        var csv = "hello\nworld\ntest";

        // Act
        var result = await _sut.ImportWordsFromCsvAsync(dict.Id, userId, csv);

        // Assert - words without commas still import (first part is the word)
        result.ImportedCount.Should().Be(3);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ImportCsv_ExtraCommas_ParsesFirstTwoColumnsOnly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dict = CreateDictionaryForUser(userId);
        var csv = "hello,Beginner,extra,columns,here";

        // Act
        var result = await _sut.ImportWordsFromCsvAsync(dict.Id, userId, csv);

        // Assert - should import "hello" with Beginner difficulty, ignoring extra commas
        result.ImportedCount.Should().Be(1);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ImportCsv_InvalidDifficulty_UsesAutoDetect()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dict = CreateDictionaryForUser(userId);
        var csv = "hello,NotADifficulty";

        // Act
        var result = await _sut.ImportWordsFromCsvAsync(dict.Id, userId, csv);

        // Assert - "NotADifficulty" won't parse, auto-detect is used, word still imports
        result.ImportedCount.Should().Be(1);
        result.Errors.Should().BeEmpty();
    }

    // --- Empty file import ---

    [Fact]
    public async Task ImportCsv_EmptyContent_ReturnsZeroImported()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dict = CreateDictionaryForUser(userId);

        // Act
        var result = await _sut.ImportWordsFromCsvAsync(dict.Id, userId, "");

        // Assert
        result.ImportedCount.Should().Be(0);
        result.SkippedCount.Should().Be(0);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ImportTxt_EmptyContent_ReturnsZeroImported()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dict = CreateDictionaryForUser(userId);

        // Act
        var result = await _sut.ImportWordsFromTxtAsync(dict.Id, userId, "");

        // Assert
        result.ImportedCount.Should().Be(0);
    }

    [Fact]
    public async Task ImportJson_EmptyArray_ReturnsZeroImported()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dict = CreateDictionaryForUser(userId);

        // Act
        var result = await _sut.ImportWordsFromJsonAsync(dict.Id, userId, "[]");

        // Assert
        result.ImportedCount.Should().Be(0);
        result.Errors.Should().BeEmpty();
    }

    // --- Very long word in import ---

    [Fact]
    public async Task ImportCsv_VeryLongWord_ReportsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dict = CreateDictionaryForUser(userId);
        var longWord = new string('a', 51); // Exceeds 50 char limit
        var csv = $"{longWord},Beginner";

        // Act
        var result = await _sut.ImportWordsFromCsvAsync(dict.Id, userId, csv);

        // Assert - DictionaryWord.Create should throw ArgumentException
        result.ImportedCount.Should().Be(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    public async Task ImportTxt_VeryLongWord_ReportsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dict = CreateDictionaryForUser(userId);
        var longWord = new string('a', 51);

        // Act
        var result = await _sut.ImportWordsFromTxtAsync(dict.Id, userId, longWord);

        // Assert
        result.ImportedCount.Should().Be(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    public async Task ImportCsv_WordExactly50Chars_Succeeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dict = CreateDictionaryForUser(userId);
        var word = new string('a', 50); // Exactly at limit

        // Act
        var result = await _sut.ImportWordsFromCsvAsync(dict.Id, userId, word);

        // Assert
        result.ImportedCount.Should().Be(1);
    }

    // --- Invalid characters in imported words ---

    [Fact]
    public async Task ImportCsv_WordWithNumbers_ReportsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dict = CreateDictionaryForUser(userId);
        var csv = "abc123,Beginner";

        // Act
        var result = await _sut.ImportWordsFromCsvAsync(dict.Id, userId, csv);

        // Assert
        result.ImportedCount.Should().Be(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    public async Task ImportCsv_WordWithSpaces_ReportsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dict = CreateDictionaryForUser(userId);
        // After split by comma, first part is "hello world" (has space)
        var csv = "hello world,Beginner";

        // Act
        var result = await _sut.ImportWordsFromCsvAsync(dict.Id, userId, csv);

        // Assert
        result.ImportedCount.Should().Be(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    public async Task ImportCsv_WordTooShort_ReportsError()
    {
        // Arrange - minimum word length is 3
        var userId = Guid.NewGuid();
        var dict = CreateDictionaryForUser(userId);
        var csv = "ab,Beginner";

        // Act
        var result = await _sut.ImportWordsFromCsvAsync(dict.Id, userId, csv);

        // Assert
        result.ImportedCount.Should().Be(0);
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    public async Task ImportCsv_CzechCharacters_Succeeds()
    {
        // Arrange - Czech diacritics should be allowed
        var userId = Guid.NewGuid();
        var dict = CreateDictionaryForUser(userId);
        var csv = "příšerně,Advanced";

        // Act
        var result = await _sut.ImportWordsFromCsvAsync(dict.Id, userId, csv);

        // Assert
        result.ImportedCount.Should().Be(1);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ImportCsv_HyphenatedWord_Succeeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dict = CreateDictionaryForUser(userId);
        var csv = "česko-slovenský,Expert";

        // Act
        var result = await _sut.ImportWordsFromCsvAsync(dict.Id, userId, csv);

        // Assert
        result.ImportedCount.Should().Be(1);
    }

    // --- JSON import with wrong structure ---

    [Fact]
    public async Task ImportJson_NotAnArray_ReportsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dict = CreateDictionaryForUser(userId);
        var json = @"{ ""word"": ""hello"" }"; // Object, not array

        // Act
        var result = await _sut.ImportWordsFromJsonAsync(dict.Id, userId, json);

        // Assert
        result.ImportedCount.Should().Be(0);
        result.Errors.Should().Contain(e => e.Contains("pole"));
    }

    [Fact]
    public async Task ImportJson_InvalidJson_ReportsParsingError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dict = CreateDictionaryForUser(userId);
        var json = "not valid json at all {{{";

        // Act
        var result = await _sut.ImportWordsFromJsonAsync(dict.Id, userId, json);

        // Assert
        result.ImportedCount.Should().Be(0);
        result.Errors.Should().Contain(e => e.Contains("JSON"));
    }

    [Fact]
    public async Task ImportJson_ArrayOfNumbers_ReportsErrors()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dict = CreateDictionaryForUser(userId);
        var json = "[1, 2, 3]";

        // Act
        var result = await _sut.ImportWordsFromJsonAsync(dict.Id, userId, json);

        // Assert
        result.ImportedCount.Should().Be(0);
        // Numbers are not strings or objects, so they result in null/empty word errors
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ImportJson_ObjectWithMissingWordProperty_ReportsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dict = CreateDictionaryForUser(userId);
        var json = @"[{ ""name"": ""hello"", ""difficulty"": ""Beginner"" }]";

        // Act
        var result = await _sut.ImportWordsFromJsonAsync(dict.Id, userId, json);

        // Assert
        result.ImportedCount.Should().Be(0);
        result.Errors.Should().NotBeEmpty(); // Missing "word" property
    }

    [Fact]
    public async Task ImportJson_MixedValidAndInvalid_ImportsValidOnly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dict = CreateDictionaryForUser(userId);
        var json = @"[""hello"", """", ""world""]"; // Middle one is empty

        // Act
        var result = await _sut.ImportWordsFromJsonAsync(dict.Id, userId, json);

        // Assert
        result.ImportedCount.Should().Be(2);
        result.Errors.Should().HaveCount(1); // Empty word error
    }

    // --- Duplicate handling ---

    [Fact]
    public async Task ImportCsv_DuplicateWord_SkipsIt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dict = CreateDictionaryForUser(userId);
        _wordRepo.ExistsInDictionaryAsync(dict.Id, "hello").Returns(true);
        var csv = "hello,Beginner";

        // Act
        var result = await _sut.ImportWordsFromCsvAsync(dict.Id, userId, csv);

        // Assert
        result.ImportedCount.Should().Be(0);
        result.SkippedCount.Should().Be(1);
    }

    // --- Unauthorized access ---

    [Fact]
    public async Task ImportCsv_NotOwner_ThrowsUnauthorized()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var dict = CreateDictionaryForUser(ownerId);

        // Act
        var act = () => _sut.ImportWordsFromCsvAsync(dict.Id, attackerId, "word,Beginner");

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ImportJson_NotOwner_ThrowsUnauthorized()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var dict = CreateDictionaryForUser(ownerId);

        // Act
        var act = () => _sut.ImportWordsFromJsonAsync(dict.Id, attackerId, @"[""hello""]");

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ImportCsv_DictionaryNotFound_ThrowsUnauthorized()
    {
        // Arrange
        var dictId = Guid.NewGuid();
        _dictionaryRepo.GetByIdAsync(dictId).Returns((CustomDictionary?)null);

        // Act
        var act = () => _sut.ImportWordsFromCsvAsync(dictId, Guid.NewGuid(), "word,Beginner");

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // --- WordCount updated correctly ---

    [Fact]
    public async Task ImportCsv_MultipleValidWords_UpdatesWordCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dict = CreateDictionaryForUser(userId);
        var csv = "slunce\nkočka\nstrom";

        // Act
        var result = await _sut.ImportWordsFromCsvAsync(dict.Id, userId, csv);

        // Assert
        result.ImportedCount.Should().Be(3);
        dict.WordCount.Should().Be(3);
        _dictionaryRepo.Received(1).Update(dict);
    }

    [Fact]
    public async Task ImportCsv_NoValidWords_DoesNotUpdateDictionary()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dict = CreateDictionaryForUser(userId);
        var csv = "ab\ncd"; // Too short (< 3 chars)

        // Act
        var result = await _sut.ImportWordsFromCsvAsync(dict.Id, userId, csv);

        // Assert
        result.ImportedCount.Should().Be(0);
        dict.WordCount.Should().Be(0);
        _dictionaryRepo.DidNotReceive().Update(dict);
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }
}
