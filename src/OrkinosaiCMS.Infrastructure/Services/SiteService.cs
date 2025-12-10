using Microsoft.EntityFrameworkCore;
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

    public SiteService(
        IRepository<Site> siteRepository,
        IRepository<Page> pageRepository,
        IUnitOfWork unitOfWork)
    {
        _siteRepository = siteRepository;
        _pageRepository = pageRepository;
        _unitOfWork = unitOfWork;
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
        // Generate a unique URL from the site name
        var baseUrl = GenerateUrlSlug(siteName);
        var url = baseUrl;
        var counter = 1;

        while (!await IsSiteUrlAvailableAsync(url))
        {
            url = $"{baseUrl}-{counter}";
            counter++;
        }

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

        // Initialize default content
        await InitializeSiteContentAsync(site.Id);

        return site;
    }

    public async Task InitializeSiteContentAsync(int siteId)
    {
        var site = await _siteRepository.GetByIdAsync(siteId);
        if (site == null)
        {
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
    }

    public async Task<bool> IsSiteUrlAvailableAsync(string url)
    {
        var sites = await _siteRepository.FindAsync(s => s.Url == url);
        return !sites.Any();
    }

    private string GenerateUrlSlug(string input)
    {
        // Convert to lowercase and replace spaces with hyphens
        var slug = input.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-");

        // Remove invalid URL characters
        var invalidChars = new[] { '/', '\\', '?', '#', '&', '=', '+', '%', '@', '!', '*', '(', ')', '[', ']', '{', '}', '<', '>', '"', '\'', ':', ';', ',', '.' };
        foreach (var c in invalidChars)
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
