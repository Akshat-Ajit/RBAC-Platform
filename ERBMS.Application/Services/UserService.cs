using ERBMS.Application.DTOs;
using ERBMS.Application.Interfaces;
using ERBMS.Domain.Entities;
using ERBMS.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ERBMS.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IIdentityService _identityService;
    private readonly string _systemAdminEmail;

    public UserService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IIdentityService identityService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _identityService = identityService;
        _systemAdminEmail = configuration["Seed:AdminEmail"] ?? "admin@erbms.local";
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(MapUser).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : MapUser(user);
    }

    public async Task<UserDto?> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken)
    {
        var identity = await _identityService.CreateUserAsync(dto, cancellationToken);
        if (identity is null)
        {
            return null;
        }

        var existing = await _userRepository.GetByEmailAsync(identity.Email, cancellationToken);
        if (existing is null)
        {
            var user = new User
            {
                Id = identity.UserId,
                Email = identity.Email,
                FullName = identity.FullName,
                IsActive = true
            };

            await _userRepository.AddAsync(user, cancellationToken);
            return MapUser(user);
        }

        return MapUser(existing);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        user.FullName = dto.FullName;
        user.Email = dto.Email;

        await _userRepository.UpdateAsync(user, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        if (IsSystemAdmin(user.Email))
        {
            return false;
        }

        var identityDeleted = await _identityService.DeleteUserAsync(user.Id, cancellationToken);
        if (!identityDeleted)
        {
            return false;
        }

        await _userRepository.DeleteAsync(user, cancellationToken);
        return true;
    }

    public async Task<bool> ApproveAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        if (!user.IsActive)
        {
            user.IsActive = true;
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        return true;
    }

    public async Task<CleanupIdentityResult> CleanupIdentityUserAsync(CleanupIdentityUserDto dto, CancellationToken cancellationToken)
    {
        if (IsSystemAdmin(dto.Email))
        {
            return CleanupIdentityResult.Forbidden;
        }

        var existing = await _userRepository.GetByEmailAsync(dto.Email, cancellationToken);
        if (existing is not null)
        {
            return CleanupIdentityResult.InUse;
        }

        var deleted = await _identityService.DeleteUserByEmailAsync(dto.Email, cancellationToken);
        return deleted ? CleanupIdentityResult.Deleted : CleanupIdentityResult.NotFound;
    }

    public async Task<bool> AssignRoleAsync(AssignRoleDto dto, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(dto.UserId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        var role = await _roleRepository.GetByNameAsync(dto.RoleName, cancellationToken);
        if (role is null)
        {
            return false;
        }

        if (user.UserRoles.All(ur => ur.RoleId != role.Id))
        {
            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            });
        }

        await _identityService.AssignRoleAsync(user.Id, role.Name, cancellationToken);

        await _userRepository.UpdateAsync(user, cancellationToken);
        return true;
    }

    public async Task<bool> RemoveRoleAsync(AssignRoleDto dto, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(dto.UserId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        var role = await _roleRepository.GetByNameAsync(dto.RoleName, cancellationToken);
        if (role is null)
        {
            return false;
        }

        var link = user.UserRoles.FirstOrDefault(ur => ur.RoleId == role.Id);
        if (link is not null)
        {
            user.UserRoles.Remove(link);
        }

        await _identityService.RemoveRoleAsync(user.Id, role.Name, cancellationToken);
        await _userRepository.UpdateAsync(user, cancellationToken);
        return true;
    }

    private UserDto MapUser(User user)
    {
        var roles = user.UserRoles
            .Select(ur => ur.Role?.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .ToList();

        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Roles = roles,
            IsSystemAdmin = IsSystemAdmin(user.Email)
        };
    }

    private bool IsSystemAdmin(string email)
    {
        return string.Equals(email, _systemAdminEmail, StringComparison.OrdinalIgnoreCase);
    }
}
