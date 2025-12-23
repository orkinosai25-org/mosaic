# Azure App Service Configuration Required

**Date:** December 22, 2025  
**Priority:** CRITICAL - REQUIRED FOR PRODUCTION DEPLOYMENT  
**Related to:** Security fix for committed credentials (UDI5TR)

---

## Overview

As of December 22, 2025, production database credentials have been **removed from source control** for security reasons. The application now **requires Azure App Service Configuration** to be properly configured for production deployments.

## What Changed

### Before (Security Issue)
- Production credentials were committed to `appsettings.Production.json`
- Anyone with repository access could see database password
- Credentials were in git history permanently

### After (Secure)
- ✅ Credentials removed from `appsettings.Production.json`
- ✅ Must be configured in Azure App Service Configuration
- ✅ No credentials in source control
- ✅ Follows security best practices

## Required Azure Configuration

### Step 1: Configure Connection String

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to: **App Services** → **mosaic-saas**
3. In the left menu, select: **Configuration**
4. Click on the **Connection strings** tab
5. Click: **+ New connection string**

### Step 2: Enter Connection String Details

**Name:** `DefaultConnection`  
**Value:**
```
Server=tcp:orkinosai.database.windows.net,1433;Initial Catalog=mosaic-saas;Persist Security Info=False;User ID=sqladmin;Password=YOUR_CURRENT_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Max Pool Size=100;Min Pool Size=5;Pooling=true
```

**Type:** `SQLServer`

**Note:** Replace `YOUR_CURRENT_PASSWORD` with the actual database password.

### Step 3: Save and Restart

1. Click **OK** on the connection string dialog
2. Click **Save** at the top of the Configuration page
3. Click **Continue** when prompted (this will restart the app)
4. Wait for the restart to complete (~30 seconds)

---

## Verification

### Verify Configuration is Set

1. Azure Portal → App Service → **Configuration** → **Connection strings**
2. You should see `DefaultConnection` listed with Type: `SQLServer`
3. Value should show as: `Hidden value. Click to show value`

### Verify Application Works

1. Open: https://mosaic-saas.azurewebsites.net/api/health
2. Expected response:
   ```json
   {
     "status": "Healthy",
     "timestamp": "2025-12-22T..."
   }
   ```

3. Check Application Logs:
   - Azure Portal → App Service → **Log Stream**
   - Look for:
     ```
     [Information] Using SQL Server database provider
     [Information] Connection string (sanitized): Server=tcp:orkinosai.database.windows.net,...Password=***...
     [Information] Database initialization completed successfully
     ```

---

## Additional Recommended Configuration

While you're in the Configuration blade, consider adding these environment variables for enhanced security:

### Azure Blob Storage

**Name:** `AzureBlobStorageConnectionString`  
**Value:** `DefaultEndpointsProtocol=https;AccountName=mosaicsaas;AccountKey=YOUR_KEY;EndpointSuffix=core.windows.net`

### Stripe API Keys (if using payment features)

**Name:** `Payment__Stripe__SecretKey`  
**Value:** `sk_live_YOUR_STRIPE_SECRET_KEY`

**Name:** `Payment__Stripe__PublishableKey`  
**Value:** `pk_live_YOUR_STRIPE_PUBLISHABLE_KEY`

**Name:** `Payment__Stripe__WebhookSecret`  
**Value:** `whsec_YOUR_WEBHOOK_SECRET`

### JWT Authentication (if using API)

**Name:** `Authentication__Jwt__SecretKey`  
**Value:** `your-production-jwt-secret-key-min-32-characters`

---

## Troubleshooting

### Error: "Connection string 'DefaultConnection' not found"

**Cause:** Connection string not configured in Azure Portal  
**Solution:** Follow Step 1-3 above to add the connection string

### Error: "Login failed for user 'sqladmin'"

**Cause:** Incorrect password in connection string  
**Solution:** Verify the password in Azure SQL Database and update the connection string

### Error: HTTP 500.30 - App failed to start

**Cause:** Missing or invalid connection string  
**Solution:** 
1. Check Azure Portal → App Service → **Log Stream** for detailed error
2. Verify connection string is set correctly
3. Check if Azure SQL firewall allows App Service access

### App worked before, now broken after update

**Cause:** Credentials were removed from source control  
**Solution:** This is intentional for security. Follow the configuration steps above.

---

## Security Best Practices

### ✅ DO:
- Store credentials in Azure App Service Configuration
- Use Azure Key Vault for highly sensitive environments
- Rotate passwords regularly (every 90 days)
- Use Managed Identity when possible
- Grant minimal database permissions needed

### ❌ DON'T:
- Commit credentials to source control
- Share passwords in chat or email
- Use default or weak passwords
- Grant excessive database permissions
- Store credentials in plain text files

---

## Password Rotation Procedure

If the database password was compromised (as it was before this fix):

### Step 1: Change Password in Azure SQL

1. Azure Portal → **SQL databases** → **mosaic-saas**
2. Click on the SQL server: **orkinosai.database.windows.net**
3. In the left menu, select: **SQL databases** → **mosaic-saas**
4. Click **Query editor** or use Azure Data Studio
5. Run SQL command:
   ```sql
   ALTER LOGIN sqladmin WITH PASSWORD = 'NewStrongPassword123!@#';
   ```

### Step 2: Update Azure App Service Configuration

1. Azure Portal → **App Services** → **mosaic-saas**
2. **Configuration** → **Connection strings**
3. Click on `DefaultConnection`
4. Update the password in the connection string value
5. Click **OK**, then **Save**
6. App will automatically restart

### Step 3: Verify

1. Check health endpoint: https://mosaic-saas.azurewebsites.net/api/health
2. Verify logs show successful database connection
3. Test application functionality

---

## Alternative: Using Azure Key Vault (Advanced)

For enhanced security, you can store the connection string in Azure Key Vault:

### Setup

1. Create Azure Key Vault (if not exists)
2. Add connection string as a secret: `DefaultConnection`
3. Grant App Service Managed Identity access to Key Vault
4. In App Service Configuration, reference the secret:
   ```
   @Microsoft.KeyVault(SecretUri=https://your-keyvault.vault.azure.net/secrets/DefaultConnection/)
   ```

### Benefits
- Centralized secret management
- Audit logging of secret access
- Automatic rotation support
- No secrets in App Service configuration

---

## Related Documentation

- [AZURE_CONNECTION_STRING_SETUP.md](./AZURE_CONNECTION_STRING_SETUP.md) - Detailed connection string setup
- [COMPREHENSIVE_AUDIT_REPORT_UDI5TR.md](./COMPREHENSIVE_AUDIT_REPORT_UDI5TR.md) - Full audit findings
- [TROUBLESHOOTING_HTTP_500_30.md](./TROUBLESHOOTING_HTTP_500_30.md) - General troubleshooting
- [DEPLOYMENT_VERIFICATION_GUIDE.md](./DEPLOYMENT_VERIFICATION_GUIDE.md) - Post-deployment checks

---

## Summary

**Action Required:** Configure production connection string in Azure App Service Configuration before deploying.

**Why:** Production credentials have been removed from source control for security.

**When:** Before next production deployment or if application is currently not working.

**How:** Follow Steps 1-3 in the "Required Azure Configuration" section above.

---

**Last Updated:** December 22, 2025  
**Status:** Configuration required for production deployment  
**Priority:** CRITICAL
