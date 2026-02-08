namespace ERBMS.Application.DTOs;

public class AuthLoginResult
{
    public AuthLoginStatus Status { get; set; }
    public AuthResponseDto? Response { get; set; }

    public static AuthLoginResult InvalidCredentials() => new() { Status = AuthLoginStatus.InvalidCredentials };
    public static AuthLoginResult PendingApproval() => new() { Status = AuthLoginStatus.PendingApproval };
    public static AuthLoginResult Success(AuthResponseDto response) => new() { Status = AuthLoginStatus.Success, Response = response };
}