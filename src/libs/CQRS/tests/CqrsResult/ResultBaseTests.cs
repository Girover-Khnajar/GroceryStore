using CQRS.CqrsResult;

namespace CQRS.Tests.CqrsResult;

public class ResultBaseTests
{
    [Fact]
    public void Constructor_WithSuccessAndNoErrors_ShouldCreateValidResult()
    {
        // Act
        var result = Result.Ok();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithFailureAndErrors_ShouldCreateValidResult()
    {
        // Arrange
        var error = Error.Validation("Test error");

        // Act
        var result = Result.Fail(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public void Constructor_WithSuccessAndErrors_ShouldThrowInvalidOperationException()
    {
        // Arrange & Act & Assert
        // This is tested indirectly through the Result.Ok() and Result.Fail() methods
        // ResultBase constructor validates this internally
        var act = () =>
        {
            // Attempting to create a success result with errors through reflection
            var resultType = typeof(Result);
            var constructor = resultType.GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)[0];
            return constructor.Invoke([true, new[] { Error.Validation("Error") }]);
        };

        act.Should().Throw<System.Reflection.TargetInvocationException>()
            .WithInnerException<InvalidOperationException>()
            .WithMessage("Successful result cannot contain errors");
    }

    [Fact]
    public void Constructor_WithFailureAndNoErrors_ShouldThrowInvalidOperationException()
    {
        // Arrange & Act & Assert
        var act = () =>
        {
            // Attempting to create a failure result without errors through reflection
            var resultType = typeof(Result);
            var constructor = resultType.GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)[0];
            return constructor.Invoke([false, Array.Empty<Error>()]);
        };

        act.Should().Throw<System.Reflection.TargetInvocationException>()
            .WithInnerException<InvalidOperationException>()
            .WithMessage("Failed result must contain at least one error");
    }

    [Fact]
    public void Errors_ShouldReturnReadOnlyList()
    {
        // Arrange
        var error1 = Error.Validation("Error 1");
        var error2 = Error.Validation("Error 2");
        var result = Result.Fail(error1, error2);

        // Act
        var errors = result.Errors;

        // Assert
        errors.Should().BeAssignableTo<IReadOnlyList<Error>>();
        errors.Should().HaveCount(2);
    }

    [Fact]
    public void Errors_WithNullInput_ShouldReturnEmptyList()
    {
        // Act
        var result = Result.Ok();

        // Assert
        result.Errors.Should().NotBeNull();
        result.Errors.Should().BeEmpty();
    }
}
