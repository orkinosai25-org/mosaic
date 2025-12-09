# MOSAIC CMS - Quick Start Guide

Get started with the MOSAIC CMS and Azure Blob Storage integration in minutes.

## Prerequisites

- .NET 9.0 SDK installed
- Azure Storage Account access (connection string or managed identity)
- Code editor (VS Code, Visual Studio, or Rider)

## Quick Setup

### 1. Clone and Navigate

```bash
git clone https://github.com/orkinosai25-org/mosaic.git
cd mosaic/src/MosaicCMS
```

### 2. Configure Connection String

**Option A: Using User Secrets (Development)**
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:AzureBlobStorageConnectionString" "YOUR_CONNECTION_STRING"
```

**Option B: Using appsettings.Development.json**
```json
{
  "ConnectionStrings": {
    "AzureBlobStorageConnectionString": "DefaultEndpointsProtocol=https;AccountName=mosaicsaas;AccountKey=YOUR_KEY;EndpointSuffix=core.windows.net"
  }
}
```

⚠️ **Important:** Never commit connection strings to Git!

### 3. Run the Application

```bash
dotnet run
```

The API will be available at: `https://localhost:5001`

## Test the API

### Upload an Image

```bash
# Create a test image or use an existing one
curl -X POST https://localhost:5001/api/media/images \
  -H "X-Tenant-Id: test-tenant-001" \
  -F "file=@test-image.jpg" \
  --insecure
```

**Response:**
```json
{
  "fileName": "test-image.jpg",
  "uri": "https://mosaicsaas.blob.core.windows.net/images/test-tenant-001/test-image.jpg",
  "tenantId": "test-tenant-001",
  "size": 245632,
  "contentType": "image/jpeg",
  "uploadedAt": "2024-12-09T22:30:00Z"
}
```

### List Files

```bash
curl -X GET "https://localhost:5001/api/media/list?containerType=images" \
  -H "X-Tenant-Id: test-tenant-001" \
  --insecure
```

### Get Temporary Access URL

```bash
curl -X GET "https://localhost:5001/api/media/sas-url?containerType=images&fileName=test-image.jpg&expiryMinutes=60" \
  -H "X-Tenant-Id: test-tenant-001" \
  --insecure
```

## API Documentation

Once running, access the OpenAPI documentation:

- **Swagger UI**: `https://localhost:5001/swagger` (if configured)
- **OpenAPI JSON**: `https://localhost:5001/openapi/v1.json`

## Supported File Types

### Images
✅ JPEG/JPG  
✅ PNG  
✅ GIF  
✅ WebP  
✅ SVG  

### Documents
✅ PDF  
✅ Microsoft Word (.doc, .docx)  
✅ Microsoft Excel (.xls, .xlsx)  
✅ Text (.txt)  
✅ CSV  

## File Size Limits

- **Images**: 10 MB
- **Documents**: 10 MB
- **SVG**: 1 MB
- **Text/CSV**: 5 MB

## Tenant Isolation

All files are automatically isolated by tenant:

```
Container: images/
├── tenant-001/
│   ├── logo.png
│   └── banner.jpg
└── tenant-002/
    ├── profile.jpg
    └── header.png
```

## Security Features

✅ **File Signature Validation**: Prevents malicious files with fake extensions  
✅ **MIME Type Validation**: Ensures declared type matches actual type  
✅ **Path Sanitization**: Prevents directory traversal attacks  
✅ **Tenant Isolation**: Automatic data separation  
✅ **HTTPS Only**: All traffic encrypted  
✅ **Size Limits**: Prevents resource exhaustion  

## Common Issues

### "Connection string not found"
**Solution:** Configure connection string in user secrets or appsettings.Development.json

### "UnauthorizedAccessException"
**Solution:** Verify your Azure Storage connection string has correct access key

### "File signature does not match"
**Solution:** Ensure the file is not corrupted and matches its declared type

### "Container not found"
**Solution:** Containers are auto-created on first upload. Check permissions.

## Next Steps

1. ✅ Test file upload and download
2. ✅ Explore different container types
3. ✅ Implement authentication for tenant IDs
4. ✅ Configure production deployment
5. ✅ Set up monitoring and alerts

## Production Deployment

For production:

1. **Use Managed Identity** instead of connection strings
2. **Enable Application Insights** for monitoring
3. **Configure CORS** for web clients
4. **Set up rate limiting** to prevent abuse
5. **Enable authentication** to protect endpoints

See [Azure Blob Storage Integration Guide](./AZURE_BLOB_STORAGE.md) for detailed production setup.

## Learn More

- 📖 [Full Integration Guide](./AZURE_BLOB_STORAGE.md)
- 📖 [CMS README](../src/MosaicCMS/README.md)
- 📖 [SaaS Features](./SaaS_FEATURES.md)
- 🔗 [Azure Storage Documentation](https://docs.microsoft.com/azure/storage/)

## Support

- 📧 Email: support@mosaic.orkinosai.com
- 🐛 Issues: [GitHub Issues](https://github.com/orkinosai25-org/mosaic/issues)
- 💬 Discussions: [GitHub Discussions](https://github.com/orkinosai25-org/mosaic/discussions)

---

**Built with ❤️ by [Orkinosai](https://github.com/orkinosai25-org)**
