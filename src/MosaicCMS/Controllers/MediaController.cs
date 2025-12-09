using Microsoft.AspNetCore.Mvc;
using MosaicCMS.Services.Storage;

namespace MosaicCMS.Controllers;

/// <summary>
/// API controller for media asset management using Azure Blob Storage
/// Supports multi-tenant file uploads with automatic tenant isolation
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<MediaController> _logger;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB limit

    private static readonly string[] AllowedImageTypes = 
    {
        "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp", "image/svg+xml"
    };

    private static readonly string[] AllowedDocumentTypes = 
    {
        "application/pdf", "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "text/plain", "text/csv"
    };

    public MediaController(
        IBlobStorageService blobStorageService,
        ILogger<MediaController> logger)
    {
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Upload an image file
    /// </summary>
    /// <param name="tenantId">Tenant identifier for isolation</param>
    /// <param name="file">Image file to upload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>URL of the uploaded image</returns>
    [HttpPost("images")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    public async Task<IActionResult> UploadImage(
        [FromHeader(Name = "X-Tenant-Id")] string tenantId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return BadRequest(new { error = "Tenant ID is required in X-Tenant-Id header" });
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file provided" });
        }

        if (file.Length > MaxFileSize)
        {
            return StatusCode(StatusCodes.Status413PayloadTooLarge,
                new { error = $"File size exceeds maximum limit of {MaxFileSize / (1024 * 1024)} MB" });
        }

        if (!AllowedImageTypes.Contains(file.ContentType.ToLower()))
        {
            return BadRequest(new { error = "Invalid image file type. Allowed types: JPEG, PNG, GIF, WebP, SVG" });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var uri = await _blobStorageService.UploadFileAsync(
                "images",
                tenantId,
                file.FileName,
                stream,
                file.ContentType,
                cancellationToken);

            return Ok(new
            {
                fileName = file.FileName,
                uri = uri,
                tenantId = tenantId,
                size = file.Length,
                contentType = file.ContentType,
                uploadedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image for tenant {TenantId}", tenantId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to upload image" });
        }
    }

    /// <summary>
    /// Upload a document file
    /// </summary>
    /// <param name="tenantId">Tenant identifier for isolation</param>
    /// <param name="file">Document file to upload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>URL of the uploaded document</returns>
    [HttpPost("documents")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadDocument(
        [FromHeader(Name = "X-Tenant-Id")] string tenantId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return BadRequest(new { error = "Tenant ID is required in X-Tenant-Id header" });
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file provided" });
        }

        if (file.Length > MaxFileSize)
        {
            return StatusCode(StatusCodes.Status413PayloadTooLarge,
                new { error = $"File size exceeds maximum limit of {MaxFileSize / (1024 * 1024)} MB" });
        }

        if (!AllowedDocumentTypes.Contains(file.ContentType.ToLower()))
        {
            return BadRequest(new { error = "Invalid document type. Allowed types: PDF, Word, Excel, Text, CSV" });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var uri = await _blobStorageService.UploadFileAsync(
                "documents",
                tenantId,
                file.FileName,
                stream,
                file.ContentType,
                cancellationToken);

            return Ok(new
            {
                fileName = file.FileName,
                uri = uri,
                tenantId = tenantId,
                size = file.Length,
                contentType = file.ContentType,
                uploadedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document for tenant {TenantId}", tenantId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to upload document" });
        }
    }

    /// <summary>
    /// List all files for a tenant in a container
    /// </summary>
    /// <param name="tenantId">Tenant identifier</param>
    /// <param name="containerType">Container type (images, documents, backups)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of file names</returns>
    [HttpGet("list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListFiles(
        [FromHeader(Name = "X-Tenant-Id")] string tenantId,
        [FromQuery] string containerType = "images",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return BadRequest(new { error = "Tenant ID is required in X-Tenant-Id header" });
        }

        var allowedContainers = new[] { "images", "documents", "user-uploads", "backups" };
        if (!allowedContainers.Contains(containerType.ToLower()))
        {
            return BadRequest(new { error = $"Invalid container type. Allowed: {string.Join(", ", allowedContainers)}" });
        }

        try
        {
            var files = await _blobStorageService.ListFilesAsync(
                containerType,
                tenantId,
                cancellationToken);

            return Ok(new
            {
                tenantId = tenantId,
                containerType = containerType,
                count = files.Count,
                files = files
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files for tenant {TenantId}", tenantId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to list files" });
        }
    }

    /// <summary>
    /// Delete a file
    /// </summary>
    /// <param name="tenantId">Tenant identifier</param>
    /// <param name="containerType">Container type</param>
    /// <param name="fileName">Name of file to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success status</returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteFile(
        [FromHeader(Name = "X-Tenant-Id")] string tenantId,
        [FromQuery] string containerType,
        [FromQuery] string fileName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return BadRequest(new { error = "Tenant ID is required in X-Tenant-Id header" });
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return BadRequest(new { error = "fileName is required" });
        }

        try
        {
            var deleted = await _blobStorageService.DeleteFileAsync(
                containerType,
                tenantId,
                fileName,
                cancellationToken);

            if (!deleted)
            {
                return NotFound(new { error = "File not found" });
            }

            return Ok(new
            {
                message = "File deleted successfully",
                tenantId = tenantId,
                fileName = fileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileName} for tenant {TenantId}", fileName, tenantId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to delete file" });
        }
    }

    /// <summary>
    /// Get a temporary SAS URL for secure file access
    /// </summary>
    /// <param name="tenantId">Tenant identifier</param>
    /// <param name="containerType">Container type</param>
    /// <param name="fileName">Name of file</param>
    /// <param name="expiryMinutes">Minutes until URL expires (default 60)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Temporary URL with SAS token</returns>
    [HttpGet("sas-url")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSasUrl(
        [FromHeader(Name = "X-Tenant-Id")] string tenantId,
        [FromQuery] string containerType,
        [FromQuery] string fileName,
        [FromQuery] int expiryMinutes = 60,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return BadRequest(new { error = "Tenant ID is required in X-Tenant-Id header" });
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return BadRequest(new { error = "fileName is required" });
        }

        if (expiryMinutes < 1 || expiryMinutes > 1440) // Max 24 hours
        {
            return BadRequest(new { error = "expiryMinutes must be between 1 and 1440" });
        }

        try
        {
            var sasUrl = await _blobStorageService.GetSasUriAsync(
                containerType,
                tenantId,
                fileName,
                expiryMinutes,
                cancellationToken);

            return Ok(new
            {
                sasUrl = sasUrl,
                expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
                tenantId = tenantId,
                fileName = fileName
            });
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning(ex, "File not found: {FileName} for tenant {TenantId}", fileName, tenantId);
            return NotFound(new { error = "File not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SAS URL for {FileName} for tenant {TenantId}", fileName, tenantId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to generate SAS URL" });
        }
    }
}
