using ERBMS.Application.DTOs;
using ERBMS.Application.Interfaces;
using ERBMS.Domain.Entities;
using ERBMS.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ERBMS.Application.Services;

public class AuthService : IAuthService
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;
    private readonly string _systemAdminEmail;

    public AuthService(
        IIdentityService identityService,
        ITokenService tokenService,
        IUserRepository userRepository,
        IConfiguration configuration)
    {
        _identityService = identityService;
        _tokenService = tokenService;
        _userRepository = userRepository;
        _systemAdminEmail = configuration["Seed:AdminEmail"] ?? "admin@erbms.local";
    }

    public async Task<bool> RegisterAsync(CreateUserDto dto, CancellationToken cancellationToken)
    {
        var identity = await _identityService.RegisterAsync(dto, cancellationToken);
        if (identity is null)
        {
            return false;
        }

        var existing = await _userRepository.GetByEmailAsync(identity.Email, cancellationToken);
        if (existing is null)
        {
            var user = new User
            {
                Id = identity.UserId,
                Email = identity.Email,
                FullName = identity.FullName,
                IsActive = false
            };

            await _userRepository.AddAsync(user, cancellationToken);
        }

        return true;
    }

    public async Task<AuthLoginResult> LoginAsync(LoginDto dto, CancellationToken cancellationToken)
    {
        var identity = await _identityService.ValidateCredentialsAsync(dto.Email, dto.Password, cancellationToken);
        if (identity is null)
        {
            return AuthLoginResult.InvalidCredentials();
        }

        var user = await _userRepository.GetByIdAsync(identity.UserId, cancellationToken);
        if (user is null)
        {
            return AuthLoginResult.InvalidCredentials();
        }

        if (!user.IsActive)
        {
            return AuthLoginResult.PendingApproval();
        }

        var response = await IssueTokensAsync(identity, user, cancellationToken);
        return response is null ? AuthLoginResult.InvalidCredentials() : AuthLoginResult.Success(response);
    }

    public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var identity = await _identityService.GetUserByRefreshTokenAsync(refreshToken, cancellationToken);
        if (identity is null)
        {
            return null;
        }

        var user = await _userRepository.GetByIdAsync(identity.UserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return null;
        }

        await _identityService.RevokeRefreshTokenAsync(refreshToken, cancellationToken);
        return await IssueTokensAsync(identity, user, cancellationToken);
    }

    public Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        return _identityService.RevokeRefreshTokenAsync(refreshToken, cancellationToken);
    }

    private async Task<AuthResponseDto?> IssueTokensAsync(IdentityUserInfo identity, User user, CancellationToken cancellationToken)
    {
        var accessToken = _tokenService.GenerateAccessToken(identity.UserId, identity.Email, identity.Roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        await _identityService.StoreRefreshTokenAsync(
            identity.UserId,
            refreshToken,
            accessToken.ExpiresAt.AddDays(7),
            cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken.Token,
            RefreshToken = refreshToken,
            ExpiresAt = accessToken.ExpiresAt,
            User = new UserDto
            {
                Id = identity.UserId,
                Email = identity.Email,
                FullName = identity.FullName,
                Roles = identity.Roles,
                IsActive = user.IsActive,
                IsSystemAdmin = string.Equals(identity.Email, _systemAdminEmail, StringComparison.OrdinalIgnoreCase)
            }
        };
    }
}
