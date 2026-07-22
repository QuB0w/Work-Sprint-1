using EventApi.Domain.Enums;

namespace EventApi.Domain.Entities;

public class User
{
    private User()
    {
    }

    public Guid Id { get; set; }
    public string Login { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public UserRole Role { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public static User Create(string login, string passwordHash, UserRole role = UserRole.User)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Login = login.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = role
        };
    }
}
