using ERBMS.Application.DTOs;
using ERBMS.Application.Interfaces;
using ERBMS.Domain.Entities;
using ERBMS.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ERBMS.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public IdentityService(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    public async Task<IdentityUserInfo?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
        if (!result.Succeeded)
        {
            return null;
        }

        return await MapIdentityUserAsync(user);
    }

    public async Task<IdentityUserInfo?> RegisterAsync(CreateUserDto dto, CancellationToken cancellationToken)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing is not null)
        {
            return null;
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            UserName = dto.Email,
            FullName = dto.FullName
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            return null;
        }

        await EnsureRoleExistsAsync("User", cancellationToken);
        await _userManager.AddToRoleAsync(user, "User");

        return await MapIdentityUserAsync(user);
    }

    public async Task<IdentityUserInfo?> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing is not null)
        {
            return null;
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            UserName = dto.Email,
            FullName = dto.FullName
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            return null;
        }

        await EnsureRoleExistsAsync("User", cancellationToken);
        await _userManager.AddToRoleAsync(user, "User");

        return await MapIdentityUserAsync(user);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        var existing = await _userManager.FindByEmailAsync(email);
        return existing is not null;
    }

    public async Task<IdentityUserInfo?> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var token = await _dbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow, cancellationToken);

        if (token is null)
        {
            return null;
        }

        var user = await _userManager.FindByIdAsync(token.UserId.ToString());
        return user is null ? null : await MapIdentityUserAsync(user);
    }

    public async Task StoreRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiresAt, CancellationToken cancellationToken)
    {
        var entity = new RefreshToken
        {
            Token = refreshToken,
            UserId = userId,
            ExpiryDate = expiresAt,
            IsRevoked = false
        };

        _dbContext.RefreshTokens.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var token = await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);
        if (token is null)
        {
            return false;
        }

        token.IsRevoked = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task EnsureRoleExistsAsync(string roleName, CancellationToken cancellationToken)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        }
    }

    public async Task<bool> AssignRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return false;
        }

        await EnsureRoleExistsAsync(roleName, cancellationToken);
        if (!await _userManager.IsInRoleAsync(user, roleName))
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }

        return true;
    }

    public async Task<bool> RemoveRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return false;
        }

        if (await _userManager.IsInRoleAsync(user, roleName))
        {
            await _userManager.RemoveFromRoleAsync(user, roleName);
        }

        return true;
    }

    public async Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return false;
        }

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> DeleteUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return false;
        }

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    private async Task<IdentityUserInfo> MapIdentityUserAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new IdentityUserInfo
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Roles = roles.ToArray()
        };
    }
}
