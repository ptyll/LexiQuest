using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.Enums;
using Xunit;

namespace LexiQuest.Core.Tests.Domain;

public class DictionaryWordTests
{
    [Fact]
    public void Create_ValidData_CreatesWord()
    {
        // Arrange
        var dictionaryId = Guid.NewGuid();
        var word = "slunce";
        var difficulty = DifficultyLevel.Intermediate;

        // Act
        var dictWord = DictionaryWord.Create(dictionaryId, word, difficulty);

        // Assert
        dictWord.Should().NotBeNull();
        dictWord.DictionaryId.Should().Be(dictionaryId);
        dictWord.Word.Should().Be(word);
        dictWord.Difficulty.Should().Be(difficulty);
        dictWord.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_NullWord_ThrowsArgumentException()
    {
        // Arrange
        var dictionaryId = Guid.NewGuid();

        // Act
        Action act = () => DictionaryWord.Create(dictionaryId, null!, DifficultyLevel.Beginner);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("word");
    }

    [Fact]
    public void Create_EmptyWord_ThrowsArgumentException()
    {
        // Arrange
        var dictionaryId = Guid.NewGuid();

        // Act
        Action act = () => DictionaryWord.Create(dictionaryId, "", DifficultyLevel.Beginner);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("word");
    }

    [Fact]
    public void Create_WordWithSpaces_ThrowsArgumentException()
    {
        // Arrange
        var dictionaryId = Guid.NewGuid();

        // Act
        Action act = () => DictionaryWord.Create(dictionaryId, "hello world", DifficultyLevel.Beginner);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("word");
    }

    [Fact]
    public void Create_WordTooLong_ThrowsArgumentException()
    {
        // Arrange
        var dictionaryId = Guid.NewGuid();
        var longWord = new string('a', 51);

        // Act
        Action act = () => DictionaryWord.Create(dictionaryId, longWord, DifficultyLevel.Beginner);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("word");
    }

    [Fact]
    public void Create_WordTooShort_ThrowsArgumentException()
    {
        // Arrange
        var dictionaryId = Guid.NewGuid();

        // Act
        Action act = () => DictionaryWord.Create(dictionaryId, "ab", DifficultyLevel.Beginner);

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("word");
    }

    [Fact]
    public void UpdateWord_ValidWord_UpdatesWord()
    {
        // Arrange
        var dictWord = DictionaryWord.Create(Guid.NewGuid(), "staré", DifficultyLevel.Beginner);
        var newWord = "nové";

        // Act
        dictWord.UpdateWord(newWord);

        // Assert
        dictWord.Word.Should().Be(newWord);
    }

    [Fact]
    public void UpdateDifficulty_ValidDifficulty_UpdatesDifficulty()
    {
        // Arrange
        var dictWord = DictionaryWord.Create(Guid.NewGuid(), "slovo", DifficultyLevel.Beginner);

        // Act
        dictWord.UpdateDifficulty(DifficultyLevel.Advanced);

        // Assert
        dictWord.Difficulty.Should().Be(DifficultyLevel.Advanced);
    }

    [Theory]
    [InlineData("pes", DifficultyLevel.Beginner)]
    [InlineData("slunce", DifficultyLevel.Intermediate)]
    [InlineData("konzervatoř", DifficultyLevel.Expert)]
    public void AutoDetectDifficulty_VariousLengths_ReturnsCorrectDifficulty(string word, DifficultyLevel expected)
    {
        // Act
        var difficulty = DictionaryWord.AutoDetectDifficulty(word);

        // Assert
        difficulty.Should().Be(expected);
    }

    [Fact]
    public void Validate_WordWithNumbers_ReturnsFalse()
    {
        // Act
        var result = DictionaryWord.Validate("word123");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Validate_WordWithSpecialChars_ReturnsFalse()
    {
        // Act
        var result = DictionaryWord.Validate("word@#$");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Validate_CzechCharacters_ReturnsTrue()
    {
        // Act
        var result = DictionaryWord.Validate("přílišžluťoučký");

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Validate_ValidWord_ReturnsTrue()
    {
        // Act
        var result = DictionaryWord.Validate("slunce");

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void GetNormalizedWord_UpperCase_ReturnsLowerCase()
    {
        // Arrange
        var dictWord = DictionaryWord.Create(Guid.NewGuid(), "SLUNCE", DifficultyLevel.Beginner);

        // Act
        var normalized = dictWord.GetNormalizedWord();

        // Assert
        normalized.Should().Be("slunce");
    }

    [Fact]
    public void GetNormalizedWord_WithDiacritics_RemovesDiacritics()
    {
        // Arrange
        var dictWord = DictionaryWord.Create(Guid.NewGuid(), "příliš", DifficultyLevel.Beginner);

        // Act
        var normalized = dictWord.GetNormalizedWord();

        // Assert
        normalized.Should().Be("prilis");
    }
}

public record ValidationResult(bool IsValid, string? ErrorMessage);
