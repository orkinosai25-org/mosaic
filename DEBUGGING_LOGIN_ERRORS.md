# Quick Reference Guide - Debugging Login HTTP 400 Errors

## Overview

This guide helps you quickly identify and resolve HTTP 400 errors during admin login using the comprehensive logging system.

## Where to Find Logs

### Local Development
- **Console**: Real-time output while running `dotnet run`
- **File**: `src/OrkinosaiCMS.Web/App_Data/Logs/mosaic-backend-YYYYMMDD.log`

### Azure App Service
- **Portal**: Azure Portal → Your App Service → Monitoring → Log stream
- **CLI**: `az webapp log tail --name mosaic-saas --resource-group <your-rg>`
- **File**: Access via Kudu at `/home/LogFiles/Application/`

## Quick Diagnosis Steps

### Step 1: Find the Error in Logs

Search for the TraceId from the error page or the timestamp when the error occurred:

```bash
# Search by TraceId
grep "0HNHPVKBUSI8M:00000001" mosaic-backend-*.log

# Search by time range (adjust time)
grep "2025-12-13 03:56" mosaic-backend-*.log
```

### Step 2: Look for These Key Indicators

#### A. Antiforgery Token Failure (Most Common)

**Look for:**
```
[WRN] => POST Security [TraceId] - HasAntiforgeryCookie: False
[WRN] [Microsoft.AspNetCore.Antiforgery] Antiforgery token validation failed
[WRN] HTTP 400 - POST /admin/login
```

**Cause**: Missing or invalid antiforgery token

**Solutions:**
- User cleared cookies → Ask user to refresh page and try again
- HTTPS misconfiguration → Check HTTPS redirect is working
- Cookie SameSite issues → Review cookie settings in Program.cs

#### B. Model Validation Error

**Look for:**
```
[WRN] Model validation failed [TraceId] - Error count: 2
[WRN] Model validation error [TraceId] - Field: Username, Error: Username is required
```

**Cause**: Form data not properly submitted or validated

**Solutions:**
- Check JavaScript errors preventing form submission
- Verify EditForm component is configured correctly
- Check DataAnnotations on LoginModel

#### C. Database Connection Error

**Look for:**
```
[ERR] [UserService] Error in VerifyPasswordAsync for username: admin - Type: Microsoft.Data.SqlClient.SqlException
[ERR] SQL error during authentication - ErrorNumber: -2, State: 0, Server: ...
[ERR] Message: Timeout expired...
```

**Cause**: Database unavailable or timeout

**Solutions:**
- Check database server status
- Verify connection string
- Check Azure SQL firewall rules (allow Azure services)
- Check database service tier (may be paused/scaling)

#### D. User Not Found or Inactive

**Look for:**
```
[WRN] [UserService] User not found by username: admin
[WRN] [AuthenticationService] Password verification failed for user: admin
```

or

```
[WRN] User account is inactive: admin (UserId: 1)
```

**Cause**: User doesn't exist or is deactivated

**Solutions:**
- Verify user exists in database
- Check IsActive flag in Users table
- Re-run database seeding if needed

#### E. Role Assignment Issues

**Look for:**
```
[WRN] [UserService] No roles found for userId: 1
[WRN] User admin has no roles assigned, using default 'User' role
```

**Cause**: User has no role assignments

**Solutions:**
- Check UserRoles table for assignments
- Re-run database seeding
- Manually assign Administrator role

## Common Error Patterns

### Pattern 1: Request Never Reaches HandleLogin

**Logs show:**
```
[INF] => Incoming Request [TraceId] - POST /admin/login
[WRN] <= Response [TraceId] - 400 POST /admin/login - Elapsed: 5ms
```

**Notice:** No "HandleLogin method called" log entry

**Diagnosis:** Request rejected by middleware (antiforgery, routing, etc.)

**Next Steps:** Check antiforgery logs and middleware configuration

### Pattern 2: HandleLogin Called but Service Fails

**Logs show:**
```
[INF] HandleLogin method called - Username: admin
[INF] Calling AuthService.LoginAsync for username: admin
[ERR] SQL error during authentication...
```

**Diagnosis:** Database connectivity or query issue

