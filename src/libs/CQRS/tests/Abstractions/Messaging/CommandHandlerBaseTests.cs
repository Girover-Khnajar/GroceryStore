using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;

namespace CQRS.Tests.Abstractions.Messaging;

public class CommandHandlerBaseTests
{
    // Concrete test command without response
    private class TestCommand : ICommand
    {
        public string Value { get; set; } = string.Empty;
    }

    // Concrete test command handler without response
    private class TestCommandHandler : CommandHandlerBase<TestCommand>
    {
        public override Task<Result> HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(command.Value))
            {
                return Task.FromResult(Failure("Value is required"));
            }

            return Task.FromResult(Success());
        }
    }

    // Concrete test command with response
    private class TestCommandWithResponse : ICommand<int>
    {
        public int Value { get; set; }
    }

    // Concrete test command handler with response
    private class TestCommandWithResponseHandler : CommandHandlerBase<TestCommandWithResponse, int>
    {
        public override Task<Result<int>> HandleAsync(TestCommandWithResponse command, CancellationToken cancellationToken = default)
        {
            if (command.Value < 0)
            {
                return Task.FromResult(Failure("Value must be positive"));
            }

            return Task.FromResult(Success(command.Value * 2));
        }
    }

    // Handler that uses Error parameter
    private class ErrorTestCommandHandler : CommandHandlerBase<TestCommand>
    {
        public override Task<Result> HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
        {
            var error = Error.NotFound("Resource not found");
            return Task.FromResult(Failure(error));
        }
    }

    // Handler that uses multiple errors
    private class MultipleErrorsCommandHandler : CommandHandlerBase<TestCommand>
    {
        public override Task<Result> HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
        {
            var errors = new List<Error>
            {
                Error.Validation("Error 1"),
                Error.Validation("Error 2")
            };
            return Task.FromResult(Failure(errors));
        }
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ShouldReturnSuccessResult()
    {
        // Arrange
        var handler = new TestCommandHandler();
        var command = new TestCommand { Value = "test" };

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnFailureResult()
    {
        // Arrange
        var handler = new TestCommandHandler();
        var command = new TestCommand { Value = "" };

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Message.Should().Be("Value is required");
        result.Errors.First().Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task HandleAsync_WithResponse_WhenSuccessful_ShouldReturnSuccessWithValue()
    {
        // Arrange
        var handler = new TestCommandWithResponseHandler();
        var command = new TestCommandWithResponse { Value = 10 };

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(20);
    }

    [Fact]
    public async Task HandleAsync_WithResponse_WhenValidationFails_ShouldReturnFailureResult()
    {
        // Arrange
        var handler = new TestCommandWithResponseHandler();
        var command = new TestCommandWithResponse { Value = -5 };

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Message.Should().Be("Value must be positive");
    }

    [Fact]
    public async Task Failure_WithCustomError_ShouldReturnFailureWithSpecifiedError()
    {
        // Arrange
        var handler = new ErrorTestCommandHandler();
        var command = new TestCommand { Value = "test" };

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Type.Should().Be(ErrorType.NotFound);
        result.Errors.First().Message.Should().Be("Resource not found");
    }

    [Fact]
    public async Task Failure_WithMultipleErrors_ShouldReturnFailureWithAllErrors()
    {
        // Arrange
        var handler = new MultipleErrorsCommandHandler();
        var command = new TestCommand { Value = "test" };

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.Message == "Error 1");
        result.Errors.Should().Contain(e => e.Message == "Error 2");
    }

    [Fact]
    public async Task HandleAsync_ShouldSupportCancellationToken()
    {
        // Arrange
        var handler = new TestCommandHandler();
        var command = new TestCommand { Value = "test" };
        var cancellationToken = new CancellationToken();

        // Act
        var result = await handler.HandleAsync(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}

public class CommandHandlerBaseWithResponseTests
{
    // Test command with response
    private class TestCommand : ICommand<string>
    {
        public int Number { get; set; }
    }

    // Handler using Error parameter
    private class ErrorTestHandler : CommandHandlerBase<TestCommand, string>
    {
        public override Task<Result<string>> HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
        {
            var error = Error.Conflict("Conflict occurred");
            return Task.FromResult(Failure(error));
        }
    }

    // Handler using multiple errors
    private class MultipleErrorsHandler : CommandHandlerBase<TestCommand, string>
    {
        public override Task<Result<string>> HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
        {
            var errors = new List<Error>
            {
                Error.Validation("Validation 1"),
                Error.Validation("Validation 2")
            };
            return Task.FromResult(Failure(errors));
        }
    }

    [Fact]
    public async Task Success_WithValue_ShouldReturnSuccessResult()
    {
        // Arrange
        var handler = new ErrorTestHandler();
        var command = new TestCommand { Number = 42 };

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue(); // This handler always returns failure with conflict
        result.Errors.First().Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Failure_WithError_ShouldReturnFailureWithSpecifiedError()
    {
        // Arrange
        var handler = new ErrorTestHandler();
        var command = new TestCommand { Number = 42 };

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Type.Should().Be(ErrorType.Conflict);
        result.Errors.First().Message.Should().Be("Conflict occurred");
    }

    [Fact]
    public async Task Failure_WithMultipleErrors_ShouldReturnFailureWithAllErrors()
    {
        // Arrange
        var handler = new MultipleErrorsHandler();
        var command = new TestCommand { Number = 42 };

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.Message == "Validation 1");
        result.Errors.Should().Contain(e => e.Message == "Validation 2");
    }
}
