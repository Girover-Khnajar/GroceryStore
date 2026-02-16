using CQRS.CqrsResult;

namespace CQRS.Tests.CqrsResult;

public class ErrorTests
{
    [Fact]
    public void Failure_ShouldCreateErrorWithCorrectProperties()
    {
        // Arrange
        var code = "TEST_CODE";
        var message = "Test message";

        // Act
        var error = Error.Failure(code, message);

        // Assert
        error.Code.Should().Be(code);
        error.Message.Should().Be(message);
        error.Type.Should().Be(ErrorType.Failure);
        error.Metadata.Should().BeNull();
    }

    [Fact]
    public void Validation_ShouldCreateValidationError()
    {
        // Arrange
        var message = "Validation failed";

        // Act
        var error = Error.Validation(message);

        // Assert
        error.Code.Should().Be(ErrorCodes.ValidationError);
        error.Message.Should().Be(message);
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void BadRequest_ShouldCreateBadRequestError()
    {
        // Arrange
        var message = "Bad request";

        // Act
        var error = Error.BadRequest(message);

        // Assert
        error.Code.Should().Be(ErrorCodes.BadRequest);
        error.Message.Should().Be(message);
        error.Type.Should().Be(ErrorType.Badrequest);
    }

    [Fact]
    public void NotFound_ShouldCreateNotFoundError()
    {
        // Arrange
        var message = "Resource not found";

        // Act
        var error = Error.NotFound(message);

        // Assert
        error.Code.Should().Be(ErrorCodes.NotFound);
        error.Message.Should().Be(message);
        error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Conflict_ShouldCreateConflictError()
    {
        // Arrange
        var message = "Conflict occurred";

        // Act
        var error = Error.Conflict(message);

        // Assert
        error.Code.Should().Be(ErrorCodes.Conflict);
        error.Message.Should().Be(message);
        error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void Unauthorized_ShouldCreateUnauthorizedError()
    {
        // Arrange
        var message = "Unauthorized access";

        // Act
        var error = Error.Unauthorized(message);

        // Assert
        error.Code.Should().Be(ErrorCodes.Unauthorized);
        error.Message.Should().Be(message);
        error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public void Forbidden_ShouldCreateForbiddenError()
    {
        // Arrange
        var message = "Access forbidden";

        // Act
        var error = Error.Forbidden(message);

        // Assert
        error.Code.Should().Be(ErrorCodes.Forbidden);
        error.Message.Should().Be(message);
        error.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    public void Unexpected_ShouldCreateUnexpectedError()
    {
        // Arrange
        var message = "Unexpected error";

        // Act
        var error = Error.Unexpected(message);

        // Assert
        error.Code.Should().Be(ErrorCodes.InternalServerError);
        error.Message.Should().Be(message);
        error.Type.Should().Be(ErrorType.Unexpected);
    }

    [Fact]
    public void WithMetadata_ShouldAddMetadataToError()
    {
        // Arrange
        var error = Error.Validation("Test");
        var key = "TestKey";
        var value = "TestValue";

        // Act
        var errorWithMetadata = error.WithMetadata(key, value);

        // Assert
        errorWithMetadata.Metadata.Should().NotBeNull();
        errorWithMetadata.Metadata.Should().ContainKey(key);
        errorWithMetadata.Metadata![key].Should().Be(value);
    }

    [Fact]
    public void WithMetadata_OnErrorWithExistingMetadata_ShouldAddNewMetadata()
    {
        // Arrange
        var metadata = new Dictionary<string, object> { { "Key1", "Value1" } };
        var error = Error.Failure("CODE", "Message", metadata);

        // Act
        var errorWithMetadata = error.WithMetadata("Key2", "Value2");

        // Assert
        errorWithMetadata.Metadata.Should().NotBeNull();
        errorWithMetadata.Metadata.Should().HaveCount(2);
        errorWithMetadata.Metadata!["Key1"].Should().Be("Value1");
        errorWithMetadata.Metadata["Key2"].Should().Be("Value2");
    }

    [Fact]
    public void WithMetadata_ShouldNotModifyOriginalError()
    {
        // Arrange
        var error = Error.Validation("Test");

        // Act
        var errorWithMetadata = error.WithMetadata("Key", "Value");

        // Assert
        error.Metadata.Should().BeNull();
        errorWithMetadata.Metadata.Should().NotBeNull();
    }

    [Fact]
    public void Equals_WithSameCodeAndType_ShouldReturnTrue()
    {
        // Arrange
        var error1 = Error.Validation("Message 1");
        var error2 = Error.Validation("Message 2");

        // Act & Assert
        error1.Equals(error2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentCode_ShouldReturnFalse()
    {
        // Arrange
        var error1 = Error.Validation("Message");
        var error2 = Error.BadRequest("Message");

        // Act & Assert
        error1.Equals(error2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        var error1 = Error.Failure("CODE", "Message", null);
        var error2 = Error.Unexpected("Message");

        // Act & Assert
        error1.Equals(error2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var error = Error.Validation("Message");

        // Act & Assert
        error.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithSameReference_ShouldReturnTrue()
    {
        // Arrange
        var error = Error.Validation("Message");

        // Act & Assert
        error.Equals(error).Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_WithEqualErrors_ShouldReturnTrue()
    {
        // Arrange
        var error1 = Error.NotFound("Message 1");
        var error2 = Error.NotFound("Message 2");

        // Act & Assert
        (error1 == error2).Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_WithDifferentErrors_ShouldReturnTrue()
    {
        // Arrange
        var error1 = Error.NotFound("Message");
        var error2 = Error.Conflict("Message");

        // Act & Assert
        (error1 != error2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameCodeAndType_ShouldReturnSameHashCode()
    {
        // Arrange
        var error1 = Error.Validation("Message 1");
        var error2 = Error.Validation("Message 2");

        // Act & Assert
        error1.GetHashCode().Should().Be(error2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var error = Error.NotFound("Resource not found");

        // Act
        var result = error.ToString();

        // Assert
        result.Should().Be($"[{ErrorType.NotFound}] {ErrorCodes.NotFound}: Resource not found");
    }

    [Fact]
    public void Factory_WithMetadata_ShouldCreateErrorWithMetadata()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            { "Field", "Email" },
            { "AttemptedValue", "invalid@" }
        };

        // Act
        var error = Error.Validation("Invalid email format", metadata);

        // Assert
        error.Metadata.Should().NotBeNull();
        error.Metadata.Should().HaveCount(2);
        error.Metadata!["Field"].Should().Be("Email");
        error.Metadata["AttemptedValue"].Should().Be("invalid@");
    }
}
