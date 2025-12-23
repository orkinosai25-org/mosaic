# Appsettings Restoration Guide

**Date:** December 23, 2025  
**Priority:** CRITICAL - Required for Application Startup  
**Related Issue:** Restore All Appsettings to Earliest Working Version

---

## Overview

This document provides a comprehensive guide for restoring all application settings to their last known working configuration. The app was broken after recent Copilot agent changes, and this guide ensures all critical settings are properly restored.

## What Was Restored

### 1. ✅ Code Repository (appsettings.json)

The following configuration was **added back** to `src/OrkinosaiCMS.Web/appsettings.json`:

#### Azure Blob Storage Configuration
```json
"AzureBlobStorage": {
  "AccountName": "mosaicsaas",
  "PrimaryEndpoint": "https://mosaicsaas.blob.core.windows.net/",
  "Location": "uksouth",
  "SKU": "Standard_RAGRS",
  "ResourceId": "/subscriptions/0142b600-b263-48d1-83fe-3ead960e1781/resourceGroups/orkinosai_group/providers/Microsoft.Storage/storageAccounts/mosaicsaas",
  "Endpoints": {
    "Blob": "https://mosaicsaas.blob.core.windows.net/",
    "File": "https://mosaicsaas.file.core.windows.net/",
    "Queue": "https://mosaicsaas.queue.core.windows.net/",
    "Table": "https://mosaicsaas.table.core.windows.net/",
    "Dfs": "https://mosaicsaas.dfs.core.windows.net/"
  },
  "Security": {
    "PublicAccess": false,
    "EncryptionEnabled": true,
    "FileSharesEnabled": true,
    "MinimumTlsVersion": "TLS1_2",
    "AllowBlobPublicAccess": false,
    "SupportsHttpsTrafficOnly": true
  },
  "Containers": {
    "MediaAssets": "media-assets",
    "UserUploads": "user-uploads",
    "Documents": "documents",
    "Backups": "backups",
    "Images": "images"
  },
  "ConnectionStringKey": "AzureBlobStorageConnectionString",
  "MaxFileSizeBytes": 10485760
}
```

**Why restored:** This configuration was present in the MosaicCMS project but missing from OrkinosaiCMS.Web. It's required for:
- Media file uploads
- Document storage
- User assets
- Multi-tenant file isolation
- Backup functionality

#### Payment Configuration (Already Present - No Changes)
The Payment/Stripe configuration is **already present** in appsettings.json:
```json
"Payment": {
  "Stripe": {
    "PublishableKey": "",
    "SecretKey": "",
    "WebhookSecret": "",
    "ApiVersion": "2024-11-20.acacia",
    "Currency": "usd",
    "EnableTestMode": true,
    "PriceIds": {
      "Starter_Monthly": "",
      "Starter_Yearly": "",
      "Pro_Monthly": "",
      "Pro_Yearly": "",
      "Business_Monthly": "",
      "Business_Yearly": ""
    }
  }
}
```

**Status:** ✅ Already configured in appsettings.json. The keys are empty (placeholders) which is correct for security - actual keys must be set via Azure App Service Configuration.

---

## 2. ⚠️ Azure Portal Configuration (MANUAL RESTORATION REQUIRED)

The following settings **must be configured in Azure Portal** to restore full functionality. These settings were likely lost when connection strings were removed from the portal.

### Required Steps in Azure Portal

