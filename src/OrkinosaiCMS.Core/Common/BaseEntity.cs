namespace OrkinosaiCMS.Core.Common;

/// <summary>
/// Base entity class providing common properties for all entities
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Date and time when the entity was created
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// User who created the entity
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the entity was last modified
    /// </summary>
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// User who last modified the entity
    /// </summary>
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; }
}
