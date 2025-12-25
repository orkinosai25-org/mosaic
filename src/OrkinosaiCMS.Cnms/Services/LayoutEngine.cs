using Microsoft.Extensions.Logging;
using OrkinosaiCMS.Cnms.Interfaces;

namespace OrkinosaiCMS.Cnms.Services;

/// <summary>
/// Layout engine implementation inspired by Umbraco Grid and Oqtane containers.
/// Provides flexible layout rendering with Bootstrap-style grid system.
/// </summary>
public class LayoutEngine : ILayoutEngine
{
    private readonly ILogger<LayoutEngine> _logger;
    private readonly Dictionary<string, LayoutTemplate> _templateCache = new();

    public LayoutEngine(ILogger<LayoutEngine> logger)
    {
        _logger = logger;
        InitializeDefaultTemplates();
    }

    public async Task<LayoutRenderResult> RenderLayoutAsync(LayoutRenderContext context)
    {
        var result = new LayoutRenderResult();

        try
        {
            _logger.LogInformation("Rendering layout '{LayoutName}' for page {PageId}", 
                context.LayoutName, context.PageId);

            var template = await GetLayoutAsync(context.LayoutName);
            if (template == null)
            {
                result.Success = false;
                result.Errors.Add($"Layout template '{context.LayoutName}' not found");
                return result;
            }

            // Validate configuration
            var validation = await ValidateLayoutAsync(context.Configuration);
            if (!validation.IsValid)
            {
                result.Success = false;
                result.Errors.AddRange(validation.Errors);
                return result;
            }

            // Render layout HTML
            var html = RenderLayoutHtml(context.Configuration, template);
            result.Success = true;
            result.Html = html;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering layout '{LayoutName}'", context.LayoutName);
            result.Success = false;
            result.Errors.Add($"Rendering error: {ex.Message}");
            return result;
        }
    }

    public Task<IEnumerable<LayoutTemplate>> GetAvailableLayoutsAsync()
    {
        return Task.FromResult<IEnumerable<LayoutTemplate>>(_templateCache.Values);
    }

    public Task<LayoutTemplate?> GetLayoutAsync(string layoutName)
    {
        _templateCache.TryGetValue(layoutName, out var template);
        return Task.FromResult(template);
    }

    public Task<LayoutValidationResult> ValidateLayoutAsync(LayoutConfiguration config)
    {
        var result = new LayoutValidationResult { IsValid = true };

        try
        {
            if (string.IsNullOrWhiteSpace(config.TemplateName))
            {
                result.Errors.Add("Template name is required");
            }

            // Validate areas
            foreach (var area in config.Areas)
            {
                if (area.Cells.Sum(c => c.ColumnSpan) > area.Columns)
                {
                    result.Errors.Add($"Area '{area.Name}' has cells exceeding total columns");
                }
            }

            result.IsValid = result.Errors.Count == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating layout configuration");
            result.IsValid = false;
            result.Errors.Add($"Validation error: {ex.Message}");
        }

        return Task.FromResult(result);
    }

    public Task<LayoutConfiguration> CreateFromTemplateAsync(string templateName, Dictionary<string, object> settings)
    {
        if (!_templateCache.TryGetValue(templateName, out var template))
        {
            throw new ArgumentException($"Template '{templateName}' not found", nameof(templateName));
        }

        var config = new LayoutConfiguration
        {
            TemplateName = templateName,
            Areas = new List<LayoutArea>(template.Areas),
            Settings = new Dictionary<string, object>(settings)
        };

        // Merge default settings
        foreach (var (key, value) in template.DefaultSettings)
        {
            if (!config.Settings.ContainsKey(key))
            {
                config.Settings[key] = value;
            }
        }

        return Task.FromResult(config);
    }

    private string RenderLayoutHtml(LayoutConfiguration config, LayoutTemplate template)
    {
        var html = new System.Text.StringBuilder();
        html.AppendLine($"<div class=\"layout layout-{config.TemplateName}\">");

        foreach (var area in config.Areas)
        {
            html.AppendLine($"  <div class=\"layout-area layout-area-{area.Name}\">");
            html.AppendLine("    <div class=\"row\">");

            foreach (var cell in area.Cells)
            {
                var colClass = GetBootstrapColumnClass(cell.ColumnSpan);
                html.AppendLine($"      <div class=\"{colClass} {cell.CssClass}\">");
                html.AppendLine($"        <!-- Module content for cell: {cell.ModuleIds.Count} modules -->");
                
                foreach (var moduleId in cell.ModuleIds)
                {
                    html.AppendLine($"        <div class=\"module-container\" data-module-id=\"{moduleId}\"></div>");
                }
                
                html.AppendLine("      </div>");
            }

            html.AppendLine("    </div>");
            html.AppendLine("  </div>");
        }

        html.AppendLine("</div>");
        return html.ToString();
    }

    private string GetBootstrapColumnClass(int columnSpan)
    {
        return $"col-md-{columnSpan}";
    }

    private void InitializeDefaultTemplates()
    {
        // Single column layout
        _templateCache["single-column"] = new LayoutTemplate
        {
            Name = "single-column",
            DisplayName = "Single Column",
            Description = "Simple single column layout",
            Areas = new List<LayoutArea>
            {
                new LayoutArea
                {
                    Name = "main",
                    Columns = 12,
                    Cells = new List<LayoutCell>
                    {
                        new LayoutCell { ColumnSpan = 12 }
                    }
                }
            }
        };

        // Two column layout (8-4)
        _templateCache["two-column-8-4"] = new LayoutTemplate
        {
            Name = "two-column-8-4",
            DisplayName = "Two Columns (8-4)",
            Description = "Two column layout with 8-4 split",
            Areas = new List<LayoutArea>
            {
                new LayoutArea
                {
                    Name = "main",
                    Columns = 12,
                    Cells = new List<LayoutCell>
                    {
                        new LayoutCell { ColumnSpan = 8, CssClass = "main-content" },
                        new LayoutCell { ColumnSpan = 4, CssClass = "sidebar" }
                    }
                }
            }
        };

        // Three column layout (3-6-3)
        _templateCache["three-column-3-6-3"] = new LayoutTemplate
        {
            Name = "three-column-3-6-3",
            DisplayName = "Three Columns (3-6-3)",
            Description = "Three column layout with 3-6-3 split",
            Areas = new List<LayoutArea>
            {
                new LayoutArea
                {
                    Name = "main",
                    Columns = 12,
                    Cells = new List<LayoutCell>
                    {
                        new LayoutCell { ColumnSpan = 3, CssClass = "left-sidebar" },
                        new LayoutCell { ColumnSpan = 6, CssClass = "main-content" },
                        new LayoutCell { ColumnSpan = 3, CssClass = "right-sidebar" }
                    }
                }
            }
        };

        _logger.LogInformation("Initialized {Count} default layout templates", _templateCache.Count);
    }
}
