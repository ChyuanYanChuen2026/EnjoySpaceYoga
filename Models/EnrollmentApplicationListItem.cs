namespace EnjoySpace.Web.Models;

public class EnrollmentApplicationListItem
{
    public long Id { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public DateTimeOffset SubmittedAt { get; set; }
}
