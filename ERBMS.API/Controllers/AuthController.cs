using ERBMS.API.Auth;
using ERBMS.Application.DTOs;
using ERBMS.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ERBMS.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IIdentityService _identityService;

    public AuthController(IAuthService authService, IIdentityService identityService)
    {
        _authService = authService;
        _identityService = identityService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
    {
        var exists = await _identityService.EmailExistsAsync(dto.Email, cancellationToken);
        if (exists)
        {
            return Conflict("Email already used.");
        }

        var created = await _authService.RegisterAsync(dto, cancellationToken);
        return created
            ? Accepted(new { message = "Account created. Await admin approval." })
            : BadRequest("Registration failed.");
    }

    [HttpGet("email-available")]
    public async Task<IActionResult> EmailAvailable([FromQuery] string email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Email is required.");
        }

        var exists = await _identityService.EmailExistsAsync(email, cancellationToken);
        return Ok(new { available = !exists });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(dto, cancellationToken);
        return result.Status switch
        {
            AuthLoginStatus.Success => Ok(result.Response),
            AuthLoginStatus.PendingApproval => StatusCode(403, "Account pending admin approval."),
            _ => Unauthorized()
        };
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
        return result is null ? Unauthorized() : Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var revoked = await _authService.LogoutAsync(request.RefreshToken, cancellationToken);
        return revoked ? Ok() : NotFound();
    }
}
