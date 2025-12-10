using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrkinosaiCMS.Core.Entities.Sites;
using OrkinosaiCMS.Core.Interfaces.Repositories;
using OrkinosaiCMS.Core.Interfaces.Services;

namespace OrkinosaiCMS.Infrastructure.Services;

/// <summary>
/// Service implementation for site management operations
/// </summary>
public class SiteService : ISiteService
{
    private readonly IRepository<Site> _siteRepository;
    private readonly IRepository<Page> _pageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SiteService> _logger;

    public SiteService(
        IRepository<Site> siteRepository,
        IRepository<Page> pageRepository,
        IUnitOfWork unitOfWork,
        ILogger<SiteService> logger)
    {
        _siteRepository = siteRepository;
        _pageRepository = pageRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<Site>> GetAllSitesAsync()
    {
        return await _siteRepository.GetAllAsync();
    }

    public async Task<IEnumerable<Site>> GetSitesByUserAsync(string userEmail)
    {
        return await _siteRepository.FindAsync(s => s.AdminEmail == userEmail && s.IsActive);
    }

    public async Task<Site?> GetSiteByIdAsync(int id)
    {
        return await _siteRepository.GetByIdAsync(id);
    }

    public async Task<Site?> GetSiteByUrlAsync(string url)
    {
        var sites = await _siteRepository.FindAsync(s => s.Url == url);
        return sites.FirstOrDefault();
    }

    public async Task<Site> CreateSiteAsync(Site site)
    {
        // Ensure URL is unique
        if (!await IsSiteUrlAvailableAsync(site.Url))
        {
            throw new InvalidOperationException($"Site URL '{site.Url}' is already in use.");
        }

        site.IsActive = true;
        site.CreatedOn = DateTime.UtcNow;
        site.CreatedBy = site.AdminEmail;

        await _siteRepository.AddAsync(site);
        await _unitOfWork.SaveChangesAsync();

        return site;
    }

    public async Task<Site> UpdateSiteAsync(Site site)
    {
        var existing = await _siteRepository.GetByIdAsync(site.Id);
        if (existing == null)
        {
            throw new InvalidOperationException($"Site with ID {site.Id} not found.");
        }

        // If URL changed, ensure new URL is unique
        if (existing.Url != site.Url && !await IsSiteUrlAvailableAsync(site.Url))
        {
            throw new InvalidOperationException($"Site URL '{site.Url}' is already in use.");
        }

        existing.Name = site.Name;
        existing.Description = site.Description;
        existing.Url = site.Url;
        existing.ThemeId = site.ThemeId;
        existing.LogoUrl = site.LogoUrl;
        existing.FaviconUrl = site.FaviconUrl;
        existing.AdminEmail = site.AdminEmail;
        existing.DefaultLanguage = site.DefaultLanguage;
        existing.ModifiedOn = DateTime.UtcNow;
        existing.ModifiedBy = site.AdminEmail;

        _siteRepository.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        return existing;
    }

    public async Task DeleteSiteAsync(int id)
    {
        var site = await _siteRepository.GetByIdAsync(id);
        if (site == null)
        {
            throw new InvalidOperationException($"Site with ID {id} not found.");
        }

        // Soft delete
        site.IsActive = false;
        site.ModifiedOn = DateTime.UtcNow;

        _siteRepository.Update(site);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Site> ProvisionSiteAsync(string siteName, string adminEmail, string? description, int? themeId)
    {
        _logger.LogInformation("Starting site provisioning: SiteName={SiteName}, AdminEmail={AdminEmail}, ThemeId={ThemeId}", 
            siteName, adminEmail, themeId);

        try
        {
            // Generate a unique URL from the site name
            var baseUrl = GenerateUrlSlug(siteName);
            var url = baseUrl;
            var counter = 1;

            _logger.LogDebug("Generated base URL slug: {BaseUrl}", baseUrl);

            while (!await IsSiteUrlAvailableAsync(url))
            {
                url = $"{baseUrl}-{counter}";
                counter++;
                _logger.LogDebug("URL {BaseUrl} not available, trying {NewUrl}", baseUrl, url);
            }

            _logger.LogInformation("Selected URL for site: {Url}", url);

            var site = new Site
            {
                Name = siteName,
                Description = description,
                Url = url,
                AdminEmail = adminEmail,
                ThemeId = themeId,
                IsActive = true,
                DefaultLanguage = "en-US",
                CreatedOn = DateTime.UtcNow,
                CreatedBy = adminEmail
            };

            await _siteRepository.AddAsync(site);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Site entity created with ID: {SiteId}", site.Id);

            // Initialize default content
            await InitializeSiteContentAsync(site.Id);

            _logger.LogInformation("Site provisioning completed successfully: SiteId={SiteId}, Url={Url}", site.Id, site.Url);

            return site;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to provision site: SiteName={SiteName}, AdminEmail={AdminEmail}", siteName, adminEmail);
            throw;
        }
    }

    public async Task InitializeSiteContentAsync(int siteId)
    {
        _logger.LogInformation("Initializing default content for site: {SiteId}", siteId);

        try
        {
            var site = await _siteRepository.GetByIdAsync(siteId);
            if (site == null)
            {
                _logger.LogError("Site with ID {SiteId} not found during content initialization", siteId);
                throw new InvalidOperationException($"Site with ID {siteId} not found.");
            }

            // Create default home page
            var homePage = new Page
            {
                Title = "Home",
                Path = "/",
                MetaDescription = $"Welcome to {site.Name}",
                IsPublished = true,
                SiteId = siteId,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = site.AdminEmail
            };

            await _pageRepository.AddAsync(homePage);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Default home page created for site: {SiteId}, PageId: {PageId}", siteId, homePage.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize content for site: {SiteId}", siteId);
            throw;
        }
    }

    public async Task<bool> IsSiteUrlAvailableAsync(string url)
    {
        var sites = await _siteRepository.FindAsync(s => s.Url == url && s.IsActive);
        return !sites.Any();
    }

    private static readonly char[] InvalidUrlChars = new[] { 
        '/', '\\', '?', '#', '&', '=', '+', '%', '@', '!', '*', '(', ')', '[', ']', '{', '}', '<', '>', '"', '\'', ':', ';', ',', '.' 
    };

    private string GenerateUrlSlug(string input)
    {
        // Convert to lowercase and replace spaces with hyphens
        var slug = input.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-");

        // Remove invalid URL characters
        foreach (var c in InvalidUrlChars)
        {
            slug = slug.Replace(c.ToString(), "");
        }

        // Remove multiple consecutive hyphens
        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }

        // Trim hyphens from start and end
        slug = slug.Trim('-');

        // Ensure it's not empty
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = "site";
        }

        return slug;
    }
}
