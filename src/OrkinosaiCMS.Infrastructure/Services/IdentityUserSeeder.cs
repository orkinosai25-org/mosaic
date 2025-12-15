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
            _logger.LogInformation("=== Starting Identity User Seeding ===");
            
            // Create Administrator role if it doesn't exist
            var adminRoleName = "Administrator";
            
            _logger.LogInformation("Checking if Administrator role exists in AspNetRoles table...");
            bool roleExists = false;
            try
            {
                roleExists = await _roleManager.RoleExistsAsync(adminRoleName);
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 208)
            {
                _logger.LogError("AspNetRoles table does not exist! SQL Error: {Message}", sqlEx.Message);
                _logger.LogError("This indicates that migrations have NOT been applied to the database.");
                _logger.LogError("Please ensure all EF Core migrations are applied before seeding data.");
                throw new InvalidOperationException(
                    "AspNetRoles table does not exist. Please apply all database migrations first using 'dotnet ef database update'.", 
                    sqlEx);
            }
            
            if (!roleExists)
            {
                _logger.LogInformation("Creating Administrator role in AspNetRoles table...");
                var adminRole = new IdentityRole<int> { Name = adminRoleName };
                var roleResult = await _roleManager.CreateAsync(adminRole);
                
                if (roleResult.Succeeded)
                {
                    _logger.LogInformation("✓ Administrator role created successfully with Id: {RoleId}", adminRole.Id);
                }
                else
                {
                    _logger.LogError("Failed to create Administrator role:");
                    foreach (var error in roleResult.Errors)
                    {
                        _logger.LogError("  - {Code}: {Description}", error.Code, error.Description);
                    }
                    throw new InvalidOperationException($"Failed to create Administrator role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                _logger.LogInformation("✓ Administrator role already exists in AspNetRoles table");
            }

            // Check if admin user already exists
            var adminUsername = "admin";
            _logger.LogInformation("Checking if admin user '{Username}' exists in AspNetUsers table...", adminUsername);
            
            ApplicationUser? existingAdmin = null;
            try
            {
                existingAdmin = await _userManager.FindByNameAsync(adminUsername);
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == 208)
            {
                _logger.LogError("AspNetUsers table does not exist! SQL Error: {Message}", sqlEx.Message);
                _logger.LogError("This indicates that migrations have NOT been applied to the database.");
                throw new InvalidOperationException(
                    "AspNetUsers table does not exist. Please apply all database migrations first.", 
                    sqlEx);
            }
            
            if (existingAdmin == null)
            {
                _logger.LogInformation("Creating default admin user in AspNetUsers table...");
                
                // Get default admin password from configuration or use default
                var defaultPassword = _configuration.GetValue<string>("DefaultAdminPassword") ?? "Admin@123";
                _logger.LogInformation("Using admin password from configuration (DefaultAdminPassword)");
                _logger.LogInformation("Admin credentials: Username='{Username}', Email='admin@mosaicms.com'", adminUsername);

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
                    _logger.LogInformation("✓ Admin user created successfully in AspNetUsers table");
                    _logger.LogInformation("  UserId: {UserId}", adminUser.Id);
                    _logger.LogInformation("  Username: {Username}", adminUser.UserName);
                    _logger.LogInformation("  Email: {Email}", adminUser.Email);
                    
                    // Assign Administrator role
                    _logger.LogInformation("Assigning Administrator role to admin user...");
                    var roleAssignResult = await _userManager.AddToRoleAsync(adminUser, adminRoleName);
                    if (roleAssignResult.Succeeded)
                    {
                        _logger.LogInformation("✓ Administrator role assigned to admin user successfully");
                        
                        // Verify the user can be found and has the role
                        var verifyUser = await _userManager.FindByNameAsync(adminUsername);
                        if (verifyUser != null)
                        {
                            var roles = await _userManager.GetRolesAsync(verifyUser);
                            _logger.LogInformation("✓ Verification successful: Admin user has roles: {Roles}", string.Join(", ", roles));
                        }
                    }
                    else
                    {
                        _logger.LogError("Failed to assign Administrator role to admin user:");
                        foreach (var error in roleAssignResult.Errors)
                        {
                            _logger.LogError("  - {Code}: {Description}", error.Code, error.Description);
                        }
                    }
                }
                else
                {
                    _logger.LogError("Failed to create admin user in AspNetUsers table");
                    _logger.LogError("Identity validation errors:");
                    
                    // Log detailed error information
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError("  - {Code}: {Description}", error.Code, error.Description);
                    }
                    
                    throw new InvalidOperationException($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                _logger.LogInformation("✓ Admin user already exists in AspNetUsers table");
                _logger.LogInformation("  UserId: {UserId}", existingAdmin.Id);
                _logger.LogInformation("  Username: {Username}", existingAdmin.UserName);
                _logger.LogInformation("  Email: {Email}", existingAdmin.Email);
                
                // Verify the user has the Administrator role
                var roles = await _userManager.GetRolesAsync(existingAdmin);
                if (!roles.Contains(adminRoleName))
                {
                    _logger.LogWarning("Admin user exists but doesn't have Administrator role. Adding role...");
                    var roleAssignResult = await _userManager.AddToRoleAsync(existingAdmin, adminRoleName);
                    if (roleAssignResult.Succeeded)
                    {
                        _logger.LogInformation("✓ Administrator role added to existing admin user");
                    }
                    else
                    {
                        _logger.LogError("Failed to add Administrator role to existing admin user:");
                        foreach (var error in roleAssignResult.Errors)
                        {
                            _logger.LogError("  - {Code}: {Description}", error.Code, error.Description);
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("✓ Admin user already has Administrator role: {Roles}", string.Join(", ", roles));
                }
            }
            
            _logger.LogInformation("=== Identity User Seeding Completed Successfully ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CRITICAL ERROR during Identity user seeding:");
            _logger.LogError("  Exception Type: {Type}", ex.GetType().FullName);
            _logger.LogError("  Message: {Message}", ex.Message);
            if (ex.InnerException != null)
            {
                _logger.LogError("  Inner Exception: {InnerType} - {InnerMessage}", 
                    ex.InnerException.GetType().FullName, ex.InnerException.Message);
            }
            throw;
        }
    }
}
