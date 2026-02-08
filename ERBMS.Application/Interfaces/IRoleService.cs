using ERBMS.Application.DTOs;

namespace ERBMS.Application.Interfaces;

public interface IRoleService
{
    Task<IReadOnlyList<RoleDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<RoleDto> CreateAsync(CreateRoleDto dto, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Guid id, CreateRoleDto dto, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> AssignPermissionAsync(AssignPermissionDto dto, CancellationToken cancellationToken);
}
