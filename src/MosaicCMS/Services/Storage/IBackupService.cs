namespace MosaicCMS.Services.Storage;

/// <summary>
/// Service interface for tenant backup and restore operations
/// Provides dedicated functionality for backing up and restoring tenant data
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Creates a backup of all tenant files from specified containers
    /// </summary>
    /// <param name="tenantId">Tenant identifier</param>
    /// <param name="containers">List of container names to backup</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Backup metadata including backup ID and file count</returns>
    Task<BackupResult> CreateBackupAsync(
        string tenantId, 
        List<string> containers, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores tenant files from a backup
    /// </summary>
    /// <param name="tenantId">Tenant identifier</param>
    /// <param name="backupId">Unique backup identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Restore result with file count and status</returns>
    Task<RestoreResult> RestoreBackupAsync(
        string tenantId, 
        string backupId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all available backups for a tenant
    /// </summary>
    /// <param name="tenantId">Tenant identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of backup metadata</returns>
    Task<List<BackupMetadata>> ListBackupsAsync(
        string tenantId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a specific backup
    /// </summary>
    /// <param name="tenantId">Tenant identifier</param>
    /// <param name="backupId">Unique backup identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if backup was deleted successfully</returns>
    Task<bool> DeleteBackupAsync(
        string tenantId, 
        string backupId, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a backup operation
/// </summary>
public record BackupResult(
    string BackupId,
    string TenantId,
    int FileCount,
    long TotalSizeBytes,
    DateTime CreatedAt,
    List<string> BackedUpContainers
);

/// <summary>
/// Result of a restore operation
/// </summary>
public record RestoreResult(
    string BackupId,
    string TenantId,
    int FilesRestored,
    long TotalSizeBytes,
    DateTime RestoredAt,
    bool Success,
    string? ErrorMessage = null
);

/// <summary>
/// Metadata for a backup
/// </summary>
public record BackupMetadata(
    string BackupId,
    string TenantId,
    int FileCount,
    long TotalSizeBytes,
    DateTime CreatedAt,
    List<string> Containers
);
