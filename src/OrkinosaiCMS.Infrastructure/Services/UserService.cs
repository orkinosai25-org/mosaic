using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrkinosaiCMS.Core.Entities.Identity;
using OrkinosaiCMS.Core.Entities.Sites;
using OrkinosaiCMS.Core.Interfaces.Repositories;
using OrkinosaiCMS.Core.Interfaces.Services;
using OrkinosaiCMS.Infrastructure.Data;

namespace OrkinosaiCMS.Infrastructure.Services;

/// <summary>
/// Service implementation for user management operations
/// Enhanced to support both legacy User table and ASP.NET Core Identity
/// </summary>
public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<UserRole> _userRoleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        IRepository<UserRole> userRoleRepository,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _unitOfWork = unitOfWork;
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.LegacyUsers
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("UserService.GetByUsernameAsync called for username: {Username}", username);
            
            // First, try to find user in Identity (AspNetUsers)
            var identityUser = await _userManager.FindByNameAsync(username);
            if (identityUser != null)
            {
                _logger.LogInformation("User found in Identity system - Username: {Username}, Id: {UserId}, Email: {Email}", 
                    identityUser.UserName, identityUser.Id, identityUser.Email);
                
                // Convert ApplicationUser to legacy User for compatibility
                // This allows existing code to work with Identity users
                var user = new User
                {
                    Id = identityUser.Id,
                    Username = identityUser.UserName ?? string.Empty,
                    Email = identityUser.Email ?? string.Empty,
                    DisplayName = identityUser.DisplayName,
                    IsActive = !identityUser.IsDeleted, // Identity users are active if not deleted
                    CreatedOn = identityUser.CreatedOn,
                    ModifiedOn = identityUser.ModifiedOn
                };
                
                return user;
            }
            
            // Fall back to legacy Users table
            _logger.LogInformation("User not found in Identity, checking legacy Users table for username: {Username}", username);
            var legacyUser = await _userRepository.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
            
            if (legacyUser != null)
            {
                _logger.LogInformation("User found in legacy table - Username: {Username}, Id: {UserId}, Email: {Email}, IsActive: {IsActive}", 
                    legacyUser.Username, legacyUser.Id, legacyUser.Email, legacyUser.IsActive);
            }
            else
            {
                _logger.LogWarning("User not found in either Identity or legacy Users table: {Username}", username);
            }
            
            return legacyUser;
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
            
            // First, try to get roles from Identity system
            var identityUser = await _userManager.FindByIdAsync(userId.ToString());
            if (identityUser != null)
            {
                _logger.LogInformation("User found in Identity system, fetching Identity roles for userId: {UserId}", userId);
                var identityRoleNames = await _userManager.GetRolesAsync(identityUser);
                
                if (identityRoleNames.Any())
                {
                    _logger.LogInformation("Found {Count} Identity roles for userId: {UserId} - Roles: [{Roles}]", 
                        identityRoleNames.Count, userId, string.Join(", ", identityRoleNames));
                    
                    // Convert Identity role names to legacy Role objects for compatibility
                    var roles = identityRoleNames.Select(roleName => new Role
                    {
                        Id = 0, // Identity roles don't map to legacy role IDs
                        Name = roleName,
                        Description = $"Identity role: {roleName}",
                        IsSystem = true,
                        CreatedOn = DateTime.UtcNow
                    }).ToList();
                    
                    return roles;
                }
                
                _logger.LogWarning("User found in Identity but has no roles assigned: userId: {UserId}", userId);
            }
            
            // Fall back to legacy role system
            _logger.LogInformation("Checking legacy role system for userId: {UserId}", userId);
            var userRoles = await _userRoleRepository.FindAsync(ur => ur.UserId == userId, cancellationToken);
            var roleIds = userRoles.Select(ur => ur.RoleId).ToList();

            _logger.LogInformation("Found {Count} legacy role mappings for userId: {UserId}, RoleIds: [{RoleIds}]", 
                roleIds.Count, userId, string.Join(", ", roleIds));

            if (!roleIds.Any())
            {
                _logger.LogWarning("No roles found in either Identity or legacy system for userId: {UserId}", userId);
                return Enumerable.Empty<Role>();
            }

            var legacyRoles = await _roleRepository.FindAsync(r => roleIds.Contains(r.Id), cancellationToken);
            var rolesList = legacyRoles.ToList();
            
            _logger.LogInformation("Retrieved {Count} legacy roles for userId: {UserId} - Roles: [{RoleNames}]", 
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
            
            // First, try to authenticate with Identity (AspNetUsers)
            _logger.LogInformation("Checking Identity (AspNetUsers) for username: {Username}", username);
            var identityUser = await _userManager.FindByNameAsync(username);
            
            if (identityUser != null)
            {
                _logger.LogInformation("User found in Identity system - UserId: {UserId}, Username: {Username}, Email: {Email}", 
                    identityUser.Id, identityUser.UserName, identityUser.Email);
                
                // Verify password using Identity's password hasher
                var passwordValid = await _userManager.CheckPasswordAsync(identityUser, password);
                _logger.LogInformation("Identity password verification result for {Username}: {IsValid}", username, passwordValid);
                
                // Return the result immediately - don't fall back to legacy for Identity users
                return passwordValid;
            }
            else
            {
                _logger.LogInformation("User not found in Identity system, checking legacy Users table");
            }
            
            // Fall back to legacy User table for backward compatibility
            _logger.LogInformation("Checking legacy Users table for username: {Username}", username);
            var legacyUser = await GetByUsernameAsync(username, cancellationToken);
            if (legacyUser == null)
            {
                _logger.LogWarning("User not found in either Identity or legacy Users table: {Username}", username);
                return false;
            }

            _logger.LogInformation("User found in legacy table - Username: {Username}, UserId: {UserId}, IsActive: {IsActive}", 
                username, legacyUser.Id, legacyUser.IsActive);

            var isValid = VerifyHashedPassword(legacyUser.PasswordHash, password);
            _logger.LogInformation("Legacy password hash verification result for {Username}: {IsValid}", username, isValid);
            
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
            
            // Try to update Identity user first
            var identityUser = await _userManager.FindByIdAsync(userId.ToString());
            if (identityUser != null)
            {
                _logger.LogInformation("Updating last login for Identity user: {UserId}", userId);
                identityUser.ModifiedOn = DateTime.UtcNow;
                await _userManager.UpdateAsync(identityUser);
                _logger.LogInformation("Last login updated for Identity user: {UserId}", userId);
                return;
            }
            
            // Fall back to legacy user
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user != null)
            {
                var previousLogin = user.LastLoginOn;
                user.LastLoginOn = DateTime.UtcNow;
                _userRepository.Update(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation(
                    "Last login updated for legacy userId: {UserId} - Previous: {PreviousLogin}, New: {NewLogin}", 
                    userId, previousLogin, user.LastLoginOn);
            }
            else
            {
                _logger.LogWarning("User not found in either Identity or legacy system for UpdateLastLoginAsync: userId {UserId}", userId);
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
        // Handle null or empty password hash gracefully
        if (string.IsNullOrEmpty(hashedPassword))
        {
            _logger.LogWarning("VerifyHashedPassword called with null or empty password hash");
            return false;
        }
        
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
