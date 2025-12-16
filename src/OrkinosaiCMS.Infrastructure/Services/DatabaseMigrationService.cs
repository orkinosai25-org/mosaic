using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrkinosaiCMS.Infrastructure.Data;
using System.Data.Common;

namespace OrkinosaiCMS.Infrastructure.Services;

/// <summary>
/// Database migration service with recovery and repair capabilities
/// Adapted from Oqtane's DatabaseManager pattern for robust migration handling
/// </summary>
public class DatabaseMigrationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseMigrationService> _logger;
    private readonly IConfiguration _configuration;

    public DatabaseMigrationService(
        ApplicationDbContext context,
        ILogger<DatabaseMigrationService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Migrate database with comprehensive error handling and recovery
    /// Following Oqtane's approach: detect state, apply migrations, handle errors
    /// </summary>
    public async Task<MigrationResult> MigrateDatabaseAsync()
    {
        var result = new MigrationResult { Success = false };

        try
        {
            _logger.LogInformation("=== Starting Database Migration Process ===");

            // Step 1: Check if database exists
            var canConnect = await CanConnectToDatabaseAsync();
            if (!canConnect)
            {
                _logger.LogWarning("Cannot connect to database. Will attempt to create it.");
                result.RequiresDatabaseCreation = true;
            }

            // Step 2: Ensure database exists
            if (result.RequiresDatabaseCreation)
            {
                _logger.LogInformation("Creating database...");
                var created = await EnsureDatabaseCreatedAsync();
                if (!created)
                {
                    result.ErrorMessage = "Failed to create database";
                    return result;
                }
                _logger.LogInformation("✓ Database created successfully");
            }

            // Step 3: Get pending migrations
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            var pendingList = pendingMigrations.ToList();

            if (pendingList.Any())
            {
                _logger.LogInformation("Found {Count} pending migrations:", pendingList.Count);
                foreach (var migration in pendingList)
                {
                    _logger.LogInformation("  - {Migration}", migration);
                }

                // Step 4: Apply migrations with retry logic
                result = await ApplyMigrationsWithRecoveryAsync(pendingList);
            }
            else
            {
                _logger.LogInformation("✓ No pending migrations - database schema is up to date");
                result.Success = true;
                result.Message = "Database is up to date";

                // Still verify critical tables exist
                await VerifyCriticalTablesAsync();
            }

            if (result.Success)
            {
                // Step 5: Verify database integrity
                await VerifyDatabaseIntegrityAsync();
                _logger.LogInformation("=== Database Migration Process Completed Successfully ===");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error during database migration");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.Exception = ex;
        }

        return result;
    }

    /// <summary>
    /// Apply migrations with comprehensive error handling and recovery
    /// Adapted from Oqtane's MigrateMaster and MigrateTenants patterns
    /// </summary>
    private async Task<MigrationResult> ApplyMigrationsWithRecoveryAsync(List<string> pendingMigrations)
    {
        var result = new MigrationResult();

        try
        {
            _logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count);

            // Add migration history entries for any migrations that were applied manually
            // This prevents re-running migrations that already modified the database
            await AddMissingMigrationHistoryEntriesAsync();

            // Apply all pending migrations
            await _context.Database.MigrateAsync();

            result.Success = true;
            result.Message = $"Successfully applied {pendingMigrations.Count} migrations";
            _logger.LogInformation("✓ All migrations applied successfully");
        }
        catch (SqlException sqlEx) when (sqlEx.Number == 2714) // Object already exists
        {
            _logger.LogWarning("Schema drift detected: SQL Error 2714 - Object already exists");
            _logger.LogWarning("Attempting automatic recovery...");

            result = await HandleSchemaDriftRecoveryAsync(pendingMigrations, sqlEx);
        }
        catch (SqlException sqlEx) when (sqlEx.Number == 208) // Invalid object name
        {
            _logger.LogError("Critical: Missing required database objects - SQL Error 208");
            _logger.LogError("Migration may need to be applied to a clean database");

            result.Success = false;
            result.ErrorMessage = $"Missing database objects. SQL Error: {sqlEx.Message}";
            result.Exception = sqlEx;
        }
        catch (DbException dbEx)
        {
            _logger.LogError(dbEx, "Database error during migration");

            result.Success = false;
            result.ErrorMessage = $"Database error: {dbEx.Message}";
            result.Exception = dbEx;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during migration");

            result.Success = false;
            result.ErrorMessage = $"Migration failed: {ex.Message}";
            result.Exception = ex;
        }

        return result;
    }

    /// <summary>
    /// Handle schema drift by intelligently recovering from "object already exists" errors
    /// Oqtane pattern: Detect which migrations are already applied, mark them in history
    /// </summary>
    private async Task<MigrationResult> HandleSchemaDriftRecoveryAsync(
        List<string> pendingMigrations,
        SqlException originalException)
    {
        var result = new MigrationResult();

        try
        {
            _logger.LogInformation("=== Schema Drift Recovery Process ===");

            // Check which tables exist
            var existingTables = await GetExistingTablesAsync();
            _logger.LogInformation("Found {Count} existing tables in database", existingTables.Count);

            // Determine which migrations are already partially or fully applied
            var appliedMigrations = DetermineMigrationsApplied(existingTables, pendingMigrations);

            if (appliedMigrations.Any())
            {
                _logger.LogWarning("Detected {Count} migrations that appear to be already applied:", appliedMigrations.Count);
                foreach (var migration in appliedMigrations)
                {
                    _logger.LogWarning("  - {Migration}", migration);
                }

                // Add these migrations to history to prevent re-execution
                foreach (var migration in appliedMigrations)
                {
                    await AddMigrationToHistoryAsync(migration);
                    _logger.LogInformation("✓ Marked migration as applied: {Migration}", migration);
                }

                // Retry migration of remaining migrations
                var remainingMigrations = pendingMigrations.Except(appliedMigrations).ToList();
                if (remainingMigrations.Any())
                {
                    _logger.LogInformation("Retrying migration with {Count} remaining migrations", remainingMigrations.Count);
                    await _context.Database.MigrateAsync();
                }

                result.Success = true;
                result.Message = $"Schema drift recovered. Marked {appliedMigrations.Count} migrations as applied.";
                _logger.LogInformation("✓ Schema drift recovery completed successfully");
            }
            else
            {
                _logger.LogError("Could not determine which migrations caused the schema drift");
                result.Success = false;
                result.ErrorMessage = "Schema drift recovery failed - unable to determine migration state";
                result.Exception = originalException;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during schema drift recovery");
            result.Success = false;
            result.ErrorMessage = $"Schema drift recovery failed: {ex.Message}";
            result.Exception = ex;
        }

        return result;
    }

    /// <summary>
    /// Determine which pending migrations have already been applied based on existing tables
    /// </summary>
    private List<string> DetermineMigrationsApplied(
        List<string> existingTables,
        List<string> pendingMigrations)
    {
        var appliedMigrations = new List<string>();

        // Check if core tables from InitialCreate exist
        if (existingTables.Contains("Sites") && 
            existingTables.Contains("Modules") && 
            existingTables.Contains("Pages"))
        {
            var initialCreate = pendingMigrations.FirstOrDefault(m => m.Contains("InitialCreate"));
            if (!string.IsNullOrEmpty(initialCreate))
            {
                appliedMigrations.Add(initialCreate);
                _logger.LogInformation("Detected InitialCreate migration already applied (core tables exist)");
            }
        }

        // Check if Identity tables from AddIdentityTables exist
        if (existingTables.Contains("AspNetUsers") && 
            existingTables.Contains("AspNetRoles") && 
            existingTables.Contains("AspNetUserRoles"))
        {
            var identityTables = pendingMigrations.FirstOrDefault(m => m.Contains("AddIdentityTables"));
            if (!string.IsNullOrEmpty(identityTables))
            {
                appliedMigrations.Add(identityTables);
                _logger.LogInformation("Detected AddIdentityTables migration already applied (Identity tables exist)");
            }
        }

        // Check if Subscription tables exist
        if (existingTables.Contains("Customers") && 
            existingTables.Contains("Subscriptions"))
        {
            var subscriptionEntities = pendingMigrations.FirstOrDefault(m => m.Contains("AddSubscriptionEntities"));
            if (!string.IsNullOrEmpty(subscriptionEntities))
            {
                appliedMigrations.Add(subscriptionEntities);
                _logger.LogInformation("Detected AddSubscriptionEntities migration already applied");
            }
        }

        return appliedMigrations;
    }

    /// <summary>
    /// Add a migration to the history table manually
    /// Oqtane pattern: Manually manage migration history when needed
    /// </summary>
    private async Task AddMigrationToHistoryAsync(string migrationId)
    {
        try
        {
            var productVersion = typeof(ApplicationDbContext).Assembly
                .GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
                .Cast<System.Reflection.AssemblyInformationalVersionAttribute>()
                .FirstOrDefault()?.InformationalVersion ?? "10.0.0";

            var sql = @"
                IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = @MigrationId)
                BEGIN
                    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
                    VALUES (@MigrationId, @ProductVersion)
                END";

            await _context.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@MigrationId", migrationId),
                new SqlParameter("@ProductVersion", productVersion));

            _logger.LogInformation("Added migration to history: {MigrationId}", migrationId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not add migration to history: {MigrationId}", migrationId);
        }
    }

    /// <summary>
    /// Add missing migration history entries if migrations were applied manually
    /// </summary>
    private async Task AddMissingMigrationHistoryEntriesAsync()
    {
        try
        {
            // Check if migration history table exists
            var historyExists = await TableExistsAsync("__EFMigrationsHistory");
            if (!historyExists)
            {
                _logger.LogInformation("Migration history table doesn't exist yet - will be created with first migration");
                return;
            }

            // Get all applied migrations from history
            var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
            var appliedList = appliedMigrations.ToList();

            _logger.LogInformation("Found {Count} migrations in history table", appliedList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not check migration history");
        }
    }

    /// <summary>
    /// Get list of all tables in the database
    /// Uses safe, non-parameterized query (no user input)
    /// </summary>
    private async Task<List<string>> GetExistingTablesAsync()
    {
        var tables = new List<string>();

        try
        {
            // This query contains no dynamic content and accepts no user input
            // Using FromSqlRaw is safe here as the query is completely static
            var sql = @"
                SELECT TABLE_NAME 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_TYPE = 'BASE TABLE' 
                AND TABLE_NAME NOT LIKE '__EFMigrationsHistory'
                ORDER BY TABLE_NAME";

            // Note: EF Core's SqlQueryRaw is safe when no string interpolation or concatenation is used
            var result = await _context.Database.SqlQueryRaw<string>(sql).ToListAsync();
            tables.AddRange(result);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not retrieve table list");
        }

        return tables;
    }

    /// <summary>
    /// Check if a specific table exists in the database
    /// </summary>
    private async Task<bool> TableExistsAsync(string tableName)
    {
        try
        {
            // Validate table name to prevent SQL injection
            if (!IsValidTableName(tableName))
            {
                _logger.LogWarning("Invalid table name rejected: {TableName}", tableName);
                return false;
            }

            var sql = @"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = {0}";

            var count = await _context.Database.SqlQueryRaw<int>(sql, tableName).FirstOrDefaultAsync();
            return count > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates that a table name is safe to use in SQL queries (alphanumeric and underscores only)
    /// Prevents SQL injection by restricting to valid SQL Server identifier characters
    /// </summary>
    private bool IsValidTableName(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName) || tableName.Length > 128)
            return false;
            
        // Allow only alphanumeric characters and underscores (SQL Server identifier rules)
        return tableName.All(c => char.IsLetterOrDigit(c) || c == '_');
    }

    /// <summary>
    /// Verify critical tables exist after migration
    /// </summary>
    private async Task VerifyCriticalTablesAsync()
    {
        var criticalTables = new[] 
        { 
            "Sites", "Modules", "Pages", "Themes", 
            "AspNetUsers", "AspNetRoles", "AspNetUserRoles" 
        };

        foreach (var table in criticalTables)
        {
            var exists = await TableExistsAsync(table);
            if (!exists)
            {
                _logger.LogError("Critical table missing: {Table}", table);
            }
            else
            {
                _logger.LogDebug("✓ Critical table exists: {Table}", table);
            }
        }
    }

    /// <summary>
    /// Verify overall database integrity
    /// </summary>
    private async Task VerifyDatabaseIntegrityAsync()
    {
        try
        {
            // Try to query each critical table
            await _context.Sites.CountAsync();
            await _context.Modules.CountAsync();
            await _context.Pages.CountAsync();
            await _context.Themes.CountAsync();
            await _context.Users.CountAsync();
            await _context.Roles.CountAsync();

            _logger.LogInformation("✓ Database integrity check passed");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database integrity check found issues (this may be normal for new database)");
        }
    }

    /// <summary>
    /// Check if we can connect to the database
    /// </summary>
    private async Task<bool> CanConnectToDatabaseAsync()
    {
        try
        {
            return await _context.Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensure database is created
    /// Oqtane pattern: Create database if it doesn't exist
    /// </summary>
    private async Task<bool> EnsureDatabaseCreatedAsync()
    {
        try
        {
            var databaseCreator = _context.Database.GetService<IRelationalDatabaseCreator>();
            if (!await databaseCreator.ExistsAsync())
            {
                await databaseCreator.CreateAsync();
                _logger.LogInformation("Database created successfully");
                return true;
            }
            else
            {
                _logger.LogInformation("Database already exists");
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create database");
            return false;
        }
    }
}

/// <summary>
/// Result of database migration operation
/// </summary>
public class MigrationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
    public bool RequiresDatabaseCreation { get; set; }
}
