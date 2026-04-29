using GroceryStore.Domain.Entities;
using GroceryStore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GroceryStore.Infrastructure.Persistence.Auth;

internal sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db) => _db = db;

    public Task<User?> FindByUsernameAsync(string username, CancellationToken ct = default)
        => _db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _db.Users.AddAsync(user, ct);
        await _db.SaveChangesAsync(ct);
    }

    public Task<bool> AnyAsync(CancellationToken ct = default)
        => _db.Users.AnyAsync(ct);

    public async Task UpdatePasswordAsync(User user, string newPasswordHash, CancellationToken ct = default)
    {
        user.PasswordHash = newPasswordHash;
        _db.Users.Update(user);
        await _db.SaveChangesAsync(ct);
    }
}
