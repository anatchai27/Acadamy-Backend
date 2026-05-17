using academy_API.Data;
using academy_API.Models;
using Microsoft.EntityFrameworkCore;

namespace academy_API.Controllers;

public static class StudentEndpoints
{
    public static IEndpointRouteBuilder MapStudentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/students")
            .WithTags("Students")
            .WithOpenApi();

        group.MapGet("/", async (TutoringDbContext db, CancellationToken ct) =>
            await db.Students
                .Include(s => s.User)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(ct));

        group.MapGet("/{id:int}", async (int id, TutoringDbContext db, CancellationToken ct) =>
        {
            var student = await db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id, ct);

            return student is null
                ? Results.NotFound(new { Error = "Student not found." })
                : Results.Ok(student);
        });

        group.MapPost("/", async (Student request, TutoringDbContext db, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                return Results.BadRequest(new { Error = "FullName is required." });

            request.CreatedAt = DateTime.UtcNow;
            db.Students.Add(request);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/students/{request.Id}", request);
        });

        return app;
    }
}
