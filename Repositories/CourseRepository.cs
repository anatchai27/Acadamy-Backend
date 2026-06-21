using academy_API.Data;
using academy_API.DTOs;
using academy_API.Models;
using Microsoft.EntityFrameworkCore;

namespace academy_API.Repositories;

public interface ICourseRepository
{
    Task<List<CourseItem>> SearchAsync(int? instituteId, string? search, int? teacherId, CancellationToken ct = default);
}

public class CourseRepository(TutoringDbContext context) : ICourseRepository
{
    private readonly TutoringDbContext _context = context;

    public async Task<List<CourseItem>> SearchAsync(int? instituteId, string? search, int? teacherId, CancellationToken ct = default)
    {
        var query = _context.Courses
            .Include(c => c.Teacher)
            .AsQueryable();

        if (instituteId.HasValue)
            query = query.Where(c => c.InstituteId == instituteId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                c.Name.Contains(term) ||
                c.Subject.Contains(term));
        }

        if (teacherId.HasValue)
            query = query.Where(c => c.TeacherId == teacherId.Value);

        return await query
            .OrderBy(c => c.Name)
            .Select(c => new CourseItem(
                c.Id,
                c.Name,
                c.Subject,
                c.TotalSessions,
                c.Price,
                c.Teacher != null ? c.Teacher.FullName : null
            ))
            .ToListAsync(ct);
    }
}
