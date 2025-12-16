# Deployment Verification Guide - OrkinosaiCMS

## Quick Health Check

Run this command to verify your deployment is healthy:

```bash
# Check application is running
curl http://localhost:5000/api/health

# Check admin login page loads
curl -I http://localhost:5000/admin/login

# Check database connectivity
dotnet run --project src/OrkinosaiCMS.Web -- --verify-database
```

## Step-by-Step Verification

### 1. Database Migrations ✅

**Verify migrations are applied:**

```bash
cd src/OrkinosaiCMS.Infrastructure
dotnet ef database update --startup-project ../OrkinosaiCMS.Web
```

**Expected output:**
```
Applying migration '20251215224415_SyncPendingModelChanges'
Done.
```

**Verify Identity tables exist:**

```sql
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('AspNetUsers', 'AspNetRoles', 'AspNetUserRoles')
ORDER BY TABLE_NAME;
```

**Expected result:**
- AspNetRoles
- AspNetUserRoles
- AspNetUsers

### 2. Admin User Creation ✅

**Check admin user exists:**

```sql
SELECT Id, UserName, Email, EmailConfirmed 
FROM AspNetUsers 
WHERE UserName = 'admin';
```

**Expected result:**
```
Id | UserName | Email                | EmailConfirmed
1  | admin    | admin@mosaicms.com  | 1
```

**Verify admin has Administrator role:**

```sql
SELECT u.UserName, r.Name as RoleName
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.UserName = 'admin';
```

**Expected result:**
```
UserName | RoleName
admin    | Administrator
```

### 3. Admin Login Test ✅

**Test credentials:**
- **Username:** `admin`
- **Password:** `Admin@123` (default - change in production!)

**Login flow:**
1. Navigate to `http://your-domain/admin/login`
2. Enter credentials
3. Click "Sign In"
4. Should redirect to `/admin` dashboard

**Common issues:**

| Error | Cause | Solution |
|-------|-------|----------|
| "A database error occurred" | Database not migrated | Run `dotnet ef database update` |
| "Invalid username or password" | Admin user not created | Check SeedData logs, verify AspNetUsers table |
| "Antiforgery token could not be decrypted" | Data Protection keys mismatch | Check App_Data/DataProtection-Keys directory |

### 4. Theme Application (Green Button) ✅

**Test theme application:**
1. Login to admin panel (`/admin`)
2. Navigate to Theme Management (`/admin/themes`)
3. Click green "✓ Apply Theme" button on any theme
4. Verify success message appears

**Expected behavior:**
```
✓ Successfully applied theme 'Default'. The site theme has been updated.
```

**Common issues:**

| Error | Cause | Solution |
|-------|-------|----------|
| "Site with ID 1 not found" | No default site created | Check SeedData logs, verify Sites table has data |
| "Theme is disabled" | Trying to apply disabled theme | Only apply enabled themes |
| "Unable to apply theme due to a database error" | Database connection issue | Check connection string, database accessibility |

### 5. Page Creation Test ✅

**Verify pages exist:**

```sql
SELECT Id, Title, Path, SiteId 
FROM Pages 
ORDER BY Id;
```

**Expected result:** At least 4-6 default pages including:
- Home page (`/`)
- CMS page (`/cms`)
- About page, Services page, etc.

**Create new page test:**
1. Navigate to `/admin/pages` (when implemented)
2. Click "Create Page"
3. Fill in page details
4. Save

### 6. Application Logs ✅

**Check startup logs:**

```bash
cat src/OrkinosaiCMS.Web/App_Data/Logs/mosaic-backend-$(date +%Y%m%d).log
```

**Verify these log entries appear:**

```
✓ Database migration completed
✓ Administrator role created successfully
✓ Admin user created successfully in AspNetUsers table
✓ Administrator role assigned to admin user successfully
Database initialization completed successfully
OrkinosaiCMS application started successfully
Ready to accept requests
```

**Common startup errors:**

```
AspNetUsers table does not exist
→ Solution: Run database migrations

Connection string 'DefaultConnection' not found
→ Solution: Set ConnectionStrings__DefaultConnection environment variable

Unable to connect to the database
→ Solution: Check SQL Server is running, firewall allows connections
```

## Production Deployment Checklist

### Pre-Deployment

- [ ] Backup existing database (if upgrading)
- [ ] Review and test migrations on staging environment
- [ ] Update connection string in environment variables
- [ ] Change default admin password
- [ ] Configure JWT secret key (32+ characters)
- [ ] Set up Data Protection keys persistence (Azure Blob or Key Vault)
- [ ] Configure Serilog for Azure App Insights or external logging service

### Deployment

