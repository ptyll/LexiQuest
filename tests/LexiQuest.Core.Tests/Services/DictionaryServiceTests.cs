using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using LexiQuest.Shared.DTOs.Dictionaries;
using LexiQuest.Shared.Enums;
using NSubstitute;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

public class DictionaryServiceTests
{
    private readonly ICustomDictionaryRepository _dictionaryRepo;
    private readonly IDictionaryWordRepository _wordRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDictionaryService _service;

    public DictionaryServiceTests()
    {
        _dictionaryRepo = Substitute.For<ICustomDictionaryRepository>();
        _wordRepo = Substitute.For<IDictionaryWordRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _service = new DictionaryService(_dictionaryRepo, _wordRepo, _unitOfWork);
    }

    [Fact]
    public async Task CreateDictionaryAsync_ValidData_CreatesDictionary()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateDictionaryRequest("Můj slovník", "Popis");

        // Act
        var result = await _service.CreateDictionaryAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Description.Should().Be(request.Description);
        await _dictionaryRepo.Received(1).AddAsync(Arg.Any<CustomDictionary>());
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task GetUserDictionariesAsync_ReturnsUserDictionaries()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dictionaries = new List<CustomDictionary>
        {
            CustomDictionary.Create(userId, "Slovník 1", "Popis 1"),
            CustomDictionary.Create(userId, "Slovník 2", "Popis 2")
        };
        _dictionaryRepo.GetByUserIdAsync(userId).Returns(dictionaries);

