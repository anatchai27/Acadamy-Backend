namespace academy_API.Models;

public class Attendance
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int? SessionId { get; set; }
    public string? Status { get; set; }
    public string? Note { get; set; }
    public DateTime? CheckinAt { get; set; }
    public DateTime? CheckoutAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Student Student { get; set; } = null!;
}
