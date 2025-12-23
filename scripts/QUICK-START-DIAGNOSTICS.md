# QUICK START: Run Diagnostics NOW

If your app is broken and you need immediate diagnostics, follow these steps:

## Option 1: Run Locally (FASTEST - 30 seconds)

```powershell
# Clone/navigate to repo
cd /path/to/mosaic

# Run diagnostics with HTML report
pwsh scripts/diagnostics.ps1 -GenerateHtmlReport

# View the report
start scripts/diagnostic-output/diagnostic-report-*.html
```

## Option 2: Run in GitHub Actions (2 minutes)

1. Go to: https://github.com/orkinosai25-org/mosaic/actions/workflows/run-diagnostics.yml
2. Click "Run workflow" button
3. Select options:
   - ✅ Generate HTML report: Yes
   - Skip connectivity tests: No (unless app is completely down)
4. Wait ~1 minute
5. Download artifacts from the workflow run

## Option 3: Run in Azure Kudu Console (3 minutes)

1. Go to Azure Portal → Your App Service
2. Advanced Tools (Kudu) → Debug Console → PowerShell
3. Navigate: `cd D:\home\site\wwwroot\scripts`
4. Run: `.\diagnostics.ps1 -GenerateHtmlReport`
5. Download reports from `diagnostic-output` folder

---

## What the Script Checks

✅ **System & Environment**
- OS, runtime, Azure/GitHub environment
- All environment variables (with masking)

✅ **Configuration**
- All appsettings.json files
- Connection strings (masked)
- Azure Blob Storage config
- Stripe payment config

✅ **Logs & Errors**
- Application logs from multiple locations
- Windows Event Logs (ASP.NET errors)
- Recent stack traces and crashes

✅ **Connectivity**
- SQL Server database connection test
- Azure Blob Storage validation
- Stripe API key verification

✅ **Health Summary**
- Color-coded status for each check
- Overall health assessment (Healthy/Degraded/Unhealthy)

---

## Common Issues & Quick Fixes

### 🔴 Database Connection Failed

**Symptoms in report:**
- ✗ Database Connectivity: Fail
- Error: "network-related" or "login failed"

**Quick Fix:**
```powershell
# Check connection string
$env:ConnectionStrings__DefaultConnection

# Test manually
sqlcmd -S "your-server.database.windows.net" -U "username" -P "password" -Q "SELECT 1"
```

**Common causes:**
1. Firewall not allowing your IP
2. Wrong credentials
3. Connection string typo
4. SQL Server not running

### 🔴 App Logs Show Stack Traces

**Symptoms in report:**
- ✗ Crash Stack Traces: Found
- Recent errors in App_Data/Logs/

**Quick Fix:**
```powershell
# View most recent log
Get-Content (Get-ChildItem src/OrkinosaiCMS.Web/App_Data/Logs/*.log | Sort-Object LastWriteTime -Descending | Select-Object -First 1).FullName -Tail 100
```

**Common causes:**
1. Missing configuration (check appsettings.json)
2. Database migration needed
3. Missing environment variables
4. Dependencies not installed

### 🔴 HTTP 500/503 Errors

**Symptoms in report:**
- ✗ Recent Errors: Found
- Event log shows HTTP errors

**Quick Fix:**
```powershell
# Check IIS/Kestrel logs
Get-Content src/OrkinosaiCMS.Web/LogFiles/http-*.log -Tail 50

# Restart app service (Azure)
az webapp restart --name mosaic-saas --resource-group orkinosai_group
```

**Common causes:**
1. App startup failure
2. Connection pool exhaustion
3. Memory pressure
4. Configuration error

### 🟡 Configuration Files Missing

**Symptoms in report:**
- ✗ Configuration Files: Missing
- appsettings.json not found

**Quick Fix:**
```powershell
# Verify files exist
ls src/OrkinosaiCMS.Web/*.json

# Copy from example (if needed)
cp src/OrkinosaiCMS.Web/appsettings.Production.json src/OrkinosaiCMS.Web/appsettings.json
```

### 🟡 Blob Storage Not Configured

**Symptoms in report:**
- ⊘ Azure Blob Storage: Skipped
- No connection string configured

**Quick Fix:**
```powershell
# Set environment variable
$env:AzureBlobStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=mosaicsaas;AccountKey=YOUR_KEY;EndpointSuffix=core.windows.net"

# Or add to appsettings.json in Azure Portal Configuration
```

---

## Interpreting the Health Summary

### ✓ Healthy (Green)
- All critical checks passed
- App should be operational
- May have warnings but no failures

**Action:** Monitor for issues

### ⚠ Degraded (Yellow)
- Some non-critical checks failed
- App may be operational but with reduced functionality
- Warnings that need attention

**Action:** Review warnings and plan fixes

### ✗ Unhealthy (Red)
- Critical checks failed
- App likely non-functional
- Immediate attention required

**Action:** Follow quick fixes above or escalate

---

## Getting More Help

### 1. Share the Report
The HTML report is safe to share (sensitive data is masked):
- Upload to GitHub issue
- Share with team via Slack/email
- Attach to support ticket

### 2. Check Existing Issues
- Search: https://github.com/orkinosai25-org/mosaic/issues
- Look for similar error patterns
- Check troubleshooting docs

### 3. Create New Issue
Include:
- Diagnostic report (HTML or text)
- Description of the problem
- Steps to reproduce
- Expected vs actual behavior

### 4. Emergency Contacts
- Platform Engineering Team: [Add contact info]
- On-call rotation: [Add rotation link]
- Azure Support: [Add support link]

---

## Advanced: Automated Diagnostics

### Schedule Regular Health Checks

Add to crontab (Linux) or Task Scheduler (Windows):

```bash
# Run daily at 2 AM
0 2 * * * cd /path/to/mosaic && pwsh scripts/diagnostics.ps1 -GenerateHtmlReport -OutputPath /var/diagnostics/$(date +\%Y\%m\%d)
```

### GitHub Actions on Schedule

Uncomment these lines in `.github/workflows/run-diagnostics.yml`:

```yaml
schedule:
  - cron: '0 */6 * * *'  # Run every 6 hours
```

### Azure App Insights Integration

Future enhancement: Automatic alerting when diagnostics detect issues.

---

**Last Updated:** 2025-12-23  
**Script Version:** 1.0.0
