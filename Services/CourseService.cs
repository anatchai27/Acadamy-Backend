using academy_API.DTOs;

namespace academy_API.Services;

public interface ICourseService
{
    Task<CourseListResponse> GetAllAsync(int? instituteId, string? search, int? teacherId, CancellationToken ct = default);
}

public class CourseService(Repositories.ICourseRepository repository) : ICourseService
{
    private readonly Repositories.ICourseRepository _repository = repository;

    public async Task<CourseListResponse> GetAllAsync(int? instituteId, string? search, int? teacherId, CancellationToken ct = default)
    {
        var courses = await _repository.SearchAsync(instituteId, search, teacherId, ct);
        return new CourseListResponse("success", new CourseListData(courses));
    }
}
