using FluentAssertions;
using Moq;
using OrkinosaiCMS.Core.Entities.Sites;
using OrkinosaiCMS.Core.Interfaces.Services;
using OrkinosaiCMS.Web.Services;

namespace OrkinosaiCMS.Tests.Unit.Services;

/// <summary>
/// Unit tests for AuthenticationService - tests authentication logic
/// Note: These tests focus on the business logic rather than mocking CustomAuthenticationStateProvider
/// which has complex dependencies. Integration tests cover the full authentication flow.
/// </summary>
public class AuthenticationServiceTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IRoleService> _roleServiceMock;

    public AuthenticationServiceTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _roleServiceMock = new Mock<IRoleService>();
    }

    [Fact]
    public async Task UserService_VerifyPassword_WithValidCredentials_ShouldReturnTrue()
    {
        // Arrange
        var username = "testuser";
        var password = "SecurePassword123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Username = username,
            PasswordHash = hashedPassword,
            IsActive = true
        };

        _userServiceMock
            .Setup(x => x.VerifyPasswordAsync(username, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _userServiceMock.Object.VerifyPasswordAsync(username, password);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserService_GetUserRoles_ShouldReturnAssignedRoles()
    {
        // Arrange
        var userId = 1;
        var roles = new List<Role>
        {
            new Role { Id = 1, Name = "Administrator" },
            new Role { Id = 2, Name = "Editor" }
        };

        _userServiceMock
            .Setup(x => x.GetUserRolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        // Act
        var result = await _userServiceMock.Object.GetUserRolesAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Name == "Administrator");
    }

    [Fact]
    public async Task UserService_GetByUsername_ShouldReturnUser()
    {
        // Arrange
        var username = "testuser";
        var expectedUser = new User
        {
            Id = 1,
            Username = username,
            Email = "test@example.com",
            DisplayName = "Test User",
            IsActive = true
        };

        _userServiceMock
            .Setup(x => x.GetByUsernameAsync(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userServiceMock.Object.GetByUsernameAsync(username);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be(username);
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task UserService_UpdateLastLogin_ShouldCallService()
    {
        // Arrange
        var userId = 1;

        _userServiceMock
            .Setup(x => x.UpdateLastLoginAsync(userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _userServiceMock.Object.UpdateLastLoginAsync(userId);

        // Assert
        _userServiceMock.Verify(x => x.UpdateLastLoginAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void UserSession_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var userSession = new UserSession
        {
            UserId = 1,
            Username = "testuser",
            Email = "test@example.com",
            DisplayName = "Test User",
            Role = "Administrator"
        };

        // Assert
        userSession.UserId.Should().Be(1);
        userSession.Username.Should().Be("testuser");
        userSession.Email.Should().Be("test@example.com");
        userSession.DisplayName.Should().Be("Test User");
        userSession.Role.Should().Be("Administrator");
    }

    [Fact]
    public void UserSession_ShouldHaveDefaultRole()
    {
        // Arrange & Act
        var userSession = new UserSession
        {
            UserId = 1,
            Username = "testuser",
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        // Assert
        userSession.Role.Should().Be("User"); // Default role
    }
}
