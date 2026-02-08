using ERBMS.Application.DTOs;

namespace ERBMS.Application.Interfaces;

public interface IAuthService
{
    Task<bool> RegisterAsync(CreateUserDto dto, CancellationToken cancellationToken);
    Task<AuthLoginResult> LoginAsync(LoginDto dto, CancellationToken cancellationToken);
    Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken);
}
