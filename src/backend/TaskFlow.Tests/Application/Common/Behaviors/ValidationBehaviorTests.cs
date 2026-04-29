using ErrorOr;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NSubstitute;
using TaskFlow.Application.Common.Behaviors;
using MediatR;

namespace TaskFlow.Tests.Application.Common.Behaviors;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_NoValidator_PassesThrough()
    {
        var request = Substitute.For<IRequest<ErrorOr<string>>>();
        var next = Substitute.For<RequestHandlerDelegate<ErrorOr<string>>>();
        next(Arg.Any<CancellationToken>()).Returns("success");

        var behavior = new ValidationBehavior<IRequest<ErrorOr<string>>, ErrorOr<string>>(null);

        var result = await behavior.Handle(request, next, CancellationToken.None);

        result.Value.Should().Be("success");
    }

    [Fact]
    public async Task Handle_ValidRequest_PassesThrough()
    {
        var request = new TestRequest("valid");
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.ValidateAsync(Arg.Any<TestRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());
        var next = Substitute.For<RequestHandlerDelegate<ErrorOr<string>>>();
        next(Arg.Any<CancellationToken>()).Returns("success");

        var behavior = new ValidationBehavior<TestRequest, ErrorOr<string>>(validator);

        var result = await behavior.Handle(request, next, CancellationToken.None);

        result.Value.Should().Be("success");
        await validator.Received(1).ValidateAsync(request, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InvalidRequest_ReturnsValidationErrors()
    {
        var request = new TestRequest("invalid");
        var validationFailures = new List<ValidationFailure>
        {
            new("PropertyName1", "Error message 1"),
            new("PropertyName2", "Error message 2")
        };
        var validator = Substitute.For<IValidator<TestRequest>>();
        validator.ValidateAsync(Arg.Any<TestRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(validationFailures));
        var next = Substitute.For<RequestHandlerDelegate<ErrorOr<string>>>();

        var behavior = new ValidationBehavior<TestRequest, ErrorOr<string>>(validator);

        var result = await behavior.Handle(request, next, CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.Errors.Should().HaveCount(2);
        result.Errors[0].Code.Should().Be("PropertyName1");
        result.Errors[0].Description.Should().Be("Error message 1");
        result.Errors[1].Code.Should().Be("PropertyName2");
        result.Errors[1].Description.Should().Be("Error message 2");
    }
}

public record TestRequest(string Value) : IRequest<ErrorOr<string>>;
