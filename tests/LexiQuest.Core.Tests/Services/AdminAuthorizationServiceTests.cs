using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Services;
using LexiQuest.Shared.Enums;
using NSubstitute;

namespace LexiQuest.Core.Tests.Services;

public class AdminAuthorizationServiceTests
{
    private readonly IAdminRoleAssignmentRepository _roleRepository;
    private readonly AdminAuthorizationService _sut;

    public AdminAuthorizationServiceTests()
    {
        _roleRepository = Substitute.For<IAdminRoleAssignmentRepository>();
        _sut = new AdminAuthorizationService(_roleRepository);
    }

    [Fact]
    public async Task AdminAuthorizationService_IsAdmin_ReturnsTrueForAdminRole()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var assignment = AdminRoleAssignment.Create(userId, AdminRole.Admin);

        _roleRepository.GetByUserIdAndRoleAsync(userId, AdminRole.Admin, Arg.Any<CancellationToken>())
            .Returns(assignment);

        // Act
        var result = await _sut.IsAdminAsync(userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AdminAuthorizationService_IsModerator_ReturnsTrueForModRole()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var assignments = new List<AdminRoleAssignment>
        {
            AdminRoleAssignment.Create(userId, AdminRole.Moderator)
        };

        _roleRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(assignments);

        // Act
        var result = await _sut.IsModeratorAsync(userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AdminAuthorizationService_RegularUser_ReturnsFalseForAdmin()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _roleRepository.GetByUserIdAndRoleAsync(userId, AdminRole.Admin, Arg.Any<CancellationToken>())
            .Returns((AdminRoleAssignment?)null);

        // Act
        var result = await _sut.IsAdminAsync(userId);

        // Assert
        result.Should().BeFalse();
    }
}
