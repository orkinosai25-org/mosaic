using OrkinosaiCMS.Core.Entities.Sites;

namespace OrkinosaiCMS.Core.Interfaces.Services;

/// <summary>
/// Service interface for site management operations
/// </summary>
public interface ISiteService
{
    /// <summary>
    /// Get all sites
    /// </summary>
    Task<IEnumerable<Site>> GetAllSitesAsync();

    /// <summary>
    /// Get sites by user email
    /// </summary>
    Task<IEnumerable<Site>> GetSitesByUserAsync(string userEmail);

    /// <summary>
    /// Get a site by ID
    /// </summary>
    Task<Site?> GetSiteByIdAsync(int id);

    /// <summary>
    /// Get a site by URL
    /// </summary>
    Task<Site?> GetSiteByUrlAsync(string url);

    /// <summary>
    /// Create a new site
    /// </summary>
    Task<Site> CreateSiteAsync(Site site);

    /// <summary>
    /// Update an existing site
    /// </summary>
    Task<Site> UpdateSiteAsync(Site site);

    /// <summary>
    /// Delete a site (soft delete)
    /// </summary>
    Task DeleteSiteAsync(int id);

    /// <summary>
    /// Provision a new site with initial setup
    /// </summary>
    Task<Site> ProvisionSiteAsync(string siteName, string adminEmail, string? description, int? themeId);

    /// <summary>
    /// Initialize site with default content
    /// </summary>
    Task InitializeSiteContentAsync(int siteId);

    /// <summary>
    /// Check if a site URL is available
    /// </summary>
    Task<bool> IsSiteUrlAvailableAsync(string url);
}
