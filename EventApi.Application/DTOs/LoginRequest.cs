using System.ComponentModel.DataAnnotations;

namespace EventApi.Application.DTOs;

public class LoginRequest
{
    [Required]
    public string Login { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}
