using academy_API.Data;
using academy_API.Models;
using academy_API.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace academy_API.Controllers;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        group.MapPost("/login", async (LoginRequest request, IUserService userService, CancellationToken ct) =>
        {
            var result = await userService.LoginAsync(request.Email, request.Password, ct);

            if (result is null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new LoginResponse(result.Token, result.UserId, result.Email, result.Role));
        });

        group.MapPost("/register-institute", RegisterInstitute)
            .AllowAnonymous();

        group.MapGet("/me", async (HttpContext httpContext, IUserService userService, CancellationToken ct) =>
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var result = await userService.GetCurrentUserAsync(userId, ct);
            if (result is null)
                return Results.Json(new { status = "error", error_code = "USER_NOT_FOUND", message = "ไม่พบบัญชีผู้ใช้" }, statusCode: 404);

            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPost("/logout", () =>
        {
            return Results.Ok(new { status = "success", message = "ออกจากระบบสำเร็จ" });
        }).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> RegisterInstitute(
        RegisterUserRequest request,
        ITokenService tokenService,
        TutoringDbContext db,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString()
            ?? httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return Results.BadRequest(new { Error = "Email and password are required." });

        if (request.Role != UserRole.admin)
            return Results.BadRequest(new { Error = "Role must be 'admin' for institute registration." });

        if (request.Institute?.Name == null)
            return Results.BadRequest(new { Error = "Institute name is required." });

        try
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);

                // 1. Create Institute
                var institute = new Institute
                {
                    Name = request.Institute.Name,
                    ContactPhone = request.Institute.ContactPhone,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                db.Institutes.Add(institute);
                await db.SaveChangesAsync(ct);

                if (!string.IsNullOrWhiteSpace(request.Institute.LogoBase64))
                    institute.LogoUrl = request.Institute.LogoBase64;

                // 2. Check email uniqueness
                if (await db.Users.AnyAsync(u => u.Email == request.Email, ct))
                {
                    await transaction.RollbackAsync(ct);
                    return Results.BadRequest(new { Error = "Email is already registered." });
                }

                // 3. Hash password
                var passwordHash = tokenService.HashPassword(request.Password);

                // 4. Create User (admin)
                var user = new User
                {
                    InstituteId = institute.Id,
                    Email = request.Email,
                    Phone = request.Phone,
                    Role = UserRole.admin,
                    LineUserId = request.LineUserId,
                    PasswordHash = passwordHash,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                db.Users.Add(user);
                await db.SaveChangesAsync(ct);

                // 5. Create Teacher profile
                var adminFullName = request.Admin?.FullName ?? "Admin";
                var teacher = new Teacher
                {
                    InstituteId = institute.Id,
                    UserId = user.Id,
                    FullName = adminFullName
                };

                db.Teachers.Add(teacher);

                // 6. Create PdpaConsent
                db.PdpaConsents.Add(new PdpaConsent
                {
                    UserId = user.Id,
                    ConsentVersion = string.IsNullOrWhiteSpace(request.PdpaConsentVersion) ? "1.0" : request.PdpaConsentVersion,
                    IsAccepted = request.AcceptPdpa,
                    IpAddress = ipAddress,
                    AcceptedAt = DateTime.UtcNow
                });

                await db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                // 7. Generate JWT token
                var token = tokenService.GenerateToken(user);

                return Results.Created($"/api/auth/me", new
                {
                    status = "success",
                    message = "ลงทะเบียนสถาบันสำเร็จ",
                    token,
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        role = user.Role.ToString(),
                        instituteId = institute.Id,
                        instituteName = institute.Name
                    }
                });
        }
        catch (Exception)
        {
            return Results.Problem("เกิดข้อผิดพลาดในการลงทะเบียนสถาบัน กรุณาลองใหม่อีกครั้ง", statusCode: 500);
        }
    }
}

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, int UserId, string Email, string Role);