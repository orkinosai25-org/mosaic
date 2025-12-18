using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OrkinosaiCMS.Infrastructure.Data;

namespace OrkinosaiCMS.Infrastructure.Services;

/// <summary>
/// Custom health check that validates database state including migrations and critical tables
/// This health check ensures the application does not report as "healthy" when database is not properly initialized
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    // SQL Server error codes
    private const int SQL_ERROR_INVALID_OBJECT_NAME = 208;  // Table doesn't exist
    
    // SQLite error codes
    private const int SQLITE_ERROR = 1;  // Generic SQLite error (often indicates table doesn't exist)

    public DatabaseHealthCheck(
        ApplicationDbContext context,
        ILogger<DatabaseHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if database is accessible
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
            {
                _logger.LogError("Database health check FAILED: Cannot connect to database");
                return HealthCheckResult.Unhealthy(
                    "Database is not accessible. Check connection string and ensure database server is running.",
                    data: new Dictionary<string, object>
                    {
                        { "CanConnect", false },
                        { "Reason", "Database connection failed" }
                    });
            }

            // Validate critical Identity tables exist
            var identityTablesExist = await ValidateIdentityTablesAsync(cancellationToken);
            if (!identityTablesExist)
            {
                _logger.LogError("Database health check FAILED: Identity tables (AspNetUsers) do not exist");
                return HealthCheckResult.Unhealthy(
                    "Database migrations have not been applied. AspNetUsers table is missing. Admin login will not work.",
                    data: new Dictionary<string, object>
                    {
                        { "CanConnect", true },
                        { "IdentityTablesExist", false },
                        { "Reason", "Database migrations not applied" },
                        { "RequiredAction", "Run: dotnet ef database update --startup-project src/OrkinosaiCMS.Web" }
                    });
            }

            // Validate core CMS tables exist
            var coreTablesExist = await ValidateCoreTablesAsync(cancellationToken);
            if (!coreTablesExist)
            {
                _logger.LogError("Database health check FAILED: Core CMS tables (Sites, Pages) do not exist");
                return HealthCheckResult.Degraded(
                    "Core CMS tables are missing. Database may not be fully initialized.",
                    data: new Dictionary<string, object>
                    {
                        { "CanConnect", true },
                        { "IdentityTablesExist", true },
                        { "CoreTablesExist", false },
                        { "Reason", "Core tables missing" }
                    });
            }

            // All checks passed
            _logger.LogInformation("Database health check PASSED: All critical tables exist and database is accessible");
            return HealthCheckResult.Healthy(
                "Database is healthy and all critical tables exist",
                data: new Dictionary<string, object>
                {
                    { "CanConnect", true },
                    { "IdentityTablesExist", true },
                    { "CoreTablesExist", true }
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check FAILED with exception: {Message}", ex.Message);
            return HealthCheckResult.Unhealthy(
                $"Database health check failed: {ex.Message}",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "Exception", ex.GetType().Name },
                    { "Message", ex.Message }
                });
        }
    }

    private async Task<bool> ValidateIdentityTablesAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Try to query AspNetUsers table - this will fail if migrations haven't been applied
            var userCount = await _context.Users.CountAsync(cancellationToken);
            _logger.LogDebug("Identity tables validation: AspNetUsers exists with {Count} users", userCount);
            return true;
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == SQL_ERROR_INVALID_OBJECT_NAME)
        {
            // SQL Server: SQL error 208 = Invalid object name (table doesn't exist)
            _logger.LogWarning("Identity tables validation failed: AspNetUsers table does not exist (SQL Server Error {ErrorNumber})", 
                SQL_ERROR_INVALID_OBJECT_NAME);
            return false;
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx) 
            when (dbEx.InnerException is Microsoft.Data.Sqlite.SqliteException sqliteEx 
                  && sqliteEx.SqliteErrorCode == SQLITE_ERROR)
        {
            // SQLite: Error code 1 (SQLITE_ERROR) with "no such table" message
            _logger.LogWarning("Identity tables validation failed: AspNetUsers table does not exist (SQLite Error {ErrorCode})", 
                SQLITE_ERROR);
            return false;
        }
        catch (Exception ex)
        {
            // Generic exception handling for other database providers or unexpected errors
            // Check if the error message indicates a missing table
            if (ex.Message.Contains("AspNetUsers", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("no such table", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(ex, "Identity tables validation failed: AspNetUsers table likely does not exist");
                return false;
            }
            
            _logger.LogWarning(ex, "Identity tables validation failed with unexpected error: {Message}", ex.Message);
            return false;
        }
    }

    private async Task<bool> ValidateCoreTablesAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Try to query Sites table
            var siteCount = await _context.Sites.CountAsync(cancellationToken);
            _logger.LogDebug("Core tables validation: Sites exists with {Count} sites", siteCount);
            return true;
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Number == SQL_ERROR_INVALID_OBJECT_NAME)
        {
            // SQL Server: Invalid object name (table doesn't exist)
            _logger.LogWarning("Core tables validation failed: Sites table does not exist (SQL Server Error {ErrorNumber})", 
                SQL_ERROR_INVALID_OBJECT_NAME);
            return false;
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx) 
            when (dbEx.InnerException is Microsoft.Data.Sqlite.SqliteException sqliteEx 
                  && sqliteEx.SqliteErrorCode == SQLITE_ERROR)
        {
            // SQLite: Error code 1 (SQLITE_ERROR) with "no such table" message
            _logger.LogWarning("Core tables validation failed: Sites table does not exist (SQLite Error {ErrorCode})", 
                SQLITE_ERROR);
            return false;
        }
        catch (Exception ex)
        {
            // Generic exception handling for other database providers
            if (ex.Message.Contains("Sites", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("no such table", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(ex, "Core tables validation failed: Sites table likely does not exist");
                return false;
            }
            
            _logger.LogWarning(ex, "Core tables validation failed with unexpected error: {Message}", ex.Message);
            return false;
        }
    }
}
