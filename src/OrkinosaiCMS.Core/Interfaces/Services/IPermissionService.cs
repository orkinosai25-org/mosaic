using OrkinosaiCMS.Core.Entities.Sites;

namespace OrkinosaiCMS.Core.Interfaces.Services;

/// <summary>
/// Service interface for permission management operations
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Get permission by ID
    /// </summary>
    Task<Permission?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get permission by name
    /// </summary>
    Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all permissions
    /// </summary>
    Task<IEnumerable<Permission>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new permission
    /// </summary>
    Task<Permission> CreateAsync(Permission permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing permission
    /// </summary>
    Task<Permission> UpdateAsync(Permission permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a permission (soft delete)
    /// </summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user has permission
    /// </summary>
    Task<bool> UserHasPermissionAsync(int userId, string permissionName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all permissions for a user
    /// </summary>
    Task<IEnumerable<Permission>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default);
}
