using OrkinosaiCMS.Core.Common;

namespace OrkinosaiCMS.Core.Entities.Sites;

/// <summary>
/// Many-to-many relationship between Users and Roles
/// </summary>
public class UserRole : BaseEntity
{
    /// <summary>
    /// User ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Role ID
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// Navigation user
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Navigation role
    /// </summary>
    public Role? Role { get; set; }
}
