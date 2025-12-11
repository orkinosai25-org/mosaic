using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using OrkinosaiCMS.Core.Entities.Sites;
using OrkinosaiCMS.Core.Interfaces.Repositories;
using OrkinosaiCMS.Infrastructure.Data;
using OrkinosaiCMS.Infrastructure.Services;

namespace OrkinosaiCMS.Tests.Unit.Services;

/// <summary>
/// Unit tests for UserService - tests business logic for user management
/// </summary>
public class UserServiceTests
{
    private readonly Mock<IRepository<User>> _userRepositoryMock;
    private readonly Mock<IRepository<Role>> _roleRepositoryMock;
    private readonly Mock<IRepository<UserRole>> _userRoleRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ApplicationDbContext _context;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        _userRepositoryMock = new Mock<IRepository<User>>();
        _roleRepositoryMock = new Mock<IRepository<Role>>();
        _userRoleRepositoryMock = new Mock<IRepository<UserRole>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _userService = new UserService(
            _userRepositoryMock.Object,
            _roleRepositoryMock.Object,
            _userRoleRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _context
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldHashPasswordAndSetDefaults()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            DisplayName = "Test User"
        };
        var password = "SecurePassword123!";

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.CreateAsync(user, password);

        // Assert
        result.Should().NotBeNull();
        result.PasswordHash.Should().NotBeNullOrEmpty();
        result.PasswordHash.Should().NotBe(password); // Password should be hashed
        result.IsActive.Should().BeTrue();
        result.CreatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _userRepositoryMock.Verify(x => x.AddAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyPasswordAsync_WithValidCredentials_ShouldReturnTrue()
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

        _userRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.VerifyPasswordAsync(username, password);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyPasswordAsync_WithInvalidPassword_ShouldReturnFalse()
    {
        // Arrange
        var username = "testuser";
        var correctPassword = "SecurePassword123!";
        var wrongPassword = "WrongPassword";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword);

        var user = new User
        {
            Username = username,
            PasswordHash = hashedPassword,
            IsActive = true
        };

        _userRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.VerifyPasswordAsync(username, wrongPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyPasswordAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var username = "nonexistent";
        var password = "anypassword";

        _userRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.VerifyPasswordAsync(username, password);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnUser()
    {
        // Arrange
        var username = "testuser";
        var expectedUser = new User
        {
            Id = 1,
            Username = username,
            Email = "test@example.com"
        };

        _userRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.GetByUsernameAsync(username);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be(username);
        result.Id.Should().Be(1);
    }

    [Fact]
    public async Task UpdateAsync_ShouldSetModifiedOn()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            IsActive = true
        };

        // Act
        var result = await _userService.UpdateAsync(user);

        // Assert
        result.ModifiedOn.Should().NotBeNull();
        result.ModifiedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithCorrectCurrentPassword_ShouldSucceed()
    {
        // Arrange
        var userId = 1;
        var currentPassword = "OldPassword123!";
        var newPassword = "NewPassword123!";
        var hashedCurrentPassword = BCrypt.Net.BCrypt.HashPassword(currentPassword);

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = hashedCurrentPassword
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _userService.ChangePasswordAsync(userId, currentPassword, newPassword);

        // Assert
        user.PasswordHash.Should().NotBe(hashedCurrentPassword);
        user.ModifiedOn.Should().NotBeNull();
        _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithIncorrectCurrentPassword_ShouldThrow()
    {
        // Arrange
        var userId = 1;
        var currentPassword = "OldPassword123!";
        var wrongPassword = "WrongPassword";
        var newPassword = "NewPassword123!";
        var hashedCurrentPassword = BCrypt.Net.BCrypt.HashPassword(currentPassword);

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = hashedCurrentPassword
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var act = async () => await _userService.ChangePasswordAsync(userId, wrongPassword, newPassword);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Current password is incorrect.");
    }

    [Fact]
    public async Task UpdateLastLoginAsync_ShouldUpdateLastLoginTime()
    {
        // Arrange
        var userId = 1;
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            LastLoginOn = null
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _userService.UpdateLastLoginAsync(userId);

        // Assert
        user.LastLoginOn.Should().NotBeNull();
        user.LastLoginOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _userRepositoryMock.Verify(x => x.Update(user), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetActiveUsersAsync_ShouldReturnOnlyActiveUsers()
    {
        // Arrange
        var activeUsers = new List<User>
        {
            new User { Id = 1, Username = "active1", IsActive = true },
            new User { Id = 2, Username = "active2", IsActive = true }
        };

        _userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeUsers);

        // Act
        var result = await _userService.GetActiveUsersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(u => u.IsActive);
    }
}
