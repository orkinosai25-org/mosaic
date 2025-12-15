# Serilog Logging Guide

This document describes the logging implementation for OrkinosaiCMS using Serilog.

## Overview

OrkinosaiCMS uses Serilog for comprehensive logging of errors, exceptions, and diagnostic information. Logs are written to both console and rotating file sinks, making it easy for developers and operations teams to troubleshoot issues.

## Log File Location

By default, log files are written to:

```
src/OrkinosaiCMS.Web/App_Data/Logs/
```

Log files follow the naming convention:
```
mosaic-backend-YYYYMMDD.log
```

For example:
- `mosaic-backend-20231210.log` - Logs for December 10, 2023
- `mosaic-backend-20231211.log` - Logs for December 11, 2023

### Log Rotation and Retention

- **Rolling Interval**: Daily (new file created at midnight)
- **Retention Period**: 7 days (older log files are automatically deleted)
- **File Size Limit**: 10 MB per file (will roll over if size is exceeded)

## Log Levels

Serilog supports the following log levels (from least to most severe):

1. **Verbose** - Most detailed level, includes all trace information
2. **Debug** - Detailed debugging information
3. **Information** - General informational messages about application flow
4. **Warning** - Warnings about potential issues that aren't errors
5. **Error** - Error events that don't prevent the application from running
6. **Fatal** - Critical errors that cause the application to fail

### Default Configuration

| Environment | Default Level | Description |
|------------|---------------|-------------|
| **Production** | Error | Only logs errors and fatal issues to minimize log volume |
| **Development** | Information | Logs informational messages and above for better debugging |

## Changing Log Level

You can change the log level without recompiling the application by modifying the appropriate configuration file:

### For Development Environment

Edit `appsettings.Development.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",  // Change this to: Verbose, Debug, Information, Warning, Error, or Fatal
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore.Database.Command": "Information"
      }
    }
  }
}
```

### For Production Environment

Edit `appsettings.Production.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Error",  // Change this to desired level
      "Override": {
        "Microsoft.AspNetCore": "Warning"
      }
    }
  }
}
```

### Main Configuration

The complete logging configuration is in `appsettings.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Error",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "App_Data/Logs/mosaic-backend-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ]
  }
}
```

**Note**: After changing the configuration, restart the application for changes to take effect.

## Log Format

Each log entry includes the following information:

```
[Timestamp] [Level] [MachineName] [ThreadId] [SourceContext] Message
Exception Stack Trace (if applicable)
```

Example log entry:
```
[2023-12-10 14:32:15.123 +00:00] [ERR] [WEBSERVER01] [42] [OrkinosaiCMS.Web.Controllers.SiteController] Failed to provision site: SiteName=MyNewSite, AdminEmail=admin@example.com
System.InvalidOperationException: Site URL 'my-new-site' is already in use.
   at OrkinosaiCMS.Infrastructure.Services.SiteService.ProvisionSiteAsync(String siteName, String adminEmail, String description, Nullable`1 themeId) in /app/src/OrkinosaiCMS.Infrastructure/Services/SiteService.cs:line 167
   at OrkinosaiCMS.Web.Controllers.SiteController.CreateSite(CreateSiteDto dto) in /app/src/OrkinosaiCMS.Web/Controllers/SiteController.cs:line 148
```

## What Gets Logged

### Site Creation and Management

All site creation operations are logged with full context:

- **Success**: Site provisioning completion (Info level in dev, not logged in prod by default)
- **Errors**: Full exception details including:
  - Exception message
  - Stack trace
  - Inner exceptions
  - Request parameters (site name, admin email, theme ID)

### Unhandled Exceptions

All unhandled exceptions throughout the application are automatically captured with:
- Complete stack trace
- Request information (URL, method, user)
- Environment context (machine name, thread ID)

### HTTP Requests

HTTP request logging includes:
- Request method and path
- Response status code
- Request duration
- Remote IP address
- Request host

## Reviewing Logs for Troubleshooting

### Locating Log Files

1. Navigate to the application directory
2. Open `App_Data/Logs/` folder
3. Open the log file for the relevant date

### Finding Specific Errors

**Using Command Line (Linux/Mac):**
```bash
# Find all errors in today's log
grep "\[ERR\]" mosaic-backend-20231210.log

# Find errors related to site creation
grep -A 10 "CreateSite" mosaic-backend-20231210.log | grep -A 10 "\[ERR\]"

# Search for specific exception types
grep -A 20 "InvalidOperationException" mosaic-backend-20231210.log
```

**Using Command Line (Windows PowerShell):**
```powershell
# Find all errors in today's log
Select-String -Path "mosaic-backend-20231210.log" -Pattern "\[ERR\]"

