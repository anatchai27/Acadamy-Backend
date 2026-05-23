using academy_API.Models;

namespace academy_API.Services.Contracts;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);
    Task<User> CreateWithConsentAsync(UserCreateRequest request, string? ipAddress, CancellationToken cancellationToken = default);
    Task<bool> IsDuplicateAsync(string email, string? phone, CancellationToken cancellationToken = default);
}
