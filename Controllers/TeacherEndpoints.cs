using academy_API.Data;
using academy_API.Models;
using academy_API.Utilities;
using Microsoft.EntityFrameworkCore;

namespace academy_API.Controllers;

public static class TeacherEndpoints
{
    public static IEndpointRouteBuilder MapTeacherEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/teachers")
            .WithTags("Teachers")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", async (HttpContext httpContext, TutoringDbContext db, CancellationToken ct) =>
        {
            var instituteId = httpContext.GetInstituteId();
            var query = db.Teachers
                .Include(t => t.User)
                .AsQueryable();

            if (instituteId.HasValue)
                query = query.Where(t => t.InstituteId == instituteId.Value);

            return await query.OrderBy(t => t.FullName).ToListAsync(ct);
        });

        group.MapGet("/{id:int}", async (int id, HttpContext httpContext, TutoringDbContext db, CancellationToken ct) =>
        {
            var instituteId = httpContext.GetInstituteId();
            var teacher = await db.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id && t.InstituteId == instituteId, ct);

            return teacher is null
                ? Results.NotFound(new { Error = "Teacher not found." })
                : Results.Ok(teacher);
        });

        group.MapPost("/", async (Teacher request, HttpContext httpContext, TutoringDbContext db, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                return Results.BadRequest(new { Error = "FullName is required." });

            var instituteId = httpContext.GetInstituteId();
            request.InstituteId = instituteId;

            db.Teachers.Add(request);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/teachers/{request.Id}", request);
        });

        return app;
    }
}