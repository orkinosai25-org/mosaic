# OrkinosaiCMS.Web Configuration Notes

## Important: This Application Does NOT Use Azure Blob Storage

**OrkinosaiCMS.Web** (the main CMS application deployed to production) does NOT require or use Azure Blob Storage configuration.

### Architecture Clarification

This repository contains **two separate projects**:

1. **OrkinosaiCMS.Web** (`src/OrkinosaiCMS.Web/`)
   - Main CMS application
   - Deployed to production
   - Uses SQL Server for all data storage
   - **Does NOT use blob storage**
   - Configuration files:
     - `appsettings.json` (development)
     - `appsettings.Production.json` (production)

2. **MosaicCMS** (`src/MosaicCMS/`)
   - Separate API service
   - **DOES have blob storage integration**
   - Configuration includes `AzureBlobStorage` section
   - Not the primary deployed application

### Required Configuration for OrkinosaiCMS.Web

The **ONLY** required configuration for OrkinosaiCMS.Web to start successfully:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;User ID=...;Password=..."
  }
}
```

### What's NOT Needed

❌ **Azure Blob Storage configuration** - Not used by this application  
❌ `AzureBlobStorage` section in appsettings - Not required  
❌ Blob connection strings - Not needed  

### HTTP 500.30 Troubleshooting

If you see HTTP 500.30 errors, they are typically caused by:

1. **Database connection issues**
   - Invalid connection string
   - Azure SQL database unavailable (transient error)
   - Firewall blocking connections
   - Wrong credentials

2. **Missing/invalid JWT secret** (if using API authentication)

3. **Azure infrastructure issues**
   - App Service restart
   - Resource constraints
   - Network connectivity

**NOT caused by missing blob storage configuration** - because this app doesn't use it.

### Configuration File Protection

⚠️ **DO NOT REMOVE** the following from `appsettings.Production.json`:
- `ConnectionStrings.DefaultConnection` ✅ REQUIRED
- `DatabaseEnabled` and `DatabaseProvider` ✅ REQUIRED
- `Database.AutoApplyMigrations` ✅ REQUIRED

✅ **SAFE TO OMIT** (not used by OrkinosaiCMS.Web):
- `AzureBlobStorage` section - Not needed
- Blob-related connection strings - Not needed

### How to Verify Configuration

Check that `appsettings.Production.json` contains:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:orkinosai.database.windows.net,1433;..."
  },
  "DatabaseEnabled": true,
  "DatabaseProvider": "SqlServer",
  "Database": {
    "AutoApplyMigrations": true
  }
}
```

That's all that's needed for the app to start successfully.

### References

- Troubleshooting: See `TROUBLESHOOTING_HTTP_500_30.md`
- Blob storage info (for MosaicCMS API only): See `docs/AZURE_BLOB_STORAGE.md`
- Deployment guide: See `docs/DEPLOYMENT_ARCHITECTURE.md`

---

**Last Updated**: December 2024  
**Applies to**: OrkinosaiCMS.Web v1.0+
