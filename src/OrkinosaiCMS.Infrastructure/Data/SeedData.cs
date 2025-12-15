using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var loggerFactory = scope.ServiceProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger("OrkinosaiCMS.SeedData");

        // Apply pending migrations to ensure all database tables exist (including Identity tables)
        // This is critical for production deployments where migrations need to be applied
        // Note: EnsureCreated() does NOT apply migrations and would skip Identity tables
        logger?.LogInformation("Checking for pending database migrations...");
        try
        {
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            var pendingMigrationsList = pendingMigrations.ToList();
            
            if (pendingMigrationsList.Any())
            {
                logger?.LogWarning("Found {Count} pending migrations: {Migrations}", 
                    pendingMigrationsList.Count, string.Join(", ", pendingMigrationsList));
                logger?.LogInformation("Applying pending migrations to database...");
                await context.Database.MigrateAsync();
                logger?.LogInformation("Database migrations applied successfully");
            }
            else
            {
                logger?.LogInformation("No pending migrations - database is up to date");
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error while checking or applying migrations. Attempting to ensure database exists...");
            // Fallback to EnsureCreated if migrations fail (e.g., in InMemory database for tests)
            await context.Database.EnsureCreatedAsync();
        }

        // Check if data already exists
        bool isFirstRun = !await context.Sites.AnyAsync();
        
        if (isFirstRun)
        {
            logger?.LogInformation("First run detected - seeding initial data...");
            
            await SeedThemesAsync(context);
            await SeedSiteAsync(context);
            await SeedMasterPagesAsync(context);
            await SeedModulesAsync(context);
            await SeedPagesAsync(context, logger);
            await SeedPermissionsAndRolesAsync(context);
            await SeedUsersAsync(context, configuration);

            await context.SaveChangesAsync();
            
            logger?.LogInformation("Initial data seeding completed successfully");
        }
        else
        {
            // Validate and repair critical data
            await ValidateAndRepairHomePageAsync(context, logger);
        }
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
            },
            // New requested themes for SharePoint-like theme selection
            new Theme
            {
                Name = "Modern Minimal",
                Description = "Sleek, minimalist design focusing on clean lines and whitespace",
                AssetsPath = "/css/themes/modern-minimal-theme.css",
                Category = "Business",
                LayoutType = "TopNavigation",
                PrimaryColor = "#1a1a1a",
                SecondaryColor = "#f5f5f5",
                AccentColor = "#0066ff",
                ThumbnailUrl = "/images/themes/modern-minimal.png",
                DefaultSettings = JsonSerializer.Serialize(new
                {
                    PrimaryColor = "#1a1a1a",
                    SecondaryColor = "#f5f5f5",
                    AccentColor = "#0066ff",
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
                Name = "Classic Corporate",
                Description = "Traditional professional theme ideal for corporate websites",
                AssetsPath = "/css/themes/classic-corporate-theme.css",
                Category = "Business",
                LayoutType = "TopNavigation",
                PrimaryColor = "#003366",
                SecondaryColor = "#336699",
                AccentColor = "#cc9900",
                ThumbnailUrl = "/images/themes/classic-corporate.png",
                DefaultSettings = JsonSerializer.Serialize(new
                {
                    PrimaryColor = "#003366",
                    SecondaryColor = "#336699",
                    AccentColor = "#cc9900",
                    FontFamily = "Georgia, serif"
                }),
                IsEnabled = true,
                IsSystem = true,
                IsMobileResponsive = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Theme
            {
                Name = "Bright Retail",
                Description = "Vibrant and eye-catching theme perfect for e-commerce and retail",
                AssetsPath = "/css/themes/bright-retail-theme.css",
                Category = "Commerce",
                LayoutType = "TopNavigation",
                PrimaryColor = "#ff6b6b",
                SecondaryColor = "#4ecdc4",
                AccentColor = "#ffe66d",
                ThumbnailUrl = "/images/themes/bright-retail.png",
                DefaultSettings = JsonSerializer.Serialize(new
                {
                    PrimaryColor = "#ff6b6b",
                    SecondaryColor = "#4ecdc4",
                    AccentColor = "#ffe66d",
                    FontFamily = "Poppins, sans-serif"
                }),
                IsEnabled = true,
                IsSystem = true,
                IsMobileResponsive = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Theme
            {
                Name = "Elegant Portfolio",
                Description = "Sophisticated theme showcasing creative work and portfolios",
                AssetsPath = "/css/themes/elegant-portfolio-theme.css",
                Category = "Portfolio",
                LayoutType = "TopNavigation",
                PrimaryColor = "#2d2d2d",
                SecondaryColor = "#8b7355",
                AccentColor = "#d4af37",
                ThumbnailUrl = "/images/themes/elegant-portfolio.png",
                DefaultSettings = JsonSerializer.Serialize(new
                {
                    PrimaryColor = "#2d2d2d",
                    SecondaryColor = "#8b7355",
                    AccentColor = "#d4af37",
                    FontFamily = "Playfair Display, serif"
                }),
                IsEnabled = true,
                IsSystem = true,
                IsMobileResponsive = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Theme
            {
                Name = "Dark Mode Pro",
                Description = "Professional dark theme reducing eye strain with elegant contrast",
                AssetsPath = "/css/themes/dark-mode-pro-theme.css",
                Category = "Modern",
                LayoutType = "TopNavigation",
                PrimaryColor = "#0a0a0a",
                SecondaryColor = "#1e1e1e",
                AccentColor = "#00d9ff",
                ThumbnailUrl = "/images/themes/dark-mode-pro.png",
                DefaultSettings = JsonSerializer.Serialize(new
                {
                    PrimaryColor = "#0a0a0a",
                    SecondaryColor = "#1e1e1e",
                    AccentColor = "#00d9ff",
                    FontFamily = "Inter, sans-serif",
                    DarkMode = true
                }),
                IsEnabled = true,
                IsSystem = true,
                IsMobileResponsive = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Theme
            {
                Name = "Vibrant Startup",
                Description = "Bold and energetic theme for innovative startups and tech companies",
                AssetsPath = "/css/themes/vibrant-startup-theme.css",
                Category = "Business",
                LayoutType = "TopNavigation",
                PrimaryColor = "#6c63ff",
                SecondaryColor = "#ff6584",
                AccentColor = "#ffd93d",
                ThumbnailUrl = "/images/themes/vibrant-startup.png",
                DefaultSettings = JsonSerializer.Serialize(new
                {
                    PrimaryColor = "#6c63ff",
                    SecondaryColor = "#ff6584",
                    AccentColor = "#ffd93d",
                    FontFamily = "Montserrat, sans-serif"
                }),
                IsEnabled = true,
                IsSystem = true,
                IsMobileResponsive = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Theme
            {
                Name = "News Magazine",
                Description = "Content-rich layout optimized for news and editorial websites",
                AssetsPath = "/css/themes/news-magazine-theme.css",
                Category = "Blog",
                LayoutType = "TopNavigation",
                PrimaryColor = "#212121",
                SecondaryColor = "#757575",
                AccentColor = "#d32f2f",
                ThumbnailUrl = "/images/themes/news-magazine.png",
                DefaultSettings = JsonSerializer.Serialize(new
                {
                    PrimaryColor = "#212121",
                    SecondaryColor = "#757575",
                    AccentColor = "#d32f2f",
                    FontFamily = "Merriweather, serif"
                }),
                IsEnabled = true,
                IsSystem = true,
                IsMobileResponsive = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Theme
            {
                Name = "Education School",
                Description = "Friendly and accessible theme for educational institutions",
                AssetsPath = "/css/themes/education-school-theme.css",
                Category = "Education",
                LayoutType = "TopNavigation",
                PrimaryColor = "#1976d2",
                SecondaryColor = "#43a047",
                AccentColor = "#ffa726",
                ThumbnailUrl = "/images/themes/education-school.png",
                DefaultSettings = JsonSerializer.Serialize(new
                {
                    PrimaryColor = "#1976d2",
                    SecondaryColor = "#43a047",
                    AccentColor = "#ffa726",
                    FontFamily = "Roboto, sans-serif"
                }),
                IsEnabled = true,
                IsSystem = true,
                IsMobileResponsive = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Theme
            {
                Name = "Health Wellness",
                Description = "Calming and trustworthy theme for healthcare and wellness sites",
                AssetsPath = "/css/themes/health-wellness-theme.css",
                Category = "Health",
                LayoutType = "TopNavigation",
                PrimaryColor = "#00897b",
                SecondaryColor = "#4db6ac",
                AccentColor = "#81c784",
                ThumbnailUrl = "/images/themes/health-wellness.png",
                DefaultSettings = JsonSerializer.Serialize(new
                {
                    PrimaryColor = "#00897b",
                    SecondaryColor = "#4db6ac",
                    AccentColor = "#81c784",
                    FontFamily = "Open Sans, sans-serif"
                }),
                IsEnabled = true,
                IsSystem = true,
                IsMobileResponsive = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Theme
            {
                Name = "Restaurant Food",
                Description = "Appetizing theme designed for restaurants and food businesses",
                AssetsPath = "/css/themes/restaurant-food-theme.css",
                Category = "Food",
                LayoutType = "TopNavigation",
                PrimaryColor = "#bf360c",
                SecondaryColor = "#ff6f00",
                AccentColor = "#ffd54f",
                ThumbnailUrl = "/images/themes/restaurant-food.png",
                DefaultSettings = JsonSerializer.Serialize(new
                {
                    PrimaryColor = "#bf360c",
                    SecondaryColor = "#ff6f00",
                    AccentColor = "#ffd54f",
                    FontFamily = "Raleway, sans-serif"
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

    private static async Task SeedPagesAsync(ApplicationDbContext context, ILogger? logger)
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
                Title = "CMS Demo Home",
                Path = "/cms",
                MetaDescription = "Welcome to Mosaic CMS Demo - A modern, modular Content Management System built on .NET 10 and Blazor",
                MetaKeywords = "CMS, .NET, Blazor, Content Management, Mosaic CMS",
                IsPublished = true,
                ShowInNavigation = true,
                Order = 1,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new Page
            {
                SiteId = site.Id,
                MasterPageId = fullWidthMaster.Id,
                Title = "CMS Demo Home (Legacy)",
                Path = "/cms-home",
                MetaDescription = "Welcome to Mosaic CMS Demo - A modern, modular Content Management System built on .NET 10 and Blazor",
                MetaKeywords = "CMS, .NET, Blazor, Content Management, Mosaic CMS",
                IsPublished = true,
                // Hidden from navigation - /cms-home is a legacy path kept for backward compatibility
                // The primary CMS demo page is at "/cms"
                ShowInNavigation = false,
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
        
        logger?.LogInformation("Created {Count} default CMS demo pages including primary page at path '/cms'", pages.Count);
    }

    /// <summary>
    /// Validates that a CMS demo home page exists and creates one if missing
    /// Note: The root "/" path is reserved for the SaaS portal (React app)
    /// </summary>
    private static async Task ValidateAndRepairHomePageAsync(ApplicationDbContext context, ILogger? logger)
    {
        // Check if a CMS demo home page (path = "/cms") exists for each site
        var sites = await context.Sites.ToListAsync();
        
        foreach (var site in sites)
        {
            var cmsHomePage = await context.Pages
                .FirstOrDefaultAsync(p => p.Path == "/cms" && p.SiteId == site.Id);
            
            if (cmsHomePage == null)
            {
                logger?.LogWarning("CMS demo home page not found for site '{SiteName}' (ID: {SiteId}). Creating default CMS home page...", 
                    site.Name, site.Id);
                
                // Get default master page
                var defaultMaster = await context.MasterPages
                    .FirstOrDefaultAsync(m => m.SiteId == site.Id && m.IsDefault)
                    ?? await context.MasterPages
                        .FirstOrDefaultAsync(m => m.SiteId == site.Id);
                
                if (defaultMaster == null)
                {
                    logger?.LogError("No master page found for site '{SiteName}' (ID: {SiteId}). Cannot create CMS demo home page.", 
                        site.Name, site.Id);
                    continue;
                }
                
                var newCmsHomePage = new Page
                {
                    SiteId = site.Id,
                    MasterPageId = defaultMaster.Id,
                    Title = "CMS Demo Home",
                    Path = "/cms",
                    MetaDescription = $"Welcome to {site.Name} CMS Demo",
                    IsPublished = true,
                    ShowInNavigation = true,
                    Order = 0,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "System (Auto-Repair)"
                };
                
                context.Pages.Add(newCmsHomePage);
                await context.SaveChangesAsync();
                
                logger?.LogInformation("Successfully created CMS demo home page for site '{SiteName}' (ID: {SiteId}) at path '/cms'", 
                    site.Name, site.Id);
            }
            else
            {
                // Ensure CMS home page is published
                if (!cmsHomePage.IsPublished)
                {
                    logger?.LogWarning("CMS demo home page for site '{SiteName}' (ID: {SiteId}) is not published. Publishing now...", 
                        site.Name, site.Id);
                    cmsHomePage.IsPublished = true;
                    cmsHomePage.ModifiedOn = DateTime.UtcNow;
                    await context.SaveChangesAsync();
                    logger?.LogInformation("CMS demo home page for site '{SiteName}' has been published", site.Name);
                }
                else
                {
                    logger?.LogInformation("CMS demo home page validation passed for site '{SiteName}' (ID: {SiteId})", 
                        site.Name, site.Id);
                }
            }
        }
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

        context.LegacyRoles.AddRange(roles);
        await context.SaveChangesAsync();

        // Assign permissions to roles
        var adminRole = await context.LegacyRoles.FirstAsync(r => r.Name == "Administrator");
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

    private static async Task SeedUsersAsync(ApplicationDbContext context, IConfiguration configuration)
    {
        // Note: Identity users (AspNetUsers, AspNetRoles) are seeded by IdentityUserSeeder
        // which uses UserManager to properly hash passwords and create users.
        // This method only seeds legacy users for backward compatibility.
        
        var defaultPassword = configuration.GetValue<string>("DefaultAdminPassword") ?? "Admin@123";
        
        // Only seed legacy Users table for backward compatibility
        // This can be removed once fully migrated to Identity
        if (!await context.LegacyUsers.AnyAsync())
        {
            var legacyAdminUser = new User
            {
                Username = "admin",
                Email = "admin@mosaicms.com",
                DisplayName = "Administrator",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword),
                IsActive = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            };

            context.LegacyUsers.Add(legacyAdminUser);
            
            var legacyAdminRole = await context.LegacyRoles.FirstOrDefaultAsync(r => r.Name == "Administrator");
            if (legacyAdminRole != null)
            {
                var legacyUserRole = new UserRole
                {
                    UserId = legacyAdminUser.Id,
                    RoleId = legacyAdminRole.Id,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "System"
                };
                context.LegacyUserRoles.Add(legacyUserRole);
            }
            
            await context.SaveChangesAsync();
        }
    }
}
