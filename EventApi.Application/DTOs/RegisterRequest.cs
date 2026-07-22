using System.ComponentModel.DataAnnotations;

namespace EventApi.Application.DTOs;

public class RegisterRequest
{
    [Required]
    [MinLength(3)]
    public string Login { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;

    public string Role { get; set; } = "User";
}
