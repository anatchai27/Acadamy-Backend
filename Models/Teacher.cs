namespace academy_API.Models;

public class Teacher
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }

    public string FullName { get; set; } = null!;
    public string? Nickname { get; set; }
    public string? Specialization { get; set; }
    public string? Bio { get; set; }
    public decimal? HourlyRate { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
