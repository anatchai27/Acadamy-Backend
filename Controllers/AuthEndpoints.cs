using academy_API.Services.Contracts;
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
}

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, int UserId, string Email, string Role);