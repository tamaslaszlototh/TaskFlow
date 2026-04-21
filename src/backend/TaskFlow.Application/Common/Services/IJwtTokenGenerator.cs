using TaskFlow.Domain.User;

namespace TaskFlow.Application.Common.Services;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}