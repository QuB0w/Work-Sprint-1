using EventApi.Domain.Entities;

namespace EventApi.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
