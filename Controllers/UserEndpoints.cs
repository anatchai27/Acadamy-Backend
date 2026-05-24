using System.Text;
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

        group.MapPost("/forget-password", ForgetPassword)
            .WithTags("Users")
            .WithOpenApi()
            .AllowAnonymous();

        group.MapPost("/reset-password", ResetPassword)
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

    private static async Task<IResult> ForgetPassword(
        ForgetPasswordRequest request,
        IUserService userService,
        IEmailService emailService,
        IConfiguration config,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var frontendUrl = config["Frontend:BaseUrl"] ?? "http://localhost:3000";
        var resetLink = $"{frontendUrl}/reset-password?email={Uri.EscapeDataString(request.Email)}";

        var userExists = await userService.ForgetPasswordAsync(request.Email, resetLink, ct);

        if (userExists)
        {
            var emailTemplate = Path.Combine(Directory.GetCurrentDirectory(), "templates", "forgetPasswordEmail.html");
            var htmlBody = await File.ReadAllTextAsync(emailTemplate, Encoding.UTF8, ct);

            htmlBody = htmlBody
                .Replace("{{UserName}}", request.Email.Split('@')[0])
                .Replace("{{ResetLink}}", resetLink)
                .Replace("{{ExpiryTime}}", "1 hour");

            try
            {
                await emailService.SendEmailAsync(request.Email, "Reset Your Password", htmlBody, ct);
            }
            catch
            {
                // Log error but don't expose it to the client
            }
        }

        // Always return success to prevent email enumeration
        return Results.Ok(new { Message = "If the email exists, a reset link has been sent." });
    }

    private static async Task<IResult> ResetPassword(
        ResetPasswordRequest request,
        IUserService userService,
        CancellationToken ct)
    {
        var success = await userService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword, ct);

        if (!success)
        {
            return Results.BadRequest(new { Error = "Invalid or expired reset token." });
        }

        return Results.Ok(new { Message = "Password reset successfully." });
    }
}

public record ForgetPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Token, string NewPassword);