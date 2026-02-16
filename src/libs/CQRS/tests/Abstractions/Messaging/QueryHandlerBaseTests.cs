using CQRS.Abstractions.Messaging;
using CQRS.CqrsResult;

namespace CQRS.Tests.Abstractions.Messaging;

public class QueryHandlerBaseTests
{
    // Test query
    private class TestQuery : IQuery<string>
    {
        public string Filter { get; set; } = string.Empty;
    }

    // Concrete test query handler
    private class TestQueryHandler : QueryHandlerBase<TestQuery, string>
    {
        public override Task<Result<string>> HandleAsync(TestQuery query, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(query.Filter))
            {
                return Task.FromResult(Failure("Filter is required"));
            }

            return Task.FromResult(Success($"Filtered by: {query.Filter}"));
        }
    }

    // Handler that returns NotFound
    private class NotFoundQueryHandler : QueryHandlerBase<TestQuery, string>
    {
        public override Task<Result<string>> HandleAsync(TestQuery query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(NotFound("Resource not found"));
        }
    }

    // Handler that uses custom error
    private class CustomErrorQueryHandler : QueryHandlerBase<TestQuery, string>
    {
        public override Task<Result<string>> HandleAsync(TestQuery query, CancellationToken cancellationToken = default)
        {
            var error = Error.Unauthorized("Access denied");
            return Task.FromResult(Failure(error));
        }
    }

    // Test query returning integer
    private class CountQuery : IQuery<int>
    {
        public bool IncludeDeleted { get; set; }
    }

    // Handler returning integer
    private class CountQueryHandler : QueryHandlerBase<CountQuery, int>
    {
        public override Task<Result<int>> HandleAsync(CountQuery query, CancellationToken cancellationToken = default)
        {
            var count = query.IncludeDeleted ? 100 : 50;
            return Task.FromResult(Success(count));
        }
    }

    [Fact]
    public async Task HandleAsync_WhenSuccessful_ShouldReturnSuccessWithValue()
    {
        // Arrange
        var handler = new TestQueryHandler();
        var query = new TestQuery { Filter = "active" };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Filtered by: active");
    }

    [Fact]
    public async Task HandleAsync_WhenValidationFails_ShouldReturnFailureResult()
    {
        // Arrange
        var handler = new TestQueryHandler();
        var query = new TestQuery { Filter = "" };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Message.Should().Be("Filter is required");
        result.Errors.First().Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task NotFound_ShouldReturnNotFoundError()
    {
        // Arrange
        var handler = new NotFoundQueryHandler();
        var query = new TestQuery { Filter = "test" };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Type.Should().Be(ErrorType.NotFound);
        result.Errors.First().Message.Should().Be("Resource not found");
    }

    [Fact]
    public async Task NotFound_WithDefaultMessage_ShouldUseDefaultMessage()
    {
        // Arrange
        var handler = new NotFoundQueryHandler();
        var query = new TestQuery { Filter = "test" };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.First().Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Failure_WithCustomError_ShouldReturnFailureWithSpecifiedError()
    {
        // Arrange
        var handler = new CustomErrorQueryHandler();
        var query = new TestQuery { Filter = "test" };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Type.Should().Be(ErrorType.Unauthorized);
        result.Errors.First().Message.Should().Be("Access denied");
    }

    [Fact]
    public async Task HandleAsync_WithIntegerReturn_ShouldReturnCorrectValue()
    {
        // Arrange
        var handler = new CountQueryHandler();
        var query = new CountQuery { IncludeDeleted = true };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(100);
    }

    [Fact]
    public async Task HandleAsync_ShouldSupportCancellationToken()
    {
        // Arrange
        var handler = new TestQueryHandler();
        var query = new TestQuery { Filter = "test" };
        var cancellationToken = new CancellationToken();

        // Act
        var result = await handler.HandleAsync(query, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Success_WithValue_ShouldCreateSuccessResult()
    {
        // Arrange
        var handler = new CountQueryHandler();
        var query = new CountQuery { IncludeDeleted = false };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(50);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Failure_WithString_ShouldCreateValidationError()
    {
        // Arrange
        var handler = new TestQueryHandler();
        var query = new TestQuery { Filter = "" };

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.First().Type.Should().Be(ErrorType.Validation);
        result.Errors.First().Code.Should().Be(ErrorCodes.ValidationError);
    }
}
