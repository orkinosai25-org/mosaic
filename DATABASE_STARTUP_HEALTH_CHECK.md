# Database Startup Health Check - Implementation Guide

## Overview

This document describes the **fail-fast startup mechanism** implemented to prevent OrkinosaiCMS from starting when database migrations have not been applied or the database is inaccessible.

**Problem Solved:** Prior to this implementation, the application would start in a broken state when database migrations were missing, leading to confusing errors when users tried to login. The "green allow button" (Sign In button on `/admin/login`) would be visible but non-functional.

**Solution:** The application now validates the database state during startup and **blocks startup entirely** if critical tables (especially `AspNetUsers`) are missing, ensuring users receive clear error messages instead of experiencing a non-functional admin portal.

---

## How It Works

### 1. Startup Database Validation

During application startup (in `Program.cs`), the following sequence occurs:

1. **Database Migration Check** - Attempts to apply pending migrations automatically (if `Database:AutoApplyMigrations` is enabled)
2. **Database State Validation** - Validates that critical tables exist:
   - `AspNetUsers` (required for admin login)
   - `AspNetRoles` (required for role-based access)
   - `Sites`, `Pages`, `Themes` (core CMS tables)
3. **Startup Decision**:
   - ✅ **If validation passes** → Application starts normally
   - ❌ **If validation fails** → Application startup is **BLOCKED** (throws exception)

### 2. Exception Thrown on Validation Failure

When database validation fails in **Production** or **Development** environments, the application throws an `InvalidOperationException` with a detailed error message:

```
🛑 DATABASE VALIDATION FAILED - Application startup blocked.

Error: Identity tables (AspNetUsers, AspNetRoles) do not exist. Admin login will not work.

The 'green allow button' (Sign In button) cannot function without AspNetUsers table and other Identity tables.

REQUIRED ACTION: Apply database migrations using:
  cd src/OrkinosaiCMS.Infrastructure
  dotnet ef database update --startup-project ../OrkinosaiCMS.Web

Then restart the application.
```

**Important:** In **Testing** environments with InMemory database, validation failures are logged as warnings but do NOT block startup.

---

## Health Check Endpoints

Two health check endpoints are available for monitoring:

### 1. `/api/health` - Full Health Check

Returns detailed health status including all checks.

**Example Response (Unhealthy):**
```json
{
  "status": "Unhealthy",
  "timestamp": "2025-12-18T01:30:00.000Z",
  "duration": "00:00:00.1234567",
  "checks": [
    {
      "name": "database",
      "status": "Unhealthy",
      "description": "Database migrations have not been applied. AspNetUsers table is missing. Admin login will not work.",
      "data": {
        "CanConnect": true,
        "IdentityTablesExist": false,
        "Reason": "Database migrations not applied",
        "RequiredAction": "Run: dotnet ef database update --startup-project src/OrkinosaiCMS.Web"
      },
      "exception": null
    }
  ]
}
```

**Example Response (Healthy):**
```json
{
  "status": "Healthy",
  "timestamp": "2025-12-18T01:30:00.000Z",
  "duration": "00:00:00.0234567",
  "checks": [
    {
      "name": "database",
      "status": "Healthy",
      "description": "Database is healthy and all critical tables exist",
      "data": {
        "CanConnect": true,
        "IdentityTablesExist": true,
        "CoreTablesExist": true
      },
      "exception": null
    }
  ]
}
```

### 2. `/api/health/ready` - Readiness Check

Returns readiness status for orchestrators (Kubernetes, Azure App Service).

**Purpose:** Determines if the application is ready to accept traffic. Only returns `Healthy` if database is fully initialized.

**HTTP Status Codes:**
- `200 OK` - Application is ready, database is healthy
- `503 Service Unavailable` - Application is NOT ready, database validation failed

