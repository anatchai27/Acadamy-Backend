namespace academy_API.DTOs;

public record CourseListResponse(
    string Status,
    CourseListData Data
);

public record CourseListData(
    List<CourseItem> Courses
);

public record CourseItem(
    int Id,
    string Name,
    string? Subject,
    int TotalSessions,
    decimal Price,
    string? TeacherName
);
