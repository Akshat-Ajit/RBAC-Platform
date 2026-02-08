using Microsoft.AspNetCore.Identity;

namespace ERBMS.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
}
