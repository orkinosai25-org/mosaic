# Entity Framework Core Migrations

This directory contains all EF Core migrations for the OrkinosaiCMS ApplicationDbContext.

## Migration History

### 20251129175729_InitialCreate
**Created:** November 29, 2025  
**Purpose:** Initial database schema creation with core CMS entities.

**Tables Created:**
- Sites, Pages, MasterPages
- Modules, PageModules
- Themes
- Users, Roles, Permissions (legacy)
- UserRoles, RolePermissions
- Contents

### 20251209164111_AddThemeEnhancements
**Created:** December 9, 2025  
**Purpose:** Enhanced theme system with additional theme templates and color customization.

**Changes:**
- Added theme categories (Modern, SharePoint, Business, etc.)
- Added color customization fields (PrimaryColor, SecondaryColor, AccentColor)
- Added multiple theme templates
- Added SharePoint-like theme support

### 20251211225909_AddSubscriptionEntities
**Created:** December 11, 2025  
**Purpose:** Added subscription and payment management for SaaS multi-tenancy.

**Tables Created:**
- Customers
- Subscriptions
- Invoices
- PaymentMethods

**Purpose:**
Enables subscription-based SaaS platform with Stripe integration for payment processing.

### 20251215015307_AddIdentityTables
**Created:** December 15, 2025  
**Purpose:** Integrated ASP.NET Core Identity for authentication and authorization.

**Tables Created:**
- AspNetUsers
- AspNetRoles
- AspNetUserRoles
- AspNetUserClaims
- AspNetRoleClaims
- AspNetUserLogins
- AspNetUserTokens

**Purpose:**
Replaced custom authentication with ASP.NET Core Identity following Oqtane's approach for battle-tested security.

**Breaking Changes:**
- Legacy Users/Roles tables remain for backward compatibility
- New authentication uses Identity tables (AspNetUsers, AspNetRoles)
- Password hashing migrated from BCrypt to Identity's PasswordHasher

### 20251215224415_SyncPendingModelChanges
**Created:** December 15, 2025  
**Purpose:** Synchronized pending model changes and standardized SQL Server data types.

**Changes:**
- Converted SQLite TEXT types to SQL Server nvarchar types
- Updated Identity table indexes (UserNameIndex, RoleNameIndex)
- Standardized datetime types from TEXT to datetime2
- Ensured all model changes are captured in migrations

**Important:**
This migration was necessary to resolve the "pending changes" error that occurred when the model diverged from the migration snapshot.

## Applying Migrations

### Development (LocalDB - Windows)
```bash
cd src/OrkinosaiCMS.Infrastructure
dotnet ef database update --startup-project ../OrkinosaiCMS.Web
```

### Development (Docker SQL Server)
```bash
# Start SQL Server
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Password" \
  -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest

# Apply migrations
cd src/OrkinosaiCMS.Infrastructure
ConnectionStrings__DefaultConnection="Server=localhost;Database=MosaicCMS;User Id=sa;Password=YourStrong@Password;TrustServerCertificate=True" \
dotnet ef database update --startup-project ../OrkinosaiCMS.Web
```

### Production (Azure SQL Database)
Migrations are applied automatically on application startup via `SeedData.InitializeAsync()`. 

For manual control in production:
```bash
cd src/OrkinosaiCMS.Infrastructure
ConnectionStrings__DefaultConnection=$AZURE_SQL_CONNECTION_STRING \
ASPNETCORE_ENVIRONMENT=Production \
dotnet ef database update --startup-project ../OrkinosaiCMS.Web
```

## Creating New Migrations

When you make changes to entity models, create a new migration:

```bash
cd src/OrkinosaiCMS.Infrastructure

# Use Production environment to avoid InMemory database
ASPNETCORE_ENVIRONMENT=Production dotnet ef migrations add YourMigrationName \
  --project . \
  --startup-project ../OrkinosaiCMS.Web
```

**Migration Naming Convention:**
- Use descriptive names: `AddUserEmailIndex`, `UpdateThemeSchema`
- Avoid generic names: `Migration1`, `Update`, `Changes`
- Use PascalCase

## Checking for Pending Changes

Before creating a migration, check if there are pending model changes:

```bash
cd src/OrkinosaiCMS.Infrastructure

ASPNETCORE_ENVIRONMENT=Production dotnet ef migrations has-pending-model-changes \
  --project . \
  --startup-project ../OrkinosaiCMS.Web
```

## Generating SQL Scripts

To review SQL before applying migrations:

```bash
cd src/OrkinosaiCMS.Infrastructure

# Generate complete script
ASPNETCORE_ENVIRONMENT=Production dotnet ef migrations script \
  --output migration-script.sql \
  --startup-project ../OrkinosaiCMS.Web

# Generate script for specific migration range
ASPNETCORE_ENVIRONMENT=Production dotnet ef migrations script \
  20251129175729_InitialCreate 20251215224415_SyncPendingModelChanges \
  --output incremental-script.sql \
  --startup-project ../OrkinosaiCMS.Web
```

## Rolling Back Migrations

To rollback to a specific migration:

```bash
cd src/OrkinosaiCMS.Infrastructure

# Rollback to specific migration
ASPNETCORE_ENVIRONMENT=Production dotnet ef database update 20251129175729_InitialCreate \
  --startup-project ../OrkinosaiCMS.Web

# Rollback all migrations (drops all tables)
ASPNETCORE_ENVIRONMENT=Production dotnet ef database update 0 \
  --startup-project ../OrkinosaiCMS.Web
```

⚠️ **Warning:** Rolling back migrations in production can result in data loss. Always backup your database first.

## Removing Migrations

To remove the last migration that hasn't been applied to the database:

```bash
cd src/OrkinosaiCMS.Infrastructure

ASPNETCORE_ENVIRONMENT=Production dotnet ef migrations remove \
  --startup-project ../OrkinosaiCMS.Web
```

⚠️ **Warning:** Never remove migrations that have been applied to production databases.

## Migration Files

Each migration consists of three files:

1. **{timestamp}_{name}.cs** - Contains Up() and Down() methods with migration logic
2. **{timestamp}_{name}.Designer.cs** - Metadata about the migration
3. **ApplicationDbContextModelSnapshot.cs** - Current state of the entire model (updated with each migration)

## Testing Migrations

Before applying migrations to production:

1. Test in development environment
2. Review generated SQL script
3. Test on a copy of production data
4. Backup production database
5. Apply during maintenance window with rollback plan

## Database Providers

Migrations support multiple database providers:

- **SQL Server** (Production default)
- **SQLite** (Local development option)
- **InMemory** (Testing only)

The provider is configured in `appsettings.json`:
```json
{
  "DatabaseProvider": "SqlServer"
}
```

## Common Issues

### "Changes have been made to the model since the last migration"

**Solution:** Create a new migration to capture the changes.

### "Invalid object name 'TableName'"

**Cause:** Migrations not applied to database.  
**Solution:** Run `dotnet ef database update`

### "Relational-specific methods can only be used when the context is using a relational database provider"

**Cause:** Using InMemory database provider with EF tools.  
**Solution:** Use `ASPNETCORE_ENVIRONMENT=Production` to force SQL Server provider.

## Resources

- [EF Core Migrations Documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Migration Verification Guide](../../docs/MIGRATION_VERIFICATION_GUIDE.md)
- [Database Architecture](../../docs/DATABASE.md)

---

**Last Updated:** 2025-12-15  
**Migration Count:** 5  
**Database Schema Version:** 20251215224415_SyncPendingModelChanges
