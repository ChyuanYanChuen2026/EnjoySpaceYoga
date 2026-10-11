using System.Data;
using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Services;

public class LeaveMakeupService(AppDbContext db, TimeProvider clock)
{
    private DateTime UtcNow => clock.GetUtcNow().UtcDateTime;
    private async Task RequireAdminAsync(long actorId)
    {
        if (!await db.UserRoles.AnyAsync(x => x.UserId == actorId && x.User.IsActive && x.Role.IsActive && x.Role.RoleCode == "Admin"))
            throw new InvalidOperationException("請使用有效的管理員帳號登入。");
    }
    private static void Version(byte[] actual, byte[]? submitted)
    {
        if (submitted is null || !actual.SequenceEqual(submitted))
            throw new InvalidOperationException("資料已變更，請重新整理並核對後操作。");
    }
    private static void Note(string? note, bool required)
    {
        if (note?.Length > 500 || (required && string.IsNullOrWhiteSpace(note)))
            throw new InvalidOperationException("請填寫 1 至 500 字的原因。" );
    }
    private IQueryable<ClassSessionParticipant> Participants() => db.ClassSessionParticipants
        .Include(x => x.ClassSession).ThenInclude(x => x.Course)
        .Include(x => x.Student).ThenInclude(x => x.User)
        .Include(x => x.CourseEnrollment).Include(x => x.Attendance);
    private IQueryable<MakeupEntitlement> Entitlements() => db.MakeupEntitlements
        .Include(x => x.Student).ThenInclude(x => x.User).Include(x => x.SourceTerm)
        .Include(x => x.SourceLeave).ThenInclude(x => x.Participant).ThenInclude(x => x.CourseEnrollment)
        .Include(x => x.SourceLeave).ThenInclude(x => x.Participant).ThenInclude(x => x.ClassSession).ThenInclude(x => x.Course);
    private void RequireSource(MakeupEntitlement entitlement)
    {
        if (entitlement.SourceLeave.Status != "Active" || entitlement.SourceLeave.Participant.Status != "OnLeave"
            || entitlement.SourceLeave.Participant.CourseEnrollment.Status != "Active" || !entitlement.Student.IsActive || !entitlement.Student.User.IsActive)
            throw new InvalidOperationException("原請假或學生報名已失效，無法使用補課資格。");
    }
    private void BeforeStart(ClassSession session)
    {
        if (session.StartsAt <= clock.GetUtcNow()) throw new InvalidOperationException("課堂已開始，不能再辦理此操作。");
    }

    public async Task<LeaveCreateModel?> GetLeaveFormAsync(long participantId)
    {
        var p = await Participants().AsNoTracking().SingleOrDefaultAsync(x => x.SessionParticipantId == participantId);
        if (p is null) return null;
        return new LeaveCreateModel { ParticipantId = p.SessionParticipantId, RowVersion = p.RowVersion,
            StudentName = p.Student.User.DisplayName, CourseName = p.ClassSession.Course.Name,
            StartsAt = p.ClassSession.StartsAt, SessionId = p.ClassSessionId };
    }

    public async Task<long> CreateLeaveAsync(LeaveCreateModel input, long actorId)
    {
        await RequireAdminAsync(actorId);
        Note(input.Reason, true);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var p = await Participants().SingleOrDefaultAsync(x => x.SessionParticipantId == input.ParticipantId)
            ?? throw new InvalidOperationException("找不到課堂名單。");
        Version(p.RowVersion, input.RowVersion);
        BeforeStart(p.ClassSession);
        if (p.ParticipationType != "Regular" || p.Status != "Active" || p.CourseEnrollment.Status != "Active" || p.Attendance.Status != "Scheduled")
            throw new InvalidOperationException("僅限有效、未點名的固定期課辦理請假；補課請使用取消補課。");
        var leave = new LeaveRequest { Participant = p, Reason = input.Reason.Trim(), CreatedByUserId = actorId, RequestedAtUtc = UtcNow };
        leave.Entitlement = new MakeupEntitlement { StudentId = p.StudentId, SourceTermId = p.ClassSession.Course.TermId, CreatedAtUtc = UtcNow };
        db.LeaveRequests.Add(leave);
        p.Status = "OnLeave";
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return leave.LeaveRequestId;
    }

