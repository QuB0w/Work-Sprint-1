using System.ComponentModel.DataAnnotations;

namespace Events.Application.DTOs;

public class UpdateEventRequest
{
    [Required]
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    [Required]
    public DateTime StartAt { get; set; }
    [Required]
    public DateTime EndAt { get; set; }
    [Range(1, int.MaxValue)]
    public int TotalSeats { get; set; }
}
