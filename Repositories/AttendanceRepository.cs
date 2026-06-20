using academy_API.Data;
using academy_API.DTOs;
using academy_API.Models;
using Microsoft.EntityFrameworkCore;

namespace academy_API.Repositories;

public interface IAttendanceRepository
{
    Task<Student?> ValidateQrTokenAsync(string qrToken, CancellationToken ct = default);
    Task<bool> IsDuplicateScanAsync(int studentId, int? sessionId, CancellationToken ct = default);
    Task<Attendance?> GetExistingAttendanceAsync(int studentId, int? sessionId, CancellationToken ct = default);
    Task<Attendance> RecordCheckinAsync(int studentId, int? sessionId, CancellationToken ct = default);
    Task RecordCheckoutAsync(Attendance attendance, CancellationToken ct = default);
    Task<int> DecrementSessionsAsync(int studentId, CancellationToken ct = default);
    Task<Attendance> RecordManualAsync(int sessionId, int studentId, string status, string? note, CancellationToken ct = default);
    Task<Session?> GetSessionByIdAsync(int sessionId, CancellationToken ct = default);
    Task<List<DailyAttendanceRow>> GetDailyAttendanceAsync(int? sessionId, DateTime date, CancellationToken ct = default);
    Task<List<Parent>> GetParentsWithLineAsync(int studentId, CancellationToken ct = default);
}

public class AttendanceRepository(TutoringDbContext context) : IAttendanceRepository
{
    private readonly TutoringDbContext _context = context;

    public async Task<Student?> ValidateQrTokenAsync(string qrToken, CancellationToken ct = default)
    {
        return await _context.Students
            .FirstOrDefaultAsync(s =>
                s.QrToken == qrToken &&
                s.QrTokenExpiry != null &&
                s.QrTokenExpiry > DateTime.UtcNow, ct);
    }

    public async Task<bool> IsDuplicateScanAsync(int studentId, int? sessionId, CancellationToken ct = default)
    {
        return await _context.Attendances.AnyAsync(a =>
            a.StudentId == studentId &&
            a.SessionId == sessionId &&
            a.CheckinAt != null, ct);
    }

    public async Task<Attendance?> GetExistingAttendanceAsync(int studentId, int? sessionId, CancellationToken ct = default)
    {
        return await _context.Attendances
            .FirstOrDefaultAsync(a =>
                a.StudentId == studentId &&
                a.SessionId == sessionId &&
                a.CheckinAt != null, ct);
    }

    public async Task<Attendance> RecordCheckinAsync(int studentId, int? sessionId, CancellationToken ct = default)
    {
        var attendance = new Attendance
        {
            StudentId = studentId,
            SessionId = sessionId,
            CheckinAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync(ct);
        return attendance;
    }

    public async Task RecordCheckoutAsync(Attendance attendance, CancellationToken ct = default)
    {
        attendance.CheckoutAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> DecrementSessionsAsync(int studentId, CancellationToken ct = default)
    {
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId, ct);

        if (enrollment is null)
            return 0;

        enrollment.SessionsRemaining = Math.Max(0, enrollment.SessionsRemaining - 1);
        await _context.SaveChangesAsync(ct);
        return enrollment.SessionsRemaining;
    }

    public async Task<Attendance> RecordManualAsync(int sessionId, int studentId, string status, string? note, CancellationToken ct = default)
    {
        var attendance = new Attendance
        {
            StudentId = studentId,
            SessionId = sessionId,
            Status = status,
            Note = note?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        if (status == "present" || status == "late")
            attendance.CheckinAt = DateTime.UtcNow;

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync(ct);
        return attendance;
    }

    public async Task<Session?> GetSessionByIdAsync(int sessionId, CancellationToken ct = default)
    {
        return await _context.Sessions.FirstOrDefaultAsync(s => s.Id == sessionId, ct);
    }

    public async Task<List<DailyAttendanceRow>> GetDailyAttendanceAsync(int? sessionId, DateTime date, CancellationToken ct = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var enrollmentQuery = _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .AsQueryable();

        if (sessionId.HasValue)
            enrollmentQuery = enrollmentQuery.Where(e => e.CourseId == sessionId.Value);

        var enrolled = await enrollmentQuery
            .Select(e => e.Student)
            .Distinct()
            .ToListAsync(ct);

        var attendanceMap = await _context.Attendances
            .Where(a =>
                a.CreatedAt >= startOfDay &&
                a.CreatedAt < endOfDay &&
                (sessionId == null || a.SessionId == sessionId))
            .ToListAsync(ct);

        return enrolled.Select(s =>
        {
            var record = attendanceMap.FirstOrDefault(a => a.StudentId == s.Id);
            var status = record?.Status ?? (record?.CheckinAt != null ? "present" : "pending");

            return new DailyAttendanceRow(
                s.Id,
                s.FullName,
                s.Nickname,
                status,
                record?.CheckinAt,
                record?.CheckoutAt,
                null
            );
        }).ToList();
    }

    public async Task<List<Parent>> GetParentsWithLineAsync(int studentId, CancellationToken ct = default)
    {
        return await _context.Parents
            .Where(p => p.StudentId == studentId && p.LineUserId != null)
            .ToListAsync(ct);
    }
}
