# Azure Deployment Configuration Guide

## Critical: Fixing HTTP Error 500.30

This guide explains how to properly configure the Mosaic CMS application in Azure App Service to prevent HTTP Error 500.30 (ASP.NET Core app failed to start).

## Overview

The application **will not start** without proper configuration. As of the latest security updates, sensitive credentials have been removed from source control and **must** be configured via Azure App Service Configuration.

## Required Configuration

### 1. Database Connection String (REQUIRED)

**Without this, the application will fail with HTTP 500.30**

#### Steps to Configure in Azure Portal:

1. Navigate to your Azure App Service
2. Go to **Configuration** > **Connection strings**
3. Click **+ New connection string**
4. Enter the following:
   - **Name**: `DefaultConnection`
   - **Value**: Your SQL Server connection string (see format below)
   - **Type**: `SQLServer`
5. Click **OK**
6. Click **Save** at the top
7. Click **Continue** to restart the app

#### Connection String Format:

```
Server=tcp:orkinosai.database.windows.net,1433;Initial Catalog=mosaic-saas;Persist Security Info=False;User ID=sqladmin;Password=YOUR_PASSWORD_HERE;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Max Pool Size=100;Min Pool Size=5;Pooling=true
```

**Important Connection Pool Settings:**
- `Max Pool Size=100` - Prevents connection pool exhaustion
- `Min Pool Size=5` - Maintains baseline connections for performance  
- `Pooling=true` - Enables connection pooling
- `Connection Timeout=30` - Reasonable timeout for establishing connections

### 2. Application Settings (REQUIRED)

Navigate to **Configuration** > **Application settings** and add:

#### Required Settings:

| Name | Value | Description |
|------|-------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Sets the environment |
| `DefaultAdminPassword` | `YOUR_SECURE_PASSWORD` | Admin account password |
| `Authentication__Jwt__SecretKey` | `YOUR_32_CHAR_SECRET` | JWT signing key (min 32 chars) |

#### Recommended Settings:

| Name | Value | Description |
|------|-------|-------------|
| `DatabaseProvider` | `SqlServer` | Database type (default) |
| `Database__AutoApplyMigrations` | `true` | Auto-apply migrations on startup |
| `ErrorHandling__ShowDetailedErrors` | `false` | Hide detailed errors in production |

## Security Best Practices

### ✅ DO:
- **Configure all sensitive values via Azure App Service Configuration**
- Use Azure Key Vault for highly sensitive production credentials
- Rotate passwords regularly
- Use strong passwords (12+ characters, mixed case, numbers, symbols)
- Restrict access to Azure Portal configuration
- Use SQL authentication (not Windows authentication) for Azure SQL

### ❌ DON'T:
- **Never commit production passwords to source control**
- Don't use the hardcoded values from appsettings.Production.json (they have been removed)
- Don't share connection strings via email or chat
- Don't use weak or default passwords
- Don't disable SSL/TLS encryption on connections

## Verifying Configuration

### 1. Check Application Logs

After deployment:

1. Go to Azure Portal > Your App Service > **Log stream**
2. Look for startup messages:
   ```
   Starting OrkinosaiCMS application
   Environment: Production
   Configuring database with provider: SqlServer
   Connection string (sanitized): Server=tcp:orkinosai.database.windows.net...
   ```

3. Verify no errors appear

### 2. Test Health Endpoint

```bash
curl https://mosaic-saas.azurewebsites.net/api/health
```

Expected response:
```json
{
  "status": "Healthy",
  "results": {
    "database": {
      "status": "Healthy",
      "description": "Database is accessible and migrations are applied"
    }
  }
}
```

### 3. Test Admin Login

1. Navigate to `https://mosaic-saas.azurewebsites.net/admin/login`
2. Try logging in with:
   - **Username**: `admin@mosaic.com`
   - **Password**: The value you set for `DefaultAdminPassword`
3. Login should succeed

## Troubleshooting

### Error: "Connection string 'DefaultConnection' not found"

**Cause**: Connection string not configured in Azure

**Fix**:
1. Follow steps in "Database Connection String" section above
2. Ensure the connection string name is exactly `DefaultConnection`
3. Save and restart the app

### Error: "No such host is known" or Error 40

**Cause**: Invalid server name in connection string

**Fix**:
1. Verify the server name in your connection string
2. Ensure it matches your Azure SQL Server name
3. Check firewall rules allow Azure services

### Error: Login failed for user 'sqladmin'

**Cause**: Invalid password in connection string

**Fix**:
1. Verify the password in your connection string
2. Ensure it matches your Azure SQL Server admin password
3. Try resetting the SQL Server admin password if needed

### Error: HTTP 503 Service Unavailable

**Cause**: Connection pool exhaustion or database overload

**Fix**:
1. Ensure connection string includes pooling settings (see format above)
2. Check database performance in Azure Portal
3. Consider scaling up the database tier
4. Review application logs for connection leaks

### Error: Database validation failed - AspNetUsers table missing

**Cause**: Database migrations not applied

**Fix**:
1. Set `Database__AutoApplyMigrations=true` in Application settings
2. Restart the app to auto-apply migrations
3. OR manually apply migrations:
   ```bash
   cd src/OrkinosaiCMS.Infrastructure
   dotnet ef database update --startup-project ../OrkinosaiCMS.Web
   ```

## Running the Diagnostic Workflow

If the app still fails to start:

1. Go to GitHub repository > **Actions** tab
2. Click on **"Fetch and Diagnose App Errors"** workflow
3. Click **Run workflow**
4. Wait for completion
5. Download the artifacts (logs)
6. Check `startup-errors-extracted.txt` for the actual error

## Migration Guide (for existing deployments)

If your deployment was using the old hardcoded connection string:

1. **Backup** your current connection string (including password)
2. **Add** the connection string to Azure App Service Configuration (steps above)
3. **Deploy** the latest code (which has removed hardcoded credentials)
4. **Verify** the app starts successfully
5. **Test** all functionality

## Environment Variable Format

If you prefer using environment variables instead of Connection strings:

```bash
# Connection string
ConnectionStrings__DefaultConnection="Server=tcp:orkinosai.database.windows.net,1433;..."

# Application settings
DefaultAdminPassword="YOUR_SECURE_PASSWORD"
Authentication__Jwt__SecretKey="YOUR_32_CHAR_SECRET"
DatabaseProvider="SqlServer"
Database__AutoApplyMigrations="true"
```

## Related Documentation

- [TROUBLESHOOTING_HTTP_500_30.md](./TROUBLESHOOTING_HTTP_500_30.md) - Detailed HTTP 500.30 troubleshooting
- [AZURE_CONNECTION_STRING_SETUP.md](./AZURE_CONNECTION_STRING_SETUP.md) - Connection string setup guide
- [DEPLOYMENT_VERIFICATION_GUIDE.md](./DEPLOYMENT_VERIFICATION_GUIDE.md) - Post-deployment verification
- [HTTP_500_30_FIX_SUMMARY.md](./HTTP_500_30_FIX_SUMMARY.md) - Previous fix implementations

## Support

If you encounter issues not covered in this guide:

1. Check the [Troubleshooting](./TROUBLESHOOTING_HTTP_500_30.md) guide
2. Review application logs in Azure Portal
3. Run the diagnostic workflow
4. Open an issue with the error details

---

**Last Updated**: December 2024  
**Status**: ✅ Security Enhanced - Hardcoded credentials removed
