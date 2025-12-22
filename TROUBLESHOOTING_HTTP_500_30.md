# Troubleshooting HTTP Error 500.30 - ASP.NET Core App Failed to Start

This guide helps diagnose and fix the common "HTTP Error 500.30 - ASP.NET Core app failed to start" error when deploying to Azure App Service or IIS.

## 🚀 Automated Diagnostics (Recommended First Step)

**NEW**: We have an automated workflow that fetches and analyzes logs from your Azure deployment!

### How to Run Automated Diagnostics

1. Go to the [Actions tab](../../actions/workflows/fetch-diagnose-app-errors.yml)
2. Click "Run workflow"
3. (Optional) Adjust the time range if needed (default: last 60 minutes)
4. Click "Run workflow" button
5. Wait for the workflow to complete (~1 minute)
6. Check the workflow summary for extracted startup errors
7. Download artifacts to see detailed logs

### What the Workflow Does

The automated diagnostics workflow:
- ✅ Fetches the latest stdout logs from Azure App Service
- ✅ Extracts startup errors and exceptions automatically
- ✅ Displays the actual error in the workflow summary
- ✅ Creates a `startup-errors-extracted.txt` file with the root cause
- ✅ Collects all related logs for deeper analysis
- ✅ Provides actionable troubleshooting guidance

**Benefits:**
- 🎯 **See the actual error immediately** without manual log hunting
- ⚡ **Fast diagnosis** - results in ~1 minute
- 📋 **Comprehensive** - collects all relevant logs in one place
- 🔍 **Automated error extraction** - no need to parse logs manually

If the automated workflow shows errors, follow the specific guidance provided. If you need manual troubleshooting, continue with the steps below.

---

## Quick Fix Checklist

If you're seeing HTTP 500.30, work through this checklist:

- [ ] **Enable stdout logging** - Check `web.config` has `stdoutLogEnabled="true"`
- [ ] **Enable Azure logging** - Azure Portal → App Service → App Service Logs → Enable Application Logging
- [ ] **View logs** - Azure Portal → Log Stream OR Kudu → LogFiles/stdout
- [ ] **Check connection string** - Verify `ConnectionStrings__DefaultConnection` is set in Configuration
- [ ] **Verify .NET runtime** - Ensure .NET 10 runtime is installed OR use self-contained deployment
- [ ] **Apply migrations** - Run `dotnet ef database update` before first deployment
- [ ] **Check environment variables** - Verify all required settings are in Configuration

## Understanding HTTP Error 500.30

**What it means:** The ASP.NET Core application failed during startup. The process started but crashed before it could handle requests.

**Common causes:**
1. Missing or invalid connection strings
2. Database not accessible or migrations not applied
3. Missing environment variables or configuration
4. Unhandled exceptions in `Program.cs` or `Startup.cs`
5. Missing dependencies or incompatible runtime
6. File permission issues (logs, data protection keys)

## Step 1: Enable Stdout Logging

### Verify web.config Exists

Check that `src/OrkinosaiCMS.Web/web.config` exists with this configuration:

```xml
<aspNetCore processPath="dotnet"
            arguments=".\OrkinosaiCMS.Web.dll"
            stdoutLogEnabled="true"
            stdoutLogFile=".\logs\stdout"
            hostingModel="inprocess">
```

**Key settings:**
- `stdoutLogEnabled="true"` - **CRITICAL**: Enables stdout capture
- `stdoutLogFile=".\logs\stdout"` - Log file location
- `hostingModel="inprocess"` - Best performance for Azure/IIS

### Enable Application Logging in Azure

1. Go to **Azure Portal** → Your App Service
2. Navigate to **App Service logs** (left menu)
3. Configure:
   - **Application Logging (Filesystem)**: **On**
   - **Level**: **Verbose** (for detailed errors)
   - **Web server logging**: **File System**
   - **Retention Period**: 7+ days
4. Click **Save**

## Step 2: View Logs

### Real-Time Logs (Recommended for Troubleshooting)

**Azure Portal:**
1. App Service → **Log stream**
2. Wait for connection
3. Deploy or restart app
4. Watch for errors in real-time

**Azure CLI:**
```bash
az webapp log tail --name <app-name> --resource-group <resource-group-name>
```


### Historical Logs

