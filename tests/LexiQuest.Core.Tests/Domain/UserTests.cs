using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.ValueObjects;

namespace LexiQuest.Core.Tests.Domain;

public class UserTests
{
    [Fact]
    public void Create_SetsDefaultValues()
    {
        var user = User.Create("test@example.com", "testuser");

        user.Id.Should().NotBe(Guid.Empty);
        user.Email.Should().Be("test@example.com");
        user.Username.Should().Be("testuser");
        user.Stats.Should().NotBeNull();
        user.Stats.TotalXP.Should().Be(0);
        user.Stats.Level.Should().Be(1);
        user.Stats.Accuracy.Should().Be(0);
        user.Stats.TotalWordsSolved.Should().Be(0);
        user.Stats.AverageResponseTime.Should().Be(TimeSpan.Zero);
        user.Preferences.Should().NotBeNull();
        user.Preferences.Theme.Should().Be("light");
        user.Preferences.Language.Should().Be("cs");
        user.Preferences.AnimationsEnabled.Should().BeTrue();
        user.Preferences.SoundsEnabled.Should().BeTrue();
        user.Streak.Should().NotBeNull();
        user.Streak.CurrentDays.Should().Be(0);
        user.Streak.LongestDays.Should().Be(0);
        user.Streak.LastActivityDate.Should().BeNull();
        user.Premium.Should().NotBeNull();
        user.Premium.IsPremium.Should().BeFalse();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
