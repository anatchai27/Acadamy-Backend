using academy_API.Services.Contracts;

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

        return app;
    }
}

public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, int UserId, string Email, string Role);