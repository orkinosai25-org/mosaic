# Mosaic CMS - Diagnostics Script

## Overview

The `diagnostics.ps1` script is a comprehensive diagnostic tool designed to help platform engineers and Copilot Agent rapidly collect and diagnose application issues, even when the main application is non-functional.

## Features

### 1. **System Information**
- Operating system details
- Runtime environment (.NET CLR version)
- Machine specifications (CPU, memory)
- Detects Azure App Service (Kudu) environment
- Detects GitHub Actions environment

### 2. **Configuration Analysis**
- Scans and parses all `appsettings.json` files
- Extracts database configuration
- Identifies Azure Blob Storage settings
- Checks Stripe payment configuration
- **Automatically masks sensitive data** (passwords, keys, secrets)

### 3. **Environment Variables**
- Lists all environment variables
- Identifies and masks sensitive variables
- Useful for debugging configuration overrides

### 4. **Comprehensive Log Collection**
- Gathers logs from multiple locations:
  - `App_Data/Logs/`
  - `LogFiles/`
  - Azure App Service log directories
- Extracts recent errors and exceptions
- Identifies stack traces for crash analysis
- Supports `.log`, `.txt`, and `.xml` files

### 5. **Event Log Analysis** (Windows only)
- Collects recent application errors from Windows Event Log
- Filters for ASP.NET and .NET related events

### 6. **Connectivity Tests**
- **Database Connectivity**: Tests SQL Server connection using configured connection string
- **Azure Blob Storage**: Validates blob storage configuration
- **Network Diagnostics**: Provides troubleshooting hints for connection failures

### 7. **Stripe Payment Validation**
- Checks for Stripe API keys (publishable, secret, webhook)
- Identifies test vs. live mode
- Masks sensitive key values

### 8. **Deployment Information**
- Git commit hash and branch
- Azure deployment ID (if applicable)
- Build metadata

### 9. **Crash Analysis**
- Scans logs for stack traces
- Extracts exception details
- Identifies patterns in application crashes

### 10. **Health Check Summary**
- Consolidated view of all checks
- Color-coded status indicators (✓ Pass, ⚠ Warning, ✗ Fail)
- Overall health assessment (Healthy, Degraded, Unhealthy)

### 11. **Report Generation**
- **Text Report**: Comprehensive plain-text diagnostic report
- **HTML Report**: Beautiful, interactive HTML report with color-coded sections

## Usage

### Basic Usage

Run full diagnostics with default settings:

```powershell
cd scripts
.\diagnostics.ps1
```

### Generate HTML Report

```powershell
.\diagnostics.ps1 -GenerateHtmlReport
```

### Custom Output Location

```powershell
.\diagnostics.ps1 -OutputPath "C:\Diagnostics\Reports" -GenerateHtmlReport
```

### Skip Connectivity Tests

Useful when external services are unavailable or to speed up diagnostics:

```powershell
.\diagnostics.ps1 -SkipConnectivityTests
```

### Skip Log Collection

Useful when logs are too large or not needed:

```powershell
.\diagnostics.ps1 -SkipLogCollection
```

### Combined Options

```powershell
.\diagnostics.ps1 -OutputPath "C:\temp\diag" -GenerateHtmlReport -SkipConnectivityTests
```

### Get Help

```powershell
.\diagnostics.ps1 -Help
Get-Help .\diagnostics.ps1 -Full
```

## Running in Different Environments

### 1. **Local Development**

```powershell
# From repository root
cd scripts
.\diagnostics.ps1 -GenerateHtmlReport

# View the HTML report
start diagnostic-output\diagnostic-report-*.html
```

### 2. **Azure App Service (Kudu Console)**

1. Navigate to your App Service in Azure Portal
2. Open **Advanced Tools (Kudu)** → **Debug Console** → **PowerShell**
3. Navigate to site directory:
   ```powershell
   cd D:\home\site\wwwroot\scripts
   ```
4. Run diagnostics:
   ```powershell
   .\diagnostics.ps1 -GenerateHtmlReport
   ```
5. Download reports from `diagnostic-output` folder

### 3. **GitHub Actions**

Add to your workflow file (`.github/workflows/diagnose.yml`):

```yaml
name: Run Diagnostics

on:
  workflow_dispatch:  # Manual trigger
  schedule:
    - cron: '0 */6 * * *'  # Run every 6 hours

jobs:
  diagnostics:
    runs-on: windows-latest
    
    steps:
      - name: Checkout repository
        uses: actions/checkout@v4
      
      - name: Run Diagnostics
        shell: pwsh
        run: |
          cd scripts
          .\diagnostics.ps1 -GenerateHtmlReport -SkipConnectivityTests
      
      - name: Upload Diagnostic Reports
        uses: actions/upload-artifact@v4
        with:
          name: diagnostic-reports
          path: scripts/diagnostic-output/
          retention-days: 7
```

