using Microsoft.AspNetCore.Mvc;
using OrkinosaiCMS.Diagnostics.Services;
using OrkinosaiCMS.Diagnostics.Models;

namespace OrkinosaiCMS.Diagnostics.Controllers;

/// <summary>
/// API controller for diagnostic data
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly IDiagnosticDataService _diagnosticService;
    private readonly ILogger<DiagnosticsController> _logger;
    private readonly IConfiguration _configuration;
    
    public DiagnosticsController(
        IDiagnosticDataService diagnosticService,
        ILogger<DiagnosticsController> logger,
        IConfiguration _configuration)
    {
        _diagnosticService = diagnosticService;
        _logger = logger;
        this._configuration = _configuration;
    }
    
    /// <summary>
    /// Get comprehensive diagnostic report
    /// GET /api/diagnostics/report
    /// </summary>
    [HttpGet("report")]
    [ProducesResponseType(typeof(DiagnosticReport), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReport()
    {
        _logger.LogInformation("Diagnostic report requested");
        
        try
        {
            // Get main app path from configuration or use default
            var mainAppPath = _configuration["MainAppPath"] ?? 
                             Path.Combine(Directory.GetCurrentDirectory(), "..", "OrkinosaiCMS.Web");
            
            var report = await _diagnosticService.GenerateReportAsync(mainAppPath);
            
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating diagnostic report");
            return StatusCode(500, new
            {
                error = "Failed to generate diagnostic report",
                message = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }
    
    /// <summary>
    /// Health check endpoint
    /// GET /api/diagnostics/health
    /// </summary>
    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "OrkinosaiCMS.Diagnostics",
            timestamp = DateTime.UtcNow,
            message = "Diagnostic service is running"
        });
    }
}
