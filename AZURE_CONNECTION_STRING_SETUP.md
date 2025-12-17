# Azure App Service Connection String Configuration

**CRITICAL:** Production database credentials must NEVER be committed to source control. This guide explains how to properly configure your Azure SQL Database connection string in Azure App Service.

## Quick Setup (Required Before First Deployment)

### Step 1: Configure Connection String in Azure Portal

1. Navigate to [Azure Portal](https://portal.azure.com)
2. Go to **App Services** → Select your app: `mosaic-saas`
3. In the left menu, select **Configuration**
4. Click on **Connection strings** tab
5. Click **+ New connection string**
6. Configure:
   - **Name**: `DefaultConnection`
   - **Value**: Your Azure SQL connection string (see format below)
   - **Type**: `SQLServer`
7. Click **OK**
8. Click **Save** at the top
9. Confirm **Continue** when prompted (app will restart)

### Step 2: Connection String Format

Use this format for your Azure SQL Database connection string:

```
Server=tcp:orkinosai.database.windows.net,1433;Initial Catalog=mosaic-saas;User ID=<your-sql-username>;Password=<your-sql-password>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;MultipleActiveResultSets=False;Persist Security Info=False;
```

**Replace:**
- `<your-sql-username>` - Your Azure SQL username
- `<your-sql-password>` - Your Azure SQL password

**Security Best Practices:**
- ✅ Use a dedicated service account (NOT `sqladmin` or `sa`)
- ✅ Grant only required permissions: `db_datareader`, `db_datawriter`, `db_ddladmin` (if migrations needed)
- ✅ Use strong passwords: 16+ characters, mixed case, numbers, special characters
- ✅ Rotate credentials every 90 days
- ✅ Consider Azure Managed Identity for enhanced security (no passwords needed)

## Why This is Required

The `appsettings.Production.json` file contains a **placeholder** connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=AZURE_SQL_SERVER;Database=AZURE_SQL_DATABASE;..."
  }
}
```

This placeholder will cause the application to fail with **HTTP Error 500.30** unless you configure the actual connection string in Azure Portal.

### Azure App Service Configuration Priority

Azure App Service applies configuration in this order (highest to lowest priority):

1. **Azure Portal Configuration > Connection strings** ← Use this for production
2. **Azure Portal Configuration > Application settings**
3. `appsettings.Production.json` (deployed with app)
4. `appsettings.json` (deployed with app)

Connection strings configured in Azure Portal override those in appsettings files, which is exactly what we want for security.

## Verifying Configuration

### Method 1: Check Azure Portal

1. Azure Portal → App Service → **Configuration** → **Connection strings**
2. Verify `DefaultConnection` is listed with Type: `SQLServer`
3. You should see the value as `Hidden value. Click to show value`

### Method 2: Check Application Logs

After deployment:

1. Azure Portal → App Service → **Log Stream**
2. Look for log entries like:
   ```
   [Information] Using SQL Server database provider
   [Information] Connection string (sanitized): Server=tcp:orkinosai.database.windows.net,...Password=***...
   ```
3. If you see `Connection string 'DefaultConnection' not found`, the configuration is missing

### Method 3: Test Health Endpoint

```bash
curl https://mosaic-saas.azurewebsites.net/api/health
```

Expected response (if configured correctly):
```json
{
  "status": "Healthy",
  "timestamp": "2024-12-17T..."
}
```

## Alternative Configuration Methods

### Option 1: Environment Variables (Local Development)

For local testing with Azure SQL:

```bash
# Windows PowerShell
$env:ConnectionStrings__DefaultConnection="Server=tcp:orkinosai.database.windows.net,1433;..."

