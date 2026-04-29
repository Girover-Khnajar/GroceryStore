using GroceryStore.Domain.Entities;

namespace GroceryStore.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByUsernameAsync(string username, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task<bool> AnyAsync(CancellationToken ct = default);
    Task UpdatePasswordAsync(User user, string newPasswordHash, CancellationToken ct = default);
}
