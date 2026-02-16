using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using CQRS.Infrastructure.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace CQRS.Tests.Infrastructure.Pipeline;

public class AuthorizationBehaviorTests
{
    private class TestCommand : ICommand
    {
        public string Value { get; set; } = string.Empty;
    }

    private class SuccessAuthorizer : IAuthorizer<TestCommand>
    {
        public Task<AuthorizationResult> AuthorizeAsync(TestCommand message, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(AuthorizationResult.Success());
        }
    }

    private class FailAuthorizer : IAuthorizer<TestCommand>
    {
        public Task<AuthorizationResult> AuthorizeAsync(TestCommand message, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(AuthorizationResult.Unauthorized("Not authorized"));
        }
    }

    private class ForbiddenAuthorizer : IAuthorizer<TestCommand>
    {
        public Task<AuthorizationResult> AuthorizeAsync(TestCommand message, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(AuthorizationResult.Forbidden("Access forbidden"));
        }
    }

    [Fact]
    public async Task HandleAsync_WithNoAuthorizers_ShouldCallNext()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var behavior = new AuthorizationBehavior<TestCommand, Result>(serviceProvider);
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
    public async Task HandleAsync_WithSuccessfulAuthorizer_ShouldCallNext()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IAuthorizer<TestCommand>, SuccessAuthorizer>();
        var serviceProvider = services.BuildServiceProvider();
        var behavior = new AuthorizationBehavior<TestCommand, Result>(serviceProvider);
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
    public async Task HandleAsync_WithFailingAuthorizer_ShouldShortCircuit()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IAuthorizer<TestCommand>, FailAuthorizer>();
        var serviceProvider = services.BuildServiceProvider();
        var behavior = new AuthorizationBehavior<TestCommand, Result>(serviceProvider);
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
        result.Errors.Should().ContainSingle();
        result.Errors.First().Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task HandleAsync_WithMultipleAuthorizers_AllPass_ShouldCallNext()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IAuthorizer<TestCommand>, SuccessAuthorizer>();
        services.AddSingleton<IAuthorizer<TestCommand>, SuccessAuthorizer>();
        var serviceProvider = services.BuildServiceProvider();
        var behavior = new AuthorizationBehavior<TestCommand, Result>(serviceProvider);
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
    public async Task HandleAsync_WithMultipleAuthorizers_OneFails_ShouldShortCircuit()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IAuthorizer<TestCommand>, SuccessAuthorizer>();
        services.AddSingleton<IAuthorizer<TestCommand>, FailAuthorizer>();
        var serviceProvider = services.BuildServiceProvider();
        var behavior = new AuthorizationBehavior<TestCommand, Result>(serviceProvider);
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
    public async Task HandleAsync_WithMultipleFailingAuthorizers_ShouldAggregateErrors()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IAuthorizer<TestCommand>, FailAuthorizer>();
        services.AddSingleton<IAuthorizer<TestCommand>, ForbiddenAuthorizer>();
        var serviceProvider = services.BuildServiceProvider();
        var behavior = new AuthorizationBehavior<TestCommand, Result>(serviceProvider);
        var command = new TestCommand { Value = "test" };

        MessageHandlerDelegate<Result> next = () => Task.FromResult(Result.Ok());

        // Act
        var result = await behavior.HandleAsync(command, next);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.Type == ErrorType.Unauthorized);
        result.Errors.Should().Contain(e => e.Type == ErrorType.Forbidden);
    }

    [Fact]
    public async Task HandleAsync_ShouldPassCancellationToken()
    {
        // Arrange
        var cancellationTokenPassed = false;
        var services = new ServiceCollection();
        services.AddSingleton<IAuthorizer<TestCommand>>(new CancellationTokenCheckingAuthorizer(
            ct => cancellationTokenPassed = ct.IsCancellationRequested == false));
        var serviceProvider = services.BuildServiceProvider();
        var behavior = new AuthorizationBehavior<TestCommand, Result>(serviceProvider);
        var command = new TestCommand { Value = "test" };
        var cts = new CancellationTokenSource();

        MessageHandlerDelegate<Result> next = () => Task.FromResult(Result.Ok());

        // Act
        await behavior.HandleAsync(command, next, cts.Token);

        // Assert
        cancellationTokenPassed.Should().BeTrue();
    }

    private class CancellationTokenCheckingAuthorizer : IAuthorizer<TestCommand>
    {
        private readonly Action<CancellationToken> _callback;

        public CancellationTokenCheckingAuthorizer(Action<CancellationToken> callback)
        {
            _callback = callback;
        }

        public Task<AuthorizationResult> AuthorizeAsync(TestCommand message, CancellationToken cancellationToken = default)
        {
            _callback(cancellationToken);
            return Task.FromResult(AuthorizationResult.Success());
        }
    }
}
