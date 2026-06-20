using academy_API.Data;
using academy_API.Models;
using Microsoft.EntityFrameworkCore;

namespace academy_API.Repositories;

public interface IEnrollmentRepository
{
    Task<Course?> GetCourseByIdAsync(int courseId, CancellationToken ct = default);
    Task<bool> ExistsActiveEnrollmentAsync(int studentId, int courseId, CancellationToken ct = default);
    Task<Enrollment> CreateAsync(Enrollment enrollment, CancellationToken ct = default);
}

public class EnrollmentRepository(TutoringDbContext context) : IEnrollmentRepository
{
    private readonly TutoringDbContext _context = context;

    public async Task<Course?> GetCourseByIdAsync(int courseId, CancellationToken ct = default)
    {
        return await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId, ct);
    }

    public async Task<bool> ExistsActiveEnrollmentAsync(int studentId, int courseId, CancellationToken ct = default)
    {
        return await _context.Enrollments.AnyAsync(e =>
            e.StudentId == studentId &&
            e.CourseId == courseId &&
            e.SessionsRemaining > 0, ct);
    }

    public async Task<Enrollment> CreateAsync(Enrollment enrollment, CancellationToken ct = default)
    {
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync(ct);
        return enrollment;
    }
}
