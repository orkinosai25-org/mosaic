using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrkinosaiCMS.Core.Entities.Sites;
using OrkinosaiCMS.Infrastructure.Data;
using OrkinosaiCMS.Tests.Integration.Fixtures;

namespace OrkinosaiCMS.Tests.Integration.Database;

/// <summary>
/// Integration tests for database connectivity and operations
/// </summary>
public class DatabaseConnectivityTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public DatabaseConnectivityTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Database_ShouldConnect()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Act
        var canConnect = await context.Database.CanConnectAsync();

        // Assert
        canConnect.Should().BeTrue();
    }

    [Fact]
    public async Task Database_ShouldSupportBasicCRUD_ForUsers()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = new User
        {
            Username = "newuser",
            Email = "newuser@test.com",
            DisplayName = "New User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };

        // Act - Create
        await context.LegacyUsers.AddAsync(user);
        await context.SaveChangesAsync();

        // Assert - Read
        var retrievedUser = await context.LegacyUsers.FirstOrDefaultAsync(u => u.Username == "newuser");
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Email.Should().Be("newuser@test.com");

        // Act - Update
        retrievedUser.DisplayName = "Updated User";
        context.LegacyUsers.Update(retrievedUser);
        await context.SaveChangesAsync();

        // Assert - Update
        var updatedUser = await context.LegacyUsers.FirstOrDefaultAsync(u => u.Username == "newuser");
        updatedUser!.DisplayName.Should().Be("Updated User");

        // Act - Delete (Soft Delete)
        retrievedUser.IsDeleted = true;
        context.LegacyUsers.Update(retrievedUser);
        await context.SaveChangesAsync();

        // Assert - Delete
        var deletedUser = await context.LegacyUsers.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Username == "newuser");
        deletedUser!.IsDeleted.Should().BeTrue();

        // Verify query filter excludes soft-deleted entities
        var queryResult = await context.LegacyUsers.FirstOrDefaultAsync(u => u.Username == "newuser");
        queryResult.Should().BeNull();
    }

    [Fact]
    public async Task Database_ShouldSupportRelationships()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var role = new Role
        {
            Name = "TestRole",
            Description = "Test Role Description",
            CreatedOn = DateTime.UtcNow
        };

        var user = new User
        {
            Username = "roleuser",
            Email = "roleuser@test.com",
            DisplayName = "Role User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };

        // Act
        await context.LegacyRoles.AddAsync(role);
        await context.LegacyUsers.AddAsync(user);
        await context.SaveChangesAsync();

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            CreatedOn = DateTime.UtcNow
        };

        await context.LegacyUserRoles.AddAsync(userRole);
        await context.SaveChangesAsync();

        // Assert
        var userWithRoles = await context.LegacyUsers
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == "roleuser");

        userWithRoles.Should().NotBeNull();
        userWithRoles!.UserRoles.Should().HaveCount(1);
        userWithRoles.UserRoles.First().Role.Name.Should().Be("TestRole");
    }

    [Fact]
    public async Task Database_ShouldSupportSiteEntities()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var site = new Site
        {
            Name = "Test Site",
            Description = "Test Site Description",
            Url = "/testsite",
            AdminEmail = "admin@testsite.com",
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };

        // Act
        await context.Sites.AddAsync(site);
        await context.SaveChangesAsync();

        // Assert
        var retrievedSite = await context.Sites.FirstOrDefaultAsync(s => s.Url == "/testsite");
        retrievedSite.Should().NotBeNull();
        retrievedSite!.Name.Should().Be("Test Site");
    }

    [Fact]
    public async Task Database_ShouldSupportPageEntities()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var site = new Site
        {
            Name = "Page Test Site",
            Description = "Site for page testing",
            Url = "/dbpagetest",
            AdminEmail = "admin@pagetest.com",
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };

        await context.Sites.AddAsync(site);
        await context.SaveChangesAsync();

        var page = new Page
        {
            SiteId = site.Id,
            Title = "Home Page",
            Path = "/home",
            Content = "Welcome to the home page",
            IsPublished = true,
            CreatedOn = DateTime.UtcNow
        };

        // Act
        await context.Pages.AddAsync(page);
        await context.SaveChangesAsync();

        // Assert
        var retrievedPage = await context.Pages
            .Include(p => p.Site)
            .FirstOrDefaultAsync(p => p.Path == "/home");

        retrievedPage.Should().NotBeNull();
        retrievedPage!.Title.Should().Be("Home Page");
        retrievedPage.Site.Should().NotBeNull();
        retrievedPage.Site.Url.Should().Be("/dbpagetest");
    }

    [Fact]
    public async Task Database_ShouldEnforceSoftDeleteQueryFilter()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user1 = new User
        {
            Username = "active",
            Email = "active@test.com",
            DisplayName = "Active User",
            PasswordHash = "hash",
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.UtcNow
        };

        var user2 = new User
        {
            Username = "deleted",
            Email = "deleted@test.com",
            DisplayName = "Deleted User",
            PasswordHash = "hash",
            IsActive = true,
            IsDeleted = true,
            CreatedOn = DateTime.UtcNow
        };

        await context.LegacyUsers.AddAsync(user1);
        await context.LegacyUsers.AddAsync(user2);
        await context.SaveChangesAsync();

        // Act
        var activeUsers = await context.LegacyUsers.ToListAsync();
        var allUsersIncludingDeleted = await context.LegacyUsers.IgnoreQueryFilters().ToListAsync();

        // Assert
        activeUsers.Should().Contain(u => u.Username == "active");
        activeUsers.Should().NotContain(u => u.Username == "deleted");
        allUsersIncludingDeleted.Should().Contain(u => u.Username == "deleted");
    }

    [Fact]
    public async Task Database_ShouldAutoSetCreatedOnAndModifiedOn()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = new User
        {
            Username = "timetest",
            Email = "timetest@test.com",
            DisplayName = "Time Test User",
            PasswordHash = "hash",
            IsActive = true
        };

        // Act - Create
        await context.LegacyUsers.AddAsync(user);
        await context.SaveChangesAsync();

        var beforeUpdate = DateTime.UtcNow;
        await Task.Delay(100); // Small delay to ensure time difference

        // Act - Update
        user.DisplayName = "Updated Name";
        context.LegacyUsers.Update(user);
        await context.SaveChangesAsync();

        // Assert
        user.CreatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        user.ModifiedOn.Should().NotBeNull();
        user.ModifiedOn.Should().BeOnOrAfter(beforeUpdate);
    }
}
