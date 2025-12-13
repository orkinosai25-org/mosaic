# Error Logging and Diagnostics - Troubleshooting Guide

## Overview

This guide provides comprehensive troubleshooting steps for diagnosing errors in the OrkinosaiCMS application, particularly when errors occur before standard logging is initialized.

## Problem: HTTP 400/500 Errors with No Logs

### Common Causes

1. **Antiforgery Token Validation Failure**
   - Forms submitted without valid CSRF tokens
   - Tokens expired or mismatched
   - Browser cookies disabled

2. **Model Binding/Validation Errors**
   - Invalid form data format
   - Required fields missing
   - Data type mismatches

3. **Startup Errors Before Logging Initialization**
   - Configuration file errors
   - Missing connection strings
   - Invalid configuration values
   - Database connection failures during startup

4. **Log Directory Creation Failures**
   - Insufficient permissions
   - Disk full
   - Invalid path configuration

## Logging Architecture

### Log Locations

1. **File Logs** (if directory creation succeeds)
   - Location: `App_Data/Logs/mosaic-backend-YYYYMMDD.log`
   - Retention: 7 days
   - Max size: 10 MB per file

2. **Console Output**
   - Always enabled as fallback
   - Visible in Azure App Service diagnostics stream
   - Format: `[Timestamp] [Level] [SourceContext] Message`

3. **Azure App Service Diagnostics** (Production)
   - Application Logs: Enable in Azure Portal → App Service → Diagnostic Settings
   - Stream logs: Use Azure CLI or Portal "Log stream" feature
   - Path: `/home/LogFiles/Application/`

### Log Levels

- **Information**: Normal application flow, startup messages, successful operations
- **Warning**: HTTP 4xx errors, failed login attempts, degraded performance
- **Error**: HTTP 5xx errors, exceptions, database errors
- **Fatal**: Application startup failures, critical errors preventing application start

## Troubleshooting Steps

### Step 1: Check Console Output

Even if file logging fails, console output is always available:

**Local Development:**
```bash
dotnet run
# Watch console output for errors
```

**Azure App Service:**
1. Azure Portal → Your App Service → Monitoring → Log stream
2. Or use Azure CLI:
   ```bash
   az webapp log tail --name <app-name> --resource-group <resource-group>
   ```

### Step 2: Enable Detailed Error Pages

For non-production environments, enable detailed errors:

**appsettings.Development.json:**
```json
{
  "ErrorHandling": {
    "ShowDetailedErrors": true,
    "IncludeStackTrace": true
  }
}
```

**Azure App Service Configuration:**
- Add application setting: `ErrorHandling__ShowDetailedErrors` = `true`

### Step 3: Check Log Directory Permissions

Ensure the application can write to the log directory:

**Local:**
```bash
# Check if directory exists and is writable
ls -la App_Data/Logs/

# Fix permissions if needed
chmod 755 App_Data/Logs/
```

**Azure App Service:**
- Logs are written to `/home/LogFiles/Application/`
- No permission issues typically (managed by Azure)

### Step 4: Verify Database Connectivity

Check database connection logs:

**Local:**
```bash
# Look for database configuration messages
grep "Configuring database" App_Data/Logs/*.log
grep "Connection string" App_Data/Logs/*.log
```

**Common Database Errors:**
- `SqlException`: Connection timeout, authentication failure
- `InvalidOperationException: Connection string 'DefaultConnection' not found`
- `Cannot open database` - Database doesn't exist or is offline

### Step 5: Check Antiforgery Token Issues

HTTP 400 errors from login page often indicate antiforgery validation failures:

**Symptoms:**
- HTTP 400 on form POST
- No exception thrown (middleware rejects request before it reaches your code)

**Logs to Check:**
```bash
grep "Bad Request Details" App_Data/Logs/*.log
grep "antiforgery" App_Data/Logs/*.log -i
```

**Solutions:**
1. Clear browser cookies
2. Ensure HTTPS is configured correctly
3. Check `SameSite` cookie settings
4. Verify antiforgery middleware is configured properly

### Step 6: Review Startup Logs

Check application startup sequence:

**What to Look For:**
```
[Information] Starting OrkinosaiCMS application
[Information] Environment: Production
[Information] Serilog configuration completed successfully
[Information] Registering application services...
[Information] Configuring database with provider: SqlServer
[Information] Service registration completed
[Information] Application built successfully
[Information] Starting database initialization...
[Information] Database initialization completed successfully
[Information] Configuring HTTP request pipeline...
[Information] OrkinosaiCMS application started successfully
```

**If Any Step Fails:**
- Fatal error is logged with detailed exception
- Exception written to both Serilog and Console.Error
- Stack trace and inner exceptions included

### Step 7: Azure-Specific Diagnostics

