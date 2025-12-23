# Diagnostic Framework Documentation

## Overview

The Diagnostic Framework provides a comprehensive system for troubleshooting application issues by exposing configuration, logs, errors, environment variables, and health status through secure admin-only endpoints and a user-friendly UI.

## Features

- **Configuration Viewer**: Display all application settings with automatic redaction of sensitive data
- **Log Viewer**: Browse recent application logs with filtering by level
- **Error Tracker**: View recent errors and exceptions
- **Environment Inspector**: Check environment variables (with security redaction)
- **Health Status**: Monitor application and database health
- **Comprehensive Reports**: Generate complete diagnostic reports for sharing

## Security

All diagnostic endpoints are protected with the following security measures:

1. **Administrator-Only Access**: All endpoints require authentication and the `Administrator` role
2. **Automatic Redaction**: Sensitive data (passwords, secrets, keys, connection strings) are automatically redacted
3. **Audit Logging**: All diagnostic access is logged for security auditing
4. **No Modification**: Read-only access - diagnostics cannot modify application state

## API Endpoints

### 1. Application Status
```
GET /api/diagnostics/status
```

**Authorization**: Administrator role required

**Response**:
```json
{
  "success": true,
  "timestamp": "2025-12-23T16:19:41.621Z",
  "data": {
    "applicationName": "OrkinosaiCMS.Web",
    "version": "1.0.0.0",
    "environment": "Production",
    "dotnetVersion": "10.0.0",
    "osVersion": "Unix 6.8.0.1017",
    "machineName": "web-server-01",
    "uptime": "00:15:23",
    "workingSet": "234 MB",
    "healthStatus": "Healthy",
    "healthChecks": [
      {
        "name": "database",
        "status": "Healthy",
        "description": "Database is accessible",
        "duration": "00:00:00.123"
      }
    ]
  }
}
```

### 2. Configuration
```
GET /api/diagnostics/config
```

**Authorization**: Administrator role required

**Response**:
```json
{
  "success": true,
  "note": "Sensitive values (passwords, secrets, keys) have been redacted for security",
  "timestamp": "2025-12-23T16:19:41.621Z",
  "data": {
    "ConnectionStrings": {
      "DefaultConnection": "***REDACTED***"
    },
    "Serilog": {
      "MinimumLevel": "Information"
    },
    "Authentication": {
      "Jwt": {
        "SecretKey": "***REDACTED***",
        "Issuer": "OrkinosaiCMS"
      }
    }
  }
}
```

### 3. Environment Variables
```
GET /api/diagnostics/environment
```

**Authorization**: Administrator role required

**Response**:
```json
{
  "success": true,
  "note": "Sensitive values (passwords, secrets, keys) have been redacted for security",
  "timestamp": "2025-12-23T16:19:41.621Z",
  "count": 42,
  "data": {
    "ASPNETCORE_ENVIRONMENT": "Production",
    "PATH": "/usr/bin:/bin",
    "ConnectionStrings__DefaultConnection": "***REDACTED***"
  }
}
```

### 4. Recent Logs
```
GET /api/diagnostics/logs?maxLines=100
```

**Parameters**:
- `maxLines` (optional): Number of log lines to return (1-1000, default: 100)

**Authorization**: Administrator role required

**Response**:
```json
{
  "success": true,
  "timestamp": "2025-12-23T16:19:41.621Z",
  "count": 100,
  "maxLines": 100,
  "data": [
    {
      "timestamp": "2025-12-23T16:15:30.123Z",
      "level": "INF",
      "source": "OrkinosaiCMS.Web.Program",
      "message": "Application started successfully",
      "rawLine": "[2025-12-23 16:15:30.123 +00:00] [INF] [OrkinosaiCMS.Web.Program] Application started successfully"
    }
  ]
}
```

### 5. Recent Errors
```
GET /api/diagnostics/errors?maxLines=50
```

**Parameters**:
- `maxLines` (optional): Number of error entries to return (1-500, default: 50)

**Authorization**: Administrator role required

**Response**: Same format as logs endpoint, but filtered to ERROR and FATAL level entries only

### 6. Comprehensive Report
```
GET /api/diagnostics/report
```

