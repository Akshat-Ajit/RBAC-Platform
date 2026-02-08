using ERBMS.Application.DTOs;
using ERBMS.Application.Interfaces;
using ERBMS.Application.Services;
using ERBMS.Domain.Entities;
using ERBMS.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;

namespace ERBMS.Tests;

public class UserServiceTests
{
    [Fact]
    public async Task CreateAsync_ReturnsNull_WhenIdentityFails()
    {
        var identityService = new Mock<IIdentityService>();
        var userRepository = new Mock<IUserRepository>();
        var roleRepository = new Mock<IRoleRepository>();

        identityService
            .Setup(service => service.CreateUserAsync(It.IsAny<CreateUserDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityUserInfo?)null);

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Seed:AdminEmail"] = "admin@erbms.local"
        }).Build();

        var service = new UserService(userRepository.Object, roleRepository.Object, identityService.Object, configuration);
        var result = await service.CreateAsync(new CreateUserDto
        {
            FullName = "Test User",
            Email = "user@erbms.local",
            Password = "Password123!"
        }, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task AssignRoleAsync_AddsRoleAndCallsIdentity()
    {
        var identityService = new Mock<IIdentityService>();
        var userRepository = new Mock<IUserRepository>();
        var roleRepository = new Mock<IRoleRepository>();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@erbms.local",
            FullName = "Test User"
        };

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Admin"
        };

        userRepository
            .Setup(repo => repo.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        roleRepository
            .Setup(repo => repo.GetByNameAsync("Admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        userRepository
            .Setup(repo => repo.UpdateAsync(user, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        identityService
            .Setup(service => service.AssignRoleAsync(user.Id, "Admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Seed:AdminEmail"] = "admin@erbms.local"
        }).Build();

        var service = new UserService(userRepository.Object, roleRepository.Object, identityService.Object, configuration);
        var result = await service.AssignRoleAsync(new AssignRoleDto { UserId = user.Id, RoleName = "Admin" }, CancellationToken.None);

        Assert.True(result);
        identityService.Verify(service => service.AssignRoleAsync(user.Id, "Admin", It.IsAny<CancellationToken>()), Times.Once);
        userRepository.Verify(repo => repo.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }
}
