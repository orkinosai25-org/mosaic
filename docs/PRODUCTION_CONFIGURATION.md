# Production Configuration Guide

This guide explains how to properly configure Mosaic CMS for production deployment with SQL Server.

## Overview

The production configuration is managed through `appsettings.Production.json`. This document outlines the correct configuration for SQL Server database connections and best practices for securing credentials.

## Configuration Method (Updated December 2025)

**IMPORTANT:** This application is configured to use connection strings directly from `appsettings.Production.json`. While environment variables and Azure Key Vault are supported as alternative methods, the primary and recommended configuration for this deployment is file-based configuration.

## Configuration Changes (December 2025)

The `appsettings.Production.json` file has been updated with a complete connection string template and comprehensive troubleshooting documentation.

### Database Configuration

The `appsettings.Production.json` file contains the complete database configuration template:

```json
{
  "DatabaseEnabled": true,
  "DatabaseProvider": "SqlServer",
  "Database": {
    "AutoApplyMigrations": true
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=YOUR_DATABASE;Persist Security Info=False;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

**Key Settings:**
- `DatabaseEnabled`: Set to `true` to enable database connectivity
- `DatabaseProvider`: Set to `"SqlServer"` for Azure SQL Database or on-premises SQL Server
- `Database.AutoApplyMigrations`: Set to `true` to automatically apply database migrations on startup (creates all required tables including AspNetUsers)
- `ConnectionStrings.DefaultConnection`: **MUST be updated** with your actual SQL Server connection details

## How to Configure Connection String in appsettings.Production.json

### Step 1: Locate the Configuration File

The configuration file is located at:
```
src/OrkinosaiCMS.Web/appsettings.Production.json
```

### Step 2: Update the Connection String

Open `appsettings.Production.json` and find the `ConnectionStrings` section. Replace the placeholder values with your actual SQL Server details:

**For Azure SQL Database:**
```json
"DefaultConnection": "Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=YourDatabase;Persist Security Info=False;User ID=yourusername;Password=yourpassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

**For On-Premises SQL Server:**
```json
"DefaultConnection": "Server=YOUR_SERVER_NAME;Database=YOUR_DATABASE;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;Connection Timeout=30;"
```

**For SQL Server Express (Local Development/Testing):**
```json
"DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=OrkinosaiCMS_Production;User ID=sa;Password=YOUR_SA_PASSWORD;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
```

### Step 3: Replace Placeholder Values

Update these placeholder values in your chosen connection string:

| Placeholder | Description | Example |
|-------------|-------------|---------|
| `YOUR_SERVER` | SQL Server hostname or Azure SQL server name | `tcp:myserver.database.windows.net,1433` or `MYSERVER\SQLEXPRESS` |
| `YOUR_DATABASE` | Database name | `OrkinosaiCMS_Production` or `mosaic-cms` |
| `YOUR_USERNAME` | SQL authentication username | `mosaicadmin` or `sa` |
| `YOUR_PASSWORD` | SQL authentication password | Your secure password |

### Step 4: Verify Configuration

After updating the connection string, verify these settings in `appsettings.Production.json`:

```json
{
  "DatabaseEnabled": true,
  "DatabaseProvider": "SqlServer",
  "Database": {
    "AutoApplyMigrations": true
  }
}
```

**Important:** `AutoApplyMigrations` must be `true` for the application to automatically create database tables (AspNetUsers, AspNetRoles, etc.) on first startup.

### Step 5: Protect the Configuration File

**SECURITY CONSIDERATIONS:**

1. **File Permissions:** Restrict read access to `appsettings.Production.json` to authorized users only
2. **Source Control:** Consider using `.gitignore` to prevent committing production credentials
3. **Deployment:** Use secure deployment methods (SFTP/SSH with credentials, Azure DevOps secure variables, etc.)
4. **Backups:** Ensure backup processes do not expose credentials in logs or unencrypted storage

**Alternative (More Secure) Configuration Methods:**

While file-based configuration is supported and enabled, you may also use:

- **Environment Variables:** Set `ConnectionStrings__DefaultConnection` environment variable (overrides appsettings.json)
- **Azure Key Vault:** Store connection string in Key Vault and reference it via Azure App Service Configuration
- **Azure App Service Configuration:** Add connection string in Azure Portal under App Service → Configuration → Connection strings

These alternatives provide additional security layers but require additional setup. The file-based method in `appsettings.Production.json` is simpler for deployments where the file can be adequately protected.

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

### Issue: HTTP Error 500.30 - Application Cannot Start

**Symptoms:**
- Application fails to start with HTTP Error 500.30
- Error message: "A network-related or instance-specific error occurred while establishing a connection to SQL Server"
- Error: "The server was not found or was not accessible. (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)"
- Win32Exception (5): Access is denied
- Migrations cannot run, AspNetUsers table is not created
- Green allow button (Sign In button) is not shown on `/admin/login`

