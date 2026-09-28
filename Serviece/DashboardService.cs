using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Services;

public class DashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int> GetCourseCountAsync()
    {
        return await _db.Courses.CountAsync();
    }
    public async Task<int> GetTodayClassSessionCountAsync()
    {
        var taiwanOffset = TimeSpan.FromHours(8);
        var now = DateTimeOffset.UtcNow.ToOffset(taiwanOffset);

        var todayStart = new DateTimeOffset(
            now.Year,
            now.Month,
            now.Day,
            0, 0, 0,
            taiwanOffset);

        var tomorrowStart = todayStart.AddDays(1);

        return await _db.ClassSessions.CountAsync(session =>
            session.StartsAt >= todayStart &&
            session.StartsAt < tomorrowStart);
    }

    public async Task<int> GetPendingEnrollmentCountAsync()
    {
        return await _db.EnrollmentApplications.CountAsync(application =>
            application.Status == EnrollmentApplicationStatus.Pending);
    }
}