using academy_API.Data;
using academy_API.Models;
using academy_API.Repositories;
using academy_API.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace academy_API.Services;

public class UserService(
    IUserRepository repository,
    IPdpaConsentRepository pdpaRepository,
    ITokenService tokenService,
    TutoringDbContext context) : IUserService
{
    private readonly IUserRepository _repository = repository;
    private readonly IPdpaConsentRepository _pdpaRepository = pdpaRepository;
    private readonly ITokenService _tokenService = tokenService;
    private readonly TutoringDbContext _context = context;

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default)
        => await _repository.GetAllAsync(ct);

    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _repository.GetByIdAsync(id, ct);

    public async Task<User> CreateAsync(User user, CancellationToken ct = default)
        => await _repository.CreateAsync(user, ct);

    public async Task<bool> IsDuplicateAsync(string email, string? phone, CancellationToken ct = default)
        => await _context.Users.AnyAsync(u => u.Email == email || (phone != null && u.Phone == phone), ct);

    public async Task<User> CreateWithConsentAsync(UserCreateRequest request, string? ipAddress, CancellationToken ct = default)
    {
        ValidateRequest(request);

        var passwordHash = _tokenService.HashPassword(request.Password);
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                await ValidateUniqueAsync(request, ct);

                var user = new User
                {
                    Email = request.Email,
                    Phone = request.Phone,
                    Role = request.Role,
                    LineUserId = request.LineUserId,
                    PasswordHash = passwordHash,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync(ct);

                _context.PdpaConsents.Add(new PdpaConsent
                {
                    UserId = user.Id,
                    ConsentVersion = string.IsNullOrWhiteSpace(request.PdpaConsentVersion) ? "1.0" : request.PdpaConsentVersion,
                    IsAccepted = true,
                    IpAddress = ipAddress,
                    ConsentedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return user;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        });
    }

    private void ValidateRequest(UserCreateRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (_tokenService == null)
            throw new InvalidOperationException("Token service is not configured.");

        if (_context == null)
            throw new InvalidOperationException("Database context is not configured.");

        if (!request.AcceptPdpa)
            throw new InvalidOperationException("PDPA consent must be accepted to create an account.");

        if (string.IsNullOrEmpty(request.Password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(request.Password));

        if (string.IsNullOrEmpty(request.Email))
            throw new ArgumentException("Email cannot be null or empty.", nameof(request.Email));
    }

    private async Task ValidateUniqueAsync(UserCreateRequest request, CancellationToken ct)
    {
        if (await _context.Users.AnyAsync(
            u => u.Email == request.Email || (request.Phone != null && u.Phone == request.Phone), ct))
        {
            throw new InvalidOperationException("Email or phone number is already registered.");
        }
    }
}
