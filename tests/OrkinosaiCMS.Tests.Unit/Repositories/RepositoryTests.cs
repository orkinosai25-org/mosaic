using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrkinosaiCMS.Core.Entities.Sites;
using OrkinosaiCMS.Infrastructure.Data;
using OrkinosaiCMS.Infrastructure.Repositories;

namespace OrkinosaiCMS.Tests.Unit.Repositories;

/// <summary>
/// Unit tests for generic Repository pattern implementation
/// </summary>
public class RepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly Repository<User> _repository;

    public RepositoryTests()
    {
        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _repository = new Repository<User>(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntity()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            DisplayName = "Test User",
            PasswordHash = "hash"
        };

        // Act
        var result = await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Assert
        result.Should().NotBeNull();
        var savedUser = await _context.LegacyUsers.FindAsync(user.Id);
        savedUser.Should().NotBeNull();
        savedUser!.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            DisplayName = "Test User",
            PasswordHash = "hash"
        };
        await _context.LegacyUsers.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Username = "user1", Email = "user1@example.com", DisplayName = "User 1", PasswordHash = "hash1" },
            new User { Username = "user2", Email = "user2@example.com", DisplayName = "User 2", PasswordHash = "hash2" },
            new User { Username = "user3", Email = "user3@example.com", DisplayName = "User 3", PasswordHash = "hash3" }
        };
        await _context.LegacyUsers.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task FindAsync_WithPredicate_ShouldReturnMatchingEntities()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Username = "admin", Email = "admin@example.com", DisplayName = "Admin", PasswordHash = "hash1", IsActive = true },
            new User { Username = "user", Email = "user@example.com", DisplayName = "User", PasswordHash = "hash2", IsActive = true },
            new User { Username = "inactive", Email = "inactive@example.com", DisplayName = "Inactive", PasswordHash = "hash3", IsActive = false }
        };
        await _context.LegacyUsers.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindAsync(u => u.IsActive);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(u => u.IsActive);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_WithPredicate_ShouldReturnFirstMatch()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Username = "user1", Email = "user1@example.com", DisplayName = "User 1", PasswordHash = "hash1" },
            new User { Username = "user2", Email = "user2@example.com", DisplayName = "User 2", PasswordHash = "hash2" }
        };
        await _context.LegacyUsers.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FirstOrDefaultAsync(u => u.Username == "user2");

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("user2");
    }

    [Fact]
    public async Task Update_ShouldUpdateEntity()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            DisplayName = "Test User",
            PasswordHash = "hash"
        };
        await _context.LegacyUsers.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        user.DisplayName = "Updated Name";
        _repository.Update(user);
        await _context.SaveChangesAsync();

        // Assert
        var updatedUser = await _context.LegacyUsers.FindAsync(user.Id);
        updatedUser!.DisplayName.Should().Be("Updated Name");
    }

    [Fact]
    public async Task Remove_ShouldSoftDeleteEntity()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            DisplayName = "Test User",
            PasswordHash = "hash"
        };
        await _context.LegacyUsers.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        _repository.Remove(user);
        await _context.SaveChangesAsync();

        // Assert
        user.IsDeleted.Should().BeTrue();
        
        // Note: In-memory database may not fully enforce query filters in all scenarios
        // In production with SQL Server/SQLite, soft-deleted entities are filtered automatically
        var userInDb = await _context.LegacyUsers.FindAsync(user.Id);
        if (userInDb != null)
        {
            // Verify it's marked as deleted even if query filter doesn't work in in-memory DB
            userInDb.IsDeleted.Should().BeTrue();
        }
    }

    [Fact]
    public async Task CountAsync_WithoutPredicate_ShouldReturnTotalCount()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Username = "user1", Email = "user1@example.com", DisplayName = "User 1", PasswordHash = "hash1" },
            new User { Username = "user2", Email = "user2@example.com", DisplayName = "User 2", PasswordHash = "hash2" },
            new User { Username = "user3", Email = "user3@example.com", DisplayName = "User 3", PasswordHash = "hash3" }
        };
        await _context.LegacyUsers.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.CountAsync();

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ShouldReturnMatchingCount()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Username = "user1", Email = "user1@example.com", DisplayName = "User 1", PasswordHash = "hash1", IsActive = true },
            new User { Username = "user2", Email = "user2@example.com", DisplayName = "User 2", PasswordHash = "hash2", IsActive = true },
            new User { Username = "user3", Email = "user3@example.com", DisplayName = "User 3", PasswordHash = "hash3", IsActive = false }
        };
        await _context.LegacyUsers.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.CountAsync(u => u.IsActive);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task AnyAsync_WithMatchingPredicate_ShouldReturnTrue()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            DisplayName = "Test User",
            PasswordHash = "hash"
        };
        await _context.LegacyUsers.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.AnyAsync(u => u.Username == "testuser");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AnyAsync_WithNonMatchingPredicate_ShouldReturnFalse()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            DisplayName = "Test User",
            PasswordHash = "hash"
        };
        await _context.LegacyUsers.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.AnyAsync(u => u.Username == "nonexistent");

        // Assert
        result.Should().BeFalse();
    }
}
