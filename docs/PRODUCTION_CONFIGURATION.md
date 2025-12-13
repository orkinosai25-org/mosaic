# Production Configuration Guide

This guide explains how to properly configure Mosaic CMS for production deployment on Azure.

## Overview

The production configuration is managed through `appsettings.Production.json` and Azure Portal settings. This document outlines the correct configuration for Azure SQL Database and best practices for securing sensitive credentials.

## Configuration Changes (December 2025)

The `appsettings.Production.json` file has been updated with the following critical settings for Azure SQL Database:

### Database Configuration

```json
{
  "DatabaseEnabled": true,
  "DatabaseProvider": "SqlServer",
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:orkinosai.database.windows.net,1433;Initial Catalog=mosaic-saas;..."
  }
}
```

**Key Settings:**
- `DatabaseEnabled`: Set to `true` to enable database connectivity
- `DatabaseProvider`: Set to `"SqlServer"` for Azure SQL Database (NOT `"SQLite"`)
- `ConnectionStrings.DefaultConnection`: Uses Azure SQL connection string format
- `SqliteConnection`: Removed from production configuration (only needed for development)

### Error Handling Configuration

```json
{
  "ErrorHandling": {
    "ShowDetailedErrors": false,
    "IncludeStackTrace": false
  }
}
```

**Production Settings:**
- `ShowDetailedErrors`: Set to `false` to prevent exposing sensitive error information
- `IncludeStackTrace`: Set to `false` to prevent exposing application internals

### Logging Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

**Production Settings:**
- `Default`: Set to `"Warning"` to reduce log noise
- `Microsoft.AspNetCore`: Set to `"Information"` for debugging production issues

## Azure Portal Configuration (REQUIRED for Production)

### SECURITY WARNING ⚠️

**NEVER** commit production credentials to source control. The connection string in `appsettings.Production.json` contains development/test credentials and must be overridden in Azure Portal for production deployments.

### How to Configure Connection Strings in Azure Portal

