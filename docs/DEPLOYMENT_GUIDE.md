# Complete Deployment Guide for OrkinosaiCMS

This guide covers all deployment methods for OrkinosaiCMS to Azure Web App, including GitHub Actions and Visual Studio 2026.

## Prerequisites

Before deploying, ensure you have:

- ✅ **Azure Subscription**: Active subscription with contributor access
- ✅ **Azure Resources**: Web App and SQL Database created
- ✅ **GitHub Repository**: Code pushed to GitHub (for automated deployment)
- ✅ **Visual Studio 2026** (optional): For manual deployment

## Quick Start Deployment

### Option 1: GitHub Actions (Recommended)

Automated deployment on every push to main branch.

#### Step 1: Create Azure Resources

Use the Azure Portal or Azure CLI to create:

**1. Create Resource Group:**
```bash
az group create --name orkinosaicms-rg --location eastus
```

**2. Create Azure SQL Server and Database:**
```bash
# Create SQL Server
az sql server create \
  --name orkinosaicms-sql \
  --resource-group orkinosaicms-rg \
  --location eastus \
  --admin-user sqladmin \
  --admin-password 'YourSecurePassword123!'

# Configure firewall
az sql server firewall-rule create \
  --resource-group orkinosaicms-rg \
  --server orkinosaicms-sql \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Create database
az sql db create \
  --resource-group orkinosaicms-rg \
  --server orkinosaicms-sql \
  --name orkinosaicms-db \
  --service-objective S1
```

**3. Create Azure Web App:**
```bash
# Create App Service Plan
az appservice plan create \
  --name orkinosaicms-plan \
  --resource-group orkinosaicms-rg \
  --location eastus \
  --sku B1 \
  --is-linux

# Create Web App
az webapp create \
  --name orkinosaicms-app \
  --resource-group orkinosaicms-rg \
  --plan orkinosaicms-plan \
  --runtime "DOTNETCORE:10.0"
```

**4. Configure Connection String:**
```bash
# Get connection string format
az sql db show-connection-string \
  --client ado.net \
  --server orkinosaicms-sql \
  --name orkinosaicms-db

# Set connection string in Web App
az webapp config connection-string set \
  --name orkinosaicms-app \
  --resource-group orkinosaicms-rg \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:orkinosaicms-sql.database.windows.net,1433;Initial Catalog=orkinosaicms-db;Persist Security Info=False;User ID=sqladmin;Password=YourSecurePassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

#### Step 2: Configure GitHub Secrets

1. **Get Publish Profile:**
```bash
az webapp deployment list-publishing-profiles \
  --name orkinosaicms-app \
  --resource-group orkinosaicms-rg \
  --xml
```

2. **Add to GitHub:**
   - Go to your repository → Settings → Secrets and variables → Actions
   - Click "New repository secret"
   - Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
   - Value: Paste the entire XML output from above
   - Click "Add secret"

#### Step 3: Update Workflow Configuration

Edit `.github/workflows/azure-deploy.yml` and update:
```yaml
env:
  AZURE_WEBAPP_NAME: orkinosaicms-app    # Your actual Web App name
```

#### Step 4: Deploy

1. Push to main branch:
```bash
git add .
git commit -m "Configure Azure deployment"
git push origin main
```

2. Monitor deployment:
   - Go to GitHub repository → Actions tab
   - Watch the deployment workflow

3. Access your site:
   - https://orkinosaicms-app.azurewebsites.net

#### Step 5: Run Database Migrations

After first deployment:
```bash
# Connect to your Web App via SSH or use Azure Cloud Shell
# Then run migrations
cd /home/site/wwwroot
dotnet ef database update --startup-project OrkinosaiCMS.Web.dll

# OR use local connection string
dotnet ef database update --startup-project ../OrkinosaiCMS.Web --connection "Your-Azure-Connection-String"
```

---

### Option 2: Visual Studio 2026 Deployment

Quick deployment directly from Visual Studio.

#### Step 1: Open Solution

1. Launch Visual Studio 2026
2. Open `OrkinosaiCMS.sln`
3. Ensure the solution builds successfully (Ctrl+Shift+B)

#### Step 2: Configure Publish Profile

1. Right-click `OrkinosaiCMS.Web` project → **Publish**
2. Click **New** to create a new publish profile
3. Select **Azure** → **Next**
4. Select **Azure App Service (Linux)** or **Azure App Service (Windows)** → **Next**
5. Sign in to your Azure account
6. Select or create:
   - **Resource Group**: `orkinosaicms-rg`
   - **App Service Plan**: Choose or create
   - **Name**: `orkinosaicms-app` (must be globally unique)
7. Click **Create** and wait for resources to be created

#### Step 3: Configure Connection String

1. In the Publish dialog, click **...** (More actions) → **Edit**
2. Go to **Settings** tab
3. Expand **Databases** → **DefaultConnection**
4. Enter your Azure SQL connection string
5. Check **Use this connection string at runtime**
6. Click **Save**

#### Step 4: Publish

1. Click **Publish** button
2. Wait for build and deployment to complete
3. Visual Studio will automatically open your site in a browser

#### Step 5: Verify Deployment

1. The site should load at: `https://orkinosaicms-app.azurewebsites.net`
2. Verify the database was seeded with sample data
3. Navigate to `/cms-home` to see the branded website

