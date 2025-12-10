using OrkinosaiCMS.Core.Common;

namespace OrkinosaiCMS.Core.Entities.Sites;

/// <summary>
/// Represents a role in the CMS
/// Inspired by SharePoint permission levels
/// </summary>
public class Role : BaseEntity
{
    /// <summary>
    /// Role name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Role description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this is a system role
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Permissions associated with this role
    /// </summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    /// <summary>
    /// Users with this role
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
