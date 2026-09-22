using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Services;

public class ClassSessionService
{
    private readonly AppDbContext _db;

    public ClassSessionService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ClassSessionListItem>> GetAllAsync()
    {
        return await _db.ClassSessions
            .AsNoTracking()
            .OrderBy(session => session.StartsAt)
            .ThenBy(session => session.Id)
            .Select(session => new ClassSessionListItem
            {
                Id = session.Id,
                CourseName = session.Course.Name,
                TeacherName = session.Teacher.Name,
                LocationName = session.Location.Name,
                StartsAt = session.StartsAt,
                EndsAt = session.EndsAt,
                EffectiveCapacity =
                    session.CapacityOverride ?? session.Capacity
            })
            .ToListAsync();
    }

    public async Task<ClassSessionDetailModel?> GetDetailAsync(int id)
    {
        return await _db.ClassSessions
            .AsNoTracking()
            .Where(session => session.Id == id)
            .Select(session => new ClassSessionDetailModel
            {
                Id = session.Id,
                CourseName = session.Course.Name,
                TeacherName = session.Teacher.Name,
                LocationName = session.Location.Name,
                StartsAt = session.StartsAt,
                EndsAt = session.EndsAt,
                Capacity = session.Capacity,
                CapacityOverride = session.CapacityOverride
            })
            .SingleOrDefaultAsync();
    }

    public async Task<List<ClassSessionListItem>> GetCalendarSessionsAsync( DateTimeOffset start, DateTimeOffset end)
    {
        if (end <= start)
        {
            throw new ArgumentException("結束時間必須晚於開始時間。");
        }

        return await _db.ClassSessions
            .AsNoTracking()
            .Where(session =>
                session.StartsAt < end &&
                session.EndsAt > start)
            .OrderBy(session => session.StartsAt)
            .ThenBy(session => session.Id)
            .Select(session => new ClassSessionListItem
            {
                Id = session.Id,
                CourseName = session.Course.Name,
                TeacherName = session.Teacher.Name,
                LocationName = session.Location.Name,
                StartsAt = session.StartsAt,
                EndsAt = session.EndsAt,
                EffectiveCapacity = session.CapacityOverride ?? session.Capacity
            }).ToListAsync();
    }
}