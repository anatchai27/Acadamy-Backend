namespace academy_API.Models;

public class Parent
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Relationship { get; set; }
    public string? LineUserId { get; set; }

    public Student Student { get; set; } = null!;
}
