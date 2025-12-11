using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrkinosaiCMS.Infrastructure.Data;
using OrkinosaiCMS.Web;

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

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override configuration for testing - use InMemory database
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseProvider"] = "InMemory" // Use in-memory for tests
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove ALL database-related registrations
            var descriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)
            ).ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Remove ApplicationDbContext itself
            var contextDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ApplicationDbContext));
            if (contextDescriptor != null)
            {
                services.Remove(contextDescriptor);
            }

            // Add DbContext using in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryTestDb_" + Guid.NewGuid().ToString());
            }, ServiceLifetime.Scoped);

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
            var testUser = new OrkinosaiCMS.Core.Entities.Sites.User
            {
                Username = "testadmin",
                Email = "admin@test.com",
                DisplayName = "Test Admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!"),
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };

            context.Users.Add(testUser);

            var adminRole = new OrkinosaiCMS.Core.Entities.Sites.Role
            {
                Name = "Administrator",
                Description = "Administrator role",
                CreatedOn = DateTime.UtcNow
            };

            context.Roles.Add(adminRole);
            context.SaveChanges();

            // Assign role to user
            var userRole = new OrkinosaiCMS.Core.Entities.Sites.UserRole
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
