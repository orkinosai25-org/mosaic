using OrkinosaiCMS.Core.Common;

namespace OrkinosaiCMS.Core.Entities.Sites;

/// <summary>
/// Represents a master page/layout template
/// Inspired by SharePoint Master Pages
/// </summary>
public class MasterPage : BaseEntity
{
    /// <summary>
    /// Site this master page belongs to
    /// </summary>
    public int SiteId { get; set; }

    /// <summary>
    /// Master page name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Master page description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Path to the Razor component that implements this layout
    /// </summary>
    public string ComponentPath { get; set; } = string.Empty;

    /// <summary>
    /// Preview thumbnail URL
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Whether this is the default master page for the site
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Defines content zones available in this master page
    /// JSON array of zone names: ["Header", "Main", "Sidebar", "Footer"]
    /// </summary>
    public string ContentZones { get; set; } = "[]";

    /// <summary>
    /// Navigation site
    /// </summary>
    public Site? Site { get; set; }
}
