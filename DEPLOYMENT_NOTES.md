# SQL Server Connection String Configuration - Deployment Notes

⚠️ **IMPORTANT: This document is DEPRECATED. Please see [AZURE_CONNECTION_STRING_SETUP.md](./AZURE_CONNECTION_STRING_SETUP.md) for current instructions.**

## Overview

**CRITICAL SECURITY NOTICE:** Production database credentials must NEVER be committed to source control.

The CMS (OrkinosaiCMS.Web) project uses placeholder connection strings in `appsettings.Production.json`. Actual production credentials MUST be configured via Azure App Service Configuration.

**Connection String Format:**
```
Server=tcp:orkinosai.database.windows.net,1433;
Initial Catalog=mosaic-saas;
User ID=<your-service-account>;
Password=<your-secure-password>;
Encrypt=True;
TrustServerCertificate=False;
Connection Timeout=30;
MultipleActiveResultSets=False;
Persist Security Info=False;
```

**➡️ For complete setup instructions, see [AZURE_CONNECTION_STRING_SETUP.md](./AZURE_CONNECTION_STRING_SETUP.md)**

## Connection String Configuration

**Production Environment:**
- Connection strings MUST be configured in Azure Portal under Configuration > Connection strings
- **Name:** `DefaultConnection`
- **Type:** `SQLServer`
- See [AZURE_CONNECTION_STRING_SETUP.md](./AZURE_CONNECTION_STRING_SETUP.md) for step-by-step instructions

**Development Environment:**
- Use `appsettings.Development.json` or User Secrets
- Never commit real credentials to source control

## Environment Overrides and Deployment Considerations

### Security Warning ⚠️

The connection string in `appsettings.json` contains development/test credentials. These are explicitly documented as **DEVELOPMENT/TEST ONLY** credentials in the OrkinosaiCMS.Web configuration file and should **NEVER be used in production**.

### Production Deployment - Best Practices

For production deployments, you should override the connection string using one of the following methods (in order of recommendation):

#### 1. Azure App Service - Connection Strings (Recommended)

Navigate to Azure Portal → Your App Service → Configuration → Connection Strings:

- **Name**: `DefaultConnection`
- **Value**: `Server=tcp:orkinosai.database.windows.net,1433;Initial Catalog=mosaic-saas;User ID=<your-service-account>;Password=<your-secure-password>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;MultipleActiveResultSets=False;Persist Security Info=False;`
- **Type**: `SQLServer`

**Security Best Practice**: Use a dedicated service account (e.g., `mosaic_app_user`) with minimal required permissions. Grant only the necessary database permissions (e.g., `db_datareader`, `db_datawriter`, `db_ddladmin` if migrations are needed).

**➡️ For detailed instructions, see [AZURE_CONNECTION_STRING_SETUP.md](./AZURE_CONNECTION_STRING_SETUP.md)**

Click **Save** and **Restart** the app.

**Note**: Connection strings configured in Azure Portal automatically override `appsettings.json` values without needing the `ConnectionStrings__` prefix.

#### 2. Azure Key Vault (Enterprise/Production)

For the most secure production deployments:

```bash
# Create Key Vault secret
az keyvault secret set \
  --vault-name mosaic-keyvault \
  --name DefaultConnection \
  --value "Server=tcp:orkinosai.database.windows.net,1433;Initial Catalog=mosaic-saas;User ID=<your-service-account>;Password=<your-secure-password>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;MultipleActiveResultSets=False;Persist Security Info=False;"
```

**Security Best Practice**: Use a dedicated service account with least privilege access. Create a SQL user specifically for the application with only the required permissions rather than using the `sqladmin` administrative account.

Ensure the application has a managed identity configured with access to the Key Vault. See `/docs/appsettings.md` for detailed setup instructions.

#### 3. Environment Variables

Set the environment variable using double underscore notation:

```bash
ConnectionStrings__DefaultConnection="Server=tcp:orkinosai.database.windows.net,1433;..."
```

This method works for:
- Azure App Service (Configuration → Application Settings)
- Docker/Docker Compose
- AWS Elastic Beanstalk
- Local development

### Configuration Priority

ASP.NET Core configuration is loaded in the following priority (highest to lowest):

1. **Command-line arguments** (highest priority)
2. **Environment variables**
3. **Azure Key Vault** (production)
4. **User Secrets** (development only, via `dotnet user-secrets`)
5. **appsettings.{Environment}.json** (e.g., appsettings.Production.json)
6. **appsettings.json** (lowest priority)

This means environment variables and Azure configuration will always override the values in `appsettings.json`.

### Local Development

For local development with user secrets:

```bash
# Navigate to project directory
cd src/OrkinosaiCMS.Web

# Set connection string via user secrets (for local development)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=tcp:orkinosai.database.windows.net,1433;Initial Catalog=mosaic-saas;User ID=<dev-account>;Password=<dev-password>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30"

# Verify
dotnet user-secrets list
```

## Testing Database Connectivity

After deployment, verify that the applications connect successfully to the Azure SQL database:

1. Check application logs for any database connection errors
2. Verify that both applications can read/write to the database
3. Test that data is shared correctly between CMS and Mosaic applications

## Additional Resources

- **➡️ CURRENT GUIDE: [AZURE_CONNECTION_STRING_SETUP.md](./AZURE_CONNECTION_STRING_SETUP.md)** - Step-by-step Azure setup
- **Comprehensive Configuration Guide**: `/docs/appsettings.md`
- **Azure Deployment Guide**: `/docs/AZURE_DEPLOYMENT.md`
- **Database Setup Guide**: `/docs/DATABASE.md`
- **Security Best Practices**: `/docs/appsettings.md#-security-best-practices`
- **Troubleshooting**: [TROUBLESHOOTING_HTTP_500_30.md](./TROUBLESHOOTING_HTTP_500_30.md)

## Build Verification

Both projects have been verified to build successfully with the new configuration:

- ✅ MosaicCMS builds with 0 errors, 0 warnings
- ✅ OrkinosaiCMS.Web builds with 0 errors, 0 warnings
- ✅ JSON syntax validated for both appsettings.json files

---

**Status**: DEPRECATED - See [AZURE_CONNECTION_STRING_SETUP.md](./AZURE_CONNECTION_STRING_SETUP.md)  
**Last Updated**: December 17, 2025  
**Superseded By**: AZURE_CONNECTION_STRING_SETUP.md
