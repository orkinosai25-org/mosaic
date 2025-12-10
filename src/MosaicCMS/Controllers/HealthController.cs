using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MosaicCMS.Models;
using MosaicCMS.Services.Storage;

namespace MosaicCMS.Controllers;

/// <summary>
/// Health check controller for monitoring system status and dependencies
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AzureBlobStorageOptions _storageOptions;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        IOptions<AzureBlobStorageOptions> storageOptions,
        ILogger<HealthController> logger)
    {
        _storageOptions = storageOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Basic health check endpoint
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "healthy",
            service = "MOSAIC CMS",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }

    /// <summary>
    /// Detailed health check including Azure Blob Storage connectivity
    /// </summary>
    [HttpGet("detailed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetDetailedHealth(CancellationToken cancellationToken)
    {
        var blobStorageHealth = await CheckBlobStorageHealthAsync(cancellationToken);
        var isHealthy = blobStorageHealth.GetType().GetProperty("status")?.GetValue(blobStorageHealth)?.ToString() == "healthy";

        var healthStatus = new
        {
            status = isHealthy ? "healthy" : "unhealthy",
            service = "MOSAIC CMS",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            dependencies = new
            {
                azureBlobStorage = blobStorageHealth
            }
        };
        
        if (!isHealthy)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, healthStatus);
        }

        return Ok(healthStatus);
    }

    /// <summary>
    /// Checks Azure Blob Storage connectivity and configuration
    /// </summary>
    private async Task<object> CheckBlobStorageHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Try to create a blob storage service dynamically for health check
            // This allows health check to work even if main service initialization failed
            var serviceProvider = HttpContext.RequestServices;
            var blobStorageService = serviceProvider.GetService<IBlobStorageService>();
            
            if (blobStorageService == null)
            {
                // Service not available, likely due to missing connection string
                return new
                {
                    status = "not_configured",
                    configured = false,
                    accountName = _storageOptions.AccountName,
                    endpoint = _storageOptions.PrimaryEndpoint,
                    message = "Azure Blob Storage service not initialized. Connection string may be missing.",
                    lastChecked = DateTime.UtcNow
                };
            }

            // Test connectivity by listing containers (without tenant-specific operations)
            var testTenantId = "health-check-test";
            var testFiles = await blobStorageService.ListFilesAsync(
                _storageOptions.Containers.Images,
                testTenantId,
                cancellationToken);

            return new
            {
                status = "healthy",
                configured = true,
                accountName = _storageOptions.AccountName,
                endpoint = _storageOptions.PrimaryEndpoint,
                location = _storageOptions.Location,
                containersConfigured = new
                {
                    images = _storageOptions.Containers.Images,
                    documents = _storageOptions.Containers.Documents,
                    userUploads = _storageOptions.Containers.UserUploads,
                    backups = _storageOptions.Containers.Backups
                },
                lastChecked = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure Blob Storage health check failed");
            
            return new
            {
                status = "unhealthy",
                configured = !string.IsNullOrEmpty(_storageOptions.AccountName),
                accountName = _storageOptions.AccountName,
                error = ex.Message,
                errorType = ex.GetType().Name,
                lastChecked = DateTime.UtcNow
            };
        }
    }
}
