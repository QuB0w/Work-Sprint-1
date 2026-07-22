using EventApi.Application.Interfaces;
using EventApi.Domain.Entities;
using EventApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventApi.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Login == login, cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }
}
