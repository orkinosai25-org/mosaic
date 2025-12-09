using OrkinosaiCMS.Core.Entities.Sites;

namespace OrkinosaiCMS.Core.Interfaces.Services;

/// <summary>
/// Service for managing CMS themes
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Get all themes
    /// </summary>
    Task<IEnumerable<Theme>> GetAllThemesAsync();

    /// <summary>
    /// Get all enabled themes
    /// </summary>
    Task<IEnumerable<Theme>> GetEnabledThemesAsync();

    /// <summary>
    /// Get theme by ID
    /// </summary>
    Task<Theme?> GetThemeByIdAsync(int id);

    /// <summary>
    /// Get theme by name
    /// </summary>
    Task<Theme?> GetThemeByNameAsync(string name);

    /// <summary>
    /// Get active theme for a site
    /// </summary>
    Task<Theme?> GetActiveSiteThemeAsync(int siteId);

    /// <summary>
    /// Create a new theme
    /// </summary>
    Task<Theme> CreateThemeAsync(Theme theme);

    /// <summary>
    /// Update an existing theme
    /// </summary>
    Task<Theme> UpdateThemeAsync(Theme theme);

    /// <summary>
    /// Delete a theme (soft delete)
    /// </summary>
    Task DeleteThemeAsync(int id);

    /// <summary>
    /// Apply theme to a site
    /// </summary>
    Task ApplyThemeToSiteAsync(int siteId, int themeId);

    /// <summary>
    /// Update theme branding (colors, logo)
    /// </summary>
    Task<Theme> UpdateThemeBrandingAsync(int themeId, string? primaryColor, string? secondaryColor, string? accentColor, string? logoUrl);

    /// <summary>
    /// Clone a theme to create a custom version
    /// </summary>
    Task<Theme> CloneThemeAsync(int themeId, string newName, string? newDescription = null);

    /// <summary>
    /// Get themes by category
    /// </summary>
    Task<IEnumerable<Theme>> GetThemesByCategoryAsync(string category);

    /// <summary>
    /// Get themes by layout type
    /// </summary>
    Task<IEnumerable<Theme>> GetThemesByLayoutTypeAsync(string layoutType);
}
