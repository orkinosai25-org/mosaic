# Azure App Log Diagnostics Workflow

## Overview

The **Fetch and Diagnose App Errors** workflow is a GitHub Actions workflow designed to automatically collect and analyze Azure Web App logs when deployment or runtime errors occur. This tool helps diagnose issues quickly by fetching comprehensive logs and identifying common error patterns.

## Features

- ✅ **Automated Log Collection**: Fetches application logs, container logs, and activity logs from Azure
- ✅ **Health Check**: Tests the application health endpoint
- ✅ **Error Pattern Detection**: Automatically scans logs for common error patterns
- ✅ **Comprehensive Reports**: Generates diagnostic summaries and error analysis
- ✅ **Artifact Storage**: Stores all logs as GitHub Actions artifacts (30-day retention)
- ✅ **Manual Trigger**: Run on-demand when issues are detected

## When to Use This Workflow

Use this workflow when:

1. **Deployment Fails**: After a deployment that results in HTTP errors (500.30, 503, etc.)
2. **Runtime Errors**: When the application is experiencing errors in production
3. **Performance Issues**: To collect logs for performance analysis
4. **Troubleshooting**: Before opening a support ticket or issue
5. **Post-Deployment Verification**: To verify a deployment was successful

## Prerequisites

### Azure Credentials Setup

The workflow requires Azure credentials to be configured as a GitHub secret:

1. **Create an Azure Service Principal**:
   ```bash
   az ad sp create-for-rbac --name "mosaic-github-actions" \
     --role contributor \
     --scopes /subscriptions/{subscription-id}/resourceGroups/mosaic-rg \
     --sdk-auth
   ```

2. **Add GitHub Secret**:
   - Go to your repository → Settings → Secrets and variables → Actions
   - Create a new secret named `AZURE_CREDENTIALS`
   - Paste the JSON output from the previous command

   The JSON should look like:
   ```json
   {
     "clientId": "<client-id>",
     "clientSecret": "<client-secret>",
     "subscriptionId": "<subscription-id>",
     "tenantId": "<tenant-id>"
   }
   ```

### Required Permissions

The service principal needs these permissions:
- **Reader** on the Resource Group
- **Website Contributor** on the App Service
- **Monitoring Reader** for activity logs

## How to Run the Workflow

### Method 1: GitHub Actions UI (Recommended)

1. Navigate to your repository on GitHub
2. Click on the **Actions** tab
3. In the left sidebar, find and click **"Fetch and Diagnose App Errors"**
4. Click the **"Run workflow"** button (top right)
5. (Optional) Customize inputs:
   - **Time range**: Number of minutes of logs to fetch (default: 60)
   - **App name**: Azure Web App name (default: mosaic-saas)
6. Click **"Run workflow"** to start

### Method 2: GitHub CLI

```bash
gh workflow run "Fetch and Diagnose App Errors" \
  --ref main \
  -f time_range=60 \
  -f app_name=mosaic-saas
```

### Method 3: API Call

```bash
curl -X POST \
  -H "Accept: application/vnd.github+json" \
  -H "Authorization: Bearer $GITHUB_TOKEN" \
  https://api.github.com/repos/orkinosai25-org/mosaic/actions/workflows/fetch-diagnose-app-errors.yml/dispatches \
  -d '{"ref":"main","inputs":{"time_range":"60","app_name":"mosaic-saas"}}'
```

## Understanding the Output

### Workflow Steps

1. **Azure Login**: Authenticates with Azure using service principal
2. **Get App Details**: Retrieves app URL, state, and metadata
3. **Health Check**: Tests the `/api/health` endpoint
4. **Fetch Logs**: Downloads application logs, container logs, and activity logs
5. **Error Analysis**: Scans logs for common error patterns
6. **Create Reports**: Generates diagnostic summary and error analysis
7. **Upload Artifacts**: Saves all logs as downloadable artifacts

### Artifacts Downloaded

After the workflow completes, download the artifacts to access:

| File | Description |
|------|-------------|
| `DIAGNOSTIC_SUMMARY.md` | Overview of the diagnostic run and quick actions |
| `error-analysis.txt` | Automated analysis of detected error patterns |
| `recent-log-stream.txt` | Last 100 lines of application logs |
| `app-logs.zip` | Complete log archive from Azure (if available) |
| `container-logs.txt` | Docker/container logs (if applicable) |
| `activity-logs.json` | Azure activity logs for the time range |
| `app-settings.json` | Application settings (secrets masked) |
| `connection-strings.json` | Connection string names (values masked) |

### Common Error Patterns Detected

The workflow automatically detects these common issues:

