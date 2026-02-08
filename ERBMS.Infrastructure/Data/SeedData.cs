using ERBMS.Domain.Entities;
using ERBMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ERBMS.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var scopedProvider = scope.ServiceProvider;

        var dbContext = scopedProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();

        var roleManager = scopedProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scopedProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var roles = configuration.GetSection("Seed:Roles").Get<string[]>()
            ?? new[] { "Admin", "Manager", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        foreach (var role in roles)
        {
            var exists = await dbContext.AccessRoles.AnyAsync(r => r.Name == role);
            if (!exists)
            {
                dbContext.AccessRoles.Add(new Role
                {
                    Id = Guid.NewGuid(),
                    Name = role,
                    Description = $"Default {role} role"
                });
            }
        }

        var permissionSeed = new[]
        {
            new Permission { Id = Guid.NewGuid(), Name = "Users.Read", Description = "View users" },
            new Permission { Id = Guid.NewGuid(), Name = "Users.Manage", Description = "Create or update users" },
            new Permission { Id = Guid.NewGuid(), Name = "Roles.Read", Description = "View roles" },
            new Permission { Id = Guid.NewGuid(), Name = "Roles.Manage", Description = "Create or update roles" },
            new Permission { Id = Guid.NewGuid(), Name = "Permissions.Manage", Description = "Manage permissions" },
            new Permission { Id = Guid.NewGuid(), Name = "Audit.Read", Description = "View audit logs" }
        };

        foreach (var permission in permissionSeed)
        {
            var exists = await dbContext.Permissions.AnyAsync(p => p.Name == permission.Name);
            if (!exists)
            {
                dbContext.Permissions.Add(permission);
            }
        }

        await dbContext.SaveChangesAsync();

        var adminRoleEntity = await dbContext.AccessRoles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRoleEntity is not null)
        {
            var permissions = await dbContext.Permissions.ToListAsync();
            foreach (var permission in permissions)
            {
                var assigned = await dbContext.RolePermissions.AnyAsync(rp => rp.RoleId == adminRoleEntity.Id && rp.PermissionId == permission.Id);
                if (!assigned)
                {
                    dbContext.RolePermissions.Add(new RolePermission
                    {
                        RoleId = adminRoleEntity.Id,
                        PermissionId = permission.Id
                    });
                }
            }
        }

        var adminEmail = configuration["Seed:AdminEmail"] ?? "admin@erbms.local";
        var adminPassword = configuration["Seed:AdminPassword"] ?? "ChangeMe123!";
        var adminName = configuration["Seed:AdminFullName"] ?? "System Admin";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = adminEmail,
                UserName = adminEmail,
                FullName = adminName
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        else if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        var businessUser = await dbContext.BusinessUsers.FirstOrDefaultAsync(u => u.Email == adminEmail);
        if (businessUser is null)
        {
            businessUser = new User
            {
                Id = adminUser.Id,
                Email = adminUser.Email ?? adminEmail,
                FullName = adminUser.FullName,
                IsActive = true
            };

            dbContext.BusinessUsers.Add(businessUser);
        }

        var adminRole = await dbContext.AccessRoles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole is not null && !await dbContext.BusinessUserRoles.AnyAsync(ur => ur.UserId == businessUser.Id && ur.RoleId == adminRole.Id))
        {
            dbContext.BusinessUserRoles.Add(new UserRole
            {
                UserId = businessUser.Id,
                RoleId = adminRole.Id
            });
        }

        await dbContext.SaveChangesAsync();
    }
}
