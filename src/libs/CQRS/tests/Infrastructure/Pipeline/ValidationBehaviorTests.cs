using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using CQRS.Infrastructure.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace CQRS.Tests.Infrastructure.Pipeline;

public class ValidationBehaviorTests
{
    private class TestCommand : ICommand
    {
        public string Value { get; set; } = string.Empty;
    }

    private class SuccessValidator : IValidator<TestCommand>
    {
        public Task<IReadOnlyList<Error>> ValidateAsync(TestCommand message, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Error>>(Array.Empty<Error>());
        }
    }

    private class FailValidator : IValidator<TestCommand>
    {
        public Task<IReadOnlyList<Error>> ValidateAsync(TestCommand message, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Error>>(new[] { Error.Validation("Value is required") });
        }
    }

    private class MultipleErrorsValidator : IValidator<TestCommand>
    {
        public Task<IReadOnlyList<Error>> ValidateAsync(TestCommand message, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Error>>(new[]
            {
                Error.Validation("Error 1"),
                Error.Validation("Error 2")
            });
        }
    }

    [Fact]
    public async Task HandleAsync_WithNoValidators_ShouldCallNext()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var behavior = new ValidationBehavior<TestCommand, Result>(serviceProvider);
        var command = new TestCommand { Value = "test" };
        var nextCalled = false;

        MessageHandlerDelegate<Result> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Ok());
        };

        // Act
        var result = await behavior.HandleAsync(command, next);

        // Assert
        nextCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithSuccessfulValidator_ShouldCallNext()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IValidator<TestCommand>, SuccessValidator>();
        var serviceProvider = services.BuildServiceProvider();
        var behavior = new ValidationBehavior<TestCommand, Result>(serviceProvider);
        var command = new TestCommand { Value = "test" };
        var nextCalled = false;

        MessageHandlerDelegate<Result> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Ok());
        };

        // Act
        var result = await behavior.HandleAsync(command, next);

        // Assert
        nextCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithFailingValidator_ShouldShortCircuit()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IValidator<TestCommand>, FailValidator>();
        var serviceProvider = services.BuildServiceProvider();
        var behavior = new ValidationBehavior<TestCommand, Result>(serviceProvider);
        var command = new TestCommand { Value = "" };
        var nextCalled = false;

        MessageHandlerDelegate<Result> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Ok());
        };

        // Act
        var result = await behavior.HandleAsync(command, next);

        // Assert
        nextCalled.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Type.Should().Be(ErrorType.Validation);
        result.Errors.First().Message.Should().Be("Value is required");
    }

    [Fact]
    public async Task HandleAsync_WithMultipleValidators_AllPass_ShouldCallNext()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IValidator<TestCommand>, SuccessValidator>();
        services.AddSingleton<IValidator<TestCommand>, SuccessValidator>();
        var serviceProvider = services.BuildServiceProvider();
        var behavior = new ValidationBehavior<TestCommand, Result>(serviceProvider);
        var command = new TestCommand { Value = "test" };
        var nextCalled = false;

        MessageHandlerDelegate<Result> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Ok());
        };

        // Act
        var result = await behavior.HandleAsync(command, next);

        // Assert
        nextCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithMultipleValidators_OneFails_ShouldShortCircuit()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IValidator<TestCommand>, SuccessValidator>();
        services.AddSingleton<IValidator<TestCommand>, FailValidator>();
        var serviceProvider = services.BuildServiceProvider();
        var behavior = new ValidationBehavior<TestCommand, Result>(serviceProvider);
        var command = new TestCommand { Value = "test" };
        var nextCalled = false;

        MessageHandlerDelegate<Result> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Ok());
        };

        // Act
        var result = await behavior.HandleAsync(command, next);

        // Assert
        nextCalled.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithMultipleFailingValidators_ShouldAggregateErrors()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IValidator<TestCommand>, FailValidator>();
        services.AddSingleton<IValidator<TestCommand>, MultipleErrorsValidator>();
        var serviceProvider = services.BuildServiceProvider();
        var behavior = new ValidationBehavior<TestCommand, Result>(serviceProvider);
        var command = new TestCommand { Value = "" };

        MessageHandlerDelegate<Result> next = () => Task.FromResult(Result.Ok());

        // Act
        var result = await behavior.HandleAsync(command, next);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(3);
    }

    [Fact]
    public async Task HandleAsync_ShouldPassCancellationToken()
    {
        // Arrange
        var cancellationTokenPassed = false;
        var services = new ServiceCollection();
        services.AddSingleton<IValidator<TestCommand>>(new CancellationTokenCheckingValidator(
            ct => cancellationTokenPassed = ct.IsCancellationRequested == false));
        var serviceProvider = services.BuildServiceProvider();
        var behavior = new ValidationBehavior<TestCommand, Result>(serviceProvider);
        var command = new TestCommand { Value = "test" };
        var cts = new CancellationTokenSource();

        MessageHandlerDelegate<Result> next = () => Task.FromResult(Result.Ok());

        // Act
        await behavior.HandleAsync(command, next, cts.Token);

        // Assert
        cancellationTokenPassed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_ValidatorReturnsNull_ShouldTreatAsSuccess()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IValidator<TestCommand>, NullReturningValidator>();
        var serviceProvider = services.BuildServiceProvider();
        var behavior = new ValidationBehavior<TestCommand, Result>(serviceProvider);
        var command = new TestCommand { Value = "test" };
        var nextCalled = false;

        MessageHandlerDelegate<Result> next = () =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Ok());
        };

        // Act
        var result = await behavior.HandleAsync(command, next);

        // Assert
        nextCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    private class CancellationTokenCheckingValidator : IValidator<TestCommand>
    {
        private readonly Action<CancellationToken> _callback;

        public CancellationTokenCheckingValidator(Action<CancellationToken> callback)
        {
            _callback = callback;
        }

        public Task<IReadOnlyList<Error>> ValidateAsync(TestCommand message, CancellationToken cancellationToken = default)
        {
            _callback(cancellationToken);
            return Task.FromResult<IReadOnlyList<Error>>(Array.Empty<Error>());
        }
    }

    private class NullReturningValidator : IValidator<TestCommand>
    {
        public Task<IReadOnlyList<Error>> ValidateAsync(TestCommand message, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Error>>(null!);
        }
    }
}
