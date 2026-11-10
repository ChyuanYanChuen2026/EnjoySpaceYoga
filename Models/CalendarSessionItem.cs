namespace EnjoySpace.Web.Models;

public sealed record CalendarSessionItem(
    int Id, long CourseId, string CourseName, string TeacherName, long TermId, string TermName,
    DateTimeOffset StartsAt, DateTimeOffset EndsAt);
