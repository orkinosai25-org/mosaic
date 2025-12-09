using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using MosaicCMS.Models;

namespace MosaicCMS.Services.Storage;

/// <summary>
/// Azure Blob Storage service implementation for multi-tenant media/asset management
/// Implements tenant isolation by prefixing blob paths with tenant IDs
/// </summary>
public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly AzureBlobStorageOptions _options;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(
        IConfiguration configuration,
        IOptions<AzureBlobStorageOptions> options,
        ILogger<BlobStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;

        // Get connection string from configuration
        var connectionString = configuration.GetConnectionString(_options.ConnectionStringKey);
        
        if (string.IsNullOrEmpty(connectionString))
        {
            // In production, this should use Managed Identity via DefaultAzureCredential
            // For now, throw an exception to ensure proper configuration
            var errorMessage = $"Azure Blob Storage connection string '{_options.ConnectionStringKey}' not found. " +
                             "Configure connection string in user secrets (development) or use Managed Identity (production).";
            _logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }
        
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    /// <inheritdoc/>
    public async Task<string> UploadFileAsync(
        string containerName,
        string tenantId,
        string fileName,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure container exists
            var containerClient = await GetOrCreateContainerAsync(containerName, cancellationToken);

            // Create tenant-isolated blob path
            var blobName = GetTenantBlobPath(tenantId, fileName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Set content type and upload options
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                },
                // Set metadata for tracking
                Metadata = new Dictionary<string, string>
                {
                    { "TenantId", tenantId },
                    { "UploadedAt", DateTime.UtcNow.ToString("O") }
                }
            };

            // Upload the file
            await blobClient.UploadAsync(fileStream, uploadOptions, cancellationToken);

            _logger.LogInformation("File uploaded successfully: {BlobName} for tenant {TenantId}", blobName, tenantId);

            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName} for tenant {TenantId}", fileName, tenantId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Stream> DownloadFileAsync(
        string containerName,
        string tenantId,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobName = GetTenantBlobPath(tenantId, fileName);
            var blobClient = containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync(cancellationToken))
            {
                throw new FileNotFoundException($"Blob {blobName} not found in container {containerName}");
            }

            var response = await blobClient.DownloadAsync(cancellationToken);
            _logger.LogInformation("File downloaded successfully: {BlobName} for tenant {TenantId}", blobName, tenantId);

            return response.Value.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file {FileName} for tenant {TenantId}", fileName, tenantId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteFileAsync(
        string containerName,
        string tenantId,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobName = GetTenantBlobPath(tenantId, fileName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var result = await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
            
            if (result.Value)
            {
                _logger.LogInformation("File deleted successfully: {BlobName} for tenant {TenantId}", blobName, tenantId);
            }
            else
            {
                _logger.LogWarning("File not found for deletion: {BlobName} for tenant {TenantId}", blobName, tenantId);
            }

            return result.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileName} for tenant {TenantId}", fileName, tenantId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<string>> ListFilesAsync(
        string containerName,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var tenantPrefix = $"{tenantId}/";
            var files = new List<string>();

            await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: tenantPrefix, cancellationToken: cancellationToken))
            {
                // Remove tenant prefix from blob name for cleaner file names
                var fileName = blobItem.Name.Substring(tenantPrefix.Length);
                files.Add(fileName);
            }

            _logger.LogInformation("Listed {Count} files for tenant {TenantId} in container {ContainerName}", 
                files.Count, tenantId, containerName);

            return files;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files for tenant {TenantId} in container {ContainerName}", 
                tenantId, containerName);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<string> GetSasUriAsync(
        string containerName,
        string tenantId,
        string fileName,
        int expiryMinutes = 60,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobName = GetTenantBlobPath(tenantId, fileName);
            var blobClient = containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync(cancellationToken))
            {
                throw new FileNotFoundException($"Blob {blobName} not found in container {containerName}");
            }

            // Check if we can generate SAS tokens (requires account key authentication)
            if (!blobClient.CanGenerateSasUri)
            {
                _logger.LogWarning("Cannot generate SAS URI. Service may be using Managed Identity without account key.");
                // Return the blob URI without SAS (will require authentication)
                return blobClient.Uri.ToString();
            }

            // Define SAS permissions and expiry
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b", // b = blob
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5), // Allow for clock skew
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes)
            };

            // Set read permissions
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);
            _logger.LogInformation("Generated SAS URI for {BlobName} for tenant {TenantId}, expires in {Minutes} minutes",
                blobName, tenantId, expiryMinutes);

            return sasUri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SAS URI for {FileName} for tenant {TenantId}", 
                fileName, tenantId);
            throw;
        }
    }

    /// <summary>
    /// Gets or creates a blob container with appropriate access level
    /// </summary>
    private async Task<BlobContainerClient> GetOrCreateContainerAsync(
        string containerName,
        CancellationToken cancellationToken)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        
        // Create container if it doesn't exist with private access (no anonymous access)
        await containerClient.CreateIfNotExistsAsync(
            PublicAccessType.None, 
            cancellationToken: cancellationToken);

        return containerClient;
    }

    /// <summary>
    /// Creates a tenant-isolated blob path by prefixing with tenant ID
    /// This ensures tenant data isolation at the storage level
    /// </summary>
    private static string GetTenantBlobPath(string tenantId, string fileName)
    {
        // Sanitize tenant ID and file name to prevent path traversal attacks
        var safeTenantId = SanitizePath(tenantId);
        var safeFileName = SanitizePath(fileName);
        
        return $"{safeTenantId}/{safeFileName}";
    }

    /// <summary>
    /// Sanitizes path components to prevent directory traversal attacks
    /// Uses allowlist approach to only permit safe characters
    /// </summary>
    private static string SanitizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or whitespace", nameof(path));
        }

        // Only allow alphanumeric, dash, underscore, and dot (but not ..)
        var sanitized = new string(path
            .Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.')
            .ToArray());

        // Remove any remaining path traversal attempts
        while (sanitized.Contains(".."))
        {
            sanitized = sanitized.Replace("..", string.Empty);
        }

        // Trim dots and dashes from start/end
        sanitized = sanitized.Trim('.', '-', '_');

        if (string.IsNullOrWhiteSpace(sanitized))
        {
            throw new ArgumentException("Path contains only invalid characters", nameof(path));
        }

        return sanitized;
    }
}
