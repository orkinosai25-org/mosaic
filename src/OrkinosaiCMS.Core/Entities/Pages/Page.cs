using OrkinosaiCMS.Core.Common;

namespace OrkinosaiCMS.Core.Entities.Sites;

/// <summary>
/// Represents a page in the CMS
/// Inspired by SharePoint Application Pages concept
/// </summary>
public class Page : BaseEntity
{
    /// <summary>
    /// Site this page belongs to
    /// </summary>
    public int SiteId { get; set; }

    /// <summary>
    /// Parent page ID for hierarchical navigation
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// Page title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Page URL path (relative to site)
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Page content/body
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Master page/layout template to use
    /// </summary>
    public int? MasterPageId { get; set; }

    /// <summary>
    /// Display order for navigation
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Whether the page is published
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Whether the page appears in navigation
    /// </summary>
    public bool ShowInNavigation { get; set; } = true;

    /// <summary>
    /// SEO meta description
    /// </summary>
    public string? MetaDescription { get; set; }

    /// <summary>
    /// SEO meta keywords
    /// </summary>
    public string? MetaKeywords { get; set; }

    /// <summary>
    /// Icon CSS class for navigation
    /// </summary>
    public string? IconCssClass { get; set; }

    /// <summary>
    /// Permission level required to view this page
    /// </summary>
    public string? RequiredPermission { get; set; }

    /// <summary>
    /// Navigation site
    /// </summary>
    public Site? Site { get; set; }

    /// <summary>
    /// Parent page for hierarchical navigation
    /// </summary>
    public Page? Parent { get; set; }

    /// <summary>
    /// Child pages
    /// </summary>
    public ICollection<Page> Children { get; set; } = new List<Page>();

    /// <summary>
    /// Modules/web parts on this page
    /// </summary>
    public ICollection<PageModule> PageModules { get; set; } = new List<PageModule>();
}
