namespace academy_API.Models;

public class PdpaConsent
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int? StudentId { get; set; }
    public string ConsentVersion { get; set; } = "1.0";
    public bool IsAccepted { get; set; }
    public string? IpAddress { get; set; }
    public DateTime ConsentedAt { get; set; }

    public User? User { get; set; }
    public Student? Student { get; set; }
}