# macOS/Linux
export ConnectionStrings__DefaultConnection="Server=tcp:orkinosai.database.windows.net,1433;..."
```

### Option 2: User Secrets (Local Development - Recommended)

```bash
cd src/OrkinosaiCMS.Web
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=tcp:orkinosai.database.windows.net,1433;..."
```

### Option 3: Azure Key Vault (Enterprise - Most Secure)

For enterprise deployments:

1. Create Azure Key Vault
2. Add connection string as secret
3. Grant App Service Managed Identity access to Key Vault
4. Reference in App Service Configuration:
   ```
   @Microsoft.KeyVault(SecretUri=https://your-keyvault.vault.azure.net/secrets/DefaultConnection/)
   ```

## Troubleshooting

### Error: "Connection string 'DefaultConnection' not found"

**Cause:** Connection string not configured in Azure Portal

**Solution:**
1. Follow Step 1 above to add connection string
2. Restart the app service
3. Check logs to verify

### Error: "Cannot open server... Azure Services and resources to access this server"

**Cause:** Azure SQL firewall blocking connections

**Solution:**
1. Azure Portal → SQL Server → **Firewalls and virtual networks**
2. Set **Allow Azure services and resources to access this server**: **Yes**
3. Click **Save**

### Error: "Login failed for user..."

**Cause:** Invalid credentials

**Solution:**
1. Verify username and password are correct
2. Check if user exists in database
3. Verify user has required permissions

### Error: HTTP 500.30 - App failed to start

**Cause:** Multiple possible causes

**Solution:**
1. Check stdout logs: Azure Portal → App Service → **Advanced Tools (Kudu)** → **LogFiles/stdout**
2. Enable Application Logging: App Service → **App Service logs** → Application Logging: **On**
3. View Log Stream: App Service → **Log stream**
4. See `TROUBLESHOOTING_HTTP_500_30.md` for detailed troubleshooting steps

## Security Checklist

Before deploying to production:

- [ ] Connection string configured in Azure Portal (NOT in appsettings.json)
- [ ] Using a dedicated service account (not `sqladmin`)
- [ ] Strong password: 16+ characters, mixed case, numbers, special chars
- [ ] SQL Server firewall configured to allow Azure services
- [ ] Database user has minimal required permissions
- [ ] Application logs do NOT expose passwords
- [ ] Consider implementing Azure Managed Identity
- [ ] Schedule credential rotation (every 90 days)

## Database Migration

After configuring the connection string, ensure database migrations are applied:

### Option 1: Automatic (Default)

The application automatically applies migrations on startup when:
```json
{
  "Database": {
    "AutoApplyMigrations": true
  }
}
```

Watch logs during startup for:
```
[Information] Applying pending database migrations...
[Information] Database migrations applied successfully
```

### Option 2: Manual (Production Best Practice)

For production with strict change control:

1. Set in Azure Portal Configuration:
   - **Name**: `Database__AutoApplyMigrations`
   - **Value**: `false`

2. Apply migrations manually before deployment:
   ```bash
   # Local machine with connection to Azure SQL
   cd src/OrkinosaiCMS.Web
   dotnet ef database update --connection "Server=tcp:orkinosai.database.windows.net,1433;..."
   ```

3. Or use migration scripts:
   ```bash
   chmod +x scripts/apply-migrations.sh
   ./scripts/apply-migrations.sh
   ```

## Related Documentation

- [TROUBLESHOOTING_HTTP_500_30.md](./TROUBLESHOOTING_HTTP_500_30.md) - Troubleshooting deployment errors
- [docs/PRODUCTION_CONFIGURATION.md](./docs/PRODUCTION_CONFIGURATION.md) - Complete production configuration guide
- [docs/AZURE_DEPLOYMENT.md](./docs/AZURE_DEPLOYMENT.md) - Azure deployment instructions
- [docs/DATABASE.md](./docs/DATABASE.md) - Database setup and migrations

## Support

If you encounter issues:

1. Check Application Insights or App Service logs
2. Review [TROUBLESHOOTING_HTTP_500_30.md](./TROUBLESHOOTING_HTTP_500_30.md)
3. Verify all steps in this guide are completed
4. Check Azure SQL Server firewall rules
5. Verify database user permissions

---

**Last Updated:** December 17, 2024  
**App Service Name:** mosaic-saas  
**Azure SQL Server:** orkinosai.database.windows.net  
**Database Name:** mosaic-saas
