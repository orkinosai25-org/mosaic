# Troubleshooting: Log File Errors

## Issue: "Database error occurred" with log file path in error message

### Symptoms

When attempting to log in to the admin panel, you may see an error message like:

```
A database error occurred. Please try again later or contact support if the issue persists.
{"Message":"'C:\\home\\site\\wwwroot\\App_Data\\Logs\\mosaic-backend-20251215_001.log' not found."}
```

This error message is misleading - it's **not actually a database error**. The root cause is that the Serilog logging system cannot create or write to log files.

### Root Cause

This issue occurs when:

1. The `App_Data/Logs` directory doesn't exist or can't be created
2. The application doesn't have write permissions to the log directory
3. The file system (especially in Azure App Service) has restrictions
4. A FileNotFoundException or IOException occurs during logging operations

The error appears as a "database error" because:
- The exception occurs during the login process
- The exception handling code was catching all exceptions in a broad way
- File I/O exceptions were not handled separately from database exceptions

### Solution

This issue has been fixed in the codebase with the following changes:

#### 1. Robust Serilog Configuration

The application now wraps the Serilog file sink configuration in a try-catch block. If file logging fails, it automatically falls back to console-only logging:

```csharp
// From Program.cs
builder.Host.UseSerilog((context, services, configuration) =>
{
    try
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            // ... includes File sink if configured
    }
    catch (Exception fileEx)
    {
        // Fall back to console-only logging
        Console.WriteLine($"WARNING: Serilog file sink configuration failed: {fileEx.Message}");
        configuration
            .WriteTo.Console(/* ... */);
    }
});
```

#### 2. Specific Exception Handling in Login

The login page now handles file-related exceptions separately from database exceptions:

```csharp
// From Login.razor
try
{
    // Login logic
}
catch (FileNotFoundException fileEx)
{
    // Handle file not found - don't block login
}
catch (System.IO.IOException ioEx)
{
    // Handle IO errors - don't block login
}
catch (Microsoft.Data.SqlClient.SqlException sqlEx)
{
    // Handle actual database errors
}
```

#### 3. Production Configuration

The production configuration (`appsettings.Production.json`) now uses console-only logging by default to avoid Azure file system issues:

```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ]
  }
}
```

#### 4. Safe Logger Calls

All `Logger.LogError` calls in the login flow are now wrapped in try-catch blocks to prevent logging failures from blocking login:

```csharp
try
{
    Logger.LogError(ex, "Error message");
}
catch
{
    // If logging fails, write to console as fallback
    Console.WriteLine($"Error: {ex.Message}");
}
```

### Impact

After this fix:

- ✅ **Users can log in successfully** even if file logging is unavailable
- ✅ **Clear error messages**: File errors don't appear as database errors
- ✅ **Graceful degradation**: Application continues with console logging if file logging fails
- ✅ **No more misleading error popups** on the login page
- ✅ **The green allow button** (login button) works without errors

### Prevention

To prevent similar issues in the future:

1. **Always use console-only logging in production** (Azure App Service)
2. **Catch specific exception types** rather than broad Exception catches
3. **Never let logging failures block critical operations** like login
4. **Test in environments that match production** (e.g., containers with restricted file systems)

### Manual Fix for Older Deployments

If you're running an older version without this fix, you can work around the issue by:

1. **Update `appsettings.Production.json`** to remove the File sink:
   ```json
   {
     "Serilog": {
       "WriteTo": [
         {
           "Name": "Console"
         }
       ]
     }
   }
   ```

2. **Manually create the log directory** with appropriate permissions (if needed):
   ```bash
   mkdir -p App_Data/Logs
   chmod 755 App_Data/Logs
   ```

3. **Use Azure's built-in logging** instead of file logging:
   - Enable Application Logging in Azure Portal
   - Use Log Stream for real-time logs
   - Consider Application Insights for advanced monitoring

### Related Documentation

- [LOGGING.md](./LOGGING.md) - Complete logging configuration guide
- [PRODUCTION_CONFIGURATION.md](./PRODUCTION_CONFIGURATION.md) - Production deployment guide
- [ERROR_LOGGING_TROUBLESHOOTING.md](../ERROR_LOGGING_TROUBLESHOOTING.md) - General error logging guide

### Azure App Service Best Practices

When deploying to Azure App Service:

1. **Don't rely on file-based logging**: Use Azure's built-in logging infrastructure
2. **Enable Application Insights**: For comprehensive application monitoring
3. **Use Azure Log Stream**: For real-time log viewing
4. **Configure App Service Logs**: Enable Application Logging (Filesystem or Blob)
5. **Monitor with Azure Monitor**: Set up alerts for errors and performance issues

### Testing the Fix

To verify the fix is working:

1. Navigate to the admin login page
2. Enter valid credentials
3. Click the login button (the "green allow button")
4. **Expected behavior**: Login succeeds without any error popups
5. **Expected behavior**: If there's a real database error, you'll see a clear, accurate error message

### Still Having Issues?

If you're still experiencing problems after applying this fix:

1. Check the console logs for any error messages
2. Verify the production configuration is using console-only logging
3. Ensure the database connection string is correct
4. Check Azure App Service logs for detailed error information
5. Open an issue in the GitHub repository with:
   - Error message (full text)
   - Console logs
   - Azure App Service logs
   - Steps to reproduce

---

**Last Updated**: December 15, 2025
**Issue Reference**: Fix for missing log file error causing false "database error" messages