1. **Navigate to your Azure App Service:**
   - Go to [Azure Portal](https://portal.azure.com)
   - Select: **App Services** → **mosaic-saas**

2. **Go to Configuration:**
   - In the left menu, select: **Configuration**

### 2.1 Connection Strings (CRITICAL - App Won't Start Without This)

Click on **Connection strings** tab:

| Name | Value | Type |
|------|-------|------|
| `DefaultConnection` | `Server=tcp:orkinosai.database.windows.net,1433;Initial Catalog=mosaic-saas;Persist Security Info=False;User ID=sqladmin;Password=YOUR_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Max Pool Size=100;Min Pool Size=5;Pooling=true` | SQLServer |

**⚠️ IMPORTANT:**
- Replace `YOUR_PASSWORD` with your actual SQL Server password
- This is THE MOST CRITICAL setting - the app will not start without it
- See [AZURE_APP_SERVICE_CONFIGURATION_REQUIRED.md](./AZURE_APP_SERVICE_CONFIGURATION_REQUIRED.md) for detailed instructions

### 2.2 Application Settings (Required for Full Functionality)

Click on **Application settings** tab and add these settings:

#### Core Application Settings

| Name | Value | Description |
|------|-------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Sets the environment to production |
| `DefaultAdminPassword` | `YOUR_SECURE_PASSWORD` | Admin account password (change from default) |
| `Authentication__Jwt__SecretKey` | `YOUR_32_CHAR_SECRET_KEY` | JWT signing key (min 32 characters) |

#### Azure Blob Storage Settings

| Name | Value | Description |
|------|-------|-------------|
| `ConnectionStrings__AzureBlobStorageConnectionString` | `DefaultEndpointsProtocol=https;AccountName=mosaicsaas;AccountKey=YOUR_KEY;EndpointSuffix=core.windows.net` | Azure Blob Storage connection |

**To get the storage account key:**
```bash
az storage account keys list --resource-group orkinosai_group --account-name mosaicsaas --query "[0].value" -o tsv
```

Or in Azure Portal:
- Navigate to Storage Account: **mosaicsaas**
- Click **Access keys** in left menu
- Copy **key1** value

#### Azure Diagnostics Settings (For Logging)

| Name | Value | Description |
|------|-------|-------------|
| `DIAGNOSTICS_AZUREBLOBCONTAINERSASURL` | `https://mosaicsaas.blob.core.windows.net/logs?<SAS_TOKEN>` | Azure diagnostics blob SAS URL |

**To generate SAS URL:**
1. Go to Storage Account: **mosaicsaas**
2. Click **Shared access signature** in left menu
3. Configure:
   - Allowed services: **Blob**
   - Allowed resource types: **Container, Object**
   - Allowed permissions: **Read, Write, List, Add, Create**
   - Set expiry date: **1 year from now**
4. Click **Generate SAS and connection string**
5. Copy the **Blob service SAS URL**
6. Create a container named `logs` if it doesn't exist
7. Use the SAS URL for the logs container

#### Payment Settings (Optional - If Using Stripe)

| Name | Value | Description |
|------|-------|-------------|
| `Payment__Stripe__SecretKey` | `sk_live_YOUR_KEY` or `sk_test_YOUR_KEY` | Stripe secret key |
| `Payment__Stripe__PublishableKey` | `pk_live_YOUR_KEY` or `pk_test_YOUR_KEY` | Stripe publishable key |
| `Payment__Stripe__WebhookSecret` | `whsec_YOUR_SECRET` | Stripe webhook secret |

**Note:** Use test keys (`sk_test_`, `pk_test_`) for development/staging. Use live keys (`sk_live_`, `pk_live_`) only in production.

**Stripe API Version:** The configured version `2024-11-20.acacia` uses Stripe's new versioning format where "acacia" is the release family name. This is valid and part of Stripe's biannual release process.

---

## 3. Verification Steps

After restoring all settings, verify the application is working:

### 3.1 Verify Configuration in Azure Portal

1. **Check Connection Strings:**
   - Azure Portal → App Service → Configuration → Connection strings
   - Verify `DefaultConnection` is present
   - Value should show as "Hidden value. Click to show value"

2. **Check Application Settings:**
   - Azure Portal → App Service → Configuration → Application settings
   - Verify all required settings are present
   - Sensitive values should show as "Hidden value"

3. **Restart the App:**
   - Azure Portal → App Service → Overview
   - Click **Restart** button
   - Wait for restart to complete (~30 seconds)

### 3.2 Test Application Health

1. **Check Health Endpoint:**
   ```bash
   curl https://mosaic-saas.azurewebsites.net/api/health
   ```
   
   Expected response:
   ```json
   {
     "status": "Healthy",
     "timestamp": "2025-12-23T..."
   }
   ```

2. **Check Application Logs:**
   - Azure Portal → App Service → Log stream
   - Look for successful startup messages:
     ```
     Starting OrkinosaiCMS application
     Environment: Production
     Configuring database with provider: SqlServer
     Database initialization completed successfully
     Application started. Press Ctrl+C to shut down.
     ```

3. **Test Admin Login:**
   - Navigate to: `https://mosaic-saas.azurewebsites.net/admin/login`
   - Try logging in with admin credentials
   - Login should succeed

4. **Test Blob Storage (if configured):**
   - Try uploading a file through the application
   - Verify file appears in Azure Storage Explorer

---

## 4. Common Issues and Solutions

### Issue: "Connection string 'DefaultConnection' not found"

**Cause:** Connection string not configured in Azure Portal  
**Solution:** Follow Section 2.1 to add the connection string

### Issue: "Cannot generate SAS URI for blob storage"

**Cause:** Missing `ConnectionStrings__AzureBlobStorageConnectionString`  
**Solution:** Follow Section 2.2 to add the blob storage connection string

### Issue: HTTP 500.30 - App failed to start

**Cause:** Missing or invalid connection string  
**Solution:**
1. Check Azure Portal → Log Stream for detailed error
2. Verify connection string is correctly formatted
3. Test SQL connection with:
   ```bash
   az sql server show --name orkinosai --resource-group orkinosai_group
   ```
4. See [TROUBLESHOOTING_HTTP_500_30.md](./TROUBLESHOOTING_HTTP_500_30.md)

### Issue: Stripe payment not working

**Cause:** Missing Stripe configuration  
**Solution:** Add Stripe keys as shown in Section 2.2 (Payment Settings)

### Issue: File uploads failing

**Cause:** Missing Azure Blob Storage configuration  
**Solution:**
1. Verify `AzureBlobStorage` section exists in appsettings.json ✅ (restored)
2. Add `ConnectionStrings__AzureBlobStorageConnectionString` in Azure Portal
3. Verify storage account is accessible

---

## 5. Configuration History

### What Changed and Why

**December 22, 2025:**
- Production database credentials removed from source control (security fix)
- Connection strings must now be configured in Azure Portal
- Documentation created: [AZURE_APP_SERVICE_CONFIGURATION_REQUIRED.md](./AZURE_APP_SERVICE_CONFIGURATION_REQUIRED.md)

**December 23, 2025 (This Restoration):**
- Added `AzureBlobStorage` configuration to `appsettings.json`
- Documented all required Azure Portal settings
- Created this comprehensive restoration guide

### Why Settings Are Split

**In Source Control (appsettings.json):**
- ✅ Non-sensitive configuration (endpoint URLs, feature flags)
- ✅ Development/test default values
- ✅ Structure and schema definitions
- ❌ NO production credentials or secrets

**In Azure Portal (App Service Configuration):**
- ✅ Production database passwords
- ✅ API keys and secrets
- ✅ Connection strings with credentials
- ✅ Environment-specific overrides

This separation follows security best practices and prevents credential leaks.

---

## 6. Reference Documentation

- [AZURE_APP_SERVICE_CONFIGURATION_REQUIRED.md](./AZURE_APP_SERVICE_CONFIGURATION_REQUIRED.md) - Azure Portal setup
- [AZURE_DEPLOYMENT_CONFIGURATION.md](./AZURE_DEPLOYMENT_CONFIGURATION.md) - Deployment guide
- [AZURE_BLOB_STORAGE.md](./docs/AZURE_BLOB_STORAGE.md) - Blob storage integration
- [TROUBLESHOOTING_HTTP_500_30.md](./TROUBLESHOOTING_HTTP_500_30.md) - Startup error troubleshooting
- [COMPREHENSIVE_AUDIT_REPORT_UDI5TR.md](./COMPREHENSIVE_AUDIT_REPORT_UDI5TR.md) - Previous audit findings

---

## 7. Summary of Changes

### Files Modified in This PR
1. **`src/OrkinosaiCMS.Web/appsettings.json`**
   - ✅ Added `AzureBlobStorage` configuration section
   - Source: Copied from `src/MosaicCMS/appsettings.json`
   - Reason: Required for media uploads and file storage

2. **`APPSETTINGS_RESTORATION_GUIDE.md`** (this file)
   - ✅ Created comprehensive restoration guide
   - Documents all required Azure Portal settings
   - Includes verification steps and troubleshooting

### Settings Already Present (No Changes Needed)
- ✅ Payment/Stripe configuration (empty keys - correct)
- ✅ Authentication/JWT configuration
- ✅ Database configuration structure
- ✅ Logging/Serilog configuration
- ✅ All other application settings

### Azure Portal Configuration (Manual Action Required)
- ⚠️ Connection string: `DefaultConnection` - **MUST BE SET**
- ⚠️ Blob storage connection string - **RECOMMENDED**
- ⚠️ Diagnostics blob SAS URL - **RECOMMENDED FOR LOGGING**
- ⚠️ Stripe keys - **REQUIRED IF USING PAYMENTS**

---

## 8. Next Steps

1. **Immediate Action (Required):**
   - [ ] Add `DefaultConnection` to Azure Portal (Section 2.1)
   - [ ] Restart the Azure App Service
   - [ ] Verify app starts successfully (Section 3.2)

2. **Follow-up Actions (Recommended):**
   - [ ] Add Azure Blob Storage connection string (Section 2.2)
   - [ ] Configure diagnostics blob SAS URL (Section 2.2)
   - [ ] Add Stripe keys if using payments (Section 2.2)
   - [ ] Test all functionality (uploads, payments, etc.)

3. **Documentation:**
   - [ ] Save actual connection string values in a secure location (password manager, Azure Key Vault)
   - [ ] Document any custom settings specific to your deployment
   - [ ] Update team on restored configuration

---

**Last Updated:** December 23, 2025  
**Status:** ✅ Configuration Restored in Code - Azure Portal Configuration Required  
**Next Action:** Configure Azure Portal settings (Section 2)
