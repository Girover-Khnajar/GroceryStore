using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using CQRS.Infrastructure.Options;
using CQRS.Infrastructure.Pipeline;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CQRS.Tests.Infrastructure.Pipeline;

public class PerformanceBehaviorTests
{
    private class TestCommand : ICommand
    {
        public string Value { get; set; } = string.Empty;
    }

    private class FakeLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Logs { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Logs.Add((logLevel, formatter(state, exception)));
        }
    }

    private class FakeOptions<T> : IOptions<T> where T : class, new()
    {
        public T Value { get; }

        public FakeOptions(T value)
        {
            Value = value;
        }
    }

    [Fact]
    public async Task HandleAsync_BelowThreshold_ShouldNotLogWarning()
    {
        // Arrange
        var logger = new FakeLogger<PerformanceBehavior<TestCommand, Result>>();
        var options = new FakeOptions<PerformanceBehaviorOptions>(new PerformanceBehaviorOptions
        {
            WarningThresholdMilliseconds = 1000
        });
        var behavior = new PerformanceBehavior<TestCommand, Result>(logger, options);
        var command = new TestCommand { Value = "test" };

        MessageHandlerDelegate<Result> next = () => Task.FromResult(Result.Ok());

        // Act
        var result = await behavior.HandleAsync(command, next);

        // Assert
        result.IsSuccess.Should().BeTrue();
        logger.Logs.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_AboveThreshold_ShouldLogWarning()
    {
        // Arrange
        var logger = new FakeLogger<PerformanceBehavior<TestCommand, Result>>();
        var options = new FakeOptions<PerformanceBehaviorOptions>(new PerformanceBehaviorOptions
        {
            WarningThresholdMilliseconds = 10
        });
        var behavior = new PerformanceBehavior<TestCommand, Result>(logger, options);
        var command = new TestCommand { Value = "test" };

        MessageHandlerDelegate<Result> next = async () =>
        {
            await Task.Delay(50);
            return Result.Ok();
        };

        // Act
        var result = await behavior.HandleAsync(command, next);

        // Assert
        result.IsSuccess.Should().BeTrue();
        logger.Logs.Should().ContainSingle();
        logger.Logs[0].Level.Should().Be(LogLevel.Warning);
        logger.Logs[0].Message.Should().Contain("Slow CQRS message");
        logger.Logs[0].Message.Should().Contain("TestCommand");
    }

    [Fact]
    public async Task HandleAsync_WithZeroThreshold_ShouldNotLogWarning()
    {
        // Arrange
        var logger = new FakeLogger<PerformanceBehavior<TestCommand, Result>>();
        var options = new FakeOptions<PerformanceBehaviorOptions>(new PerformanceBehaviorOptions
        {
            WarningThresholdMilliseconds = 0
        });
        var behavior = new PerformanceBehavior<TestCommand, Result>(logger, options);
        var command = new TestCommand { Value = "test" };

        MessageHandlerDelegate<Result> next = async () =>
        {
            await Task.Delay(10);
            return Result.Ok();
        };

        // Act
        var result = await behavior.HandleAsync(command, next);

        // Assert
        logger.Logs.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ShouldCallNextAndReturnResult()
    {
        // Arrange
        var logger = new FakeLogger<PerformanceBehavior<TestCommand, Result>>();
        var options = new FakeOptions<PerformanceBehaviorOptions>(new PerformanceBehaviorOptions());
        var behavior = new PerformanceBehavior<TestCommand, Result>(logger, options);
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
    public async Task HandleAsync_WhenNextFails_ShouldStillReturnResult()
    {
        // Arrange
        var logger = new FakeLogger<PerformanceBehavior<TestCommand, Result>>();
        var options = new FakeOptions<PerformanceBehaviorOptions>(new PerformanceBehaviorOptions
        {
            WarningThresholdMilliseconds = 1000
        });
        var behavior = new PerformanceBehavior<TestCommand, Result>(logger, options);
        var command = new TestCommand { Value = "test" };
        var error = Error.Validation("Validation failed");

        MessageHandlerDelegate<Result> next = () => Task.FromResult(Result.Fail(error));

        // Act
        var result = await behavior.HandleAsync(command, next);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public async Task HandleAsync_WarningMessage_ShouldContainThreshold()
    {
        // Arrange
        var logger = new FakeLogger<PerformanceBehavior<TestCommand, Result>>();
        var threshold = 5;
        var options = new FakeOptions<PerformanceBehaviorOptions>(new PerformanceBehaviorOptions
        {
            WarningThresholdMilliseconds = threshold
        });
        var behavior = new PerformanceBehavior<TestCommand, Result>(logger, options);
        var command = new TestCommand { Value = "test" };

        MessageHandlerDelegate<Result> next = async () =>
        {
            await Task.Delay(50);
            return Result.Ok();
        };

        // Act
        await behavior.HandleAsync(command, next);

        // Assert
        logger.Logs.Should().ContainSingle();
        logger.Logs[0].Message.Should().Contain(threshold.ToString());
    }

    [Fact]
    public async Task HandleAsync_DefaultOptions_ShouldUse500msThreshold()
    {
        // Arrange
        var options = new PerformanceBehaviorOptions();

        // Assert
        options.WarningThresholdMilliseconds.Should().Be(500);
    }
}
