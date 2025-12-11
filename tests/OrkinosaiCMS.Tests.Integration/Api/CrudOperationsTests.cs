using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OrkinosaiCMS.Core.Entities.Sites;
using OrkinosaiCMS.Core.Interfaces.Services;
using OrkinosaiCMS.Tests.Integration.Fixtures;

namespace OrkinosaiCMS.Tests.Integration.Api;

/// <summary>
/// Integration tests for CRUD operations on main entities
/// </summary>
public class CrudOperationsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CrudOperationsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SiteService_ShouldPerformCRUDOperations()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();

        var newSite = new Site
        {
            Name = "CRUD Test Site",
            Description = "Site for CRUD testing",
            Url = "/crudtest",
            AdminEmail = "admin@crudtest.com",
            IsActive = true
        };

        // Act - Create
        var createdSite = await siteService.CreateSiteAsync(newSite);

        // Assert - Create
        createdSite.Should().NotBeNull();
        createdSite.Id.Should().BeGreaterThan(0);

        // Act - Read
        var retrievedSite = await siteService.GetSiteByIdAsync(createdSite.Id);

        // Assert - Read
        retrievedSite.Should().NotBeNull();
        retrievedSite!.Name.Should().Be("CRUD Test Site");

        // Act - Update
        retrievedSite.Description = "Updated Description";
        var updatedSite = await siteService.UpdateSiteAsync(retrievedSite);

        // Assert - Update
        updatedSite.Description.Should().Be("Updated Description");

        // Act - Delete
        await siteService.DeleteSiteAsync(createdSite.Id);

        // Assert - Delete (soft delete)
        var deletedSite = await siteService.GetSiteByIdAsync(createdSite.Id);
        deletedSite.Should().BeNull(); // Should not be found due to query filter
    }

    [Fact]
    public async Task PageService_ShouldPerformCRUDOperations()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
        var pageService = scope.ServiceProvider.GetRequiredService<IPageService>();

        // Create a site first
        var site = await siteService.CreateSiteAsync(new Site
        {
            Name = "Page Test Site",
            Url = "/pagetest",
            AdminEmail = "admin@pagetest.com",
            IsActive = true
        });

        var newPage = new Page
        {
            SiteId = site.Id,
            Title = "Test Page",
            Path = "/test-page",
            Content = "Test content",
            IsPublished = true
        };

        // Act - Create
        var createdPage = await pageService.CreateAsync(newPage);

        // Assert - Create
        createdPage.Should().NotBeNull();
        createdPage.Id.Should().BeGreaterThan(0);

        // Act - Read
        var retrievedPage = await pageService.GetByIdAsync(createdPage.Id);

        // Assert - Read
        retrievedPage.Should().NotBeNull();
        retrievedPage!.Title.Should().Be("Test Page");

        // Act - Update
        retrievedPage.Content = "Updated content";
        var updatedPage = await pageService.UpdateAsync(retrievedPage);

        // Assert - Update
        updatedPage.Content.Should().Be("Updated content");

        // Act - Delete
        await pageService.DeleteAsync(createdPage.Id);

        // Assert - Delete
        var deletedPage = await pageService.GetByIdAsync(createdPage.Id);
        deletedPage.Should().BeNull();
    }

    [Fact]
    public async Task UserService_ShouldPerformCRUDOperations()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var newUser = new User
        {
            Username = "cruduser",
            Email = "cruduser@test.com",
            DisplayName = "CRUD User"
        };

        // Act - Create
        var createdUser = await userService.CreateAsync(newUser, "Password123!");

        // Assert - Create
        createdUser.Should().NotBeNull();
        createdUser.Id.Should().BeGreaterThan(0);
        createdUser.IsActive.Should().BeTrue();

        // Act - Read
        var retrievedUser = await userService.GetByIdAsync(createdUser.Id);

        // Assert - Read
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Username.Should().Be("cruduser");

        // Act - Update
        retrievedUser.DisplayName = "Updated CRUD User";
        var updatedUser = await userService.UpdateAsync(retrievedUser);

        // Assert - Update
        updatedUser.DisplayName.Should().Be("Updated CRUD User");

        // Act - Delete
        await userService.DeleteAsync(createdUser.Id);

        // Assert - Delete
        var deletedUser = await userService.GetByIdAsync(createdUser.Id);
        deletedUser.Should().BeNull();
    }

    [Fact]
    public async Task RoleService_ShouldPerformCRUDOperations()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var roleService = scope.ServiceProvider.GetRequiredService<IRoleService>();

        var newRole = new Role
        {
            Name = "Test Role",
            Description = "Role for testing"
        };

        // Act - Create
        var createdRole = await roleService.CreateAsync(newRole);

        // Assert - Create
        createdRole.Should().NotBeNull();
        createdRole.Id.Should().BeGreaterThan(0);

        // Act - Read
        var retrievedRole = await roleService.GetByIdAsync(createdRole.Id);

        // Assert - Read
        retrievedRole.Should().NotBeNull();
        retrievedRole!.Name.Should().Be("Test Role");

        // Act - Update
        retrievedRole.Description = "Updated description";
        var updatedRole = await roleService.UpdateAsync(retrievedRole);

        // Assert - Update
        updatedRole.Description.Should().Be("Updated description");

        // Act - Delete
        await roleService.DeleteAsync(createdRole.Id);

        // Assert - Delete
        var deletedRole = await roleService.GetByIdAsync(createdRole.Id);
        deletedRole.Should().BeNull();
    }

    [Fact]
    public async Task UserService_ShouldManageUserRoles()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var roleService = scope.ServiceProvider.GetRequiredService<IRoleService>();

        var user = await userService.CreateAsync(new User
        {
            Username = "roleuser",
            Email = "roleuser@test.com",
            DisplayName = "Role User"
        }, "Password123!");

        var role1 = await roleService.CreateAsync(new Role
        {
            Name = "Role 1",
            Description = "First role"
        });

        var role2 = await roleService.CreateAsync(new Role
        {
            Name = "Role 2",
            Description = "Second role"
        });

        // Act - Assign roles
        await userService.AssignRolesAsync(user.Id, new[] { role1.Id, role2.Id });

        // Assert - Check assigned roles
        var userRoles = await userService.GetUserRolesAsync(user.Id);
        userRoles.Should().HaveCount(2);
        userRoles.Should().Contain(r => r.Name == "Role 1");
        userRoles.Should().Contain(r => r.Name == "Role 2");

        // Act - Remove one role
        await userService.RemoveRolesAsync(user.Id, new[] { role1.Id });

        // Assert - Check remaining roles
        var remainingRoles = await userService.GetUserRolesAsync(user.Id);
        remainingRoles.Should().HaveCount(1);
        remainingRoles.Should().Contain(r => r.Name == "Role 2");
    }

    [Fact]
    public async Task SiteService_ShouldGetAllSites()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();

        // Create multiple sites
        await siteService.CreateSiteAsync(new Site { Name = "Site 1", Url = "/site1", AdminEmail = "admin@site1.com", IsActive = true });
        await siteService.CreateSiteAsync(new Site { Name = "Site 2", Url = "/site2", AdminEmail = "admin@site2.com", IsActive = true });

        // Act
        var sites = await siteService.GetAllSitesAsync();

        // Assert
        sites.Should().NotBeNull();
        sites.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task PageService_ShouldGetPagesBySite()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var siteService = scope.ServiceProvider.GetRequiredService<ISiteService>();
        var pageService = scope.ServiceProvider.GetRequiredService<IPageService>();

        var site = await siteService.CreateSiteAsync(new Site
        {
            Name = "Multi-Page Site",
            Url = "/multipage",
            AdminEmail = "admin@multipage.com",
            IsActive = true
        });

        // Create multiple pages for the site
        await pageService.CreateAsync(new Page
        {
            SiteId = site.Id,
            Title = "Page 1",
            Path = "/page-1",
            Content = "Content 1",
            IsPublished = true
        });

        await pageService.CreateAsync(new Page
        {
            SiteId = site.Id,
            Title = "Page 2",
            Path = "/page-2",
            Content = "Content 2",
            IsPublished = true
        });

        // Act
        var pages = await pageService.GetBySiteAsync(site.Id);

        // Assert
        pages.Should().NotBeNull();
        pages.Should().HaveCount(2);
    }
}
