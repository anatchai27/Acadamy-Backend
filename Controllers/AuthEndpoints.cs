using academy_API.Models;
using academy_API.Services.Contracts;

namespace academy_API.Controllers;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        group.MapPost("/login", async (LoginRequest request, IUserService userService, ITokenService tokenService, CancellationToken ct) =>
        {
            var users = await userService.GetAllAsync(ct);
            var user = users.FirstOrDefault(u => u.Email == request.Email);

            if (user is null || !tokenService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Results.Unauthorized();
            }

            var token = tokenService.GenerateToken(user);
            return Results.Ok(new LoginResponse(token, user.Id, user.Email, user.Role.ToString()));
        });

        return app;
    }
}

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, int UserId, string Email, string Role);
