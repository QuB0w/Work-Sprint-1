using Users.Application.DTOs;
using Users.Application.Interfaces;
using Users.Domain.Entities;

namespace Users.Application.Services;

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
        var existing = await _userRepository.GetByLoginAsync(request.Login);
        if (existing is not null)
            throw new InvalidOperationException($"User '{request.Login}' already exists.");

        var role = Enum.TryParse<UserRole>(request.Role, true, out var parsed)
            ? parsed
            : UserRole.User;

        var hash = _passwordHasher.Hash(request.Password);
        var user = User.Create(request.Login, hash, role);
        await _userRepository.AddAsync(user);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByLoginAsync(request.Login);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new KeyNotFoundException("Invalid login or password.");

        var token = _jwtTokenService.GenerateToken(user);
        return new LoginResponse { Token = token };
    }
}
