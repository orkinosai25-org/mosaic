using OrkinosaiCMS.Core.Common;

namespace OrkinosaiCMS.Core.Entities.Sites;

/// <summary>
/// Many-to-many relationship between Roles and Permissions
/// </summary>
public class RolePermission : BaseEntity
{
    /// <summary>
    /// Role ID
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// Permission ID
    /// </summary>
    public int PermissionId { get; set; }

    /// <summary>
    /// Navigation role
    /// </summary>
    public Role? Role { get; set; }

    /// <summary>
    /// Navigation permission
    /// </summary>
    public Permission? Permission { get; set; }
}
