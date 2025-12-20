# Quick Start: Azure App Error Diagnostics

This guide provides a quick reference for using the Azure log diagnostics workflow.

## 🚀 Quick Run

### From GitHub UI

1. Go to [Actions tab](../../actions)
2. Select **"Fetch and Diagnose App Errors"**
3. Click **"Run workflow"** (green button)
4. (Optional) Set time range (default: 60 minutes)
5. (Optional) Set app name (default: mosaic-saas)
6. (Optional) Set resource group name (default: orkinosai_group)
7. Click **"Run workflow"** to start

### From Command Line

```bash
# Using GitHub CLI (with defaults)
gh workflow run "Fetch and Diagnose App Errors"

# With custom time range (120 minutes)
gh workflow run "Fetch and Diagnose App Errors" -f time_range=120

# With custom resource group
gh workflow run "Fetch and Diagnose App Errors" -f resource_group=my-resource-group

# With multiple parameters
gh workflow run "Fetch and Diagnose App Errors" \
  -f time_range=120 \
  -f app_name=mosaic-saas \
  -f resource_group=orkinosai_group
```

## 📥 Download Logs

After the workflow completes:

1. Click on the completed workflow run
2. Scroll to **"Artifacts"** section at the bottom
3. Download **azure-app-logs-{run-number}**
4. Extract the ZIP file

## 📊 What You Get

Inside the artifact:

| File | What to Check |
|------|---------------|
| **error-analysis.txt** | Start here - shows detected error patterns |
| **DIAGNOSTIC_SUMMARY.md** | Overview and quick actions |
| **recent-log-stream.txt** | Last 100 lines of app logs |
| **app-logs.zip** | Complete log archive |

## 🔍 Common Error Patterns

| Error in Logs | Likely Cause | Fix Documentation |
|--------------|--------------|-------------------|
| **HTTP 500.30** | App failed to start | [TROUBLESHOOTING_HTTP_500_30.md](../TROUBLESHOOTING_HTTP_500_30.md) |
| **SqlException** | Database connection issue | [DATABASE_CONNECTION_FIX_SUMMARY.md](../DATABASE_CONNECTION_FIX_SUMMARY.md) |
| **Authentication failed** | JWT/Identity config | [AUTHENTICATION_README.md](../AUTHENTICATION_README.md) |
| **FileNotFoundException** | Missing deployment files | [DEPLOYMENT_NOTES.md](../DEPLOYMENT_NOTES.md) |

## ⚙️ Setup (One-Time)

If the workflow fails to run, you need to set up Azure credentials:

1. **Find your resource group name** (in Azure CLI):
   ```bash
   # List all resource groups to find yours
   az group list --query "[].{Name:name, Location:location}" -o table
   ```

2. **Create Service Principal** (in Azure CLI):
   ```bash
   # Replace {your-sub-id} with your subscription ID
   # Replace {your-resource-group} with your actual resource group name (e.g., orkinosai_group)
   az ad sp create-for-rbac --name "mosaic-github-diagnostics" \
     --role contributor \
     --scopes /subscriptions/{your-sub-id}/resourceGroups/{your-resource-group} \
     --sdk-auth
   ```

3. **Add to GitHub Secrets**:
   - Go to Settings → Secrets and variables → Actions
   - Create secret named `AZURE_CREDENTIALS`
   - Paste the JSON output from step 2

4. **Update workflow defaults** (if needed):
   - Edit `.github/workflows/fetch-diagnose-app-errors.yml`
   - Update the `AZURE_RESOURCE_GROUP` value (line ~21) to match your resource group
   - Commit and push the change

5. **Done!** Run the workflow again

## 📖 Full Documentation

For complete documentation, see:
- **[AZURE_LOG_DIAGNOSTICS.md](./AZURE_LOG_DIAGNOSTICS.md)** - Complete workflow guide
- **[GITHUB_SECRETS_SETUP.md](./GITHUB_SECRETS_SETUP.md)** - Detailed setup instructions

## 💡 Tips

- **After deployment failures**: Run immediately to capture error details
- **Time range**: Use 60 minutes for recent errors, 1440 for full day
- **Artifacts**: Kept for 30 days, download before they expire
- **Copilot help**: Mention the workflow run URL when asking @copilot for help

## 🆘 Troubleshooting

### "Azure Login Failed"
→ Check `AZURE_CREDENTIALS` secret exists and is valid

### "Azure Resource Group Not Found"
→ The resource group name doesn't match your Azure setup
→ Re-run workflow with the correct resource group name, or
→ Update the default in `.github/workflows/fetch-diagnose-app-errors.yml`

### "No logs collected"
→ Enable logging in Azure Portal → App Service → App Service logs

### "Health check failed"
→ Your app might be down, check the error-analysis.txt for details

---

Need more help? See the [full documentation](./AZURE_LOG_DIAGNOSTICS.md) or ask @copilot in an issue.
