using ERBMS.Application.DTOs;
using ERBMS.Application.Interfaces;
using ERBMS.Application.Services;
using ERBMS.Domain.Entities;
using ERBMS.Domain.Interfaces;
using Moq;

namespace ERBMS.Tests;

public class RoleServiceTests
{
    [Fact]
    public async Task CreateAsync_EnsuresIdentityRole()
    {
        var roleRepository = new Mock<IRoleRepository>();
        var permissionRepository = new Mock<IPermissionRepository>();
        var identityService = new Mock<IIdentityService>();

        roleRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        identityService
            .Setup(service => service.EnsureRoleExistsAsync("Admin", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new RoleService(roleRepository.Object, permissionRepository.Object, identityService.Object);
        var result = await service.CreateAsync(new CreateRoleDto { Name = "Admin", Description = "Admin role" }, CancellationToken.None);

        Assert.Equal("Admin", result.Name);
        identityService.Verify(service => service.EnsureRoleExistsAsync("Admin", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignPermissionAsync_ReturnsFalse_WhenRoleMissing()
    {
        var roleRepository = new Mock<IRoleRepository>();
        var permissionRepository = new Mock<IPermissionRepository>();
        var identityService = new Mock<IIdentityService>();

        roleRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        var service = new RoleService(roleRepository.Object, permissionRepository.Object, identityService.Object);
        var result = await service.AssignPermissionAsync(new AssignPermissionDto
        {
            RoleId = Guid.NewGuid(),
            PermissionId = Guid.NewGuid()
        }, CancellationToken.None);

        Assert.False(result);
    }
}
