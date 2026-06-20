using academy_API.DTOs;

namespace academy_API.Controllers;

public static class CourseEndpoints
{
    public static IEndpointRouteBuilder MapCourseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/courses")
            .WithTags("Courses")
            .WithOpenApi();

        group.MapGet("/", async (
            Services.ICourseService service,
            string? search,
            int? teacher_id,
            CancellationToken ct) =>
        {
            var result = await service.GetAllAsync(search, teacher_id, ct);
            return Results.Ok(result);
        });

        return app;
    }
}
