# Azure Connection String Configuration Guide

## Problem: HTTP Error 500.30 - Missing Connection String

If you see **HTTP Error 500.30 - ASP.NET Core app failed to start**, the most common cause is a missing or incorrectly configured database connection string.

## ✅ Correct Way: Azure App Service Configuration

**IMPORTANT**: Production database credentials should **NEVER** be hardcoded in `appsettings.Production.json`. Always use Azure App Service Configuration.

### Step-by-Step Instructions

#### Method 1: Azure Portal (Recommended)

1. **Navigate to your App Service**
   - Go to [Azure Portal](https://portal.azure.com)
   - Select your App Service (e.g., `mosaic-saas`)

2. **Open Configuration Settings**
   - In the left sidebar, click **Configuration**
   - Click on the **Connection strings** tab

3. **Add Connection String**
   - Click **+ New connection string**
   - Fill in the following:
     - **Name**: `DefaultConnection` (must be exactly this)
     - **Value**: Your Azure SQL connection string (see format below)
     - **Type**: `SQLServer`
   - Click **OK**

4. **Save and Restart**
   - Click **Save** at the top
   - Click **Continue** to confirm
   - Wait for the app to restart (may take 1-2 minutes)

5. **Verify**
   - Check the health endpoint: `https://your-app.azurewebsites.net/api/health`
   - Should return HTTP 200 with status "Healthy"

#### Method 2: Azure CLI

```bash
# Set the connection string
az webapp config connection-string set \
  --name mosaic-saas \
  --resource-group orkinosai_group \
  --connection-string-type SQLServer \
  --settings DefaultConnection="Server=tcp:orkinosai.database.windows.net,1433;Initial Catalog=mosaic-saas;User ID=sqladmin;Password=YOUR_PASSWORD;Encrypt=True;Connection Timeout=30;Max Pool Size=100;Min Pool Size=5;Pooling=true"

# Restart the app
az webapp restart \
  --name mosaic-saas \
  --resource-group orkinosai_group
```

**Important**: Replace `YOUR_PASSWORD` with your actual database password.

#### Method 3: Environment Variable (Local Testing)

For local testing only (not recommended for production):

```bash
# Windows PowerShell
$env:ConnectionStrings__DefaultConnection="Server=tcp:orkinosai.database.windows.net,1433;Initial Catalog=mosaic-saas;User ID=sqladmin;Password=YOUR_PASSWORD;Encrypt=True;Connection Timeout=30;Max Pool Size=100;Min Pool Size=5;Pooling=true"

# Linux/macOS
export ConnectionStrings__DefaultConnection="Server=tcp:orkinosai.database.windows.net,1433;Initial Catalog=mosaic-saas;User ID=sqladmin;Password=YOUR_PASSWORD;Encrypt=True;Connection Timeout=30;Max Pool Size=100;Min Pool Size=5;Pooling=true"
```

## Connection String Format

### Azure SQL Database

```
Server=tcp:YOUR_SERVER.database.windows.net,1433;
Initial Catalog=YOUR_DATABASE;
Persist Security Info=False;
User ID=YOUR_USERNAME;
Password=YOUR_PASSWORD;
MultipleActiveResultSets=False;
Encrypt=True;
TrustServerCertificate=False;
Connection Timeout=30;
Max Pool Size=100;
Min Pool Size=5;
Pooling=true
```

### Required Parameters

| Parameter | Description | Example |
|-----------|-------------|---------|
| `Server` | Azure SQL server address | `tcp:orkinosai.database.windows.net,1433` |
| `Initial Catalog` | Database name | `mosaic-saas` |
| `User ID` | SQL username | `sqladmin` |
| `Password` | SQL password | `YourSecurePassword123!` |
| `Encrypt` | Use encryption | `True` (required for Azure SQL) |
| `Connection Timeout` | Connection timeout in seconds | `30` |
| `Max Pool Size` | Maximum connection pool size | `100` (prevents HTTP 503) |
| `Min Pool Size` | Minimum connection pool size | `5` |
| `Pooling` | Enable connection pooling | `true` (prevents HTTP 503) |

### Optional Parameters

| Parameter | Description | Default | Recommended |
|-----------|-------------|---------|-------------|
| `MultipleActiveResultSets` | Allow multiple result sets | `False` | `False` for Azure SQL |
| `TrustServerCertificate` | Trust server certificate | `False` | `False` for Azure SQL |
| `Persist Security Info` | Keep credentials in connection | `False` | `False` (security) |

## Verifying Configuration

### 1. Check Connection String is Set

**Azure Portal:**
- Go to App Service → Configuration → Connection strings
- Verify `DefaultConnection` exists and has a value

**Azure CLI:**
```bash
az webapp config connection-string list \
  --name mosaic-saas \
  --resource-group orkinosai_group \
  --query "[?name=='DefaultConnection']"
```

### 2. Check Application Logs

**Azure Portal:**
- Go to App Service → Log stream
- Look for startup messages mentioning connection string

**Expected output:**
```
[2024-12-22 01:00:00.000 +00:00] [INF] Using SQL Server database provider
[2024-12-22 01:00:00.001 +00:00] [INF] Connection string (sanitized): Server=tcp:orkinosai.database.windows.net,1433;...Password=***;...
[2024-12-22 01:00:00.002 +00:00] [INF] Connection pool settings: MaxPoolSize=100, MinPoolSize=5, Pooling=True
```

**If you see this error:**
```
[FATAL] CONFIGURATION ERROR: Connection string 'DefaultConnection' is not set.
```
→ The connection string is not configured. Follow the steps above.

### 3. Test Health Endpoint

```bash
curl https://mosaic-saas.azurewebsites.net/api/health
```

**Expected response (HTTP 200):**
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "database",
      "status": "Healthy"
    }
  ]
}
```

**If you get HTTP 500.30:**
→ Connection string is missing or invalid. Check logs.

**If you get HTTP 503:**
→ Database connection is failing. Verify:
- Database server is running
- Firewall allows connections
- Credentials are correct

## Troubleshooting

### Error: "Connection string 'DefaultConnection' is not set"

**Cause**: Connection string not configured in Azure Configuration.

**Solution**: Follow steps above to set connection string in Azure Portal.

### Error: "No such host is known" (Error 11001)

**Cause**: Invalid server name in connection string.

**Solution**: 
- Verify server name: `Server=tcp:YOUR_SERVER.database.windows.net,1433`
- Check for typos in server address
- Ensure server exists in Azure Portal

### Error: "Login failed for user" (Error 18456)

**Cause**: Invalid username or password.

**Solution**:
- Verify credentials in Azure SQL → Authentication
- Reset password if needed
- Update connection string with correct credentials

### Error: "Cannot open server" (Error 40615)

**Cause**: Firewall blocking connection.

**Solution**:
- Azure Portal → SQL Server → Firewalls and virtual networks
- Add rule: "Allow Azure services and resources to access this server" → ON
- Or add specific IP address of App Service

### Error: HTTP 503 Service Unavailable

**Cause**: Connection pool exhausted.

**Solution**:
- Ensure connection string includes:
  - `Max Pool Size=100`
  - `Min Pool Size=5`
  - `Pooling=true`
- These are automatically added by Program.cs if missing

## Security Best Practices

### ✅ DO

- **Set connection string in Azure Configuration** (Connection Strings section)
- **Use Azure Key Vault** for additional security
- **Use SQL authentication** for Azure SQL (not Windows auth)
- **Rotate passwords regularly**
- **Enable firewall rules** to restrict access
- **Use strong passwords** (12+ characters, mixed case, numbers, symbols)
- **Monitor access logs** in Azure SQL

### ❌ DON'T

- **Never commit connection strings to source control** (even in appsettings.Production.json)
- **Never share connection strings** via email or chat
- **Never use weak passwords** (e.g., "Password123")
- **Never disable encryption** (`Encrypt=True` is required)
- **Never trust server certificates blindly** (`TrustServerCertificate=False` for Azure SQL)
- **Never hardcode credentials** in code or config files

## Alternative: Azure Key Vault (Advanced)

For maximum security, use Azure Key Vault:

1. **Store connection string in Key Vault**
   ```bash
   az keyvault secret set \
     --vault-name your-keyvault \
     --name ConnectionStrings--DefaultConnection \
     --value "Server=tcp:..."
   ```

2. **Grant App Service access to Key Vault**
   - Enable Managed Identity on App Service
   - Grant "Get" and "List" permissions to App Service identity

3. **Reference in App Service Configuration**
   - Connection String Name: `DefaultConnection`
   - Value: `@Microsoft.KeyVault(SecretUri=https://your-keyvault.vault.azure.net/secrets/ConnectionStrings--DefaultConnection/)`
   - Type: `SQLServer`

## Related Documentation

- [TROUBLESHOOTING_HTTP_500_30.md](./TROUBLESHOOTING_HTTP_500_30.md) - Complete HTTP 500.30 troubleshooting
- [DEPLOYMENT_NOTES.md](./DEPLOYMENT_NOTES.md) - Deployment guide
- [AZURE_CONNECTION_STRING_SETUP.md](./AZURE_CONNECTION_STRING_SETUP.md) - Legacy setup guide
- [Microsoft Docs: Connection Strings](https://learn.microsoft.com/en-us/azure/app-service/configure-common)
- [Microsoft Docs: Azure SQL Connection](https://learn.microsoft.com/en-us/azure/azure-sql/database/connect-query-content-reference-guide)

## Summary

**To fix HTTP Error 500.30 caused by missing connection string:**

1. ✅ Go to Azure Portal → App Service → Configuration → Connection strings
2. ✅ Add `DefaultConnection` with your Azure SQL connection string
3. ✅ Click Save and restart the app
4. ✅ Verify health endpoint returns HTTP 200
5. ✅ Never hardcode credentials in appsettings.Production.json

**The appsettings.Production.json file should have an empty connection string** - this forces proper configuration via Azure Configuration and prevents security issues.
