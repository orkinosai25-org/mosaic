# Azure Deployment Guide for OrkinosaiCMS

This guide provides step-by-step instructions for deploying OrkinosaiCMS to Azure Web Apps with Azure SQL Database.

## Prerequisites

- **Azure Subscription**: Active Azure subscription with appropriate permissions
- **Azure CLI**: Installed and configured ([Download](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli))
- **Git**: For deployment from repository
- **.NET 10 SDK**: For local testing before deployment

## Architecture Overview

OrkinosaiCMS on Azure consists of:
- **Azure Web App**: Hosts the Blazor web application
- **Azure SQL Database**: Stores all application data
- **Application Insights** (optional): Monitors application performance

## Deployment Options

### Option 1: Azure Portal Deployment (Recommended for First-Time Setup)

#### Step 1: Create Azure SQL Database

1. Navigate to the [Azure Portal](https://portal.azure.com)
2. Click **Create a resource** → **SQL Database**
3. Configure the database:
   - **Resource Group**: Create new or select existing
   - **Database Name**: `orkinosaicms-db`
   - **Server**: Create new with:
     - **Server Name**: `orkinosaicms-sql-server` (must be globally unique)
     - **Location**: Choose nearest region
     - **Authentication**: SQL authentication
     - **Admin Login**: Choose secure username
     - **Password**: Choose strong password (save securely)
   - **Compute + storage**: Choose appropriate tier
     - **Development**: Basic (5 DTUs)
     - **Production**: Standard S1 or higher
4. Click **Review + create** → **Create**

#### Step 2: Configure Firewall Rules

1. Navigate to your SQL Server resource
2. Under **Security**, select **Networking**
3. Add firewall rules:
   - Check **Allow Azure services and resources to access this server**
   - Add your client IP for management
4. Click **Save**

#### Step 3: Get Connection String

1. Navigate to your SQL Database
2. Select **Connection strings** under **Settings**
3. Copy the **ADO.NET** connection string
4. Replace `{your_password}` with your actual password
5. Save this connection string securely

Example connection string:
```
Server=tcp:orkinosaicms-sql-server.database.windows.net,1433;Initial Catalog=orkinosaicms-db;Persist Security Info=False;User ID=sqladmin;Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

#### Step 4: Create Azure Web App

1. Navigate to Azure Portal → **Create a resource** → **Web App**
2. Configure the web app:
   - **Resource Group**: Same as database
   - **Name**: `orkinosaicms-app` (must be globally unique)
   - **Publish**: Code
   - **Runtime stack**: .NET 10 (LTS)
   - **Operating System**: Linux (recommended) or Windows
   - **Region**: Same as database
   - **App Service Plan**: Create new or select existing
     - **SKU**: B1 or higher for production
3. Click **Review + create** → **Create**

#### Step 5: Configure App Settings

1. Navigate to your Web App resource
2. Select **Configuration** under **Settings**
3. Under **Application settings**, add:
   - **Name**: `ConnectionStrings__DefaultConnection`
   - **Value**: Your SQL connection string from Step 3
   - Click **OK**
4. Under **General settings**, verify:
   - **Stack**: .NET
   - **Version**: 10
   - **Always On**: On (for production)
5. Click **Save**

#### Step 6: Deploy the Application

**Using Azure CLI:**

```bash
# Login to Azure
az login

# Set variables
RESOURCE_GROUP="your-resource-group"
APP_NAME="orkinosaicms-app"
REPO_URL="https://github.com/orkinosai25-org/orkinosaiCMS.git"
BRANCH="main"

# Configure deployment from GitHub
az webapp deployment source config --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --repo-url $REPO_URL \
  --branch $BRANCH \
  --manual-integration

# Or deploy from local folder
cd /path/to/orkinosaiCMS
dotnet publish src/OrkinosaiCMS.Web/OrkinosaiCMS.Web.csproj -c Release -o ./publish
cd publish
zip -r ../orkinosaicms.zip .
az webapp deployment source config-zip --resource-group $RESOURCE_GROUP \
  --name $APP_NAME \
  --src ../orkinosaicms.zip
```

**Using Visual Studio:**

1. Right-click on `OrkinosaiCMS.Web` project
2. Select **Publish**
3. Choose **Azure** → **Azure App Service (Linux)** or **(Windows)**
4. Select your subscription and web app
5. Click **Finish** → **Publish**

#### Step 7: Apply Database Migrations

After deployment, apply migrations to create database schema:

**Option A: Using Azure Cloud Shell**

```bash
# Connect to the web app
az webapp ssh --resource-group $RESOURCE_GROUP --name $APP_NAME

# Navigate to app directory (path may vary)
cd /home/site/wwwroot

# Run migrations (requires dotnet-ef installed)
dotnet tool install --global dotnet-ef --version 10.0.0
export PATH="$PATH:/root/.dotnet/tools"
dotnet ef database update --project OrkinosaiCMS.Infrastructure.dll --startup-project OrkinosaiCMS.Web.dll
```

**Option B: Using Local Machine with Connection String**

```bash
# Set connection string as environment variable
export ConnectionStrings__DefaultConnection="<your-azure-sql-connection-string>"

# Apply migrations
cd src/OrkinosaiCMS.Infrastructure
dotnet ef database update --startup-project ../OrkinosaiCMS.Web
```

**Option C: Run Migrations on Startup (Recommended for Production)**

Add migration code to `Program.cs`:

```csharp
// After app.Build()
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (app.Environment.IsProduction())
    {
        context.Database.Migrate();
    }
}
```

#### Step 8: Verify Deployment

1. Navigate to your web app URL: `https://{app-name}.azurewebsites.net`
2. Verify the application loads successfully
3. Check logs in Azure Portal under **Monitoring** → **Log stream**

