using ErrorOr;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using TaskFlow.Application.Authentication.Login;
using TaskFlow.Application.Common.Services;
using TaskFlow.Domain.User;

namespace TaskFlow.Tests.Application.Authentication.Login;

public class LoginCommandHandlerTests
{
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly UserManager<User> _userManager;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userManager = Substitute.For<UserManager<User>>(
            Substitute.For<IUserStore<User>>(),
            null, null, null, null, null, null, null, null);

        _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        _signInManager = Substitute.For<SignInManager<User>>(
            _userManager,
            Substitute.For<IHttpContextAccessor>(),
            Substitute.For<IUserClaimsPrincipalFactory<User>>(),
            null, null, null, null);

        _handler = new LoginCommandHandler(_signInManager, _jwtTokenGenerator, _userManager);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsFailure()
    {
        var command = new LoginCommand("nonexistent", "password123");
        _userManager.FindByNameAsync(command.UserName).Returns((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.FirstError.Type.Should().Be(ErrorType.Failure);
        result.FirstError.Code.Should().Be("InvalidCredentials");
    }

    [Fact]
    public async Task Handle_InvalidPassword_ReturnsFailure()
    {
        var command = new LoginCommand("testuser", "wrongpassword");
        var user = new User { Id = "user-123", UserName = "testuser" };
        _userManager.FindByNameAsync(command.UserName).Returns(user);
        _signInManager.CheckPasswordSignInAsync(user, command.Password, true)
            .Returns(SignInResult.Failed);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.FirstError.Type.Should().Be(ErrorType.Failure);
        result.FirstError.Code.Should().Be("InvalidCredentials");
    }

    [Fact]
    public async Task Handle_Success_ReturnsTokenAndRefreshToken()
    {
        var command = new LoginCommand("testuser", "password123");
        var user = new User { Id = "user-123", UserName = "testuser" };
        _userManager.FindByNameAsync(command.UserName).Returns(user);
        _signInManager.CheckPasswordSignInAsync(user, command.Password, true)
            .Returns(SignInResult.Success);
        _userManager.GetRolesAsync(user).Returns(["Member"]);
        _jwtTokenGenerator.GenerateToken(user, Arg.Any<IList<string>>()).Returns("jwt-token");
        _jwtTokenGenerator.GenerateRefreshTokenAsync(user).Returns("refresh-token");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        var success = result.Value;
        success.Token.Should().Be("jwt-token");
        success.RefreshToken.Should().Be("refresh-token");
        success.ExpiresInMinutes.Should().Be(30);
    }

    [Fact]
    public async Task Handle_Success_ReturnsUserRoles()
    {
        var command = new LoginCommand("testuser", "password123");
        var user = new User { Id = "user-123", UserName = "testuser" };
        _userManager.FindByNameAsync(command.UserName).Returns(user);
        _signInManager.CheckPasswordSignInAsync(user, command.Password, true)
            .Returns(SignInResult.Success);
        _userManager.GetRolesAsync(user).Returns(["Admin", "Member"]);
        _jwtTokenGenerator.GenerateToken(user, Arg.Any<IList<string>>()).Returns("jwt-token");
        _jwtTokenGenerator.GenerateRefreshTokenAsync(user).Returns("refresh-token");

        await _handler.Handle(command, CancellationToken.None);

        await _userManager.Received(1).GetRolesAsync(user);
        _jwtTokenGenerator.Received(1).GenerateToken(user, Arg.Is<IList<string>>(r => r.Contains("Admin")));
    }
}
