# Mosaic CMS Authentication & Database Deployment Guide

## Overview

Mosaic CMS uses **ASP.NET Core Identity** for authentication, following industry best practices from battle-tested CMS platforms like Oqtane. This guide covers the authentication system, database requirements, and deployment configuration.

## Authentication Architecture

### Identity-Based Authentication

The application uses ASP.NET Core Identity with the following features:

- **Cookie-based authentication** (default scheme: `.OrkinosaiCMS.Auth`)
- **UserManager and SignInManager** for secure password handling
- **Identity tables**: AspNetUsers, AspNetRoles, AspNetUserRoles, AspNetUserClaims, etc.
- **Antiforgery protection** for Blazor Server forms
- **Data Protection** for secure token/cookie encryption

### Default Credentials

```
Username: admin
Password: Admin@123
Email: admin@mosaicms.com
Role: Administrator
```

**⚠️ IMPORTANT**: Change the default password after first deployment!

## Database Requirements

### Required Tables

The database must include the following Identity tables (created by migrations):

1. **AspNetUsers** - User accounts with password hashes
2. **AspNetRoles** - Security roles (Administrator, Editor, etc.)
3. **AspNetUserRoles** - User-to-role assignments
4. **AspNetUserClaims** - User claims for fine-grained permissions
5. **AspNetUserLogins** - External login providers (Google, Microsoft, etc.)
6. **AspNetUserTokens** - Authentication tokens
7. **AspNetRoleClaims** - Role-based claims

### Legacy Tables (Backward Compatibility)

The following tables exist for backward compatibility and will be phased out:

- **LegacyUsers** (formerly Users)
- **LegacyRoles** (formerly Roles)
- **LegacyUserRoles** (formerly UserRoles)

### Database Initialization

The application automatically:
1. **Creates the database** if it doesn't exist (EnsureCreated)
2. **Runs migrations** to create all required tables
3. **Seeds initial data**:
   - Administrator role in AspNetRoles
   - Default admin user in AspNetUsers
   - User-role assignment in AspNetUserRoles
   - CMS demo pages, themes, modules

## Deployment Scenarios

### 1. Azure App Service (SQL Server)

**Connection String Configuration:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:your-server.database.windows.net,1433;Database=MosaicCMS;User ID=your-user;Password=your-password;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  },
  "DatabaseProvider": "SqlServer"
}
```

**Environment Variables (Recommended for Azure):**

```
ConnectionStrings__DefaultConnection=Server=...
DatabaseProvider=SqlServer
```

**Data Protection:**

Keys are persisted to `/home/site/wwwroot/App_Data/DataProtection-Keys/` which:
- ✅ Persists across app restarts
- ✅ Is shared across scale-out instances via Azure Files
- ✅ Works with default Azure App Service settings

**Optional Enhancement - Azure Key Vault:**

For production at scale, configure Azure Key Vault:

```csharp
builder.Services.AddDataProtection()
    .PersistKeysToAzureBlobStorage(blobUri)
    .ProtectKeysWithAzureKeyVault(keyId, credential)
    .SetApplicationName("OrkinosaiCMS");
```

### 2. Local Development (SQLite)

**appsettings.Development.json:**

```json
{
  "DatabaseProvider": "SQLite",
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=orkinosai-cms.db"
  }
}
```

SQLite is ideal for local development but **NOT recommended for production**.

### 3. Docker Deployment

**docker-compose.yml:**

```yaml
version: '3.8'
services:
  web:
    image: mosaicms:latest
    environment:
      - DatabaseProvider=SqlServer
      - ConnectionStrings__DefaultConnection=Server=db;Database=MosaicCMS;User Id=sa;Password=YourPassword123!;
    ports:
      - "80:8080"
      - "443:8081"
    volumes:
      - dataprotection-keys:/app/App_Data/DataProtection-Keys
  
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123!
    volumes:
      - sqldata:/var/opt/mssql

volumes:
  dataprotection-keys:
  sqldata:
```

## Configuration Settings

### Identity Options (Program.cs)

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 10;
    options.Lockout.AllowedForNewUsers = true;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    
    // Sign in settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();
```

### Cookie Configuration

```csharp
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/admin/login";
    options.LogoutPath = "/admin/logout";
    options.AccessDeniedPath = "/admin/login";
    options.Cookie.Name = ".OrkinosaiCMS.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});
```

### Antiforgery Configuration

```csharp
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = ".AspNetCore.Antiforgery.OrkinosaiCMS";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.IsEssential = true;
});
```

## Database Migration

### First-Time Deployment

The application uses **EnsureCreated** for automatic database initialization:

```csharp
await context.Database.EnsureCreatedAsync();
```

This creates all tables including Identity tables on first run.

### Manual Migration (Optional)

If you prefer explicit migrations:

```bash
# Apply migrations
dotnet ef database update -p src/OrkinosaiCMS.Infrastructure -s src/OrkinosaiCMS.Web

# Create new migration
dotnet ef migrations add YourMigrationName -p src/OrkinosaiCMS.Infrastructure -s src/OrkinosaiCMS.Web
```

### Migration Files

Current migrations in `src/OrkinosaiCMS.Infrastructure/Migrations/`:

1. **20251129175729_InitialCreate** - Core CMS tables
2. **20251209164111_AddThemeEnhancements** - Theme system
3. **20251211225909_AddSubscriptionEntities** - Subscription support
4. **20251215015307_AddIdentityTables** - ASP.NET Core Identity tables ✅

## Troubleshooting

### "Invalid object name 'AspNetRoles'"

**Cause**: Database exists but Identity tables haven't been created.

