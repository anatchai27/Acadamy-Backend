namespace academy_API.Models;

public class Course
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Subject { get; set; }
    public int TotalSessions { get; set; }
    public decimal Price { get; set; }
    public int? TeacherId { get; set; }

    public Teacher? Teacher { get; set; }
}
