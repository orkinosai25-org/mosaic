# Database Auto-Migration Configuration

## Overview

OrkinosaiCMS automatically applies Entity Framework Core migrations on application startup to ensure the database schema is up-to-date. This feature addresses the "Invalid object name 'AspNetUsers'" error (SQL Error 208) that occurs when Identity tables are missing from the database.

## Configuration

Auto-migration behavior is controlled by the `Database:AutoApplyMigrations` setting in `appsettings.json`:

```json
{
  "Database": {
    "AutoApplyMigrations": true
  }
}
```

### Options

- **`true` (default)**: Migrations are automatically applied on startup
  - Recommended for development and most production deployments
  - Ensures database schema is always up-to-date
  - Prevents "Invalid object name" errors
  
- **`false`**: Auto-migration is disabled
  - Migrations must be applied manually before starting the application
  - Recommended for:
    - Blue-green deployments
    - Environments where DBAs must approve all schema changes
    - Scenarios requiring migration rollback capability
    - Production environments with strict change control

## How It Works

### Startup Flow

1. **Application starts** and initializes services
2. **Database connection** is validated
3. **Pending migrations** are detected
   - Compares database schema against migration history
   - Lists all unapplied migrations
4. **Auto-apply decision**:
   - If `AutoApplyMigrations = true`: Migrations are applied automatically
   - If `AutoApplyMigrations = false`: Application logs error and refuses to start
5. **Validation**: AspNetUsers and other critical tables are verified
6. **Seed data**: Default admin user and initial data are created

### Migration Application

When auto-migration is enabled:

```
=== Starting Database Migration Process ===
Found 5 pending migrations:
  - 20251129175729_InitialCreate
  - 20251209164111_AddThemeEnhancements
  - 20251211225909_AddSubscriptionEntities
  - 20251215015307_AddIdentityTables
  - 20251215224415_SyncPendingModelChanges
Auto-apply migrations is ENABLED - applying 5 pending migrations...
Successfully applied 5 migrations
✓ Database migration completed successfully
```

### Error Handling

If migrations fail to apply, the application:

1. **Logs detailed error** including SQL error codes and messages
2. **References the root cause**: "Invalid object name 'AspNetUsers'" (SQL Error 208)
3. **Provides remediation steps**:
   - Check database connection
   - Verify database permissions
   - Apply migrations manually if needed
4. **Prevents application startup** to avoid runtime errors

Example error log:

```
=== CRITICAL: Database Migration Failed ===

Error: Insufficient permissions to create table 'AspNetUsers'

This means the AspNetUsers table and other Identity tables are MISSING.
Admin login WILL NOT WORK until migrations are applied successfully.

Common Causes:
  1. Database connection issues (network, firewall, credentials)
  2. Insufficient database permissions for user
  3. Database server not running or unreachable
  4. Migration conflicts or schema drift

REQUIRED ACTION:
  1. Verify database connection string in appsettings.json
  2. Ensure database server is running and accessible
  3. Check database user has sufficient permissions (CREATE TABLE, ALTER, etc.)
  4. Review the error details above for specific issues
  5. Once issues are resolved, restart the application

For manual migration application:
  dotnet ef database update --startup-project src/OrkinosaiCMS.Web
  OR
  bash scripts/apply-migrations.sh update

See DEPLOYMENT_VERIFICATION_GUIDE.md for detailed troubleshooting.
```

## Manual Migration Application

### When Auto-Migration is Disabled

If you set `AutoApplyMigrations = false`, you must apply migrations manually before starting the application.

#### Option 1: Using dotnet ef tool

```bash
cd src/OrkinosaiCMS.Infrastructure
dotnet ef database update --startup-project ../OrkinosaiCMS.Web
```

#### Option 2: Using migration script

```bash
bash scripts/apply-migrations.sh update
```

#### Option 3: Generate SQL script

For production deployments where you want to review changes:

