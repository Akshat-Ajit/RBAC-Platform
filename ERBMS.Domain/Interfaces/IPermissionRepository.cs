using ERBMS.Domain.Entities;

namespace ERBMS.Domain.Interfaces;

public interface IPermissionRepository
{
    Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(Permission permission, CancellationToken cancellationToken);
    Task UpdateAsync(Permission permission, CancellationToken cancellationToken);
    Task DeleteAsync(Permission permission, CancellationToken cancellationToken);
}
