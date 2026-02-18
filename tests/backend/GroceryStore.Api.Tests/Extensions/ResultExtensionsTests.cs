using CQRS.CqrsResult;
using FluentAssertions;
using GroceryStore.Api.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GroceryStore.Api.Tests.Extensions;

public class ResultExtensionsTests
{
    #region ToHttpResult (non-generic)

    [Fact]
    public void ToHttpResult_WhenSuccess_ReturnsNoContent()
    {
        // Arrange
        var result = Result.Ok();

        // Act
        var httpResult = result.ToHttpResult();

        // Assert
        httpResult.Should().BeOfType<NoContent>();
    }

    [Fact]
    public void ToHttpResult_WhenNotFoundError_ReturnsProblemWith404()
    {
        // Arrange
        var result = Result.Fail(Error.NotFound("Item not found"));

        // Act
        var httpResult = result.ToHttpResult();

        // Assert
        var problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public void ToHttpResult_WhenConflictError_ReturnsProblemWith409()
    {
        // Arrange
        var result = Result.Fail(Error.Conflict("Duplicate detected"));

        // Act
        var httpResult = result.ToHttpResult();

        // Assert
        var problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public void ToHttpResult_WhenValidationError_ReturnsProblemWith422()
    {
        // Arrange
        var result = Result.Fail(Error.Validation("Name is required"));

        // Act
        var httpResult = result.ToHttpResult();

        // Assert
        var problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(422);
    }

    [Fact]
    public void ToHttpResult_WhenBadRequestError_ReturnsProblemWith400()
    {
        // Arrange
        var result = Result.Fail(Error.BadRequest("Invalid input"));

        // Act
        var httpResult = result.ToHttpResult();

        // Assert
        var problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public void ToHttpResult_WhenUnauthorizedError_ReturnsProblemWith401()
    {
        // Arrange
        var result = Result.Fail(Error.Unauthorized("Not authenticated"));

        // Act
        var httpResult = result.ToHttpResult();

        // Assert
        var problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(401);
    }

    [Fact]
    public void ToHttpResult_WhenForbiddenError_ReturnsProblemWith403()
    {
        // Arrange
        var result = Result.Fail(Error.Forbidden("Not allowed"));

        // Act
        var httpResult = result.ToHttpResult();

        // Assert
        var problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(403);
    }

    [Fact]
    public void ToHttpResult_WhenUnexpectedError_ReturnsProblemWith500()
    {
        // Arrange
        var result = Result.Fail(Error.Unexpected("Something went wrong"));

        // Act
        var httpResult = result.ToHttpResult();

        // Assert
        var problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region ToHttpResult (generic)

    [Fact]
    public void ToHttpResult_Generic_WhenSuccess_ReturnsOkWithValue()
    {
        // Arrange
        var result = Result.Ok("test-value");

        // Act
        var httpResult = result.ToHttpResult();

        // Assert
        var okResult = httpResult.Should().BeOfType<Ok<string>>().Subject;
        okResult.Value.Should().Be("test-value");
    }

    [Fact]
    public void ToHttpResult_Generic_WhenNotFound_ReturnsProblemWith404()
    {
        // Arrange
        var result = Result<string>.Fail(Error.NotFound("Not found"));

        // Act
        var httpResult = result.ToHttpResult();

        // Assert
        var problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region ToCreatedHttpResult

    [Fact]
    public void ToCreatedHttpResult_WhenSuccess_ReturnsCreatedAtRoute()
    {
        // Arrange
        var id = Guid.NewGuid();
        var result = Result.Ok(id);

        // Act
        var httpResult = result.ToCreatedHttpResult("GetById");

        // Assert
        httpResult.Should().BeAssignableTo<IResult>();
    }

    [Fact]
    public void ToCreatedHttpResult_WhenFailure_ReturnsProblem()
    {
        // Arrange
        var result = Result<Guid>.Fail(Error.Conflict("Already exists"));

        // Act
        var httpResult = result.ToCreatedHttpResult("GetById");

        // Assert
        var problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(409);
    }

    #endregion

    #region Multiple errors

    [Fact]
    public void ToHttpResult_WithMultipleErrors_ReturnsProblemWithFirstErrorStatusCode()
    {
        // Arrange
        var result = Result.Fail(
            Error.Validation("Name is required"),
            Error.Validation("Slug is required"));

        // Act
        var httpResult = result.ToHttpResult();

        // Assert
        var problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(422);
    }

    #endregion
}