**Root Cause:**
The application cannot connect to SQL Server to apply database migrations and create essential tables (AspNetUsers, AspNetRoles, etc.). Without these tables, admin login fails and the application cannot start.

**Solutions:**

1. **Verify Connection String is Configured**
   - Open `src/OrkinosaiCMS.Web/appsettings.Production.json`
   - Locate the `ConnectionStrings.DefaultConnection` property
   - Ensure placeholder values (`YOUR_SERVER`, `YOUR_DATABASE`, `YOUR_USERNAME`, `YOUR_PASSWORD`) have been replaced with actual values
   - **Example of what NOT to use:**
     ```json
     "DefaultConnection": "Server=tcp:YOUR_SERVER.database.windows.net,1433;..."
     ```
   - **Example of properly configured connection string:**
     ```json
     "DefaultConnection": "Server=tcp:myserver.database.windows.net,1433;Initial Catalog=MosaicCMS;User ID=mosaicadmin;Password=MySecureP@ssw0rd123;..."
     ```

2. **Verify SQL Server is Accessible**
   - **For Azure SQL Database:**
     - Verify SQL Server firewall rules allow connections from your deployment environment
     - In Azure Portal → SQL Server → Networking → Firewall rules
     - Add rule "Allow Azure services" or add specific IP addresses
   - **For On-Premises SQL Server:**
     - Verify SQL Server service is running
     - Verify TCP/IP protocol is enabled in SQL Server Configuration Manager
     - Verify Windows Firewall allows SQL Server connections (port 1433)
     - Verify network connectivity between application server and SQL Server

3. **Fix Access Denied (Win32Exception Error 5)**
   
   This error occurs when using Windows Authentication without proper permissions. **Use SQL Authentication instead:**
   
   - **WRONG (Windows Authentication - causes Access Denied):**
     ```json
     "DefaultConnection": "Server=MYSERVER;Database=MosaicCMS;Trusted_Connection=True;..."
     ```
   
   - **CORRECT (SQL Authentication - recommended for production):**
     ```json
     "DefaultConnection": "Server=MYSERVER;Database=MosaicCMS;User ID=mosaicuser;Password=SecurePassword123;Encrypt=True;TrustServerCertificate=True;..."
     ```

4. **Verify Database Exists**
   - The database specified in `Initial Catalog` or `Database` parameter must exist
   - Create the database manually if it doesn't exist:
     ```sql
     CREATE DATABASE MosaicCMS;
     ```
   - Or use SQL Server Management Studio / Azure Data Studio to create the database

5. **Verify User Credentials**
   - Test the connection string using SQL Server Management Studio or Azure Data Studio
   - Verify the username and password are correct
   - Ensure the SQL Server user has necessary permissions:
     ```sql
     USE MosaicCMS;
     ALTER ROLE db_datareader ADD MEMBER mosaicuser;
     ALTER ROLE db_datawriter ADD MEMBER mosaicuser;
     ALTER ROLE db_ddladmin ADD MEMBER mosaicuser; -- Required for migrations
     ```

6. **Check Application Logs**
   - Review application startup logs for specific error messages
   - Look for SQL error codes:
     - **Error 40:** Cannot connect to server (network/firewall issue)
     - **Error 18456:** Login failed (incorrect credentials)
     - **Error 208:** Invalid object name 'AspNetUsers' (migrations not applied)
     - **Win32Exception (5):** Access denied (Windows auth permission issue)

7. **Verify AutoApplyMigrations is Enabled**
   - In `appsettings.Production.json`, ensure:
     ```json
     "Database": {
       "AutoApplyMigrations": true
     }
     ```
   - This setting ensures migrations run automatically on startup to create required tables

### Issue: Green Allow Button Not Shown / Admin Login Fails

**Symptoms:**
- Cannot see the green "Sign In" button on `/admin/login` page
- Login form doesn't work or shows errors
- Logs show: "Invalid object name 'AspNetUsers'" (SQL Error 208)

**Root Cause:**
Database migrations have not been applied. The AspNetUsers table and other Identity tables don't exist in the database.

**Solutions:**

1. **Verify Database Connection** (see above)

