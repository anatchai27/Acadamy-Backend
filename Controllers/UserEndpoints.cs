using academy_API.Data;
using academy_API.Models;
using academy_API.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace academy_API.Controllers;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", async (IUserService userService, CancellationToken ct) =>
            await userService.GetAllAsync(ct));

        group.MapGet("/{id:int}", async (int id, IUserService userService, CancellationToken ct) =>
        {
            var user = await userService.GetByIdAsync(id, ct);
            return user is null
                ? Results.NotFound(new { Error = "User not found." })
                : Results.Ok(user);
        });

        group.MapPost("/", async (IUserService userService, UserCreateRequest request, CancellationToken ct) =>
        {
            var user = new User
            {
                Email = request.Email,
                Phone = request.Phone,
                Role = request.Role,
                LineUserId = request.LineUserId,
                PasswordHash = request.PasswordHash,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await userService.CreateAsync(user, ct);
            return Results.Created($"/api/users/{created.Id}", created);
        });

        return app;
    }
}


