using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrkinosaiCMS.Core.Entities.Sites;
using OrkinosaiCMS.Core.Interfaces.Services;
using OrkinosaiCMS.Shared.DTOs;

namespace OrkinosaiCMS.Web.Controllers;

/// <summary>
/// API Controller for theme management, used by Zoota agent and admin UI
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class ThemeController : ControllerBase
{
    private readonly IThemeService _themeService;
    private readonly ILogger<ThemeController> _logger;

    public ThemeController(IThemeService themeService, ILogger<ThemeController> logger)
    {
        _themeService = themeService;
        _logger = logger;
    }

    /// <summary>
    /// Get all themes
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Theme>), 200)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var themes = await _themeService.GetAllThemesAsync();
            return Ok(themes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all themes");
            return StatusCode(500, new { message = "Error retrieving themes" });
        }
    }

    /// <summary>
    /// Get enabled themes only
    /// </summary>
    [HttpGet("enabled")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<Theme>), 200)]
    public async Task<IActionResult> GetEnabled()
    {
        try
        {
            var themes = await _themeService.GetEnabledThemesAsync();
            return Ok(themes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting enabled themes");
            return StatusCode(500, new { message = "Error retrieving enabled themes" });
        }
    }

    /// <summary>
    /// Get theme by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Theme), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var theme = await _themeService.GetThemeByIdAsync(id);
            if (theme == null)
                return NotFound(new { message = $"Theme with ID {id} not found" });

            return Ok(theme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting theme by ID {ThemeId}", id);
            return StatusCode(500, new { message = "Error retrieving theme" });
        }
    }

    /// <summary>
    /// Get themes by category
    /// </summary>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(IEnumerable<Theme>), 200)]
    public async Task<IActionResult> GetByCategory(string category)
    {
        try
        {
            var themes = await _themeService.GetThemesByCategoryAsync(category);
            return Ok(themes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting themes by category {Category}", category);
            return StatusCode(500, new { message = "Error retrieving themes by category" });
        }
    }

    /// <summary>
    /// Get themes by layout type
    /// </summary>
    [HttpGet("layout/{layoutType}")]
    [ProducesResponseType(typeof(IEnumerable<Theme>), 200)]
    public async Task<IActionResult> GetByLayoutType(string layoutType)
    {
        try
        {
            var themes = await _themeService.GetThemesByLayoutTypeAsync(layoutType);
            return Ok(themes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting themes by layout type {LayoutType}", layoutType);
            return StatusCode(500, new { message = "Error retrieving themes by layout type" });
        }
    }

    /// <summary>
    /// Get active theme for a site
    /// </summary>
    [HttpGet("active/{siteId}")]
    [ProducesResponseType(typeof(Theme), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetActiveSiteTheme(int siteId)
    {
        try
        {
            var theme = await _themeService.GetActiveSiteThemeAsync(siteId);
            if (theme == null)
                return NotFound(new { message = $"No active theme found for site {siteId}" });

            return Ok(theme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active theme for site {SiteId}", siteId);
            return StatusCode(500, new { message = "Error retrieving active theme" });
        }
    }

    /// <summary>
    /// Create a new theme
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Theme), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateThemeDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Theme name is required" });

            var theme = new Theme
            {
                Name = dto.Name,
                Description = dto.Description,
                Category = dto.Category,
                LayoutType = dto.LayoutType,
                PrimaryColor = dto.PrimaryColor,
                SecondaryColor = dto.SecondaryColor,
                AccentColor = dto.AccentColor,
                LogoUrl = dto.LogoUrl,
                IsEnabled = true,
                IsSystem = false,
                IsMobileResponsive = true,
                AssetsPath = $"/css/themes/{GenerateSafeFileName(dto.Name)}-theme.css"
            };

            var created = await _themeService.CreateThemeAsync(theme);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating theme");
            return StatusCode(500, new { message = "Error creating theme" });
        }
    }

    /// <summary>
    /// Generate a safe filename from a theme name
    /// </summary>
    private static string GenerateSafeFileName(string themeName)
    {
        // Convert to lowercase and replace spaces with hyphens
        var slug = themeName.ToLowerInvariant().Replace(" ", "-");
        
        // Remove invalid filename characters
        var invalidChars = Path.GetInvalidFileNameChars();
        slug = string.Concat(slug.Where(c => !invalidChars.Contains(c)));
        
        // Remove multiple consecutive hyphens
        while (slug.Contains("--"))
            slug = slug.Replace("--", "-");
        
        // Trim hyphens from start and end
        slug = slug.Trim('-');
        
        // Ensure it's not empty
        if (string.IsNullOrWhiteSpace(slug))
            slug = "custom-theme";
        
        return slug;
    }

    /// <summary>
    /// Update theme branding (colors, logo)
    /// </summary>
    [HttpPut("{id}/branding")]
    [ProducesResponseType(typeof(Theme), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateBranding(int id, [FromBody] UpdateThemeBrandingDto dto)
    {
        try
        {
            var theme = await _themeService.UpdateThemeBrandingAsync(
                id,
                dto.PrimaryColor,
                dto.SecondaryColor,
                dto.AccentColor,
                dto.LogoUrl
            );

            return Ok(theme);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when updating theme branding");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating theme branding for theme {ThemeId}", id);
            return StatusCode(500, new { message = "Error updating theme branding" });
        }
    }

    /// <summary>
    /// Clone a theme to create a custom version
    /// </summary>
    [HttpPost("clone")]
    [ProducesResponseType(typeof(Theme), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Clone([FromBody] CloneThemeDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.NewName))
                return BadRequest(new { message = "New theme name is required" });

            var cloned = await _themeService.CloneThemeAsync(
                dto.SourceThemeId,
                dto.NewName,
                dto.NewDescription
            );

            return CreatedAtAction(nameof(GetById), new { id = cloned.Id }, cloned);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when cloning theme");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cloning theme");
            return StatusCode(500, new { message = "Error cloning theme" });
        }
    }

    /// <summary>
    /// Apply theme to a site
    /// </summary>
    [HttpPost("apply")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ApplyToSite([FromBody] ApplyThemeDto dto)
    {
        try
        {
            await _themeService.ApplyThemeToSiteAsync(dto.SiteId, dto.ThemeId);
            return Ok(new { message = $"Theme {dto.ThemeId} applied to site {dto.SiteId}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying theme to site");
            return StatusCode(500, new { message = "Error applying theme to site" });
        }
    }

    /// <summary>
    /// Delete a theme (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var theme = await _themeService.GetThemeByIdAsync(id);
            if (theme == null)
                return NotFound(new { message = $"Theme with ID {id} not found" });

            if (theme.IsSystem)
                return BadRequest(new { message = "Cannot delete system themes" });

            await _themeService.DeleteThemeAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting theme {ThemeId}", id);
            return StatusCode(500, new { message = "Error deleting theme" });
        }
    }

    /// <summary>
    /// Upload a theme package (zip file containing theme assets and metadata)
    /// </summary>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(Theme), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UploadThemePackage(IFormFile file, [FromForm] string? themeName, [FromForm] string? description)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded" });

            // Validate file type (should be .zip)
            if (!file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Only .zip files are supported" });

            // Validate file size (max 10MB)
            if (file.Length > 10 * 1024 * 1024)
                return BadRequest(new { message = "File size exceeds 10MB limit" });

            // For now, return a not implemented response
            // Full implementation would extract the zip, validate contents, and create theme
            return StatusCode(501, new { 
                message = "Theme package upload is not yet implemented. Please use the custom theme creator instead.",
                hint = "Use the 'Create Custom Theme' button to create a new theme with colors and settings."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading theme package");
            return StatusCode(500, new { message = "Error uploading theme package" });
        }
    }
}
