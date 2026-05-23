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

        group.MapPost("/register", RegisterUser)
            .WithTags("Users")
            .WithOpenApi()
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> RegisterUser(
        IUserService userService,
        RegisterUserRequest request,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString()
            ?? httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        var userRequest = new UserCreateRequest
        {
            Email = request.Email,
            Password = request.Password,
            Phone = request.Phone,
            Role = request.Role,
            LineUserId = request.LineUserId,
            AcceptPdpa = request.AcceptPdpa,
            PdpaConsentVersion = request.PdpaConsentVersion
        };

        try
        {
            var created = await userService.CreateWithConsentAsync(userRequest, ipAddress, ct);
            return Results.Created($"/api/users/{created.Id}", new { created.Id, created.Email, created.Role });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { Error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { Error = ex.Message });
        }
        catch (DbUpdateException)
        {
            return Results.BadRequest(new { Error = "Failed to create user. Please try again." });
        }
    }
}