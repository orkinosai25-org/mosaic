# MOSAIC CMS - Azure Blob Storage Integration

This is the MOSAIC Content Management System (CMS) with integrated Azure Blob Storage support for multi-tenant media and asset management.

## Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- Azure Storage Account (configured)
- Connection string or Managed Identity credentials

### Configuration

1. **Add Connection String** (Development):
   ```bash
   cd src/MosaicCMS
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:AzureBlobStorageConnectionString" "YOUR_CONNECTION_STRING"
   ```

2. **Update appsettings.json** (if needed):
   - Storage account details are pre-configured in `appsettings.json`
   - Override settings in `appsettings.Development.json` for local development

### Running the Application

```bash
cd src/MosaicCMS
dotnet run
```

The API will be available at `https://localhost:5001` (or the configured port).

## API Endpoints

### Media Operations

#### Upload Image
```http
POST /api/media/images
Headers:
  X-Tenant-Id: your-tenant-id
  Content-Type: multipart/form-data
Body:
  file: [binary file data]
```

#### Upload Document
```http
POST /api/media/documents
Headers:
  X-Tenant-Id: your-tenant-id
  Content-Type: multipart/form-data
Body:
  file: [binary file data]
```

#### List Files
```http
GET /api/media/list?containerType=images
Headers:
  X-Tenant-Id: your-tenant-id
```

#### Delete File
```http
DELETE /api/media?containerType=images&fileName=logo.png
Headers:
  X-Tenant-Id: your-tenant-id
```

#### Get SAS URL
```http
GET /api/media/sas-url?containerType=images&fileName=logo.png&expiryMinutes=60
Headers:
  X-Tenant-Id: your-tenant-id
```

### Backup Operations

#### Create Backup
```http
POST /api/backup
Headers:
  X-Tenant-Id: your-tenant-id
  Content-Type: application/json
Body:
  {
    "containers": ["images", "documents", "user-uploads"]
  }
```

#### List Backups
```http
GET /api/backup
Headers:
  X-Tenant-Id: your-tenant-id
```

#### Restore Backup
```http
POST /api/backup/restore/{backupId}
Headers:
  X-Tenant-Id: your-tenant-id
```

#### Delete Backup
```http
DELETE /api/backup/{backupId}
Headers:
  X-Tenant-Id: your-tenant-id
```

### Health Monitoring

#### Basic Health Check
```http
GET /api/health
```

#### Detailed Health Check (includes storage connectivity)
```http
GET /api/health/detailed
```

## Project Structure

```
MosaicCMS/
├── Controllers/
│   ├── MediaController.cs          # API endpoints for media operations
│   ├── BackupController.cs         # API endpoints for backup/restore
│   └── HealthController.cs         # Health check endpoints
├── Models/
│   └── AzureBlobStorageOptions.cs  # Configuration models
├── Services/
│   └── Storage/
│       ├── IBlobStorageService.cs      # Blob storage interface
│       ├── BlobStorageService.cs       # Blob storage implementation
│       ├── IBackupService.cs           # Backup service interface
│       ├── BackupService.cs            # Backup service implementation
│       ├── IFileStorageService.cs      # Abstract file storage interface
│       └── FileValidationService.cs    # File validation utilities
├── appsettings.json                # Application configuration
└── Program.cs                      # Application startup
```

## Security Features

- ✅ Tenant isolation (files prefixed with tenant ID)
- ✅ Input validation and sanitization
- ✅ File signature validation (magic number detection)
- ✅ File type restrictions with MIME type verification
- ✅ File size limits (10 MB default, configurable)
- ✅ Path traversal protection with allowlist approach
- ✅ HTTPS enforcement
- ✅ SAS token generation for temporary access
- ✅ No public blob access allowed
- ✅ Encryption at rest and in transit (TLS 1.2+)

## Development

### Adding New Container Types

1. Update `appsettings.json`:
   ```json
   "Containers": {
     "NewContainer": "new-container"
   }
   ```

2. Update allowed containers in `MediaController.cs`:
   ```csharp
   var allowedContainers = new[] { "images", "documents", "new-container" };
   ```

### Running Tests

```bash
dotnet test
```

## Deployment

### Azure App Service

1. Publish the application:
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. Deploy to Azure:
   ```bash
   az webapp deployment source config-zip \
     --resource-group orkinosai_group \
     --name your-app-name \
     --src publish.zip
   ```

3. Configure Managed Identity (Recommended):
   - Enable system-assigned managed identity on App Service
   - Grant "Storage Blob Data Contributor" role to the identity
   - Remove connection string from configuration

## Troubleshooting

### "Connection string not found" warning
- Add connection string to user secrets or configure Managed Identity

### Cannot upload files
- Verify container names match configuration
- Check file type and size restrictions
- Ensure valid tenant ID is provided

### SAS generation fails
- Verify connection string includes account key (not just endpoint)
- Or implement custom SAS generation logic for Managed Identity

## Learn More

- [Azure Blob Storage Integration Guide](../../docs/AZURE_BLOB_STORAGE.md)
- [MOSAIC Documentation](../../docs/)
- [Azure Storage Documentation](https://docs.microsoft.com/azure/storage/)

## Support

For issues or questions:
- 📧 Email: support@mosaic.orkinosai.com
- 🐛 Issues: [GitHub Issues](https://github.com/orkinosai25-org/mosaic/issues)
