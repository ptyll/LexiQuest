using Bunit;
using FluentAssertions;
using LexiQuest.Blazor.Pages;
using LexiQuest.Blazor.Services;
using LexiQuest.Shared.DTOs.Shop;
using LexiQuest.Shared.DTOs.Users;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Tempo.Blazor.Localization;

namespace LexiQuest.Blazor.Tests.Pages;

public class ProfilePageTests : BunitContext
{
    private readonly IUserService _userService;
    private readonly IShopService _shopService;
    private readonly IStringLocalizer<Profile> _localizer;
    private readonly ITmLocalizer _tmLocalizer;

    public ProfilePageTests()
    {
        _userService = Substitute.For<IUserService>();
        _shopService = Substitute.For<IShopService>();
        _localizer = Substitute.For<IStringLocalizer<Profile>>();
        _tmLocalizer = Substitute.For<ITmLocalizer>();

        _localizer[Arg.Any<string>()].Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), callInfo.Arg<string>()));
        _localizer[Arg.Any<string>(), Arg.Any<object[]>()].Returns(callInfo => new LocalizedString(callInfo.Arg<string>(), callInfo.Arg<string>()));
        _tmLocalizer[Arg.Any<string>()].Returns(callInfo => callInfo.Arg<string>());

        _userService.GetProfileAsync().Returns(Task.FromResult<UserProfileDto?>(CreateProfile()));
        _shopService.GetUserInventoryAsync().Returns(Task.FromResult<IEnumerable<UserInventoryItemDto>>(Array.Empty<UserInventoryItemDto>()));

        Services.AddSingleton(_userService);
        Services.AddSingleton(_shopService);
        Services.AddSingleton(_localizer);
        Services.AddSingleton(_tmLocalizer);
    }

    [Fact]
    public void ProfilePage_LoadsEquippedShopItems()
    {
        // Arrange
        _shopService.GetUserInventoryAsync().Returns(Task.FromResult<IEnumerable<UserInventoryItemDto>>(CreateEquippedInventory()));

        // Act
        var cut = Render<Profile>();
        cut.WaitForState(() => cut.Find("[data-testid='profile-summary-card']") != null);

        // Assert
        cut.Find("[data-testid='profile-equipped-frame']").TextContent.Should().Contain("Stříbrný rámeček");
        cut.Find("[data-testid='profile-equipped-theme']").TextContent.Should().Contain("Noční téma");
        cut.Find("[data-testid='profile-equipped-boost']").TextContent.Should().Contain("XP boost malý");
    }

    [Fact]
    public void ProfilePage_WrapsAvatarWhenFrameIsEquipped()
    {
        // Arrange
        _shopService.GetUserInventoryAsync().Returns(Task.FromResult<IEnumerable<UserInventoryItemDto>>(CreateEquippedInventory()));

        // Act
        var cut = Render<Profile>();
        cut.WaitForState(() => cut.Find("[data-testid='profile-summary-card']") != null);

        // Assert
        cut.Find("[data-testid='profile-avatar-frame']").ClassList.Should().Contain("profile-avatar-frame--silver");
    }

    [Fact]
    public void ProfilePage_AchievementsCard_LinksToAchievementsPage()
    {
        // Act
        var cut = Render<Profile>();
        cut.WaitForState(() => cut.Find("[data-testid='profile-achievements-card']") != null);

        // Assert
        var link = cut.Find("[data-testid='profile-achievements-link']");
        link.GetAttribute("href").Should().Be("/achievements");
        cut.Find("[data-testid='profile-achievements-card']").TextContent.Should().NotContain("Brzy");
    }

    private static UserProfileDto CreateProfile()
    {
        return new UserProfileDto
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.test",
            AvatarUrl = "/assets/shop/avatar-diamond.svg",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            Stats = new UserStatsDto
            {
                Level = 3,
                TotalXP = 450,
                WordsSolved = 30,
                CurrentStreak = 2,
                LongestStreak = 5,
                Accuracy = 80
            },
            Preferences = new UserPreferencesDto
            {
                Theme = AppTheme.Dark,
                Language = "cs",
                AnimationsEnabled = true,
                SoundsEnabled = true
            }
        };
    }

    private static List<UserInventoryItemDto> CreateEquippedInventory()
    {
        return
        [
            new UserInventoryItemDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Stříbrný rámeček",
                "Frame",
                "/icon-192.png",
                true,
                DateTime.UtcNow.AddDays(-2)),
            new UserInventoryItemDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Noční téma",
                "Theme",
                "/icon-192.png",
                true,
                DateTime.UtcNow.AddDays(-1)),
            new UserInventoryItemDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "XP boost malý",
                "Boost",
                "/icon-192.png",
                true,
                DateTime.UtcNow)
        ];
    }
}
