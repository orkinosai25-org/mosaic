using Microsoft.Extensions.Logging;
using OrkinosaiCMS.Cnms.Interfaces;

namespace OrkinosaiCMS.Cnms.Services;

/// <summary>
/// Master page renderer implementation inspired by SharePoint and Umbraco.
/// Provides robust master page rendering with slot-based content areas.
/// </summary>
public class MasterPageRenderer : IMasterPageRenderer
{
    private readonly ILogger<MasterPageRenderer> _logger;
    private readonly Dictionary<string, List<ContentSlot>> _masterPageSlots = new();
    private readonly Dictionary<string, Dictionary<string, Func<Task<string>>>> _slotContentProviders = new();

    public MasterPageRenderer(ILogger<MasterPageRenderer> logger)
    {
        _logger = logger;
        InitializeDefaultMasterPages();
    }

    public async Task<RenderResult> RenderMasterPageAsync(MasterPageRenderContext context)
    {
        var result = new RenderResult();

        try
        {
            _logger.LogInformation("Rendering master page '{MasterPage}' for page {PageId}", 
                context.MasterPageName, context.PageId);

            // Validate master page exists
            if (!_masterPageSlots.ContainsKey(context.MasterPageName))
            {
                result.Success = false;
                result.Errors.Add($"Master page '{context.MasterPageName}' not found");
                return result;
            }

            // Validate required slots are provided
            var validation = await ValidateMasterPageAsync(context.MasterPageName);
            if (!validation.IsValid)
            {
                result.Success = false;
                result.Errors.AddRange(validation.Errors);
                return result;
            }

            var requiredSlots = validation.Slots.Where(s => s.IsRequired);
            foreach (var slot in requiredSlots)
            {
                if (!context.SlotContent.ContainsKey(slot.Name))
                {
                    result.Errors.Add($"Required slot '{slot.Name}' content not provided");
                }
            }

            if (result.Errors.Any())
            {
                result.Success = false;
                return result;
            }

            // Render master page HTML
            var html = await RenderMasterPageHtmlAsync(context);
            result.Success = true;
            result.Html = html;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering master page '{MasterPage}'", context.MasterPageName);
            result.Success = false;
            result.Errors.Add($"Rendering error: {ex.Message}");
            return result;
        }
    }

    public Task<IEnumerable<ContentSlot>> GetAvailableSlotsAsync(string masterPageName)
    {
        if (_masterPageSlots.TryGetValue(masterPageName, out var slots))
        {
            return Task.FromResult<IEnumerable<ContentSlot>>(slots);
        }

        return Task.FromResult<IEnumerable<ContentSlot>>(Enumerable.Empty<ContentSlot>());
    }

    public void RegisterSlotContent(string slotName, Func<Task<string>> contentProvider)
    {
        var currentPageKey = "default"; // This would be context-aware in a real implementation
        
        if (!_slotContentProviders.ContainsKey(currentPageKey))
        {
            _slotContentProviders[currentPageKey] = new Dictionary<string, Func<Task<string>>>();
        }

        _slotContentProviders[currentPageKey][slotName] = contentProvider;
        _logger.LogDebug("Registered content provider for slot '{SlotName}'", slotName);
    }

    public Task<MasterPageValidationResult> ValidateMasterPageAsync(string masterPageName)
    {
        var result = new MasterPageValidationResult { IsValid = true };

        try
        {
            if (!_masterPageSlots.TryGetValue(masterPageName, out var slots))
            {
                result.IsValid = false;
                result.Errors.Add($"Master page '{masterPageName}' not found");
                return Task.FromResult(result);
            }

            result.Slots = new List<ContentSlot>(slots);
            
            // Add warnings for best practices
            if (!slots.Any(s => s.Name.Equals("content", StringComparison.OrdinalIgnoreCase)))
            {
                result.Warnings.Add("Master page should have a 'content' slot for main content");
            }

            result.IsValid = result.Errors.Count == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating master page '{MasterPage}'", masterPageName);
            result.IsValid = false;
            result.Errors.Add($"Validation error: {ex.Message}");
        }

        return Task.FromResult(result);
    }

