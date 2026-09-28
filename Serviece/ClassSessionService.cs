using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models;
using EnjoySpace.Web.Models.Entities;
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
                TeacherName = session.Teacher.User!.DisplayName,
                LocationName = session.StudioLocation.Name,
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
                TeacherName = session.Teacher.User!.DisplayName,
                LocationName = session.StudioLocation.Name,
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
                TeacherName = session.Teacher.User!.DisplayName,
                LocationName = session.StudioLocation.Name,
                StartsAt = session.StartsAt,
                EndsAt = session.EndsAt,
                EffectiveCapacity = session.CapacityOverride ?? session.Capacity
            }).ToListAsync();
    }

    public async Task<List<Course>> GetCourseOptionsAsync()
    {
        return await _db.Courses
            .AsNoTracking()
            .OrderByDescending(course => course.StartDate)
            .ThenBy(course => course.Id)
            .ToListAsync();
    }

    public async Task<List<Teacher>> GetTeacherOptionsAsync()
    {
        return await _db.Teachers
            .AsNoTracking()
            .Include(teacher => teacher.User)
            .OrderBy(teacher => teacher.User!.DisplayName)
            .ThenBy(teacher => teacher.TeacherId)
            .ToListAsync();
    }

    public async Task<List<StudioLocation>> GetLocationOptionsAsync()
    {
        return await _db.StudioLocations
            .AsNoTracking()
            .Where(location => location.IsActive)
            .OrderBy(location => location.Name)
            .ThenBy(location => location.StudioLocationId)
            .ToListAsync();
    }

    public async Task<string?> CreateAsync( int courseId, long teacherId,  long studioLocationId, DateTime startsAt, DateTime endsAt)
    {
        if (endsAt <= startsAt)
        {
            return "結束時間必須晚於開始時間。";
        }
        var course = await _db.Courses
            .AsNoTracking()
            .SingleOrDefaultAsync(course => course.Id == courseId);
        if (course is null)
        {
            return "所選課程不存在，請重新選擇。";
        }

        var startDate = DateOnly.FromDateTime(startsAt);
        var endDate = DateOnly.FromDateTime(endsAt);

        if (startDate < course.StartDate || endDate > course.EndDate)
        {
            return $"場次必須位於課程期間 "
                + $"{course.StartDate:yyyy/MM/dd}～"
                + $"{course.EndDate:yyyy/MM/dd} 內。";
        }

        var teacherExists = await _db.Teachers
            .AnyAsync(teacher => teacher.TeacherId == teacherId);
        if (!teacherExists)
        {
            return "所選老師不存在，請重新選擇。";
        }

        var locationExists = await _db.StudioLocations
            .AnyAsync(location =>
                location.StudioLocationId == studioLocationId &&
                location.IsActive);
        if (!locationExists)
        {
            return "所選地點不存在或已停用，請重新選擇。";
        }

        var taiwanOffset = TimeSpan.FromHours(8);
        var session = new ClassSession
        {
            CourseId = courseId,
            TeacherId = teacherId,
            StudioLocationId = studioLocationId,
            StartsAt = new DateTimeOffset(
                DateTime.SpecifyKind(startsAt, DateTimeKind.Unspecified),
                taiwanOffset),
            EndsAt = new DateTimeOffset(
                DateTime.SpecifyKind(endsAt, DateTimeKind.Unspecified),
                taiwanOffset),
            Capacity = 10,
            CapacityOverride = null
        };

        _db.ClassSessions.Add(session);
        await _db.SaveChangesAsync();

        return null;
    }
}