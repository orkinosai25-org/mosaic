# Database Migration Verification Guide

This guide provides step-by-step instructions to verify that Entity Framework Core migrations are properly created, up-to-date, and can be applied to a SQL Server database.

## Prerequisites

- .NET 10 SDK installed
- EF Core tools installed: `dotnet tool install --global dotnet-ef`
- Access to a SQL Server instance (LocalDB, SQL Server Express, Azure SQL, or Docker container)

## 1. Check for Pending Model Changes

To verify that all model changes have been captured in migrations:

```bash
cd src/OrkinosaiCMS.Infrastructure

# Use Production environment to avoid InMemory database
ASPNETCORE_ENVIRONMENT=Production dotnet ef migrations has-pending-model-changes \
  --project . \
  --startup-project ../OrkinosaiCMS.Web
```

**Expected Output:**
```
No changes have been made to the model since the last migration.
```

If there are pending changes, you'll see:
```
Changes have been made to the model since the last migration. Add a new migration.
```

## 2. List All Migrations

To see all migrations in chronological order:

```bash
cd src/OrkinosaiCMS.Infrastructure

ASPNETCORE_ENVIRONMENT=Production dotnet ef migrations list \
  --project . \
  --startup-project ../OrkinosaiCMS.Web
```

**Expected Output (as of 2025-12-15):**
```
20251129175729_InitialCreate
20251209164111_AddThemeEnhancements
20251211225909_AddSubscriptionEntities
20251215015307_AddIdentityTables
20251215224415_SyncPendingModelChanges
```

## 3. Generate SQL Script

To review the SQL that will be executed when migrations are applied:

```bash
cd src/OrkinosaiCMS.Infrastructure

ASPNETCORE_ENVIRONMENT=Production dotnet ef migrations script \
  --project . \
  --startup-project ../OrkinosaiCMS.Web \
  --output /tmp/migration-script.sql
```

This generates a complete SQL script showing all database changes. Review this file to understand what will happen to your database.

## 4. Apply Migrations to Database

### Option A: Using LocalDB (Windows only)

```bash
cd src/OrkinosaiCMS.Infrastructure

# Connection string is in appsettings.json
ASPNETCORE_ENVIRONMENT=Production dotnet ef database update \
  --project . \
  --startup-project ../OrkinosaiCMS.Web
```

### Option B: Using Custom Connection String

```bash
cd src/OrkinosaiCMS.Infrastructure

# Override connection string via environment variable
ConnectionStrings__DefaultConnection="Server=localhost;Database=MosaicCMS;User Id=sa;Password=YourPassword;TrustServerCertificate=True" \
ASPNETCORE_ENVIRONMENT=Production \
dotnet ef database update \
  --project . \
  --startup-project ../OrkinosaiCMS.Web
```

### Option C: Using Docker SQL Server

```bash
# 1. Start SQL Server in Docker
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Password123" \
  -p 1433:1433 --name mosaic-sql \
  -d mcr.microsoft.com/mssql/server:2022-latest

# 2. Wait a few seconds for SQL Server to start
sleep 10

# 3. Apply migrations
cd src/OrkinosaiCMS.Infrastructure

ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=MosaicCMS;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=True;MultipleActiveResultSets=True" \
ASPNETCORE_ENVIRONMENT=Production \
dotnet ef database update \
  --project . \
  --startup-project ../OrkinosaiCMS.Web
```

## 5. Verify Database Tables

After applying migrations, verify that all expected tables exist:

```bash
# Connect to SQL Server using your preferred tool (SSMS, Azure Data Studio, sqlcmd, etc.)
# Or use Docker exec if using Docker SQL Server:

docker exec -it mosaic-sql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Password123" -C \
  -Q "USE MosaicCMS; SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME;"
```

**Expected Tables:**
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
- MasterPages
- Modules
- PageModules
- Pages
- PaymentMethods
- Permissions
- RolePermissions
- Roles (Legacy)
- Sites
- Subscriptions
- Themes
- UserRoles (Legacy)
- Users (Legacy)

## 6. Verify Identity Tables Have Data

After running the application, verify that the Identity tables are seeded with admin user and role:

```sql
-- Check AspNetRoles
SELECT * FROM AspNetRoles;

-- Expected: At least one row with Name = 'Administrator'

-- Check AspNetUsers
SELECT Id, UserName, Email, DisplayName FROM AspNetUsers;

-- Expected: At least one row with UserName = 'admin'

-- Check AspNetUserRoles (junction table)
SELECT ur.*, u.UserName, r.Name as RoleName
FROM AspNetUserRoles ur
JOIN AspNetUsers u ON ur.UserId = u.Id
JOIN AspNetRoles r ON ur.RoleId = r.Id;

-- Expected: admin user assigned to Administrator role
```