```bash
cd src/OrkinosaiCMS.Infrastructure
dotnet ef migrations script --startup-project ../OrkinosaiCMS.Web --output /tmp/migrations.sql

# Review the SQL script
less /tmp/migrations.sql

# Apply manually to database
sqlcmd -S your-server -d your-database -i /tmp/migrations.sql
```

## Environment-Specific Configuration

### Development

```json
{
  "Database": {
    "AutoApplyMigrations": true
  }
}
```

- Auto-migration enabled for quick development cycles
- Schema changes applied immediately on restart

### Production (Auto-Migration Enabled)

```json
{
  "Database": {
    "AutoApplyMigrations": true
  }
}
```

- Recommended for most production scenarios
- Ensures database is always up-to-date
- Prevents deployment issues due to missing migrations

### Production (Manual Migration)

```json
{
  "Database": {
    "AutoApplyMigrations": false
  }
}
```

- For environments with strict change control
- Migrations applied as part of deployment pipeline
- Allows for rollback and testing before production

**Environment Variable Override:**

```bash
export Database__AutoApplyMigrations=true
# or
export Database__AutoApplyMigrations=false
```

## Troubleshooting

### Issue: "Auto-apply migrations is DISABLED"

**Symptoms:**
- Application fails to start
- Error message: "Migrations are pending but auto-apply is disabled"

**Solution:**
Either:
1. Enable auto-migration: Set `Database:AutoApplyMigrations = true`
2. Apply migrations manually: Run `dotnet ef database update`

### Issue: "Invalid object name 'AspNetUsers'" (SQL Error 208)

**Root Cause:** 
- Database exists but migrations haven't been applied
- AspNetUsers table and other Identity tables are missing

**Solution:**
1. Ensure `AutoApplyMigrations = true` in appsettings.json
2. Restart the application (migrations will be applied automatically)
3. OR apply migrations manually and set `AutoApplyMigrations = false`

### Issue: Migration application fails

**Common Causes:**
- Insufficient database permissions
- Network connectivity issues
- Database server not running
- Migration conflicts or schema drift

**Resolution:**
1. Check logs for specific error details
2. Verify database connection string
3. Ensure database user has CREATE TABLE, ALTER, and other DDL permissions
4. Review pending migrations: `dotnet ef migrations list`
5. Check for manual schema changes that conflict with migrations

## Migration Recovery

If migrations fail with "object already exists" errors (SQL Error 2714), the migration service includes automatic recovery:

1. **Detects existing tables** in the database
2. **Determines which migrations** have already been partially applied
3. **Marks them as applied** in migration history
4. **Retries remaining migrations**

Example recovery log:

```
Schema drift detected: SQL Error 2714 - Object already exists
Attempting automatic recovery...
Found 25 existing tables in database
Detected 3 migrations that appear to be already applied:
  - 20251129175729_InitialCreate
  - 20251209164111_AddThemeEnhancements
  - 20251215015307_AddIdentityTables
✓ Marked migration as applied: 20251129175729_InitialCreate
✓ Marked migration as applied: 20251209164111_AddThemeEnhancements
✓ Marked migration as applied: 20251215015307_AddIdentityTables
Retrying migration with 2 remaining migrations
✓ Schema drift recovery completed successfully
```

## See Also

- [DEPLOYMENT_VERIFICATION_GUIDE.md](DEPLOYMENT_VERIFICATION_GUIDE.md) - Deployment troubleshooting
- [DATABASE_MIGRATION_TROUBLESHOOTING.md](DATABASE_MIGRATION_TROUBLESHOOTING.md) - Migration issues
- [Migration README](../src/OrkinosaiCMS.Infrastructure/Migrations/README.md) - Creating migrations

## References

This feature addresses the issue described in:
- **Bug Report:** "Admin login fails with: 'SQL error during login for username: admin - Error: 208, Message: Invalid object name 'AspNetUsers'.'"
- **Root Cause:** Essential Identity table 'AspNetUsers' missing from database
- **Solution:** Automatic migration application on startup with clear error messages and recovery

Following the Oqtane CMS pattern for robust database migration handling.
