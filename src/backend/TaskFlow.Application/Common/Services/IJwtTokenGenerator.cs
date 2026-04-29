using TaskFlow.Domain.User;

namespace TaskFlow.Application.Common.Services;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, IList<string> roles);
    Task<string> GenerateRefreshTokenAsync(User user);
    Task<bool> VerifyRefreshTokenAsync(User user, string token);
}