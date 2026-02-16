using CQRS.CqrsResult;

namespace CQRS.Tests.CqrsResult;

public class ResultTests
{
    [Fact]
    public void Ok_ShouldCreateSuccessResult()
    {
        // Act
        var result = Result.Ok();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Fail_WithSingleError_ShouldCreateFailureResult()
    {
        // Arrange
        var error = Error.Validation("Test error");

        // Act
        var result = Result.Fail(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Should().Be(error);
    }

    [Fact]
    public void Fail_WithMultipleErrors_ShouldCreateFailureResult()
    {
        // Arrange
        var error1 = Error.Validation("Error 1");
        var error2 = Error.Validation("Error 2");

        // Act
        var result = Result.Fail(error1, error2);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(error1);
        result.Errors.Should().Contain(error2);
    }
}

public class ResultOfTTests
{
    [Fact]
    public void Ok_WithValue_ShouldCreateSuccessResult()
    {
        // Arrange
        var value = 42;

        // Act
        var result = Result<int>.Ok(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(value);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Ok_UsingStaticHelper_ShouldCreateSuccessResult()
    {
        // Arrange
        var value = "test";

        // Act
        var result = Result.Ok(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void Fail_WithSingleError_ShouldCreateFailureResult()
    {
        // Arrange
        var error = Error.NotFound("Not found");

        // Act
        var result = Result<string>.Fail(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Should().Be(error);
    }

    [Fact]
    public void Fail_WithMultipleErrors_ShouldCreateFailureResult()
    {
        // Arrange
        var error1 = Error.Validation("Error 1");
        var error2 = Error.Validation("Error 2");

        // Act
        var result = Result<int>.Fail(error1, error2);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void Value_OnFailedResult_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var result = Result<int>.Fail(Error.Validation("Error"));

        // Act
        var act = () => result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot access the value of a failed result.");
    }

    [Fact]
    public void TryGetValue_OnSuccessResult_ShouldReturnTrue()
    {
        // Arrange
        var value = "test value";
        var result = Result<string>.Ok(value);

        // Act
        var success = result.TryGetValue(out var retrievedValue);

        // Assert
        success.Should().BeTrue();
        retrievedValue.Should().Be(value);
    }

    [Fact]
    public void TryGetValue_OnFailureResult_ShouldReturnFalse()
    {
        // Arrange
        var result = Result<string>.Fail(Error.Validation("Error"));

        // Act
        var success = result.TryGetValue(out var retrievedValue);

        // Assert
        success.Should().BeFalse();
        retrievedValue.Should().BeNull();
    }

    [Fact]
    public void Match_OnSuccessResult_ShouldExecuteOnSuccess()
    {
        // Arrange
        var value = 10;
        var result = Result<int>.Ok(value);

        // Act
        var output = result.Match(
            onSuccess: v => $"Success: {v}",
            onFailure: errors => $"Failure: {errors.Count}");

        // Assert
        output.Should().Be("Success: 10");
    }

    [Fact]
    public void Match_OnFailureResult_ShouldExecuteOnFailure()
    {
        // Arrange
        var error1 = Error.Validation("Error 1");
        var error2 = Error.Validation("Error 2");
        var result = Result<int>.Fail(error1, error2);

        // Act
        var output = result.Match(
            onSuccess: v => $"Success: {v}",
            onFailure: errors => $"Failure: {errors.Count}");

        // Assert
        output.Should().Be("Failure: 2");
    }
}