**Kudu Console (Advanced Tools):**
1. Azure Portal → App Service → **Advanced Tools** → **Go**
2. Navigate to **Debug console** → **CMD**
3. Browse to `D:\home\LogFiles\Application\` or `D:\home\LogFiles\stdout\`
4. Download and view log files

**FTP Access:**
- Server: `<app-name>.scm.azurewebsites.net`
- Path: `/LogFiles/Application/` and `/LogFiles/stdout/`

## Step 3: Common Errors and Solutions

### Error: "Connection string 'DefaultConnection' not found"

**Cause:** Missing database connection string configuration.

**Solution:**
1. Azure Portal → App Service → **Configuration** → **Connection strings**
2. Add new connection string:
   - **Name**: `DefaultConnection`
   - **Value**: Your Azure SQL connection string
   - **Type**: `SQLServer`
3. Click **Save** and **Restart**

**Connection string format:**
```
Server=tcp:your-server.database.windows.net,1433;
Initial Catalog=your-database;
Persist Security Info=False;
User ID=your-user;
Password=your-password;
MultipleActiveResultSets=False;
Encrypt=True;
TrustServerCertificate=False;
Connection Timeout=30;
```

### Error: "Invalid object name 'AspNetUsers'"

**Cause:** Database migrations have not been applied.

**Solution:**

**Option 1 - Local migrations (Recommended):**
```bash
# From your development machine
cd src/OrkinosaiCMS.Infrastructure

# Set connection string to production database
export ConnectionStrings__DefaultConnection="<your-prod-connection-string>"

# Apply migrations
dotnet ef database update --startup-project ../OrkinosaiCMS.Web

# OR use the script
bash ../../scripts/apply-migrations.sh update
```

**Option 2 - Enable auto-migrations (Not recommended for production):**

Set in Azure Portal → Configuration → Application settings:
- **Name**: `Database__AutoApplyMigrations`
- **Value**: `true`

⚠️ **Warning:** Auto-migrations can be risky in production. Always test migrations in staging first.

### Error: "There is already an object named 'Modules' in the database" (Schema Drift)

**Full Error:**
```
Database migration failed: Schema drift recovery failed: There is already an object named 'Modules' in the database.
```

**Cause:** Database schema is out of sync with migrations. This typically occurs when:
- Migrations were applied manually or partially
- Database was created with old schema
- Concurrent migrations from multiple instances
- Previous migration failed mid-execution

**Solution:**

**Option 1 - Reset migration history (Safe - Recommended):**

This marks existing tables as migrated without recreating them.

```bash
# Connect to your Azure SQL database and run:
# This tells EF Core that migrations have already been applied

-- Check current migration state
SELECT * FROM __EFMigrationsHistory ORDER BY MigrationId;

-- If schema drift detected, manually add missing migration entries
-- Get list of migrations from your project
-- Then insert them into __EFMigrationsHistory table

-- Example (replace with your actual migration IDs):
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
VALUES ('20241215224415_SyncPendingModelChanges', '10.0.0');
```

**Option 2 - Fresh database (Development/Staging only):**

⚠️ **WARNING: This deletes all data!**

```bash
# Drop and recreate database (ONLY for dev/staging)
dotnet ef database drop --startup-project src/OrkinosaiCMS.Web
dotnet ef database update --startup-project src/OrkinosaiCMS.Web
```

**Option 3 - Manual schema sync:**

If specific tables conflict, rename or drop them manually:

```sql
-- Rename conflicting table (preserves data)
EXEC sp_rename 'Modules', 'Modules_Backup';

-- Then retry migration
-- If successful, you can drop the backup:
-- DROP TABLE Modules_Backup;
```

**Prevention:**
- Always test migrations in staging environment first
- Use single-instance deployment during migrations
- Enable `Database__AutoApplyMigrations=false` in production
- Apply migrations manually via deployment pipeline

### Error: "The antiforgery token could not be decrypted"

**Cause:** Data Protection keys are not persisted or changed between restarts.

**Solution:**

The app is configured to persist keys to `App_Data/DataProtection-Keys/` which is shared storage in Azure App Service.

If issues persist:
1. **Clear browser cookies** (client-side)
2. **Restart the app** (Azure Portal → Restart)
3. **Verify storage**: Check that `/home/site/wwwroot/App_Data/DataProtection-Keys/` exists in Kudu

**For production with multiple instances**, consider Azure Key Vault:
```csharp
builder.Services.AddDataProtection()
    .PersistKeysToAzureBlobStorage(blobUri)
    .ProtectKeysWithAzureKeyVault(keyId, credential);
```

### Error: "Failed to start application" with no additional details

**Cause:** Unhandled exception during startup in `Program.cs`.

**Solution:**
1. Check stdout logs for the full exception stack trace
2. Common startup exceptions:
   - Missing required configuration (JWT secret, admin password)
   - Database connection timeout
   - Service registration errors
   - File permission issues

**Enable detailed errors (staging only):**

Azure Portal → Configuration → Application settings:
- **Name**: `ErrorHandling__ShowDetailedErrors`
- **Value**: `true`

⚠️ **Never enable in production** - exposes sensitive information.

### Error: ".NET runtime not found"

**Cause:** .NET 10 runtime is not installed on Azure App Service.

**Solution:**

**Option 1 - Self-contained deployment (Recommended):**
```bash
# Publish with runtime included
dotnet publish src/OrkinosaiCMS.Web/OrkinosaiCMS.Web.csproj \
  --configuration Release \
  --self-contained true \
  --runtime win-x64 \
  --output ./publish
