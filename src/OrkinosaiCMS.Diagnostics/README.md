# OrkinosaiCMS Independent Diagnostic Harness

## Overview

The **OrkinosaiCMS.Diagnostics** service is an independent diagnostic harness designed to provide critical system information and troubleshooting capabilities **even when the main OrkinosaiCMS application fails to start** (e.g., HTTP 500.30 errors).

## Key Features

- ✅ **Runs independently** of the main application on a separate port (5001)
- ✅ **Accessible even during main app failure** - provides diagnostics when main app returns HTTP 500.30
- ✅ **Comprehensive diagnostic information**:
  - Application configuration (with sensitive data redacted)
  - Environment variables (with sensitive data redacted)
  - Recent logs from main application
  - Startup errors and exceptions
  - System information (OS, .NET version, memory, CPU)
  - Health status
- ✅ **User-friendly web dashboard** with tabbed interface
- ✅ **Copy/Download functionality** for all diagnostic sections
- ✅ **Secure access** with optional token-based authentication
- ✅ **Zero dependencies** on main app startup - can gather data from file system directly

## Architecture

The diagnostic service is a minimal ASP.NET Core application that:
1. Runs on port 5001 (configurable)
2. Reads configuration files and logs directly from the main app's file system
3. Provides REST API endpoints for programmatic access
4. Serves a responsive web UI for visual diagnostics

## Quick Start

### Running Locally

```bash
cd src/OrkinosaiCMS.Diagnostics
dotnet run
```

Then open your browser to: http://localhost:5001/diagnostics

### Running in Production

The diagnostics service should be deployed alongside the main application but configured to run on a separate port or subdomain.

#### Option 1: Same Server, Different Port

Configure your reverse proxy (nginx, IIS, Azure App Service) to route `/diagnostics` to the diagnostic service on port 5001.

**Azure App Service Example (web.config):**
```xml
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="Diagnostics Route" stopProcessing="true">
          <match url="^diagnostics(.*)" />
          <action type="Rewrite" url="http://localhost:5001/{R:1}" />
        </rule>
      </rules>
    </rewrite>
  </system.webServer>
</configuration>
```

#### Option 2: Separate App Service / Container

Deploy the diagnostics service as a separate Azure App Service or container:
- Main App: `https://your-app.azurewebsites.net`
- Diagnostics: `https://your-app-diagnostics.azurewebsites.net`

## Configuration

### appsettings.json

```json
{
  "Urls": "http://localhost:5001",
  "MainAppPath": "../OrkinosaiCMS.Web",
  "DiagnosticsSettings": {
    "EnableSecureAccess": false,
    "AccessToken": ""
  }
}
```

### Environment Variables

For production deployments, set these environment variables:

- `Urls` - Port/URL for diagnostics service (default: http://*:5001)
- `MainAppPath` - Path to main application directory (default: /home/site/wwwroot in Azure)
- `DiagnosticsSettings__AccessToken` - Optional access token for secure access

### Azure App Service Configuration

1. Create a new App Service for diagnostics (or use deployment slot)
2. Set environment variables:
   - `MainAppPath` = `/home/site/wwwroot` (or path to main app)
   - `ASPNETCORE_ENVIRONMENT` = `Production`
3. Deploy the OrkinosaiCMS.Diagnostics project
4. Access at: `https://<your-diagnostics-app>.azurewebsites.net`

## API Endpoints

### GET /api/diagnostics/report

Returns comprehensive diagnostic report with all information.

**Response:**
```json
{
  "timestamp": "2025-12-23T17:00:00Z",
  "machineName": "server-01",
  "application": {
    "name": "OrkinosaiCMS",
    "version": "1.0.0",
    "environment": "Production"
  },
  "startup": {
    "isHealthy": false,
    "lastKnownError": "Database connection failed",
    "startupErrors": [...]
  },
  "recentLogs": [...],
  "recentErrors": [...],
  "configuration": {...},
  "environmentVariables": {...},
  "system": {...}
}
```

### GET /api/diagnostics/health

Health check endpoint for the diagnostics service itself.

**Response:**
```json
{
  "status": "Healthy",
  "service": "OrkinosaiCMS.Diagnostics",
  "timestamp": "2025-12-23T17:00:00Z"
}
```

## Web Dashboard

The diagnostic dashboard is accessible at `/diagnostics` and provides:

### Tabs:
1. **📊 Overview** - Application status, version, environment
2. **⚠️ Errors** - Startup errors and recent error logs
3. **📝 Logs** - Recent application logs with syntax highlighting
4. **⚙️ Configuration** - Application settings (sensitive data redacted)
5. **🌍 Environment** - Environment variables (sensitive data redacted)
6. **💻 System** - System information (OS, .NET, memory, CPU)

### Features:
- **Copy to Clipboard** - Each section has a copy button
- **Color-coded Logs** - Errors in red, warnings in yellow, info in blue
- **Responsive Design** - Works on desktop, tablet, and mobile
- **Real-time Status** - Shows if main app is healthy or has issues
- **Beautiful UI** - Modern gradient design with smooth animations

## Security

### Data Redaction

The diagnostics service automatically redacts sensitive information:
- Passwords
- API keys and secrets
- Connection strings
- Client secrets
- OAuth tokens
- Any value matching sensitive patterns

### Access Control

For production, enable secure access:

```json
{
  "DiagnosticsSettings": {
    "EnableSecureAccess": true,
    "AccessToken": "your-secure-token-here"
  }
}
```

When enabled, all requests must include: `Authorization: Bearer <token>`

### Recommendations

1. **Use separate subdomain** - Don't expose diagnostics on main domain
2. **Enable access token** - Always use secure access in production
3. **Monitor access logs** - Track who accesses diagnostics
4. **Regular token rotation** - Change access token periodically
5. **Firewall rules** - Restrict access to admin IPs only

## Troubleshooting

### Main App Path Not Found

If diagnostics can't find the main app:
1. Check `MainAppPath` configuration
2. Ensure diagnostics service has read access to main app directory
3. Verify log directory exists: `{MainAppPath}/App_Data/Logs`

### No Logs Available

If no logs are shown:
1. Check main app is configured with Serilog file sink
2. Verify log directory: `{MainAppPath}/App_Data/Logs`
3. Check file permissions

### Diagnostics Service Won't Start

If the diagnostics service fails:
1. Check port 5001 is not in use
2. View diagnostics service logs: `App_Data/Logs/diagnostics-*.log`
3. Ensure .NET 9.0 SDK is installed

## Use Cases

### Scenario 1: Main App Returns HTTP 500.30

1. Main app fails to start
2. Access diagnostics dashboard: `http://your-app:5001/diagnostics`
3. Check **Errors** tab for startup errors
4. Review **Logs** tab for recent errors
5. Check **Configuration** tab for missing/invalid settings
6. Copy diagnostics data and share with support

### Scenario 2: Database Connection Issues

1. Access diagnostics dashboard
2. Check **Errors** tab for database connection errors
3. Review **Configuration** tab for connection string settings
4. Check **Environment** tab for Azure SQL configuration
5. Verify firewall rules and credentials

### Scenario 3: Post-Deployment Verification

1. Deploy new version
2. Access diagnostics dashboard
3. Verify **Overview** shows correct version
4. Check **Errors** tab is empty
5. Confirm **System** tab shows expected resources

## CI/CD Integration

### Azure DevOps Pipeline

```yaml
- task: DotNetCoreCLI@2
  displayName: 'Publish Diagnostics Service'
  inputs:
    command: 'publish'
    projects: 'src/OrkinosaiCMS.Diagnostics/OrkinosaiCMS.Diagnostics.csproj'
    arguments: '--configuration Release --output $(Build.ArtifactStagingDirectory)/diagnostics'

- task: AzureWebApp@1
  displayName: 'Deploy Diagnostics to Azure'
  inputs:
    azureSubscription: 'Your-Subscription'
    appName: 'your-app-diagnostics'
    package: '$(Build.ArtifactStagingDirectory)/diagnostics'
```

### GitHub Actions

```yaml
- name: Build Diagnostics Service
  run: dotnet publish src/OrkinosaiCMS.Diagnostics/OrkinosaiCMS.Diagnostics.csproj -c Release -o ./diagnostics

- name: Deploy Diagnostics to Azure
  uses: azure/webapps-deploy@v2
  with:
    app-name: 'your-app-diagnostics'
    publish-profile: ${{ secrets.AZURE_DIAGNOSTICS_PUBLISH_PROFILE }}
    package: './diagnostics'
```

## Development

### Project Structure

```
OrkinosaiCMS.Diagnostics/
├── Controllers/
│   └── DiagnosticsController.cs    # API endpoints
├── Models/
│   └── DiagnosticReport.cs         # Data models
├── Pages/
│   ├── Diagnostics.cshtml          # Web UI
│   └── Diagnostics.cshtml.cs       # Page model
├── Services/
│   └── DiagnosticDataService.cs    # Business logic
├── Program.cs                       # App entry point
├── appsettings.json                # Configuration
└── OrkinosaiCMS.Diagnostics.csproj # Project file
```

### Building

```bash
cd src/OrkinosaiCMS.Diagnostics
dotnet build
```

### Testing

```bash
cd src/OrkinosaiCMS.Diagnostics
dotnet run
# Open browser to http://localhost:5001/diagnostics
```

### Adding New Diagnostic Information

1. Add new properties to `DiagnosticReport` model
2. Implement data gathering in `DiagnosticDataService`
3. Update web UI in `Diagnostics.cshtml` to display new data

## Support

For issues or questions:
1. Check diagnostics dashboard for detailed error information
2. Review logs in `App_Data/Logs/diagnostics-*.log`
3. Create an issue on GitHub with diagnostic report attached
4. Contact support@orkinosai.com with diagnostics data

---

**Last Updated**: December 23, 2025
**Version**: 1.0.0
**Status**: Production Ready ✅