**Example Response (Not Ready):**
```json
{
  "status": "Unhealthy",
  "ready": false,
  "message": "Application is NOT ready. Database migrations have not been applied. Admin login will fail.",
  "timestamp": "2025-12-18T01:30:00.000Z",
  "checks": [
    {
      "name": "database",
      "status": "Unhealthy",
      "description": "Database migrations have not been applied. AspNetUsers table is missing.",
      "data": {
        "CanConnect": true,
        "IdentityTablesExist": false,
        "Reason": "Database migrations not applied"
      }
    }
  ]
}
```

**Example Response (Ready):**
```json
{
  "status": "Healthy",
  "ready": true,
  "message": "Application is ready to accept traffic. Database is initialized and admin login will work.",
  "timestamp": "2025-12-18T01:30:00.000Z",
  "checks": [
    {
      "name": "database",
      "status": "Healthy",
      "description": "Database is healthy and all critical tables exist",
      "data": {
        "CanConnect": true,
        "IdentityTablesExist": true,
        "CoreTablesExist": true
      }
    }
  ]
}
```

---

## Configuration for Azure App Service

### Readiness Probe Configuration

Configure Azure App Service to use the readiness endpoint:

**Azure Portal:**
1. Navigate to: App Service → Configuration → Health check
2. Enable health check
3. Set Health check path: `/api/health/ready`
4. Set Probe interval: `30` seconds
5. Set Unhealthy threshold: `3` consecutive failures

**ARM Template / Bicep:**
```json
{
  "properties": {
    "siteConfig": {
      "healthCheckPath": "/api/health/ready"
    }
  }
}
```

**Benefits:**
- Azure won't route traffic to unhealthy instances
- Prevents users from seeing broken login pages
- Automatic instance replacement if health check fails persistently

---

## Kubernetes Deployment

### Liveness and Readiness Probes

Add health check probes to your Kubernetes deployment:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: orkinosai-cms
spec:
  template:
    spec:
      containers:
      - name: web
        image: orkinosai-cms:latest
        ports:
        - containerPort: 5000
        livenessProbe:
          httpGet:
            path: /api/health
            port: 5000
          initialDelaySeconds: 30
          periodSeconds: 30
          timeoutSeconds: 5
          failureThreshold: 3
        readinessProbe:
          httpGet:
            path: /api/health/ready
            port: 5000
          initialDelaySeconds: 10
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
```

**Benefits:**
- Pods won't receive traffic until database is ready
- Failed pods are automatically restarted
- Zero-downtime deployments

---

## Troubleshooting

### Scenario 1: Application Won't Start

**Symptoms:**
- Application crashes during startup
- Logs show: `🛑 DATABASE VALIDATION FAILED - Application startup blocked`
- Error message: `AspNetUsers table is missing`

**Root Cause:** Database migrations have not been applied.

**Solution:**
```bash
# Navigate to Infrastructure project
cd src/OrkinosaiCMS.Infrastructure

# Apply all pending migrations
dotnet ef database update --startup-project ../OrkinosaiCMS.Web

# Verify migrations were applied
dotnet ef database update --startup-project ../OrkinosaiCMS.Web --verbose

# Restart the application
```

**Verification:**
```bash
# Check health endpoint
curl http://localhost:5000/api/health