### Option 2: Azure CLI Deployment

Complete automation script:

```bash
#!/bin/bash

# Configuration
RESOURCE_GROUP="orkinosaicms-rg"
LOCATION="eastus"
SQL_SERVER="orkinosaicms-sql-$(date +%s)"
SQL_DB="orkinosaicms-db"
SQL_ADMIN="sqladmin"
SQL_PASSWORD="YourSecure@Password123"
APP_NAME="orkinosaicms-app-$(date +%s)"
APP_PLAN="orkinosaicms-plan"

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create SQL Server
az sql server create \
  --name $SQL_SERVER \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --admin-user $SQL_ADMIN \
  --admin-password $SQL_PASSWORD

# Configure firewall
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Create SQL Database
az sql db create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name $SQL_DB \
  --service-objective S1

# Get connection string
CONNECTION_STRING="Server=tcp:${SQL_SERVER}.database.windows.net,1433;Initial Catalog=${SQL_DB};Persist Security Info=False;User ID=${SQL_ADMIN};Password=${SQL_PASSWORD};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Create App Service Plan
az appservice plan create \
  --name $APP_PLAN \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku B1 \
  --is-linux

# Create Web App
az webapp create \
  --resource-group $RESOURCE_GROUP \
  --plan $APP_PLAN \
  --name $APP_NAME \
  --runtime "DOTNET:10"

# Configure connection string
az webapp config connection-string set \
  --resource-group $RESOURCE_GROUP \
  --name $APP_NAME \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="$CONNECTION_STRING"

# Configure app settings
az webapp config appsettings set \
  --resource-group $RESOURCE_GROUP \
  --name $APP_NAME \
  --settings ASPNETCORE_ENVIRONMENT="Production"

# Deploy from GitHub (optional)
# az webapp deployment source config --name $APP_NAME \
#   --resource-group $RESOURCE_GROUP \
#   --repo-url https://github.com/orkinosai25-org/orkinosaiCMS.git \
#   --branch main \
#   --manual-integration

echo "Deployment complete!"
echo "Web App URL: https://${APP_NAME}.azurewebsites.net"
echo "SQL Server: ${SQL_SERVER}.database.windows.net"
echo "Database: ${SQL_DB}"
```

## Configuration Best Practices

### Environment-Specific Settings

Use Azure App Settings for production configuration instead of `appsettings.json`:

1. **Connection Strings**: Set via Connection Strings in Azure Portal
2. **Application Settings**: Use app settings for:
   - `ASPNETCORE_ENVIRONMENT`: Production
   - `CMS__SiteName`: Your site name
   - `CMS__EnableModuleDiscovery`: false
   - `CMS__AllowRegistration`: false

### Security Recommendations

1. **Use Managed Identity**: Eliminate connection string credentials
   ```bash
   # Enable managed identity
   az webapp identity assign --resource-group $RESOURCE_GROUP --name $APP_NAME
   
   # Configure SQL with managed identity
   # (Requires additional SQL configuration)
   ```

2. **Key Vault Integration**: Store secrets in Azure Key Vault
   ```bash
   # Create Key Vault
   az keyvault create --name orkinosaicms-kv --resource-group $RESOURCE_GROUP
   
   # Store connection string
   az keyvault secret set --vault-name orkinosaicms-kv \
     --name "ConnectionStrings--DefaultConnection" \
     --value "$CONNECTION_STRING"
   
   # Grant web app access
   az keyvault set-policy --name orkinosaicms-kv \
     --object-id $(az webapp identity show --resource-group $RESOURCE_GROUP --name $APP_NAME --query principalId -o tsv) \
     --secret-permissions get list
   ```

