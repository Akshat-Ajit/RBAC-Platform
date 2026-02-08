using ERBMS.Application.DTOs;
using ERBMS.Application.Interfaces;
using ERBMS.Application.Services;
using ERBMS.Domain.Entities;
using ERBMS.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Collections.Generic;

namespace ERBMS.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenCredentialsInvalid()
    {
        var identityService = new Mock<IIdentityService>();
        var tokenService = new Mock<ITokenService>();
        var userRepository = new Mock<IUserRepository>();

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Seed:AdminEmail"] = "admin@erbms.local"
        }).Build();

        identityService
            .Setup(service => service.ValidateCredentialsAsync("user@erbms.local", "bad", It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityUserInfo?)null);

        var service = new AuthService(identityService.Object, tokenService.Object, userRepository.Object, configuration);
        var result = await service.LoginAsync(new LoginDto { Email = "user@erbms.local", Password = "bad" }, CancellationToken.None);

        Assert.Equal(AuthLoginStatus.InvalidCredentials, result.Status);
    }

    [Fact]
    public async Task LoginAsync_ReturnsTokens_WhenCredentialsValid()
    {
        var identityService = new Mock<IIdentityService>();
        var tokenService = new Mock<ITokenService>();
        var userRepository = new Mock<IUserRepository>();

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Seed:AdminEmail"] = "admin@erbms.local"
        }).Build();

        var identity = new IdentityUserInfo
        {
            UserId = Guid.NewGuid(),
            Email = "user@erbms.local",
            FullName = "Test User",
            Roles = new[] { "User" }
        };

        identityService
            .Setup(service => service.ValidateCredentialsAsync(identity.Email, "good", It.IsAny<CancellationToken>()))
            .ReturnsAsync(identity);

        userRepository
            .Setup(repo => repo.GetByIdAsync(identity.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = identity.UserId, Email = identity.Email, FullName = identity.FullName, IsActive = true });

        tokenService
            .Setup(service => service.GenerateAccessToken(identity.UserId, identity.Email, identity.Roles))
            .Returns(new AccessTokenResult { Token = "access-token", ExpiresAt = DateTime.UtcNow.AddMinutes(60) });

        tokenService
            .Setup(service => service.GenerateRefreshToken())
            .Returns("refresh-token");

        identityService
            .Setup(service => service.StoreRefreshTokenAsync(identity.UserId, "refresh-token", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new AuthService(identityService.Object, tokenService.Object, userRepository.Object, configuration);
        var result = await service.LoginAsync(new LoginDto { Email = identity.Email, Password = "good" }, CancellationToken.None);

        Assert.Equal(AuthLoginStatus.Success, result.Status);
        Assert.NotNull(result.Response);
        Assert.Equal("access-token", result.Response!.AccessToken);
        Assert.Equal("refresh-token", result.Response.RefreshToken);
        Assert.Equal(identity.Email, result.Response.User.Email);

        identityService.Verify(
            service => service.StoreRefreshTokenAsync(identity.UserId, "refresh-token", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
