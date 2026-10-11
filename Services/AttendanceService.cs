using System.Data;
using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Services;

public class AttendanceService(AppDbContext db, TimeProvider clock)
{
    // Called inside the enrollment approval transaction; both enrollment and roster commit together.
    public static async Task AddEnrollmentRosterAsync(AppDbContext db, CourseEnrollment enrollment, long actorId)
    {
        var sessionIds = await db.ClassSessions.Where(x => x.CourseId == enrollment.CourseId)
            .Select(x => x.Id).ToListAsync();
        db.ClassSessionParticipants.AddRange(sessionIds.Select(id => new ClassSessionParticipant
        {
            ClassSessionId = id, StudentId = enrollment.StudentId,
            CourseEnrollment = enrollment, AddedByUserId = actorId
        }));
    }

    public async Task<AttendanceRosterViewModel?> GetRosterAsync(int sessionId)
    {
        var roster = await db.ClassSessions.AsNoTracking().Where(x => x.Id == sessionId)
            .Select(x => new AttendanceRosterViewModel
            {
                SessionId = x.Id, CourseName = x.Course.Name, TeacherName = x.Teacher.User.DisplayName,
                LocationName = x.StudioLocation.Name, StartsAt = x.StartsAt, EndsAt = x.EndsAt
            }).SingleOrDefaultAsync();
        if (roster is null) return null;
        roster.HasStarted = roster.StartsAt <= clock.GetUtcNow();
        roster.Participants = await db.ClassSessionParticipants.AsNoTracking()
            .Where(x => x.ClassSessionId == sessionId).OrderBy(x => x.Student.User.DisplayName).ThenBy(x => x.SessionParticipantId)
            .Select(x => new AttendanceRowViewModel
            {
                ParticipantId = x.SessionParticipantId, StudentName = x.Student.User.DisplayName,
                StudentNo = x.Student.StudentNo, Status = x.Attendance.Status,
                IsActive = x.Status == "Active" && x.CourseEnrollment.Status == "Active",
                ParticipantStatus = x.Status, ParticipationType = x.ParticipationType,
                RowVersion = x.Attendance.RowVersion, ManualReason = x.Attendance.ManualReason,
                UpdatedBy = x.Attendance.CheckedInByUser == null ? null : x.Attendance.CheckedInByUser.DisplayName,
                UpdatedAtUtc = x.Attendance.UpdatedAtUtc
            }).ToListAsync();
        return roster;
    }

    public async Task RecordAsync(AttendanceEditViewModel input, long actorId)
    {
        if (!await db.UserRoles.AnyAsync(x => x.UserId == actorId && x.User.IsActive && x.Role.IsActive && x.Role.RoleCode == "Admin"))
            throw new InvalidOperationException("請使用有效的管理員帳號登入。");
        if (input.Status is null || !AttendanceStatuses.Labels.ContainsKey(input.Status))
            throw new InvalidOperationException("請選擇有效的出缺席狀態。");
        if (input.ManualReason?.Length > 500)
            throw new InvalidOperationException("備註最多 500 字。");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var record = await db.AttendanceRecords.Include(x => x.Participant).ThenInclude(x => x.ClassSession)
            .Include(x => x.Participant).ThenInclude(x => x.CourseEnrollment)
            .SingleOrDefaultAsync(x => x.SessionParticipantId == input.ParticipantId && x.Participant.ClassSessionId == input.SessionId)
            ?? throw new InvalidOperationException("找不到此場次的學員名單，請重新整理。");
        if (record.Participant.Status != "Active" || record.Participant.CourseEnrollment.Status != "Active")
            throw new InvalidOperationException("此學員已不在有效名單中，無法點名。");
        if (record.Participant.ClassSession.StartsAt > clock.GetUtcNow())
            throw new InvalidOperationException("課堂尚未開始，暫時不能登記出缺席。");
        if (input.RowVersion is null || !record.RowVersion.SequenceEqual(input.RowVersion))
            throw new InvalidOperationException("點名紀錄已被其他人修改，請重新整理後確認。");
        if (record.Status != "Scheduled" && record.Status != input.Status && string.IsNullOrWhiteSpace(input.ManualReason))
            throw new InvalidOperationException("更正已登記的出缺席時，請填寫原因。");
        record.Status = input.Status;
        record.CheckInMethod = "Manual";
        record.CheckedInByUserId = actorId;
        record.ManualReason = input.ManualReason?.Trim();
        record.UpdatedAtUtc = clock.GetUtcNow().UtcDateTime;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }
}
