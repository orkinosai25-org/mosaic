using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MosaicCMS.Models;
using MosaicCMS.Services.Storage;

namespace MosaicCMS.Controllers;

/// <summary>
/// API controller for tenant backup and restore operations
/// Provides endpoints for creating, listing, and restoring backups of tenant data
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BackupController : ControllerBase
{
    private readonly IBackupService _backupService;
    private readonly AzureBlobStorageOptions _storageOptions;
    private readonly ILogger<BackupController> _logger;

    public BackupController(
        IBackupService backupService,
        IOptions<AzureBlobStorageOptions> storageOptions,
        ILogger<BackupController> logger)
    {
        _backupService = backupService;
        _storageOptions = storageOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Creates a backup of tenant data
    /// </summary>
    /// <param name="tenantId">Tenant identifier from header</param>
    /// <param name="request">Backup request with container selection</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Backup result with backup ID and metadata</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateBackup(
        [FromHeader(Name = "X-Tenant-Id")] string tenantId,
        [FromBody] CreateBackupRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return BadRequest(new { error = "Tenant ID is required in X-Tenant-Id header" });
        }

        if (request.Containers == null || !request.Containers.Any())
        {
            return BadRequest(new { error = "At least one container must be specified for backup" });
        }

        // Validate container names
        var validContainers = new[]
        {
            _storageOptions.Containers.Images,
            _storageOptions.Containers.Documents,
            _storageOptions.Containers.UserUploads,
            _storageOptions.Containers.MediaAssets
        };

        var invalidContainers = request.Containers.Where(c => !validContainers.Contains(c)).ToList();
        if (invalidContainers.Any())
        {
            return BadRequest(new
            {
                error = "Invalid container names",
                invalidContainers = invalidContainers,
                validContainers = validContainers
            });
        }

        try
        {
            var result = await _backupService.CreateBackupAsync(
                tenantId,
                request.Containers,
                cancellationToken);

            return Ok(new
            {
                backupId = result.BackupId,
                tenantId = result.TenantId,
                fileCount = result.FileCount,
                totalSizeBytes = result.TotalSizeBytes,
                createdAt = result.CreatedAt,
                containers = result.BackedUpContainers,
                message = "Backup created successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating backup for tenant {TenantId}", tenantId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to create backup" });
        }
    }

    /// <summary>
    /// Lists all backups for a tenant
    /// </summary>
    /// <param name="tenantId">Tenant identifier from header</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of backup metadata</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListBackups(
        [FromHeader(Name = "X-Tenant-Id")] string tenantId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return BadRequest(new { error = "Tenant ID is required in X-Tenant-Id header" });
        }

        try
        {
            var backups = await _backupService.ListBackupsAsync(tenantId, cancellationToken);

            return Ok(new
            {
                tenantId = tenantId,
                backupCount = backups.Count,
                backups = backups
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing backups for tenant {TenantId}", tenantId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to list backups" });
        }
    }

    /// <summary>
    /// Restores a backup for a tenant
    /// </summary>
    /// <param name="tenantId">Tenant identifier from header</param>
    /// <param name="backupId">Backup identifier to restore</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Restore result with status and file count</returns>
    [HttpPost("restore/{backupId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RestoreBackup(
        [FromHeader(Name = "X-Tenant-Id")] string tenantId,
        [FromRoute] string backupId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return BadRequest(new { error = "Tenant ID is required in X-Tenant-Id header" });
        }

        if (string.IsNullOrWhiteSpace(backupId))
        {
            return BadRequest(new { error = "Backup ID is required" });
        }

        try
        {
            var result = await _backupService.RestoreBackupAsync(tenantId, backupId, cancellationToken);

            if (!result.Success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    error = "Restore operation failed",
                    message = result.ErrorMessage
                });
            }

            return Ok(new
            {
                backupId = result.BackupId,
                tenantId = result.TenantId,
                filesRestored = result.FilesRestored,
                totalSizeBytes = result.TotalSizeBytes,
                restoredAt = result.RestoredAt,
                success = result.Success,
                message = "Backup restored successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring backup {BackupId} for tenant {TenantId}", 
                backupId, tenantId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to restore backup" });
        }
    }

    /// <summary>
    /// Deletes a backup
    /// </summary>
    /// <param name="tenantId">Tenant identifier from header</param>
    /// <param name="backupId">Backup identifier to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion result</returns>
    [HttpDelete("{backupId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBackup(
        [FromHeader(Name = "X-Tenant-Id")] string tenantId,
        [FromRoute] string backupId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return BadRequest(new { error = "Tenant ID is required in X-Tenant-Id header" });
        }

        if (string.IsNullOrWhiteSpace(backupId))
        {
            return BadRequest(new { error = "Backup ID is required" });
        }

        try
        {
            var deleted = await _backupService.DeleteBackupAsync(tenantId, backupId, cancellationToken);

            if (!deleted)
            {
                return NotFound(new { error = "Backup not found" });
            }

            return Ok(new
            {
                message = "Backup deleted successfully",
                backupId = backupId,
                tenantId = tenantId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting backup {BackupId} for tenant {TenantId}", 
                backupId, tenantId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to delete backup" });
        }
    }
}

/// <summary>
/// Request model for creating a backup
/// </summary>
public class CreateBackupRequest
{
    /// <summary>
    /// List of container names to include in the backup
    /// </summary>
    public List<string> Containers { get; set; } = new();
}