```

**Option 2 - Verify Azure runtime:**
- Azure Portal → App Service → **Configuration** → **General settings**
- **Stack**: .NET
- **Major version**: .NET 10 (preview)
- **Minor version**: Latest

## Step 4: Verify Azure Configuration

### Required Application Settings

Azure Portal → App Service → Configuration → Application settings:

| Setting | Value | Required |
|---------|-------|----------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Yes |
| `ConnectionStrings__DefaultConnection` | See connection strings section | Yes |
| `DefaultAdminPassword` | Secure password | Yes |
| `Authentication__Jwt__SecretKey` | 32+ character secret | Recommended |
| `DatabaseProvider` | `SqlServer` | Yes |

### Required Connection Strings

Azure Portal → App Service → Configuration → Connection strings:

| Name | Type | Required |
|------|------|----------|
| `DefaultConnection` | SQLServer | Yes |

## Step 5: Test Deployment Locally

Before deploying to Azure, test the published output locally:

```bash
# Build and publish
dotnet publish src/OrkinosaiCMS.Web/OrkinosaiCMS.Web.csproj \
  --configuration Release \
  --output ./publish

# Set environment variables
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="your-connection-string"

# Run the published app
cd ./publish
dotnet OrkinosaiCMS.Web.dll
```

Check for startup errors before deploying to Azure.

## Step 6: Health Check

After successful deployment, verify:

### 1. Health Endpoint
```bash
curl https://your-app-name.azurewebsites.net/api/health
```

**Expected response:**
```json
{"status":"Healthy","results":{"database":{"status":"Healthy"}}}
```

### 2. Admin Login
1. Navigate to `https://your-app-name.azurewebsites.net/admin/login`
2. Login with admin credentials
3. Verify dashboard loads

### 3. Check Logs
No errors in:
- Log Stream
- stdout logs
- Application logs

## Advanced Troubleshooting

### Enable Diagnostic Logging

**Application Insights (Recommended):**
1. Azure Portal → App Service → **Application Insights**
2. Enable and configure
3. View detailed telemetry and exceptions

**File System Logging:**
```bash
# In web.config, verify:
stdoutLogEnabled="true"

# In appsettings.Production.json, configure Serilog:
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning"
      }
    }
  }
}
```

### Remote Debugging

**Kudu Debug Console:**
1. Navigate to `https://<app-name>.scm.azurewebsites.net`
2. **Debug console** → **CMD**
3. Run commands:
   ```cmd
   # Check environment variables
   set
   
   # View running processes
   tasklist
   
   # Check app files
   cd D:\home\site\wwwroot
   dir
   ```

### Check File Permissions

Ensure the app can create directories:
```cmd
# In Kudu console
cd D:\home\site\wwwroot
mkdir App_Data\Logs
mkdir App_Data\DataProtection-Keys
mkdir logs
```

## Prevention: Deployment Checklist

Before each deployment:

- [ ] Run migrations on staging database first
- [ ] Test published build locally
- [ ] Verify all environment variables are set
- [ ] Check stdout logging is enabled
- [ ] Enable Azure logging before deployment
- [ ] Have Log Stream open during deployment
- [ ] Test health endpoint after deployment
- [ ] Verify admin login works
- [ ] Monitor logs for 5-10 minutes post-deployment

## Getting Help

If the issue persists:

1. **Collect logs:**
   - stdout logs from `D:\home\LogFiles\stdout\`
   - Application logs from `App_Data/Logs/`
   - Deployment logs from Azure Portal

2. **Provide context:**
   - Azure App Service configuration
   - Connection string format (sanitized)
   - Deployment method (CI/CD, manual, etc.)
   - Last successful deployment time

3. **Check documentation:**
   - [ASP.NET Core Azure troubleshooting](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/azure-apps/troubleshoot)
   - [DEPLOYMENT_VERIFICATION_GUIDE.md](./DEPLOYMENT_VERIFICATION_GUIDE.md)

## Related Documentation

- [DEPLOYMENT_VERIFICATION_GUIDE.md](./DEPLOYMENT_VERIFICATION_GUIDE.md) - Post-deployment verification
- [DEPLOYMENT_NOTES.md](./DEPLOYMENT_NOTES.md) - Connection string configuration
- [appsettings.json](./src/OrkinosaiCMS.Web/appsettings.json) - Configuration reference
- [web.config](./src/OrkinosaiCMS.Web/web.config) - IIS/Azure module configuration

## Summary

HTTP Error 500.30 is almost always a **startup failure** that can be diagnosed by:
1. **Enabling stdout logging** in web.config
2. **Viewing the logs** in Azure Portal or Kudu
3. **Fixing the root cause** (usually missing config or database issues)

The most common fixes are:
- Set connection string in Azure Configuration
- Apply database migrations
- Set required environment variables
- Enable Application Logging in Azure

With stdout logging enabled, you'll see the **exact error message and stack trace** that caused the startup failure, making it much easier to diagnose and fix.
