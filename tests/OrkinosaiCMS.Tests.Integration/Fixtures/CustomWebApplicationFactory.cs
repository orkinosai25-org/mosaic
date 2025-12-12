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
/// Configures test database and services
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
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
        // Seed initial test data
        if (!context.Users.Any())
        {
            var testUser = new User
            {
                Username = "testadmin",
                Email = "admin@test.com",
                DisplayName = "Test Admin",
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
