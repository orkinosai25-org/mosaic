# Quick Start: Azure App Error Diagnostics

This guide provides a quick reference for using the Azure log diagnostics workflow.

## 🚀 Quick Run

### From GitHub UI

1. Go to [Actions tab](../../actions)
2. Select **"Fetch and Diagnose App Errors"**
3. Click **"Run workflow"** (green button)
4. (Optional) Set time range (default: 60 minutes)
5. Click **"Run workflow"** to start

### From Command Line

```bash
# Using GitHub CLI
gh workflow run "Fetch and Diagnose App Errors"

# With custom time range (120 minutes)
gh workflow run "Fetch and Diagnose App Errors" -f time_range=120
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

1. **Create Service Principal** (in Azure CLI):
   ```bash
   az ad sp create-for-rbac --name "mosaic-github-diagnostics" \
     --role contributor \
     --scopes /subscriptions/{your-sub-id}/resourceGroups/mosaic-rg \
     --sdk-auth
   ```

2. **Add to GitHub Secrets**:
   - Go to Settings → Secrets and variables → Actions
   - Create secret named `AZURE_CREDENTIALS`
   - Paste the JSON output from step 1

3. **Done!** Run the workflow again

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

### "No logs collected"
→ Enable logging in Azure Portal → App Service → App Service logs

### "Health check failed"
→ Your app might be down, check the error-analysis.txt for details

---

Need more help? See the [full documentation](./AZURE_LOG_DIAGNOSTICS.md) or ask @copilot in an issue.