### 4. **Scheduled Diagnostics**

Create a scheduled task to run diagnostics regularly:

```powershell
# Windows Task Scheduler
$action = New-ScheduledTaskAction -Execute "PowerShell.exe" `
    -Argument "-File C:\path\to\mosaic\scripts\diagnostics.ps1 -GenerateHtmlReport"

$trigger = New-ScheduledTaskTrigger -Daily -At 2am

Register-ScheduledTask -TaskName "Mosaic-Diagnostics" `
    -Action $action -Trigger $trigger `
    -Description "Daily diagnostics for Mosaic CMS"
```

## Output Files

### Directory Structure

```
diagnostic-output/
├── diagnostic-report-20251223-143022.txt
└── diagnostic-report-20251223-143022.html
```

### Text Report Format

```
================================================================================
MOSAIC CMS - DIAGNOSTIC REPORT
================================================================================
Generated: 2025-12-23 14:30:22
Machine: WEBAPP-01
Environment: Production

================================================================================
SYSTEM INFORMATION
================================================================================
...

================================================================================
APPLICATION CONFIGURATION FILES
================================================================================
...
```

### HTML Report Features

- **Responsive design** with professional styling
- **Color-coded sections** (green = success, yellow = warning, red = error)
- **Collapsible sections** for easy navigation
- **JSON-formatted data** in code blocks
- **Quick navigation** with table of contents
- **Mobile-friendly** layout

## Security & Privacy

### Sensitive Data Protection

The diagnostics script automatically masks sensitive information:

- **Connection strings**: Passwords are masked (`password=********`)
- **API keys**: First 8-12 characters visible, rest masked
- **Secrets**: Completely masked or truncated
- **Tokens**: Masked in environment variables and configuration

### Safe to Share

The generated reports are safe to share with:
- Support teams
- GitHub issues (for debugging)
- Internal stakeholders

However, always review reports before sharing externally.

## Troubleshooting

### Script Execution Policy

If you encounter "execution policy" errors:

```powershell
# Temporarily bypass (current session only)
PowerShell -ExecutionPolicy Bypass -File .\diagnostics.ps1

# Or set for current user
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Permission Issues

If the script cannot access certain directories or logs:

1. Run PowerShell as Administrator
2. Check Azure App Service permissions (should work in Kudu)
3. Verify file system paths exist

### Database Connection Failures

If database connectivity test fails:

1. Check connection string in `appsettings.json`
2. Verify SQL Server is running and accessible
3. Check firewall rules (Azure SQL requires allowlisting)
4. Ensure credentials are correct

### Missing Logs

If no logs are found:

1. Check if application has been running (logs won't exist if app hasn't started)
2. Verify log paths in `appsettings.json` (`Serilog` section)
3. Ensure write permissions to log directory
4. Check if logs are being written to a different location

## Enhancements Roadmap

Future improvements to be added when investigating issues:

- [ ] **Self-Healing Capabilities**
  - Auto-restart stuck services
  - Clear cache when issues detected
  - Reset connection pools

- [ ] **Advanced Health Checks**
  - API endpoint testing
  - Frontend health (React app)
  - External service dependencies

- [ ] **Performance Metrics**
  - Memory usage analysis
  - CPU utilization
  - Slow query detection

- [ ] **Log Analysis AI**
  - Pattern recognition in errors
  - Suggested fixes based on error patterns
  - Correlation of errors across logs

- [ ] **Integration Tests**
  - End-to-end workflow testing
  - User authentication flow
  - Payment processing validation

- [ ] **Automated Alerting**
  - Email/Slack notifications
  - Azure Monitor integration
  - PagerDuty integration

## Contributing

When using this script for investigations:

1. **Document new issues discovered** → Add checks to detect them
2. **Identify missing diagnostics** → Extend the script
3. **Find manual fixes** → Automate them in the script
4. **Update this README** with new usage patterns

## Related Documentation

- [Azure App Service Diagnostics](https://docs.microsoft.com/azure/app-service/overview-diagnostics)
- [Troubleshooting Guide](../TROUBLESHOOTING_HTTP_500_30.md) (if available)
- [Deployment Verification](../DEPLOYMENT_VERIFICATION_GUIDE.md) (if available)
- [Scripts Documentation](./README.md)

## Support

For issues with the diagnostics script itself:

1. Check this README for troubleshooting steps
2. Review the script output for error messages
3. Open an issue on GitHub: https://github.com/orkinosai25-org/mosaic/issues
4. Tag with `diagnostics` and `tooling` labels

---

**Version**: 1.0.0  
**Last Updated**: 2025-12-23  
**Maintainer**: Copilot Agent & Platform Engineering Team
