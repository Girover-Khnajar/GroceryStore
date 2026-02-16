using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using CQRS.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace CQRS.Tests.Infrastructure;

public class MessageDispatcherTests
{
    // Test command without response
    public class TestCommand : ICommand
    {
        public string Value { get; set; } = string.Empty;
    }

    // Test command with response
    public class TestCommandWithResponse : ICommand<string>
    {
        public int Number { get; set; }
    }

    // Test query
    public class TestQuery : IQuery<int>
    {
        public string Filter { get; set; } = string.Empty;
    }

    [Fact]
    public async Task SendAsync_Command_ShouldCallHandlerAndReturnResult()
    {
        // Arrange
        var command = new TestCommand { Value = "test" };
        var expectedResult = Result.Ok();

        var mockHandler = new Mock<ICommandHandler<TestCommand>>();
        mockHandler
            .Setup(h => h.HandleAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var services = new ServiceCollection();
        services.AddSingleton(mockHandler.Object);
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new MessageDispatcher(serviceProvider);

        // Act
        var result = await dispatcher.SendAsync(command);

        // Assert
        result.Should().Be(expectedResult);
        mockHandler.Verify(h => h.HandleAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_CommandWithResponse_ShouldCallHandlerAndReturnResult()
    {
        // Arrange
        var command = new TestCommandWithResponse { Number = 42 };
        var expectedResult = Result<string>.Ok("Success: 42");

        var mockHandler = new Mock<ICommandHandler<TestCommandWithResponse, string>>();
        mockHandler
            .Setup(h => h.HandleAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var services = new ServiceCollection();
        services.AddSingleton(mockHandler.Object);
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new MessageDispatcher(serviceProvider);

        // Act
        var result = await dispatcher.SendAsync<TestCommandWithResponse, string>(command);

        // Assert
        result.Should().Be(expectedResult);
        result.Value.Should().Be("Success: 42");
        mockHandler.Verify(h => h.HandleAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task QueryAsync_ShouldCallHandlerAndReturnResult()
    {
        // Arrange
        var query = new TestQuery { Filter = "active" };
        var expectedResult = Result<int>.Ok(100);

        var mockHandler = new Mock<IQueryHandler<TestQuery, int>>();
        mockHandler
            .Setup(h => h.HandleAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var services = new ServiceCollection();
        services.AddSingleton(mockHandler.Object);
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new MessageDispatcher(serviceProvider);

        // Act
        var result = await dispatcher.QueryAsync<TestQuery, int>(query);

        // Assert
        result.Should().Be(expectedResult);
        result.Value.Should().Be(100);
        mockHandler.Verify(h => h.HandleAsync(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_WithCancellationToken_ShouldPassTokenToHandler()
    {
        // Arrange
        var command = new TestCommand { Value = "test" };
        var cancellationToken = new CancellationToken();

        var mockHandler = new Mock<ICommandHandler<TestCommand>>();
        mockHandler
            .Setup(h => h.HandleAsync(command, cancellationToken))
            .ReturnsAsync(Result.Ok());

        var services = new ServiceCollection();
        services.AddSingleton(mockHandler.Object);
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new MessageDispatcher(serviceProvider);

        // Act
        await dispatcher.SendAsync(command, cancellationToken);

        // Assert
        mockHandler.Verify(h => h.HandleAsync(command, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task SendAsync_Command_WhenHandlerReturnsFailure_ShouldReturnFailure()
    {
        // Arrange
        var command = new TestCommand { Value = "test" };
        var error = Error.Validation("Validation failed");
        var expectedResult = Result.Fail(error);

        var mockHandler = new Mock<ICommandHandler<TestCommand>>();
        mockHandler
            .Setup(h => h.HandleAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var services = new ServiceCollection();
        services.AddSingleton(mockHandler.Object);
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new MessageDispatcher(serviceProvider);

        // Act
        var result = await dispatcher.SendAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Should().Be(error);
    }

    [Fact]
    public void SendAsync_WhenHandlerNotRegistered_ShouldThrowException()
    {
        // Arrange
        var command = new TestCommand { Value = "test" };
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new MessageDispatcher(serviceProvider);

        // Act
        Func<Task> act = async () => await dispatcher.SendAsync(command);

        // Assert
        act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task QueryAsync_WithCancellationToken_ShouldPassTokenToHandler()
    {
        // Arrange
        var query = new TestQuery { Filter = "test" };
        var cancellationToken = new CancellationToken();

        var mockHandler = new Mock<IQueryHandler<TestQuery, int>>();
        mockHandler
            .Setup(h => h.HandleAsync(query, cancellationToken))
            .ReturnsAsync(Result<int>.Ok(42));

        var services = new ServiceCollection();
        services.AddSingleton(mockHandler.Object);
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new MessageDispatcher(serviceProvider);

        // Act
        await dispatcher.QueryAsync<TestQuery, int>(query, cancellationToken);

        // Assert
        mockHandler.Verify(h => h.HandleAsync(query, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task QueryAsync_WhenHandlerReturnsFailure_ShouldReturnFailure()
    {
        // Arrange
        var query = new TestQuery { Filter = "test" };
        var error = Error.NotFound("Not found");
        var expectedResult = Result<int>.Fail(error);

        var mockHandler = new Mock<IQueryHandler<TestQuery, int>>();
        mockHandler
            .Setup(h => h.HandleAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var services = new ServiceCollection();
        services.AddSingleton(mockHandler.Object);
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new MessageDispatcher(serviceProvider);

        // Act
        var result = await dispatcher.QueryAsync<TestQuery, int>(query);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Should().Be(error);
    }
}
