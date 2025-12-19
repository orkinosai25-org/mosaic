# HTTP 503 Service Unavailable Fix - Placeholder Connection String Detection

**Date:** December 19, 2025  
**Issue:** HTTP Error 503 - Service Unavailable due to unconfigured database connection string  
**Status:** ✅ RESOLVED

## Problem Statement

The application fails to start with HTTP 503 Service Unavailable errors when deployed with an unconfigured connection string containing placeholder values. The error manifests as:

```
Service Unavailable
HTTP Error 503. The service is unavailable.
ClientConnectionId:00000000-0000-0000-0000-000000000000
Error Number:11001,State:0,Class:20

Microsoft.Data.SqlClient.SqlException (0x80131904): A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: TCP Provider, error: 0 - No such host is known.)
 ---> System.ComponentModel.Win32Exception (11001): No such host is known.
```

## Root Cause

The production configuration file (`appsettings.Production.json`) contains placeholder connection string values:
- `Server=tcp:YOUR_SERVER.database.windows.net`
- `Initial Catalog=YOUR_DATABASE`
- `User ID=YOUR_USERNAME`
- `Password=YOUR_PASSWORD`

When the application starts, it attempts to connect to these placeholder values, resulting in DNS resolution failure (Error 11001: "No such host is known"). This error is cryptic and doesn't clearly indicate the actual problem - that the connection string hasn't been configured.

## Solution Implemented

### 1. Connection String Validation (Program.cs)

Added early validation to detect placeholder values in the connection string before attempting database connection:

```csharp
// Validate that the connection string doesn't contain placeholder values
// These placeholder values cause "No such host is known" errors (Error 11001)
// which manifest as HTTP 503 Service Unavailable errors
var placeholderPatterns = new[]
{
    "YOUR_SERVER", "YOUR_DATABASE", "YOUR_USERNAME", "YOUR_PASSWORD",
    "yourserver", "yourusername", "yourpassword", "YourDatabase"
};

var foundPlaceholders = placeholderPatterns
    .Where(p => connectionString.Contains(p, StringComparison.OrdinalIgnoreCase))
    .ToList();

if (foundPlaceholders.Any())
{
    var errorMsg = $"CONFIGURATION ERROR: Connection string contains placeholder values: {string.Join(", ", foundPlaceholders)}\n\n" +
        "The connection string in appsettings.Production.json has not been configured with actual database credentials.\n\n" +
        "This causes HTTP 503 Service Unavailable errors with the following error:\n" +
        "  'A network-related or instance-specific error occurred while establishing a connection to SQL Server.\n" +
        "   The server was not found or was not accessible. (provider: TCP Provider, error: 0 - No such host is known.)'\n\n" +
        "REQUIRED ACTIONS:\n" +
        "1. Configure the connection string in Azure App Service:\n" +
        "   - Go to Azure Portal > Your App Service > Configuration > Connection strings\n" +
        "   - Add or update 'DefaultConnection' with your actual SQL Server connection string\n" +
        "   - Click Save and restart the app\n\n" +
        "2. OR update appsettings.Production.json with actual values:\n" +
        "   - Replace 'YOUR_SERVER' with your SQL Server hostname (e.g., 'myserver.database.windows.net')\n" +
        "   - Replace 'YOUR_DATABASE' with your database name\n" +
        "   - Replace 'YOUR_USERNAME' with your SQL username\n" +
        "   - Replace 'YOUR_PASSWORD' with your SQL password\n\n" +
        "3. OR set the environment variable:\n" +
        "   ConnectionStrings__DefaultConnection=<your-actual-connection-string>\n\n" +
        "See AZURE_CONNECTION_STRING_SETUP.md for detailed setup instructions.";
    
    Log.Fatal(errorMsg);
    throw new InvalidOperationException(errorMsg);
}
```

### 2. Error Message Improvements

The validation provides:
- **Clear identification** of the problem (placeholder values detected)
- **Root cause explanation** (connection string not configured)
- **Error correlation** (links to the HTTP 503 and "No such host is known" errors)
- **Three actionable solutions**:
  1. Azure Portal configuration (recommended for production)
  2. appsettings.Production.json update (for file-based config)
  3. Environment variable (for containerized deployments)
- **Reference to documentation** (AZURE_CONNECTION_STRING_SETUP.md)

## Technical Benefits

### Before Fix ❌
- **Cryptic error message**: "No such host is known" - doesn't indicate configuration issue
- **Late failure**: Error occurs during database connection attempt
- **HTTP 503**: Generic service unavailable error without context
- **Transient retry**: EF Core retries connection, wasting time
- **No guidance**: Developers must debug to identify the issue

### After Fix ✅
- **Clear error message**: Explicitly states placeholder values detected
- **Early failure**: Validation occurs before connection attempt
- **Actionable guidance**: Three specific solutions with step-by-step instructions
- **No retry waste**: Application fails fast before attempting connection
- **Self-documenting**: Error message includes configuration examples

## Files Changed

