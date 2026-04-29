using ErrorOr;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Controllers;

namespace TaskFlow.Tests.Api.Controllers;

public class ApiControllerTests
{
    private readonly TestApiController _controller;

    public ApiControllerTests()
    {
        _controller = new TestApiController();
    }

    [Fact]
    public void Problem_EmptyErrors_Returns500()
    {
        var errors = new List<Error>();

        var result = _controller.TestProblem(errors);

        var objectResult = result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void Problem_AllValidationErrors_Returns400()
    {
        var errors = new List<Error>
        {
            Error.Validation("field1", "Error 1"),
            Error.Validation("field2", "Error 2")
        };

        var result = _controller.TestProblem(errors);

        var objectResult = result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    [Fact]
    public void Problem_AllUnauthorizedErrors_Returns401()
    {
        var errors = new List<Error>
        {
            Error.Unauthorized("auth1", "Unauthorized"),
            Error.Unauthorized("auth2", "Another unauthorized")
        };

        var result = _controller.TestProblem(errors);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public void Problem_ConflictError_Returns409()
    {
        var errors = new List<Error> { Error.Conflict("conflict", "Resource conflict") };

        var result = _controller.TestProblem(errors);

        var objectResult = result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public void Problem_NotFoundError_Returns404()
    {
        var errors = new List<Error> { Error.NotFound("notfound", "Resource not found") };

        var result = _controller.TestProblem(errors);

        var objectResult = result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public void Problem_ForbiddenError_Returns403()
    {
        var errors = new List<Error> { Error.Forbidden("forbidden", "Access denied") };

        var result = _controller.TestProblem(errors);

        var objectResult = result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public void Problem_FailureError_Returns500()
    {
        var errors = new List<Error> { Error.Failure("failure", "Something failed") };

        var result = _controller.TestProblem(errors);

        var objectResult = result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void Problem_MixedErrors_ReturnsStatusCodeForFirstError()
    {
        var errors = new List<Error>
        {
            Error.Conflict("conflict", "Resource conflict"),
            Error.Validation("field1", "Error 1")
        };

        var result = _controller.TestProblem(errors);

        var objectResult = result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }
}

public class TestApiController : ApiController
{
    public IActionResult TestProblem(List<Error> errors)
    {
        return Problem(errors);
    }
}
