# GitHub Secrets and Environment Variables Configuration

This guide explains how to configure secrets and environment variables for MOSAIC deployment pipelines (GitHub Actions, Azure, Vercel).

## 📋 Overview

MOSAIC requires various API keys and secrets for different services:

| Service | Keys Required | Purpose |
|---------|---------------|---------|
| **Stripe** | Publishable Key, Secret Key, Webhook Secret | Payment processing |
| **Azure Blob Storage** | Connection String | Media and file storage |
| **Azure OpenAI** | API Key, Endpoint | AI agents (MOSAIC, Zoota) |
| **SendGrid** | API Key | Email notifications |
| **Database** | Connection String | Data persistence |
| **JWT** | Secret Key | Authentication tokens |

## 🔐 GitHub Secrets Setup

GitHub Secrets are used in GitHub Actions workflows for CI/CD.

### Step 1: Navigate to Repository Settings

1. Go to your repository: `https://github.com/orkinosai25-org/mosaic`
2. Click **Settings** (requires admin access)
3. In the left sidebar, click **Secrets and variables** → **Actions**

### Step 2: Add Repository Secrets

Click **New repository secret** and add each of the following:

#### Stripe Payment Integration

| Secret Name | Value Example | Description |
|-------------|--------------|-------------|
| `STRIPE_PUBLISHABLE_KEY` | `pk_test_51AbCd...` or `pk_live_51AbCd...` | Stripe publishable key (safe for frontend) |
| `STRIPE_SECRET_KEY` | `sk_test_51AbCd...` or `sk_live_51AbCd...` | Stripe secret key (backend only) |
| `STRIPE_WEBHOOK_SECRET` | `whsec_123456...` | Webhook signature verification |

#### Azure Deployment

| Secret Name | Value Example | Description |
|-------------|--------------|-------------|
| `AZURE_WEBAPP_PUBLISH_PROFILE_MOSAIC` | (XML content) | Azure Web App publish profile |
| `AZURE_CREDENTIALS` | (JSON content) | Azure service principal credentials for log access and diagnostics |

#### Database

| Secret Name | Value Example | Description |
|-------------|--------------|-------------|
| `AZURE_SQL_CONNECTION_STRING` | `Server=tcp:...;Database=...;User ID=...;Password=...;` | Azure SQL connection string |

#### Azure Blob Storage

| Secret Name | Value Example | Description |
|-------------|--------------|-------------|
| `AZURE_BLOB_STORAGE_CONNECTION_STRING` | `DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;` | Blob storage connection |

#### Azure OpenAI

| Secret Name | Value Example | Description |
|-------------|--------------|-------------|
| `AZURE_OPENAI_API_KEY` | `a1b2c3d4e5f6...` | Azure OpenAI API key |
| `AZURE_OPENAI_ENDPOINT` | `https://your-resource.openai.azure.com/` | Azure OpenAI endpoint URL |

#### Email Service (SendGrid)

| Secret Name | Value Example | Description |
|-------------|--------------|-------------|
| `SENDGRID_API_KEY` | `SG.AbCdEf123456...` | SendGrid API key for emails |

#### JWT Authentication

| Secret Name | Value Example | Description |
|-------------|--------------|-------------|
| `JWT_SECRET_KEY` | `your-super-secret-min-32-chars` | JWT signing key (min 32 chars) |

### Step 3: Verify Secrets

After adding all secrets:
1. Go to **Actions** tab in your repository
2. Select a workflow run
3. Check that secrets are marked as `***` in logs (never displayed)

## 🔧 Using Secrets in GitHub Actions

### Current Deployment Workflow

The `.github/workflows/deploy.yml` file already uses the `AZURE_WEBAPP_PUBLISH_PROFILE_MOSAIC` secret:

```yaml
- name: Deploy to Azure Web App
  uses: azure/webapps-deploy@v3
  with:
    app-name: mosaic-saas
    publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE_MOSAIC }}
    package: .
```

### Adding Stripe Secrets to Deployment

