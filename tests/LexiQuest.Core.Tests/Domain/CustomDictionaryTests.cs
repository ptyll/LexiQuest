using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using Xunit;

namespace LexiQuest.Core.Tests.Domain;

public class CustomDictionaryTests
{
    [Fact]
    public void Create_ValidData_CreatesDictionary()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var name = "Můj slovník";
        var description = "Slovník pro učení";

        // Act
        var dictionary = CustomDictionary.Create(userId, name, description);

        // Assert
        dictionary.Should().NotBeNull();
        dictionary.UserId.Should().Be(userId);
        dictionary.Name.Should().Be(name);
        dictionary.Description.Should().Be(description);
        dictionary.IsPublic.Should().BeFalse();
        dictionary.WordCount.Should().Be(0);
        dictionary.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_NullName_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        Action act = () => CustomDictionary.Create(userId, null!, "Description");

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    [Fact]
    public void Create_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        Action act = () => CustomDictionary.Create(userId, "", "Description");

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    [Fact]
    public void Create_NameTooLong_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var longName = new string('a', 101);

        // Act
        Action act = () => CustomDictionary.Create(userId, longName, "Description");

        // Assert
        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    [Fact]
    public void UpdateName_ValidName_UpdatesName()
    {
        // Arrange
        var dictionary = CustomDictionary.Create(Guid.NewGuid(), "Starý název", "Desc");
        var newName = "Nový název";

        // Act
        dictionary.UpdateName(newName);

        // Assert
        dictionary.Name.Should().Be(newName);
        dictionary.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateDescription_ValidDescription_UpdatesDescription()
    {
        // Arrange
        var dictionary = CustomDictionary.Create(Guid.NewGuid(), "Název", "Starý popis");
        var newDescription = "Nový popis";

        // Act
        dictionary.UpdateDescription(newDescription);

        // Assert
        dictionary.Description.Should().Be(newDescription);
        dictionary.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SetPublicStatus_True_SetsIsPublic()
    {
        // Arrange
        var dictionary = CustomDictionary.Create(Guid.NewGuid(), "Název", "Desc");

        // Act
        dictionary.SetPublicStatus(true);

        // Assert
        dictionary.IsPublic.Should().BeTrue();
    }

    [Fact]
    public void AddWord_IncreasesWordCount()
    {
        // Arrange
        var dictionary = CustomDictionary.Create(Guid.NewGuid(), "Název", "Desc");

        // Act
        dictionary.AddWord();

        // Assert
        dictionary.WordCount.Should().Be(1);
    }

    [Fact]
    public void RemoveWord_DecreasesWordCount()
    {
        // Arrange
        var dictionary = CustomDictionary.Create(Guid.NewGuid(), "Název", "Desc");
        dictionary.AddWord();
        dictionary.AddWord();

        // Act
        dictionary.RemoveWord();

        // Assert
        dictionary.WordCount.Should().Be(1);
    }

    [Fact]
    public void RemoveWord_WhenZero_DoesNotGoBelowZero()
    {
        // Arrange
        var dictionary = CustomDictionary.Create(Guid.NewGuid(), "Název", "Desc");

        // Act
        dictionary.RemoveWord();

        // Assert
        dictionary.WordCount.Should().Be(0);
    }

    [Fact]
    public void CanBeAccessedBy_UserIsOwner_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dictionary = CustomDictionary.Create(userId, "Název", "Desc");

        // Act
        var result = dictionary.CanBeAccessedBy(userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanBeAccessedBy_UserIsNotOwnerAndPrivate_ReturnsFalse()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var dictionary = CustomDictionary.Create(ownerId, "Název", "Desc");

        // Act
        var result = dictionary.CanBeAccessedBy(otherUserId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanBeAccessedBy_UserIsNotOwnerAndPublic_ReturnsTrue()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var dictionary = CustomDictionary.Create(ownerId, "Název", "Desc");
        dictionary.SetPublicStatus(true);

        // Act
        var result = dictionary.CanBeAccessedBy(otherUserId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanBeModifiedBy_UserIsOwner_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dictionary = CustomDictionary.Create(userId, "Název", "Desc");

        // Act
        var result = dictionary.CanBeModifiedBy(userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanBeModifiedBy_UserIsNotOwner_ReturnsFalse()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var dictionary = CustomDictionary.Create(ownerId, "Název", "Desc");

        // Act
        var result = dictionary.CanBeModifiedBy(otherUserId);

        // Assert
        result.Should().BeFalse();
    }
}