    private async Task<string> RenderMasterPageHtmlAsync(MasterPageRenderContext context)
    {
        var html = new System.Text.StringBuilder();
        
        // Standard HTML structure
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset=\"utf-8\" />");
        html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />");
        html.AppendLine($"    <title>{context.Parameters.GetValueOrDefault("Title", "Mosaic CMS")}</title>");
        html.AppendLine("    <link rel=\"stylesheet\" href=\"/css/bootstrap.min.css\" />");
        html.AppendLine("    <link rel=\"stylesheet\" href=\"/css/site.css\" />");
        
        // Render head slot if provided
        if (context.SlotContent.TryGetValue("head", out var headProvider))
        {
            var headContent = await headProvider();
            html.AppendLine(headContent);
        }
        
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        // Render header slot
        if (context.SlotContent.TryGetValue("header", out var headerProvider))
        {
            html.AppendLine("    <header>");
            var headerContent = await headerProvider();
            html.AppendLine(headerContent);
            html.AppendLine("    </header>");
        }

        // Render navigation slot
        if (context.SlotContent.TryGetValue("navigation", out var navProvider))
        {
            html.AppendLine("    <nav>");
            var navContent = await navProvider();
            html.AppendLine(navContent);
            html.AppendLine("    </nav>");
        }

        // Main content area
        html.AppendLine("    <main class=\"container\">");
        
        // Render content slot (required)
        if (context.SlotContent.TryGetValue("content", out var contentProvider))
        {
            var content = await contentProvider();
            html.AppendLine(content);
        }
        else
        {
            html.AppendLine("        <!-- No content provided -->");
        }
        
        html.AppendLine("    </main>");

        // Render footer slot
        if (context.SlotContent.TryGetValue("footer", out var footerProvider))
        {
            html.AppendLine("    <footer>");
            var footerContent = await footerProvider();
            html.AppendLine(footerContent);
            html.AppendLine("    </footer>");
        }

        // Render scripts slot
        if (context.SlotContent.TryGetValue("scripts", out var scriptsProvider))
        {
            var scripts = await scriptsProvider();
            html.AppendLine(scripts);
        }

        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    private void InitializeDefaultMasterPages()
    {
        // Standard master page with all common slots
        _masterPageSlots["standard"] = new List<ContentSlot>
        {
            new ContentSlot { Name = "head", Description = "Additional head content (CSS, meta tags)", IsRequired = false },
            new ContentSlot { Name = "header", Description = "Page header/banner", IsRequired = false },
            new ContentSlot { Name = "navigation", Description = "Main navigation menu", IsRequired = false },
            new ContentSlot { Name = "content", Description = "Main page content", IsRequired = true },
            new ContentSlot { Name = "footer", Description = "Page footer", IsRequired = false },
            new ContentSlot { Name = "scripts", Description = "Additional scripts", IsRequired = false }
        };

        // Full-width master page (minimal chrome)
        _masterPageSlots["full-width"] = new List<ContentSlot>
        {
            new ContentSlot { Name = "head", Description = "Additional head content", IsRequired = false },
            new ContentSlot { Name = "content", Description = "Full-width content", IsRequired = true },
            new ContentSlot { Name = "scripts", Description = "Additional scripts", IsRequired = false }
        };

        // Blog master page
        _masterPageSlots["blog"] = new List<ContentSlot>
        {
            new ContentSlot { Name = "head", Description = "Additional head content", IsRequired = false },
            new ContentSlot { Name = "header", Description = "Blog header", IsRequired = false },
            new ContentSlot { Name = "navigation", Description = "Blog navigation", IsRequired = false },
            new ContentSlot { Name = "content", Description = "Blog post content", IsRequired = true },
            new ContentSlot { Name = "sidebar", Description = "Blog sidebar (categories, tags)", IsRequired = false },
            new ContentSlot { Name = "footer", Description = "Blog footer", IsRequired = false },
            new ContentSlot { Name = "scripts", Description = "Additional scripts", IsRequired = false }
        };

        _logger.LogInformation("Initialized {Count} default master pages", _masterPageSlots.Count);
    }
}
