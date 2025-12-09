using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrkinosaiCMS.Core.Entities.Sites;
using System.Text.Json;

namespace OrkinosaiCMS.Infrastructure.Data;

/// <summary>
/// Seeds initial data for OrkinosaiCMS
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Initialize database with seed data
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Check if data already exists
        if (await context.Sites.AnyAsync())
        {
            return; // Database has been seeded
        }

        await SeedThemesAsync(context);
        await SeedSiteAsync(context);
        await SeedMasterPagesAsync(context);
        await SeedModulesAsync(context);
        await SeedPagesAsync(context);
        await SeedPermissionsAndRolesAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task SeedThemesAsync(ApplicationDbContext context)
    {
        var themes = new List<Theme>
        {
            new Theme
            {
                Name = "Orkinosai Professional",
                Description = "Modern, clean professional theme with blue and green color scheme",
                AssetsPath = "/css/themes/orkinosai-theme.css",
                Category = "Modern",
                LayoutType = "TopNavigation",
                PrimaryColor = "#0066cc",
                SecondaryColor = "#00a86b",
                AccentColor = "#ff6b35",
                DefaultSettings = JsonSerializer.Serialize(new
                {
                    PrimaryColor = "#0066cc",
                    SecondaryColor = "#00a86b",
                    AccentColor = "#ff6b35",
                    FontFamily = "Segoe UI, sans-serif"
                }),
                IsEnabled = true,
                IsSystem = true,
                IsMobileResponsive = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Theme
            {
                Name = "SharePoint Portal",
                Description = "SharePoint-inspired portal theme with left navigation and modern UI",
                AssetsPath = "/css/themes/sharepoint-portal-theme.css",
                Category = "SharePoint",
                LayoutType = "LeftNavigation",
                PrimaryColor = "#0078d4",
                SecondaryColor = "#005a9e",
                AccentColor = "#8764b8",
                ThumbnailUrl = "/images/themes/sharepoint-portal.png",
                DefaultSettings = JsonSerializer.Serialize(new
                {
                    PrimaryColor = "#0078d4",
                    SecondaryColor = "#005a9e",
                    AccentColor = "#8764b8",
                    FontFamily = "Segoe UI, sans-serif",
                    ShowQuickLaunch = true,
                    ShowSuiteBar = true
                }),
                IsEnabled = true,
                IsSystem = true,
                IsMobileResponsive = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Theme
            {
                Name = "Top Navigation",
                Description = "Clean modern theme with top horizontal navigation",
                AssetsPath = "/css/themes/top-navigation-theme.css",
                Category = "Modern",
                LayoutType = "TopNavigation",
                PrimaryColor = "#2563eb",
                SecondaryColor = "#059669",
                AccentColor = "#f59e0b",
                ThumbnailUrl = "/images/themes/top-navigation.png",
                DefaultSettings = JsonSerializer.Serialize(new
                {
                    PrimaryColor = "#2563eb",
                    SecondaryColor = "#059669",
                    AccentColor = "#f59e0b",
                    FontFamily = "Inter, sans-serif"
                }),
                IsEnabled = true,
                IsSystem = true,
                IsMobileResponsive = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Theme
            {
                Name = "Dashboard",
                Description = "Modern dashboard theme perfect for admin interfaces and data visualization",
                AssetsPath = "/css/themes/dashboard-theme.css",
                Category = "Dashboard",
                LayoutType = "LeftNavigation",
                PrimaryColor = "#6366f1",
                SecondaryColor = "#10b981",
                AccentColor = "#f59e0b",
                ThumbnailUrl = "/images/themes/dashboard.png",
                DefaultSettings = JsonSerializer.Serialize(new
                {
                    PrimaryColor = "#6366f1",
                    SecondaryColor = "#10b981",
                    AccentColor = "#f59e0b",
                    FontFamily = "Inter, sans-serif",
                    ShowSidebar = true,
                    DarkMode = false
                }),
                IsEnabled = true,
                IsSystem = true,
                IsMobileResponsive = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Theme
            {
                Name = "Minimal",
                Description = "Clean and simple design with focus on content and readability",
                AssetsPath = "/css/themes/minimal-theme.css",
                Category = "Minimal",
                LayoutType = "TopNavigation",
                PrimaryColor = "#000000",
                SecondaryColor = "#666666",
                AccentColor = "#0066cc",
                ThumbnailUrl = "/images/themes/minimal.png",
                DefaultSettings = JsonSerializer.Serialize(new
                {
                    PrimaryColor = "#000000",
                    SecondaryColor = "#666666",
                    AccentColor = "#0066cc",
                    FontFamily = "Helvetica Neue, Arial, sans-serif"
                }),
                IsEnabled = true,
                IsSystem = true,
                IsMobileResponsive = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Theme
            {
                Name = "Marketing Landing",
                Description = "Bold, conversion-focused theme perfect for marketing and landing pages",
                AssetsPath = "/css/themes/marketing-theme.css",
                Category = "Marketing",
                LayoutType = "TopNavigation",
                PrimaryColor = "#7c3aed",
                SecondaryColor = "#ec4899",
                AccentColor = "#06b6d4",
                ThumbnailUrl = "/images/themes/marketing.png",
                DefaultSettings = JsonSerializer.Serialize(new
                {
                    PrimaryColor = "#7c3aed",
                    SecondaryColor = "#ec4899",
                    AccentColor = "#06b6d4",
                    FontFamily = "Inter, sans-serif",
                    ShowHero = true,
                    ShowCTA = true
                }),
                IsEnabled = true,
                IsSystem = true,
                IsMobileResponsive = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            }
        };

        context.Themes.AddRange(themes);
        await context.SaveChangesAsync();
    }

    private static async Task SeedMasterPagesAsync(ApplicationDbContext context)
    {
        var site = await context.Sites.FirstAsync();
        
        var masterPages = new List<MasterPage>
        {
            new MasterPage
            {
                SiteId = site.Id,
                Name = "Standard Layout",
                Description = "Standard page layout with header, navigation, main content, sidebar, and footer",
                ComponentPath = "/Components/MasterPages/StandardMasterPage.razor",
                ContentZones = JsonSerializer.Serialize(new[] 
                { 
                    "Header", 
                    "Navigation", 
                    "Main", 
                    "Sidebar", 
                    "Footer" 
                }),
                IsDefault = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new MasterPage
            {
                SiteId = site.Id,
                Name = "Full Width Layout",
                Description = "Full-width landing page layout with hero section and multi-column footer",
                ComponentPath = "/Components/MasterPages/FullWidthMasterPage.razor",
                ContentZones = JsonSerializer.Serialize(new[] 
                { 
                    "Header", 
                    "Navigation", 
                    "Hero", 
                    "Main", 
                    "FooterColumn1", 
                    "FooterColumn2", 
                    "FooterColumn3", 
                    "FooterColumn4" 
                }),
                IsDefault = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            }
        };

        context.MasterPages.AddRange(masterPages);
        await context.SaveChangesAsync();
    }

    private static async Task SeedModulesAsync(ApplicationDbContext context)
    {
        var modules = new List<Module>
        {
            new Module
            {
                Name = "HtmlContent",
                Title = "HTML Content Module",
                Description = "Display rich HTML content",
                Version = "1.0.0",
                AssemblyName = "OrkinosaiCMS.Modules.Content",
                ComponentType = "OrkinosaiCMS.Modules.Content.HtmlContentModule",
                Category = "Content",
                IsEnabled = true,
                IsSystem = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Module
            {
                Name = "Hero",
                Title = "Hero Section",
                Description = "Eye-catching hero section with title, subtitle, and call-to-action",
                Version = "1.0.0",
                AssemblyName = "OrkinosaiCMS.Modules.Hero",
                ComponentType = "OrkinosaiCMS.Modules.Hero.HeroModule",
                Category = "Marketing",
                IsEnabled = true,
                IsSystem = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Module
            {
                Name = "Features",
                Title = "Features Grid",
                Description = "Display features in a responsive grid with icons",
                Version = "1.0.0",
                AssemblyName = "OrkinosaiCMS.Modules.Features",
                ComponentType = "OrkinosaiCMS.Modules.Features.FeaturesModule",
                Category = "Marketing",
                IsEnabled = true,
                IsSystem = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Module
            {
                Name = "ContactForm",
                Title = "Contact Form",
                Description = "Contact form with validation",
                Version = "1.0.0",
                AssemblyName = "OrkinosaiCMS.Modules.ContactForm",
                ComponentType = "OrkinosaiCMS.Modules.ContactForm.ContactFormModule",
                Category = "Forms",
                IsEnabled = true,
                IsSystem = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            }
        };

        context.Modules.AddRange(modules);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSiteAsync(ApplicationDbContext context)
    {
        var theme = await context.Themes.FirstAsync();

        var site = new Site
        {
            Name = "OrkinosaiCMS Demo Site",
            Url = "https://localhost:5001",
            Description = "A modern, modular Content Management System built on .NET 10 and Blazor",
            ThemeId = theme.Id,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System"
        };

        context.Sites.Add(site);
        await context.SaveChangesAsync();
    }

    private static async Task SeedPagesAsync(ApplicationDbContext context)
    {
        var site = await context.Sites.FirstAsync();
        var standardMaster = await context.MasterPages
            .FirstAsync(m => m.Name == "Standard Layout");
        var fullWidthMaster = await context.MasterPages
            .FirstAsync(m => m.Name == "Full Width Layout");

        var pages = new List<Page>
        {
            new Page
            {
                SiteId = site.Id,
                MasterPageId = fullWidthMaster.Id,
                Title = "Home - OrkinosaiCMS",
                Path = "/cms-home",
                MetaDescription = "Welcome to OrkinosaiCMS - A modern, modular Content Management System built on .NET 10 and Blazor",
                MetaKeywords = "CMS, .NET, Blazor, Content Management, OrkinosaiCMS",
                IsPublished = true,
                ShowInNavigation = true,
                Order = 1,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Page
            {
                SiteId = site.Id,
                MasterPageId = standardMaster.Id,
                Title = "About - OrkinosaiCMS",
                Path = "/cms-about",
                MetaDescription = "Learn about OrkinosaiCMS architecture, vision, and technology stack",
                MetaKeywords = "About, Architecture, Vision, Technology",
                IsPublished = true,
                ShowInNavigation = true,
                Order = 2,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Page
            {
                SiteId = site.Id,
                MasterPageId = standardMaster.Id,
                Title = "Contact - OrkinosaiCMS",
                Path = "/cms-contact",
                MetaDescription = "Get in touch with the OrkinosaiCMS team",
                MetaKeywords = "Contact, Support, Get in Touch",
                IsPublished = true,
                ShowInNavigation = true,
                Order = 3,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            }
        };

        context.Pages.AddRange(pages);
        await context.SaveChangesAsync();
    }

    private static async Task SeedPermissionsAndRolesAsync(ApplicationDbContext context)
    {
        // Seed Permissions
        var permissions = new List<Permission>
        {
            new Permission { Name = "View", Description = "View content", CreatedOn = DateTime.UtcNow, CreatedBy = "System" },
            new Permission { Name = "Edit", Description = "Edit content", CreatedOn = DateTime.UtcNow, CreatedBy = "System" },
            new Permission { Name = "Delete", Description = "Delete content", CreatedOn = DateTime.UtcNow, CreatedBy = "System" },
            new Permission { Name = "Manage", Description = "Manage site settings", CreatedOn = DateTime.UtcNow, CreatedBy = "System" },
            new Permission { Name = "Publish", Description = "Publish content", CreatedOn = DateTime.UtcNow, CreatedBy = "System" },
            new Permission { Name = "Design", Description = "Modify site design", CreatedOn = DateTime.UtcNow, CreatedBy = "System" }
        };

        context.Permissions.AddRange(permissions);
        await context.SaveChangesAsync();

        // Seed Roles
        var roles = new List<Role>
        {
            new Role 
            { 
                Name = "Administrator", 
                Description = "Full control over the site", 
                IsSystem = true,
                CreatedOn = DateTime.UtcNow, 
                CreatedBy = "System" 
            },
            new Role 
            { 
                Name = "Designer", 
                Description = "Design and layout management", 
                IsSystem = true,
                CreatedOn = DateTime.UtcNow, 
                CreatedBy = "System" 
            },
            new Role 
            { 
                Name = "Editor", 
                Description = "Create, edit, and publish content", 
                IsSystem = true,
                CreatedOn = DateTime.UtcNow, 
                CreatedBy = "System" 
            },
            new Role 
            { 
                Name = "Contributor", 
                Description = "Create and edit own content", 
                IsSystem = true,
                CreatedOn = DateTime.UtcNow, 
                CreatedBy = "System" 
            },
            new Role 
            { 
                Name = "Reader", 
                Description = "View published content", 
                IsSystem = true,
                CreatedOn = DateTime.UtcNow, 
                CreatedBy = "System" 
            }
        };

        context.Roles.AddRange(roles);
        await context.SaveChangesAsync();

        // Assign permissions to roles
        var adminRole = await context.Roles.FirstAsync(r => r.Name == "Administrator");
        var allPermissions = await context.Permissions.ToListAsync();
        
        foreach (var permission in allPermissions)
        {
            context.RolePermissions.Add(new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = permission.Id,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            });
        }

        await context.SaveChangesAsync();
    }
}
