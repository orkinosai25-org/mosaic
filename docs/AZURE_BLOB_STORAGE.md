# Azure Blob Storage Integration

This document describes the Azure Blob Storage integration for the MOSAIC CMS platform, including configuration, usage, security considerations, and migration tips.

## Overview

MOSAIC CMS leverages **Azure Blob Storage** for scalable, secure, and multi-tenant media and asset management. All user-uploaded content (images, documents, backups) is stored in Azure Blob Storage with automatic tenant isolation.

## Quick Start for CMS Users and Admins

### For CMS Users

As a CMS user, Azure Blob Storage integration is **transparent** - all file uploads automatically use blob storage with no additional configuration needed. Simply:

1. **Upload files** through the CMS interface or API
2. **Manage files** using the standard file management tools
3. **Access files** via generated URLs or the CMS interface

All files are automatically:
- ✅ Stored securely in Azure Blob Storage
- ✅ Isolated to your tenant (no cross-tenant access)
- ✅ Backed up according to retention policies
- ✅ Encrypted at rest and in transit

### For CMS Administrators

#### Initial Setup (One-Time)

**Step 1: Obtain Azure Storage Credentials**

You need either:
- **Connection String** (for development/testing)
- **Managed Identity** (recommended for production)

To get your connection string:
```bash
# From Azure Portal
1. Navigate to your Storage Account
2. Go to "Access keys"
3. Copy the connection string from key1 or key2
```

**Step 2: Configure the CMS**

For **development** (using user secrets):
```bash
cd src/MosaicCMS
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:AzureBlobStorageConnectionString" "YOUR_CONNECTION_STRING"
```

For **production** (using Azure Key Vault):
```bash
# Add connection string to Key Vault
az keyvault secret set \
  --vault-name your-keyvault \
  --name AzureBlobStorageConnectionString \
  --value "YOUR_CONNECTION_STRING"

# Configure CMS to use Key Vault (in appsettings.Production.json)
{
  "KeyVault": {
    "VaultUri": "https://your-keyvault.vault.azure.net/"
  }
}
```

For **production with Managed Identity** (most secure):
```bash
# 1. Enable managed identity on your App Service/VM
az webapp identity assign --name your-app --resource-group your-rg

# 2. Grant "Storage Blob Data Contributor" role
az role assignment create \
  --role "Storage Blob Data Contributor" \
  --assignee-object-id <managed-identity-object-id> \
  --scope /subscriptions/<sub-id>/resourceGroups/<rg>/providers/Microsoft.Storage/storageAccounts/mosaicsaas

# 3. Update BlobStorageService to use DefaultAzureCredential instead of connection string
```

**Step 3: Verify Configuration**

Test the setup:
```bash
# Start the CMS
dotnet run

# Check health endpoint
curl http://localhost:5000/api/health/detailed

# Should return: "status": "healthy"
```

**Step 4: Test File Upload**

```bash
# Upload a test image
curl -X POST http://localhost:5000/api/media/images \
  -H "X-Tenant-Id: test-tenant-001" \
  -F "file=@test-image.jpg"

# Verify in Azure Storage Explorer or Portal
# Files appear as: images/test-tenant-001/test-image.jpg
```

#### Ongoing Management

**Monitor Storage Usage**
```bash
# Check detailed health (includes storage status)
curl http://localhost:5000/api/health/detailed

# Monitor in Azure Portal
# Navigate to: Storage Account > Metrics > Blob Capacity
```

**Create Tenant Backups**
```bash
# Backup all containers for a tenant
curl -X POST http://localhost:5000/api/backup \
  -H "X-Tenant-Id: tenant-001" \
  -H "Content-Type: application/json" \
  -d '{"containers": ["images", "documents", "user-uploads"]}'

# List backups
curl http://localhost:5000/api/backup \
  -H "X-Tenant-Id: tenant-001"
```

**Restore from Backup**
```bash
# Restore specific backup
curl -X POST http://localhost:5000/api/backup/restore/backup-20241210-123456-abc123 \
  -H "X-Tenant-Id: tenant-001"
```

**Manage Storage Costs**
- Monitor storage usage in Azure Portal
- Set up lifecycle management to move old files to cool/archive tiers
- Configure retention policies to auto-delete old backups
- Use Azure Cost Management for billing alerts

#### Troubleshooting for Admins

