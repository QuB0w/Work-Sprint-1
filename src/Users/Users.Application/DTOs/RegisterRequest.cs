using System.ComponentModel.DataAnnotations;

namespace Users.Application.DTOs;

public class RegisterRequest
{
    [Required]
    public string Login { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;

    public string Role { get; set; } = "User";
}
