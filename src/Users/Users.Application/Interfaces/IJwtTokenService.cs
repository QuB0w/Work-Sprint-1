using Users.Domain.Entities;

namespace Users.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
