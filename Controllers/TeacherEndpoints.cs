using academy_API.Data;
using academy_API.Models;
using Microsoft.EntityFrameworkCore;

namespace academy_API.Controllers;

public static class TeacherEndpoints
{
    public static IEndpointRouteBuilder MapTeacherEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/teachers")
            .WithTags("Teachers")
            .WithOpenApi();

        group.MapGet("/", async (TutoringDbContext db, CancellationToken ct) =>
            await db.Teachers
                .Include(t => t.User)
                .OrderBy(t => t.FullName)
                .ToListAsync(ct));

        group.MapGet("/{id:int}", async (int id, TutoringDbContext db, CancellationToken ct) =>
        {
            var teacher = await db.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            return teacher is null
                ? Results.NotFound(new { Error = "Teacher not found." })
                : Results.Ok(teacher);
        });

        group.MapPost("/", async (Teacher request, TutoringDbContext db, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                return Results.BadRequest(new { Error = "FullName is required." });

            request.CreatedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;
            db.Teachers.Add(request);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/teachers/{request.Id}", request);
        });

        return app;
    }
}
