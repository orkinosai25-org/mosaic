using OrkinosaiCMS.Core.Entities.Sites;

namespace OrkinosaiCMS.Core.Interfaces.Services;

/// <summary>
/// Service interface for page management operations
/// </summary>
public interface IPageService
{
    /// <summary>
    /// Get page by ID
    /// </summary>
    Task<Page?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get page by path
    /// </summary>
    Task<Page?> GetByPathAsync(string path, int siteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all pages
    /// </summary>
    Task<IEnumerable<Page>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pages by site
    /// </summary>
    Task<IEnumerable<Page>> GetBySiteAsync(int siteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get published pages
    /// </summary>
    Task<IEnumerable<Page>> GetPublishedPagesAsync(int siteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get draft pages
    /// </summary>
    Task<IEnumerable<Page>> GetDraftPagesAsync(int siteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get child pages
    /// </summary>
    Task<IEnumerable<Page>> GetChildPagesAsync(int parentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new page
    /// </summary>
    Task<Page> CreateAsync(Page page, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing page
    /// </summary>
    Task<Page> UpdateAsync(Page page, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a page (soft delete)
    /// </summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publish a page
    /// </summary>
    Task PublishAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unpublish a page (set to draft)
    /// </summary>
    Task UnpublishAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reorder pages
    /// </summary>
    Task ReorderAsync(int id, int newOrder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Move page to different parent
    /// </summary>
    Task MoveAsync(int id, int? newParentId, CancellationToken cancellationToken = default);
}
