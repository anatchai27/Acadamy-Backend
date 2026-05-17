using academy_API.Models;

namespace academy_API.Services.Contracts;

public interface ITokenService
{
    string GenerateToken(User user);
    bool VerifyPassword(string password, string passwordHash);
    string HashPassword(string password);
}
