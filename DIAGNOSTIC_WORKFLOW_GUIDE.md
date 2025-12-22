# Azure App Service Diagnostic Workflow Guide

## Overview

The **Fetch and Diagnose App Errors** workflow is an automated tool that helps you quickly identify and diagnose HTTP 500.30 startup errors in your Azure App Service deployment.

## When to Use This Workflow

Use this workflow when:
- ✅ Your app shows HTTP Error 500.30 (ASP.NET Core app failed to start)
- ✅ The app is deployed but not responding correctly
- ✅ You need to see startup logs and errors
- ✅ You want to quickly diagnose deployment issues

## How to Run the Workflow

### Step 1: Navigate to the Workflow

1. Go to your GitHub repository
2. Click on the **Actions** tab
3. Find "Fetch and Diagnose App Errors" in the left sidebar
4. Click on it to open the workflow

**Direct link:** `https://github.com/YOUR-ORG/YOUR-REPO/actions/workflows/fetch-diagnose-app-errors.yml`

### Step 2: Trigger the Workflow

1. Click the **"Run workflow"** dropdown button (top right)
2. Configure parameters (optional):
   - **Time range**: How far back to look for logs (default: 60 minutes)
   - **App name**: Azure Web App name (default: mosaic-saas)
   - **Resource group**: Azure Resource Group (default: orkinosai_group)
3. Click **"Run workflow"** to start
4. The workflow will begin immediately

### Step 3: Monitor Progress

1. Wait for the workflow to complete (typically 1-2 minutes)
2. Watch the steps execute:
   - ✓ Azure Login
   - ✓ Get App Details
   - ✓ Health Check
   - ✓ Fetch Logs
   - ✓ Extract Startup Errors (NEW!)
   - ✓ Analyze Errors
   - ✓ Upload Artifacts

### Step 4: Review Results

#### In the Workflow Summary

The workflow creates a comprehensive summary with:
- 🔥 **Extracted startup errors** displayed directly in the summary
- Application health status
- Error analysis results
- Direct links to troubleshooting documentation

**Key indicators:**
- 🔥 "Startup Errors Detected" = Critical errors found in stdout logs
- ⚠️ "Errors Detected" = Common error patterns found
- ✅ "No Errors Detected" = No obvious errors (may need manual review)

#### In the Artifacts

1. Click on the workflow run
2. Scroll to the bottom to find "Artifacts"
3. Download **azure-app-logs-[run-number]**
4. Extract the ZIP file

**Priority files to check** (in order):
1. 🔥 **`startup-errors-extracted.txt`** - START HERE! Contains the actual startup error
2. **`latest-stdout.log`** - Complete stdout log with full context
3. **`error-analysis.txt`** - Automated error pattern detection
4. **`recent-log-stream.txt`** - Recent application logs (500 lines)
5. **`DIAGNOSTIC_SUMMARY.md`** - Complete diagnostic report

## What the Workflow Collects

### 1. Stdout Logs (Most Important!)
- **Location in Azure**: `D:\home\LogFiles\stdout\`
- **What it contains**: Startup errors, unhandled exceptions, configuration issues
- **Extracted automatically**: Yes! See `startup-errors-extracted.txt`

### 2. Application Logs
- **Location in Azure**: `D:\home\LogFiles\Application\`
- **What it contains**: Runtime application logs, Serilog output
- **File**: `recent-log-stream.txt`

### 3. Container Logs
- **Location in Azure**: Docker/Container logs
- **What it contains**: Container-level issues, deployment problems
- **File**: `container-logs.txt`

### 4. Azure Activity Logs
- **What it contains**: Azure resource-level events
- **File**: `activity-logs.json`

### 5. Configuration
- **What it contains**: App settings and connection string names (values masked)
- **Files**: `app-settings.json`, `connection-strings.json`

## Understanding the Results

### Scenario 1: Startup Errors Detected 🔥

**Workflow Summary Shows:**
```
🔥 Startup Errors Detected

Critical startup errors were extracted from stdout logs:

