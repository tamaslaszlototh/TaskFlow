using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Application.Authentication.Login;
using TaskFlow.Application.Common.Services;
using TaskFlow.Domain.User;

namespace TaskFlow.Application.Authentication.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ErrorOr<LoginCommandResult>>
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RefreshTokenCommandHandler(UserManager<User> userManager, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<ErrorOr<LoginCommandResult>> Handle(RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromRefreshToken(request.RefreshToken);
        if (string.IsNullOrEmpty(userId))
        {
            return Error.Unauthorized("InvalidRefreshToken", "Invalid or expired refresh token");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Error.Unauthorized("InvalidRefreshToken", "Invalid or expired refresh token");
        }

        var isValid = await _jwtTokenGenerator.VerifyRefreshTokenAsync(user, request.RefreshToken);
        if (!isValid)
        {
            return Error.Unauthorized("InvalidRefreshToken", "Invalid or expired refresh token");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var jwtToken = _jwtTokenGenerator.GenerateToken(user, roles);
        var newRefreshToken = await _jwtTokenGenerator.GenerateRefreshTokenAsync(user);

        return new LoginCommandResult(jwtToken, newRefreshToken, 30);
    }

    private static string? GetUserIdFromRefreshToken(string token)
    {
        var parts = token.Split('.', 2);
        return parts.Length >= 2 ? parts[0] : null;
    }
}