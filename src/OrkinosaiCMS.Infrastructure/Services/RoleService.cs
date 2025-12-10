using OrkinosaiCMS.Core.Entities.Sites;
using OrkinosaiCMS.Core.Interfaces.Repositories;
using OrkinosaiCMS.Core.Interfaces.Services;

namespace OrkinosaiCMS.Infrastructure.Services;

/// <summary>
/// Service implementation for role management operations
/// </summary>
public class RoleService : IRoleService
{
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<Permission> _permissionRepository;
    private readonly IRepository<RolePermission> _rolePermissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RoleService(
        IRepository<Role> roleRepository,
        IRepository<Permission> permissionRepository,
        IRepository<RolePermission> rolePermissionRepository,
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _rolePermissionRepository = rolePermissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Role?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _roleRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _roleRepository.FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _roleRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Role> CreateAsync(Role role, CancellationToken cancellationToken = default)
    {
        role.CreatedOn = DateTime.UtcNow;
        await _roleRepository.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return role;
    }

    public async Task<Role> UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        role.ModifiedOn = DateTime.UtcNow;
        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return role;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(id, cancellationToken);
        if (role != null)
        {
            if (role.IsSystem)
            {
                throw new InvalidOperationException("Cannot delete system roles.");
            }

            _roleRepository.Remove(role);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AssignPermissionsAsync(int roleId, IEnumerable<int> permissionIds, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
        if (role == null)
        {
            throw new ArgumentException($"Role with ID {roleId} not found.");
        }

        foreach (var permissionId in permissionIds)
        {
            var permission = await _permissionRepository.GetByIdAsync(permissionId, cancellationToken);
            if (permission == null)
            {
                continue;
            }

            // Check if role permission already exists
            var existingRolePermission = await _rolePermissionRepository.FirstOrDefaultAsync(
                rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

            if (existingRolePermission == null)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId,
                    CreatedOn = DateTime.UtcNow
                };
                await _rolePermissionRepository.AddAsync(rolePermission, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemovePermissionsAsync(int roleId, IEnumerable<int> permissionIds, CancellationToken cancellationToken = default)
    {
        foreach (var permissionId in permissionIds)
        {
            var rolePermission = await _rolePermissionRepository.FirstOrDefaultAsync(
                rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

            if (rolePermission != null)
            {
                _rolePermissionRepository.Remove(rolePermission);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var rolePermissions = await _rolePermissionRepository.FindAsync(rp => rp.RoleId == roleId, cancellationToken);
        var permissionIds = rolePermissions.Select(rp => rp.PermissionId).ToList();

        if (!permissionIds.Any())
        {
            return Enumerable.Empty<Permission>();
        }

        return await _permissionRepository.FindAsync(p => permissionIds.Contains(p.Id), cancellationToken);
    }
}
