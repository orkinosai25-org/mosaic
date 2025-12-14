using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrkinosaiCMS.Core.Entities.Identity;

namespace OrkinosaiCMS.Infrastructure.Services;

/// <summary>
/// Service to seed Identity users (uses UserManager properly)
/// Following Oqtane's pattern of seeding users with Identity
/// </summary>
public class IdentityUserSeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IdentityUserSeeder> _logger;

    public IdentityUserSeeder(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        IConfiguration configuration,
        ILogger<IdentityUserSeeder> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        // Create Administrator role if it doesn't exist
        var adminRoleName = "Administrator";
        if (!await _roleManager.RoleExistsAsync(adminRoleName))
        {
            _logger.LogInformation("Creating Administrator role");
            var adminRole = new IdentityRole<int> { Name = adminRoleName };
            await _roleManager.CreateAsync(adminRole);
        }

        // Check if admin user already exists
        var adminUsername = "admin";
        var existingAdmin = await _userManager.FindByNameAsync(adminUsername);
        
        if (existingAdmin == null)
        {
            _logger.LogInformation("Creating default admin user");
            
            // Get default admin password from configuration or use default
            var defaultPassword = _configuration.GetValue<string>("DefaultAdminPassword") ?? "Admin@123";

            var adminUser = new ApplicationUser
            {
                UserName = adminUsername,
                Email = "admin@mosaicms.com",
                DisplayName = "Administrator",
                EmailConfirmed = true,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow
            };

            // Create user with password using UserManager (Oqtane approach)
            var result = await _userManager.CreateAsync(adminUser, defaultPassword);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("Admin user created successfully");
                
                // Assign Administrator role
                await _userManager.AddToRoleAsync(adminUser, adminRoleName);
                _logger.LogInformation("Administrator role assigned to admin user");
            }
            else
            {
                _logger.LogError("Failed to create admin user: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            _logger.LogInformation("Admin user already exists, skipping creation");
        }
    }
}
