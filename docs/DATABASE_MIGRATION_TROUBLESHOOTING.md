# Database Migration Troubleshooting Guide

This guide helps resolve common database migration issues, especially schema drift and "object already exists" errors.

## Problem: "There is already an object named 'Modules' in the database"

### Root Cause
This error occurs when:
1. The database has tables from a previous schema (before Identity integration)
2. Migrations try to create tables that already exist
3. Schema drift between code and database state

### Diagnosis

Run the following query to check which tables exist:

```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
```

**Expected tables (after all migrations):**
- AspNetRoleClaims
- AspNetRoles
- AspNetUserClaims
- AspNetUserLogins
- AspNetUserRoles
- AspNetUsers
- AspNetUserTokens
- Contents
- Customers
- Invoices
- LegacyRoles (renamed from Roles)
- LegacyUserRoles (renamed from UserRoles)
- LegacyUsers (renamed from Users)
- MasterPages
- Modules
- PageModules
- Pages
- PaymentMethods
- Permissions
- RolePermissions
- Sites
- Subscriptions
- Themes

**Problem indicators:**
- ✗ `Roles` table exists (should be `LegacyRoles`)
- ✗ `Users` table exists (should be `LegacyUsers`)
- ✗ `AspNetUsers` table MISSING
- ✗ `AspNetRoles` table MISSING

### Solution Options

#### Option 1: Fresh Database (RECOMMENDED for Production)

**Best for:** Production deployments, Azure SQL Database, when no critical data exists

```bash
# 1. Backup existing database (if it has data)
# SQL Server:
BACKUP DATABASE MosaicCMS TO DISK = 'C:\Backups\MosaicCMS_PreMigration.bak'

# 2. Drop and recreate database
DROP DATABASE MosaicCMS;
CREATE DATABASE MosaicCMS;

# 3. Apply all migrations cleanly
cd src/OrkinosaiCMS.Infrastructure
ASPNETCORE_ENVIRONMENT=Production dotnet ef database update \
  --startup-project ../OrkinosaiCMS.Web
```

#### Option 2: Manual Schema Correction (RISKY)

**Best for:** Development databases with test data you want to keep

**WARNING:** This can cause data loss. Always backup first!

```sql
-- Step 1: Rename legacy tables to match new schema
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
    EXEC sp_rename 'Users', 'LegacyUsers';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
    EXEC sp_rename 'Roles', 'LegacyRoles';

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'UserRoles')
    EXEC sp_rename 'UserRoles', 'LegacyUserRoles';

-- Step 2: Rename columns in legacy tables
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LegacyUsers') AND name = 'SubscriptionTier')
    EXEC sp_rename 'LegacyUsers.SubscriptionTier', 'SubscriptionTierValue', 'COLUMN';

-- Step 3: Mark migrations as applied (if tables already match)
-- WARNING: Only do this if you're certain the schema matches the migration
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
VALUES 
    ('20251129175729_InitialCreate', '10.0.0'),
    ('20251209164111_AddThemeEnhancements', '10.0.0'),
    ('20251211225909_AddSubscriptionEntities', '10.0.0');

-- Step 4: Now apply remaining migrations
-- Run from command line:
-- dotnet ef database update
```

#### Option 3: Start Over with Application Auto-Migration

**Best for:** Development/testing environments

The application now includes enhanced schema drift detection that will:
1. Detect missing Identity tables
2. Provide clear error messages
3. Guide you to the correct resolution

Simply start the application and check the logs for guidance:

```bash
cd src/OrkinosaiCMS.Web
ASPNETCORE_ENVIRONMENT=Production dotnet run
```

Check logs for:
- "Schema drift detected" messages
- Table existence verification
- Migration status

## Problem: "Invalid object name 'AspNetUsers'" during login

### Root Cause
Identity tables (AspNetUsers, AspNetRoles) don't exist in the database.

### Diagnosis

```sql
-- Check if Identity tables exist
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME LIKE 'AspNet%'
ORDER BY TABLE_NAME;
```

**Expected output:**
- AspNetRoleClaims
- AspNetRoles
- AspNetUserClaims
- AspNetUserLogins
- AspNetUserRoles
- AspNetUsers
- AspNetUserTokens

If you see 0 results, Identity migrations haven't been applied.

### Solution

```bash
# Apply all migrations including Identity tables
cd src/OrkinosaiCMS.Infrastructure
ASPNETCORE_ENVIRONMENT=Production dotnet ef database update \
  --startup-project ../OrkinosaiCMS.Web
```

## Problem: "The antiforgery token could not be decrypted"

