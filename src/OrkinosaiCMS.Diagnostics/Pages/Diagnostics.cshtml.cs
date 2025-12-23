using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OrkinosaiCMS.Diagnostics.Pages;

public class DiagnosticsModel : PageModel
{
    private readonly ILogger<DiagnosticsModel> _logger;
    
    public DiagnosticsModel(ILogger<DiagnosticsModel> logger)
    {
        _logger = logger;
    }
    
    public void OnGet()
    {
        _logger.LogInformation("Diagnostics page accessed");
    }
}
