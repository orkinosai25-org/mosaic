namespace OrkinosaiCMS.Cnms.Interfaces;

/// <summary>
/// Theme provider interface inspired by Umbraco's theme system.
/// Provides robust theme discovery, loading, and management capabilities.
/// </summary>
public interface IThemeProvider
{
    /// <summary>
    /// Gets all available themes in the system.
    /// </summary>
    Task<IEnumerable<ThemeDescriptor>> GetAvailableThemesAsync();

    /// <summary>
    /// Gets a specific theme by name.
    /// </summary>
    Task<ThemeDescriptor?> GetThemeAsync(string themeName);

    /// <summary>
    /// Gets the active theme for a site.
    /// </summary>
    Task<ThemeDescriptor?> GetActiveThemeAsync(int siteId);

    /// <summary>
    /// Sets the active theme for a site.
    /// </summary>
    Task<bool> SetActiveThemeAsync(int siteId, string themeName);

    /// <summary>
    /// Validates a theme configuration.
    /// </summary>
    Task<ThemeValidationResult> ValidateThemeAsync(string themeName);

    /// <summary>
    /// Resolves theme asset path (CSS, JS, images).
    /// </summary>
    string ResolveAssetPath(string themeName, string assetPath);
}

/// <summary>
/// Describes a theme's metadata and configuration.
/// </summary>
public class ThemeDescriptor
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string PreviewImageUrl { get; set; } = string.Empty;
    public List<string> MasterPages { get; set; } = new();
    public List<string> Layouts { get; set; } = new();
    public Dictionary<string, string> Settings { get; set; } = new();
    public bool SupportsLightMode { get; set; } = true;
    public bool SupportsDarkMode { get; set; } = true;
}

/// <summary>
/// Result of theme validation.
/// </summary>
public class ThemeValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
