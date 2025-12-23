# Diagnostics Framework - Quick Start Guide

## What is it?

The Diagnostic Framework helps you troubleshoot application issues by providing:
- ✅ View all configurations
- 📋 Browse recent logs
- ❌ Filter recent errors
- 🌍 Check environment variables
- 🏥 Monitor health status
- 📊 Generate comprehensive reports

## Quick Access

### Web UI (Recommended for Most Users)

1. **Login** as administrator at `/admin/login`
2. **Navigate** to `/admin/diagnostics`
3. **Browse** tabs to view different diagnostic information
4. **Copy** any section using the copy button

### API (For Automation/Scripts)

```bash
# 1. Login (save cookies)
curl -c cookies.txt -X POST https://your-app.com/api/authentication/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"YourPassword"}'

# 2. Get diagnostics
curl -b cookies.txt https://your-app.com/api/diagnostics/status
curl -b cookies.txt https://your-app.com/api/diagnostics/config
curl -b cookies.txt https://your-app.com/api/diagnostics/logs
curl -b cookies.txt https://your-app.com/api/diagnostics/errors
```

## Common Scenarios

### Scenario 1: App Won't Start

**Use**: Status endpoint to check health

```bash
curl -b cookies.txt https://your-app.com/api/diagnostics/status
```

**Look for**:
- `healthStatus`: Should be "Healthy"
- `healthChecks`: Check if database is healthy

### Scenario 2: Configuration Issue

**Use**: Config endpoint to verify settings

```bash
curl -b cookies.txt https://your-app.com/api/diagnostics/config
```

**Look for**:
- Connection strings are configured
- Required settings are present
- No placeholder values (e.g., "YOUR_SERVER")

### Scenario 3: Runtime Errors

**Use**: Errors endpoint to see recent failures

```bash
curl -b cookies.txt https://your-app.com/api/diagnostics/errors?maxLines=20
```

**Look for**:
- Error messages and stack traces
- Patterns in error timing
- Affected components

### Scenario 4: Environment Problems

**Use**: Environment endpoint to check variables

```bash
curl -b cookies.txt https://your-app.com/api/diagnostics/environment
```

**Look for**:
- Required environment variables set
- Correct environment (Production/Development)
- Azure configuration present

### Scenario 5: Complete Troubleshooting Report

**Use**: Report endpoint for everything

```bash
curl -b cookies.txt https://your-app.com/api/diagnostics/report > diagnostic-report.json
```

**Share** the report with your team or support

## Security Notes

✅ **Safe to Use**:
- All endpoints are read-only
- Sensitive data is automatically redacted
- Only administrators can access

⚠️ **Important**:
- Don't share reports publicly (may contain server info)
- Review redacted data before sharing
- Use HTTPS for all requests

## Troubleshooting Access

### Can't Access Web UI

1. Verify you're logged in: `/admin/login`
2. Check you have Administrator role
3. Clear browser cache and try again

### Can't Access API

1. Ensure you login first to get cookies
2. Check cookies are being sent with requests
3. Verify user has Administrator role

### Empty Data

**Logs/Errors Empty**:
- Check log directory exists: `App_Data/Logs/`
- Verify logging is configured in `appsettings.json`

**Config Empty**:
- Check application is running
- Verify appsettings.json is present

## Need More Help?

- 📖 Full documentation: [DIAGNOSTICS_FRAMEWORK.md](./DIAGNOSTICS_FRAMEWORK.md)
- 🐛 Report issues: GitHub Issues
- 💬 Ask the team: Development team contact

---

**Quick Links**:
- Web UI: `/admin/diagnostics`
- API Status: `/api/diagnostics/status`
- Health Check: `/api/health`