1. **Navigate to Azure Portal**
   - Go to [https://portal.azure.com](https://portal.azure.com)
   - Sign in with your Azure credentials

2. **Open Your App Service**
   - Search for your App Service (e.g., "mosaic-cms-production")
   - Click on the App Service name

3. **Access Configuration Settings**
   - In the left sidebar, under **Settings**, click **Configuration**

4. **Add Connection String**
   - Click on the **Connection strings** tab
   - Click **+ New connection string**
   - Enter the following:
     - **Name**: `DefaultConnection`
     - **Value**: Your production Azure SQL connection string (see format below)
     - **Type**: Select `SQLServer`
   - Click **OK**

5. **Save Configuration**
   - Click **Save** at the top of the Configuration page
   - Click **Continue** to confirm the application restart

6. **Verify Configuration**
   - The app will automatically restart with the new connection string
   - Connection strings set in Azure Portal automatically override `appsettings.json` values

### Production Connection String Format

```
Server=tcp:orkinosai.database.windows.net,1433;Database=mosaic-saas;User ID=<PRODUCTION_USER>;Password=<PRODUCTION_PASSWORD>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;MultipleActiveResultSets=False;Persist Security Info=False;
```

**Replace the following placeholders:**
- `<PRODUCTION_USER>`: Your production database user (NOT `sqladmin` - use a service account)
- `<PRODUCTION_PASSWORD>`: Your secure production password

### Best Practices for Production Credentials

1. **Use Dedicated Service Account**
   - Create a dedicated database user for the application (e.g., `mosaic_app_user`)
   - DO NOT use the `sqladmin` administrative account in production
   - Grant only necessary permissions (`db_datareader`, `db_datawriter`, `db_ddladmin` if migrations needed)

2. **Strong Password Requirements**
   - Use a strong, randomly generated password
   - Minimum 16 characters
   - Include uppercase, lowercase, numbers, and special characters

3. **Rotate Credentials Regularly**
   - Change production database passwords every 90 days
   - Update in Azure Portal after rotation

4. **Use Azure Key Vault (Recommended for Enterprise)**
   - Store connection strings in Azure Key Vault
   - Reference Key Vault secrets in App Service Configuration
   - Enable Managed Identity for secure access

## Alternative Configuration Methods

### Option 1: Environment Variables (Azure App Service)

Instead of using Connection Strings, you can set the connection string as an Application Setting:

1. Navigate to **Configuration** → **Application settings**
2. Click **+ New application setting**
3. Enter:
   - **Name**: `ConnectionStrings__DefaultConnection`
   - **Value**: Your production connection string
4. Click **OK** → **Save**

**Note:** Use double underscores (`__`) to represent nested configuration sections.

### Option 2: Azure Key Vault Integration

For enterprise deployments, integrate with Azure Key Vault:

1. **Create a Key Vault**
   ```bash
   az keyvault create --name mosaic-keyvault --resource-group mosaic-rg --location eastus
   ```

2. **Store Connection String**
   ```bash
   az keyvault secret set --vault-name mosaic-keyvault --name "DefaultConnection" --value "Server=tcp:..."
   ```

3. **Enable Managed Identity on App Service**
   ```bash
   az webapp identity assign --name mosaic-cms-production --resource-group mosaic-rg
   ```

4. **Grant Access to Key Vault**
   ```bash
   az keyvault set-policy --name mosaic-keyvault --object-id <MANAGED_IDENTITY_ID> --secret-permissions get list
   ```

5. **Reference in App Service Configuration**
   - In Configuration → Application settings
   - **Name**: `ConnectionStrings__DefaultConnection`
   - **Value**: `@Microsoft.KeyVault(SecretUri=https://mosaic-keyvault.vault.azure.net/secrets/DefaultConnection/)`

## Troubleshooting

### Issue: Application cannot connect to database

**Symptoms:**
- HTTP 400 errors on login
- "Connection string 'DefaultConnection' not found" errors
- Startup failures

**Solutions:**

1. **Verify Database Configuration**
   - Check that `DatabaseProvider` is set to `"SqlServer"` in `appsettings.Production.json`
   - Verify `DatabaseEnabled` is set to `true`

2. **Verify Connection String**
   - Ensure connection string is set in Azure Portal under Configuration → Connection strings
   - Verify the connection string format matches Azure SQL format
   - Test connection string using SQL Server Management Studio or Azure Data Studio

3. **Check Firewall Rules**
   - Navigate to SQL Server → Networking
   - Ensure "Allow Azure services and resources to access this server" is checked
   - Add App Service outbound IP addresses to firewall rules if needed

4. **Check Credentials**
   - Verify username and password are correct
   - Ensure user has appropriate database permissions

5. **Review Application Logs**
   - Navigate to App Service → Log stream
   - Look for database connection errors
   - Check for authentication failures

### Issue: Detailed errors showing in production

**Solution:**
- Verify `ErrorHandling.ShowDetailedErrors` is set to `false` in `appsettings.Production.json`
- Verify `ErrorHandling.IncludeStackTrace` is set to `false`
- Restart the App Service after making changes

### Issue: Configuration changes not taking effect

**Solution:**
- Click **Save** in Azure Portal Configuration page
- Restart the App Service manually if needed
- Check that the correct environment is being used (`ASPNETCORE_ENVIRONMENT=Production`)

## Configuration Checklist

Before deploying to production, verify:

- [ ] `DatabaseProvider` is set to `"SqlServer"` (not `"SQLite"`)
- [ ] `DatabaseEnabled` is set to `true`
- [ ] `SqliteConnection` is NOT present in production configuration
- [ ] `ErrorHandling.ShowDetailedErrors` is set to `false`
- [ ] `ErrorHandling.IncludeStackTrace` is set to `false`
- [ ] Production connection string is configured in Azure Portal (NOT in appsettings.json)
- [ ] Connection string uses dedicated service account (NOT sqladmin)
- [ ] SQL Server firewall allows Azure services
- [ ] Application setting `ASPNETCORE_ENVIRONMENT` is set to `Production`
- [ ] Database migrations have been applied
- [ ] Application logs show successful database connection

## Related Documentation

- [Azure Deployment Guide](./AZURE_DEPLOYMENT.md) - Complete deployment instructions
- [Database Setup](./DATABASE.md) - Database configuration and migrations
- [Deployment Checklist](./DEPLOYMENT_CHECKLIST.md) - Pre-deployment verification

## Support

For issues related to production configuration:
1. Check Application Insights or App Service logs
2. Review this documentation
3. Consult the Azure Deployment Guide
4. Contact the development team

---

**Last Updated:** December 2025  
**Version:** 1.0
