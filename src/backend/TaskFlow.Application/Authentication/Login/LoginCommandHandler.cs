using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Application.Common.Services;
using TaskFlow.Domain.User;

namespace TaskFlow.Application.Authentication.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ErrorOr<LoginCommandResult>>
{
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly UserManager<User> _userManager;

    public LoginCommandHandler(SignInManager<User> signInManager, IJwtTokenGenerator jwtTokenGenerator,
        UserManager<User> userManager)
    {
        _signInManager = signInManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _userManager = userManager;
    }

    public async Task<ErrorOr<LoginCommandResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);

        if (user is null)
            return Error.Failure("InvalidCredentials", "Invalid username or password");

        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            return Error.Failure("InvalidCredentials", "Invalid username or password");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var jwtToken = _jwtTokenGenerator.GenerateToken(user, roles);
        return new LoginCommandResult(jwtToken);
    }
}