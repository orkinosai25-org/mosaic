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
    /// Uses enhanced DatabaseMigrationService adapted from Oqtane patterns
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var loggerFactory = scope.ServiceProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger("OrkinosaiCMS.SeedData");

        logger?.LogInformation("=== Starting Database Initialization ===");
        
        // Check if auto-apply migrations is enabled (defaults to true for backwards compatibility)
        var autoApplyMigrations = configuration.GetValue<bool?>("Database:AutoApplyMigrations") ?? true;
        logger?.LogInformation("Auto-apply migrations: {AutoApplyMigrations}", autoApplyMigrations);
        
        // Use enhanced migration service adapted from Oqtane for robust migration handling
        var migrationService = new Services.DatabaseMigrationService(
            context,
            loggerFactory?.CreateLogger<Services.DatabaseMigrationService>() 
                ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<Services.DatabaseMigrationService>.Instance,
            configuration);

        var migrationResult = await migrationService.MigrateDatabaseAsync();
        
        if (!migrationResult.Success)
        {
            logger?.LogError("Database migration failed: {Error}", migrationResult.ErrorMessage);
            
            // Fallback to EnsureCreated for testing scenarios only (InMemory database)
            var databaseProvider = configuration.GetValue<string>("DatabaseProvider");
            if (databaseProvider?.Equals("InMemory", StringComparison.OrdinalIgnoreCase) == true)
            {
                logger?.LogWarning("Using InMemory database - attempting EnsureCreatedAsync fallback");
                try
                {
                    await context.Database.EnsureCreatedAsync();
                    logger?.LogWarning("✓ Database created using EnsureCreatedAsync (InMemory only)");
                }
                catch (Exception ensureEx)
                {
                    logger?.LogCritical(ensureEx, "CRITICAL: Failed to initialize InMemory database");
                    throw new InvalidOperationException(
                        "Database initialization failed. Please check logs for details.", 
                        ensureEx);
                }
            }
            else
            {
                logger?.LogCritical("");
                logger?.LogCritical("=== CRITICAL: Database Migration Failed ===");
                logger?.LogCritical("");
                logger?.LogCritical("Error: {Error}", migrationResult.ErrorMessage);
                logger?.LogCritical("");
                logger?.LogCritical("This means the AspNetUsers table and other Identity tables are MISSING.");
                logger?.LogCritical("Admin login WILL NOT WORK until migrations are applied successfully.");
                logger?.LogCritical("");
                logger?.LogCritical("Common Causes:");
                logger?.LogCritical("  1. Database connection issues (network, firewall, credentials)");
                logger?.LogCritical("  2. Insufficient database permissions for user");
                logger?.LogCritical("  3. Database server not running or unreachable");
                logger?.LogCritical("  4. Migration conflicts or schema drift");
                if (!autoApplyMigrations)
                {
                    logger?.LogCritical("  5. Auto-apply migrations is DISABLED (Database:AutoApplyMigrations = false)");
                }
                logger?.LogCritical("");
                logger?.LogCritical("REQUIRED ACTION:");
                if (!autoApplyMigrations)
                {
                    logger?.LogCritical("  NOTE: Auto-apply migrations is DISABLED. You must apply migrations manually.");
                    logger?.LogCritical("");
                }
                logger?.LogCritical("  1. Verify database connection string in appsettings.json or environment variables");
                logger?.LogCritical("  2. Ensure database server is running and accessible");
                logger?.LogCritical("  3. Check database user has sufficient permissions (CREATE TABLE, ALTER, etc.)");
                logger?.LogCritical("  4. Review the error details above for specific issues");
                logger?.LogCritical("  5. Once issues are resolved, restart the application");
                logger?.LogCritical("");
                logger?.LogCritical("For manual migration application:");
                logger?.LogCritical("  dotnet ef database update --startup-project src/OrkinosaiCMS.Web");
                logger?.LogCritical("  OR");
                logger?.LogCritical("  bash scripts/apply-migrations.sh update");
                logger?.LogCritical("");
                if (!autoApplyMigrations)
                {
                    logger?.LogCritical("To enable auto-apply migrations, set Database__AutoApplyMigrations=true");
                    logger?.LogCritical("in appsettings.json or via environment variable.");
                    logger?.LogCritical("");
                }
                logger?.LogCritical("See DEPLOYMENT_VERIFICATION_GUIDE.md for detailed troubleshooting.");
                logger?.LogCritical("===========================================");
                logger?.LogCritical("");
                
                throw new InvalidOperationException(
                    $"Database migration failed: {migrationResult.ErrorMessage}. " +
                    $"AspNetUsers table and other Identity tables are missing. Admin login will not work. " +
                    $"See logs above for detailed troubleshooting steps.", 
                    migrationResult.Exception);
            }
        }
        else
        {
            logger?.LogInformation("✓ Database migration completed: {Message}", migrationResult.Message);
        }

        // Check if data already exists (now safe to query tables after migration)
        logger?.LogInformation("Checking if initial data seeding is required...");
        bool isFirstRun = false;
        try
        {
            isFirstRun = !await context.Sites.AnyAsync();
            logger?.LogInformation("First run check: {IsFirstRun} (Sites table {Status})", 
                isFirstRun, isFirstRun ? "is empty" : "has data");
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx)
        {
            logger?.LogError(sqlEx, "SQL error checking Sites table existence:");
            logger?.LogError("  SQL Error Number: {ErrorNumber} (Object name '{ObjectName}' may be invalid)", 
                sqlEx.Number, sqlEx.Number == 208 ? "Sites" : "unknown");
            logger?.LogWarning("Sites table may not exist yet. Treating as first run.");
            // If Sites table doesn't exist, it's definitely a first run
            isFirstRun = true;
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Unable to check if Sites table exists. Database may not be fully initialized.");
            logger?.LogWarning("Error type: {Type}", ex.GetType().FullName);
            // If Sites table doesn't exist, it's definitely a first run
            isFirstRun = true;
        }
        
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

    /// <summary>
    /// Handle schema drift by verifying database state and marking migrations as applied if needed
    /// </summary>
    private static async Task HandleSchemaDriftAsync(ApplicationDbContext context, ILogger? logger)
    {
        try
        {
            logger?.LogInformation("=== Schema Drift Recovery Process ===");
            
            // Check which critical tables exist
            var criticalTables = new[] { "Sites", "Modules", "AspNetUsers", "AspNetRoles", "Themes", "Pages" };
            var existingTables = new List<string>();
            
            // Batch check all tables in a single query to reduce round-trips
            var tableListParam = string.Join("','", criticalTables);
            var tableCheckQuery = $@"
                SELECT TABLE_NAME 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME IN ('{tableListParam}') 
                AND TABLE_TYPE = 'BASE TABLE'";
            
            try
            {
                var existingTablesResult = await context.Database.SqlQueryRaw<string>(tableCheckQuery).ToListAsync();
                existingTables.AddRange(existingTablesResult);
                
                foreach (var tableName in criticalTables)
                {
                    if (existingTables.Contains(tableName))
                    {
                        logger?.LogInformation("  ✓ Table '{TableName}' exists", tableName);
                    }
                    else
                    {
                        logger?.LogWarning("  ✗ Table '{TableName}' missing", tableName);
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Could not batch check tables - falling back to individual checks");
                
                // Fallback to individual checks if batch fails
                foreach (var tableName in criticalTables)
                {
                    try
                    {
                        // Note: Using parameterized query to prevent SQL injection
                        // We also validate the table name for defense in depth
                        if (!IsValidTableName(tableName))
                        {
                            logger?.LogWarning("Skipping invalid table name: {TableName}", tableName);
                            continue;
                        }
                        
                        var exists = await context.Database.SqlQueryRaw<int>(
                            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = {0}", tableName)
                            .FirstOrDefaultAsync();
                        
                        if (exists > 0)
                        {
                            existingTables.Add(tableName);
                            logger?.LogInformation("  ✓ Table '{TableName}' exists", tableName);
                        }
                        else
                        {
                            logger?.LogWarning("  ✗ Table '{TableName}' missing", tableName);
                        }
                    }
                    catch (Exception tableEx)
                    {
                        logger?.LogWarning(tableEx, "Could not check table '{TableName}'", tableName);
                    }
                }
            }
            
            // If Identity tables don't exist but other tables do, we have a partial schema
            bool hasIdentityTables = existingTables.Contains("AspNetUsers") && existingTables.Contains("AspNetRoles");
            bool hasCoreTables = existingTables.Contains("Sites") || existingTables.Contains("Modules");
            
            if (hasCoreTables && !hasIdentityTables)
            {
                logger?.LogWarning("SCHEMA DRIFT DETECTED:");
                logger?.LogWarning("  - Core tables (Sites, Modules, etc.) exist from older schema");
                logger?.LogWarning("  - Identity tables (AspNetUsers, AspNetRoles) are missing");
                logger?.LogWarning("  - This indicates migrations were not fully applied");
                logger?.LogError("CRITICAL: Cannot automatically recover from this state.");
                logger?.LogError("RESOLUTION OPTIONS:");
                logger?.LogError("  1. RECOMMENDED: Create a fresh database and apply all migrations cleanly");
                logger?.LogError("  2. MANUAL: Run migrations manually with '--context' flag");
                logger?.LogError("  3. RISKY: Drop and recreate database (will lose all data)");
                
                throw new InvalidOperationException(
                    "Database schema is in an inconsistent state. " +
                    "Identity tables are missing but core tables exist. " +
                    "Please create a fresh database or manually fix the schema.");
            }
            else if (hasIdentityTables && hasCoreTables)
            {
                logger?.LogInformation("Schema appears complete - all critical tables exist.");
                logger?.LogInformation("Attempting to continue with existing schema...");
                
                // Try to verify and repair if needed
                await VerifyIdentityTablesAsync(context, logger);
            }
            else
            {
                logger?.LogWarning("Unexpected schema state - attempting standard migration...");
                // Let the normal migration process handle it
                await context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Schema drift recovery failed");
            throw;
        }
    }

    /// <summary>
    /// Verify that Identity tables exist and have the correct structure
    /// </summary>
    private static async Task VerifyIdentityTablesAsync(ApplicationDbContext context, ILogger? logger)
    {
        try
        {
            logger?.LogInformation("Verifying Identity tables...");
            
            // Check all Identity tables in a single batched query
            var identityTables = new[] { "AspNetRoles", "AspNetUsers", "AspNetUserRoles" };
            var tableListParam = string.Join("','", identityTables);
            var tableCheckQuery = $@"
                SELECT TABLE_NAME 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME IN ('{tableListParam}') 
                AND TABLE_TYPE = 'BASE TABLE'";
            
            var existingIdentityTables = await context.Database.SqlQueryRaw<string>(tableCheckQuery).ToListAsync();
            
            var rolesTableExists = existingIdentityTables.Contains("AspNetRoles");
            var usersTableExists = existingIdentityTables.Contains("AspNetUsers");  
            var userRolesTableExists = existingIdentityTables.Contains("AspNetUserRoles");
            
            if (rolesTableExists && usersTableExists && userRolesTableExists)
            {
                logger?.LogInformation("✓ Identity tables verification PASSED:");
                logger?.LogInformation("  - AspNetRoles: EXISTS");
                logger?.LogInformation("  - AspNetUsers: EXISTS");
                logger?.LogInformation("  - AspNetUserRoles: EXISTS");
                
                // Verify we can query the tables (combined query for efficiency)
                try
                {
                    var countsQuery = @"
                        SELECT COUNT(*) FROM AspNetRoles
                        UNION ALL
                        SELECT COUNT(*) FROM AspNetUsers";
                    
                    var counts = await context.Database.SqlQueryRaw<int>(countsQuery).ToListAsync();
                    
                    logger?.LogInformation("✓ Identity tables are queryable:");
                    logger?.LogInformation("  - Roles count: {RoleCount}", counts.ElementAtOrDefault(0));
                    logger?.LogInformation("  - Users count: {UserCount}", counts.ElementAtOrDefault(1));
                }
                catch (Exception queryEx)
                {
                    logger?.LogWarning(queryEx, "Could not query Identity tables - they may need data seeding");
                }
            }
            else
            {
                logger?.LogError("✗ Identity tables verification FAILED:");
                logger?.LogError("  - AspNetRoles: {Status}", rolesTableExists ? "EXISTS" : "MISSING");
                logger?.LogError("  - AspNetUsers: {Status}", usersTableExists ? "EXISTS" : "MISSING");
                logger?.LogError("  - AspNetUserRoles: {Status}", userRolesTableExists ? "EXISTS" : "MISSING");
                logger?.LogError("CRITICAL: Identity tables are missing or incomplete!");
                logger?.LogError("Admin login will NOT work until Identity tables are properly created.");
                logger?.LogError("Please apply all database migrations using 'dotnet ef database update'.");
                
                throw new InvalidOperationException(
                    "Identity tables (AspNetUsers, AspNetRoles) are missing. " +
                    "Please apply all database migrations first using 'dotnet ef database update'.");
            }
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == SqlErrorCodes.InvalidObjectName)
        {
            // Table doesn't exist - this is expected in some scenarios
            logger?.LogWarning("Identity table verification failed - tables don't exist yet (this is normal for InMemory database)");
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            logger?.LogWarning(ex, "Could not verify Identity tables existence (this is normal for InMemory database)");
        }
    }
    
    /// <summary>
    /// Validates that a table name is safe to use in SQL queries (alphanumeric and underscores only)
    /// </summary>
    private static bool IsValidTableName(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName) || tableName.Length > 128)
            return false;
            
        // Allow only alphanumeric characters and underscores (SQL Server identifier rules)
        return tableName.All(c => char.IsLetterOrDigit(c) || c == '_');
    }
    
    /// <summary>
    /// SQL Server error codes used in migration handling
    /// </summary>
    private static class SqlErrorCodes
    {
        /// <summary>Invalid object name (table doesn't exist)</summary>
        public const int InvalidObjectName = 208;
        
        /// <summary>Object already exists</summary>
        public const int ObjectAlreadyExists = 2714;
    }
}
