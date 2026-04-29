using FluentAssertions;
using FluentValidation.TestHelper;
using TaskFlow.Application.Authentication.Register;

namespace TaskFlow.Tests.Application.Authentication.Register;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void ValidCommand_PassesValidation()
    {
        var command = new RegisterCommand("test@example.com", "password123", "testuser");

        var result = _validator.TestValidate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    public void UserName_TooShort_ReturnsValidationError(string userName)
    {
        var command = new RegisterCommand("test@example.com", "password123", userName);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void UserName_TooLong_ReturnsValidationError()
    {
        var command = new RegisterCommand("test@example.com", "password123", "thisisaverylongusername");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void UserName_Empty_ReturnsValidationError()
    {
        var command = new RegisterCommand("test@example.com", "password123", "");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage("User name is required");
    }

    [Fact]
    public void Password_TooShort_ReturnsValidationError()
    {
        var command = new RegisterCommand("test@example.com", "12345", "validuser");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_TooLong_ReturnsValidationError()
    {
        var command = new RegisterCommand("test@example.com", "thispasswordistoolong123", "validuser");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_Empty_ReturnsValidationError()
    {
        var command = new RegisterCommand("test@example.com", "", "validuser");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData("notanemail")]
    [InlineData("missingatsign.com")]
    [InlineData("@missinglocalpart.com")]
    public void Email_InvalidFormat_ReturnsValidationError(string email)
    {
        var command = new RegisterCommand(email, "password123", "validuser");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_Empty_ReturnsValidationError()
    {
        var command = new RegisterCommand("", "password123", "validuser");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }
}
