using OrkinosaiCMS.Core.Common;

namespace OrkinosaiCMS.Core.Entities.Sites;

/// <summary>
/// Represents a site in the CMS
/// Similar to SharePoint Site Collection concept
/// </summary>
public class Site : BaseEntity
{
    /// <summary>
    /// Name of the site
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the site
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Site URL or path
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Theme associated with the site
    /// </summary>
    public int? ThemeId { get; set; }

    /// <summary>
    /// Logo URL for the site
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Favicon URL for the site
    /// </summary>
    public string? FaviconUrl { get; set; }

    /// <summary>
    /// Site administrator email
    /// </summary>
    public string AdminEmail { get; set; } = string.Empty;

    /// <summary>
    /// Whether the site is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Default language/culture for the site
    /// </summary>
    public string DefaultLanguage { get; set; } = "en-US";

    /// <summary>
    /// Navigation pages associated with this site
    /// </summary>
    public ICollection<Page> Pages { get; set; } = new List<Page>();
}
