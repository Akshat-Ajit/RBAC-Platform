using ERBMS.Application.DTOs;

namespace ERBMS.Application.Interfaces;

public interface ITokenService
{
    AccessTokenResult GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles);
    string GenerateRefreshToken();
}
