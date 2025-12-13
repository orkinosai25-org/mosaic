using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrkinosaiCMS.Infrastructure.Data;
using OrkinosaiCMS.Tests.Integration.Fixtures;

namespace OrkinosaiCMS.Tests.Integration.Database;

/// <summary>
/// Integration tests for home page seeding and validation
/// </summary>
public class HomePageSeedingTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public HomePageSeedingTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SeedData_ShouldCreateHomePageAtRootPath()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Act
        var homePage = await context.Pages
            .FirstOrDefaultAsync(p => p.Path == "/");

        // Assert
        homePage.Should().NotBeNull("a home page at path '/' should exist after seeding");
        homePage!.Title.Should().NotBeNullOrEmpty();
        homePage.IsPublished.Should().BeTrue("the home page should be published");
    }

    [Fact]
    public async Task SeedData_ShouldCreateAtLeastOnePublishedPage()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Act
        var publishedPages = await context.Pages
            .Where(p => p.IsPublished)
            .ToListAsync();

        // Assert
        publishedPages.Should().NotBeEmpty("at least one published page should exist");
    }

    [Fact]
    public async Task SeedData_HomePageShouldBeInNavigation()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Act
        var homePage = await context.Pages
            .FirstOrDefaultAsync(p => p.Path == "/");

        // Assert
        homePage.Should().NotBeNull();
        homePage!.ShowInNavigation.Should().BeTrue("home page should appear in navigation");
    }

    [Fact]
    public async Task SeedData_HomePageShouldHaveValidMasterPage()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Act
        var homePage = await context.Pages
            .Include(p => p.Site)
            .FirstOrDefaultAsync(p => p.Path == "/");

        // Assert
        homePage.Should().NotBeNull();
        homePage!.MasterPageId.Should().NotBeNull("home page should have a master page assigned");
        homePage.SiteId.Should().BeGreaterThan(0, "home page should belong to a site");
    }

    [Fact]
    public async Task SeedData_ShouldCreateNavigationPages()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Act
        var navigationPages = await context.Pages
            .Where(p => p.ShowInNavigation && p.IsPublished)
            .OrderBy(p => p.Order)
            .ToListAsync();

        // Assert
        navigationPages.Should().NotBeEmpty("navigation should have at least one page");
        navigationPages.Should().Contain(p => p.Path == "/" || p.Path == "/cms-home", 
            "navigation should include the home page");
    }

    [Fact]
    public async Task ValidateAndRepair_ShouldCreateHomePageIfMissing()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Remove home page if it exists (for testing repair functionality)
        var existingHomePage = await context.Pages.FirstOrDefaultAsync(p => p.Path == "/");
        if (existingHomePage != null)
        {
            context.Pages.Remove(existingHomePage);
            await context.SaveChangesAsync();
        }

        // Act - Call the seed data initialization which includes validation
        await SeedData.InitializeAsync(scope.ServiceProvider);

        // Assert
        var homePage = await context.Pages.FirstOrDefaultAsync(p => p.Path == "/");
        homePage.Should().NotBeNull("home page should be created by auto-repair");
        homePage!.IsPublished.Should().BeTrue("auto-created home page should be published");
    }

    [Fact]
    public async Task ValidateAndRepair_ShouldPublishUnpublishedHomePage()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Unpublish home page if it exists
        var homePage = await context.Pages.FirstOrDefaultAsync(p => p.Path == "/");
        if (homePage != null)
        {
            homePage.IsPublished = false;
            await context.SaveChangesAsync();
            
            // Detach the entity so we can get a fresh copy after validation
            context.Entry(homePage).State = EntityState.Detached;
        }

        // Act - Call the seed data initialization which includes validation
        await SeedData.InitializeAsync(scope.ServiceProvider);

        // Assert - Reload from database to get updated state
        var reloadedHomePage = await context.Pages
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Path == "/");
        reloadedHomePage.Should().NotBeNull();
        reloadedHomePage!.IsPublished.Should().BeTrue("unpublished home page should be auto-published");
    }
}
