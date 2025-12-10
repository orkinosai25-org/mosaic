using OrkinosaiCMS.Core.Common;

namespace OrkinosaiCMS.Core.Entities.Sites;

/// <summary>
/// Represents a permission in the CMS
/// Fine-grained permissions similar to SharePoint
/// </summary>
public class Permission : BaseEntity
{
    /// <summary>
    /// Unique permission name/key
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display name
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Permission description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Category for grouping permissions
    /// </summary>
    public string Category { get; set; } = "General";

    /// <summary>
    /// Whether this is a system permission
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Roles that have this permission
    /// </summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
