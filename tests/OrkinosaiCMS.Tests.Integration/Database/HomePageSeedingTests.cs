using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrkinosaiCMS.Infrastructure.Data;
using OrkinosaiCMS.Tests.Integration.Fixtures;

namespace OrkinosaiCMS.Tests.Integration.Database;

/// <summary>
/// Integration tests for CMS demo home page seeding and validation
/// Note: The root "/" path is reserved for the SaaS portal (React app)
/// The CMS demo home page is at "/cms"
/// </summary>
public class HomePageSeedingTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public HomePageSeedingTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SeedData_ShouldCreateCmsHomePageAtCmsPath()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Act
        var cmsHomePage = await context.Pages
            .FirstOrDefaultAsync(p => p.Path == "/cms");

        // Assert
        cmsHomePage.Should().NotBeNull("a CMS demo home page at path '/cms' should exist after seeding");
        cmsHomePage!.Title.Should().NotBeNullOrEmpty();
        cmsHomePage.IsPublished.Should().BeTrue("the CMS demo home page should be published");
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
    public async Task SeedData_CmsHomePageShouldBeInNavigation()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Act
        var cmsHomePage = await context.Pages
            .FirstOrDefaultAsync(p => p.Path == "/cms");

        // Assert
        cmsHomePage.Should().NotBeNull();
        cmsHomePage!.ShowInNavigation.Should().BeTrue("CMS demo home page should appear in navigation");
    }

    [Fact]
    public async Task SeedData_CmsHomePageShouldHaveValidMasterPage()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Act
        var cmsHomePage = await context.Pages
            .Include(p => p.Site)
            .FirstOrDefaultAsync(p => p.Path == "/cms");

        // Assert
        cmsHomePage.Should().NotBeNull();
        cmsHomePage!.MasterPageId.Should().NotBeNull("CMS demo home page should have a master page assigned");
        cmsHomePage.SiteId.Should().BeGreaterThan(0, "CMS demo home page should belong to a site");
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
        // The CMS demo home page at "/cms" should be in navigation
        navigationPages.Should().Contain(p => p.Path == "/cms", 
            "navigation should include the CMS demo home page at '/cms'");
    }

    [Fact]
    public async Task ValidateAndRepair_ShouldCreateCmsHomePageIfMissing()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Remove CMS home page if it exists (for testing repair functionality)
        var existingCmsHomePage = await context.Pages.FirstOrDefaultAsync(p => p.Path == "/cms");
        if (existingCmsHomePage != null)
        {
            context.Pages.Remove(existingCmsHomePage);
            await context.SaveChangesAsync();
        }

        // Act - Call the seed data initialization which includes validation
        await SeedData.InitializeAsync(scope.ServiceProvider);

        // Assert
        var cmsHomePage = await context.Pages.FirstOrDefaultAsync(p => p.Path == "/cms");
        cmsHomePage.Should().NotBeNull("CMS demo home page should be created by auto-repair");
        cmsHomePage!.IsPublished.Should().BeTrue("auto-created CMS demo home page should be published");
    }

    [Fact]
    public async Task ValidateAndRepair_ShouldPublishUnpublishedCmsHomePage()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Unpublish CMS home page if it exists
        var cmsHomePage = await context.Pages.FirstOrDefaultAsync(p => p.Path == "/cms");
        if (cmsHomePage != null)
        {
            cmsHomePage.IsPublished = false;
            await context.SaveChangesAsync();
            
            // Detach the entity so we can get a fresh copy after validation
            context.Entry(cmsHomePage).State = EntityState.Detached;
        }

        // Act - Call the seed data initialization which includes validation
        await SeedData.InitializeAsync(scope.ServiceProvider);

        // Assert - Reload from database to get updated state
        var reloadedCmsHomePage = await context.Pages
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Path == "/cms");
        reloadedCmsHomePage.Should().NotBeNull();
        reloadedCmsHomePage!.IsPublished.Should().BeTrue("unpublished CMS demo home page should be auto-published");
    }
}
