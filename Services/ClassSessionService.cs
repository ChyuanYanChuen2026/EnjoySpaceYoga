using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models;
using EnjoySpace.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;
using EnjoySpace.Web.Areas.Admin.ViewModels;

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
                RegisteredCount = _db.ClassSessionParticipants.Count(participant =>
                    participant.ClassSessionId == session.Id && participant.Status == "Active"),
                IsExtraSession = session.IsExtraSession,
                CapacityOverride = session.CapacityOverride,
                RowVersion = session.RowVersion,
                TermStatus = session.Course.Term.Status,
                HasApplications = _db.EnrollmentApplications.Any(x => x.CourseId == session.CourseId || x.FirstSessionId == session.Id)
                    || _db.CourseEnrollments.Any(x => x.CourseId == session.CourseId)
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
            .Include(course => course.Term)
            .Where(course => course.IsActive && course.Term.Status == TermStatusOptions.Draft
                && !_db.EnrollmentApplications.Any(x => x.CourseId == course.CourseId)
                && !_db.CourseEnrollments.Any(x => x.CourseId == course.CourseId))
            .OrderByDescending(course => course.Term.StartDate)
            .ThenBy(course => course.CourseId)
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

    public async Task<string?> CreateAsync(long courseId, long teacherId, long studioLocationId, DateTime startsAt, DateTime endsAt,
        long? expectedTermId = null, byte[]? expectedTermVersion = null)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        if (endsAt <= startsAt)
        {
            return "結束時間必須晚於開始時間。";
        }
        var course = await _db.Courses
            .AsNoTracking()
            .Include(course => course.Term)
            .SingleOrDefaultAsync(course => course.CourseId == courseId);
        if (course is null)
        {
            return "所選課程不存在，請重新選擇。";
        }

        if (!course.IsActive || course.Term.Status != TermStatusOptions.Draft)
            return "只有草稿學期的啟用課程可以新增場次；開放報名後請走取消／調課流程。";
        if (expectedTermId.HasValue)
        {
            if (course.TermId != expectedTermId || expectedTermVersion is null)
                return "課程不屬於目前的學期，請重新開啟設定。";
            await ScheduleVersion.RequireCurrentAsync(_db, course.TermId, expectedTermVersion);
            if (!await _db.Teachers.AnyAsync(x => x.TeacherId == teacherId && x.IsActive))
                return "所選老師不存在或已停用。";
            var start = new DateTimeOffset(DateTime.SpecifyKind(startsAt, DateTimeKind.Unspecified), TimeSpan.FromHours(8));
            var end = new DateTimeOffset(DateTime.SpecifyKind(endsAt, DateTimeKind.Unspecified), TimeSpan.FromHours(8));
            if (await _db.ClassSessions.AnyAsync(x => x.Course.IsActive && x.StartsAt < end && x.EndsAt > start &&
                (x.CourseId == courseId || x.TeacherId == teacherId || x.StudioLocationId == studioLocationId)))
                return "此時段與現有課程、老師或教室場次衝突，請調整時間。";
        }
        if (await HasApplicationsAsync(courseId))
            return "此課程已有報名紀錄，不能直接新增場次。";

        var startDate = DateOnly.FromDateTime(startsAt);
        var endDate = DateOnly.FromDateTime(endsAt);

        if (startDate < course.Term.StartDate || endDate > course.Term.EndDate)
        {
            return $"場次必須位於課程期間 "
                + $"{course.Term.StartDate:yyyy/MM/dd}～"
                + $"{course.Term.EndDate:yyyy/MM/dd} 內。";
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
            Capacity = expectedTermId.HasValue ? course.DefaultCapacity : (short)10,
            CapacityOverride = null
        };

        _db.ClassSessions.Add(session);
        await ScheduleVersion.TouchDraftAsync(_db, course.TermId);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return null;
    }

    public async Task DeleteAsync(int id, byte[] rowVersion)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var session = await _db.ClassSessions.Include(x => x.Course).ThenInclude(x => x.Term)
            .SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException("找不到場次，可能已被刪除。");
        if (rowVersion is null || !session.RowVersion.SequenceEqual(rowVersion))
            throw new InvalidOperationException("場次資料已變更，請重新開啟刪除確認頁。");
        if (await HasApplicationsAsync(session.CourseId)
            || await _db.EnrollmentApplications.AnyAsync(x => x.FirstSessionId == id))
            throw new InvalidOperationException("此課程已有報名紀錄，不能直接刪除場次；請保留紀錄並另行安排取消／調課。");
        if (session.IsExtraSession || session.Course.Term.Status != TermStatusOptions.Draft)
            throw new InvalidOperationException("只有草稿學期且尚無報名的課程可以直接刪除場次。");
        _db.Entry(session).Property(x => x.RowVersion).OriginalValue = rowVersion;
        await ScheduleVersion.TouchDraftAsync(_db, session.Course.TermId);
        _db.ClassSessions.Remove(session);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    private async Task<bool> HasApplicationsAsync(long courseId) =>
        await _db.EnrollmentApplications.AnyAsync(x => x.CourseId == courseId)
        || await _db.CourseEnrollments.AnyAsync(x => x.CourseId == courseId);
}
