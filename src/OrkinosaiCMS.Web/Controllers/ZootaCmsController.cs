using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrkinosaiCMS.Core.Entities.Sites;
using OrkinosaiCMS.Core.Interfaces.Services;

namespace OrkinosaiCMS.Web.Controllers;

/// <summary>
/// API controller for Zoota AI Assistant CMS operations
/// Admin-only endpoints for managing CMS content
/// </summary>
[ApiController]
[Route("api/zoota/cms")]
[Authorize(Roles = "Administrator")]
public class ZootaCmsController : ControllerBase
{
    private readonly IPageService _pageService;
    private readonly IContentService _contentService;
    private readonly IUserService _userService;
    private readonly ILogger<ZootaCmsController> _logger;

    public ZootaCmsController(
        IPageService pageService,
        IContentService contentService,
        IUserService userService,
        ILogger<ZootaCmsController> logger)
    {
        _pageService = pageService;
        _contentService = contentService;
        _userService = userService;
        _logger = logger;
    }

    #region Pages

    /// <summary>
    /// List all pages
    /// </summary>
    [HttpGet("pages")]
    public async Task<IActionResult> GetPages()
    {
        try
        {
            var pages = await _pageService.GetAllAsync();
            return Ok(new
            {
                success = true,
                data = pages.Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Path,
                    p.IsPublished,
                    p.CreatedOn,
                    p.ModifiedOn
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pages");
            return StatusCode(500, new { success = false, message = "Error retrieving pages" });
        }
    }

    /// <summary>
    /// Create a new page
    /// </summary>
    [HttpPost("pages")]
    public async Task<IActionResult> CreatePage([FromBody] CreatePageRequest request)
    {
        try
        {
            var page = new Page
            {
                Title = request.Title,
                Path = request.Path ?? GenerateSlug(request.Title),
                MetaDescription = request.MetaDescription,
                IsPublished = request.IsPublished ?? false,
                SiteId = 1, // Default site
                CreatedBy = User.Identity?.Name ?? "admin",
                CreatedOn = DateTime.UtcNow
            };

            var created = await _pageService.CreateAsync(page);
            
            return Ok(new
            {
                success = true,
                message = $"Page '{created.Title}' created successfully",
                data = new { created.Id, created.Title, created.Path }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating page");
            return StatusCode(500, new { success = false, message = "Error creating page" });
        }
    }

    /// <summary>
    /// Update an existing page
    /// </summary>
    [HttpPut("pages/{id}")]
    public async Task<IActionResult> UpdatePage(int id, [FromBody] UpdatePageRequest request)
    {
        try
        {
            var page = await _pageService.GetByIdAsync(id);
            if (page == null)
            {
                return NotFound(new { success = false, message = "Page not found" });
            }

            if (!string.IsNullOrEmpty(request.Title))
                page.Title = request.Title;
            
            if (!string.IsNullOrEmpty(request.Path))
                page.Path = request.Path;
            
            if (request.MetaDescription != null)
                page.MetaDescription = request.MetaDescription;
            
            if (request.IsPublished.HasValue)
                page.IsPublished = request.IsPublished.Value;

            page.ModifiedBy = User.Identity?.Name ?? "admin";
            page.ModifiedOn = DateTime.UtcNow;

            await _pageService.UpdateAsync(page);

            return Ok(new
            {
                success = true,
                message = $"Page '{page.Title}' updated successfully",
                data = new { page.Id, page.Title, page.Path }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating page");
            return StatusCode(500, new { success = false, message = "Error updating page" });
        }
    }

    /// <summary>
    /// Delete a page
    /// </summary>
    [HttpDelete("pages/{id}")]
    public async Task<IActionResult> DeletePage(int id)
    {
        try
        {
            var page = await _pageService.GetByIdAsync(id);
            if (page == null)
            {
                return NotFound(new { success = false, message = "Page not found" });
            }

            var title = page.Title;
            await _pageService.DeleteAsync(id);

            return Ok(new
            {
                success = true,
                message = $"Page '{title}' deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting page");
            return StatusCode(500, new { success = false, message = "Error deleting page" });
        }
    }

    #endregion

    #region Content

    /// <summary>
    /// List all content items
    /// </summary>
    [HttpGet("content")]
    public async Task<IActionResult> GetContent()
    {
        try
        {
            var content = await _contentService.GetAllAsync();
            return Ok(new
            {
                success = true,
                data = content.Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.ContentType,
                    c.CreatedOn,
                    c.ModifiedOn
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting content");
            return StatusCode(500, new { success = false, message = "Error retrieving content" });
        }
    }

    /// <summary>
    /// Create new content
    /// </summary>
    [HttpPost("content")]
    public async Task<IActionResult> CreateContent([FromBody] CreateContentRequest request)
    {
        try
        {
            var content = new Content
            {
                Title = request.Title,
                ContentType = request.ContentType ?? "Document",
                Body = request.Body,
                SiteId = 1, // Default site
                CreatedBy = User.Identity?.Name ?? "admin",
                CreatedOn = DateTime.UtcNow
            };

            var created = await _contentService.CreateAsync(content);

            return Ok(new
            {
                success = true,
                message = $"Content '{created.Title}' created successfully",
                data = new { created.Id, created.Title, created.ContentType }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating content");
            return StatusCode(500, new { success = false, message = "Error creating content" });
        }
    }

    /// <summary>
    /// Update existing content
    /// </summary>
    [HttpPut("content/{id}")]
    public async Task<IActionResult> UpdateContent(int id, [FromBody] UpdateContentRequest request)
    {
        try
        {
            var content = await _contentService.GetByIdAsync(id);
            if (content == null)
            {
                return NotFound(new { success = false, message = "Content not found" });
            }

            if (!string.IsNullOrEmpty(request.Title))
                content.Title = request.Title;
            
            if (request.Body != null)
                content.Body = request.Body;

            content.ModifiedBy = User.Identity?.Name ?? "admin";
            content.ModifiedOn = DateTime.UtcNow;

            await _contentService.UpdateAsync(content);

            return Ok(new
            {
                success = true,
                message = $"Content '{content.Title}' updated successfully",
                data = new { content.Id, content.Title }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating content");
            return StatusCode(500, new { success = false, message = "Error updating content" });
        }
    }

    /// <summary>
    /// Delete content
    /// </summary>
    [HttpDelete("content/{id}")]
    public async Task<IActionResult> DeleteContent(int id)
    {
        try
        {
            var content = await _contentService.GetByIdAsync(id);
            if (content == null)
            {
                return NotFound(new { success = false, message = "Content not found" });
            }

            var title = content.Title;
            await _contentService.DeleteAsync(id);

            return Ok(new
            {
                success = true,
                message = $"Content '{title}' deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting content");
            return StatusCode(500, new { success = false, message = "Error deleting content" });
        }
    }

    #endregion

    #region Users

    /// <summary>
    /// List all users (excluding passwords)
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _userService.GetAllAsync();
            return Ok(new
            {
                success = true,
                data = users.Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.DisplayName,
                    u.IsActive,
                    u.CreatedOn,
                    u.LastLoginOn
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, new { success = false, message = "Error retrieving users" });
        }
    }

    #endregion

    #region Helpers

    private string GenerateSlug(string title)
    {
        // Convert to lowercase and remove diacritics
        var slug = title.ToLower().Trim();
        
        // Replace invalid characters with hyphens
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        
        // Replace multiple spaces or hyphens with single hyphen
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[\s-]+", "-");
        
        // Remove leading and trailing hyphens
        slug = slug.Trim('-');
        
        return slug;
    }

    #endregion
}

#region Request Models

public class CreatePageRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Path { get; set; }
    public string? MetaDescription { get; set; }
    public bool? IsPublished { get; set; }
}

public class UpdatePageRequest
{
    public string? Title { get; set; }
    public string? Path { get; set; }
    public string? MetaDescription { get; set; }
    public bool? IsPublished { get; set; }
}

public class CreateContentRequest
{
    public string Title { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public string? Body { get; set; }
}

public class UpdateContentRequest
{
    public string? Title { get; set; }
    public string? Body { get; set; }
}

#endregion