        // Act
        var result = await _service.GetUserDictionariesAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Select(d => d.Name).Should().Contain("Slovník 1", "Slovník 2");
    }

    [Fact]
    public async Task GetDictionaryByIdAsync_ExistingDictionary_ReturnsDictionary()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dictionary = CustomDictionary.Create(userId, "Test", "Popis");
        _dictionaryRepo.GetByIdAsync(dictionary.Id).Returns(dictionary);

        // Act
        var result = await _service.GetDictionaryByIdAsync(dictionary.Id, userId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetDictionaryByIdAsync_PrivateDictionary_OtherUser_ReturnsNull()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var dictionary = CustomDictionary.Create(ownerId, "Test", "Popis");
        _dictionaryRepo.GetByIdAsync(dictionary.Id).Returns(dictionary);

        // Act
        var result = await _service.GetDictionaryByIdAsync(dictionary.Id, otherUserId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddWordAsync_ValidWord_AddsWord()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dictionary = CustomDictionary.Create(userId, "Test", "Popis");
        var request = new AddWordRequest("slunce", DifficultyLevel.Intermediate);
        
        _dictionaryRepo.GetByIdAsync(dictionary.Id).Returns(dictionary);

        // Act
        var result = await _service.AddWordAsync(dictionary.Id, userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Word.Should().Be("slunce");
        await _wordRepo.Received(1).AddAsync(Arg.Any<DictionaryWord>());
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task AddWordAsync_NotOwner_ThrowsUnauthorized()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var dictionary = CustomDictionary.Create(ownerId, "Test", "Popis");
        var request = new AddWordRequest("slunce", DifficultyLevel.Intermediate);
        
        _dictionaryRepo.GetByIdAsync(dictionary.Id).Returns(dictionary);

        // Act
        Func<Task> act = () => _service.AddWordAsync(dictionary.Id, otherUserId, request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ImportWordsFromCsvAsync_ValidCsv_ImportsWords()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dictionary = CustomDictionary.Create(userId, "Test", "Popis");
        var csvContent = "pes,Beginner\nslunce,Intermediate\nkonzervatoř,Advanced";
        
        _dictionaryRepo.GetByIdAsync(dictionary.Id).Returns(dictionary);

        // Act
        var result = await _service.ImportWordsFromCsvAsync(dictionary.Id, userId, csvContent);

        // Assert
        result.ImportedCount.Should().Be(3);
        result.Errors.Should().BeEmpty();
        await _wordRepo.Received(3).AddAsync(Arg.Any<DictionaryWord>());
    }

    [Fact]
    public async Task ImportWordsFromTxtAsync_ValidTxt_ImportsWordsWithAutoDifficulty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dictionary = CustomDictionary.Create(userId, "Test", "Popis");
        var txtContent = "pes\nslunce\nkonzervatoř";
        
        _dictionaryRepo.GetByIdAsync(dictionary.Id).Returns(dictionary);

        // Act
        var result = await _service.ImportWordsFromTxtAsync(dictionary.Id, userId, txtContent);

        // Assert
        result.ImportedCount.Should().Be(3);
        result.Errors.Should().BeEmpty();
        await _wordRepo.Received(3).AddAsync(Arg.Any<DictionaryWord>());
    }

    [Fact]
    public async Task DeleteDictionaryAsync_Owner_DeletesDictionary()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dictionary = CustomDictionary.Create(userId, "Test", "Popis");
        _dictionaryRepo.GetByIdAsync(dictionary.Id).Returns(dictionary);

        // Act
        var result = await _service.DeleteDictionaryAsync(dictionary.Id, userId);

        // Assert
        result.Should().BeTrue();
        _dictionaryRepo.Received(1).Delete(dictionary);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task GetPublicDictionariesAsync_ReturnsOnlyPublicDictionaries()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var publicDict = CustomDictionary.Create(userId, "Veřejný", "Popis");
        publicDict.SetPublicStatus(true);
        
        var dictionaries = new List<CustomDictionary> { publicDict };
        _dictionaryRepo.GetPublicDictionariesAsync().Returns(dictionaries);

        // Act
        var result = await _service.GetPublicDictionariesAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().IsPublic.Should().BeTrue();
    }

    [Fact]
    public async Task ImportWordsFromJsonAsync_ValidJsonArray_ImportsWords()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dictionary = CustomDictionary.Create(userId, "Test", "Popis");
        var jsonContent = "[\"pes\", \"slunce\", \"konzervatoř\"]";
        
        _dictionaryRepo.GetByIdAsync(dictionary.Id).Returns(dictionary);

        // Act
        var result = await _service.ImportWordsFromJsonAsync(dictionary.Id, userId, jsonContent);

        // Assert
        result.ImportedCount.Should().Be(3);
        result.Errors.Should().BeEmpty();
        await _wordRepo.Received(3).AddAsync(Arg.Any<DictionaryWord>());
    }

    [Fact]
    public async Task ImportWordsFromJsonAsync_ValidJsonObject_ImportsWordsWithDifficulty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dictionary = CustomDictionary.Create(userId, "Test", "Popis");
        var jsonContent = @"[
            { ""word"": ""pes"", ""difficulty"": ""Beginner"" },
            { ""word"": ""slunce"", ""difficulty"": ""Intermediate"" },
            { ""word"": ""konzervatoř"", ""difficulty"": ""Advanced"" }
        ]";
        
        _dictionaryRepo.GetByIdAsync(dictionary.Id).Returns(dictionary);

        // Act
        var result = await _service.ImportWordsFromJsonAsync(dictionary.Id, userId, jsonContent);

        // Assert
        result.ImportedCount.Should().Be(3);
        result.Errors.Should().BeEmpty();
        await _wordRepo.Received(3).AddAsync(Arg.Any<DictionaryWord>());
    }

    [Fact]
    public async Task ImportWordsFromJsonAsync_InvalidJson_ReturnsErrors()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dictionary = CustomDictionary.Create(userId, "Test", "Popis");
        var jsonContent = "not valid json";
        
        _dictionaryRepo.GetByIdAsync(dictionary.Id).Returns(dictionary);

        // Act
        var result = await _service.ImportWordsFromJsonAsync(dictionary.Id, userId, jsonContent);

        // Assert
        result.ImportedCount.Should().Be(0);
        result.Errors.Should().ContainSingle();
    }
}
