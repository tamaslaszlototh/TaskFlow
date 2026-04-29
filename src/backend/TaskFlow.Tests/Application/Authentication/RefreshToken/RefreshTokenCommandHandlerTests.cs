using ErrorOr;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using TaskFlow.Application.Authentication.RefreshToken;
using TaskFlow.Application.Common.Services;
using TaskFlow.Domain.User;

namespace TaskFlow.Tests.Application.Authentication.RefreshToken;

public class RefreshTokenCommandHandlerTests
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _userManager = Substitute.For<UserManager<User>>(
            Substitute.For<IUserStore<User>>(),
            null, null, null, null, null, null, null, null);

        _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        _handler = new RefreshTokenCommandHandler(_userManager, _jwtTokenGenerator);
    }

    [Fact]
    public async Task Handle_EmptyToken_ReturnsUnauthorized()
    {
        var command = new RefreshTokenCommand("");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        result.FirstError.Code.Should().Be("InvalidRefreshToken");
    }

    [Fact]
    public async Task Handle_TokenWithoutDot_ReturnsUnauthorized()
    {
        var command = new RefreshTokenCommand("nodotintoken");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        result.FirstError.Code.Should().Be("InvalidRefreshToken");
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsUnauthorized()
    {
        var command = new RefreshTokenCommand("user-123.token");
        _userManager.FindByIdAsync("user-123").Returns((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        result.FirstError.Code.Should().Be("InvalidRefreshToken");
    }

    [Fact]
    public async Task Handle_InvalidToken_ReturnsUnauthorized()
    {
        var command = new RefreshTokenCommand("user-123.invalidtoken");
        var user = new User { Id = "user-123" };
        _userManager.FindByIdAsync("user-123").Returns(user);
        _jwtTokenGenerator.VerifyRefreshTokenAsync(user, command.RefreshToken).Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.FirstError.Type.Should().Be(ErrorType.Unauthorized);
        result.FirstError.Code.Should().Be("InvalidRefreshToken");
    }

    [Fact]
    public async Task Handle_ValidToken_ReturnsNewTokens()
    {
        var command = new RefreshTokenCommand("user-123.validtoken");
        var user = new User { Id = "user-123", UserName = "testuser" };
        _userManager.FindByIdAsync("user-123").Returns(user);
        _jwtTokenGenerator.VerifyRefreshTokenAsync(user, command.RefreshToken).Returns(true);
        _userManager.GetRolesAsync(user).Returns(["Member"]);
        _jwtTokenGenerator.GenerateToken(user, Arg.Any<IList<string>>()).Returns("new-jwt-token");
        _jwtTokenGenerator.GenerateRefreshTokenAsync(user).Returns("new-refresh-token");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsError.Should().BeFalse();
        var success = result.Value;
        success.Token.Should().Be("new-jwt-token");
        success.RefreshToken.Should().Be("new-refresh-token");
        success.ExpiresInMinutes.Should().Be(30);
    }

    [Fact]
    public async Task Handle_VerifiesTokenCorrectly()
    {
        var command = new RefreshTokenCommand("user-456.someToken");
        var user = new User { Id = "user-456" };
        _userManager.FindByIdAsync("user-456").Returns(user);
        _jwtTokenGenerator.VerifyRefreshTokenAsync(user, command.RefreshToken).Returns(true);
        _userManager.GetRolesAsync(user).Returns(["Member"]);
        _jwtTokenGenerator.GenerateToken(user, Arg.Any<IList<string>>()).Returns("token");
        _jwtTokenGenerator.GenerateRefreshTokenAsync(user).Returns("refresh");

        await _handler.Handle(command, CancellationToken.None);

        await _jwtTokenGenerator.Received(1).VerifyRefreshTokenAsync(user, command.RefreshToken);
    }
}
