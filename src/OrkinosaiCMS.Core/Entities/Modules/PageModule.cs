using OrkinosaiCMS.Core.Common;

namespace OrkinosaiCMS.Core.Entities.Sites;

/// <summary>
/// Represents an instance of a module on a specific page
/// Links modules to pages with specific configuration
/// </summary>
public class PageModule : BaseEntity
{
    /// <summary>
    /// Page this module instance belongs to
    /// </summary>
    public int PageId { get; set; }

    /// <summary>
    /// Module definition/type
    /// </summary>
    public int ModuleId { get; set; }

    /// <summary>
    /// Display title for this module instance
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Content zone where the module should be rendered
    /// </summary>
    public string Zone { get; set; } = "Main";

    /// <summary>
    /// Order within the zone
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Whether the module title should be shown
    /// </summary>
    public bool ShowTitle { get; set; } = true;

    /// <summary>
    /// Instance-specific settings as JSON
    /// </summary>
    public string? Settings { get; set; }

    /// <summary>
    /// CSS class to apply to the module container
    /// </summary>
    public string? ContainerCssClass { get; set; }

    /// <summary>
    /// Whether the module is visible
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Navigation page
    /// </summary>
    public Page? Page { get; set; }

    /// <summary>
    /// Navigation module definition
    /// </summary>
    public Module? Module { get; set; }
}
