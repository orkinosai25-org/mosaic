namespace OrkinosaiCMS.Cnms.Interfaces;

/// <summary>
/// Layout engine interface inspired by Umbraco Grid and Oqtane's container system.
/// Provides flexible, designer-friendly layout management.
/// </summary>
public interface ILayoutEngine
{
    /// <summary>
    /// Renders a layout with modules/content.
    /// </summary>
    Task<LayoutRenderResult> RenderLayoutAsync(LayoutRenderContext context);

    /// <summary>
    /// Gets available layout templates.
    /// </summary>
    Task<IEnumerable<LayoutTemplate>> GetAvailableLayoutsAsync();

    /// <summary>
    /// Gets a specific layout template.
    /// </summary>
    Task<LayoutTemplate?> GetLayoutAsync(string layoutName);

    /// <summary>
    /// Validates layout configuration.
    /// </summary>
    Task<LayoutValidationResult> ValidateLayoutAsync(LayoutConfiguration config);

    /// <summary>
    /// Creates a layout from a template.
    /// </summary>
    Task<LayoutConfiguration> CreateFromTemplateAsync(string templateName, Dictionary<string, object> settings);
}

/// <summary>
/// Context for rendering a layout.
/// </summary>
public class LayoutRenderContext
{
    public string LayoutName { get; set; } = string.Empty;
    public int PageId { get; set; }
    public LayoutConfiguration Configuration { get; set; } = new();
    public Dictionary<string, object> Data { get; set; } = new();
}

/// <summary>
/// Layout template descriptor (similar to Umbraco Grid layouts).
/// </summary>
public class LayoutTemplate
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PreviewImageUrl { get; set; } = string.Empty;
    public List<LayoutArea> Areas { get; set; } = new();
    public Dictionary<string, object> DefaultSettings { get; set; } = new();
}

/// <summary>
/// Layout area (row/column structure).
/// </summary>
public class LayoutArea
{
    public string Name { get; set; } = string.Empty;
    public int Columns { get; set; } = 12; // Bootstrap-style 12-column grid
    public List<LayoutCell> Cells { get; set; } = new();
}

/// <summary>
/// Layout cell (individual content area).
/// </summary>
public class LayoutCell
{
    public int ColumnSpan { get; set; } = 12;
    public string CssClass { get; set; } = string.Empty;
    public List<int> ModuleIds { get; set; } = new();
}

/// <summary>
/// Layout configuration instance.
/// </summary>
public class LayoutConfiguration
{
    public int Id { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public List<LayoutArea> Areas { get; set; } = new();
    public Dictionary<string, object> Settings { get; set; } = new();
}

/// <summary>
/// Result of layout rendering.
/// </summary>
public class LayoutRenderResult
{
    public bool Success { get; set; }
    public string Html { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Result of layout validation.
/// </summary>
public class LayoutValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
