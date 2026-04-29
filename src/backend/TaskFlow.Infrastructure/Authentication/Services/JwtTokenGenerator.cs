using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Application.Common.Services;
using TaskFlow.Domain.User;

namespace TaskFlow.Infrastructure.Authentication.Services;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;
    private readonly UserManager<User> _userManager;
    private const string RefreshTokenPurpose = "RefreshToken";

    public JwtTokenGenerator(IOptions<JwtSettings> jwtSettings, UserManager<User> userManager)
    {
        _jwtSettings = jwtSettings.Value;
        _userManager = userManager;
    }

    public string GenerateToken(User user, IList<string> roles)
    {
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Name, user.UserName!),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var securityToken = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            claims: claims,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }

    public async Task<string> GenerateRefreshTokenAsync(User user)
    {
        var identityToken =
            await _userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, RefreshTokenPurpose);

        return $"{user.Id}.{identityToken}";
    }

    public async Task<bool> VerifyRefreshTokenAsync(User user, string token)
    {
        var actualToken = token.Split('.', 2)[1];
        return await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultProvider, RefreshTokenPurpose,
            actualToken);
    }
}