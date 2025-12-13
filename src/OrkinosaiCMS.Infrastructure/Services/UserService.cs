using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrkinosaiCMS.Core.Entities.Sites;
using OrkinosaiCMS.Core.Interfaces.Repositories;
using OrkinosaiCMS.Core.Interfaces.Services;
using OrkinosaiCMS.Infrastructure.Data;

namespace OrkinosaiCMS.Infrastructure.Services;

/// <summary>
/// Service implementation for user management operations
/// </summary>
public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<UserRole> _userRoleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        IRepository<UserRole> userRoleRepository,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("UserService.GetByUsernameAsync called for username: {Username}", username);
            var user = await _userRepository.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
            
            if (user != null)
            {
                _logger.LogInformation("User found - Username: {Username}, Id: {UserId}, Email: {Email}, IsActive: {IsActive}", 
                    user.Username, user.Id, user.Email, user.IsActive);
            }
            else
            {
                _logger.LogWarning("User not found by username: {Username}", username);
            }
            
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error in GetByUsernameAsync for username: {Username} - Type: {ExceptionType}, Message: {Message}", 
                username, ex.GetType().FullName, ex.Message);
            throw;
        }
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _userRepository.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetAllAsync(cancellationToken);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _userRepository.FindAsync(u => u.IsActive, cancellationToken);
    }

    public async Task<User> CreateAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        // Hash the password
        user.PasswordHash = HashPassword(password);
        user.CreatedOn = DateTime.UtcNow;
        user.IsActive = true;

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user;
    }

    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        user.ModifiedOn = DateTime.UtcNow;
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user != null)
        {
            _userRepository.Remove(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AssignRolesAsync(int userId, IEnumerable<int> roleIds, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new ArgumentException($"User with ID {userId} not found.");
        }

        foreach (var roleId in roleIds)
        {
            var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
            if (role == null)
            {
                continue;
            }

            // Check if user role already exists
            var existingUserRole = await _userRoleRepository.FirstOrDefaultAsync(
                ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);

            if (existingUserRole == null)
            {
                var userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    CreatedOn = DateTime.UtcNow
                };
                await _userRoleRepository.AddAsync(userRole, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveRolesAsync(int userId, IEnumerable<int> roleIds, CancellationToken cancellationToken = default)
    {
        foreach (var roleId in roleIds)
        {
            var userRole = await _userRoleRepository.FirstOrDefaultAsync(
                ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);

            if (userRole != null)
            {
                _userRoleRepository.Remove(userRole);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("UserService.GetUserRolesAsync called for userId: {UserId}", userId);
            
            var userRoles = await _userRoleRepository.FindAsync(ur => ur.UserId == userId, cancellationToken);
            var roleIds = userRoles.Select(ur => ur.RoleId).ToList();

            _logger.LogInformation("Found {Count} role mappings for userId: {UserId}, RoleIds: [{RoleIds}]", 
                roleIds.Count, userId, string.Join(", ", roleIds));

            if (!roleIds.Any())
            {
                _logger.LogWarning("No roles found for userId: {UserId}", userId);
                return Enumerable.Empty<Role>();
            }

            var roles = await _roleRepository.FindAsync(r => roleIds.Contains(r.Id), cancellationToken);
            var rolesList = roles.ToList();
            
            _logger.LogInformation("Retrieved {Count} roles for userId: {UserId} - Roles: [{RoleNames}]", 
                rolesList.Count, userId, string.Join(", ", rolesList.Select(r => r.Name)));
            
            return rolesList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error in GetUserRolesAsync for userId: {UserId} - Type: {ExceptionType}, Message: {Message}", 
                userId, ex.GetType().FullName, ex.Message);
            throw;
        }
    }

    public async Task<bool> VerifyPasswordAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("UserService.VerifyPasswordAsync called for username: {Username}", username);
            
            var user = await GetByUsernameAsync(username, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User not found in VerifyPasswordAsync: {Username}", username);
                return false;
            }

            _logger.LogInformation("User found for password verification - Username: {Username}, UserId: {UserId}, IsActive: {IsActive}", 
                username, user.Id, user.IsActive);

            var isValid = VerifyHashedPassword(user.PasswordHash, password);
            _logger.LogInformation("Password hash verification result for {Username}: {IsValid}", username, isValid);
            
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error in VerifyPasswordAsync for username: {Username} - Type: {ExceptionType}, Message: {Message}", 
                username, ex.GetType().FullName, ex.Message);
            throw;
        }
    }

    public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new ArgumentException($"User with ID {userId} not found.");
        }

        if (!VerifyHashedPassword(user.PasswordHash, currentPassword))
        {
            throw new InvalidOperationException("Current password is incorrect.");
        }

        user.PasswordHash = HashPassword(newPassword);
        user.ModifiedOn = DateTime.UtcNow;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateLastLoginAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("UserService.UpdateLastLoginAsync called for userId: {UserId}", userId);
            
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user != null)
            {
                var previousLogin = user.LastLoginOn;
                user.LastLoginOn = DateTime.UtcNow;
                _userRepository.Update(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation(
                    "Last login updated for userId: {UserId} - Previous: {PreviousLogin}, New: {NewLogin}", 
                    userId, previousLogin, user.LastLoginOn);
            }
            else
            {
                _logger.LogWarning("User not found in UpdateLastLoginAsync for userId: {UserId}", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error in UpdateLastLoginAsync for userId: {UserId} - Type: {ExceptionType}, Message: {Message}", 
                userId, ex.GetType().FullName, ex.Message);
            throw;
        }
    }

    // Simple password hashing - in production, use ASP.NET Core Identity or a proper password hashing library
    private string HashPassword(string password)
    {
        // Using BCrypt.Net for password hashing
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyHashedPassword(string hashedPassword, string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
