using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NSubstitute;
using TaskFlow.Domain.User;
using TaskFlow.Infrastructure.Authentication;
using TaskFlow.Infrastructure.Authentication.Services;

namespace TaskFlow.Tests.Infrastructure.Authentication.Services;

public class JwtTokenGeneratorTests
{
    private readonly UserManager<User> _userManager;
    private readonly JwtTokenGenerator _generator;

    public JwtTokenGeneratorTests()
    {
        _userManager = Substitute.For<UserManager<User>>(
            Substitute.For<IUserStore<User>>(),
            null, null, null, null, null, null, null, null);

        var jwtSettings = new JwtSettings
        {
            Secret = "this-is-a-secret-key-that-is-long-enough-for-hmacsha256",
            ExpiryMinutes = 60,
            Issuer = "TestIssuer",
            Audience = "TestAudience"
        };
        var options = Options.Create(jwtSettings);

        _generator = new JwtTokenGenerator(options, _userManager);
    }

    [Fact]
    public void GenerateToken_ContainsCorrectClaims()
    {
        var user = new User { Id = "user-123", UserName = "testuser", Email = "test@example.com" };
        var roles = new List<string> { "Admin", "Member" };

        var token = _generator.GenerateToken(user, roles);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Name && c.Value == user.UserName);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
    }

    [Fact]
    public void GenerateToken_ContainsRoleClaims()
    {
        var user = new User { Id = "user-123", UserName = "testuser", Email = "test@example.com" };
        var roles = new List<string> { "Admin", "Member" };

        var token = _generator.GenerateToken(user, roles);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Member");
    }

    [Fact]
    public void GenerateToken_HasCorrectIssuerAndAudience()
    {
        var user = new User { Id = "user-123", UserName = "testuser", Email = "test@example.com" };
        var roles = new List<string> { "Member" };

        var token = _generator.GenerateToken(user, roles);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Issuer.Should().Be("TestIssuer");
        jwtToken.Audiences.Should().Contain("TestAudience");
    }

    [Fact]
    public void GenerateToken_HasCorrectExpiry()
    {
        var user = new User { Id = "user-123", UserName = "testuser", Email = "test@example.com" };
        var roles = new List<string> { "Member" };

        var token = _generator.GenerateToken(user, roles);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_ReturnsCorrectFormat()
    {
        var user = new User { Id = "user-123", UserName = "testuser", Email = "test@example.com" };
        _userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, "RefreshToken")
            .Returns("generated-token");

        var result = await _generator.GenerateRefreshTokenAsync(user);

        result.Should().Be("user-123.generated-token");
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_GeneratesUserToken()
    {
        var user = new User { Id = "user-123", UserName = "testuser", Email = "test@example.com" };
        _userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, "RefreshToken")
            .Returns("generated-token");

        await _generator.GenerateRefreshTokenAsync(user);

        await _userManager.Received(1).GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, "RefreshToken");
    }

    [Fact]
    public async Task VerifyRefreshTokenAsync_VerifiesToken()
    {
        var user = new User { Id = "user-123", UserName = "testuser", Email = "test@example.com" };
        var token = "user-123.some-token-value";
        _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, "RefreshToken", "some-token-value")
            .Returns(true);

        var result = await _generator.VerifyRefreshTokenAsync(user, token);

        result.Should().BeTrue();
        await _userManager.Received(1).VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, "RefreshToken", "some-token-value");
    }

    [Fact]
    public async Task VerifyRefreshTokenAsync_ReturnsFalseForInvalidToken()
    {
        var user = new User { Id = "user-123", UserName = "testuser", Email = "test@example.com" };
        var token = "user-123.invalid-token";
        _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, "RefreshToken", "invalid-token")
            .Returns(false);

        var result = await _generator.VerifyRefreshTokenAsync(user, token);

        result.Should().BeFalse();
    }
}