| File | Changes | Purpose |
|------|---------|---------|
| `src/OrkinosaiCMS.Web/Program.cs` | +44 lines | Connection string placeholder validation |
| `HTTP_503_PLACEHOLDER_CONNECTION_STRING_FIX.md` | New file | Documentation of the fix |

**Total Changes:** 2 files, 44 insertions(+)

## Deployment Impact

### Error Scenarios Handled

1. **Placeholder values in appsettings.Production.json**:
   - Old behavior: HTTP 503 with cryptic "No such host is known"
   - New behavior: Clear error message with configuration instructions

2. **Missing connection string**:
   - Already handled with "Connection string 'DefaultConnection' not found"

3. **Invalid hostname** (non-placeholder):
   - Still produces "No such host is known" but only for actual invalid hostnames
   - Validation only catches known placeholder patterns

## Verification Steps

After deploying this fix, the application will:

1. **Detect placeholder values** on startup
2. **Log clear error message** to console and logs
3. **Prevent startup** with descriptive error
4. **Guide administrators** to proper configuration

### Example Error Output

```
[2025-12-19 19:45:00.000 +00:00] [FTL] CONFIGURATION ERROR: Connection string contains placeholder values: YOUR_SERVER, YOUR_DATABASE, YOUR_USERNAME, YOUR_PASSWORD

The connection string in appsettings.Production.json has not been configured with actual database credentials.

This causes HTTP 503 Service Unavailable errors with the following error:
  'A network-related or instance-specific error occurred while establishing a connection to SQL Server.
   The server was not found or was not accessible. (provider: TCP Provider, error: 0 - No such host is known.)'

REQUIRED ACTIONS:
1. Configure the connection string in Azure App Service:
   - Go to Azure Portal > Your App Service > Configuration > Connection strings
   - Add or update 'DefaultConnection' with your actual SQL Server connection string
   - Click Save and restart the app

2. OR update appsettings.Production.json with actual values:
   - Replace 'YOUR_SERVER' with your SQL Server hostname (e.g., 'myserver.database.windows.net')
   - Replace 'YOUR_DATABASE' with your database name
   - Replace 'YOUR_USERNAME' with your SQL username
   - Replace 'YOUR_PASSWORD' with your SQL password

3. OR set the environment variable:
   ConnectionStrings__DefaultConnection=<your-actual-connection-string>

See AZURE_CONNECTION_STRING_SETUP.md for detailed setup instructions.
```

## Related Documentation

- **[AZURE_CONNECTION_STRING_SETUP.md](./AZURE_CONNECTION_STRING_SETUP.md)** - Connection string setup guide
- **[DATABASE_CONNECTION_FIX_SUMMARY.md](./DATABASE_CONNECTION_FIX_SUMMARY.md)** - Security fix for credentials
- **[HTTP_503_CONNECTION_POOL_FIX_SUMMARY.md](./HTTP_503_CONNECTION_POOL_FIX_SUMMARY.md)** - Connection pooling fix
- **[TROUBLESHOOTING_HTTP_500_30.md](./TROUBLESHOOTING_HTTP_500_30.md)** - General troubleshooting

## Best Practices

### Connection String Configuration Priority

1. **Azure App Service (Recommended for Production)**
   - Configure in Azure Portal > App Service > Configuration > Connection strings
   - Overrides all file-based configuration
   - Secure, centralized management
   - No secrets in source control

2. **Environment Variables**
   - Set `ConnectionStrings__DefaultConnection`
   - Good for containerized deployments
   - Works with Docker, Kubernetes

3. **appsettings.Production.json**
   - File-based configuration
   - Requires file system security
   - Suitable when file-based config is preferred

### Placeholder Detection Patterns

The validation detects these common placeholder patterns (case-insensitive):
- `YOUR_SERVER`
- `YOUR_DATABASE`
- `YOUR_USERNAME`
- `YOUR_PASSWORD`
- `yourserver`
- `yourusername`
- `yourpassword`
- `YourDatabase`

These patterns match standard placeholder conventions used in configuration templates.

## Success Criteria

All criteria met ✅

- [x] Placeholder values detected before connection attempt
- [x] Clear error messages with actionable guidance
- [x] Three configuration methods documented
- [x] Build succeeds with no errors
- [x] Error message references existing documentation
- [x] Validation is case-insensitive
- [x] Password sanitization in logs maintained
- [x] No impact on valid connection strings

## Conclusion

This fix addresses the HTTP 503 Service Unavailable error caused by placeholder connection string values. The implementation:

1. ✅ Detects placeholder values early in the startup process
2. ✅ Provides clear, actionable error messages
3. ✅ Prevents cryptic "No such host is known" errors
4. ✅ Guides administrators to proper configuration
5. ✅ Supports multiple configuration methods
6. ✅ Maintains security (password sanitization in logs)

**The application now fails fast with clear guidance when placeholder connection strings are detected, preventing confusing HTTP 503 errors.**

---

**Implementation Date:** December 19, 2025  
**Status:** ✅ Complete and Verified  
**Build Status:** ✅ Successful  
**Error Handling:** ✅ Improved with clear guidance
