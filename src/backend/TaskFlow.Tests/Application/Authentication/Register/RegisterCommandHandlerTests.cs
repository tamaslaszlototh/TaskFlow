using ErrorOr;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using TaskFlow.Application.Authentication.Register;
using TaskFlow.Application.Common.Services;
using TaskFlow.Domain.User;

namespace TaskFlow.Tests.Application.Authentication.Register;

public class RegisterCommandHandlerTests
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _userManager = Substitute.For<UserManager<User>>(
            Substitute.For<IUserStore<User>>(),
            null, null, null, null, null, null, null, null);

        _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        _roleManager = Substitute.For<RoleManager<IdentityRole>>(
            Substitute.For<IRoleStore<IdentityRole>>(),
            null, null, null, null);

        _handler = new RegisterCommandHandler(_userManager, _jwtTokenGenerator, _roleManager);
    }

    [Fact]
    public async Task Handle_EmailExists_ReturnsConflict()
    {
        var command = new RegisterCommand("existing@example.com", "password123", "newuser");
        _userManager.FindByEmailAsync(command.Email).Returns(new User());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Code.Should().Be("UserAlreadyExists");
        await _userManager.Received(0).CreateAsync(Arg.Any<User>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_UserNameExists_ReturnsConflict()
    {
        var command = new RegisterCommand("new@example.com", "password123", "existinguser");
        _userManager.FindByEmailAsync(command.Email).Returns((User?)null);
        _userManager.FindByNameAsync(command.UserName).Returns(new User());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.FirstError.Type.Should().Be(ErrorType.Conflict);
        result.FirstError.Code.Should().Be("UserAlreadyExists");
        await _userManager.Received(0).CreateAsync(Arg.Any<User>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_UserCreationFails_ReturnsFailure()
    {
        var command = new RegisterCommand("new@example.com", "password123", "newuser");
        _userManager.FindByEmailAsync(command.Email).Returns((User?)null);
        _userManager.FindByNameAsync(command.UserName).Returns((User?)null);
        _userManager.CreateAsync(Arg.Any<User>(), command.Password)
            .Returns(IdentityResult.Failed());
        _roleManager.RoleExistsAsync("Member").Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.FirstError.Type.Should().Be(ErrorType.Failure);
        result.FirstError.Code.Should().Be("FailedToCreateUser");
    }

    [Fact]
    public async Task Handle_Success_ReturnsTokenAndRefreshToken()
    {
        var command = new RegisterCommand("new@example.com", "password123", "newuser");
        var user = new User { Id = "user-123", Email = command.Email, UserName = command.UserName };
        _userManager.FindByEmailAsync(command.Email).Returns((User?)null);
        _userManager.FindByNameAsync(command.UserName).Returns((User?)null);
        _userManager.CreateAsync(Arg.Any<User>(), command.Password)
            .Returns(IdentityResult.Success);
        _userManager.AddToRoleAsync(Arg.Any<User>(), "Member").Returns(IdentityResult.Success);
        _roleManager.RoleExistsAsync("Member").Returns(true);
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Member"]);
        _jwtTokenGenerator.GenerateToken(Arg.Any<User>(), Arg.Any<IList<string>>()).Returns("jwt-token");
        _jwtTokenGenerator.GenerateRefreshTokenAsync(Arg.Any<User>()).Returns("refresh-token");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        var success = result.Value;
        success.Token.Should().Be("jwt-token");
        success.RefreshToken.Should().Be("refresh-token");
        success.ExpiresInMinutes.Should().Be(30);
    }

    [Fact]
    public async Task Handle_CreatesUserWithCorrectData()
    {
        var command = new RegisterCommand("new@example.com", "password123", "newuser");
        _userManager.FindByEmailAsync(command.Email).Returns((User?)null);
        _userManager.FindByNameAsync(command.UserName).Returns((User?)null);
        _userManager.CreateAsync(Arg.Any<User>(), command.Password)
            .Returns(IdentityResult.Success);
        _userManager.AddToRoleAsync(Arg.Any<User>(), "Member").Returns(IdentityResult.Success);
        _roleManager.RoleExistsAsync("Member").Returns(true);
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Member"]);
        _jwtTokenGenerator.GenerateToken(Arg.Any<User>(), Arg.Any<IList<string>>()).Returns("jwt-token");
        _jwtTokenGenerator.GenerateRefreshTokenAsync(Arg.Any<User>()).Returns("refresh-token");

        await _handler.Handle(command, CancellationToken.None);

        await _userManager.Received(1).CreateAsync(
            Arg.Is<User>(u => u.Email == command.Email && u.UserName == command.UserName),
            command.Password);
    }

    [Fact]
    public async Task Handle_CreatesDefaultRoleWhenNotExists()
    {
        var command = new RegisterCommand("new@example.com", "password123", "newuser");
        _userManager.FindByEmailAsync(command.Email).Returns((User?)null);
        _userManager.FindByNameAsync(command.UserName).Returns((User?)null);
        _userManager.CreateAsync(Arg.Any<User>(), command.Password)
            .Returns(IdentityResult.Success);
        _userManager.AddToRoleAsync(Arg.Any<User>(), "Member").Returns(IdentityResult.Success);
        _roleManager.RoleExistsAsync("Member").Returns(false);
        _userManager.GetRolesAsync(Arg.Any<User>()).Returns(["Member"]);
        _jwtTokenGenerator.GenerateToken(Arg.Any<User>(), Arg.Any<IList<string>>()).Returns("jwt-token");
        _jwtTokenGenerator.GenerateRefreshTokenAsync(Arg.Any<User>()).Returns("refresh-token");

        await _handler.Handle(command, CancellationToken.None);

        await _roleManager.Received(1).CreateAsync(
            Arg.Is<IdentityRole>(r => r.Name == "Member"));
    }
}
