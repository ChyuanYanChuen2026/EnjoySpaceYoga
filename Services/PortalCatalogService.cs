using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;

namespace EnjoySpace.Web.Services;

// Public projections of the same entities edited by the MVC admin.
// Never expose entity navigation graphs, private teacher notes, or contact details.
public class PortalCatalogService(AppDbContext db)
{
    public IQueryable<Course> Courses => db.Courses.Where(x => x.IsActive && x.IsPubliclyVisible &&
        (x.Term.Status == "Open" || x.Term.Status == "Closed") && x.DefaultTeacher.IsActive && x.DefaultLocation.IsActive);
    public IQueryable<ClassSession> Sessions => db.ClassSessions.Where(x => Courses.Select(c => c.CourseId).Contains(x.CourseId)
        && x.Teacher.IsActive && x.StudioLocation.IsActive);
    // Imported teachers may have disabled login accounts while remaining available for teaching.
    // Account activation is enforced by authentication, not by public course visibility.
    // Same effective membership rule as the admin attendance roster.
    public IQueryable<ClassSessionParticipant> Participants => db.ClassSessionParticipants.Where(x => x.Status == "Active"
        && (x.CourseEnrollmentId == null || x.CourseEnrollment!.Status == "Active"));

    public IQueryable<PortalCourseData> CourseData => Courses.Select(x => new PortalCourseData
    {
        Id = x.CourseId, Name = x.Name, CategoryId = x.ClassCategoryId, Category = x.ClassCategory.Name,
        TermId = x.TermId, TermName = x.Term.Name, TermStatus = x.Term.Status,
        TermStartDate = x.Term.StartDate, TermEndDate = x.Term.EndDate,
        TeacherId = x.DefaultTeacherId, Teacher = x.DefaultTeacher.User.DisplayName,
        LocationId = x.DefaultLocationId, Location = x.DefaultLocation.Name, Address = x.DefaultLocation.Address,
        TermPrice = x.TermPrice, Capacity = x.DefaultCapacity,
        AllowsFlexibleBooking = x.AllowsFlexibleBooking, AllowsMakeup = x.AllowsMakeup,
        SessionCount = db.ClassSessions.Count(s => s.CourseId == x.CourseId && !s.IsExtraSession),
        ScheduleConfirmed = x.Term.ScheduleConfirmedAtUtc != null,
        FirstSessionAt = db.ClassSessions.Where(s => s.CourseId == x.CourseId && !s.IsExtraSession).Min(s => (DateTimeOffset?)s.StartsAt),
        HasEnrollmentCapacity = db.CourseEnrollments.Count(e => e.CourseId == x.CourseId && e.Status == "Active") < x.DefaultCapacity &&
            !db.ClassSessions.Any(s => s.CourseId == x.CourseId && !s.IsExtraSession && Participants.Count(p => p.ClassSessionId == s.Id) >= (s.CapacityOverride ?? s.Capacity)),
        HasUnavailableSessions = db.ClassSessions.Any(s => s.CourseId == x.CourseId && !s.IsExtraSession && (s.IsCancelled || !s.Teacher.IsActive || !s.StudioLocation.IsActive))
    });

    public IQueryable<PortalSessionData> SessionData(IQueryable<ClassSession> query) => query.Select(x => new PortalSessionData
    {
        Id = x.Id, CourseId = x.CourseId, Name = x.Course.Name, CategoryId = x.Course.ClassCategoryId, Category = x.Course.ClassCategory.Name,
        TermId = x.Course.TermId, TermName = x.Course.Term.Name,
        TeacherId = x.TeacherId, Teacher = x.Teacher.User.DisplayName,
        LocationId = x.StudioLocationId, Location = x.StudioLocation.Name, Address = x.StudioLocation.Address,
        StartsAt = x.StartsAt, EndsAt = x.EndsAt, IsExtraSession = x.IsExtraSession,
        Capacity = x.CapacityOverride ?? x.Capacity,
        Enrolled = Participants.Count(p => p.ClassSessionId == x.Id)
    });
}

public class PortalCourseData
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public int CategoryId { get; set; }
    public string Category { get; set; } = "";
    public long TermId { get; set; }
    public string TermName { get; set; } = "";
    public string TermStatus { get; set; } = "";
    public string TermStatusText => EnjoySpace.Web.Areas.Admin.ViewModels.TermStatusOptions.GetText(TermStatus);
    public DateOnly TermStartDate { get; set; }
    public DateOnly TermEndDate { get; set; }
    public long TeacherId { get; set; }
    public string Teacher { get; set; } = "";
    public long LocationId { get; set; }
    public string Location { get; set; } = "";
    public string Address { get; set; } = "";
    public decimal TermPrice { get; set; }
    public int Capacity { get; set; }
    public bool AllowsFlexibleBooking { get; set; }
    public bool AllowsMakeup { get; set; }
    public int SessionCount { get; set; }
    public bool ScheduleConfirmed { get; set; }
    public DateTimeOffset? FirstSessionAt { get; set; }
    public bool HasEnrollmentCapacity { get; set; }
    public bool HasUnavailableSessions { get; set; }
    public string? EnrollmentUnavailableReason => FirstSessionAt <= DateTimeOffset.UtcNow ? "此班級已開課，中途報名請聯繫工作室。" :
        TermStatus != "Open" || !ScheduleConfirmed || FirstSessionAt == null ? "此課程尚未開放整期報名。" :
        HasUnavailableSessions ? "課表尚有未開放場次，請聯繫工作室。" : !HasEnrollmentCapacity ? "整期班級已額滿，請聯繫工作室。" : null;
    public bool CanEnroll => EnrollmentUnavailableReason == null;
}

public class PortalSessionData
{
    public int Id { get; set; }
    public long CourseId { get; set; }
    public string Name { get; set; } = "";
    public int CategoryId { get; set; }
    public string Category { get; set; } = "";
    public long TermId { get; set; }
    public string TermName { get; set; } = "";
    public long TeacherId { get; set; }
    public string Teacher { get; set; } = "";
    public long LocationId { get; set; }
    public string Location { get; set; } = "";
    public string Address { get; set; } = "";
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset EndsAt { get; set; }
    public bool IsExtraSession { get; set; }
    public int Capacity { get; set; }
    public int Enrolled { get; set; }
}
