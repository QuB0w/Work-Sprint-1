using System.ComponentModel.DataAnnotations;

namespace EventApi.Application.DTOs;

public class UpdateEventRequest
{
    [Required]
    [MinLength(1)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public DateTime StartAt { get; set; }

    [Required]
    public DateTime EndAt { get; set; }
}