[Error details displayed here]
```

**What to do:**
1. Read the error message in the summary
2. Download artifacts and open `startup-errors-extracted.txt`
3. Follow the specific error guidance (see Common Errors below)
4. Apply the fix
5. Restart the app or redeploy

### Scenario 2: Health Check Failed ❌

**Workflow Summary Shows:**
```
Health: unhealthy
```

**What to do:**
1. Check if startup errors were extracted
2. If no startup errors, review `recent-log-stream.txt`
3. Check database connectivity
4. Verify environment variables in Azure Portal

### Scenario 3: No Errors Detected ✅

**Workflow Summary Shows:**
```
✅ No Common Errors Detected
```

**What to do:**
1. App might be healthy or have intermittent issues
2. Review logs manually for warnings
3. Check if the issue is reproducible
4. Consider enabling more verbose logging

## Common Startup Errors and Solutions

### Error: "Connection string 'DefaultConnection' not found"

**Cause:** Missing database connection string

**Solution:**
1. Azure Portal → App Service → Configuration → Connection strings
2. Add `DefaultConnection` with your SQL Server connection string
3. Save and restart

### Error: "Invalid object name 'AspNetUsers'"

**Cause:** Database migrations not applied

**Solution:**
```bash
cd src/OrkinosaiCMS.Infrastructure
dotnet ef database update --startup-project ../OrkinosaiCMS.Web
```

### Error: "HTTP 500.30"

**Cause:** Generic startup failure

**Solution:**
1. Check stdout logs for specific error
2. Verify all environment variables are set
3. Ensure database is accessible
4. Check .NET runtime is installed

### Error: "No such host is known"

**Cause:** Invalid SQL Server hostname in connection string

**Solution:**
1. Verify SQL Server name is correct
2. Check firewall rules allow connections
3. Ensure connection string uses correct format

## Advanced Usage

### Custom Time Range

To look further back in time:
1. Run workflow
2. Set "Time range" to a larger value (e.g., 180 for 3 hours)
3. Useful for finding intermittent issues

### Different App/Resource Group

To diagnose a different deployment:
1. Run workflow
2. Enter custom "App name" and "Resource group"
3. Useful for staging vs production comparisons

### Running via API

You can trigger the workflow via GitHub API:
```bash
curl -X POST \
  -H "Authorization: Bearer YOUR_GITHUB_TOKEN" \
  -H "Accept: application/vnd.github+json" \
  -H "X-GitHub-Api-Version: 2022-11-28" \
  https://api.github.com/repos/OWNER/REPO/actions/workflows/fetch-diagnose-app-errors.yml/dispatches \
  -d '{"ref":"main","inputs":{"time_range":"60"}}'
```

## Troubleshooting the Workflow Itself

### Issue: "Azure Login Failed"

**Cause:** Invalid or expired Azure credentials

**Solution:**
1. Check `AZURE_CREDENTIALS` secret in GitHub repository
2. Verify service principal credentials are correct
3. See the workflow summary for detailed fix guide

### Issue: "Resource Group Not Found"

**Cause:** Wrong resource group name

**Solution:**
1. Run `az group list` to see all resource groups
2. Update workflow default or re-run with correct name

### Issue: "Could not retrieve kudu credentials"

**Cause:** Insufficient permissions or deployment user not configured

**Solution:**
1. Verify service principal has proper role (Contributor or Website Contributor)
2. Check app service allows deployment via SCM

### Issue: "Stdout logs not found"

**Cause:** Logging not enabled or no logs generated yet

**Solution:**
1. Enable Application Logging in Azure Portal
2. Restart the app to generate fresh logs
3. Ensure `stdoutLogEnabled="true"` in web.config

## Best Practices

### 1. Run Immediately After Deployment
- Run the workflow right after each deployment
- Catch issues early before users are affected
- Validate deployment success

### 2. Keep Logs for Reference
- Download and archive artifacts for important deployments
- Compare logs between successful and failed deployments
- Track error trends over time

### 3. Enable Verbose Logging
Before running diagnostics:
- Set Application Logging to "Verbose" in Azure Portal
- This provides maximum detail in stdout logs
- Don't forget to reduce to "Information" after debugging

### 4. Check Both Staging and Production
- Run diagnostics on both environments
- Compare differences
- Identify environment-specific issues

### 5. Document Recurring Issues
- If you see the same error repeatedly, document the solution
- Add to team knowledge base
- Update application configuration to prevent recurrence

## Integration with CI/CD

You can integrate this workflow into your deployment pipeline:

```yaml
- name: Deploy to Azure
  # ... deployment steps ...

- name: Verify Deployment Health
  uses: actions/github-script@v6
  with:
    script: |
      // Trigger diagnostic workflow
      await github.rest.actions.createWorkflowDispatch({
        owner: context.repo.owner,
        repo: context.repo.repo,
        workflow_id: 'fetch-diagnose-app-errors.yml',
        ref: 'main'
      });
```

## Related Documentation

- [TROUBLESHOOTING_HTTP_500_30.md](./TROUBLESHOOTING_HTTP_500_30.md) - Detailed troubleshooting steps
- [DEPLOYMENT_NOTES.md](./DEPLOYMENT_NOTES.md) - Deployment configuration guide
- [ERROR_LOGGING_TROUBLESHOOTING.md](./ERROR_LOGGING_TROUBLESHOOTING.md) - Logging configuration
- [HTTP_500_30_FIX_SUMMARY.md](./HTTP_500_30_FIX_SUMMARY.md) - Summary of HTTP 500.30 fixes

## Support

If you encounter issues with the diagnostic workflow:

1. Check that Azure credentials are valid
2. Verify you have necessary permissions
3. Review the workflow run logs for specific errors
4. Consult the troubleshooting section above

For application-specific errors, use the extracted logs and follow the troubleshooting guides linked above.

---

**Last Updated:** December 22, 2024  
**Workflow Version:** Enhanced with stdout error extraction  
**Status:** ✅ Production Ready
