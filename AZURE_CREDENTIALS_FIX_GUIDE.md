# Azure Credentials Fix Guide

## Problem Summary

The workflow run [#20392348388](https://github.com/orkinosai25-org/mosaic/actions/runs/20392348388) failed to authenticate with Azure, showing this error:

```
AADSTS7000215: Invalid client secret provided. 
Ensure the secret being sent in the request is the client secret value, 
not the client secret ID, for a secret added to app.
```

This means the `AZURE_CREDENTIALS` GitHub secret contains an expired or invalid Azure service principal client secret. Until this is fixed, the diagnostic workflow cannot fetch logs or diagnose issues with your Azure Web App.

## What You Need to Fix

You need to update the `AZURE_CREDENTIALS` secret in your GitHub repository with fresh Azure service principal credentials.

## Step-by-Step Fix Instructions

### Step 1: Create or Update Azure Service Principal

1. **Open Azure Portal** and sign in to [https://portal.azure.com](https://portal.azure.com)

2. **Navigate to Azure Active Directory**:
   - Search for "Azure Active Directory" in the top search bar
   - Click on **Azure Active Directory** from the results

3. **Go to App Registrations**:
   - In the left sidebar, click **App registrations**
   - Find your existing service principal (or create a new one if needed)
   - If creating new: Click **+ New registration**
     - Name: `mosaic-github-actions` (or similar)
     - Supported account types: Single tenant
     - Click **Register**

4. **Note the Application (client) ID and Directory (tenant) ID**:
   - On the Overview page, copy these values:
     - **Application (client) ID**
     - **Directory (tenant) ID**
   - Keep these handy for later

### Step 2: Create a New Client Secret

1. **In your App Registration**, click **Certificates & secrets** in the left sidebar

2. **Create a new client secret**:
   - Click **+ New client secret**
   - Description: `GitHub Actions - Created [Current Date]`
   - Expires: Choose duration (recommended: 12-24 months)
   - Click **Add**

3. **IMMEDIATELY COPY THE SECRET VALUE**:
   - ⚠️ **CRITICAL**: Copy the **Value** (not the Secret ID!)
   - This value is only shown once and cannot be retrieved later
   - Store it temporarily in a secure location

### Step 3: Assign Required Permissions

1. **Navigate to your Resource Group**:
   - Search for "Resource groups" in the Azure Portal
   - Find your resource group (e.g., `mosaic-rg` for this project)
   - Click on it

2. **Add role assignment**:
   - Click **Access control (IAM)** in the left sidebar
   - Click **+ Add** → **Add role assignment**
   - Role: Select **Contributor** (or **Website Contributor** for web app-specific access)
   - Members: Search for your service principal by name
   - Click **Review + assign**

### Step 4: Get Your Subscription ID

1. **Find your Subscription ID**:
   - Click on **Subscriptions** in the Azure Portal (search bar)
   - Find your subscription
   - Copy the **Subscription ID**

### Step 5: Create the Credentials JSON

Create a JSON object with the following structure (replace the placeholders with your actual values):

```json
{
  "clientId": "YOUR_APPLICATION_CLIENT_ID",
  "clientSecret": "YOUR_NEW_CLIENT_SECRET_VALUE",
  "subscriptionId": "YOUR_SUBSCRIPTION_ID",
  "tenantId": "YOUR_DIRECTORY_TENANT_ID"
}
```

**Example** (with fake values):
```json
{
  "clientId": "12345678-1234-1234-1234-123456789abc",
  "clientSecret": "abc123~DEF456_ghi789.JKL012",
  "subscriptionId": "87654321-4321-4321-4321-987654321xyz",
  "tenantId": "11111111-2222-3333-4444-555555555555"
}
```

### Step 6: Update GitHub Secret

1. **Go to your GitHub repository**: `https://github.com/YOUR_ORG/YOUR_REPO` (for this project: [orkinosai25-org/mosaic](https://github.com/orkinosai25-org/mosaic))

2. **Navigate to Settings**:
   - Click **Settings** tab
   - Click **Secrets and variables** → **Actions** in the left sidebar

3. **Update the AZURE_CREDENTIALS secret**:
   - Find `AZURE_CREDENTIALS` in the list of secrets
   - Click the **Update** button (pencil icon)
   - Paste the entire JSON object from Step 5
   - Click **Update secret**

### Step 7: Verify the Fix

1. **Run the diagnostic workflow again**:
   - Go to **Actions** tab in your GitHub repository
   - Find the **Fetch and Diagnose App Errors** workflow
   - Click **Run workflow** button
   - Select `main` branch
   - Click **Run workflow**

2. **Check the results**:
   - Wait for the workflow to complete
   - The Azure Login step should now succeed (green checkmark ✅)
   - The workflow will fetch and analyze logs from your Azure Web App
   - Download the artifacts to review the diagnostic results

## Common Issues and Troubleshooting

### Issue: "Invalid client secret" persists

**Solutions**:
- Double-check you copied the **secret value**, not the secret ID
- Ensure there are no extra spaces or line breaks in the JSON
- The secret value is case-sensitive
- Try creating a brand new client secret

### Issue: "Unauthorized" or "Forbidden" errors

**Solutions**:
- Verify the service principal has **Contributor** role on the resource group
- Check that you're using the correct subscription ID
- Ensure the service principal isn't expired or disabled

### Issue: "Tenant not found" or "Invalid tenant"

**Solutions**:
- Verify the `tenantId` matches your Azure AD Directory ID
- Check that the service principal exists in the correct Azure AD tenant

### Issue: JSON format errors

**Solutions**:
- Validate your JSON using [jsonlint.com](https://jsonlint.com)
- Ensure all strings are in double quotes
- No trailing commas in the JSON object
- The JSON must be on a single line or properly formatted

## Security Best Practices

1. **Rotate secrets regularly**: Update client secrets every 6-12 months
2. **Use minimal permissions**: Grant only the permissions needed (Website Contributor is better than Contributor for web app-only access)
3. **Never commit secrets**: Keep credentials only in GitHub Secrets, never in code
4. **Monitor usage**: Review Azure Activity logs for unusual service principal activity
5. **Document expiration dates**: Set calendar reminders before secrets expire

## Additional Resources

- [Azure Login Action Documentation](https://github.com/Azure/login#configure-a-service-principal-with-a-secret)
- [Azure Service Principal Documentation](https://learn.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal)
- [GitHub Actions Secrets Documentation](https://docs.github.com/en/actions/security-guides/encrypted-secrets)
- [Azure RBAC Documentation](https://learn.microsoft.com/en-us/azure/role-based-access-control/overview)

## Quick Reference Checklist

- [ ] Created or located Azure service principal
- [ ] Generated new client secret (saved the VALUE, not ID)
- [ ] Copied Application (client) ID
- [ ] Copied Directory (tenant) ID  
- [ ] Copied Subscription ID
- [ ] Assigned Contributor role to service principal on resource group
- [ ] Created credentials JSON with all 4 values
- [ ] Updated AZURE_CREDENTIALS secret in GitHub
- [ ] Re-ran the diagnostic workflow successfully
- [ ] Verified Azure login step succeeded
- [ ] Downloaded and reviewed diagnostic artifacts

## What Happens After Fixing

Once the credentials are updated:

1. The diagnostic workflow will be able to log in to Azure
2. It will fetch logs from your Azure web app (e.g., `mosaic-saas` for this project)
3. It will analyze logs for common errors (500.30, database connection, auth issues, etc.)
4. It will create a comprehensive diagnostic report
5. You can download artifacts with all logs and analysis
6. The workflow summary will show app status, health check results, and detected errors

This will help you identify and fix any actual issues with your Azure Web App.

## Need Help?

If you're still having issues after following this guide:

1. Check the workflow run logs for specific error messages
2. Review [Azure troubleshooting documentation](https://learn.microsoft.com/en-us/azure/app-service/troubleshoot-authentication-authorization)
3. Verify all four credential values are correct
4. Try creating a completely new service principal from scratch
5. Open an issue in this repository with:
   - The workflow run URL
   - Error messages (with secrets redacted)
   - Steps you've already tried

---

*Last updated: 2025-12-20*
*Related workflow: `.github/workflows/fetch-diagnose-app-errors.yml`*