**Connection Issues**
```bash
# Test 1: Verify connection string format
# Should be: DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net

# Test 2: Check network connectivity
curl https://mosaicsaas.blob.core.windows.net

# Test 3: Verify RBAC permissions (for Managed Identity)
az role assignment list --assignee <identity-object-id> --scope <storage-account-resource-id>
```

**Upload Failures**
- Check file size limits (default 10 MB)
- Verify file type is allowed
- Check tenant ID is valid
- Review application logs for detailed errors

**Performance Issues**
- Enable Azure CDN for frequently accessed files
- Use SAS tokens with appropriate expiry times
- Monitor blob storage metrics in Azure Portal
- Consider enabling blob versioning for critical files

## Azure Storage Account Details

The MOSAIC platform uses the following Azure Storage account:

| Property | Value |
|----------|-------|
| **Account Name** | `mosaicsaas` |
| **Primary Endpoint** | `https://mosaicsaas.blob.core.windows.net/` |
| **Location** | UK South (`uksouth`) |
| **SKU** | Standard_RAGRS (Geo-redundant storage with read access) |
| **Resource ID** | `/subscriptions/0142b600-b263-48d1-83fe-3ead960e1781/resourceGroups/orkinosai_group/providers/Microsoft.Storage/storageAccounts/mosaicsaas` |

### Available Endpoints

- **Blob Service**: `https://mosaicsaas.blob.core.windows.net/`
- **File Service**: `https://mosaicsaas.file.core.windows.net/`
- **Queue Service**: `https://mosaicsaas.queue.core.windows.net/`
- **Table Service**: `https://mosaicsaas.table.core.windows.net/`
- **Data Lake Storage (DFS)**: `https://mosaicsaas.dfs.core.windows.net/`

## Security Configuration

### Current Security Settings

| Security Feature | Status | Description |
|------------------|--------|-------------|
| **Public Access** | Disabled | No anonymous access to blobs |
| **Encryption** | Enabled | Data encrypted at rest using Microsoft-managed keys |
| **File Shares** | Enabled | Azure Files service available |
| **Minimum TLS Version** | TLS 1.2 | Modern encryption for data in transit |
| **HTTPS Only** | Enforced | All connections must use HTTPS |

### Authentication Methods

The platform supports multiple authentication methods:

1. **Connection String** (Development/Testing)
   - Store in Azure Key Vault or secure configuration
   - Never commit connection strings to source control

2. **Managed Identity** (Production - Recommended)
   - No credentials stored in code
   - Automatic credential rotation
   - Azure handles authentication

3. **Shared Access Signatures (SAS)**
   - Time-limited access tokens
   - Used for temporary file access
   - Automatically generated by the service

## Configuration

### appsettings.json

The Azure Blob Storage configuration is defined in `src/MosaicCMS/appsettings.json`:

```json
{
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
    "ConnectionStringKey": "AzureBlobStorageConnectionString"
  }
}
```

### Connection String Configuration

For development and testing, add the connection string to your secrets:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:AzureBlobStorageConnectionString" "YOUR_CONNECTION_STRING"
```

For production, use Azure Key Vault:

```json
{
  "KeyVault": {
    "VaultUri": "https://your-keyvault.vault.azure.net/"
  }
}
```

## Blob Container Structure

### Container Types

The platform uses dedicated containers for different types of content:

| Container | Purpose | Access Level |
|-----------|---------|--------------|
| `images` | User-uploaded images | Private |
| `documents` | User-uploaded documents | Private |
| `user-uploads` | General user uploads | Private |
| `media-assets` | Platform media assets | Private |
| `backups` | Tenant backup files | Private |

### Tenant Isolation

All files are stored with tenant isolation using path prefixing:

```
Container Structure:
├── images/
│   ├── tenant-abc123/
│   │   ├── logo.png
│   │   └── banner.jpg
│   └── tenant-xyz789/
│       ├── profile.jpg
│       └── gallery-01.png
├── documents/
│   ├── tenant-abc123/
│   │   ├── invoice.pdf
│   │   └── contract.docx
│   └── tenant-xyz789/
│       └── report.xlsx
```

**Benefits:**
- ✅ Automatic tenant data isolation
- ✅ Easy tenant-specific listing and management
- ✅ Simple backup and restore per tenant
- ✅ Prevents accidental cross-tenant access

## Usage Examples

### 1. Upload an Image

**Endpoint:** `POST /api/media/images`

**Headers:**
```
X-Tenant-Id: tenant-abc123
Content-Type: multipart/form-data
```

**Body:**
```
file: [binary image data]
```

**cURL Example:**
```bash
curl -X POST https://your-api.com/api/media/images \
  -H "X-Tenant-Id: tenant-abc123" \
  -F "file=@/path/to/image.jpg"
