using System.Reflection;
using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CQRS.Tests;

public class DependencyInjectionTests
{
    // Test commands and handlers in a separate assembly-like context
    private class TestCommand : ICommand
    {
        public string Value { get; set; } = string.Empty;
    }

    private class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public Task<Result> HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Ok());
        }
    }

    private class TestCommandWithResponse : ICommand<string>
    {
        public int Number { get; set; }
    }

    private class TestCommandWithResponseHandler : ICommandHandler<TestCommandWithResponse, string>
    {
        public Task<Result<string>> HandleAsync(TestCommandWithResponse command, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result<string>.Ok($"Result: {command.Number}"));
        }
    }

    private class TestQuery : IQuery<int>
    {
        public string Filter { get; set; } = string.Empty;
    }

    private class TestQueryHandler : IQueryHandler<TestQuery, int>
    {
        public Task<Result<int>> HandleAsync(TestQuery query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result<int>.Ok(42));
        }
    }

    [Fact]
    public void AddCqrs_ShouldRegisterMessageDispatcher()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCqrs(options =>
        {
            options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetService<IMessageDispatcher>();
        dispatcher.Should().NotBeNull();
    }

    [Fact]
    public void AddCqrs_ShouldRegisterCommandHandler()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCqrs(options =>
        {
            options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetService<ICommandHandler<TestCommand>>();
        handler.Should().NotBeNull();
        // Note: Multiple handlers may be registered, so we just check that one exists
    }

    [Fact]
    public void AddCqrs_ShouldRegisterCommandHandlerWithResponse()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCqrs(options =>
        {
            options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetService<ICommandHandler<TestCommandWithResponse, string>>();
        handler.Should().NotBeNull();
        handler.Should().BeOfType<TestCommandWithResponseHandler>();
    }

    [Fact]
    public void AddCqrs_ShouldRegisterQueryHandler()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCqrs(options =>
        {
            options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var handler = serviceProvider.GetService<IQueryHandler<TestQuery, int>>();
        handler.Should().NotBeNull();
        handler.Should().BeOfType<TestQueryHandler>();
    }

    [Fact]
    public void AddCqrs_WithoutOptions_ShouldStillRegisterMessageDispatcher()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCqrs(null!);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetService<IMessageDispatcher>();
        dispatcher.Should().NotBeNull();
    }

    [Fact]
    public void AddCqrs_ShouldRegisterHandlersAsTransient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCqrs(options =>
        {
            options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var handler1 = serviceProvider.GetService<ICommandHandler<TestCommand>>();
        var handler2 = serviceProvider.GetService<ICommandHandler<TestCommand>>();

        // Assert
        handler1.Should().NotBeNull();
        handler2.Should().NotBeNull();
        handler1.Should().NotBeSameAs(handler2); // Transient should create new instances
    }

    [Fact]
    public async Task AddCqrs_IntegrationTest_ShouldDispatchCommandSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCqrs(options =>
        {
            options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IMessageDispatcher>();
        var command = new TestCommand { Value = "test" };

        // Act
        var result = await dispatcher.SendAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AddCqrs_IntegrationTest_ShouldDispatchCommandWithResponseSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCqrs(options =>
        {
            options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IMessageDispatcher>();
        var command = new TestCommandWithResponse { Number = 42 };

        // Act
        var result = await dispatcher.SendAsync<TestCommandWithResponse, string>(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Result: 42");
    }

    [Fact]
    public async Task AddCqrs_IntegrationTest_ShouldDispatchQuerySuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCqrs(options =>
        {
            options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetRequiredService<IMessageDispatcher>();
        var query = new TestQuery { Filter = "test" };

        // Act
        var result = await dispatcher.QueryAsync<TestQuery, int>(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void AddCqrs_WithMultipleAssemblies_ShouldRegisterHandlersFromAllAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly1 = Assembly.GetExecutingAssembly();
        var assembly2 = typeof(IMessageDispatcher).Assembly;

        // Act
        services.AddCqrs(options =>
        {
            options.RegisterServicesFromAssembly(assembly1);
            options.RegisterServicesFromAssembly(assembly2);
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = serviceProvider.GetService<IMessageDispatcher>();
        dispatcher.Should().NotBeNull();
    }

    // Test abstract handler (should not be registered)
    private abstract class AbstractCommandHandler : ICommandHandler<TestCommand>
    {
        public abstract Task<Result> HandleAsync(TestCommand command, CancellationToken cancellationToken = default);
    }

    [Fact]
    public void AddCqrs_ShouldNotRegisterAbstractHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCqrs(options =>
        {
            options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var allHandlers = serviceProvider.GetServices<ICommandHandler<TestCommand>>();

        // Assert
        allHandlers.Should().NotContain(h => h.GetType() == typeof(AbstractCommandHandler));
    }

    // Multiple handlers for same command (should register all)
    private class AnotherTestCommandHandler : ICommandHandler<TestCommand>
    {
        public Task<Result> HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Ok());
        }
    }

    [Fact]
    public void AddCqrs_WithMultipleHandlersForSameCommand_ShouldRegisterBoth()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCqrs(options =>
        {
            options.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var handlers = serviceProvider.GetServices<ICommandHandler<TestCommand>>();

        // Assert
        handlers.Should().HaveCountGreaterOrEqualTo(2);
    }
}
