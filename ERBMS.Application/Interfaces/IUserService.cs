using ERBMS.Application.DTOs;

namespace ERBMS.Application.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<UserDto?> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> AssignRoleAsync(AssignRoleDto dto, CancellationToken cancellationToken);
    Task<bool> RemoveRoleAsync(AssignRoleDto dto, CancellationToken cancellationToken);
    Task<bool> ApproveAsync(Guid id, CancellationToken cancellationToken);
    Task<CleanupIdentityResult> CleanupIdentityUserAsync(CleanupIdentityUserDto dto, CancellationToken cancellationToken);
}
