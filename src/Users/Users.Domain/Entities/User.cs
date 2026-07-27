namespace Users.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Login { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRole Role { get; private set; }

    private User() { }

    public static User Create(string login, string passwordHash, UserRole role)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Login = login,
            PasswordHash = passwordHash,
            Role = role
        };
    }
}
