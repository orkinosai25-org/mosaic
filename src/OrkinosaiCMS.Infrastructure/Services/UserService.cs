using Microsoft.EntityFrameworkCore;
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

    public UserService(
        IRepository<User> userRepository,
        IRepository<Role> roleRepository,
        IRepository<UserRole> userRoleRepository,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _unitOfWork = unitOfWork;
        _context = context;
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
        return await _userRepository.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
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
        var userRoles = await _userRoleRepository.FindAsync(ur => ur.UserId == userId, cancellationToken);
        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();

        if (!roleIds.Any())
        {
            return Enumerable.Empty<Role>();
        }

        return await _roleRepository.FindAsync(r => roleIds.Contains(r.Id), cancellationToken);
    }

    public async Task<bool> VerifyPasswordAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var user = await GetByUsernameAsync(username, cancellationToken);
        if (user == null)
        {
            return false;
        }

        return VerifyHashedPassword(user.PasswordHash, password);
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
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user != null)
        {
            user.LastLoginOn = DateTime.UtcNow;
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
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
