using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Models;
using LexiQuest.Core.Services;
using LexiQuest.Shared.DTOs.Admin;
using LexiQuest.Shared.DTOs.Auth;
using NSubstitute;

namespace LexiQuest.Core.Tests.Services;

public class AdminUserServiceTests
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordResetService _passwordResetService;
    private readonly AdminUserService _sut;

    public AdminUserServiceTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _passwordResetService = Substitute.For<IPasswordResetService>();
        _sut = new AdminUserService(_userRepository, _unitOfWork, _passwordResetService);
    }

    [Fact]
    public async Task AdminUserService_GetUsers_ReturnsPaginatedList()
    {
        // Arrange
        var user1 = User.Create("user1@test.com", "user1");
        var user2 = User.Create("user2@test.com", "user2");

        _userRepository.GetActiveUsersAsync(Arg.Any<CancellationToken>())
            .Returns(new List<User> { user1, user2 });
        _userRepository.GetInactiveUsersAsync(0, Arg.Any<CancellationToken>())
            .Returns(new List<User>());

        var request = new AdminUserListRequest(null, null, null, null, null, 1, 25);

        // Act
        var result = await _sut.GetUsersAsync(request);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task AdminUserService_SuspendUser_SetsLockedOut()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@test.com", "testuser");
        user.SetId(userId);

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        var result = await _sut.SuspendUserAsync(userId);

        // Assert
        result.Should().BeTrue();
        user.IsLockedOut().Should().BeTrue();
        _userRepository.Received(1).Update(user);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AdminUserService_UnsuspendUser_ClearsLockout()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@test.com", "testuser");
        user.SetId(userId);
        user.LockAccountUntil(DateTime.UtcNow.AddYears(100));

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        var result = await _sut.UnsuspendUserAsync(userId);

        // Assert
        result.Should().BeTrue();
        user.IsLockedOut().Should().BeFalse();
        _userRepository.Received(1).Update(user);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AdminUserService_ResetPassword_SendsResetEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@test.com", "testuser");
        user.SetId(userId);

        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordResetService.RequestResetAsync(Arg.Any<RequestPasswordResetDto>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _sut.ResetPasswordAsync(userId);

        // Assert
        result.Should().BeTrue();
        await _passwordResetService.Received(1).RequestResetAsync(
            Arg.Is<RequestPasswordResetDto>(r => r.Email == "test@test.com"),
            Arg.Any<CancellationToken>());
    }
}
