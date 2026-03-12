using FluentAssertions;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using NSubstitute;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

public class PremiumFeatureServiceTests
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly PremiumFeatureService _service;

    public PremiumFeatureServiceTests()
    {
        _subscriptionService = Substitute.For<ISubscriptionService>();
        _service = new PremiumFeatureService(_subscriptionService);
    }

    [Fact]
    public async Task IsPremiumAsync_ActiveSubscription_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _subscriptionService.IsPremiumAsync(userId).Returns(true);

        // Act
        var result = await _service.IsPremiumAsync(userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsPremiumAsync_NoSubscription_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _subscriptionService.IsPremiumAsync(userId).Returns(false);

        // Act
        var result = await _service.IsPremiumAsync(userId);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(PremiumFeature.NoAds)]
    [InlineData(PremiumFeature.StreakFreeze)]
    [InlineData(PremiumFeature.StreakShield)]
    [InlineData(PremiumFeature.DoubleXPWeekends)]
    [InlineData(PremiumFeature.ExclusivePaths)]
    [InlineData(PremiumFeature.CustomDictionaries)]
    [InlineData(PremiumFeature.DetailedStats)]
    [InlineData(PremiumFeature.CustomAvatar)]
    [InlineData(PremiumFeature.DiamondLeague)]
    [InlineData(PremiumFeature.TeamCreation)]
    public async Task HasFeatureAsync_PremiumUser_ReturnsTrueForAllFeatures(PremiumFeature feature)
    {
        // Arrange
        var userId = Guid.NewGuid();
        _subscriptionService.IsPremiumAsync(userId).Returns(true);

        // Act
        var result = await _service.HasFeatureAsync(userId, feature);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(PremiumFeature.NoAds)]
    [InlineData(PremiumFeature.StreakFreeze)]
    [InlineData(PremiumFeature.StreakShield)]
    [InlineData(PremiumFeature.DoubleXPWeekends)]
    [InlineData(PremiumFeature.ExclusivePaths)]
    [InlineData(PremiumFeature.CustomDictionaries)]
    [InlineData(PremiumFeature.DetailedStats)]
    [InlineData(PremiumFeature.CustomAvatar)]
    [InlineData(PremiumFeature.DiamondLeague)]
    [InlineData(PremiumFeature.TeamCreation)]
    public async Task HasFeatureAsync_FreeUser_ReturnsFalseForAllFeatures(PremiumFeature feature)
    {
        // Arrange
        var userId = Guid.NewGuid();
        _subscriptionService.IsPremiumAsync(userId).Returns(false);

        // Act
        var result = await _service.HasFeatureAsync(userId, feature);

        // Assert
        result.Should().BeFalse();
    }
}
