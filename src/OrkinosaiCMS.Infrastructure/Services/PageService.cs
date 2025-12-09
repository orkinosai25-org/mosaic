using Microsoft.EntityFrameworkCore;
using OrkinosaiCMS.Core.Entities.Sites;
using OrkinosaiCMS.Core.Interfaces.Repositories;
using OrkinosaiCMS.Core.Interfaces.Services;
using OrkinosaiCMS.Infrastructure.Data;

namespace OrkinosaiCMS.Infrastructure.Services;

/// <summary>
/// Service implementation for page management operations
/// </summary>
public class PageService : IPageService
{
    private readonly IRepository<Page> _pageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;

    public PageService(
        IRepository<Page> pageRepository,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context)
    {
        _pageRepository = pageRepository;
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<Page?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Pages
            .Include(p => p.PageModules)
            .Include(p => p.Children)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Page?> GetByPathAsync(string path, int siteId, CancellationToken cancellationToken = default)
    {
        return await _pageRepository.FirstOrDefaultAsync(
            p => p.Path == path && p.SiteId == siteId, cancellationToken);
    }

    public async Task<IEnumerable<Page>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _pageRepository.GetAllAsync(cancellationToken);
    }

    public async Task<IEnumerable<Page>> GetBySiteAsync(int siteId, CancellationToken cancellationToken = default)
    {
        return await _pageRepository.FindAsync(p => p.SiteId == siteId, cancellationToken);
    }

    public async Task<IEnumerable<Page>> GetPublishedPagesAsync(int siteId, CancellationToken cancellationToken = default)
    {
        return await _pageRepository.FindAsync(
            p => p.SiteId == siteId && p.IsPublished, cancellationToken);
    }

    public async Task<IEnumerable<Page>> GetDraftPagesAsync(int siteId, CancellationToken cancellationToken = default)
    {
        return await _pageRepository.FindAsync(
            p => p.SiteId == siteId && !p.IsPublished, cancellationToken);
    }

    public async Task<IEnumerable<Page>> GetChildPagesAsync(int parentId, CancellationToken cancellationToken = default)
    {
        return await _pageRepository.FindAsync(p => p.ParentId == parentId, cancellationToken);
    }

    public async Task<Page> CreateAsync(Page page, CancellationToken cancellationToken = default)
    {
        page.CreatedOn = DateTime.UtcNow;
        await _pageRepository.AddAsync(page, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return page;
    }

    public async Task<Page> UpdateAsync(Page page, CancellationToken cancellationToken = default)
    {
        page.ModifiedOn = DateTime.UtcNow;
        _pageRepository.Update(page);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return page;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var page = await _pageRepository.GetByIdAsync(id, cancellationToken);
        if (page != null)
        {
            _pageRepository.Remove(page);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task PublishAsync(int id, CancellationToken cancellationToken = default)
    {
        var page = await _pageRepository.GetByIdAsync(id, cancellationToken);
        if (page == null)
        {
            throw new ArgumentException($"Page with ID {id} not found.");
        }

        page.IsPublished = true;
        page.ModifiedOn = DateTime.UtcNow;

        _pageRepository.Update(page);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UnpublishAsync(int id, CancellationToken cancellationToken = default)
    {
        var page = await _pageRepository.GetByIdAsync(id, cancellationToken);
        if (page == null)
        {
            throw new ArgumentException($"Page with ID {id} not found.");
        }

        page.IsPublished = false;
        page.ModifiedOn = DateTime.UtcNow;

        _pageRepository.Update(page);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ReorderAsync(int id, int newOrder, CancellationToken cancellationToken = default)
    {
        var page = await _pageRepository.GetByIdAsync(id, cancellationToken);
        if (page == null)
        {
            throw new ArgumentException($"Page with ID {id} not found.");
        }

        page.Order = newOrder;
        page.ModifiedOn = DateTime.UtcNow;

        _pageRepository.Update(page);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task MoveAsync(int id, int? newParentId, CancellationToken cancellationToken = default)
    {
        var page = await _pageRepository.GetByIdAsync(id, cancellationToken);
        if (page == null)
        {
            throw new ArgumentException($"Page with ID {id} not found.");
        }

        // Validate that we're not creating a circular reference
        if (newParentId.HasValue)
        {
            var newParent = await _pageRepository.GetByIdAsync(newParentId.Value, cancellationToken);
            if (newParent == null)
            {
                throw new ArgumentException($"Parent page with ID {newParentId} not found.");
            }

            // Check for circular reference
            if (await IsDescendantOf(newParentId.Value, id, cancellationToken))
            {
                throw new InvalidOperationException("Cannot move page to one of its descendants.");
            }
        }

        page.ParentId = newParentId;
        page.ModifiedOn = DateTime.UtcNow;

        _pageRepository.Update(page);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<bool> IsDescendantOf(int potentialDescendantId, int ancestorId, CancellationToken cancellationToken)
    {
        var page = await _pageRepository.GetByIdAsync(potentialDescendantId, cancellationToken);
        
        while (page?.ParentId != null)
        {
            if (page.ParentId == ancestorId)
            {
                return true;
            }
            page = await _pageRepository.GetByIdAsync(page.ParentId.Value, cancellationToken);
        }

        return false;
    }
}