- [ ] Deploy application files
- [ ] Apply database migrations: `dotnet ef database update`
- [ ] Verify web app starts without errors
- [ ] Test admin login with new credentials
- [ ] Verify theme application works
- [ ] Test creating a new page
- [ ] Smoke test critical user flows

### Post-Deployment

- [ ] Monitor application logs for errors
- [ ] Verify database connection pooling is working
- [ ] Test authentication across multiple instances (if scaled out)
- [ ] Verify antiforgery tokens work correctly
- [ ] Load test theme application and page creation
- [ ] Set up health check monitoring
- [ ] Configure alerts for critical errors

## Troubleshooting Commands

### Reset Admin Password

```csharp
// Run in C# Interactive or create a console app
using Microsoft.AspNetCore.Identity;
using OrkinosaiCMS.Core.Entities.Identity;

var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
var user = await userManager.FindByNameAsync("admin");
if (user != null)
{
    var token = await userManager.GeneratePasswordResetTokenAsync(user);
    var result = await userManager.ResetPasswordAsync(user, token, "NewSecurePassword123!");
    Console.WriteLine(result.Succeeded ? "Password reset successful" : "Failed");
}
```

### Verify Database Schema

```bash
# List all tables
cd src/OrkinosaiCMS.Infrastructure
dotnet ef database tables --startup-project ../OrkinosaiCMS.Web

# Generate SQL script to see what would be created
dotnet ef migrations script --startup-project ../OrkinosaiCMS.Web
```

### Clear Data Protection Keys (Antiforgery Issues)

```bash
# WARNING: This will log out all users!
rm -rf src/OrkinosaiCMS.Web/App_Data/DataProtection-Keys/*
```

### View Recent Logs

```bash
# Last 100 lines of today's log
tail -100 src/OrkinosaiCMS.Web/App_Data/Logs/mosaic-backend-$(date +%Y%m%d).log

# Search for errors
grep -i "error\|exception\|fail" src/OrkinosaiCMS.Web/App_Data/Logs/mosaic-backend-*.log
```

## Environment Configuration

### Required Environment Variables

```bash
# Production SQL Server
export ConnectionStrings__DefaultConnection="Server=tcp:yourserver.database.windows.net,1433;Database=MosaicCMS_Prod;User ID=yourusername;Password=yourpassword;Encrypt=True;TrustServerCertificate=False;"

# Admin Password (change from default!)
export DefaultAdminPassword="YourSecurePasswordHere123!"

# JWT Secret (32+ characters)
export Authentication__Jwt__SecretKey="your-super-secret-jwt-key-min-32-chars-production-value"

# Database Provider
export DatabaseProvider="SqlServer"  # or "SQLite" for development
```

### Azure App Service Configuration

Set these in **Configuration > Application settings**:

| Setting | Value | Example |
|---------|-------|---------|
| `ConnectionStrings__DefaultConnection` | Azure SQL connection string | `Server=tcp:...` |
| `DefaultAdminPassword` | Secure admin password | `Adm1nP@ssw0rd!2024` |
| `Authentication__Jwt__SecretKey` | 32+ character secret | `production-jwt-secret-key-2024-secure` |
| `DatabaseProvider` | `SqlServer` | `SqlServer` |
| `ASPNETCORE_ENVIRONMENT` | `Production` | `Production` |

## Health Check Endpoint

**Implement this in Program.cs to add health checks:**

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database");

app.MapHealthChecks("/api/health");
```

**Then test:**

```bash
curl http://localhost:5000/api/health
# Expected: {"status":"Healthy","results":{"database":{"status":"Healthy"}}}
```

## Support

If issues persist after following this guide:

1. **Check logs:** `App_Data/Logs/mosaic-backend-*.log`
2. **Enable detailed errors:** Set `ErrorHandling__ShowDetailedErrors: true` in appsettings.json
3. **Run database verification:** Use the scripts in `scripts/` directory
4. **Review migrations:** Check `src/OrkinosaiCMS.Infrastructure/Migrations/`
5. **Contact support:** Provide logs, error messages, and deployment environment details

## Quick Reference

**Default Credentials:**
- Username: `admin`
- Password: `Admin@123`
- ⚠️ **CHANGE IN PRODUCTION!**

**Key URLs:**
- Admin Login: `/admin/login`
- Admin Dashboard: `/admin`
- Theme Management: `/admin/themes`
- API Health: `/api/health`

**Key Files:**
- Logs: `App_Data/Logs/`
- Data Protection Keys: `App_Data/DataProtection-Keys/`
- Configuration: `appsettings.json`, `appsettings.Production.json`
- Migrations: `src/OrkinosaiCMS.Infrastructure/Migrations/`
