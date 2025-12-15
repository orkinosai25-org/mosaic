using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using OrkinosaiCMS.Core.Entities.Identity;
using OrkinosaiCMS.Core.Entities.Sites;
using OrkinosaiCMS.Infrastructure.Data;
using OrkinosaiCMS.Web;
using Serilog;
using Serilog.Events;

namespace OrkinosaiCMS.Tests.Integration.Fixtures;

/// <summary>
/// Custom WebApplicationFactory for integration tests
/// Configures test database and services with proper test isolation
/// 
/// KEY FEATURES:
/// - Uses a unique InMemory database instance per test class to prevent test interference
/// - Configures test-specific settings (e.g., Stripe test keys)
/// - Seeds minimal test data (admin user and admin role using ASP.NET Core Identity)
/// - Isolated from production/CI database configuration
/// 
/// TEST ISOLATION STRATEGY:
/// Each test class that uses this factory gets its own database instance via IClassFixture.
/// This prevents tests from interfering with each other when they create/modify data.
/// For example, if one test creates a Pro subscription for admin@test.com, it won't affect
/// another test that expects admin@test.com to have no subscription (Free tier).
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // Use a unique database name for each test factory instance to avoid test interference
    // This ensures each test class has its own isolated database
    // Example: One test class creates subscriptions, another tests without subscriptions
    private readonly string _databaseName = $"InMemoryTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Set configuration before services are configured
        builder.UseSetting("DatabaseProvider", "InMemory");
        
        // Configure test-specific Stripe settings
        builder.UseSetting("Payment:Stripe:SecretKey", "sk_test_dummy");
        builder.UseSetting("Payment:Stripe:PublishableKey", "pk_test_dummy");
        builder.UseSetting("Payment:Stripe:WebhookSecret", "whsec_test_dummy");
        
        // Configure Stripe price IDs for tests
        builder.UseSetting("Payment:Stripe:PriceIds:Starter_Monthly", "price_test_starter_monthly");
        builder.UseSetting("Payment:Stripe:PriceIds:Starter_Yearly", "price_test_starter_yearly");
        builder.UseSetting("Payment:Stripe:PriceIds:Pro_Monthly", "price_test_pro_monthly");
        builder.UseSetting("Payment:Stripe:PriceIds:Pro_Yearly", "price_test_pro_yearly");
        builder.UseSetting("Payment:Stripe:PriceIds:Business_Monthly", "price_test_business_monthly");
        builder.UseSetting("Payment:Stripe:PriceIds:Business_Yearly", "price_test_business_yearly");

        // Configure logging for tests - use a fresh logger instance
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Warning); // Reduce noise in tests
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration to replace with test-specific one
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add DbContext with unique InMemory database for test isolation
            // Each test class instance gets its own database to prevent test interference
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole<int>>>();

                // Ensure the database is created
                db.Database.EnsureCreated();

                // Seed test data if needed
                SeedTestData(db, userManager, roleManager).Wait();
            }
        });
    }

    private async Task SeedTestData(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<int>> roleManager)
    {
        // Seed minimal test data for integration tests using ASP.NET Core Identity
        // Only creates a basic admin user and role - tests should create their own data as needed
        // This prevents test interference where seeded data affects test expectations
        
        // Check if user already exists
        var existingUser = await userManager.FindByNameAsync("testadmin");
        if (existingUser == null)
        {
            // Create Administrator role first in AspNetRoles (Identity table)
            var adminRoleExists = await roleManager.RoleExistsAsync("Administrator");
            if (!adminRoleExists)
            {
                var adminRole = new IdentityRole<int>
                {
                    Name = "Administrator",
                    NormalizedName = "ADMINISTRATOR"
                };
                var roleResult = await roleManager.CreateAsync(adminRole);
                if (!roleResult.Succeeded)
                {
                    throw new Exception($"Failed to create Administrator role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }

            // Create test user in AspNetUsers (Identity table)
            var testUser = new ApplicationUser
            {
                UserName = "testadmin",
                Email = "admin@test.com",
                DisplayName = "Test Admin",
                EmailConfirmed = true, // Auto-confirm for tests
                CreatedOn = DateTime.UtcNow
            };

            // Password: TestPassword123! - used in authentication tests
            var userResult = await userManager.CreateAsync(testUser, "TestPassword123!");
            if (!userResult.Succeeded)
            {
                throw new Exception($"Failed to create test user: {string.Join(", ", userResult.Errors.Select(e => e.Description))}");
            }

            // Assign Administrator role to user in AspNetUserRoles (Identity table)
            var addRoleResult = await userManager.AddToRoleAsync(testUser, "Administrator");
            if (!addRoleResult.Succeeded)
            {
                throw new Exception($"Failed to assign role to test user: {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
            }

            // Also seed legacy tables for backward compatibility tests
            // Create role first to avoid FK constraint errors
            var legacyAdminRole = new Role
            {
                Name = "Administrator",
                Description = "Administrator role",
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            };

            context.LegacyRoles.Add(legacyAdminRole);
            await context.SaveChangesAsync();

            // Then create legacy user
            var legacyTestUser = new User
            {
                Username = "testadmin",
                Email = "admin@test.com",
                DisplayName = "Test Admin",
                // Password: TestPassword123! - used in authentication tests
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!"),
                IsActive = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            };

            context.LegacyUsers.Add(legacyTestUser);
            await context.SaveChangesAsync();

            // Finally assign role to user (now both FK references exist)
            var legacyUserRole = new UserRole
            {
                UserId = legacyTestUser.Id,
                RoleId = legacyAdminRole.Id,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "System"
            };

            context.LegacyUserRoles.Add(legacyUserRole);
            await context.SaveChangesAsync();
        }
    }
}
