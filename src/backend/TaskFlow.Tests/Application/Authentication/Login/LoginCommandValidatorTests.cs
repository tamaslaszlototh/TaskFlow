using FluentAssertions;
using FluentValidation.TestHelper;
using TaskFlow.Application.Authentication.Login;

namespace TaskFlow.Tests.Application.Authentication.Login;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void ValidCommand_PassesValidation()
    {
        var command = new LoginCommand("validuser", "password123");

        var result = _validator.TestValidate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    public void UserName_TooShort_ReturnsValidationError(string userName)
    {
        var command = new LoginCommand(userName, "password123");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void UserName_TooLong_ReturnsValidationError()
    {
        var command = new LoginCommand("thisisaverylongusername", "password123");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void UserName_Empty_ReturnsValidationError()
    {
        var command = new LoginCommand("", "password123");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage("User name is required");
    }

    [Fact]
    public void Password_TooShort_ReturnsValidationError()
    {
        var command = new LoginCommand("validuser", "12345");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_TooLong_ReturnsValidationError()
    {
        var command = new LoginCommand("validuser", "thispasswordistoolong123");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_Empty_ReturnsValidationError()
    {
        var command = new LoginCommand("validuser", "");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required");
    }
}
