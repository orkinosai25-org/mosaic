using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OrkinosaiCMS.Core.Entities.Sites;
using OrkinosaiCMS.Core.Interfaces.Services;
using OrkinosaiCMS.Shared.DTOs;

namespace OrkinosaiCMS.Web.Controllers;

/// <summary>
/// API Controller for site management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SiteController : ControllerBase
{
    private readonly ISiteService _siteService;
    private readonly IThemeService _themeService;
    private readonly ILogger<SiteController> _logger;
    private readonly IConfiguration _configuration;
    private readonly bool _showDetailedErrors;
    private readonly bool _includeStackTrace;

    public SiteController(
        ISiteService siteService,
        IThemeService themeService,
        ILogger<SiteController> logger,
        IConfiguration configuration)
    {
        _siteService = siteService;
        _themeService = themeService;
        _logger = logger;
        _configuration = configuration;
        _showDetailedErrors = _configuration.GetValue<bool>("ErrorHandling:ShowDetailedErrors", false);
        _includeStackTrace = _configuration.GetValue<bool>("ErrorHandling:IncludeStackTrace", false);
    }

    /// <summary>
    /// Get all sites for the current user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SiteDto>), 200)]
    public async Task<IActionResult> GetUserSites([FromQuery] string? userEmail)
    {
        try
        {
            IEnumerable<Site> sites;

            // Require userEmail parameter for security
            if (string.IsNullOrEmpty(userEmail))
            {
                return BadRequest(new { message = "userEmail parameter is required" });
            }

            sites = await _siteService.GetSitesByUserAsync(userEmail);

            var siteDtos = sites.Select(s => new SiteDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Url = s.Url,
                ThemeId = s.ThemeId,
                LogoUrl = s.LogoUrl,
                FaviconUrl = s.FaviconUrl,
                AdminEmail = s.AdminEmail,
                IsActive = s.IsActive,
                DefaultLanguage = s.DefaultLanguage,
                CreatedOn = s.CreatedOn,
                ModifiedOn = s.ModifiedOn,
                Status = s.IsActive ? "active" : "inactive"
            });

            return Ok(siteDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user sites");
            return StatusCode(500, new { message = "Error retrieving sites" });
        }
    }

    /// <summary>
    /// Get a specific site by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SiteDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSiteById(int id)
    {
        try
        {
            var site = await _siteService.GetSiteByIdAsync(id);
            if (site == null)
            {
                return NotFound(new { message = $"Site with ID {id} not found" });
            }

            var siteDto = new SiteDto
            {
                Id = site.Id,
                Name = site.Name,
                Description = site.Description,
                Url = site.Url,
                ThemeId = site.ThemeId,
                LogoUrl = site.LogoUrl,
                FaviconUrl = site.FaviconUrl,
                AdminEmail = site.AdminEmail,
                IsActive = site.IsActive,
                DefaultLanguage = site.DefaultLanguage,
                CreatedOn = site.CreatedOn,
                ModifiedOn = site.ModifiedOn,
                Status = site.IsActive ? "active" : "inactive"
            };

            return Ok(siteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting site by ID {SiteId}", id);
            return StatusCode(500, new { message = "Error retrieving site" });
        }
    }

    /// <summary>
    /// Create and provision a new site
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SiteProvisioningResultDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateSite([FromBody] CreateSiteDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new { message = "Site name is required" });
            }

            if (string.IsNullOrWhiteSpace(dto.AdminEmail))
            {
                return BadRequest(new { message = "Admin email is required" });
            }

            _logger.LogInformation("Creating new site: {SiteName} for user: {AdminEmail}", dto.Name, dto.AdminEmail);

            // Provision the site with initial setup
            var site = await _siteService.ProvisionSiteAsync(
                dto.Name,
                dto.AdminEmail,
                dto.Description,
                dto.ThemeId
            );

            _logger.LogInformation("Site created successfully with ID: {SiteId}", site.Id);

            // Get theme name if theme was selected
            string? themeName = null;
            if (site.ThemeId.HasValue)
            {
                var theme = await _themeService.GetThemeByIdAsync(site.ThemeId.Value);
                themeName = theme?.Name;
            }

            var siteDto = new SiteDto
            {
                Id = site.Id,
                Name = site.Name,
                Description = site.Description,
                Url = site.Url,
                ThemeId = site.ThemeId,
                ThemeName = themeName,
                LogoUrl = site.LogoUrl,
                FaviconUrl = site.FaviconUrl,
                AdminEmail = site.AdminEmail,
                IsActive = site.IsActive,
                DefaultLanguage = site.DefaultLanguage,
                CreatedOn = site.CreatedOn,
                ModifiedOn = site.ModifiedOn,
                Status = "active"
            };

            var result = new SiteProvisioningResultDto
            {
                Success = true,
                Message = $"Site '{site.Name}' created successfully!",
                Site = siteDto,
                CmsDashboardUrl = $"/admin?site={site.Id}"
            };

            return CreatedAtAction(nameof(GetSiteById), new { id = site.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when creating site: {SiteName}, AdminEmail: {AdminEmail}, ThemeId: {ThemeId}", 
                dto.Name, dto.AdminEmail, dto.ThemeId);
            
            return BadRequest(new SiteProvisioningResultDto
            {
                Success = false,
                Message = "Failed to create site",
                ErrorDetails = _showDetailedErrors ? ex.Message : "An error occurred during site creation",
                StackTrace = _includeStackTrace ? ex.StackTrace : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating site: {SiteName}, AdminEmail: {AdminEmail}, ThemeId: {ThemeId}", 
                dto.Name, dto.AdminEmail, dto.ThemeId);
            
            return StatusCode(500, new SiteProvisioningResultDto
            {
                Success = false,
                Message = "An unexpected error occurred while creating the site",
                ErrorDetails = _showDetailedErrors ? ex.Message : "Please contact support if this issue persists",
                StackTrace = _includeStackTrace ? ex.StackTrace : null
            });
        }
    }

    /// <summary>
    /// Update an existing site
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SiteDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateSite(int id, [FromBody] UpdateSiteDto dto)
    {
        try
        {
            if (id != dto.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            var existing = await _siteService.GetSiteByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = $"Site with ID {id} not found" });
            }

            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.Url = dto.Url;
            existing.ThemeId = dto.ThemeId;
            existing.LogoUrl = dto.LogoUrl;
            existing.FaviconUrl = dto.FaviconUrl;
            existing.DefaultLanguage = dto.DefaultLanguage;

            var updated = await _siteService.UpdateSiteAsync(existing);

            var siteDto = new SiteDto
            {
                Id = updated.Id,
                Name = updated.Name,
                Description = updated.Description,
                Url = updated.Url,
                ThemeId = updated.ThemeId,
                LogoUrl = updated.LogoUrl,
                FaviconUrl = updated.FaviconUrl,
                AdminEmail = updated.AdminEmail,
                IsActive = updated.IsActive,
                DefaultLanguage = updated.DefaultLanguage,
                CreatedOn = updated.CreatedOn,
                ModifiedOn = updated.ModifiedOn,
                Status = updated.IsActive ? "active" : "inactive"
            };

            return Ok(siteDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when updating site");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating site {SiteId}", id);
            return StatusCode(500, new { message = "Error updating site" });
        }
    }

    /// <summary>
    /// Delete a site (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteSite(int id)
    {
        try
        {
            var site = await _siteService.GetSiteByIdAsync(id);
            if (site == null)
            {
                return NotFound(new { message = $"Site with ID {id} not found" });
            }

            await _siteService.DeleteSiteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting site {SiteId}", id);
            return StatusCode(500, new { message = "Error deleting site" });
        }
    }

    /// <summary>
    /// Check if a site URL is available
    /// </summary>
    [HttpGet("check-url")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> CheckUrlAvailability([FromQuery] string url)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest(new { message = "URL is required" });
            }

            var isAvailable = await _siteService.IsSiteUrlAvailableAsync(url);
            return Ok(new { url, isAvailable });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking URL availability");
            return StatusCode(500, new { message = "Error checking URL availability" });
        }
    }
}
