using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrkinosaiCMS.Core.Entities.Sites;
using OrkinosaiCMS.Core.Interfaces.Repositories;
using OrkinosaiCMS.Core.Interfaces.Services;

namespace OrkinosaiCMS.Infrastructure.Services;

/// <summary>
/// Service for managing CMS themes
/// Based on Oqtane CMS error handling patterns for robust database operations
/// </summary>
public class ThemeService : IThemeService
{
    private readonly IRepository<Theme> _themeRepository;
    private readonly IRepository<Site> _siteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ThemeService> _logger;

    public ThemeService(
        IRepository<Theme> themeRepository,
        IRepository<Site> siteRepository,
        IUnitOfWork unitOfWork,
        ILogger<ThemeService> logger)
    {
        _themeRepository = themeRepository;
        _siteRepository = siteRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<Theme>> GetAllThemesAsync()
    {
        return await _themeRepository.GetAllAsync();
    }

    public async Task<IEnumerable<Theme>> GetEnabledThemesAsync()
    {
        return await _themeRepository.FindAsync(t => t.IsEnabled);
    }

    public async Task<Theme?> GetThemeByIdAsync(int id)
    {
        return await _themeRepository.GetByIdAsync(id);
    }

    public async Task<Theme?> GetThemeByNameAsync(string name)
    {
        var themes = await _themeRepository.FindAsync(t => t.Name == name);
        return themes.FirstOrDefault();
    }

    public async Task<Theme?> GetActiveSiteThemeAsync(int siteId)
    {
        var site = await _siteRepository.GetByIdAsync(siteId);
        if (site?.ThemeId == null)
            return null;

        return await _themeRepository.GetByIdAsync(site.ThemeId.Value);
    }

    public async Task<Theme> CreateThemeAsync(Theme theme)
    {
        theme.CreatedOn = DateTime.UtcNow;
        await _themeRepository.AddAsync(theme);
        await _unitOfWork.SaveChangesAsync();
        return theme;
    }

    public async Task<Theme> UpdateThemeAsync(Theme theme)
    {
        theme.ModifiedOn = DateTime.UtcNow;
        _themeRepository.Update(theme);
        await _unitOfWork.SaveChangesAsync();
        return theme;
    }

    public async Task DeleteThemeAsync(int id)
    {
        var theme = await _themeRepository.GetByIdAsync(id);
        if (theme != null && !theme.IsSystem)
        {
            _themeRepository.Remove(theme);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Apply theme to a site with robust error handling
    /// Following Oqtane CMS patterns for database exception handling
    /// </summary>
    public async Task ApplyThemeToSiteAsync(int siteId, int themeId)
    {
        try
        {
            _logger.LogInformation("Attempting to apply theme {ThemeId} to site {SiteId}", themeId, siteId);

            // Validate site exists
            var site = await _siteRepository.GetByIdAsync(siteId);
            if (site == null)
            {
                var errorMsg = $"Site with ID {siteId} not found. Cannot apply theme.";
                _logger.LogError(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }

            // Validate theme exists
            var theme = await _themeRepository.GetByIdAsync(themeId);
            if (theme == null)
            {
                var errorMsg = $"Theme with ID {themeId} not found.";
                _logger.LogError(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }

            // Validate theme is enabled
            if (!theme.IsEnabled)
            {
                var errorMsg = $"Theme '{theme.Name}' is disabled and cannot be applied.";
                _logger.LogWarning(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }

            // Apply theme to site
            site.ThemeId = themeId;
            site.ModifiedOn = DateTime.UtcNow;
            _siteRepository.Update(site);
            
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Successfully applied theme '{ThemeName}' (ID: {ThemeId}) to site '{SiteName}' (ID: {SiteId})", 
                theme.Name, themeId, site.Name, siteId);
        }
        catch (SqlException sqlEx)
        {
            // Handle SQL-specific exceptions (following Oqtane pattern)
            _logger.LogError(sqlEx, 
                "SQL error applying theme {ThemeId} to site {SiteId}. Error Number: {ErrorNumber}, State: {State}, Server: {Server}",
                themeId, siteId, sqlEx.Number, sqlEx.State, sqlEx.Server);
            
            throw new InvalidOperationException(
                $"Unable to apply theme due to a database error. Please try again or contact support if the issue persists.",
                sqlEx);
        }
        catch (DbUpdateException dbEx)
        {
            // Handle EF Core update exceptions
            _logger.LogError(dbEx, 
                "Database update error applying theme {ThemeId} to site {SiteId}",
                themeId, siteId);
            
            throw new InvalidOperationException(
                $"Unable to save theme changes to the database. Please try again or contact support if the issue persists.",
                dbEx);
        }
        catch (InvalidOperationException)
        {
            // Re-throw validation errors as-is
            throw;
        }
        catch (Exception ex)
        {
            // Handle any other unexpected exceptions
            _logger.LogError(ex, 
                "Unexpected error applying theme {ThemeId} to site {SiteId}",
                themeId, siteId);
            
            throw new InvalidOperationException(
                $"An unexpected error occurred while applying the theme. Please try again or contact support if the issue persists.",
                ex);
        }
    }

    public async Task<Theme> UpdateThemeBrandingAsync(int themeId, string? primaryColor, string? secondaryColor, string? accentColor, string? logoUrl)
    {
        var theme = await _themeRepository.GetByIdAsync(themeId);
        if (theme == null)
            throw new InvalidOperationException($"Theme with ID {themeId} not found");

        if (theme.IsSystem)
            throw new InvalidOperationException("Cannot modify system themes. Clone the theme first.");

        if (!string.IsNullOrWhiteSpace(primaryColor))
            theme.PrimaryColor = primaryColor;
        
        if (!string.IsNullOrWhiteSpace(secondaryColor))
            theme.SecondaryColor = secondaryColor;
        
        if (!string.IsNullOrWhiteSpace(accentColor))
            theme.AccentColor = accentColor;
        
        if (!string.IsNullOrWhiteSpace(logoUrl))
            theme.LogoUrl = logoUrl;

        theme.ModifiedOn = DateTime.UtcNow;
        _themeRepository.Update(theme);
        await _unitOfWork.SaveChangesAsync();

        return theme;
    }

    public async Task<Theme> CloneThemeAsync(int themeId, string newName, string? newDescription = null)
    {
        var originalTheme = await _themeRepository.GetByIdAsync(themeId);
        if (originalTheme == null)
            throw new InvalidOperationException($"Theme with ID {themeId} not found");

        var clonedTheme = new Theme
        {
            Name = newName,
            Description = newDescription ?? $"Cloned from {originalTheme.Name}",
            Version = "1.0.0",
            Author = originalTheme.Author,
            AssetsPath = originalTheme.AssetsPath,
            ThumbnailUrl = originalTheme.ThumbnailUrl,
            IsEnabled = true,
            IsSystem = false,
            DefaultSettings = originalTheme.DefaultSettings,
            Category = originalTheme.Category,
            LayoutType = originalTheme.LayoutType,
            PrimaryColor = originalTheme.PrimaryColor,
            SecondaryColor = originalTheme.SecondaryColor,
            AccentColor = originalTheme.AccentColor,
            LogoUrl = originalTheme.LogoUrl,
            CustomCss = originalTheme.CustomCss,
            IsMobileResponsive = originalTheme.IsMobileResponsive,
            SharePointThemeJson = originalTheme.SharePointThemeJson,
            CreatedOn = DateTime.UtcNow
        };

        await _themeRepository.AddAsync(clonedTheme);
        await _unitOfWork.SaveChangesAsync();

        return clonedTheme;
    }

    public async Task<IEnumerable<Theme>> GetThemesByCategoryAsync(string category)
    {
        return await _themeRepository.FindAsync(t => t.Category == category && t.IsEnabled);
    }

    public async Task<IEnumerable<Theme>> GetThemesByLayoutTypeAsync(string layoutType)
    {
        return await _themeRepository.FindAsync(t => t.LayoutType == layoutType && t.IsEnabled);
    }
}
