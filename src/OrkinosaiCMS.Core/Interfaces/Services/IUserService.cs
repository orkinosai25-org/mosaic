using OrkinosaiCMS.Core.Entities.Sites;

namespace OrkinosaiCMS.Core.Interfaces.Services;

/// <summary>
/// Service interface for user management operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user by username
    /// </summary>
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user by email
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all users
    /// </summary>
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active users
    /// </summary>
    Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new user
    /// </summary>
    Task<User> CreateAsync(User user, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing user
    /// </summary>
    Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a user (soft delete)
    /// </summary>
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assign roles to a user
    /// </summary>
    Task AssignRolesAsync(int userId, IEnumerable<int> roleIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove roles from a user
    /// </summary>
    Task RemoveRolesAsync(int userId, IEnumerable<int> roleIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user's roles
    /// </summary>
    Task<IEnumerable<Role>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify user password
    /// </summary>
    Task<bool> VerifyPasswordAsync(string username, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Change user password
    /// </summary>
    Task ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update last login timestamp
    /// </summary>
    Task UpdateLastLoginAsync(int userId, CancellationToken cancellationToken = default);
}