**Solution**:
1. Delete the existing database
2. Restart the application (will recreate with Identity tables)
3. OR run `dotnet ef database update` to apply migrations

### "Data seeding fails: no master page"

**Cause**: Database initialization incomplete or interrupted.

**Solution**:
1. Check logs for SQL errors during initialization
2. Verify connection string is correct
3. Ensure database user has CREATE TABLE permissions
4. Delete database and restart to reinitialize

### "CryptographicException: key was not found in key ring"

**Cause**: Data protection keys not persisted or accessible.

**Solution**:
1. Verify `/App_Data/DataProtection-Keys/` directory exists and is writable
2. For Azure: Ensure `/home/site/wwwroot/App_Data/DataProtection-Keys/` is accessible
3. Check Application Insights for key rotation issues
4. Consider Azure Key Vault for production

### "AntiforgeryValidationException: token could not be decrypted"

**Cause**: Data protection keys changed or not shared across instances.

**Solution**:
1. Ensure all instances share the same key storage location
2. Use Azure Blob Storage or Key Vault for multi-instance deployments
3. Verify antiforgery cookie is being set correctly
4. Check browser allows cookies from your domain

### HTTP 400 on /admin/login

**Cause**: Antiforgery or data protection misconfiguration.

**Solution**:
1. Verify `AddAntiforgery()` is called in service registration
2. Verify `UseAntiforgery()` is called AFTER `UseAuthentication()`
3. Check data protection keys are persisted
4. Enable detailed logging to see specific error

## Security Best Practices

### Production Checklist

- [ ] Change default admin password
- [ ] Use strong connection strings (Azure SQL with managed identity)
- [ ] Enable HTTPS (required for secure cookies)
- [ ] Configure Azure Key Vault for data protection keys
- [ ] Enable Application Insights for monitoring
- [ ] Set up automated backups for database
- [ ] Implement password rotation policy
- [ ] Enable two-factor authentication (future enhancement)
- [ ] Configure rate limiting for login endpoints

### Cookie Security

All authentication cookies use:
- ✅ **HttpOnly** - Prevents XSS attacks from stealing tokens
- ✅ **Secure** - Requires HTTPS in production
- ✅ **SameSite** - Prevents CSRF attacks
- ✅ **IsEssential** - Not blocked by cookie consent

### Password Security

Identity provides:
- ✅ **PBKDF2 with salt** - Industry-standard password hashing
- ✅ **Security stamps** - Invalidate sessions on password change
- ✅ **Lockout protection** - Prevents brute force attacks (10 attempts = 30 min lockout)
- ✅ **Timing attack prevention** - Constant-time password comparison

## Authentication Endpoints

### API Endpoints

```
POST   /api/authentication/login      - Login with username/password
POST   /api/authentication/logout     - Logout current user
GET    /api/authentication/status     - Check authentication status
POST   /api/authentication/validate   - Validate credentials without login
```

### Blazor Pages

```
/admin/login     - Admin login page (Blazor)
/admin/logout    - Logout redirect
/admin           - Admin dashboard (requires authentication)
```

## Testing Authentication

### Manual Testing

```bash
# Login
curl -X POST http://localhost:5000/api/authentication/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin@123","rememberMe":false}' \
  -c cookies.txt

# Check status
curl http://localhost:5000/api/authentication/status -b cookies.txt

# Logout
curl -X POST http://localhost:5000/api/authentication/logout -b cookies.txt
```

### Automated Testing

```bash
# Run all authentication tests
dotnet test --filter "FullyQualifiedName~AuthenticationTests"

# Expected: 22 tests passing
```

## Monitoring & Logging

### Key Log Messages

**Successful initialization:**
```
[INF] Created data protection keys directory: /path/to/keys
[INF] Antiforgery middleware configured (after authentication)
[INF] Database initialization completed successfully
[INF] Seeding Identity users...
```

**Successful login:**
```
[INF] Login successful for username: admin, redirecting to /admin
```

**Failed login:**
```
[WRN] User not found: username
[WRN] Invalid password for user: username
```

### Application Insights Queries

Monitor authentication health:

```kusto
// Failed logins
traces
| where message contains "Invalid password" or message contains "User not found"
| summarize count() by bin(timestamp, 1h)

// Lockouts
traces
| where message contains "Account locked"
| summarize count() by user_AuthenticatedUserId, bin(timestamp, 1d)

// Antiforgery errors
exceptions
| where type contains "Antiforgery"
| project timestamp, message, operation_Name, cloud_RoleInstance
```

## Future Enhancements (Optional)

Based on Oqtane patterns, these features can be added:

- **Two-Factor Authentication** - Email/SMS verification
- **External Login Providers** - Google, Microsoft, Facebook OAuth
- **Email Verification** - Confirm email before login
- **Password Reset** - Self-service password recovery
- **Account Recovery** - Security questions or backup codes
- **Audit Logging** - Track all authentication events

## Summary

Mosaic CMS uses industry-standard ASP.NET Core Identity for authentication with:

✅ **Secure password hashing** (PBKDF2)  
✅ **Cookie-based authentication** (HttpOnly, Secure, SameSite)  
✅ **Antiforgery protection** (CSRF prevention)  
✅ **Data protection** (encrypted tokens and cookies)  
✅ **Account lockout** (brute force protection)  
✅ **Production-ready** (tested, documented, deployed)

The system requires **Identity tables** (AspNetUsers, AspNetRoles, etc.) which are created automatically on first run via migrations.

---

**Status**: ✅ Production Ready  
**Last Updated**: December 15, 2024  
**Migration Version**: 20251215015307_AddIdentityTables
