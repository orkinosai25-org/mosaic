using Microsoft.EntityFrameworkCore;
using OrkinosaiCMS.Core.Entities.Sites;
using OrkinosaiCMS.Core.Interfaces.Repositories;
using OrkinosaiCMS.Core.Interfaces.Services;

namespace OrkinosaiCMS.Infrastructure.Services;

/// <summary>
/// Service implementation for content management operations
/// </summary>
public class ContentService : IContentService
{
    private readonly IRepository<Content> _contentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ContentService(
        IRepository<Content> contentRepository,
        IUnitOfWork unitOfWork)
    {
        _contentRepository = contentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Content?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _contentRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Content>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _contentRepository.GetAllAsync(cancellationToken);
    }

    public async Task<IEnumerable<Content>> GetBySiteAsync(int siteId, CancellationToken cancellationToken = default)
    {
        return await _contentRepository.FindAsync(c => c.SiteId == siteId, cancellationToken);
    }

    public async Task<IEnumerable<Content>> GetByTypeAsync(string contentType, CancellationToken cancellationToken = default)
    {
        return await _contentRepository.FindAsync(c => c.ContentType == contentType, cancellationToken);
    }

    public async Task<IEnumerable<Content>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _contentRepository.FindAsync(c => c.Category == category, cancellationToken);
    }

    public async Task<IEnumerable<Content>> GetByTagsAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default)
    {
        // TODO: Optimize for large datasets by implementing database-level tag searching
        // Consider storing tags in a separate table or using JSON query functions
        var allContent = await _contentRepository.GetAllAsync(cancellationToken);
        
        return allContent.Where(c => 
            !string.IsNullOrEmpty(c.Tags) && 
            tags.Any(tag => c.Tags.Contains(tag, StringComparison.OrdinalIgnoreCase))
        );
    }

    public async Task<IEnumerable<Content>> GetPublishedContentAsync(int siteId, CancellationToken cancellationToken = default)
    {
        return await _contentRepository.FindAsync(
            c => c.SiteId == siteId && c.IsPublished, cancellationToken);
    }

    public async Task<IEnumerable<Content>> GetDraftContentAsync(int siteId, CancellationToken cancellationToken = default)
    {
        return await _contentRepository.FindAsync(
            c => c.SiteId == siteId && !c.IsPublished, cancellationToken);
    }

    public async Task<IEnumerable<Content>> GetByAuthorAsync(int authorId, CancellationToken cancellationToken = default)
    {
        return await _contentRepository.FindAsync(c => c.AuthorId == authorId, cancellationToken);
    }

    public async Task<Content> CreateAsync(Content content, CancellationToken cancellationToken = default)
    {
        content.CreatedOn = DateTime.UtcNow;
        await _contentRepository.AddAsync(content, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return content;
    }

    public async Task<Content> UpdateAsync(Content content, CancellationToken cancellationToken = default)
    {
        content.ModifiedOn = DateTime.UtcNow;
        _contentRepository.Update(content);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return content;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var content = await _contentRepository.GetByIdAsync(id, cancellationToken);
        if (content != null)
        {
            _contentRepository.Remove(content);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task PublishAsync(int id, CancellationToken cancellationToken = default)
    {
        var content = await _contentRepository.GetByIdAsync(id, cancellationToken);
        if (content == null)
        {
            throw new ArgumentException($"Content with ID {id} not found.");
        }

        content.IsPublished = true;
        content.PublishedOn = DateTime.UtcNow;
        content.ModifiedOn = DateTime.UtcNow;

        _contentRepository.Update(content);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UnpublishAsync(int id, CancellationToken cancellationToken = default)
    {
        var content = await _contentRepository.GetByIdAsync(id, cancellationToken);
        if (content == null)
        {
            throw new ArgumentException($"Content with ID {id} not found.");
        }

        content.IsPublished = false;
        content.ModifiedOn = DateTime.UtcNow;

        _contentRepository.Update(content);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Content>> SearchAsync(string searchTerm, int siteId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement database-level full-text search for better performance
        // Consider using SQL Server CONTAINS function or adding a dedicated search index
        var allContent = await _contentRepository.FindAsync(c => c.SiteId == siteId, cancellationToken);
        
        return allContent.Where(c =>
            (!string.IsNullOrEmpty(c.Title) && c.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrEmpty(c.Body) && c.Body.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
        );
    }
}
