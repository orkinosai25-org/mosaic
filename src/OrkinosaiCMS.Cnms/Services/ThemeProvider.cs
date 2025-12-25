using Microsoft.Extensions.Logging;
using OrkinosaiCMS.Cnms.Interfaces;
using OrkinosaiCMS.Core.Interfaces.Services;

namespace OrkinosaiCMS.Cnms.Services;

/// <summary>
/// Theme provider implementation inspired by Umbraco's theme system.
/// Provides robust theme management with caching and validation.
/// </summary>
public class ThemeProvider : IThemeProvider
{
    private readonly IThemeService _themeService;
    private readonly ILogger<ThemeProvider> _logger;
    private readonly Dictionary<string, ThemeDescriptor> _themeCache = new();

    public ThemeProvider(
        IThemeService themeService,
        ILogger<ThemeProvider> logger)
    {
        _themeService = themeService;
        _logger = logger;
    }

    public async Task<IEnumerable<ThemeDescriptor>> GetAvailableThemesAsync()
    {
        try
        {
            _logger.LogInformation("Loading available themes");
            
            var themes = await _themeService.GetAllThemesAsync();
            var descriptors = new List<ThemeDescriptor>();

            foreach (var theme in themes)
            {
                var descriptor = MapToDescriptor(theme);
                descriptors.Add(descriptor);
                _themeCache[theme.Name] = descriptor;
            }

            _logger.LogInformation("Loaded {Count} themes", descriptors.Count);
            return descriptors;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading available themes");
            return Enumerable.Empty<ThemeDescriptor>();
        }
    }

    public async Task<ThemeDescriptor?> GetThemeAsync(string themeName)
    {
        try
        {
            // Check cache first
            if (_themeCache.TryGetValue(themeName, out var cached))
            {
                return cached;
            }

            var theme = await _themeService.GetThemeByNameAsync(themeName);
            if (theme == null)
            {
                _logger.LogWarning("Theme '{ThemeName}' not found", themeName);
                return null;
            }

            var descriptor = MapToDescriptor(theme);
            _themeCache[themeName] = descriptor;
            return descriptor;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading theme '{ThemeName}'", themeName);
            return null;
        }
    }

    public async Task<ThemeDescriptor?> GetActiveThemeAsync(int siteId)
    {
        try
        {
            var theme = await _themeService.GetActiveSiteThemeAsync(siteId);
            if (theme == null)
            {
                _logger.LogWarning("No active theme found for site {SiteId}", siteId);
                return null;
            }

            return await GetThemeAsync(theme.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active theme for site {SiteId}", siteId);
            return null;
        }
    }

    public async Task<bool> SetActiveThemeAsync(int siteId, string themeName)
    {
        try
        {
            _logger.LogInformation("Setting theme '{ThemeName}' as active for site {SiteId}", themeName, siteId);

            // Validate theme exists
            var theme = await _themeService.GetThemeByNameAsync(themeName);
            if (theme == null)
            {
                _logger.LogWarning("Cannot set theme '{ThemeName}' - theme not found", themeName);
                return false;
            }

            // Apply theme to site using existing service method
            await _themeService.ApplyThemeToSiteAsync(siteId, theme.Id);
            
            _logger.LogInformation("Theme '{ThemeName}' set as active for site {SiteId}", themeName, siteId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting active theme '{ThemeName}' for site {SiteId}", themeName, siteId);
            return false;
        }
    }

    public async Task<ThemeValidationResult> ValidateThemeAsync(string themeName)
    {
        var result = new ThemeValidationResult();

        try
        {
            var theme = await GetThemeAsync(themeName);
            if (theme == null)
            {
                result.IsValid = false;
                result.Errors.Add($"Theme '{themeName}' not found");
                return result;
            }

            // Validate theme structure
            if (string.IsNullOrWhiteSpace(theme.Name))
            {
                result.Errors.Add("Theme name is required");
            }

            if (string.IsNullOrWhiteSpace(theme.DisplayName))
            {
                result.Errors.Add("Theme display name is required");
            }

            // Add warnings for optional fields
            if (string.IsNullOrWhiteSpace(theme.Description))
            {
                result.Warnings.Add("Theme description is recommended");
            }

            if (string.IsNullOrWhiteSpace(theme.PreviewImageUrl))
            {
                result.Warnings.Add("Preview image is recommended for better UX");
            }

            result.IsValid = result.Errors.Count == 0;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating theme '{ThemeName}'", themeName);
            result.IsValid = false;
            result.Errors.Add($"Validation error: {ex.Message}");
            return result;
        }
    }

    public string ResolveAssetPath(string themeName, string assetPath)
    {
        // Resolve theme asset paths (CSS, JS, images)
        // Following Umbraco pattern: /themes/{themeName}/assets/{assetPath}
        return $"/themes/{themeName}/assets/{assetPath.TrimStart('/')}";
    }

    /// <summary>
    /// Maps a Theme entity to a ThemeDescriptor.
    /// Helper method to avoid code duplication and provide consistent mapping.
    /// </summary>
    private ThemeDescriptor MapToDescriptor(OrkinosaiCMS.Core.Entities.Sites.Theme theme)
    {
        return new ThemeDescriptor
        {
            // Theme entity uses Name field for both internal name and display
            // Using Name as DisplayName as a fallback until Theme entity is updated with DisplayName property
            Name = theme.Name,
            DisplayName = theme.Name,
            Description = theme.Description ?? string.Empty,
            Author = theme.Author ?? string.Empty,
            Version = theme.Version ?? "1.0.0",
            PreviewImageUrl = theme.ThumbnailUrl ?? string.Empty,
            SupportsLightMode = true,
            SupportsDarkMode = true // Theme entity doesn't have mode properties, assume both supported
        };
    }
}
