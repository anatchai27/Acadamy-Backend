using academy_API.DTOs;
using academy_API.Data;
using academy_API.Models;
using academy_API.Utilities;
using Microsoft.EntityFrameworkCore;

namespace academy_API.Controllers;

public static class CourseEndpoints
{
    public static IEndpointRouteBuilder MapCourseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/courses")
            .WithTags("Courses")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", async (
            Services.ICourseService service,
            HttpContext httpContext,
            string? search,
            int? teacher_id,
            CancellationToken ct) =>
        {
            var instituteId = httpContext.GetInstituteId();
            var result = await service.GetAllAsync(instituteId, search, teacher_id, ct);
            return Results.Ok(result);
        });

        group.MapGet("/{id:int}", async (
            int id,
            HttpContext httpContext,
            TutoringDbContext db,
            CancellationToken ct) =>
        {
            var instituteId = httpContext.GetInstituteId();
            var course = await db.Courses
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == id && c.InstituteId == instituteId, ct);
            return course is null
                ? Results.NotFound(new { Status = "error", Message = "ไม่พบคอร์สเรียน" })
                : Results.Ok(course);
        });

        group.MapPost("/", async (
            Course request,
            HttpContext httpContext,
            TutoringDbContext db,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { Error = "Name is required." });

            var instituteId = httpContext.GetInstituteId();
            request.InstituteId = instituteId;
            request.CreatedAt = DateTime.UtcNow;

            db.Courses.Add(request);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/courses/{request.Id}", request);
        });

        group.MapPut("/{id:int}", async (
            int id,
            Course request,
            HttpContext httpContext,
            TutoringDbContext db,
            CancellationToken ct) =>
        {
            var instituteId = httpContext.GetInstituteId();
            var course = await db.Courses
                .FirstOrDefaultAsync(c => c.Id == id && c.InstituteId == instituteId, ct);

            if (course is null)
                return Results.NotFound(new { Error = "Course not found." });

            course.Name = request.Name ?? course.Name;
            course.Subject = request.Subject ?? course.Subject;
            course.TotalSessions = request.TotalSessions;
            course.Price = request.Price;
            course.TeacherId = request.TeacherId ?? course.TeacherId;

            await db.SaveChangesAsync(ct);
            return Results.Ok(course);
        });

        return app;
    }
}