### Root Cause
1. Data protection keys changed/lost
2. Cookie encryption keys incompatible across deployments
3. Identity tables missing (can't validate tokens)

### Solution

```bash
# 1. Clear browser cookies
# 2. Ensure data protection keys directory exists
mkdir -p /home/site/wwwroot/App_Data/DataProtection-Keys

# 3. Restart application
# 4. Try login again
```

For Azure App Service, use persistent storage for keys:

```csharp
// In Program.cs (already configured):
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
    .SetApplicationName("OrkinosaiCMS");
```

## Problem: Migrations fail during application startup

### Symptoms
Application logs show:
```
SQL Server error during migration check/apply
SQL Error Number: 2714 (object already exists)
```

### Solution
The application now handles this automatically with enhanced error handling:

1. **Detection:** Checks if error is "object already exists" (error 2714)
2. **Recovery:** Attempts schema drift recovery
3. **Guidance:** Provides specific instructions in logs

Check application logs for:
```
=== Schema Drift Recovery Process ===
✓ Table 'Sites' exists
✓ Table 'Modules' exists  
✗ Table 'AspNetUsers' missing
```

Follow the instructions provided in the logs.

## Verification Checklist

After resolving migration issues:

- [ ] All expected tables exist (see list above)
- [ ] `AspNetUsers` and `AspNetRoles` tables exist
- [ ] `LegacyUsers` and `LegacyRoles` exist (renamed from Users/Roles)
- [ ] `__EFMigrationsHistory` table shows all 5 migrations applied
- [ ] Application starts without migration errors
- [ ] Admin login works (username: admin, password: Admin@123)
- [ ] Can create/edit content without database errors

## Manual Migration Commands

### Generate SQL Script (for review before applying)

```bash
cd src/OrkinosaiCMS.Infrastructure

# Generate complete migration script
ASPNETCORE_ENVIRONMENT=Production dotnet ef migrations script \
  --project . \
  --startup-project ../OrkinosaiCMS.Web \
  --output /tmp/migration-script.sql

# Review the script before applying
less /tmp/migration-script.sql
```

### Apply Migrations Step-by-Step

```bash
cd src/OrkinosaiCMS.Infrastructure

# Apply migrations one at a time for debugging
ASPNETCORE_ENVIRONMENT=Production dotnet ef database update 20251129175729_InitialCreate \
  --startup-project ../OrkinosaiCMS.Web

ASPNETCORE_ENVIRONMENT=Production dotnet ef database update 20251209164111_AddThemeEnhancements \
  --startup-project ../OrkinosaiCMS.Web

ASPNETCORE_ENVIRONMENT=Production dotnet ef database update 20251211225909_AddSubscriptionEntities \
  --startup-project ../OrkinosaiCMS.Web

ASPNETCORE_ENVIRONMENT=Production dotnet ef database update 20251215015307_AddIdentityTables \
  --startup-project ../OrkinosaiCMS.Web

ASPNETCORE_ENVIRONMENT=Production dotnet ef database update 20251215224415_SyncPendingModelChanges \
  --startup-project ../OrkinosaiCMS.Web
```

### Check Migration History

```sql
-- See which migrations have been applied
SELECT * FROM __EFMigrationsHistory ORDER BY MigrationId;
```

**Expected result:**
```
20251129175729_InitialCreate                      10.0.0
20251209164111_AddThemeEnhancements                10.0.0
20251211225909_AddSubscriptionEntities            10.0.0
20251215015307_AddIdentityTables                  10.0.0
20251215224415_SyncPendingModelChanges            10.0.0
```

## Azure SQL Database Specific Issues

### Connection Timeout During Migration

```bash
# Increase timeout in connection string
Server=yourserver.database.windows.net;
Database=MosaicCMS;
User Id=yourusername;
Password=yourpassword;
Connection Timeout=60;
Command Timeout=120;
```

### Firewall Rules

Ensure Azure SQL Server firewall allows connections from:
1. Your development machine IP
2. Azure App Service (if deployed)
3. GitHub Actions (for CI/CD)

### Service Tier Recommendations

For production databases with migrations:
- **Minimum:** S0 (10 DTUs) for small sites
- **Recommended:** S1 (20 DTUs) for production
- **Large sites:** S2+ or Premium tier

## Support and Additional Help

### Useful Diagnostic Queries

```sql
-- Check table structure
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AspNetUsers'
ORDER BY ORDINAL_POSITION;

-- Check for orphaned data
SELECT u.* 
FROM LegacyUsers u
WHERE NOT EXISTS (
    SELECT 1 FROM AspNetUsers au 
    WHERE au.UserName = u.Username
);

-- Verify database size
EXEC sp_spaceused;
```

### Logs to Check

1. **Application Logs:** `/home/site/wwwroot/App_Data/Logs/`
2. **Azure App Service Logs:** Log stream in Azure Portal
3. **EF Core Logs:** Look for `[INF]` and `[ERR]` prefixes

### Getting Help

If none of these solutions work:

1. Export database schema: `EXEC sp_help`
2. Check `__EFMigrationsHistory` table
3. Review application startup logs
4. Create GitHub issue with:
   - Database provider (SQL Server/SQLite)
   - Migration history
   - Error messages
   - Table list

---

**Last Updated:** 2025-12-16  
**Author:** GitHub Copilot SWE Agent  
**Related Docs:** 
- [Migration Verification Guide](MIGRATION_VERIFICATION_GUIDE.md)
- [Setup Guide](SETUP.md)
- [Architecture](ARCHITECTURE.md)
