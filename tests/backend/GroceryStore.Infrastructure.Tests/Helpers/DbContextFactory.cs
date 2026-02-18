using GroceryStore.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GroceryStore.Infrastructure.Tests.Helpers;

/// <summary>
/// Creates a fresh SQLite in-memory AppDbContext for each test.
/// SQLite in-memory properly supports complex properties unlike the EF InMemory provider.
/// </summary>
public sealed class TestDbContext : IDisposable
{
    private readonly SqliteConnection _connection;
    public AppDbContext Context { get; }

    public TestDbContext()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