# Find errors related to site creation
Select-String -Path "mosaic-backend-20231210.log" -Pattern "CreateSite" -Context 0,10 | Where-Object { $_.Context.PostContext -match "\[ERR\]" }
```

**Using Text Editor:**
1. Open log file in your preferred text editor
2. Search for `[ERR]` to find error entries
3. Search for `[FTL]` to find fatal errors
4. Look for exception stack traces (they typically span multiple lines)

### Common Error Patterns

**Site Creation Failures:**
```
Search for: "Failed to provision site" or "Error creating site"
Look for: InvalidOperationException, SqlException, EntityFrameworkException
```

**Database Connection Issues:**
```
Search for: "database" or "SqlException"
Look for: Connection timeout, authentication failures
```

**Validation Errors:**
```
Search for: "BadRequest" or "validation"
Look for: Missing required fields, invalid data
```

## Advanced Configuration

### Adjusting File Size Limits

In `appsettings.json`, modify the `fileSizeLimitBytes`:

```json
{
  "Args": {
    "fileSizeLimitBytes": 10485760,  // 10 MB (default)
    "rollOnFileSizeLimit": true
  }
}
```

### Adjusting Retention Period

Change `retainedFileCountLimit`:

```json
{
  "Args": {
    "retainedFileCountLimit": 7  // Keep 7 days (default)
  }
}
```

### Adding Custom Enrichers

Additional context can be added by configuring enrichers in `appsettings.json`:

```json
{
  "Enrich": [
    "FromLogContext",
    "WithMachineName",
    "WithThreadId",
    "WithEnvironmentUserName"  // Add Windows/Linux username
  ]
}
```

## Future Enhancements

### Admin UI Log Viewer (Planned)

A future enhancement will add an admin panel feature to:
- View recent logs in the browser
- Filter by date, level, and source
- Search log content
- Download log files
- Monitor real-time log streams

This feature will make troubleshooting even more accessible without requiring file system access.

### Structured Logging Extensions

Future versions may include:
- Integration with Application Insights
- Elasticsearch/Kibana integration
- Seq server support for structured log querying
- Slack/Teams notifications for critical errors

## Best Practices

1. **Don't log sensitive data**: Avoid logging passwords, connection strings, or personal information
2. **Use appropriate log levels**: Don't use Error for informational messages
3. **Include context**: When logging errors, include relevant parameters and state
4. **Review logs regularly**: Check logs during development and after deployments
5. **Monitor disk space**: Ensure adequate space for log files
6. **Secure log files**: Restrict access to log files in production (contains diagnostic information)

## Troubleshooting Logging Issues

### Logs Not Being Created

1. **Check directory permissions**: Ensure the application has write access to `App_Data/Logs/`
2. **Verify configuration**: Check that `appsettings.json` has correct Serilog configuration
3. **Check log level**: Ensure the minimum level allows the messages you expect to see
4. **Review startup logs**: Check console output for Serilog initialization errors

**Note on Azure App Service Deployments**: If file logging fails (e.g., due to permissions or missing directories), the application automatically falls back to console-only logging. This ensures the application continues to run even if file logging is unavailable. In production environments (Azure), it's recommended to use console logging and rely on Azure's built-in log streaming and Application Insights for log collection.

### Automatic Fallback to Console Logging

The application is configured with a robust fallback mechanism:

1. **Startup**: Attempts to create `App_Data/Logs` directory if it doesn't exist
2. **File Sink Initialization**: If file logging fails, automatically switches to console-only logging
3. **Runtime Errors**: Logger errors are caught and logged to console to prevent disrupting application functionality
4. **Production Configuration**: Production environment (`appsettings.Production.json`) uses console-only logging by default

This design ensures that **logging issues never prevent users from using the application**. The login page and other critical functions will continue to work even if file logging is unavailable.

### Production Logging Recommendations

For production deployments (especially on Azure App Service):

1. **Use console-only logging**: The default production configuration writes to console only
2. **Enable Azure App Service Logs**: Navigate to Azure Portal > App Service > App Service logs > Enable Application Logging
3. **Use Azure Log Stream**: View real-time logs via Azure Portal or Azure CLI
4. **Consider Application Insights**: For advanced logging, monitoring, and analytics
5. **Avoid file-based logging**: File systems in Azure App Service may have permission restrictions

Example production configuration (already configured in `appsettings.Production.json`):

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

### Log Files Too Large

1. Reduce `retainedFileCountLimit` to keep fewer days
2. Decrease `fileSizeLimitBytes` to roll over more frequently
3. Increase log level to Warning or Error to reduce volume
4. Add more specific overrides to filter out noisy components

### Missing Stack Traces

1. Ensure `appsettings.json` has `{Exception}` in the output template
2. Check that exceptions are being logged with `.LogError(ex, message)` syntax
3. Verify that error handling code is not swallowing exceptions

## Support

For issues related to logging:
1. Check this documentation
2. Review the log files for configuration errors
3. Check application startup logs in the console
4. Consult the [Serilog documentation](https://serilog.net/)
5. Open an issue in the project repository

---

**Last Updated**: December 2023
**Version**: 1.0
