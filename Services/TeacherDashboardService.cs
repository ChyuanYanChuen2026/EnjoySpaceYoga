using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Services;

public class TeacherDashboardService(AppDbContext db, TimeProvider clock)
{
    private async Task<long> TeacherIdAsync(long actorId)
    {
        if (!(await StaffAccess.RolesAsync(db, actorId)).Contains("Teacher"))
            throw new UnauthorizedAccessException();
        return await db.Teachers.Where(x => x.UserId == actorId && x.IsActive && x.User.IsActive)
            .Select(x => x.TeacherId).SingleAsync();
    }

    // Ownership follows the session's actual teacher, including substitute teaching.
    private IQueryable<TeacherSessionModel> Sessions(long teacherId) => db.ClassSessions.AsNoTracking()
        .Where(x => x.TeacherId == teacherId)
        .Select(x => new TeacherSessionModel {
            Id = x.Id, CourseName = x.Course.Name, TermName = x.Course.Term.Name,
            LocationName = x.StudioLocation.Name, StartsAt = x.StartsAt, EndsAt = x.EndsAt,
            Capacity = x.CapacityOverride ?? x.Capacity, IsCancelled = x.IsCancelled, IsExtraSession = x.IsExtraSession,
            RegisteredCount = db.ClassSessionParticipants.Count(p => p.ClassSessionId == x.Id && p.Status == "Active")
        });

    public async Task<TeacherDashboardModel> GetAsync(long actorId, DateOnly? month)
    {
        var teacherId = await TeacherIdAsync(actorId);
        var today = DateOnly.FromDateTime(clock.GetUtcNow().ToOffset(TimeSpan.FromHours(8)).DateTime);
        var selected = month ?? today;
        var first = new DateOnly(selected.Year, selected.Month, 1);
        var start = new DateTimeOffset(first.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8));
        var end = start.AddMonths(1);
        var dayStart = new DateTimeOffset(today.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8));
        var dayEnd = dayStart.AddDays(1);
        return new TeacherDashboardModel {
            Today = today, Month = first,
            MonthSessions = await Sessions(teacherId).Where(x => x.StartsAt >= start && x.StartsAt < end)
                .OrderBy(x => x.StartsAt).ThenBy(x => x.Id).ToListAsync(),
            TodaySessions = await Sessions(teacherId).Where(x => x.StartsAt >= dayStart && x.StartsAt < dayEnd)
                .OrderBy(x => x.StartsAt).ThenBy(x => x.Id).ToListAsync(),
            TodayStudentCount = await db.ClassSessionParticipants.Where(p => p.ClassSession.TeacherId == teacherId
                && !p.ClassSession.IsCancelled && p.ClassSession.StartsAt >= dayStart && p.ClassSession.StartsAt < dayEnd
                && p.Status == "Active").Select(p => p.StudentId).Distinct().CountAsync(),
            PendingRequestCount = await db.TeacherRequests.CountAsync(x => x.TeacherId == teacherId && x.Status == "Pending")
        };
    }

    public async Task<TeacherSessionModel?> DetailAsync(long actorId, int id) =>
        await Sessions(await TeacherIdAsync(actorId)).SingleOrDefaultAsync(x => x.Id == id);
}