**Authorization**: Administrator role required

**Response**: Combines all diagnostic information (status, config, environment, logs, errors) into a single comprehensive report

## Web Interface

### Accessing the Diagnostics Page

1. Log in as an administrator
2. Navigate to `/admin/diagnostics`
3. Use the tab interface to switch between different views:
   - **Status**: Application status and health checks
   - **Configuration**: Application settings
   - **Environment**: Environment variables
   - **Logs**: Recent log entries
   - **Errors**: Recent errors only

### Features

- **Copy to Clipboard**: Each section has a copy button to export data
- **Tabbed Interface**: Easy navigation between different diagnostic views
- **Responsive Design**: Works on desktop and mobile devices
- **Real-time Data**: Refresh the page to get updated diagnostic information
- **Color-coded Logs**: Different log levels are highlighted with colors

## Usage Examples

### CLI with curl

```bash
# Login first to get authentication cookie
curl -c cookies.txt -X POST https://your-app.com/api/authentication/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"YourPassword"}'

# Get application status
curl -b cookies.txt https://your-app.com/api/diagnostics/status

# Get recent errors
curl -b cookies.txt https://your-app.com/api/diagnostics/errors?maxLines=20

# Get comprehensive report
curl -b cookies.txt https://your-app.com/api/diagnostics/report
```

### PowerShell

```powershell
# Login
$loginBody = @{
    username = "admin"
    password = "YourPassword"
} | ConvertTo-Json

$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$login = Invoke-WebRequest -Uri "https://your-app.com/api/authentication/login" `
    -Method POST `
    -Body $loginBody `
    -ContentType "application/json" `
    -SessionVariable session

# Get diagnostics
Invoke-RestMethod -Uri "https://your-app.com/api/diagnostics/status" `
    -WebSession $session
```

## Troubleshooting Common Issues

### Issue: HTTP 401 Unauthorized

**Cause**: User is not authenticated or does not have Administrator role

**Solution**: 
1. Ensure you are logged in as an administrator
2. Check that the user has the "Administrator" role assigned
3. Verify authentication cookies are being sent with requests

### Issue: Empty Logs/Errors

**Cause**: Log files don't exist or are not accessible

**Solution**:
1. Check that the log directory exists: `App_Data/Logs/`
2. Verify application has write permissions to the log directory
3. Check Serilog configuration in `appsettings.json`

### Issue: Redacted Configuration Values

**Cause**: Values contain sensitive keywords and are automatically redacted

**Solution**: This is by design for security. Sensitive values are always redacted. To view actual values, check:
1. Azure App Service Configuration (for production)
2. Local `appsettings.json` files (for development)
3. Environment variables on the server

## Security Best Practices

1. **Limit Access**: Only assign Administrator role to trusted users
2. **Audit Regularly**: Review logs for diagnostic endpoint access
3. **Use HTTPS**: Always access diagnostics over secure connections
4. **Rotate Secrets**: If diagnostics reveal potential security issues, rotate affected secrets
5. **Monitor**: Set up alerts for frequent diagnostic endpoint access

## Integration with Monitoring

The diagnostics framework complements existing monitoring tools:

- **Health Checks**: Use `/api/health` and `/api/health/ready` for automated monitoring
- **Structured Logs**: Export logs to Azure Application Insights, ELK, or Splunk
- **Alerts**: Configure alerts based on error frequency from diagnostics data

## Future Enhancements

Planned features for future releases:

- [ ] Export diagnostics to file (JSON, PDF)
- [ ] Historical health status tracking
- [ ] Performance metrics (CPU, memory over time)
- [ ] Database query profiling
- [ ] Network connectivity tests
- [ ] Custom diagnostic checks

## Related Documentation

- [Health Checks](./HEALTH_CHECKS.md)
- [Logging Configuration](./LOGGING.md)
- [Security Guide](./SECURITY.md)
- [Deployment Guide](./DEPLOYMENT.md)

## Support

For issues or questions about the diagnostic framework:

1. Check this documentation
2. Review application logs
3. Open an issue on GitHub
4. Contact the development team

---

**Last Updated**: December 23, 2025  
**Version**: 1.0.0
