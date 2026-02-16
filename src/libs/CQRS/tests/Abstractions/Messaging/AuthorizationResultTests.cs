using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;

namespace CQRS.Tests.Abstractions.Messaging;

public class AuthorizationResultTests
{
    [Fact]
    public void Success_ShouldCreateAuthorizedResult()
    {
        // Act
        var result = AuthorizationResult.Success();

        // Assert
        result.IsAuthorized.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Fail_WithSingleError_ShouldCreateUnauthorizedResult()
    {
        // Arrange
        var error = Error.Validation("Test error");

        // Act
        var result = AuthorizationResult.Fail(error);

        // Assert
        result.IsAuthorized.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Should().Be(error);
    }

    [Fact]
    public void Fail_WithMultipleErrors_ShouldCreateUnauthorizedResult()
    {
        // Arrange
        var error1 = Error.Validation("Error 1");
        var error2 = Error.Validation("Error 2");

        // Act
        var result = AuthorizationResult.Fail(error1, error2);

        // Assert
        result.IsAuthorized.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(error1);
        result.Errors.Should().Contain(error2);
    }

    [Fact]
    public void Fail_WithEnumerableErrors_ShouldCreateUnauthorizedResult()
    {
        // Arrange
        var errors = new List<Error>
        {
            Error.Validation("Error 1"),
            Error.Validation("Error 2"),
            Error.Validation("Error 3")
        };

        // Act
        var result = AuthorizationResult.Fail(errors);

        // Assert
        result.IsAuthorized.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
    }

    [Fact]
    public void Unauthorized_WithDefaultMessage_ShouldCreateUnauthorizedError()
    {
        // Act
        var result = AuthorizationResult.Unauthorized();

        // Assert
        result.IsAuthorized.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Type.Should().Be(ErrorType.Unauthorized);
        result.Errors.First().Message.Should().Be("Unauthorized");
    }

    [Fact]
    public void Unauthorized_WithCustomMessage_ShouldCreateUnauthorizedError()
    {
        // Arrange
        var message = "Access denied for this resource";

        // Act
        var result = AuthorizationResult.Unauthorized(message);

        // Assert
        result.IsAuthorized.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Type.Should().Be(ErrorType.Unauthorized);
        result.Errors.First().Message.Should().Be(message);
    }

    [Fact]
    public void Forbidden_WithDefaultMessage_ShouldCreateForbiddenError()
    {
        // Act
        var result = AuthorizationResult.Forbidden();

        // Assert
        result.IsAuthorized.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Type.Should().Be(ErrorType.Forbidden);
        result.Errors.First().Message.Should().Be("Forbidden");
    }

    [Fact]
    public void Forbidden_WithCustomMessage_ShouldCreateForbiddenError()
    {
        // Arrange
        var message = "You do not have permission to perform this action";

        // Act
        var result = AuthorizationResult.Forbidden(message);

        // Assert
        result.IsAuthorized.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Type.Should().Be(ErrorType.Forbidden);
        result.Errors.First().Message.Should().Be(message);
    }

    [Fact]
    public void Unauthorized_ShouldHaveCorrectErrorCode()
    {
        // Act
        var result = AuthorizationResult.Unauthorized();

        // Assert
        result.Errors.First().Code.Should().Be(ErrorCodes.Unauthorized);
    }

    [Fact]
    public void Forbidden_ShouldHaveCorrectErrorCode()
    {
        // Act
        var result = AuthorizationResult.Forbidden();

        // Assert
        result.Errors.First().Code.Should().Be(ErrorCodes.Forbidden);
    }

    [Fact]
    public void Errors_ShouldBeReadOnlyList()
    {
        // Arrange
        var result = AuthorizationResult.Fail(Error.Validation("Error"));

        // Act
        var errors = result.Errors;

        // Assert
        errors.Should().BeAssignableTo<IReadOnlyList<Error>>();
    }

    [Fact]
    public void Success_ErrorsShouldBeEmptyArray()
    {
        // Act
        var result = AuthorizationResult.Success();

        // Assert
        result.Errors.Should().BeEmpty();
        result.Errors.Should().BeSameAs(Array.Empty<Error>());
    }
}