| Error Pattern | Meaning | Documentation |
|--------------|---------|---------------|
| **HTTP 500.30** | ASP.NET Core app failed to start | `TROUBLESHOOTING_HTTP_500_30.md` |
| **Database Connection** | Cannot connect to database | `DATABASE_CONNECTION_FIX_SUMMARY.md` |
| **Authentication** | Login/JWT/Identity errors | `AUTHENTICATION_README.md` |
| **Missing Files** | Dependencies or DLLs not found | `DEPLOYMENT_NOTES.md` |
| **Memory Issues** | Out of memory or resource exhaustion | Scale up App Service plan |
| **Configuration** | Missing or invalid app settings | `.env.example`, Azure Portal |

## Workflow Inputs

| Input | Description | Default | Required |
|-------|-------------|---------|----------|
| `time_range` | Minutes of logs to fetch | `60` | No |
| `app_name` | Azure Web App name | `mosaic-saas` | No |

## Troubleshooting the Workflow

### Workflow Fails with "Azure Login Failed"

**Cause**: Invalid or expired Azure credentials

**Solution**:
1. Verify `AZURE_CREDENTIALS` secret exists in GitHub
2. Ensure service principal credentials are correct
3. Check service principal hasn't expired
4. Verify service principal has necessary permissions

### No Logs Collected

**Cause**: Logging not enabled in Azure or insufficient permissions

**Solution**:
1. Enable logging in Azure Portal:
   - Go to App Service → App Service logs
   - Enable "Application Logging (Filesystem)"
   - Set level to "Verbose"
2. Verify service principal has "Reader" permissions

### Health Check Always Fails

**Cause**: Health endpoint not responding or application down

**Solution**:
1. Check if app is running: Azure Portal → App Service → Overview
2. Verify `/api/health` endpoint exists in application
3. Check app logs for startup errors

## Integration with Issue Tracking

### Automated Issue Assignment

When an error is detected, you can assign the issue to Copilot Agent:

1. Create/open an issue with the error details
2. Mention the workflow run URL in the issue description
3. Assign the issue to `@copilot` for AI-powered analysis
4. Copilot can review the artifacts and suggest fixes

### Example Issue Template

```markdown
## Bug: Deployment Error After Latest Release

### Summary
The application is showing errors after the latest deployment.

### Logs
- Workflow run: https://github.com/orkinosai25-org/mosaic/actions/runs/{RUN_ID}
- Artifacts: [Download logs](https://github.com/orkinosai25-org/mosaic/actions/runs/{RUN_ID})

### Error Analysis
{Paste relevant sections from error-analysis.txt}

### Expected Behavior
Application should start successfully and respond to health checks.

### Actual Behavior
HTTP 500.30 error on startup.

---
@copilot please analyze the logs and suggest a fix.
```

## Monitoring Recommendations

### Proactive Monitoring

1. **Enable Azure Application Insights**: For real-time monitoring
2. **Set Up Alerts**: Configure alerts for failed deployments
3. **Automated Runs**: Consider triggering this workflow automatically on deployment failures

### Post-Deployment Verification

After every deployment:
1. Run this workflow to collect baseline logs
2. Save artifacts as deployment records
3. Compare with previous successful deployments

## Cost Considerations

- **Workflow Minutes**: Uses ~2-5 minutes of GitHub Actions time per run
- **Azure Operations**: Minimal cost for log queries and downloads
- **Artifact Storage**: 30-day retention included in GitHub Actions

## Related Documentation

- **Main Troubleshooting**: `TROUBLESHOOTING_HTTP_500_30.md`
- **Deployment Guide**: `DEPLOYMENT_NOTES.md`
- **Error Logging**: `ERROR_LOGGING_TROUBLESHOOTING.md`
- **Authentication Issues**: `AUTHENTICATION_README.md`
- **Database Issues**: `DATABASE_CONNECTION_FIX_SUMMARY.md`

## Advanced Usage

### Custom Time Ranges

Fetch logs from the last 24 hours:
```bash
gh workflow run "Fetch and Diagnose App Errors" -f time_range=1440
```

### Different App Name

Diagnose a staging environment:
```bash
gh workflow run "Fetch and Diagnose App Errors" -f app_name=mosaic-saas-staging
```

### Scheduled Runs

To run this workflow daily (add to `.github/workflows/fetch-diagnose-app-errors.yml`):
```yaml
on:
  schedule:
    - cron: '0 0 * * *'  # Daily at midnight UTC
  workflow_dispatch:
    # ... existing inputs
```

## Support

For issues with the workflow itself:
1. Check GitHub Actions logs for the failed step
2. Verify Azure credentials are correct
3. Open an issue with the workflow run URL
4. Tag `@copilot` for assistance

For application errors detected by the workflow:
1. Download and review the artifacts
2. Follow relevant troubleshooting guides
3. Apply fixes and re-run the workflow to verify