```

**Response:**
```json
{
  "fileName": "image.jpg",
  "uri": "https://mosaicsaas.blob.core.windows.net/images/tenant-abc123/image.jpg",
  "tenantId": "tenant-abc123",
  "size": 245632,
  "contentType": "image/jpeg",
  "uploadedAt": "2024-12-09T10:30:00Z"
}
```

### 2. Upload a Document

**Endpoint:** `POST /api/media/documents`

**Headers:**
```
X-Tenant-Id: tenant-abc123
Content-Type: multipart/form-data
```

**cURL Example:**
```bash
curl -X POST https://your-api.com/api/media/documents \
  -H "X-Tenant-Id: tenant-abc123" \
  -F "file=@/path/to/document.pdf"
```

### 3. List Tenant Files

**Endpoint:** `GET /api/media/list?containerType=images`

**Headers:**
```
X-Tenant-Id: tenant-abc123
```

**cURL Example:**
```bash
curl -X GET "https://your-api.com/api/media/list?containerType=images" \
  -H "X-Tenant-Id: tenant-abc123"
```

**Response:**
```json
{
  "tenantId": "tenant-abc123",
  "containerType": "images",
  "count": 3,
  "files": [
    "logo.png",
    "banner.jpg",
    "profile.jpg"
  ]
}
```

### 4. Delete a File

**Endpoint:** `DELETE /api/media?containerType=images&fileName=logo.png`

**Headers:**
```
X-Tenant-Id: tenant-abc123
```

**cURL Example:**
```bash
curl -X DELETE "https://your-api.com/api/media?containerType=images&fileName=logo.png" \
  -H "X-Tenant-Id: tenant-abc123"
```

### 5. Get Temporary Access URL (SAS)

**Endpoint:** `GET /api/media/sas-url?containerType=images&fileName=logo.png&expiryMinutes=60`

**Headers:**
```
X-Tenant-Id: tenant-abc123
```

**cURL Example:**
```bash
curl -X GET "https://your-api.com/api/media/sas-url?containerType=images&fileName=logo.png&expiryMinutes=60" \
  -H "X-Tenant-Id: tenant-abc123"
```

**Response:**
```json
{
  "sasUrl": "https://mosaicsaas.blob.core.windows.net/images/tenant-abc123/logo.png?sv=2021-06-08&se=2024-12-09T11%3A30%3A00Z&sr=b&sp=r&sig=SIGNATURE",
  "expiresAt": "2024-12-09T11:30:00Z",
  "tenantId": "tenant-abc123",
  "fileName": "logo.png"
}
```

## File Upload Restrictions

### Allowed File Types

**Images:**
- JPEG/JPG
- PNG
- GIF
- WebP
- SVG

**Documents:**
- PDF
- Microsoft Word (.doc, .docx)
- Microsoft Excel (.xls, .xlsx)
- Plain Text (.txt)
- CSV

### Size Limits

- **Maximum file size:** 10 MB per file
- **Recommended:** Compress images before uploading
- **Large files:** Contact support for increased limits

## Security Best Practices

### 1. Authentication & Authorization

```csharp
// Always validate tenant ID from authenticated user context
var tenantId = User.Claims.FirstOrDefault(c => c.Type == "TenantId")?.Value;
if (string.IsNullOrEmpty(tenantId))
{
    return Unauthorized("Tenant ID not found in user claims");
}
```

### 2. Input Validation

The service automatically:
- ✅ Validates MIME types against allowed types
- ✅ Validates file signatures (magic numbers) to prevent malicious files
- ✅ Checks file sizes against limits
- ✅ Sanitizes file names using allowlist approach
- ✅ Prevents path traversal attacks (removes `..`, `/`, `\`)
- ✅ Blocks directory traversal attempts with character filtering
- ✅ Rejects files with invalid or mismatched signatures

### 3. Connection String Security

**DO:**
- ✅ Store connection strings in Azure Key Vault
- ✅ Use Managed Identity in production
- ✅ Rotate keys regularly
- ✅ Use separate storage accounts for dev/prod

**DON'T:**
- ❌ Commit connection strings to Git
- ❌ Log connection strings
- ❌ Share connection strings via email/chat
- ❌ Use production credentials in development

### 4. SAS Token Management

- Use short expiry times (60 minutes or less)
- Generate tokens on-demand
- Revoke tokens when no longer needed
- Monitor SAS token usage

## Performance Optimization

### 1. CDN Integration

For public assets, integrate Azure CDN:

```json
{
  "AzureCDN": {
    "Enabled": true,
    "Endpoint": "https://mosaicsaas.azureedge.net/",
    "CachingRules": {
      "Images": "7 days",
      "Documents": "1 day"
    }
  }
}
```

### 2. Caching Strategy

- Cache blob metadata locally
- Use ETags for conditional requests
- Implement client-side caching headers

### 3. Batch Operations

When uploading multiple files:
```csharp
// Use parallel uploads for better performance
var uploadTasks = files.Select(file => 
    _blobStorageService.UploadFileAsync(container, tenantId, file));