# Should return status: "Healthy"
```

### Scenario 2: Database Connection Failure

**Symptoms:**
- Application crashes during startup
- Logs show: `Cannot connect to database`
- Health check returns: `CanConnect: false`

**Root Cause:** Database server is not accessible or connection string is incorrect.

**Solution:**
1. Verify database server is running:
   ```bash
   # For SQL Server (Windows)
   net start MSSQL$SQLEXPRESS
   
   # For SQL Server (Linux)
   sudo systemctl start mssql-server
   ```

2. Verify connection string in `appsettings.json` or environment variables:
   ```bash
   # Check environment variable (Azure App Service / Kubernetes)
   echo $ConnectionStrings__DefaultConnection
   
   # Update connection string (Azure Portal)
   # App Service → Configuration → Connection strings
   # Name: DefaultConnection
   # Value: Server=...;Database=...;User ID=...;Password=...
   # Type: SQLServer
   ```

3. Test database connectivity:
   ```bash
   # Using sqlcmd
   sqlcmd -S your-server.database.windows.net -U your-username -P your-password -d your-database -Q "SELECT 1"
   ```

### Scenario 3: Health Check Returns "Unhealthy" But App Is Running

**Symptoms:**
- Application started successfully (no crash)
- `/api/health` returns `Unhealthy`
- Users can access the app but login fails

**Note:** This scenario **should not occur** with the new implementation, as startup is blocked when validation fails. If you encounter this, it indicates:
- The app is running in **Testing** environment (InMemory database)
- OR there was a race condition during startup

**Solution:**
1. Check environment:
   ```bash
   echo $ASPNETCORE_ENVIRONMENT
   # Should be: Production or Development (not Testing)
   ```

2. Restart the application to trigger validation again

---

## Code Implementation Details

### Custom Health Check

**File:** `/src/OrkinosaiCMS.Infrastructure/Services/DatabaseHealthCheck.cs`

The `DatabaseHealthCheck` class implements `IHealthCheck` and performs:
1. Database connectivity check (`CanConnectAsync`)
2. Identity tables validation (`AspNetUsers`, `AspNetRoles`)
3. Core tables validation (`Sites`, `Pages`, `Themes`)

**Returns:**
- `HealthCheckResult.Healthy` - All checks passed
- `HealthCheckResult.Unhealthy` - Critical failure (migrations not applied)
- `HealthCheckResult.Degraded` - Minor issues (core tables missing but Identity tables exist)

### Startup Validation

**File:** `/src/OrkinosaiCMS.Web/Program.cs`

**Key Logic (lines 485-545):**
```csharp
var validationResult = await validator.ValidateDatabaseAsync();

if (!validationResult.IsValid)
{
    if (IsTestingEnvironment(...))
    {
        // Log warning but continue startup (Testing only)
    }
    else
    {
        // BLOCK STARTUP - throw exception
        throw new InvalidOperationException("Database validation failed...");
    }
}
```

---

## Best Practices

### Development

1. **Always apply migrations before running the app:**
   ```bash
   dotnet ef database update --startup-project src/OrkinosaiCMS.Web
   dotnet run --project src/OrkinosaiCMS.Web
   ```

2. **Check health status after startup:**
   ```bash
   curl http://localhost:5000/api/health
   ```

### Production Deployment

1. **Apply migrations in deployment pipeline (before app starts):**
   ```bash
   # Azure DevOps / GitHub Actions
   - script: |
       cd src/OrkinosaiCMS.Infrastructure
       dotnet ef database update --startup-project ../OrkinosaiCMS.Web
     displayName: 'Apply Database Migrations'
   ```

2. **Configure health check in orchestrator:**
   - Azure App Service: Set health check path to `/api/health/ready`
   - Kubernetes: Configure readiness and liveness probes
   - Docker Compose: Add healthcheck directive

3. **Monitor health check endpoint:**
   - Set up alerts for `Unhealthy` status
   - Track health check response times
   - Log health check failures

### Automated Testing

1. **Include health check tests:**
   ```csharp
   [Fact]
   public async Task HealthCheck_WithoutMigrations_ReturnsUnhealthy()
   {
       var response = await _client.GetAsync("/api/health");
       var health = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();
       
       Assert.Equal("Unhealthy", health.Status);
   }
   ```

---

## Related Documentation

- **Deployment Guide:** See `DEPLOYMENT_VERIFICATION_GUIDE.md` for step-by-step deployment instructions
- **Database Migrations:** See `src/OrkinosaiCMS.Infrastructure/Migrations/README.md` for migration management
- **Authentication:** See `OQTANE_AUTHENTICATION_VERIFICATION.md` for admin login details

---

## Summary

✅ **Application now fails fast when database is not ready**  
✅ **Clear error messages guide users to fix the issue**  
✅ **Health check endpoints provide detailed status**  
✅ **Orchestrators can prevent traffic to unhealthy instances**  
✅ **The "green allow button" (Sign In) only appears when database is fully initialized**

This implementation ensures a better developer experience and prevents users from encountering confusing errors when the database is not properly initialized.
