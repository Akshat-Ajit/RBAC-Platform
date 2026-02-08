using ERBMS.Domain.Entities;
using ERBMS.Domain.Interfaces;
using ERBMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERBMS.Infrastructure.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PermissionRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Permissions.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return _dbContext.Permissions.FirstOrDefaultAsync(p => p.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Permissions.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Permission permission, CancellationToken cancellationToken)
    {
        _dbContext.Permissions.Add(permission);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Permission permission, CancellationToken cancellationToken)
    {
        _dbContext.Permissions.Update(permission);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Permission permission, CancellationToken cancellationToken)
    {
        _dbContext.Permissions.Remove(permission);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
