using Users.Domain.Entities;

namespace Users.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByLoginAsync(string login);
    Task<User> AddAsync(User user);
}
