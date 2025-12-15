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
        try
        {
            _logger.LogInformation("Starting Identity user seeding...");
            
            // Create Administrator role if it doesn't exist
            var adminRoleName = "Administrator";
            if (!await _roleManager.RoleExistsAsync(adminRoleName))
            {
                _logger.LogInformation("Creating Administrator role in AspNetRoles table");
                var adminRole = new IdentityRole<int> { Name = adminRoleName };
                var roleResult = await _roleManager.CreateAsync(adminRole);
                
                if (roleResult.Succeeded)
                {
                    _logger.LogInformation("Administrator role created successfully");
                }
                else
                {
                    _logger.LogError("Failed to create Administrator role: {Errors}", 
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    throw new InvalidOperationException($"Failed to create Administrator role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                _logger.LogInformation("Administrator role already exists in AspNetRoles table");
            }

            // Check if admin user already exists
            var adminUsername = "admin";
            var existingAdmin = await _userManager.FindByNameAsync(adminUsername);
            
            if (existingAdmin == null)
            {
                _logger.LogInformation("Creating default admin user in AspNetUsers table");
                
                // Get default admin password from configuration or use default
                var defaultPassword = _configuration.GetValue<string>("DefaultAdminPassword") ?? "Admin@123";
                _logger.LogInformation("Using configured admin password (DefaultAdminPassword from config)");

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
                // UserManager will properly hash the password using Identity's password hasher
                var result = await _userManager.CreateAsync(adminUser, defaultPassword);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("Admin user created successfully in AspNetUsers table with UserId: {UserId}", adminUser.Id);
                    
                    // Assign Administrator role
                    var roleAssignResult = await _userManager.AddToRoleAsync(adminUser, adminRoleName);
                    if (roleAssignResult.Succeeded)
                    {
                        _logger.LogInformation("Administrator role assigned to admin user successfully");
                        
                        // Verify the user can be found and has the role
                        var verifyUser = await _userManager.FindByNameAsync(adminUsername);
                        if (verifyUser != null)
                        {
                            var roles = await _userManager.GetRolesAsync(verifyUser);
                            _logger.LogInformation("Verification: Admin user exists with roles: {Roles}", string.Join(", ", roles));
                        }
                    }
                    else
                    {
                        _logger.LogError("Failed to assign Administrator role to admin user: {Errors}", 
                            string.Join(", ", roleAssignResult.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    _logger.LogError("Failed to create admin user in AspNetUsers table: {Errors}", 
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    
                    // Log detailed error information
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError("Identity Error - Code: {Code}, Description: {Description}", 
                            error.Code, error.Description);
                    }
                    
                    throw new InvalidOperationException($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                _logger.LogInformation("Admin user already exists in AspNetUsers table with UserId: {UserId}", existingAdmin.Id);
                
                // Verify the user has the Administrator role
                var roles = await _userManager.GetRolesAsync(existingAdmin);
                if (!roles.Contains(adminRoleName))
                {
                    _logger.LogWarning("Admin user exists but doesn't have Administrator role. Adding role...");
                    var roleAssignResult = await _userManager.AddToRoleAsync(existingAdmin, adminRoleName);
                    if (roleAssignResult.Succeeded)
                    {
                        _logger.LogInformation("Administrator role added to existing admin user");
                    }
                    else
                    {
                        _logger.LogError("Failed to add Administrator role to existing admin user: {Errors}", 
                            string.Join(", ", roleAssignResult.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    _logger.LogInformation("Admin user already has Administrator role: {Roles}", string.Join(", ", roles));
                }
            }
            
            _logger.LogInformation("Identity user seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Identity user seeding: {Message}", ex.Message);
            throw;
        }
    }
}