3. **SSL/TLS**: Always use HTTPS (enforced by default in Azure Web Apps)

4. **Firewall Rules**: Restrict SQL Server access to specific IPs when possible

## Monitoring and Diagnostics

### Enable Application Insights

```bash
# Create Application Insights
az monitor app-insights component create \
  --app orkinosaicms-insights \
  --location $LOCATION \
  --resource-group $RESOURCE_GROUP

# Get instrumentation key
INSIGHTS_KEY=$(az monitor app-insights component show \
  --app orkinosaicms-insights \
  --resource-group $RESOURCE_GROUP \
  --query instrumentationKey -o tsv)

# Configure web app
az webapp config appsettings set \
  --resource-group $RESOURCE_GROUP \
  --name $APP_NAME \
  --settings APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=$INSIGHTS_KEY"
```

### View Logs

```bash
# Stream logs
az webapp log tail --resource-group $RESOURCE_GROUP --name $APP_NAME

# Download logs
az webapp log download --resource-group $RESOURCE_GROUP --name $APP_NAME
```

## Scaling

### Vertical Scaling (Scale Up)

```bash
# Upgrade to higher tier
az appservice plan update \
  --name $APP_PLAN \
  --resource-group $RESOURCE_GROUP \
  --sku P1V2
```

### Horizontal Scaling (Scale Out)

```bash
# Add more instances
az appservice plan update \
  --name $APP_PLAN \
  --resource-group $RESOURCE_GROUP \
  --number-of-workers 3
```

## Backup and Restore

### Database Backup

Azure SQL Database provides automatic backups. Manual backup:

```bash
# Create manual backup
az sql db copy \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name $SQL_DB \
  --dest-name "${SQL_DB}-backup-$(date +%Y%m%d)"
```

### Web App Backup

```bash
# Configure backup
az webapp config backup create \
  --resource-group $RESOURCE_GROUP \
  --webapp-name $APP_NAME \
  --container-url "<storage-container-sas-url>" \
  --backup-name "orkinosaicms-backup-$(date +%Y%m%d)"
```

## Troubleshooting

### Common Issues

1. **Connection Timeout**
   - Verify firewall rules allow Azure services
   - Check connection string format
   - Ensure SQL Database is not paused

2. **Application Won't Start**
   - Check logs: `az webapp log tail`
   - Verify .NET 10 runtime is selected
   - Check app settings configuration

3. **Database Migration Errors**
   - Ensure connection string is correct
   - Verify SQL admin credentials
   - Check database permissions

4. **Performance Issues**
   - Review Application Insights metrics
   - Consider scaling up/out
   - Optimize database queries
   - Enable caching

### Getting Help

- **Azure Support**: [https://azure.microsoft.com/support](https://azure.microsoft.com/support)
- **GitHub Issues**: [https://github.com/orkinosai25-org/orkinosaiCMS/issues](https://github.com/orkinosai25-org/orkinosaiCMS/issues)
- **Documentation**: [docs/](./README.md)

## Cost Optimization

### Development/Testing
- **App Service**: B1 Basic (~$13/month)
- **SQL Database**: Basic (5 DTUs) (~$5/month)
- **Total**: ~$18/month

### Production (Small)
- **App Service**: P1V2 (~$73/month)
- **SQL Database**: S1 Standard (~$30/month)
- **Application Insights**: Pay-as-you-go
- **Total**: ~$103/month + usage

### Production (Medium)
- **App Service**: P2V2 (~$146/month) with 2-3 instances
- **SQL Database**: S3 Standard (~$120/month)
- **Application Insights**: Pay-as-you-go
- **Total**: ~$412-$584/month + usage

## Next Steps

1. Review [SETUP.md](./SETUP.md) for local development
2. Configure custom domain: [Azure Custom Domains](https://docs.microsoft.com/en-us/azure/app-service/app-service-web-tutorial-custom-domain)
3. Set up CI/CD: [Azure DevOps](https://docs.microsoft.com/en-us/azure/devops/) or [GitHub Actions](https://docs.github.com/en/actions)
4. Implement caching with Azure Cache for Redis
5. Configure CDN for static assets

## Additional Resources

- [Azure App Service Documentation](https://docs.microsoft.com/en-us/azure/app-service/)
- [Azure SQL Database Documentation](https://docs.microsoft.com/en-us/azure/sql-database/)
- [.NET on Azure](https://docs.microsoft.com/en-us/dotnet/azure/)
- [ASP.NET Core Deployment](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/azure-apps/)