    public async Task CancelLeaveAsync(LeaveMakeupActionModel input, long actorId)
    {
        await RequireAdminAsync(actorId);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var leave = await db.LeaveRequests.Include(x => x.Entitlement).Include(x => x.Participant).ThenInclude(x => x.ClassSession)
            .Include(x => x.Participant).ThenInclude(x => x.CourseEnrollment)
            .SingleOrDefaultAsync(x => x.LeaveRequestId == input.Id) ?? throw new InvalidOperationException("找不到請假紀錄。");
        Version(leave.RowVersion, input.RowVersion);
        BeforeStart(leave.Participant.ClassSession);
        if (leave.Status != "Active" || leave.Participant.Status != "OnLeave" || leave.Participant.CourseEnrollment.Status != "Active")
            throw new InvalidOperationException("請假或原報名已失效。");
        if (leave.Entitlement.Status != "Available" || await db.MakeupRequests.AnyAsync(x => x.MakeupEntitlementId == leave.Entitlement.MakeupEntitlementId && (x.Status == "Pending" || x.Status == "Approved")))
            throw new InvalidOperationException("請先取消待審或已核准的補課，再取消原請假。");
        await SessionCapacity.RequireSeatAsync(db, leave.Participant.ClassSessionId);
        leave.Status = "Cancelled"; leave.CancelledAtUtc = UtcNow; leave.CancelledByUserId = actorId;
        leave.Entitlement.Status = "Revoked"; leave.Entitlement.RevokedAtUtc = UtcNow;
        leave.Participant.Status = "Active";
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    private async Task<ClassSession> ValidateTargetAsync(MakeupEntitlement entitlement, int sessionId)
    {
        var target = await db.ClassSessions.Include(x => x.Course).ThenInclude(x => x.Term)
            .SingleOrDefaultAsync(x => x.Id == sessionId) ?? throw new InvalidOperationException("找不到補課場次。");
        BeforeStart(target);
        if (target.Course.TermId != entitlement.SourceTermId) throw new InvalidOperationException("目前僅開放同一期補課。");
        if (!target.Course.IsActive || !target.Course.AllowsMakeup || target.Course.Term.ScheduleConfirmedAtUtc is null
            || target.Course.Term.Status is not ("Open" or "Closed"))
            throw new InvalidOperationException("此課程未開放補課或尚未完成排課確認。");
        if (await db.ClassSessionParticipants.AnyAsync(x => x.ClassSessionId == sessionId && x.StudentId == entitlement.StudentId
            && (x.Status == "Active" || x.ParticipationType == "Regular")))
            throw new InvalidOperationException("學生已在此課堂名單中，請選其他課堂。");
        await SessionCapacity.RequireSeatAsync(db, sessionId);
        return target;
    }

    public async Task<MakeupCreateModel?> GetMakeupFormAsync(long entitlementId)
    {
        var e = await Entitlements().AsNoTracking().SingleOrDefaultAsync(x => x.MakeupEntitlementId == entitlementId);
        if (e is null) return null;
        RequireSource(e);
        if (e.Status != "Available") throw new InvalidOperationException("此補課資格目前不能申請。");
        var now = clock.GetUtcNow();
        var targets = await db.ClassSessions.AsNoTracking().Where(x => x.Course.TermId == e.SourceTermId && x.StartsAt > now
            && x.Course.IsActive && x.Course.AllowsMakeup && x.Course.Term.ScheduleConfirmedAtUtc != null
            && (x.Course.Term.Status == "Open" || x.Course.Term.Status == "Closed")
            && !db.ClassSessionParticipants.Any(p => p.ClassSessionId == x.Id && p.StudentId == e.StudentId && (p.Status == "Active" || p.ParticipationType == "Regular"))
            && db.ClassSessionParticipants.Count(p => p.ClassSessionId == x.Id && p.Status == "Active") < (x.CapacityOverride ?? x.Capacity))
            .OrderBy(x => x.StartsAt).Select(x => new { x.Id, x.StartsAt, x.Course.Name, Teacher = x.Teacher.User.DisplayName }).ToListAsync();
        return new MakeupCreateModel { EntitlementId = e.MakeupEntitlementId, RowVersion = e.RowVersion, StudentName = e.Student.User.DisplayName,
            SourceDescription = $"{e.SourceTerm.Name} · {e.SourceLeave.Participant.ClassSession.Course.Name} · {e.SourceLeave.Participant.ClassSession.StartsAt.ToOffset(TimeSpan.FromHours(8)):yyyy/MM/dd HH:mm}",
            Targets = targets.Select(x => new SelectListItem($"{x.StartsAt.ToOffset(TimeSpan.FromHours(8)):yyyy/MM/dd HH:mm} · {x.Name} · {x.Teacher}", x.Id.ToString())).ToList() };
    }

    public async Task<long> CreateMakeupAsync(MakeupCreateModel input, long actorId)
    {
        await RequireAdminAsync(actorId);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var e = await Entitlements().SingleOrDefaultAsync(x => x.MakeupEntitlementId == input.EntitlementId)
            ?? throw new InvalidOperationException("找不到補課資格。");
        Version(e.RowVersion, input.RowVersion); RequireSource(e);
        if (e.Status != "Available" || await db.MakeupRequests.AnyAsync(x => x.MakeupEntitlementId == e.MakeupEntitlementId && (x.Status == "Pending" || x.Status == "Approved")))
            throw new InvalidOperationException("此資格已有補課申請或已使用。");
        await ValidateTargetAsync(e, input.TargetSessionId);
        var request = new MakeupRequest { Entitlement = e, TargetClassSessionId = input.TargetSessionId, RequestedByUserId = actorId, RequestedAtUtc = UtcNow };
        db.MakeupRequests.Add(request); e.Status = "Reserved";
        await db.SaveChangesAsync(); await tx.CommitAsync();
        return request.MakeupRequestId;
    }

    private async Task<MakeupRequest> LoadRequestAsync(long id) => await db.MakeupRequests
        .Include(x => x.TargetSession).Include(x => x.Entitlement).ThenInclude(x => x.Student).ThenInclude(x => x.User)
        .Include(x => x.Entitlement).ThenInclude(x => x.SourceLeave).ThenInclude(x => x.Participant).ThenInclude(x => x.CourseEnrollment)
        .SingleOrDefaultAsync(x => x.MakeupRequestId == id) ?? throw new InvalidOperationException("找不到補課申請。");

    public async Task ReviewAsync(LeaveMakeupActionModel input, long actorId)
    {
        await RequireAdminAsync(actorId); Note(input.Note, !input.Approve);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var r = await LoadRequestAsync(input.Id); Version(r.RowVersion, input.RowVersion);
        if (r.Status != "Pending" || r.Entitlement.Status != "Reserved") throw new InvalidOperationException("此申請已處理，請重新整理。");
        if (input.Approve)
        {
            RequireSource(r.Entitlement);
            await ValidateTargetAsync(r.Entitlement, r.TargetClassSessionId);
            db.ClassSessionParticipants.Add(new ClassSessionParticipant
            {
                ClassSessionId = r.TargetClassSessionId, StudentId = r.Entitlement.StudentId,
                CourseEnrollmentId = r.Entitlement.SourceLeave.Participant.CourseEnrollmentId,
                ParticipationType = "Makeup", MakeupRequest = r, AddedByUserId = actorId
            });
            r.Status = "Approved"; r.Entitlement.Status = "Consumed"; r.Entitlement.ConsumedAtUtc = UtcNow;
        }
        else { r.Status = "Rejected"; r.Entitlement.Status = "Available"; }
        r.ReviewedByUserId = actorId; r.ReviewedAtUtc = UtcNow; r.ReviewNote = input.Note?.Trim();
        await db.SaveChangesAsync(); await tx.CommitAsync();
    }

    public async Task CancelMakeupAsync(LeaveMakeupActionModel input, long actorId)
    {
        await RequireAdminAsync(actorId); Note(input.Note, true);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var r = await LoadRequestAsync(input.Id); Version(r.RowVersion, input.RowVersion);
        if (r.Status is not ("Pending" or "Approved")) throw new InvalidOperationException("此申請已處理，無法取消。");
        if (r.Status == "Approved")
        {
            BeforeStart(r.TargetSession);
            var p = await db.ClassSessionParticipants.Include(x => x.Attendance).SingleAsync(x => x.MakeupRequestId == r.MakeupRequestId);
            if (p.Status != "Active" || p.Attendance.Status != "Scheduled") throw new InvalidOperationException("此補課已有出缺席紀錄或名單已變更，不能取消。");
            p.Status = "Cancelled";
        }
        r.Status = "Cancelled"; r.CancelledByUserId = actorId; r.CancelledAtUtc = UtcNow; r.CancellationReason = input.Note!.Trim();
        r.Entitlement.Status = "Available"; r.Entitlement.ConsumedAtUtc = null;
        await db.SaveChangesAsync(); await tx.CommitAsync();
    }

    public async Task<LeaveMakeupListModel> GetListAsync(string? tab, int page)
    {
        var result = new LeaveMakeupListModel { Tab = tab is "entitlements" or "requests" ? tab : "leaves", Now = clock.GetUtcNow() };
        result.TotalCount = result.Tab switch { "entitlements" => await db.MakeupEntitlements.CountAsync(), "requests" => await db.MakeupRequests.CountAsync(), _ => await db.LeaveRequests.CountAsync() };
        result.Page = Math.Clamp(page, 1, result.PageCount);
        var skip = (result.Page - 1) * 20;
        if (result.Tab == "leaves") result.Leaves = await db.LeaveRequests.AsNoTracking().Include(x => x.Entitlement)
            .Include(x => x.Participant).ThenInclude(x => x.Student).ThenInclude(x => x.User)
            .Include(x => x.Participant).ThenInclude(x => x.ClassSession).ThenInclude(x => x.Course)
            .OrderByDescending(x => x.LeaveRequestId).Skip(skip).Take(20).ToListAsync();
        else if (result.Tab == "entitlements") result.Entitlements = await Entitlements().AsNoTracking()
            .OrderByDescending(x => x.MakeupEntitlementId).Skip(skip).Take(20).ToListAsync();
        else result.Requests = await db.MakeupRequests.AsNoTracking().Include(x => x.Entitlement).ThenInclude(x => x.Student).ThenInclude(x => x.User)
            .Include(x => x.TargetSession).ThenInclude(x => x.Course)
            .OrderBy(x => x.Status == "Pending" ? 0 : 1).ThenByDescending(x => x.MakeupRequestId).Skip(skip).Take(20).ToListAsync();
        return result;
    }
}
