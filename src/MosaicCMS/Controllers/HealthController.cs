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
        
        // Check if blob storage is healthy or not_configured (both acceptable states)
        var isHealthy = blobStorageHealth is HealthCheckResult result && 
                       (result.Status == "healthy" || result.Status == "not_configured");

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
    private async Task<HealthCheckResult> CheckBlobStorageHealthAsync(CancellationToken cancellationToken)
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
                return new HealthCheckResult
                {
                    Status = "not_configured",
                    Configured = false,
                    AccountName = _storageOptions.AccountName,
                    Endpoint = _storageOptions.PrimaryEndpoint,
                    Message = "Azure Blob Storage service not initialized. Connection string may be missing.",
                    LastChecked = DateTime.UtcNow
                };
            }

            // Test connectivity by listing containers (without tenant-specific operations)
            var testTenantId = "health-check-test";
            var testFiles = await blobStorageService.ListFilesAsync(
                _storageOptions.Containers.Images,
                testTenantId,
                cancellationToken);

            return new HealthCheckResult
            {
                Status = "healthy",
                Configured = true,
                AccountName = _storageOptions.AccountName,
                Endpoint = _storageOptions.PrimaryEndpoint,
                Location = _storageOptions.Location,
                ContainersConfigured = new
                {
                    images = _storageOptions.Containers.Images,
                    documents = _storageOptions.Containers.Documents,
                    userUploads = _storageOptions.Containers.UserUploads,
                    backups = _storageOptions.Containers.Backups
                },
                LastChecked = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure Blob Storage health check failed");
            
            return new HealthCheckResult
            {
                Status = "unhealthy",
                Configured = !string.IsNullOrEmpty(_storageOptions.AccountName),
                AccountName = _storageOptions.AccountName,
                Error = ex.Message,
                ErrorType = ex.GetType().Name,
                LastChecked = DateTime.UtcNow
            };
        }
    }
}

/// <summary>
/// Health check result model for Azure Blob Storage
/// </summary>
public class HealthCheckResult
{
    public string Status { get; set; } = string.Empty;
    public bool Configured { get; set; }
    public string? AccountName { get; set; }
    public string? Endpoint { get; set; }
    public string? Location { get; set; }
    public object? ContainersConfigured { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
    public string? ErrorType { get; set; }
    public DateTime LastChecked { get; set; }
}
