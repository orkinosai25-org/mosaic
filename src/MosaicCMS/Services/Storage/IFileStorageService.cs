namespace MosaicCMS.Services.Storage;

/// <summary>
/// Abstract interface for file storage operations
/// Provides a clean abstraction layer for file/media handling
/// Can be implemented by different storage providers (Azure Blob, AWS S3, local file system, etc.)
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to storage with tenant isolation
    /// </summary>
    Task<FileUploadResult> UploadAsync(FileUploadRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file from storage
    /// </summary>
    Task<FileDownloadResult> DownloadAsync(FileDownloadRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from storage
    /// </summary>
    Task<bool> DeleteAsync(FileDeleteRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all files for a tenant in a container
    /// </summary>
    Task<FileListResult> ListAsync(FileListRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a temporary access URL for a file
    /// </summary>
    Task<string> GetTemporaryAccessUrlAsync(FileAccessRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Request model for file upload operations
/// </summary>
public record FileUploadRequest(
    string ContainerName,
    string TenantId,
    string FileName,
    Stream FileStream,
    string ContentType
);

/// <summary>
/// Result model for file upload operations
/// </summary>
public record FileUploadResult(
    string Uri,
    string FileName,
    long FileSize,
    string ContentType,
    DateTime UploadedAt
);

/// <summary>
/// Request model for file download operations
/// </summary>
public record FileDownloadRequest(
    string ContainerName,
    string TenantId,
    string FileName
);

/// <summary>
/// Result model for file download operations
/// </summary>
public record FileDownloadResult(
    Stream Content,
    string ContentType,
    long ContentLength,
    string FileName
);

/// <summary>
/// Request model for file deletion operations
/// </summary>
public record FileDeleteRequest(
    string ContainerName,
    string TenantId,
    string FileName
);

/// <summary>
/// Request model for listing files
/// </summary>
public record FileListRequest(
    string ContainerName,
    string TenantId,
    string? Prefix = null
);

/// <summary>
/// Result model for file listing operations
/// </summary>
public record FileListResult(
    List<FileMetadata> Files,
    int TotalCount
);

/// <summary>
/// Metadata information for a file
/// </summary>
public record FileMetadata(
    string FileName,
    long FileSize,
    string ContentType,
    DateTime LastModified,
    Dictionary<string, string>? Metadata = null
);

/// <summary>
/// Request model for temporary file access
/// </summary>
public record FileAccessRequest(
    string ContainerName,
    string TenantId,
    string FileName,
    int ExpiryMinutes = 60
);
