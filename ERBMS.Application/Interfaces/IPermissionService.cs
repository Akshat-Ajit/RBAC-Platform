using ERBMS.Application.DTOs;

namespace ERBMS.Application.Interfaces;

public interface IPermissionService
{
    Task<IReadOnlyList<PermissionDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<PermissionDto> CreateAsync(CreatePermissionDto dto, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Guid id, CreatePermissionDto dto, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}