---

### Option 3: Azure CLI Deployment

For advanced users and automation scripts.

#### Quick Deploy Script

Create `deploy-to-azure.sh`:

```bash
#!/bin/bash

# Configuration
RESOURCE_GROUP="orkinosaicms-rg"
LOCATION="eastus"
SQL_SERVER="orkinosaicms-sql"
SQL_DB="orkinosaicms-db"
WEBAPP_NAME="orkinosaicms-app"
SQL_ADMIN="sqladmin"
SQL_PASSWORD="YourSecurePassword123!"

# Login to Azure
az login

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create SQL Server and Database
az sql server create \
  --name $SQL_SERVER \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --admin-user $SQL_ADMIN \
  --admin-password $SQL_PASSWORD

az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

az sql db create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name $SQL_DB \
  --service-objective S1

# Create App Service Plan and Web App
az appservice plan create \
  --name "${WEBAPP_NAME}-plan" \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku B1 \
  --is-linux

az webapp create \
  --name $WEBAPP_NAME \
  --resource-group $RESOURCE_GROUP \
  --plan "${WEBAPP_NAME}-plan" \
  --runtime "DOTNETCORE:10.0"

# Set connection string
CONNECTION_STRING="Server=tcp:${SQL_SERVER}.database.windows.net,1433;Initial Catalog=${SQL_DB};Persist Security Info=False;User ID=${SQL_ADMIN};Password=${SQL_PASSWORD};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

az webapp config connection-string set \
  --name $WEBAPP_NAME \
  --resource-group $RESOURCE_GROUP \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="$CONNECTION_STRING"

# Build and publish locally
dotnet publish src/OrkinosaiCMS.Web/OrkinosaiCMS.Web.csproj \
  --configuration Release \
  --output ./publish

# Create deployment package
cd publish
zip -r ../deploy.zip .
cd ..

# Deploy to Azure
az webapp deployment source config-zip \
  --resource-group $RESOURCE_GROUP \
  --name $WEBAPP_NAME \
  --src deploy.zip

echo "Deployment complete!"
echo "Your site: https://${WEBAPP_NAME}.azurewebsites.net"
```

Make it executable and run:
```bash
chmod +x deploy-to-azure.sh
./deploy-to-azure.sh
```

---

## Post-Deployment Configuration

### 1. Configure App Settings

Add any additional settings via Azure Portal or CLI:

```bash
az webapp config appsettings set \
  --name orkinosaicms-app \
  --resource-group orkinosaicms-rg \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    WEBSITE_TIME_ZONE="Eastern Standard Time"
```

### 2. Enable Application Insights (Optional)

```bash
# Create Application Insights
az monitor app-insights component create \
  --app orkinosaicms-insights \
  --location eastus \
  --resource-group orkinosaicms-rg

# Get instrumentation key
INSTRUMENTATION_KEY=$(az monitor app-insights component show \
  --app orkinosaicms-insights \
  --resource-group orkinosaicms-rg \
  --query instrumentationKey -o tsv)

# Configure Web App
az webapp config appsettings set \
  --name orkinosaicms-app \
  --resource-group orkinosaicms-rg \
  --settings APPINSIGHTS_INSTRUMENTATIONKEY=$INSTRUMENTATION_KEY
```

### 3. Configure Custom Domain (Optional)

```bash
# Add custom domain
az webapp config hostname add \
  --webapp-name orkinosaicms-app \
  --resource-group orkinosaicms-rg \
  --hostname www.yourdomain.com

# Enable HTTPS with managed certificate
az webapp config ssl create \
  --resource-group orkinosaicms-rg \
  --name orkinosaicms-app \
  --hostname www.yourdomain.com
```

### 4. Configure Backup (Production)

