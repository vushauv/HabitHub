using backend.Models;

namespace backend.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User> CreateAsync(User user);
}
