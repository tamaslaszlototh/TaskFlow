using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Application.Common.Services;
using TaskFlow.Domain.User;

namespace TaskFlow.Application.Authentication.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ErrorOr<RegisterCommandResult>>
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly RoleManager<IdentityRole> _roleManager;

    public RegisterCommandHandler(UserManager<User> userManager, IJwtTokenGenerator jwtTokenGenerator,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _roleManager = roleManager;
    }

    public async Task<ErrorOr<RegisterCommandResult>> Handle(RegisterCommand request,
        CancellationToken cancellationToken)
    {
        if (await _userManager.FindByEmailAsync(request.Email) is not null)
            return Error.Conflict("UserAlreadyExists", "User already exists with this email.");

        if (await _userManager.FindByNameAsync(request.UserName) is not null)
            return Error.Conflict("UserAlreadyExists", "User already exists with this username.");

        var user = new User
        {
            CreatedAt = DateTime.UtcNow,
            Email = request.Email,
            UserName = request.UserName
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        const string defaultRole = "Member";

        if (!await _roleManager.RoleExistsAsync(defaultRole))
        {
            await _roleManager.CreateAsync(new IdentityRole(defaultRole));
        }

        await _userManager.AddToRoleAsync(user, defaultRole);

        if (!result.Succeeded)
        {
            return Error.Failure("FailedToCreateUser", "Failed to create user.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenGenerator.GenerateToken(user, roles);
        var refreshToken = await _jwtTokenGenerator.GenerateRefreshTokenAsync(user);

        return new RegisterCommandResult(token, refreshToken, 30);
    }
}