**Enable Application Logging:**
1. Azure Portal → App Service → App Service logs
2. Set "Application Logging (Filesystem)" to "Error" or "Information"
3. Set Retention Period (days)
4. Save

**View Logs:**
1. Log stream (real-time): Portal → Log stream
2. Download logs: Portal → Advanced Tools → Kudu → Debug console
3. Location: `D:\home\LogFiles\Application\`

**Environment Variables:**
Check if required settings are configured:
```bash
# In Kudu console
env | grep ConnectionStrings
env | grep DefaultAdminPassword
env | grep ErrorHandling
```

## Specific Error Scenarios

### Scenario 1: HTTP 400 on Admin Login (No Logs)

**Likely Cause:** Antiforgery validation failure before logging middleware

**Diagnosis:**
1. Check browser Network tab → Request headers for `RequestVerificationToken`
2. Check console logs for antiforgery warnings
3. Look for StatusCodeLoggingMiddleware messages

**Fix:**
- Clear browser cookies/cache
- Ensure cookies are enabled
- Check HTTPS configuration

### Scenario 2: Application Won't Start (No Logs in File)

**Likely Cause:** Log directory creation failed or Serilog configuration error

**Diagnosis:**
1. Check console output (it's the fallback)
2. Look for "WARNING: Failed to create log directory" message
3. Check stderr output

**Fix:**
- Check disk space
- Verify file system permissions
- Check Serilog configuration in appsettings.json

### Scenario 3: Database Connection Timeout

**Symptoms:**
- SqlException with error number -2 (timeout)
- "A network-related or instance-specific error"

**Diagnosis:**
```bash
grep "SQL connection error" App_Data/Logs/*.log
grep "timeout" App_Data/Logs/*.log -i
```

**Fix:**
1. Verify SQL Server is running
2. Check firewall allows connections
3. Verify connection string is correct
4. Check Azure SQL firewall rules (allow Azure services)

### Scenario 4: Missing Configuration

**Symptoms:**
- Application startup fails immediately
- "Connection string 'DefaultConnection' not found"

**Diagnosis:**
- Check fatal error logs
- Verify appsettings.json exists and is valid

**Fix:**
1. Ensure appsettings.json is deployed
2. Set connection string via environment variable:
   - Azure: Configuration → Connection strings
   - Local: User secrets or environment variables

## Log Analysis Tips

### Finding Errors Quickly

```bash
# Show only errors and fatal messages
grep -E "\[ERR\]|\[FTL\]" App_Data/Logs/*.log

# Show errors for specific date
cat App_Data/Logs/mosaic-backend-20240113.log | grep -E "\[ERR\]|\[FTL\]"

# Find authentication errors
grep "Login" App_Data/Logs/*.log | grep -E "ERR|WRN"

# Find database errors
grep -i "sql\|database\|timeout" App_Data/Logs/*.log | grep ERR
```

### Common Log Patterns

**Successful Login:**
```
[INF] Login attempt for username: admin
[INF] Starting authentication for user: admin
[INF] User admin has role: Administrator
[INF] Authentication successful for user: admin
[INF] Login successful for username: admin
```

**Failed Login (Bad Credentials):**
```
[INF] Login attempt for username: admin
[INF] Starting authentication for user: admin
[WRN] Password verification failed for user: admin
[WRN] Login failed - Invalid credentials for username: admin
```

**Database Error:**
```
[ERR] SQL error during authentication for user: admin - Error: -2
[ERR] SQL connection error occurred while seeding the database...
```

## Prevention Best Practices

1. **Always Enable Console Logging**
   - Fallback when file logging fails
   - Essential for Azure diagnostics

2. **Log Early and Often**
   - Log before any external dependencies (DB, network)
   - Log configuration loading
   - Log service registration steps

3. **Sanitize Sensitive Data**
   - Never log passwords in plain text
   - Sanitize connection strings (mask passwords)
   - Mask user PII in production logs

4. **Use Appropriate Log Levels**
   - Information: Normal flow
   - Warning: Degraded but functional
   - Error: Failures requiring attention
   - Fatal: Application cannot continue

5. **Include Context in Logs**
   - Request ID/Trace ID
   - User information (when available)
   - Timestamp with timezone
   - Machine name (for load-balanced scenarios)

## Support Information

When reporting issues, include:

1. **Request ID** (from error page)
2. **Timestamp** (when error occurred)
3. **Environment** (Development/Production)
4. **Relevant log entries** (sanitized)
5. **Steps to reproduce**
6. **Expected vs actual behavior**

## Related Documentation

- [ADMIN_LOGIN_FIX.md](./ADMIN_LOGIN_FIX.md) - Admin login troubleshooting
- [DEPLOYMENT_NOTES.md](./DEPLOYMENT_NOTES.md) - Deployment configuration
- [docs/appsettings.md](./docs/appsettings.md) - Configuration reference
