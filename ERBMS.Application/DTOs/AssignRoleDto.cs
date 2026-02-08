namespace ERBMS.Application.DTOs;

public class AssignRoleDto
{
    public Guid UserId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}
