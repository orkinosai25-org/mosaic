using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
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
/// - Seeds minimal test data (admin user and admin role)
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

                // Ensure the database is created
                db.Database.EnsureCreated();

                // Seed test data if needed
                SeedTestData(db);
            }
        });
    }

    private void SeedTestData(ApplicationDbContext context)
    {
        // Seed minimal test data for integration tests
        // Only creates a basic admin user and role - tests should create their own data as needed
        // This prevents test interference where seeded data affects test expectations
        if (!context.Users.Any())
        {
            var testUser = new User
            {
                Username = "testadmin",
                Email = "admin@test.com",
                DisplayName = "Test Admin",
                // Password: TestPassword123! - used in authentication tests
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!"),
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };

            context.Users.Add(testUser);

            var adminRole = new Role
            {
                Name = "Administrator",
                Description = "Administrator role",
                CreatedOn = DateTime.UtcNow
            };

            context.Roles.Add(adminRole);
            context.SaveChanges();

            // Assign role to user
            var userRole = new UserRole
            {
                UserId = testUser.Id,
                RoleId = adminRole.Id,
                CreatedOn = DateTime.UtcNow
            };

            context.UserRoles.Add(userRole);
            context.SaveChanges();
        }
    }
}
