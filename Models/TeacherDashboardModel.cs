namespace EnjoySpace.Web.Models;

public class TeacherDashboardModel
{
    public DateOnly Today { get; set; }
    public DateOnly Month { get; set; }
    public int TodayStudentCount { get; set; }
    public int PendingRequestCount { get; set; }
    public List<TeacherSessionModel> TodaySessions { get; set; } = [];
    public List<TeacherSessionModel> MonthSessions { get; set; } = [];
}

public class TeacherSessionModel
{
    public int Id { get; set; }
    public string CourseName { get; set; } = "";
    public string TermName { get; set; } = "";
    public string LocationName { get; set; } = "";
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public int RegisteredCount { get; set; }
    public int Capacity { get; set; }
    public bool IsCancelled { get; set; }
    public bool IsExtraSession { get; set; }
}
