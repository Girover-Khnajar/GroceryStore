using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using CQRS.Infrastructure.Pipeline;
using Microsoft.Extensions.Logging;

namespace CQRS.Tests.Infrastructure.Pipeline;

public class LoggingBehaviorTests
{
    private class TestCommand : ICommand
    {
        public string Value { get; set; } = string.Empty;
    }

    private class FakeLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Logs { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return new FakeDisposable();
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Logs.Add((logLevel, formatter(state, exception)));
        }

        private class FakeDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }

    [Fact]
    public async Task HandleAsync_OnSuccess_ShouldLogStartAndEnd()
    {
        // Arrange
        var logger = new FakeLogger<LoggingBehavior<TestCommand, Result>>();
        var behavior = new LoggingBehavior<TestCommand, Result>(logger);
        var command = new TestCommand { Value = "test" };

        MessageHandlerDelegate<Result> next = () => Task.FromResult(Result.Ok());

        // Act
        var result = await behavior.HandleAsync(command, next);

        // Assert
        result.IsSuccess.Should().BeTrue();
        logger.Logs.Should().HaveCount(2);
        logger.Logs[0].Level.Should().Be(LogLevel.Information);
        logger.Logs[0].Message.Should().Contain("Handling");
        logger.Logs[0].Message.Should().Contain("TestCommand");
        logger.Logs[1].Level.Should().Be(LogLevel.Information);
        logger.Logs[1].Message.Should().Contain("Handled");
        logger.Logs[1].Message.Should().Contain("TestCommand");
    }

    [Fact]
    public async Task HandleAsync_OnFailure_ShouldLogWarning()
    {
        // Arrange
        var logger = new FakeLogger<LoggingBehavior<TestCommand, Result>>();
        var behavior = new LoggingBehavior<TestCommand, Result>(logger);
        var command = new TestCommand { Value = "test" };
        var error = Error.Validation("Validation failed");

        MessageHandlerDelegate<Result> next = () => Task.FromResult(Result.Fail(error));

        // Act
        var result = await behavior.HandleAsync(command, next);

        // Assert
        result.IsFailure.Should().BeTrue();
        logger.Logs.Should().HaveCount(2);
        logger.Logs[0].Level.Should().Be(LogLevel.Information);
        logger.Logs[1].Level.Should().Be(LogLevel.Warning);
        logger.Logs[1].Message.Should().Contain("failure");
    }

    [Fact]
    public async Task HandleAsync_ShouldCallNextAndReturnResult()
    {
        // Arrange
        var logger = new FakeLogger<LoggingBehavior<TestCommand, Result>>();
        var behavior = new LoggingBehavior<TestCommand, Result>(logger);
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
    public async Task HandleAsync_ShouldLogElapsedTime()
    {
        // Arrange
        var logger = new FakeLogger<LoggingBehavior<TestCommand, Result>>();
        var behavior = new LoggingBehavior<TestCommand, Result>(logger);
        var command = new TestCommand { Value = "test" };

        MessageHandlerDelegate<Result> next = async () =>
        {
            await Task.Delay(10);
            return Result.Ok();
        };

        // Act
        await behavior.HandleAsync(command, next);

        // Assert
        logger.Logs[1].Message.Should().Contain("ms");
    }

    [Fact]
    public async Task HandleAsync_WithMultipleErrors_ShouldLogErrorCount()
    {
        // Arrange
        var logger = new FakeLogger<LoggingBehavior<TestCommand, Result>>();
        var behavior = new LoggingBehavior<TestCommand, Result>(logger);
        var command = new TestCommand { Value = "test" };
        var errors = new[] { Error.Validation("Error 1"), Error.Validation("Error 2") };

        MessageHandlerDelegate<Result> next = () => Task.FromResult(Result.Fail(errors));

        // Act
        var result = await behavior.HandleAsync(command, next);

        // Assert
        result.Errors.Should().HaveCount(2);
        logger.Logs[1].Message.Should().Contain("2");
    }

    [Fact]
    public async Task HandleAsync_WithResultOfT_OnSuccess_ShouldLogCorrectly()
    {
        // Arrange
        var logger = new FakeLogger<LoggingBehavior<TestQuery, Result<int>>>();
        var behavior = new LoggingBehavior<TestQuery, Result<int>>(logger);
        var query = new TestQuery { Filter = "test" };

        MessageHandlerDelegate<Result<int>> next = () => Task.FromResult(Result<int>.Ok(42));

        // Act
        var result = await behavior.HandleAsync(query, next);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        logger.Logs.Should().HaveCount(2);
        logger.Logs[1].Level.Should().Be(LogLevel.Information);
    }

    private class TestQuery : IQuery<int>
    {
        public string Filter { get; set; } = string.Empty;
    }
}
