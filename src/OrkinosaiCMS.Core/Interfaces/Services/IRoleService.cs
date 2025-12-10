using OrkinosaiCMS.Core.Entities.Sites;

namespace OrkinosaiCMS.Core.Interfaces.Services;

/// <summary>
/// Service interface for role management operations
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Get role by ID
    /// </summary>
    Task<Role?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get role by name
    /// </summary>
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all roles
    /// </summary>
    Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new role
    /// </summary>
    Task<Role> CreateAsync(Role role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing role
    /// </summary>
    Task<Role> UpdateAsync(Role role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a role (soft delete)
    /// </summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assign permissions to a role
    /// </summary>
    Task AssignPermissionsAsync(int roleId, IEnumerable<int> permissionIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove permissions from a role
    /// </summary>
    Task RemovePermissionsAsync(int roleId, IEnumerable<int> permissionIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get role's permissions
    /// </summary>
    Task<IEnumerable<Permission>> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken = default);
}
