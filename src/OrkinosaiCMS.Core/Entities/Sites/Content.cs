using OrkinosaiCMS.Core.Common;

namespace OrkinosaiCMS.Core.Entities.Sites;

/// <summary>
/// Represents content entity for documents, media, and custom content types
/// </summary>
public class Content : BaseEntity
{
    /// <summary>
    /// Site this content belongs to
    /// </summary>
    public int SiteId { get; set; }

    /// <summary>
    /// Content title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Content type (Document, Image, Video, Custom, etc.)
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Content body/description
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// File path or URL for media content
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// MIME type for media content
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long? FileSize { get; set; }

    /// <summary>
    /// Content metadata as JSON
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Whether the content is published
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Tags for categorization
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Category or classification
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Author user ID
    /// </summary>
    public int? AuthorId { get; set; }

    /// <summary>
    /// Published date
    /// </summary>
    public DateTime? PublishedOn { get; set; }

    /// <summary>
    /// Navigation to site
    /// </summary>
    public Site? Site { get; set; }
}