To make Stripe keys available during deployment, update the workflow:

```yaml
deploy:
  name: Deploy to Azure
  runs-on: ubuntu-latest
  needs: build
  environment:
    name: 'Production'
  
  steps:
    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v3
      with:
        app-name: mosaic-saas
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE_MOSAIC }}
        package: .
      env:
        # Stripe configuration
        STRIPE_PUBLISHABLE_KEY: ${{ secrets.STRIPE_PUBLISHABLE_KEY }}
        STRIPE_SECRET_KEY: ${{ secrets.STRIPE_SECRET_KEY }}
        STRIPE_WEBHOOK_SECRET: ${{ secrets.STRIPE_WEBHOOK_SECRET }}
        
        # Other services
        AZURE_OPENAI_API_KEY: ${{ secrets.AZURE_OPENAI_API_KEY }}
        SENDGRID_API_KEY: ${{ secrets.SENDGRID_API_KEY }}
```

## ☁️ Azure Web App Configuration

Secrets can also be configured directly in Azure Web App settings.

### Option 1: Azure Portal (GUI)

1. Go to [Azure Portal](https://portal.azure.com/)
2. Navigate to your Web App: `mosaic-saas`
3. In the left menu, click **Configuration** under **Settings**
4. Click **Application settings** tab
5. Click **+ New application setting**
6. Add each setting:

#### Stripe Settings

| Name | Value | Slot Setting |
|------|-------|--------------|
| `Payment__Stripe__PublishableKey` | `pk_test_51AbCd...` | ☐ No |
| `Payment__Stripe__SecretKey` | `sk_test_51AbCd...` | ☐ No |
| `Payment__Stripe__WebhookSecret` | `whsec_123456...` | ☐ No |

**Important**: Use double underscores `__` instead of colons `:` for nested configuration in Azure.

#### Other Settings

| Name | Value | Slot Setting |
|------|-------|--------------|
| `ConnectionStrings__DefaultConnection` | `Server=tcp:...;` | ☐ No |
| `AzureBlobStorage__ConnectionString` | `DefaultEndpointsProtocol=https;...` | ☐ No |
| `AzureOpenAI__ApiKey` | `a1b2c3d4e5f6...` | ☐ No |
| `EmailSettings__SendGrid__ApiKey` | `SG.AbCdEf123456...` | ☐ No |
| `Authentication__JwtSettings__SecretKey` | `your-secret-key-min-32-chars` | ☐ No |

7. Click **Save** at the top
8. Click **Continue** to confirm restart

### Option 2: Azure CLI

Use Azure CLI to add application settings:

```bash
# Set Stripe keys
az webapp config appsettings set \
  --name mosaic-saas \
  --resource-group orkinosai_group \
  --settings \
    Payment__Stripe__PublishableKey="pk_test_51AbCd..." \
    Payment__Stripe__SecretKey="sk_test_51AbCd..." \
    Payment__Stripe__WebhookSecret="whsec_123456..."

# Set other configuration
az webapp config appsettings set \
  --name mosaic-saas \
  --resource-group orkinosai_group \
  --settings \
    AzureOpenAI__ApiKey="a1b2c3d4e5f6..." \
    EmailSettings__SendGrid__ApiKey="SG.AbCdEf123456..."
```

### Option 3: Azure Key Vault (Recommended for Production)

For production environments, use Azure Key Vault:

```bash
# Create Key Vault (if not exists)
az keyvault create \
  --name mosaic-keyvault \
  --resource-group orkinosai_group \
  --location uksouth

# Add Stripe secrets
az keyvault secret set \
  --vault-name mosaic-keyvault \
  --name StripePublishableKey \
  --value "pk_live_51AbCd..."

az keyvault secret set \
  --vault-name mosaic-keyvault \
  --name StripeSecretKey \
  --value "sk_live_51AbCd..."

az keyvault secret set \
  --vault-name mosaic-keyvault \
  --name StripeWebhookSecret \
  --value "whsec_123456..."

# Enable managed identity on Web App
az webapp identity assign \
  --name mosaic-saas \
  --resource-group orkinosai_group

# Get principal ID
PRINCIPAL_ID=$(az webapp identity show \
  --name mosaic-saas \
  --resource-group orkinosai_group \
  --query principalId -o tsv)

# Grant Web App access to Key Vault
az keyvault set-policy \
  --name mosaic-keyvault \
  --object-id $PRINCIPAL_ID \
  --secret-permissions get list
```

Then update `appsettings.Production.json`:

```json
{
  "KeyVault": {
    "VaultUri": "https://mosaic-keyvault.vault.azure.net/"
  }
}
```

## 🔧 Azure Service Principal for Log Diagnostics

The **Fetch and Diagnose App Errors** workflow requires Azure credentials to fetch logs and diagnose issues. This uses a service principal with appropriate permissions.

### Step 1: Create Service Principal

```bash
# Replace {subscription-id} with your Azure subscription ID
# Replace {your-resource-group} with your actual resource group name
# Common names: orkinosai_group, mosaic-rg, or check with: az group list
az ad sp create-for-rbac --name "mosaic-github-diagnostics" \
  --role contributor \
  --scopes /subscriptions/{subscription-id}/resourceGroups/{your-resource-group} \
  --sdk-auth
```

This will output JSON like:

```json
{
  "clientId": "12345678-1234-1234-1234-123456789012",
  "clientSecret": "your-client-secret",
  "subscriptionId": "87654321-4321-4321-4321-210987654321",
  "tenantId": "abcdefgh-abcd-abcd-abcd-abcdefghijkl",
  "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
  "resourceManagerEndpointUrl": "https://management.azure.com/",
  "activeDirectoryGraphResourceId": "https://graph.windows.net/",
  "sqlManagementEndpointUrl": "https://management.core.windows.net:8443/",
  "galleryEndpointUrl": "https://gallery.azure.com/",
  "managementEndpointUrl": "https://management.core.windows.net/"
}
```

### Step 2: Add to GitHub Secrets

1. Copy the entire JSON output from Step 1
2. Go to GitHub repository → **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Name: `AZURE_CREDENTIALS`
5. Value: Paste the entire JSON
6. Click **Add secret**

### Step 3: Verify Permissions

The service principal needs these permissions:
- **Reader** on the Resource Group (to view resources)
- **Website Contributor** on the App Service (to access logs)
- **Monitoring Reader** (to view activity logs)

To add additional permissions:

```bash
# Add Monitoring Reader role
az role assignment create \
  --assignee <clientId-from-step-1> \
  --role "Monitoring Reader" \
  --scope /subscriptions/{subscription-id}/resourceGroups/{your-resource-group}

# Verify role assignments
az role assignment list \
  --assignee <clientId-from-step-1> \
  --output table
```

### Step 4: Test the Workflow

1. Go to **Actions** tab in GitHub
2. Select **Fetch and Diagnose App Errors** workflow
3. Click **Run workflow**
4. Check that it can authenticate and fetch logs

See [AZURE_LOG_DIAGNOSTICS.md](./AZURE_LOG_DIAGNOSTICS.md) for complete workflow documentation.

## 🚀 Vercel Deployment (Alternative Frontend Hosting)

If deploying the frontend separately to Vercel:

### Step 1: Add Environment Variables in Vercel

1. Go to [Vercel Dashboard](https://vercel.com/dashboard)
2. Select your project
3. Go to **Settings** → **Environment Variables**
4. Add the following:

#### Frontend Environment Variables

| Name | Value | Environment |
|------|-------|-------------|
| `VITE_STRIPE_PUBLISHABLE_KEY` | `pk_test_51AbCd...` | Production, Preview, Development |
| `VITE_API_BASE_URL` | `https://mosaic-saas.azurewebsites.net/api` | Production |
| `VITE_API_BASE_URL` | `https://mosaic-staging.azurewebsites.net/api` | Preview |
| `VITE_API_BASE_URL` | `http://localhost:5000/api` | Development |

**Important**: Only add the **publishable key** to frontend environment variables. Never add the secret key!

### Step 2: Redeploy

After adding environment variables, trigger a redeploy:

```bash
vercel --prod
```

Or use the Vercel Dashboard to trigger a deployment.

## 🔒 Security Best Practices

### ✅ DO

1. **Use separate keys for each environment**
   - Development: Test keys (pk_test_*, sk_test_*)
   - Production: Live keys (pk_live_*, sk_live_*)

2. **Rotate secrets regularly**
   - Update secrets every 90 days
   - Rotate immediately if compromised

3. **Use deployment slots** (Azure)
   - Test configuration in staging slot first
   - Swap to production after validation

4. **Enable audit logging**
   - Track when secrets are accessed
   - Monitor for unauthorized access

5. **Use managed identities** (Azure)
   - Avoid storing connection strings
   - Let Azure handle authentication

6. **Limit secret scope**
   - Use repository secrets for CI/CD only
   - Use environment secrets for specific environments

### ❌ DON'T

1. **Never commit secrets to Git**
   - Use `.gitignore` for `.env` files
   - Scan for exposed secrets regularly

2. **Never log secrets**
   - Mask sensitive values in logs
   - GitHub Actions automatically masks secrets

3. **Never share secrets via chat/email**
   - Use secure secret sharing tools
   - Prefer secret management systems

4. **Never use production secrets in development**
   - Always use test keys locally
   - Keep production keys separate

5. **Never expose secret keys to frontend**
   - Only publishable keys go to frontend
   - Secret keys stay on backend

## 🧪 Validating Configuration

### Test GitHub Secrets

Run a workflow manually to test:

1. Go to **Actions** tab
2. Select **Deploy MOSAIC to Azure Web App**
3. Click **Run workflow** → **Run workflow**
4. Check logs for any missing secrets

### Test Azure Configuration

```bash
# Test connection to Azure
az webapp show \
  --name mosaic-saas \
  --resource-group orkinosai_group

# List application settings (values are hidden)
az webapp config appsettings list \
  --name mosaic-saas \
  --resource-group orkinosai_group
```

### Test in Application

Create a health check endpoint:

```csharp
// In a controller
[HttpGet("health/config")]
public IActionResult CheckConfig()
{
    var hasStripeKey = !string.IsNullOrEmpty(_stripeSettings.SecretKey);
    var hasDbConnection = !string.IsNullOrEmpty(_configuration.GetConnectionString("DefaultConnection"));
    
    return Ok(new {
        stripe_configured = hasStripeKey,
        database_configured = hasDbConnection,
        // Don't expose actual values!
    });
}
```

## 📚 Additional Resources

- [GitHub Encrypted Secrets](https://docs.github.com/en/actions/security-guides/encrypted-secrets)
- [Azure App Service Configuration](https://docs.microsoft.com/azure/app-service/configure-common)
- [Azure Key Vault Documentation](https://docs.microsoft.com/azure/key-vault/)
- [Vercel Environment Variables](https://vercel.com/docs/environment-variables)
- [Stripe API Keys Documentation](https://stripe.com/docs/keys)
- [MOSAIC Stripe Setup Guide](./STRIPE_SETUP.md)

## 🆘 Troubleshooting

### Issue: Secrets not loading in workflow

**Solution**:
1. Check secret names match exactly (case-sensitive)
2. Verify you have admin access to repository
3. Re-add the secret if needed

### Issue: Application can't read configuration

**Solution**:
1. Verify environment variable naming (use `__` in Azure)
2. Restart the Web App after configuration changes
3. Check Key Vault permissions if using managed identity

### Issue: Stripe webhooks fail verification

**Solution**:
1. Verify webhook secret is correct
2. Check that webhook endpoint URL is correct in Stripe Dashboard
3. Ensure webhook secret is set in environment

---

**Last Updated:** December 2024  
**Version:** 1.0  
**Maintained by:** Orkinosai Team