**Next Steps:** Check database health, connection string, firewall rules

### Pattern 3: Authentication Succeeds but No Redirect

**Logs show:**
```
[INF] Authentication successful for user: admin
[INF] Login successful for username: admin, redirecting to /admin
[INF] Navigation initiated for username: admin
```

**Diagnosis:** Logs show success but user sees error

**Next Steps:** Check browser console for JavaScript errors, verify /admin route exists

## Log Correlation

Each request has a unique TraceId. Follow it through the logs:

```
1. [INF] => Incoming Request [abc123] - POST /admin/login
2. [INF] [abc123] Login page initialized
3. [INF] [abc123] HandleLogin method called
4. [INF] [abc123] AuthService.LoginAsync called
5. [INF] [abc123] UserService.VerifyPasswordAsync called
6. [INF] [abc123] Password verification result: True
7. [INF] [abc123] Authentication successful
8. [INF] <= Response [abc123] - 200 POST /admin/login
```

## Useful Log Queries

### Find all login attempts
```bash
grep "HandleLogin method called" mosaic-backend-*.log
```

### Find failed logins
```bash
grep -E "Login failed|Password verification failed" mosaic-backend-*.log
```

### Find HTTP 400 errors
```bash
grep "HTTP 400" mosaic-backend-*.log
```

### Find antiforgery issues
```bash
grep -i "antiforgery" mosaic-backend-*.log
```

### Find database errors
```bash
grep -E "SqlException|timeout|connection.*database" mosaic-backend-*.log
```

### Find specific user's activity
```bash
grep "Username: admin" mosaic-backend-*.log
```

## Adjusting Log Verbosity

If logs are too verbose for production:

**Edit appsettings.Production.json** (or set environment variables):

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "OrkinosaiCMS": "Information"  // Keep our app logs verbose
      }
    }
  }
}
```

This reduces ASP.NET Core framework logs but keeps application logs detailed.

## Emergency Debugging

If you need even MORE detail temporarily:

**Set environment variable** (Azure App Service → Configuration):
```
ASPNETCORE_ENVIRONMENT=Development
```

This enables:
- Developer exception page
- Detailed error messages
- Stack traces in browser

**WARNING:** Only use in non-production for security reasons!

## Support Checklist

When reporting an issue, include:

1. ✅ TraceId from error page
2. ✅ Timestamp when error occurred
3. ✅ Username attempted
4. ✅ Relevant log entries (30 lines before/after error)
5. ✅ Environment (Local, Azure, etc.)
6. ✅ Browser and version
7. ✅ Steps to reproduce

## Example: Debugging a Real HTTP 400

**User reports:** "Can't login, getting HTTP 400"

**Step 1:** Get details
- When? "2025-12-13 15:30 UTC"
- Username? "admin"
- Environment? "Azure production"

**Step 2:** Pull logs
```bash
az webapp log tail --name mosaic-saas --resource-group rg-mosaic | tee login-debug.log
```

**Step 3:** Find the request
```bash
grep "2025-12-13 15:3" login-debug.log | grep "/admin/login"
```

**Step 4:** Identify TraceId
```
[2025-12-13 15:30:15] [INF] => Incoming Request [0HNQ8R9S2KPML:00000005] - POST /admin/login
```

**Step 5:** Follow TraceId
```bash
grep "0HNQ8R9S2KPML:00000005" login-debug.log
```

**Output:**
```
[INF] => Incoming Request [0HNQ8R9S2KPML:00000005] - POST /admin/login
[INF] => POST Security [0HNQ8R9S2KPML:00000005] - HasAntiforgeryCookie: False
[WRN] Antiforgery token validation failed
[WRN] <= Response [0HNQ8R9S2KPML:00000005] - 400 POST /admin/login
```

**Diagnosis:** Missing antiforgery cookie → User's browser not accepting cookies

**Solution:** Ask user to enable cookies or try different browser

## Conclusion

With comprehensive logging, every HTTP 400 error now has a clear diagnostic trail. Use this guide to quickly identify root causes and resolve issues.

For detailed information about the logging implementation, see `COMPREHENSIVE_LOGIN_LOGGING.md`.