2. **Manually Apply Migrations** (if AutoApplyMigrations didn't work)
   ```bash
   cd src/OrkinosaiCMS.Infrastructure
   dotnet ef database update --startup-project ../OrkinosaiCMS.Web
   ```

3. **Verify Migrations Were Applied**
   - Connect to the database using SQL Server Management Studio or Azure Data Studio
   - Check that these tables exist:
     - `AspNetUsers`
     - `AspNetRoles`
     - `AspNetUserRoles`
     - `AspNetUserClaims`
     - `__EFMigrationsHistory`
   
4. **Check Migration History**
   ```sql
   SELECT * FROM __EFMigrationsHistory ORDER BY MigrationId;
   ```
   - Should show all applied migrations

5. **Restart the Application**
   - After fixing connection string or applying migrations, restart the application
   - The startup process will seed the default admin user
   - The green "Sign In" button should now appear on `/admin/login`

### Issue: Application Logs Show Migration Errors

**Symptoms:**
- Startup logs show database migration failed
- Error messages contain "pending migrations" or "could not apply migration"

**Solutions:**

1. **Check Database Permissions**
   - The SQL user needs `db_ddladmin` role to create/modify tables
   - Grant permission:
     ```sql
     USE MosaicCMS;
     ALTER ROLE db_ddladmin ADD MEMBER mosaicuser;
     ```

2. **Manually Apply Migrations**
   ```bash
   cd src/OrkinosaiCMS.Infrastructure
   dotnet ef database update --startup-project ../OrkinosaiCMS.Web
   ```

3. **Check for Conflicting Migrations**
   - Review `__EFMigrationsHistory` table for partial or failed migrations
   - If needed, manually remove failed migration records and reapply

### Issue: Detailed errors showing in production

**Solution:**
- Verify `ErrorHandling.ShowDetailedErrors` is set to `false` in `appsettings.Production.json`
- Verify `ErrorHandling.IncludeStackTrace` is set to `false`
- Restart the application after making changes

### Issue: Configuration changes not taking effect

**Solution:**
- Save changes to `appsettings.Production.json`
- Restart the application (or IIS/App Service if applicable)
- Verify the correct environment is being used (`ASPNETCORE_ENVIRONMENT=Production`)
- Check application logs for configuration loading messages

## Configuration Checklist

Before deploying to production, verify:

- [ ] **Connection String Configured:** `ConnectionStrings.DefaultConnection` in `appsettings.Production.json` has been updated with actual SQL Server details (no placeholder values)
- [ ] **Database Settings:**
  - [ ] `DatabaseProvider` is set to `"SqlServer"` (not `"SQLite"` or `"InMemory"`)
  - [ ] `DatabaseEnabled` is set to `true`
  - [ ] `Database.AutoApplyMigrations` is set to `true` (or migrations applied manually)
- [ ] **Security Settings:**
  - [ ] `ErrorHandling.ShowDetailedErrors` is set to `false`
  - [ ] `ErrorHandling.IncludeStackTrace` is set to `false`
  - [ ] Connection string uses SQL authentication with strong password
  - [ ] `appsettings.Production.json` file permissions are restricted
- [ ] **Network and Access:**
  - [ ] SQL Server is accessible from application server (firewall rules configured)
  - [ ] Database exists and is accessible with provided credentials
  - [ ] SQL user has required permissions (db_datareader, db_datawriter, db_ddladmin)
- [ ] **Deployment:**
  - [ ] Environment variable `ASPNETCORE_ENVIRONMENT` is set to `Production`
  - [ ] Application restarts successfully without errors
  - [ ] Database migrations applied successfully (check logs)
  - [ ] AspNetUsers and other Identity tables exist in database
  - [ ] Admin login works at `/admin/login` (green "Sign In" button appears)

## Related Documentation

- [Azure Deployment Guide](./AZURE_DEPLOYMENT.md) - Complete deployment instructions
- [Database Setup](./DATABASE.md) - Database configuration and migrations
- [Deployment Checklist](./DEPLOYMENT_CHECKLIST.md) - Pre-deployment verification
- [Database Migration Troubleshooting](./DATABASE_MIGRATION_TROUBLESHOOTING.md) - Migration-specific issues

## Quick Reference: Connection String Examples

### Azure SQL Database
```json
"DefaultConnection": "Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=YourDatabase;Persist Security Info=False;User ID=yourusername;Password=yourpassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

### On-Premises SQL Server
```json
"DefaultConnection": "Server=YOUR_SERVER_NAME;Database=YOUR_DATABASE;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;Connection Timeout=30;"
```

### SQL Server Express (Local)
```json
"DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=OrkinosaiCMS_Production;User ID=sa;Password=YOUR_SA_PASSWORD;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
```

## Support

For issues related to production configuration:
1. Review application startup logs for specific error messages and SQL error codes
2. Consult this documentation and troubleshooting section
3. Review the Database Migration Troubleshooting guide
4. Check the Azure Deployment Guide for deployment-specific issues
5. Contact the development team with specific error messages and logs

---

**Last Updated:** December 18, 2025  
**Version:** 2.0 (Updated for file-based configuration in appsettings.Production.json)
