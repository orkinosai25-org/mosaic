using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrkinosaiCMS.Infrastructure.Data;

namespace OrkinosaiCMS.Infrastructure.Services;

/// <summary>
/// Validates database state on startup and provides actionable error messages
/// if required tables are missing (especially AspNetUsers for admin login)
/// </summary>
public class StartupDatabaseValidator
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StartupDatabaseValidator> _logger;

    public StartupDatabaseValidator(
        ApplicationDbContext context,
        ILogger<StartupDatabaseValidator> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Validate that all critical tables exist for application startup
    /// Provides clear error messages if tables are missing
    /// </summary>
    public async Task<ValidationResult> ValidateDatabaseAsync()
    {
        var result = new ValidationResult { IsValid = true };

        try
        {
            _logger.LogInformation("=== Starting Database Validation ===");

            // Check if database is accessible
            var canConnect = await CanConnectToDatabaseAsync();
            if (!canConnect)
            {
                result.IsValid = false;
                result.ErrorMessage = "Cannot connect to database. Please check connection string and ensure database server is accessible.";
                result.ActionRequired = "Verify ConnectionStrings__DefaultConnection environment variable or appsettings.json";
                return result;
            }

            // Validate critical Identity tables
            var identityTablesResult = await ValidateIdentityTablesAsync();
            if (!identityTablesResult.IsValid)
            {
                return identityTablesResult;
            }

            // Validate core CMS tables
            var coreTablesResult = await ValidateCoreTablesAsync();
            if (!coreTablesResult.IsValid)
            {
                return coreTablesResult;
            }

            _logger.LogInformation("✓ Database validation successful - all critical tables exist");
            _logger.LogInformation("=== Database Validation Complete ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database validation failed with exception");
            result.IsValid = false;
            result.ErrorMessage = $"Database validation error: {ex.Message}";
            result.ActionRequired = "Check application logs for details";
        }

        return result;
    }

    /// <summary>
    /// Validate that Identity tables exist (required for admin login)
    /// </summary>
    private async Task<ValidationResult> ValidateIdentityTablesAsync()
    {
        var result = new ValidationResult { IsValid = true };

        try
        {
            _logger.LogInformation("Validating Identity tables (required for admin login)...");

            // Try to query AspNetUsers table
            var userCount = await _context.Users.CountAsync();
            _logger.LogInformation("✓ AspNetUsers table exists ({Count} users)", userCount);

            // Try to query AspNetRoles table
            var roleCount = await _context.Roles.CountAsync();
            _logger.LogInformation("✓ AspNetRoles table exists ({Count} roles)", roleCount);

            return result;
        }
        catch (SqlException sqlEx) when (sqlEx.Number == 208) // Invalid object name
        {
            _logger.LogError("CRITICAL: Identity tables do not exist! Admin login will fail.");
            _logger.LogError("SQL Error: {Message}", sqlEx.Message);

            result.IsValid = false;
            result.ErrorMessage = "Identity tables (AspNetUsers, AspNetRoles) do not exist. Admin login will not work.";
            result.ActionRequired = @"
REQUIRED ACTION: Apply database migrations to create Identity tables

Run one of these commands:

1. Using dotnet ef tool:
   cd src/OrkinosaiCMS.Infrastructure
   dotnet ef database update --startup-project ../OrkinosaiCMS.Web

2. Using migration script (recommended):
   bash scripts/apply-migrations.sh update

3. Manual SQL (if migration tools not available):
   - Generate SQL script: dotnet ef migrations script -o migrations.sql --startup-project ../OrkinosaiCMS.Web
   - Apply to database using SQL Server Management Studio or sqlcmd

After applying migrations, restart the application.

For detailed instructions, see: DEPLOYMENT_VERIFICATION_GUIDE.md
";

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Identity tables");
            result.IsValid = false;
            result.ErrorMessage = $"Error checking Identity tables: {ex.Message}";
            result.ActionRequired = "Check database connectivity and permissions";
            return result;
        }
    }

    /// <summary>
    /// Validate that core CMS tables exist
    /// </summary>
    private async Task<ValidationResult> ValidateCoreTablesAsync()
    {
        var result = new ValidationResult { IsValid = true };

        try
        {
            _logger.LogInformation("Validating core CMS tables...");

            // Check Sites table
            var siteCount = await _context.Sites.CountAsync();
            _logger.LogInformation("✓ Sites table exists ({Count} sites)", siteCount);

            // Check Pages table
            var pageCount = await _context.Pages.CountAsync();
            _logger.LogInformation("✓ Pages table exists ({Count} pages)", pageCount);

            // Check Themes table
            var themeCount = await _context.Themes.CountAsync();
            _logger.LogInformation("✓ Themes table exists ({Count} themes)", themeCount);

            return result;
        }
        catch (SqlException sqlEx) when (sqlEx.Number == 208)
        {
            _logger.LogError("Core CMS tables are missing");
            _logger.LogError("SQL Error: {Message}", sqlEx.Message);

            result.IsValid = false;
            result.ErrorMessage = "Core CMS tables (Sites, Pages, Themes) do not exist.";
            result.ActionRequired = "Apply database migrations using: dotnet ef database update";
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating core CMS tables");
            result.IsValid = false;
            result.ErrorMessage = $"Error checking core tables: {ex.Message}";
            return result;
        }
    }

    /// <summary>
    /// Check if database is accessible
    /// </summary>
    private async Task<bool> CanConnectToDatabaseAsync()
    {
        try
        {
            await _context.Database.CanConnectAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot connect to database");
            return false;
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string ActionRequired { get; set; } = string.Empty;
    }
}
