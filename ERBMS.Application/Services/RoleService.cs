using ERBMS.Application.DTOs;
using ERBMS.Application.Interfaces;
using ERBMS.Domain.Entities;
using ERBMS.Domain.Interfaces;

namespace ERBMS.Application.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IIdentityService _identityService;

    public RoleService(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IIdentityService identityService)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _identityService = identityService;
    }

    public async Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var roles = await _roleRepository.GetAllAsync(cancellationToken);
        return roles.Select(MapRole).ToList();
    }

    public async Task<RoleDto> CreateAsync(CreateRoleDto dto, CancellationToken cancellationToken)
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description
        };

        await _identityService.EnsureRoleExistsAsync(role.Name, cancellationToken);
        await _roleRepository.AddAsync(role, cancellationToken);
        return MapRole(role);
    }

    public async Task<bool> UpdateAsync(Guid id, CreateRoleDto dto, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(id, cancellationToken);
        if (role is null)
        {
            return false;
        }

        role.Name = dto.Name;
        role.Description = dto.Description;

        await _identityService.EnsureRoleExistsAsync(role.Name, cancellationToken);
        await _roleRepository.UpdateAsync(role, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(id, cancellationToken);
        if (role is null)
        {
            return false;
        }

        await _roleRepository.DeleteAsync(role, cancellationToken);
        return true;
    }

    public async Task<bool> AssignPermissionAsync(AssignPermissionDto dto, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(dto.RoleId, cancellationToken);
        if (role is null)
        {
            return false;
        }

        var permission = await _permissionRepository.GetByIdAsync(dto.PermissionId, cancellationToken);
        if (permission is null)
        {
            return false;
        }

        if (role.RolePermissions.All(rp => rp.PermissionId != permission.Id))
        {
            role.RolePermissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permission.Id
            });
        }

        await _roleRepository.UpdateAsync(role, cancellationToken);
        return true;
    }

    private static RoleDto MapRole(Role role)
    {
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description
        };
    }
}
