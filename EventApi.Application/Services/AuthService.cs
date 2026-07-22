using EventApi.Application.DTOs;
using EventApi.Application.Interfaces;
using EventApi.Domain.Entities;
using EventApi.Domain.Enums;

namespace EventApi.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
        {
            throw new ArgumentException($"Invalid role: {request.Role}. Allowed values: User, Admin.");
        }

        var existing = await _userRepository.GetByLoginAsync(request.Login.Trim().ToLowerInvariant());
        if (existing is not null)
        {
            throw new ArgumentException($"User with login '{request.Login}' already exists.");
        }

        var hash = _passwordHasher.Hash(request.Password);
        var user = User.Create(request.Login, hash, role);
        await _userRepository.AddAsync(user);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByLoginAsync(request.Login.Trim().ToLowerInvariant());
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new KeyNotFoundException("Invalid login or password.");
        }

        var token = _jwtTokenService.GenerateToken(user);
        return new LoginResponse { Token = token };
    }
}
