using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrkinosaiCMS.Infrastructure.Data;

namespace OrkinosaiCMS.Web.Controllers;

/// <summary>
/// Health check controller for monitoring application and database status
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(ApplicationDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Basic health check endpoint
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            service = "Mosaic CMS"
        });
    }

    /// <summary>
    /// Database connectivity health check
    /// </summary>
    [HttpGet("database")]
    public async Task<IActionResult> CheckDatabase()
    {
        try
        {
            // Try to execute a simple query to verify database connectivity
            var canConnect = await _context.Database.CanConnectAsync();
            
            if (!canConnect)
            {
                _logger.LogWarning("Database health check failed: Cannot connect to database");
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    component = "database",
                    message = "Cannot connect to database",
                    timestamp = DateTime.UtcNow
                });
            }

            // Optionally check if database is responding with a simple query
            var siteCount = await _context.Sites.CountAsync();
            
            _logger.LogInformation("Database health check passed. Sites count: {SiteCount}", siteCount);
            
            return Ok(new
            {
                status = "healthy",
                component = "database",
                message = "Database connection successful",
                sitesCount = siteCount,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "Database health check failed with SQL exception");
            
            return StatusCode(503, new
            {
                status = "unhealthy",
                component = "database",
                message = "Database connection error",
                errorType = "SqlException",
                timestamp = DateTime.UtcNow
            });
        }
        catch (TimeoutException timeoutEx)
        {
            _logger.LogError(timeoutEx, "Database health check failed with timeout");
            
            return StatusCode(503, new
            {
                status = "unhealthy",
                component = "database",
                message = "Database connection timeout",
                errorType = "TimeoutException",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed with unexpected error");
            
            return StatusCode(503, new
            {
                status = "unhealthy",
                component = "database",
                message = "Unexpected database error",
                errorType = ex.GetType().Name,
                timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Comprehensive health check for all components
    /// </summary>
    [HttpGet("ready")]
    public async Task<IActionResult> CheckReadiness()
    {
        var checks = new Dictionary<string, object>();
        var overallHealthy = true;

        // Check application
        checks["application"] = new { status = "healthy" };

        // Check database
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            checks["database"] = new
            {
                status = canConnect ? "healthy" : "unhealthy",
                canConnect
            };
            
            if (!canConnect)
            {
                overallHealthy = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness check: Database connectivity failed");
            checks["database"] = new
            {
                status = "unhealthy",
                error = ex.GetType().Name
            };
            overallHealthy = false;
        }

        var statusCode = overallHealthy ? 200 : 503;
        
        return StatusCode(statusCode, new
        {
            status = overallHealthy ? "ready" : "not_ready",
            checks,
            timestamp = DateTime.UtcNow
        });
    }
}
