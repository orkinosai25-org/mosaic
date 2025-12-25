namespace MosaicCMS.Services.Storage;

/// <summary>
/// Interface for Azure Blob Storage operations
/// Supports multi-tenant media/asset management with tenant isolation
/// </summary>
public interface IBlobStorageService
{
    /// <summary>
    /// Uploads a file to Azure Blob Storage with tenant isolation
    /// </summary>
    /// <param name="containerName">Name of the blob container</param>
    /// <param name="tenantId">Unique identifier for the tenant (for isolation)</param>
    /// <param name="fileName">Name of the file to upload</param>
    /// <param name="fileStream">Stream containing file data</param>
    /// <param name="contentType">MIME type of the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>URI of the uploaded blob</returns>
    Task<string> UploadFileAsync(
        string containerName,
        string tenantId,
        string fileName,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file from Azure Blob Storage
    /// </summary>
    /// <param name="containerName">Name of the blob container</param>
    /// <param name="tenantId">Unique identifier for the tenant</param>
    /// <param name="fileName">Name of the file to download</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stream containing file data</returns>
    Task<Stream> DownloadFileAsync(
        string containerName,
        string tenantId,
        string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from Azure Blob Storage
    /// </summary>
    /// <param name="containerName">Name of the blob container</param>
    /// <param name="tenantId">Unique identifier for the tenant</param>
    /// <param name="fileName">Name of the file to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> DeleteFileAsync(
        string containerName,
        string tenantId,
        string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all files for a tenant in a container
    /// </summary>
    /// <param name="containerName">Name of the blob container</param>
    /// <param name="tenantId">Unique identifier for the tenant</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of blob file names</returns>
    Task<List<string>> ListFilesAsync(
        string containerName,
        string tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a SAS URI for temporary access to a blob
    /// </summary>
    /// <param name="containerName">Name of the blob container</param>
    /// <param name="tenantId">Unique identifier for the tenant</param>
    /// <param name="fileName">Name of the file</param>
    /// <param name="expiryMinutes">Number of minutes until SAS token expires</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>SAS URI for temporary access</returns>
    Task<string> GetSasUriAsync(
        string containerName,
        string tenantId,
        string fileName,
        int expiryMinutes = 60,
        CancellationToken cancellationToken = default);
}
