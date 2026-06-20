namespace academy_API.Models;

public class Session
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsActive { get; set; }

    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