await Task.WhenAll(uploadTasks);
```

## Migration Guide

### Migrating from Local File Storage

**Step 1: Audit Existing Files**
```bash
# List all files to migrate
find /var/www/uploads -type f > files-to-migrate.txt
```

**Step 2: Create Migration Script**
```csharp
public async Task MigrateToAzureBlob(string localPath, string tenantId)
{
    var files = Directory.GetFiles(localPath, "*", SearchOption.AllDirectories);
    
    foreach (var filePath in files)
    {
        var fileName = Path.GetFileName(filePath);
        var contentType = GetContentType(fileName);
        
        using var stream = File.OpenRead(filePath);
        await _blobStorageService.UploadFileAsync(
            "user-uploads",
            tenantId,
            fileName,
            stream,
            contentType);
    }
}
```

**Step 3: Verify Migration**
```csharp
var uploadedFiles = await _blobStorageService.ListFilesAsync("user-uploads", tenantId);
Console.WriteLine($"Migrated {uploadedFiles.Count} files for tenant {tenantId}");
```

**Step 4: Update Application References**
- Update all file path references to use blob URIs
- Implement SAS token generation for secure access
- Test file access in the application

**Step 5: Clean Up**
- Verify all files are accessible in blob storage
- Archive local files as backup
- Remove local file storage after verification period

### Migrating from AWS S3

Use **Azure Data Factory** or **AzCopy** for bulk migration:

```bash
# Using AzCopy
azcopy copy \
  "https://your-bucket.s3.amazonaws.com/*" \
  "https://mosaicsaas.blob.core.windows.net/user-uploads" \
  --recursive
```

### Migrating from Other Cloud Storage

1. Export data from current provider
2. Transform to MOSAIC folder structure (tenant-id/filename)
3. Use AzCopy or Azure Storage Explorer for upload
4. Update application configuration
5. Test thoroughly before switching

## Monitoring & Diagnostics

### Metrics to Monitor

- **Storage Capacity:** Track blob storage usage per tenant
- **Bandwidth:** Monitor egress for cost optimization
- **Request Latency:** Track upload/download times
- **Error Rates:** Monitor failed operations

### Azure Monitor Integration

```json
{
  "ApplicationInsights": {
    "ConnectionString": "YOUR_APP_INSIGHTS_CONNECTION_STRING",
    "EnableBlobStorageTracking": true
  }
}
```

### Logging

The service automatically logs:
- ✅ Upload operations with tenant ID
- ✅ Download requests
- ✅ Deletion operations
- ✅ Errors and exceptions
- ✅ SAS token generation

Example log output:
```
[INFO] File uploaded successfully: tenant-abc123/logo.png for tenant tenant-abc123
[INFO] Generated SAS URI for tenant-abc123/logo.png for tenant tenant-abc123, expires in 60 minutes
[ERROR] Error uploading file banner.jpg for tenant tenant-abc123: UnauthorizedAccessException
```

## Cost Optimization

### Storage Tiers

Consider using different storage tiers for different content types:

| Tier | Use Case | Cost | Access Time |
|------|----------|------|-------------|
| **Hot** | Frequently accessed media | Higher storage, lower access | Immediate |
| **Cool** | Infrequently accessed files | Lower storage, higher access | Immediate |
| **Archive** | Long-term backups | Lowest storage, highest access | Hours |

### Best Practices

1. **Lifecycle Management:** Automatically move old files to cool/archive tiers
2. **Cleanup:** Delete unused files regularly
3. **Compression:** Compress images before upload
4. **CDN:** Use CDN for frequently accessed public content
5. **Monitor:** Track storage costs per tenant

## Backup & Disaster Recovery

### Geo-Redundancy

The storage account uses **Standard_RAGRS** which provides:
- 3 copies in primary region (UK South)
- 3 copies in secondary region (asynchronous replication)
- Read access to secondary region in case of regional outage

### Backup Strategy

1. **Soft Delete:** Enabled for 7 days (configure as needed)
2. **Versioning:** Enable blob versioning for critical containers
3. **Snapshots:** Take periodic snapshots of important data
4. **Cross-Region Backup:** Replicate critical data to separate storage account
5. **Automated Tenant Backups:** Use the built-in backup API for tenant-level backups

### Using the Backup API

The CMS provides a dedicated backup service for tenant data management:

**Create a Backup:**
```bash
curl -X POST https://your-cms.com/api/backup \
  -H "X-Tenant-Id: your-tenant-id" \
  -H "Content-Type: application/json" \
  -d '{
    "containers": ["images", "documents", "user-uploads"]
  }'
