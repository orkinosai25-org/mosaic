using OrkinosaiCMS.Core.Entities.Sites;

namespace OrkinosaiCMS.Core.Interfaces.Services;

/// <summary>
/// Service interface for content management operations
/// </summary>
public interface IContentService
{
    /// <summary>
    /// Get content by ID
    /// </summary>
    Task<Content?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all content
    /// </summary>
    Task<IEnumerable<Content>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get content by site
    /// </summary>
    Task<IEnumerable<Content>> GetBySiteAsync(int siteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get content by type
    /// </summary>
    Task<IEnumerable<Content>> GetByTypeAsync(string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get content by category
    /// </summary>
    Task<IEnumerable<Content>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get content by tags
    /// </summary>
    Task<IEnumerable<Content>> GetByTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get published content
    /// </summary>
    Task<IEnumerable<Content>> GetPublishedContentAsync(int siteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get draft content
    /// </summary>
    Task<IEnumerable<Content>> GetDraftContentAsync(int siteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get content by author
    /// </summary>
    Task<IEnumerable<Content>> GetByAuthorAsync(int authorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create new content
    /// </summary>
    Task<Content> CreateAsync(Content content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing content
    /// </summary>
    Task<Content> UpdateAsync(Content content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete content (soft delete)
    /// </summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publish content
    /// </summary>
    Task PublishAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unpublish content (set to draft)
    /// </summary>
    Task UnpublishAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search content by title or body
    /// </summary>
    Task<IEnumerable<Content>> SearchAsync(string searchTerm, int siteId, CancellationToken cancellationToken = default);
}
