# CQRS Tests

Comprehensive unit test suite for the CQRS library using xUnit, Moq, and FluentAssertions.

## Test Coverage

### CqrsResult Tests

#### `ResultTests.cs`
- Tests for `Result` (non-generic) success and failure creation
- Multiple error handling scenarios
- Validation of Result state properties

#### `ResultBaseTests.cs`
- Tests for the base `ResultBase` class
- Constructor validation (ensures success cannot have errors and failure must have errors)
- ReadOnly error list behavior

#### `ErrorTests.cs`
- Tests for all `Error` factory methods:
  - `Failure`, `Validation`, `BadRequest`, `NotFound`, `Conflict`
  - `Unauthorized`, `Forbidden`, `Unexpected`
- Metadata handling with `WithMetadata`
- Equality and hash code implementations
- `ToString` formatting

### Result<T> Tests

#### `ResultTests.cs` (Result<T> section)
- Success result creation with values
- Failure result creation with errors
- `Value` property access (success vs failure)
- `TryGetValue` pattern implementation
- `Match` method for functional result handling

### Infrastructure Tests

#### `MessageDispatcherTests.cs`
- Command dispatching without response (`SendAsync<TCommand>`)
- Command dispatching with response (`SendAsync<TCommand, TResponse>`)
- Query dispatching (`QueryAsync<TQuery, TResponse>`)
- CancellationToken propagation
- Error scenarios (missing handler, handler failures)

### Handler Base Class Tests

#### `CommandHandlerBaseTests.cs`
- Tests for `CommandHandlerBase<TCommand>` (without response)
- Tests for `CommandHandlerBase<TCommand, TResponse>` (with response)
- Helper method functionality (`Success`, `Failure`)
- Support for single and multiple errors
- CancellationToken support

#### `QueryHandlerBaseTests.cs`
- Tests for `QueryHandlerBase<TQuery, TResponse>`
- Helper methods (`Success`, `Failure`, `NotFound`)
- Error handling scenarios
- CancellationToken support

### Dependency Injection Tests

#### `DependencyInjectionTests.cs`
- Handler registration from assemblies
- MessageDispatcher registration
- Transient lifetime verification
- Multiple handler registration
- Abstract handler exclusion
- Integration tests with full dispatcher workflow

## Running Tests

```powershell
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~MessageDispatcherTests"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Test Statistics

- **Total Tests**: 78
- **Test Classes**: 8
- **All tests passing**: ✅

## Key Testing Patterns

### Arrange-Act-Assert
All tests follow the AAA pattern for clarity and maintainability.

### Moq for Mocking
Used to mock handler interfaces in MessageDispatcher tests.

### FluentAssertions
Provides readable and expressive assertions throughout the test suite.

## Notes

- The test project uses `InternalsVisibleTo` to access internal classes like `MessageDispatcher`
- Nested test classes are public to allow Moq to create dynamic proxies
- Integration tests verify the full CQRS workflow from dispatcher through handlers