```

**Response:**
```json
{
  "backupId": "backup-20241210-123456-abc123",
  "tenantId": "your-tenant-id",
  "fileCount": 150,
  "totalSizeBytes": 52428800,
  "createdAt": "2024-12-10T12:34:56Z",
  "containers": ["images", "documents", "user-uploads"],
  "message": "Backup created successfully"
}
```

**List Backups:**
```bash
curl -X GET https://your-cms.com/api/backup \
  -H "X-Tenant-Id: your-tenant-id"
```

**Restore a Backup:**
```bash
curl -X POST https://your-cms.com/api/backup/restore/backup-20241210-123456-abc123 \
  -H "X-Tenant-Id: your-tenant-id"
```

**Delete a Backup:**
```bash
curl -X DELETE https://your-cms.com/api/backup/backup-20241210-123456-abc123 \
  -H "X-Tenant-Id: your-tenant-id"
```

### Recovery Procedures

**Recover Deleted Blob (within soft-delete period):**
```csharp
await blobClient.UndeleteAsync();
```

**Restore from Previous Version:**
```csharp
await blobClient.StartCopyFromUriAsync(previousVersionUri);
```

## Troubleshooting

### Common Issues

**1. "Connection string not found" warning**
- **Cause:** Missing connection string in configuration
- **Solution:** Add connection string to user secrets or Key Vault

**2. "Cannot generate SAS URI"**
- **Cause:** Using Managed Identity without account key
- **Solution:** Either use account key or implement custom SAS generation

**3. "UnauthorizedAccessException"**
- **Cause:** Invalid credentials or expired SAS token
- **Solution:** Verify credentials and regenerate SAS tokens

**4. "ContainerNotFoundException"**
- **Cause:** Container doesn't exist yet
- **Solution:** Containers are auto-created on first upload

**5. File upload fails**
- **Cause:** File size exceeds limit or invalid file type
- **Solution:** Check file size (max 10MB) and allowed file types

## Additional Resources

- [Azure Blob Storage Documentation](https://docs.microsoft.com/azure/storage/blobs/)
- [Azure Storage Security Guide](https://docs.microsoft.com/azure/storage/common/storage-security-guide)
- [Azure Storage Best Practices](https://docs.microsoft.com/azure/storage/blobs/storage-blobs-introduction)
- [MOSAIC CMS Documentation](./README.md)
- [SaaS Features Overview](./SaaS_FEATURES.md)

## Support

For issues or questions related to Azure Blob Storage integration:

- 📧 Email: support@mosaic.orkinosai.com
- 🐛 Issues: [GitHub Issues](https://github.com/orkinosai25-org/mosaic/issues)
- 📖 Docs: [MOSAIC Documentation](https://docs.mosaic.orkinosai.com)

---

**Last Updated:** December 2024  
**Version:** 1.0  
**Maintained by:** Orkinosai Team
