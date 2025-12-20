# HTTP Error 500.30 - Transient Database Error Fix

## Issue
**Problem:** Application fails to start with HTTP Error 500.30 when Azure SQL Database is temporarily unavailable.

**Error Message:**
```
[2025-12-19 21:57:59.954 +00:00] [INF] [WO0XSDWK0000B6] [5] [Microsoft.EntityFrameworkCore.Infrastructure] 
A transient exception occurred during execution. The operation will be retried after 5000ms.
Microsoft.Data.SqlClient.SqlException (0x80131904): Database 'mosaic-saas' on server 
'orkinosai.database.windows.net' is not currently available. Please retry the connection later.
```

## Root Cause
The application attempts to initialize the database during startup in `Program.cs`. If the Azure SQL Database is temporarily unavailable (transient error), the startup process fails completely, resulting in HTTP 500.30.

This is a common issue with Azure SQL Database which can experience brief periods of unavailability due to:
- Automatic failover operations
- Scaling operations
- Maintenance windows
- Heavy load on shared infrastructure
- Network issues

## Solution
Modified `Program.cs` to detect and gracefully handle transient SQL errors during startup.

### Key Changes

**File:** `src/OrkinosaiCMS.Web/Program.cs`

**Location:** Lines 688-730 (SQL exception handler)

**Implementation:**
1. **Detect Transient Errors:** Identifies Azure SQL transient error codes and message patterns
2. **Graceful Degradation:** Allow application to start in degraded mode
3. **Clear Logging:** Log comprehensive warning message for troubleshooting
4. **Health Check Integration:** Health check reports unhealthy status until DB available

### Transient Error Codes Detected
```csharp
var transientErrorCodes = new[] { 
    40197,  // Service has encountered an error processing your request
    40501,  // The service is currently busy
    40613,  // Database unavailable
    49918,  // Cannot process request. Not enough resources
    49919,  // Too many create or update operations in progress
    49920,  // Too many operations in progress
    4060,   // Cannot open database (database may be starting up)
    4221,   // Login timeout expired
    -2      // Timeout expired
};
```

### Message Pattern Detection
```csharp
var transientMessagePatterns = new[] { 
    "not currently available", 
    "retry the connection", 
    "timeout" 
};
```

## Behavior

### Before Fix
- Application fails to start completely
- Returns HTTP 500.30 error
- No ability to serve requests
- Requires manual restart after database becomes available

### After Fix
- Application starts successfully even if database is temporarily unavailable
- Logs clear warning message with troubleshooting steps
- Health check endpoint (`/api/health`) reports unhealthy status
- Database operations will be retried automatically when connection is restored
- Application can serve static content and health checks
- Graceful degradation instead of complete failure

### Warning Message Logged
```
=== TRANSIENT DATABASE ERROR DETECTED ===

The database is temporarily unavailable (Error 40613)
This is a transient error and the application will continue to start.
Database operations will be retried automatically when the database becomes available.

Health Check Status: UNHEALTHY (until database is available)
Check status at: GET /api/health

If this error persists, check:
  1. Azure SQL Database is running and not paused
  2. Firewall rules allow connections from this IP
  3. Database server is not under heavy load
  4. Connection string credentials are correct

Application will start in degraded mode. Database operations will fail until connection is restored.
===========================================
```

## Testing

### Verification Steps
1. ✅ Build successful (Release configuration)
2. ✅ Unit tests pass
3. ✅ CodeQL security scan: 0 alerts
4. ✅ Code review feedback addressed

### How to Test
1. Temporarily make Azure SQL Database unavailable (pause or firewall block)
2. Start the application
3. Verify application starts successfully (no HTTP 500.30)
4. Check `/api/health` - should report unhealthy status
5. Restore database availability
6. Verify health check becomes healthy
7. Verify database operations work again

## Observability

### Health Check
```bash
curl http://localhost:5000/api/health
```

**Response when database unavailable:**
```json
{
  "status": "Unhealthy",
  "timestamp": "2025-12-19T22:00:00Z",
  "checks": [
    {
      "name": "database",
      "status": "Unhealthy",
      "description": "Database is not accessible. Check connection string..."
    }
  ]
}
```

### Logs
Check application logs for the transient error warning message. The application will log:
- Initial connection error with error code
- Transient error detection message
- Troubleshooting steps
- Status information

## Deployment Considerations

### Azure App Service
- Configure health check endpoint: `/api/health/ready`
- Set appropriate health check interval (recommended: 60 seconds)
- Consider configuring startup probe with longer timeout for initial database connection

### Kubernetes
```yaml
livenessProbe:
  httpGet:
    path: /api/health
    port: 8080
  initialDelaySeconds: 30
  periodSeconds: 10
  
readinessProbe:
  httpGet:
    path: /api/health/ready
    port: 8080
  initialDelaySeconds: 60
  periodSeconds: 10
```

### Monitoring
Set up alerts for:
- Health check failures lasting > 5 minutes
- Transient error frequency exceeding threshold
- Extended periods in degraded mode

## Benefits

1. **Resilience:** Application survives transient database failures
2. **Availability:** Maintains uptime during brief database unavailability
3. **Observability:** Clear logging and health check status
4. **User Experience:** No HTTP 500.30 errors for end users
5. **Operations:** Reduces manual intervention needed
6. **Cost:** Reduces unnecessary restarts and support escalations

## Related Documentation
- `TROUBLESHOOTING_HTTP_500_30.md` - General HTTP 500.30 troubleshooting
- `DATABASE_STARTUP_HEALTH_CHECK.md` - Database health check details
- `DEPLOYMENT_VERIFICATION_GUIDE.md` - Deployment best practices
- `DATABASE_MIGRATION_RECOVERY_SUMMARY.md` - Database migration handling

## Azure SQL Best Practices
1. Enable automatic retry in connection string (already configured with `EnableRetryOnFailure`)
2. Use connection pooling (already configured)
3. Monitor Azure SQL DTU/CPU usage
4. Set up Azure SQL alerts for availability issues
5. Configure proper timeout values
6. Consider using Azure SQL elastic pool for consistent performance

## Notes
- This fix only handles **transient** errors (temporary unavailability)
- Non-transient errors (e.g., missing tables, invalid credentials) still cause startup failure
- This is intentional to prevent starting in a broken state
- Error code 208 (Invalid object name 'AspNetUsers') still blocks startup as it indicates missing migrations

## Pull Request
- **Branch:** `copilot/fix-http-error-500-30-again`
- **Commits:**
  - `d125ca5` - Handle transient database errors during startup
  - `922fd94` - Improve code quality based on review feedback

## Author
@copilot
Date: 2025-12-19
