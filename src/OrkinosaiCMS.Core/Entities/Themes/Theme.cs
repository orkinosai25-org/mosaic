using OrkinosaiCMS.Core.Common;

namespace OrkinosaiCMS.Core.Entities.Sites;

/// <summary>
/// Represents a visual theme for the CMS
/// </summary>
public class Theme : BaseEntity
{
    /// <summary>
    /// Theme name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Theme description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Version of the theme
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Author of the theme
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Path to theme assets (CSS, images, etc.)
    /// </summary>
    public string AssetsPath { get; set; } = string.Empty;

    /// <summary>
    /// Preview thumbnail URL
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Whether the theme is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Whether this is a system theme
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Default settings as JSON
    /// </summary>
    public string? DefaultSettings { get; set; }

    /// <summary>
    /// Theme category (e.g., "SharePoint", "Modern", "Marketing", "Dashboard")
    /// </summary>
    public string Category { get; set; } = "General";

    /// <summary>
    /// Layout type (e.g., "TopNavigation", "LeftNavigation", "Portal", "Minimal")
    /// </summary>
    public string LayoutType { get; set; } = "TopNavigation";

    /// <summary>
    /// Primary brand color (hex color code)
    /// </summary>
    public string? PrimaryColor { get; set; }

    /// <summary>
    /// Secondary brand color (hex color code)
    /// </summary>
    public string? SecondaryColor { get; set; }

    /// <summary>
    /// Accent color (hex color code)
    /// </summary>
    public string? AccentColor { get; set; }

    /// <summary>
    /// Logo URL or path
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Custom CSS overrides
    /// </summary>
    public string? CustomCss { get; set; }

    /// <summary>
    /// Whether this theme is mobile responsive
    /// </summary>
    public bool IsMobileResponsive { get; set; } = true;

    /// <summary>
    /// SharePoint-inspired theme properties (stored as JSON)
    /// </summary>
    public string? SharePointThemeJson { get; set; }
}