```bash
# Create storage account for backups
az storage account create \
  --name orkinosaicmsbackup \
  --resource-group orkinosaicms-rg \
  --location eastus \
  --sku Standard_LRS

# Configure automated backup
az webapp config backup create \
  --resource-group orkinosaicms-rg \
  --webapp-name orkinosaicms-app \
  --container-url "https://orkinosaicmsbackup.blob.core.windows.net/backups" \
  --backup-name dailybackup \
  --db-connection-string-type SQLAzure \
  --db-connection-string "Your-Connection-String"
```

---

## Troubleshooting

### Common Issues

**1. Site shows "Service Unavailable"**
- Check App Service logs in Azure Portal
- Verify connection string is correct
- Ensure database firewall allows Azure services

**2. Database connection errors**
```bash
# Test connection string
az sql db show-connection-string \
  --client ado.net \
  --server orkinosaicms-sql \
  --name orkinosaicms-db
```

**3. Deployment fails in GitHub Actions**
- Verify `AZURE_WEBAPP_PUBLISH_PROFILE` secret is set correctly
- Check workflow logs for specific errors
- Ensure Azure Web App name matches

### View Application Logs

**Via Azure Portal:**
1. Navigate to your Web App
2. Go to **Monitoring** → **Log stream**
3. View real-time logs

**Via Azure CLI:**
```bash
# Enable logging
az webapp log config \
  --name orkinosaicms-app \
  --resource-group orkinosaicms-rg \
  --application-logging filesystem \
  --level information

# View logs
az webapp log tail \
  --name orkinosaicms-app \
  --resource-group orkinosaicms-rg
```

### Database Migrations

If migrations need to be applied after deployment:

```bash
# Option 1: Using local .NET EF tools
dotnet ef database update \
  --startup-project src/OrkinosaiCMS.Web \
  --project src/OrkinosaiCMS.Infrastructure \
  --connection "Your-Azure-Connection-String"

# Option 2: Using SQL script
dotnet ef migrations script \
  --project src/OrkinosaiCMS.Infrastructure \
  --output migration.sql

# Then execute migration.sql in Azure Portal Query Editor
```

---

## Monitoring and Maintenance

### Performance Monitoring

1. **Application Insights** (if configured):
   - View performance metrics
   - Track exceptions
   - Analyze user behavior

2. **Azure Monitor**:
   - Set up alerts for high CPU/memory
   - Monitor response times
   - Track availability

### Cost Optimization

**Development/Testing:**
- App Service Plan: B1 (~$13/month)
- SQL Database: Basic (~$5/month)

**Production:**
- App Service Plan: S1 or higher (~$69/month)
- SQL Database: S1 or higher (~$30/month)
- Application Insights: Free tier for basic monitoring

**Scaling:**
```bash
# Scale out (add instances)
az appservice plan update \
  --name orkinosaicms-plan \
  --resource-group orkinosaicms-rg \
  --number-of-workers 2

# Scale up (change tier)
az appservice plan update \
  --name orkinosaicms-plan \
  --resource-group orkinosaicms-rg \
  --sku S2
```

---

## Security Best Practices

### 1. Secure Connection Strings

Never store connection strings in code. Use:
- Azure Key Vault (recommended for production)
- App Settings/Connection Strings in Azure Portal
- User Secrets for local development

### 2. Enable HTTPS Only

```bash
az webapp update \
  --name orkinosaicms-app \
  --resource-group orkinosaicms-rg \
  --https-only true
```

### 3. Configure Authentication (Optional)

```bash
# Enable Azure AD authentication
az webapp auth update \
  --name orkinosaicms-app \
  --resource-group orkinosaicms-rg \
  --enabled true \
  --action LoginWithAzureActiveDirectory
```

### 4. Regular Updates

- Keep .NET runtime updated
- Apply security patches promptly
- Review Azure Security Center recommendations

---

## Next Steps

After successful deployment:

1. ✅ Verify site loads at `/cms-home`
2. ✅ Test all pages (About, Features, Contact)
3. ✅ Test contact form submission
4. ✅ Review Application Insights data
5. ✅ Set up automated backups
6. ✅ Configure custom domain (if needed)
7. ✅ Set up monitoring alerts

## Support

- **Documentation**: [docs/](../docs/)
- **GitHub Issues**: [Submit an issue](https://github.com/orkinosai25-org/orkinosaiCMS/issues)
- **Azure Support**: [Azure Portal](https://portal.azure.com)

---

Built with ❤️ using .NET 10 and Blazor
