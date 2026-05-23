namespace academy_API.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public UserRole Role { get; set; }
    public string? LineUserId { get; set; }
    public string PasswordHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Property
    public Student? Student { get; set; }
    public Teacher? Teacher { get; set; }
}
public class UserCreateRequest
{
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public UserRole Role { get; set; }
    public string? LineUserId { get; set; }
    public string Password { get; set; } = null!;
    public bool AcceptPdpa { get; set; }
    public string PdpaConsentVersion { get; set; } = "1.0";
}
public class RegisterUserRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? Phone { get; set; }
    public UserRole Role { get; set; } = UserRole.student;
    public string? LineUserId { get; set; }
    public bool AcceptPdpa { get; set; }
    public string PdpaConsentVersion { get; set; } = "1.0";
}

