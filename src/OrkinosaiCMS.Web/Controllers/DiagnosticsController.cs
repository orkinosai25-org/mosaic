using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrkinosaiCMS.Web.Services;

namespace OrkinosaiCMS.Web.Controllers;

/// <summary>
/// Diagnostics controller for troubleshooting application issues.
/// Provides endpoints to read configurations, errors, logs, and application status.
/// All endpoints are secured and require Administrator role.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class DiagnosticsController : ControllerBase
{
    private readonly IDiagnosticsService _diagnosticsService;
    private readonly ILogger<DiagnosticsController> _logger;

    public DiagnosticsController(
        IDiagnosticsService diagnosticsService,
        ILogger<DiagnosticsController> logger)
    {
        _diagnosticsService = diagnosticsService;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive application status including health checks, runtime info, and configuration
    /// GET /api/diagnostics/status
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus()
    {
        _logger.LogInformation("Diagnostics: Status requested by user {User}", User.Identity?.Name);
        
        try
        {
            var status = await _diagnosticsService.GetApplicationStatusAsync();
            
            return Ok(new
            {
                success = true,
                timestamp = DateTime.UtcNow,
                data = status
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving application status");
            return StatusCode(500, new
            {
                success = false,
                error = "Failed to retrieve application status",
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Get all application configuration with sensitive data redacted
    /// GET /api/diagnostics/config
    /// </summary>
    [HttpGet("config")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConfiguration()
    {
        _logger.LogInformation("Diagnostics: Configuration requested by user {User}", User.Identity?.Name);
        
        try
        {
            var config = await _diagnosticsService.GetConfigurationAsync();
            
            return Ok(new
            {
                success = true,
                timestamp = DateTime.UtcNow,
                note = "Sensitive values (passwords, secrets, keys) have been redacted for security",
                data = config
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving configuration");
            return StatusCode(500, new
            {
                success = false,
                error = "Failed to retrieve configuration",
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Get environment variables with sensitive data redacted
    /// GET /api/diagnostics/environment
    /// </summary>
    [HttpGet("environment")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetEnvironment()
    {
        _logger.LogInformation("Diagnostics: Environment variables requested by user {User}", User.Identity?.Name);
        
        try
        {
            var envVars = _diagnosticsService.GetEnvironmentVariables();
            
            return Ok(new
            {
                success = true,
                timestamp = DateTime.UtcNow,
                note = "Sensitive values (passwords, secrets, keys) have been redacted for security",
                count = envVars.Count,
                data = envVars
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving environment variables");
            return StatusCode(500, new
            {
                success = false,
                error = "Failed to retrieve environment variables",
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Get recent log entries
    /// GET /api/diagnostics/logs?maxLines=100
    /// </summary>
    [HttpGet("logs")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLogs([FromQuery] int maxLines = 100)
    {
        _logger.LogInformation("Diagnostics: Logs requested by user {User}, maxLines={MaxLines}", User.Identity?.Name, maxLines);
        
        try
        {
            if (maxLines < 1 || maxLines > 1000)
            {
                return BadRequest(new
                {
                    success = false,
                    error = "maxLines must be between 1 and 1000"
                });
            }
            
            var logs = await _diagnosticsService.GetRecentLogsAsync(maxLines);
            
            return Ok(new
            {
                success = true,
                timestamp = DateTime.UtcNow,
                count = logs.Count,
                maxLines,
                data = logs
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving logs");
            return StatusCode(500, new
            {
                success = false,
                error = "Failed to retrieve logs",
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Get recent errors from logs (filtered to ERROR and FATAL level entries)
    /// GET /api/diagnostics/errors?maxLines=50
    /// </summary>
    [HttpGet("errors")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetErrors([FromQuery] int maxLines = 50)
    {
        _logger.LogInformation("Diagnostics: Errors requested by user {User}, maxLines={MaxLines}", User.Identity?.Name, maxLines);
        
        try
        {
            if (maxLines < 1 || maxLines > 500)
            {
                return BadRequest(new
                {
                    success = false,
                    error = "maxLines must be between 1 and 500"
                });
            }
            
            // Fetch extra logs to ensure we get enough errors after filtering
            // Multiplier of 3 provides reasonable buffer while avoiding excessive data retrieval
            const int LogFetchMultiplier = 3;
            var allLogs = await _diagnosticsService.GetRecentLogsAsync(maxLines * LogFetchMultiplier);
            var errors = allLogs
                .Where(log => log.Level.Contains("ERR", StringComparison.OrdinalIgnoreCase) || 
                             log.Level.Contains("FTL", StringComparison.OrdinalIgnoreCase) ||
                             log.Level.Contains("FATAL", StringComparison.OrdinalIgnoreCase))
                .Take(maxLines)
                .ToList();
            
            return Ok(new
            {
                success = true,
                timestamp = DateTime.UtcNow,
                count = errors.Count,
                maxLines,
                data = errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving errors");
            return StatusCode(500, new
            {
                success = false,
                error = "Failed to retrieve errors",
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Get comprehensive diagnostics report with all information
    /// GET /api/diagnostics/report
    /// </summary>
    [HttpGet("report")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComprehensiveReport()
    {
        _logger.LogInformation("Diagnostics: Comprehensive report requested by user {User}", User.Identity?.Name);
        
        try
        {
            var report = new Dictionary<string, object>
            {
                ["timestamp"] = DateTime.UtcNow,
                ["requestedBy"] = User.Identity?.Name ?? "Unknown",
                ["status"] = await _diagnosticsService.GetApplicationStatusAsync(),
                ["configuration"] = await _diagnosticsService.GetConfigurationAsync(),
                ["environment"] = _diagnosticsService.GetEnvironmentVariables(),
                ["recentLogs"] = await _diagnosticsService.GetRecentLogsAsync(100)
            };
            
            // Filter recent errors
            var allLogs = (List<LogEntry>)report["recentLogs"];
            report["recentErrors"] = allLogs
                .Where(log => log.Level.Contains("ERR", StringComparison.OrdinalIgnoreCase) || 
                             log.Level.Contains("FTL", StringComparison.OrdinalIgnoreCase))
                .Take(20)
                .ToList();
            
            return Ok(new
            {
                success = true,
                note = "This comprehensive report includes all diagnostic information. Sensitive values have been redacted.",
                data = report
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating comprehensive report");
            return StatusCode(500, new
            {
                success = false,
                error = "Failed to generate comprehensive report",
                message = ex.Message
            });
        }
    }
}
