using Microsoft.AspNetCore.Mvc;

namespace OrkinosaiCMS.Web.Controllers;

/// <summary>
/// Test controller for verifying logging functionality (Development only)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;
    private readonly IHostEnvironment _environment;

    public TestController(ILogger<TestController> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Test endpoint to verify exception logging (Development only)
    /// </summary>
    [HttpGet("exception")]
    public IActionResult TestException()
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        _logger.LogInformation("Test exception endpoint called");
        
        try
        {
            // Simulate a nested exception
            throw new InvalidOperationException(
                "This is a test exception to verify Serilog logging",
                new ArgumentException("This is an inner exception with additional context", "testParameter"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test exception was thrown successfully. Site creation simulation failed with parameters: SiteName={SiteName}, AdminEmail={AdminEmail}", 
                "TestSite", "test@example.com");
            throw;
        }
    }

    /// <summary>
    /// Test endpoint to verify different log levels
    /// </summary>
    [HttpGet("log-levels")]
    public IActionResult TestLogLevels()
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        _logger.LogTrace("This is a TRACE level message");
        _logger.LogDebug("This is a DEBUG level message");
        _logger.LogInformation("This is an INFORMATION level message");
        _logger.LogWarning("This is a WARNING level message");
        _logger.LogError("This is an ERROR level message");
        _logger.LogCritical("This is a CRITICAL level message");

        return Ok(new { message = "Log levels tested. Check the log file at App_Data/Logs/" });
    }
}