## 7. Run Application and Verify Seeding

```bash
cd src/OrkinosaiCMS.Web

# Set environment to Production to use SQL Server (not InMemory)
# Override connection string if needed
ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=MosaicCMS;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=True;MultipleActiveResultSets=True" \
ASPNETCORE_ENVIRONMENT=Production \
dotnet run
```

**Expected Log Output:**
```
[INF] === Starting Database Initialization ===
[INF] Checking for pending database migrations...
[INF] Database connection status: Connected
[INF] ✓ No pending migrations - database schema is up to date
[INF] Checking if initial data seeding is required...
[INF] First run check: True (Sites table is empty)
[INF] Starting Identity user seeding...
[INF] === Starting Identity User Seeding ===
[INF] Checking if Administrator role exists in AspNetRoles table...
[INF] Creating Administrator role in AspNetRoles table...
[INF] ✓ Administrator role created successfully with Id: 1
[INF] Checking if admin user 'admin' exists in AspNetUsers table...
[INF] Creating default admin user in AspNetUsers table...
[INF] ✓ Admin user created successfully in AspNetUsers table
[INF]   UserId: 1
[INF]   Username: admin
[INF]   Email: admin@mosaicms.com
[INF] ✓ Administrator role assigned to admin user successfully
[INF] === Identity User Seeding Completed Successfully ===
```

## 8. Test Admin Login

Navigate to `http://localhost:5000/admin/login` and verify you can log in with:
- **Username**: admin
- **Email**: admin@mosaicms.com
- **Password**: Admin@123 (or value from `DefaultAdminPassword` in appsettings.json)

## Troubleshooting

### Error: "Changes have been made to the model since the last migration"

**Solution:**
```bash
cd src/OrkinosaiCMS.Infrastructure

# Create a new migration to capture the changes
ASPNETCORE_ENVIRONMENT=Production dotnet ef migrations add DescribeYourChanges \
  --project . \
  --startup-project ../OrkinosaiCMS.Web
```

### Error: "Invalid object name 'AspNetRoles'"

**Cause:** Migrations have not been applied to the database.

**Solution:**
```bash
# Apply all pending migrations
cd src/OrkinosaiCMS.Infrastructure
ASPNETCORE_ENVIRONMENT=Production dotnet ef database update \
  --project . \
  --startup-project ../OrkinosaiCMS.Web
```

### Error: "A database error occurred. Please try again later"

**Check Logs:** Look for detailed error messages in:
- Console output
- `src/OrkinosaiCMS.Web/App_Data/Logs/mosaic-backend-*.log`

**Common Causes:**
1. SQL Server not running
2. Incorrect connection string
3. Database user lacks permissions
4. Firewall blocking connection
5. Migrations not applied

### Error: "Cannot connect to database"

**Solution for Docker SQL Server:**
```bash
# Check if SQL Server container is running
docker ps -a

# View SQL Server logs
docker logs mosaic-sql

# Restart SQL Server container
docker restart mosaic-sql
```

## CI/CD Integration

For automated deployments, migrations can be applied automatically:

### GitHub Actions Example

```yaml
- name: Apply EF Core Migrations
  env:
    ConnectionStrings__DefaultConnection: ${{ secrets.DATABASE_CONNECTION_STRING }}
    ASPNETCORE_ENVIRONMENT: Production
  run: |
    dotnet tool install --global dotnet-ef
    cd src/OrkinosaiCMS.Infrastructure
    dotnet ef database update --project . --startup-project ../OrkinosaiCMS.Web
```

### Azure App Service

Migrations are applied automatically on application startup via `SeedData.InitializeAsync()` in Program.cs. However, for production environments, it's recommended to apply migrations manually during deployment for better control.

## Migration Best Practices

1. **Always test migrations in a non-production environment first**
2. **Review generated SQL script before applying to production**
3. **Backup database before applying migrations in production**
4. **Use idempotent SQL scripts for deployment automation**
5. **Keep migrations small and focused on specific changes**
6. **Never modify existing migrations that have been applied to production**
7. **Use descriptive migration names (e.g., AddUserEmailIndex, not Migration1)**

## Resources

- [EF Core Migrations Documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [EF Core Command Reference](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
- [Azure SQL Database Deployment](./AZURE_DEPLOYMENT.md)
- [Database Architecture](./DATABASE.md)

---

**Last Updated:** 2025-12-15  
**Author:** Copilot SWE Agent
