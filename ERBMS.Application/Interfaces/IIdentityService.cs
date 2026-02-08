using ERBMS.Application.DTOs;

namespace ERBMS.Application.Interfaces;

public interface IIdentityService
{
    Task<IdentityUserInfo?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken);
    Task<IdentityUserInfo?> RegisterAsync(CreateUserDto dto, CancellationToken cancellationToken);
    Task<IdentityUserInfo?> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
    Task<IdentityUserInfo?> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    Task StoreRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiresAt, CancellationToken cancellationToken);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    Task<bool> AssignRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken);
    Task<bool> RemoveRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken);
    Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> DeleteUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task EnsureRoleExistsAsync(string roleName, CancellationToken cancellationToken);
}
