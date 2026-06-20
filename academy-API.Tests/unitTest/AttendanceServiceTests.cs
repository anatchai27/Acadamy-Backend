using academy_API.DTOs;
using academy_API.Models;
using academy_API.Services;
using Moq;

namespace academy_API.Tests.unitTest;

public class AttendanceServiceTests
{
    private static Mock<academy_API.Repositories.IAttendanceRepository> CreateMockRepo() => new();
    private static Mock<academy_API.Services.Contracts.ILineNotificationService> CreateMockLine() => new();

    private static AttendanceService CreateSut(
        Mock<academy_API.Repositories.IAttendanceRepository>? repoMock = null,
        Mock<academy_API.Services.Contracts.ILineNotificationService>? lineMock = null) =>
        new(repoMock?.Object ?? CreateMockRepo().Object, lineMock?.Object ?? CreateMockLine().Object);

    // 1 ──────────────────── ScanAsync ────────────────────

    [Fact]
    public async Task ScanAsync_ValidQrToken_ReturnsScanResponse()
    {
        var repoMock = CreateMockRepo();
        repoMock.Setup(r => r.ValidateQrTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Student { Id = 105, FullName = "สมชาย" });
        repoMock.Setup(r => r.IsDuplicateScanAsync(105, 12, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        repoMock.Setup(r => r.RecordCheckinAsync(105, 12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Attendance { Id = 1, StudentId = 105, SessionId = 12, CheckinAt = DateTime.UtcNow });
        repoMock.Setup(r => r.DecrementSessionsAsync(105, It.IsAny<CancellationToken>())).ReturnsAsync(9);
        repoMock.Setup(r => r.GetParentsWithLineAsync(105, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var sut = CreateSut(repoMock);
        var result = await sut.ScanAsync(new ScanAttendanceRequest("valid-token", 12));

        Assert.Equal("success", result.Status);
        Assert.Equal(105, result.Data.StudentId);
        Assert.Equal("สมชาย", result.Data.StudentName);
        Assert.Equal("present", result.Data.Status);
        Assert.Equal(9, result.Data.SessionsRemaining);
    }

    // 2
    [Fact]
    public async Task ScanAsync_InvalidQrToken_ThrowsInvalidQrException()
    {
        var repoMock = CreateMockRepo();
        repoMock.Setup(r => r.ValidateQrTokenAsync("bad-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

        var sut = CreateSut(repoMock);
        var ex = await Assert.ThrowsAsync<AttendanceValidationException>(
            () => sut.ScanAsync(new ScanAttendanceRequest("bad-token", 12)));
        Assert.Equal("INVALID_QR", ex.ErrorCode);
    }

    // 3
    [Fact]
    public async Task ScanAsync_DuplicateScan_ThrowsDuplicateException()
    {
        var repoMock = CreateMockRepo();
        repoMock.Setup(r => r.ValidateQrTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Student { Id = 105 });
        repoMock.Setup(r => r.IsDuplicateScanAsync(105, 12, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var sut = CreateSut(repoMock);
        var ex = await Assert.ThrowsAsync<AttendanceValidationException>(
            () => sut.ScanAsync(new ScanAttendanceRequest("valid-token", 12)));
        Assert.Equal("DUPLICATE_SCAN", ex.ErrorCode);
    }

    // 4
    [Fact]
    public async Task ScanAsync_NullSessionId_WorksCorrectly()
    {
        var repoMock = CreateMockRepo();
        repoMock.Setup(r => r.ValidateQrTokenAsync("token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Student { Id = 105 });
        repoMock.Setup(r => r.IsDuplicateScanAsync(105, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        repoMock.Setup(r => r.RecordCheckinAsync(105, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Attendance { CheckinAt = DateTime.UtcNow });
        repoMock.Setup(r => r.DecrementSessionsAsync(105, It.IsAny<CancellationToken>())).ReturnsAsync(5);
        repoMock.Setup(r => r.GetParentsWithLineAsync(105, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var sut = CreateSut(repoMock);
        var result = await sut.ScanAsync(new ScanAttendanceRequest("token", null));
        Assert.Equal("success", result.Status);
    }

    // 5
    [Fact]
    public async Task ScanAsync_DecrementsSessionCount()
    {
        var repoMock = CreateMockRepo();
        repoMock.Setup(r => r.ValidateQrTokenAsync("token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Student { Id = 105 });
        repoMock.Setup(r => r.IsDuplicateScanAsync(105, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        repoMock.Setup(r => r.RecordCheckinAsync(105, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Attendance { CheckinAt = DateTime.UtcNow });
        repoMock.Setup(r => r.DecrementSessionsAsync(105, It.IsAny<CancellationToken>())).ReturnsAsync(7);
        repoMock.Setup(r => r.GetParentsWithLineAsync(105, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var sut = CreateSut(repoMock);
        var result = await sut.ScanAsync(new ScanAttendanceRequest("token", null));

        Assert.Equal(7, result.Data.SessionsRemaining);
        repoMock.Verify(r => r.DecrementSessionsAsync(105, It.IsAny<CancellationToken>()), Times.Once);
    }

    // 6 ──────────────────── ManualAsync ────────────────────

    [Fact]
    public async Task ManualAsync_ValidPresent_ReturnsManualResponse()
    {
        var repoMock = CreateMockRepo();
        repoMock.Setup(r => r.IsDuplicateScanAsync(105, 12, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        repoMock.Setup(r => r.RecordManualAsync(12, 105, "present", "note", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Attendance { Id = 450 });
        repoMock.Setup(r => r.DecrementSessionsAsync(105, It.IsAny<CancellationToken>())).ReturnsAsync(8);

        var sut = CreateSut(repoMock);
        var result = await sut.ManualAsync(new ManualAttendanceRequest(12, 105, "present", "note"));

        Assert.Equal("success", result.Status);
        Assert.Equal(450, result.Data.AttendanceId);
        Assert.Equal("present", result.Data.StatusRecorded);
        repoMock.Verify(r => r.DecrementSessionsAsync(105, It.IsAny<CancellationToken>()), Times.Once);
    }

    // 7
    [Fact]
    public async Task ManualAsync_AbsentStatus_DoesNotDecrementSessions()
    {
        var repoMock = CreateMockRepo();
        repoMock.Setup(r => r.IsDuplicateScanAsync(105, 12, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        repoMock.Setup(r => r.RecordManualAsync(12, 105, "absent", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Attendance { Id = 451 });

        var sut = CreateSut(repoMock);
        await sut.ManualAsync(new ManualAttendanceRequest(12, 105, "absent", null));

        repoMock.Verify(r => r.DecrementSessionsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // 8
    [Fact]
    public async Task ManualAsync_InvalidStatus_ThrowsException()
    {
        var sut = CreateSut();
        var ex = await Assert.ThrowsAsync<AttendanceValidationException>(
            () => sut.ManualAsync(new ManualAttendanceRequest(12, 105, "invalid", null)));
        Assert.Equal("INVALID_STATUS", ex.ErrorCode);
    }

    // 9
    [Fact]
    public async Task ManualAsync_DuplicateScan_ThrowsException()
    {
        var repoMock = CreateMockRepo();
        repoMock.Setup(r => r.IsDuplicateScanAsync(105, 12, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var sut = CreateSut(repoMock);
        var ex = await Assert.ThrowsAsync<AttendanceValidationException>(
            () => sut.ManualAsync(new ManualAttendanceRequest(12, 105, "present", null)));
        Assert.Equal("DUPLICATE_SCAN", ex.ErrorCode);
    }

    // 10 ──────────────────── GetDailyAsync ────────────────────

    [Fact]
    public async Task GetDailyAsync_ValidDate_ReturnsDailyResponse()
    {
        var repoMock = CreateMockRepo();
        repoMock.Setup(r => r.GetSessionByIdAsync(12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Session { Id = 12, Name = "คณิต ม.1", StartTime = new DateTime(2026, 6, 14, 13, 0, 0, DateTimeKind.Utc) });
        repoMock.Setup(r => r.GetDailyAttendanceAsync(12, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new DailyAttendanceRow(105, "สมชาย", "ชาย", "present", DateTime.UtcNow, null, null)]);

        var sut = CreateSut(repoMock);
        var result = await sut.GetDailyAsync(12, "2026-06-14");

        Assert.Equal("success", result.Status);
        Assert.NotNull(result.Data.SessionInfo);
        Assert.Equal("คณิต ม.1", result.Data.SessionInfo!.CourseName);
        Assert.Single(result.Data.Attendances);
    }

    // 11
    [Fact]
    public async Task GetDailyAsync_InvalidDateFormat_ThrowsException()
    {
        var sut = CreateSut();
        var ex = await Assert.ThrowsAsync<AttendanceValidationException>(
            () => sut.GetDailyAsync(null, "14-06-2026"));
        Assert.Equal("INVALID_DATE", ex.ErrorCode);
    }
}
