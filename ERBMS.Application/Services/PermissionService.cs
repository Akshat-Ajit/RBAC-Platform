using ERBMS.Application.DTOs;
using ERBMS.Application.Interfaces;
using ERBMS.Domain.Entities;
using ERBMS.Domain.Interfaces;

namespace ERBMS.Application.Services;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;

    public PermissionService(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<IReadOnlyList<PermissionDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var permissions = await _permissionRepository.GetAllAsync(cancellationToken);
        return permissions.Select(MapPermission).ToList();
    }

    public async Task<PermissionDto> CreateAsync(CreatePermissionDto dto, CancellationToken cancellationToken)
    {
        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description
        };

        await _permissionRepository.AddAsync(permission, cancellationToken);
        return MapPermission(permission);
    }

    public async Task<bool> UpdateAsync(Guid id, CreatePermissionDto dto, CancellationToken cancellationToken)
    {
        var permission = await _permissionRepository.GetByIdAsync(id, cancellationToken);
        if (permission is null)
        {
            return false;
        }

        permission.Name = dto.Name;
        permission.Description = dto.Description;

        await _permissionRepository.UpdateAsync(permission, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var permission = await _permissionRepository.GetByIdAsync(id, cancellationToken);
        if (permission is null)
        {
            return false;
        }

        await _permissionRepository.DeleteAsync(permission, cancellationToken);
        return true;
    }

    private static PermissionDto MapPermission(Permission permission)
    {
        return new PermissionDto
        {
            Id = permission.Id,
            Name = permission.Name,
            Description = permission.Description
        };
    }
}
