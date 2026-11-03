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
        .Include(x => x.SourceLeave).ThenInclude(x => x!.Participant).ThenInclude(x => x.CourseEnrollment)
        .Include(x => x.SourceLeave).ThenInclude(x => x!.Participant).ThenInclude(x => x.ClassSession).ThenInclude(x => x.Course)
        .Include(x => x.SourceCancellationParticipant).ThenInclude(x => x!.ClassSession).ThenInclude(x => x.Course);
    private void RequireSource(MakeupEntitlement entitlement, bool studentAction = false)
    {
        if (!studentAction && entitlement.SourceType != "Leave")
            throw new InvalidOperationException("停課補課資格目前僅供查詢，安排流程尚待恢復。");
        if (SourceUnavailableReason(entitlement) is string reason) throw new InvalidOperationException(reason);
    }
    private static string? SourceUnavailableReason(MakeupEntitlement entitlement)
    {
        if (entitlement.SourceType != "Leave")
            return "這筆資格來自教室停課，目前請聯繫工作室安排補課。";
        if (entitlement.SourceLeave is null || entitlement.SourceLeave.Status != "Active" || entitlement.SourceLeave.Participant.Status != "OnLeave"
            || entitlement.SourceLeave.Participant.CourseEnrollment?.Status != "Active" || !entitlement.Student.IsActive || !entitlement.Student.User.IsActive)
            return "原請假或學生報名已失效，無法使用補課資格，請聯繫工作室。";
        return null;
    }
    private void BeforeStart(ClassSession session)
    {
        if (session.StartsAt <= clock.GetUtcNow()) throw new InvalidOperationException("課堂已開始，不能再辦理此操作。");
    }

    private static (DateTimeOffset Start, DateTimeOffset EndExclusive) MakeupPeriod(Term term)
    {
        var taiwan = TimeSpan.FromHours(8);
        return (new DateTimeOffset(term.StartDate.ToDateTime(TimeOnly.MinValue), taiwan),
            new DateTimeOffset(term.EndDate.AddDays(1).ToDateTime(TimeOnly.MinValue), taiwan));
    }

    public async Task<LeaveCreateModel?> GetLeaveFormAsync(long participantId)
    {
        var p = await Participants().AsNoTracking().SingleOrDefaultAsync(x => x.SessionParticipantId == participantId);
        if (p is null) return null;
        return new LeaveCreateModel { ParticipantId = p.SessionParticipantId, RowVersion = p.RowVersion,
            StudentName = p.Student.User.DisplayName, CourseName = p.ClassSession.Course.Name,
            StartsAt = p.ClassSession.StartsAt, SessionId = p.ClassSessionId };
    }

    private async Task RequireOwnerAsync(ClassSessionParticipant participant, long actorId)
        => await RequireStudentOwnerAsync(participant.StudentId, actorId);
    private async Task RequireStudentOwnerAsync(long studentId, long actorId)
    {
        if (!await db.Students.AnyAsync(x => x.StudentId == studentId && x.UserId == actorId && x.IsActive && x.User.IsActive)
            || !await db.UserRoles.AnyAsync(x => x.UserId == actorId && x.Role.IsActive && x.Role.RoleCode == "Student"))
            throw new UnauthorizedAccessException();
    }

    private static string? CommonLeaveReason(ClassSessionParticipant p, DateTimeOffset now)
    {
        if (p.ClassSession.IsCancelled) return "這堂課已停課，無需辦理請假；如需協助，請聯繫工作室。";
        if (p.ClassSession.StartsAt <= now) return "這堂課已開始，請聯繫工作室。";
        if (p.ParticipationType != "Regular" || p.CourseEnrollment?.Status != "Active"
            || p.CourseEnrollment.StudentId != p.StudentId || p.CourseEnrollment.CourseId != p.ClassSession.CourseId)
            return "這堂課目前沒有您的有效固定期課報名，請聯繫工作室。";
        if (p.Attendance?.Status != "Scheduled") return "這堂課已有出席紀錄，請聯繫工作室。";
        return null;
    }
    public static string? LeaveUnavailableReason(ClassSessionParticipant p, DateTimeOffset now) =>
        CommonLeaveReason(p, now) ?? (p.Status == "Active" ? null : p.Status == "OnLeave" ? "您已辦理這堂課的請假。" : "這堂課的上課安排已變更，請聯繫工作室。");

    public static string? CancelLeaveUnavailableReason(ClassSessionParticipant p, LeaveRequest? leave, DateTimeOffset now, bool hasMakeup, bool full)
    {
        var reason = CommonLeaveReason(p, now);
        if (reason != null) return reason;
        if (leave?.Status != "Active" || p.Status != "OnLeave") return "目前沒有可取消的請假。";
        if (leave.Entitlement?.Status != "Available" || hasMakeup) return "這次請假的補課資格已申請或使用，需先取消補課；可至「我的補課」查看，或聯繫工作室協助。";
        return full ? "原課堂目前已額滿，無法取消請假；請聯繫工作室。" : null;
    }

    public async Task<long> CreateLeaveAsync(LeaveCreateModel input, long actorId, bool studentAction = false)
    {
        if (!studentAction) await RequireAdminAsync(actorId);
        Note(input.Reason, true);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var p = await Participants().SingleOrDefaultAsync(x => x.SessionParticipantId == input.ParticipantId)
            ?? throw new InvalidOperationException("找不到課堂名單。");
        if (studentAction) await RequireOwnerAsync(p, actorId);
        Version(p.RowVersion, input.RowVersion);
        if (LeaveUnavailableReason(p, clock.GetUtcNow()) is string reason) throw new InvalidOperationException(reason);
        var leave = new LeaveRequest { Participant = p, Reason = input.Reason.Trim(), CreatedByUserId = actorId, RequestedAtUtc = UtcNow,
            RequestSource = studentAction ? "Student" : "Admin" };
        leave.Entitlement = new MakeupEntitlement { StudentId = p.StudentId, SourceTermId = p.ClassSession.Course.TermId, CreatedAtUtc = UtcNow };
        db.LeaveRequests.Add(leave);
        p.Status = "OnLeave";
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return leave.LeaveRequestId;
    }

    public async Task CancelLeaveAsync(LeaveMakeupActionModel input, long actorId, bool studentAction = false)
    {
        if (!studentAction) await RequireAdminAsync(actorId);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var leave = await db.LeaveRequests.Include(x => x.Entitlement).Include(x => x.Participant).ThenInclude(x => x.ClassSession)
            .Include(x => x.Participant).ThenInclude(x => x.CourseEnrollment)
            .Include(x => x.Participant).ThenInclude(x => x.Attendance)
            .SingleOrDefaultAsync(x => x.LeaveRequestId == input.Id) ?? throw new InvalidOperationException("找不到請假紀錄。");
        if (studentAction) await RequireOwnerAsync(leave.Participant, actorId);
        Version(leave.RowVersion, input.RowVersion);
        var hasMakeup = await db.MakeupRequests.AnyAsync(x => x.MakeupEntitlementId == leave.Entitlement.MakeupEntitlementId && (x.Status == "Pending" || x.Status == "Approved"));
        if (CancelLeaveUnavailableReason(leave.Participant, leave, clock.GetUtcNow(), hasMakeup, false) is string reason)
            throw new InvalidOperationException(reason);
        await SessionCapacity.RequireSeatAsync(db, leave.Participant.ClassSessionId,
            studentAction ? "原課堂目前已額滿，無法取消請假；請聯繫工作室。" : null);
        leave.Status = "Cancelled"; leave.CancelledAtUtc = UtcNow; leave.CancelledByUserId = actorId;
        leave.Entitlement.Status = "Revoked"; leave.Entitlement.RevokedAtUtc = UtcNow;
        leave.Participant.Status = "Active";
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    private async Task<ClassSession> ValidateTargetAsync(MakeupEntitlement entitlement, int sessionId, bool studentAction = false)
    {
        var target = await db.ClassSessions.Include(x => x.Course).ThenInclude(x => x.Term)
            .SingleOrDefaultAsync(x => x.Id == sessionId) ?? throw new InvalidOperationException("找不到補課場次。");
        BeforeStart(target);
        var period = MakeupPeriod(entitlement.SourceTerm);
        if (target.StartsAt < period.Start || target.StartsAt >= period.EndExclusive)
            throw new InvalidOperationException("補課日期必須在原學期起迄日期內（台灣時間），可選擇其他學期的課程。");
        if (target.IsCancelled) throw new InvalidOperationException("此場次已停課，不能申請補課。");
        if (!target.Course.IsActive || !target.Course.AllowsMakeup || target.Course.Term.ScheduleConfirmedAtUtc is null
            || target.Course.Term.Status is not ("Open" or "Closed"))
            throw new InvalidOperationException("此課程未開放補課或尚未完成排課確認。");
        if (await db.ClassSessionParticipants.AnyAsync(x => x.ClassSessionId == sessionId && x.StudentId == entitlement.StudentId
            && (x.Status == "Active" || x.ParticipationType == "Regular")))
            throw new InvalidOperationException(studentAction ? "您已在這堂課的名單中，請選擇其他課堂。" : "學生已在此課堂名單中，請選其他課堂。");
        await SessionCapacity.RequireSeatAsync(db, sessionId);
        return target;
    }

    private IQueryable<ClassSession> MakeupTargets(MakeupEntitlement e)
    {
        var now = clock.GetUtcNow();
        var period = MakeupPeriod(e.SourceTerm);
        return db.ClassSessions.AsNoTracking().Where(x => x.StartsAt >= period.Start && x.StartsAt < period.EndExclusive && x.StartsAt > now
            && !x.IsCancelled
            && x.Course.IsActive && x.Course.AllowsMakeup && x.Course.Term.ScheduleConfirmedAtUtc != null
            && (x.Course.Term.Status == "Open" || x.Course.Term.Status == "Closed")
            && !db.ClassSessionParticipants.Any(p => p.ClassSessionId == x.Id && p.StudentId == e.StudentId && (p.Status == "Active" || p.ParticipationType == "Regular"))
            && db.ClassSessionParticipants.Count(p => p.ClassSessionId == x.Id && p.Status == "Active") < (x.CapacityOverride ?? x.Capacity))
            .OrderBy(x => x.StartsAt).ThenBy(x => x.Id);
    }

    public async Task<MakeupCreateModel?> GetMakeupFormAsync(long entitlementId)
    {
        var e = await Entitlements().AsNoTracking().SingleOrDefaultAsync(x => x.MakeupEntitlementId == entitlementId);
        if (e is null) return null;
        RequireSource(e);
        if (e.Status != "Available") throw new InvalidOperationException("此補課資格目前不能申請。");
        var targets = await MakeupTargets(e).Select(x => new { x.Id, x.StartsAt, x.Course.Name, Teacher = x.Teacher.User.DisplayName }).ToListAsync();
        return new MakeupCreateModel { EntitlementId = e.MakeupEntitlementId, RowVersion = e.RowVersion, StudentName = e.Student.User.DisplayName,
            PeriodStart = e.SourceTerm.StartDate, PeriodEnd = e.SourceTerm.EndDate,
            SourceDescription = $"{e.SourceTerm.Name} · {e.SourceLeave!.Participant.ClassSession.Course.Name} · {e.SourceLeave.Participant.ClassSession.StartsAt.ToOffset(TimeSpan.FromHours(8)):yyyy/MM/dd HH:mm}",
            Targets = targets.Select(x => new SelectListItem($"{x.StartsAt.ToOffset(TimeSpan.FromHours(8)):yyyy/MM/dd HH:mm} · {x.Name} · {x.Teacher}", x.Id.ToString())).ToList() };
    }

    public async Task<long> CreateMakeupAsync(MakeupCreateModel input, long actorId, bool studentAction = false, byte[]? targetRowVersion = null)
    {
        if (!studentAction) await RequireAdminAsync(actorId);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var e = await Entitlements().SingleOrDefaultAsync(x => x.MakeupEntitlementId == input.EntitlementId)
            ?? throw new InvalidOperationException("找不到補課資格。");
        if (studentAction) await RequireStudentOwnerAsync(e.StudentId, actorId);
        Version(e.RowVersion, input.RowVersion); RequireSource(e, studentAction);
        if (e.Status != "Available" || await db.MakeupRequests.AnyAsync(x => x.MakeupEntitlementId == e.MakeupEntitlementId && (x.Status == "Pending" || x.Status == "Approved")))
            throw new InvalidOperationException("此資格已有補課申請或已使用。");
        var target = await ValidateTargetAsync(e, input.TargetSessionId, studentAction);
        if (studentAction && (targetRowVersion == null || !target.RowVersion.SequenceEqual(targetRowVersion)))
            throw new InvalidOperationException("您選擇的補課場次已調整，請重新查詢並確認時間、老師與教室後送出。");
        var request = new MakeupRequest { Entitlement = e, TargetClassSessionId = input.TargetSessionId, RequestedByUserId = actorId, RequestedAtUtc = UtcNow };
        db.MakeupRequests.Add(request); e.Status = "Reserved";
        await db.SaveChangesAsync(); await tx.CommitAsync();
        return request.MakeupRequestId;
    }

    private async Task<MakeupRequest> LoadRequestAsync(long id) => await db.MakeupRequests
        .Include(x => x.Entitlement).ThenInclude(x => x.SourceTerm)
        .Include(x => x.TargetSession).Include(x => x.Entitlement).ThenInclude(x => x.Student).ThenInclude(x => x.User)
        .Include(x => x.Entitlement).ThenInclude(x => x.SourceLeave).ThenInclude(x => x!.Participant).ThenInclude(x => x.CourseEnrollment)
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
                CourseEnrollmentId = r.Entitlement.SourceLeave!.Participant.CourseEnrollmentId,
                ParticipationType = "Makeup", MakeupRequest = r, AddedByUserId = actorId
            });
            r.Status = "Approved"; r.Entitlement.Status = "Consumed"; r.Entitlement.ConsumedAtUtc = UtcNow;
        }
        else { r.Status = "Rejected"; r.Entitlement.Status = "Available"; }
        r.ReviewedByUserId = actorId; r.ReviewedAtUtc = UtcNow; r.ReviewNote = input.Note?.Trim();
        await db.SaveChangesAsync(); await tx.CommitAsync();
    }

    public static string? CancelMakeupUnavailableReason(MakeupRequest r, ClassSessionParticipant? participant, DateTimeOffset now)
    {
        if (r.Status is not ("Pending" or "Approved")) return "這筆補課申請已處理，無法取消。";
        if (r.Status == "Pending") return null;
        if (r.TargetSession.StartsAt <= now) return "這堂補課已開始，無法自行取消，請聯繫工作室。";
        if (participant?.Status != "Active" || participant.Attendance?.Status != "Scheduled")
            return "這堂補課已有出缺席紀錄或上課安排已變更，無法自行取消，請聯繫工作室。";
        return null;
    }

    public async Task CancelMakeupAsync(LeaveMakeupActionModel input, long actorId, bool studentAction = false)
    {
        if (!studentAction) await RequireAdminAsync(actorId);
        Note(input.Note, true);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var r = await LoadRequestAsync(input.Id);
        if (studentAction) await RequireStudentOwnerAsync(r.Entitlement.StudentId, actorId);
        Version(r.RowVersion, input.RowVersion);
        var p = r.Status == "Approved" ? await db.ClassSessionParticipants.Include(x => x.Attendance).SingleOrDefaultAsync(x => x.MakeupRequestId == r.MakeupRequestId) : null;
        if (CancelMakeupUnavailableReason(r, p, clock.GetUtcNow()) is string reason) throw new InvalidOperationException(reason);
        if (r.Status == "Approved")
        {
            p!.Status = "Cancelled";
        }
        r.Status = "Cancelled"; r.CancelledByUserId = actorId; r.CancelledAtUtc = UtcNow; r.CancellationReason = input.Note!.Trim();
        r.Entitlement.Status = "Available"; r.Entitlement.ConsumedAtUtc = null;
        await db.SaveChangesAsync(); await tx.CommitAsync();
    }

    private static string? ApplyMakeupUnavailableReason(MakeupEntitlement e, DateTimeOffset now, bool hasRequest) =>
        SourceUnavailableReason(e) ?? (e.Status != "Available" || hasRequest ? "這筆補課資格已有申請或已使用，請查看申請紀錄。" :
            now >= MakeupPeriod(e.SourceTerm).EndExclusive ? "這筆補課資格的可用期間已結束，請聯繫工作室。" : null);

    public async Task<object> GetStudentMakeupOptionsAsync(long id, long actorId)
    {
        var e = await Entitlements().AsNoTracking().SingleOrDefaultAsync(x => x.MakeupEntitlementId == id)
            ?? throw new UnauthorizedAccessException();
        await RequireStudentOwnerAsync(e.StudentId, actorId);
        var hasRequest = await db.MakeupRequests.AnyAsync(x => x.MakeupEntitlementId == id && (x.Status == "Pending" || x.Status == "Approved"));
        if (ApplyMakeupUnavailableReason(e, clock.GetUtcNow(), hasRequest) is string reason) throw new InvalidOperationException(reason);
        return new { id, e.RowVersion, periodStart = e.SourceTerm.StartDate, periodEnd = e.SourceTerm.EndDate,
            targets = await MakeupTargets(e).Select(x => new { x.Id, x.RowVersion, x.CourseId, courseName = x.Course.Name, x.StartsAt, x.EndsAt,
                teacher = x.Teacher.User.DisplayName, location = x.StudioLocation.Name,
                remaining = (x.CapacityOverride ?? x.Capacity) - db.ClassSessionParticipants.Count(p => p.ClassSessionId == x.Id && p.Status == "Active") }).ToListAsync() };
    }

    public async Task<object> GetStudentMakeupsAsync(long studentId, long actorId)
    {
        await RequireStudentOwnerAsync(studentId, actorId);
        var qualifications = await Entitlements().AsNoTracking().Where(x => x.StudentId == studentId).OrderByDescending(x => x.MakeupEntitlementId).ToListAsync();
        var requests = await db.MakeupRequests.AsNoTracking().Where(x => x.Entitlement.StudentId == studentId)
            .Include(x => x.TargetSession).ThenInclude(x => x.Course)
            .Include(x => x.TargetSession).ThenInclude(x => x.Teacher).ThenInclude(x => x.User)
            .Include(x => x.TargetSession).ThenInclude(x => x.StudioLocation)
            .OrderByDescending(x => x.MakeupRequestId).ToListAsync();
        var ids = requests.Select(x => x.MakeupRequestId).ToArray();
        var participants = await db.ClassSessionParticipants.AsNoTracking().Include(x => x.Attendance)
            .Where(x => x.StudentId == studentId && x.MakeupRequestId != null && ids.Contains(x.MakeupRequestId.Value))
            .ToDictionaryAsync(x => x.MakeupRequestId!.Value);
        var occupied = requests.Where(x => x.Status is "Pending" or "Approved").Select(x => x.MakeupEntitlementId).ToHashSet();
        var now = clock.GetUtcNow();
        return new { serverNow = now, entitlements = qualifications.Select(e =>
        {
            var source = e.SourceLeave?.Participant.ClassSession ?? e.SourceCancellationParticipant?.ClassSession;
            var reason = ApplyMakeupUnavailableReason(e, now, occupied.Contains(e.MakeupEntitlementId));
            return new { id = e.MakeupEntitlementId, e.Status, e.SourceType, sourceCourse = source?.Course.Name,
                sourceStartsAt = source?.StartsAt, sourceTerm = e.SourceTerm.Name, periodStart = e.SourceTerm.StartDate, periodEnd = e.SourceTerm.EndDate,
                canApply = reason == null, unavailableReason = reason };
        }).ToArray(), requests = requests.Select(r =>
        {
            var reason = CancelMakeupUnavailableReason(r, participants.GetValueOrDefault(r.MakeupRequestId), now);
            return new { id = r.MakeupRequestId, entitlementId = r.MakeupEntitlementId, r.RowVersion, r.Status, r.ReviewNote, r.CancellationReason,
                requestedAt = DateTime.SpecifyKind(r.RequestedAtUtc, DateTimeKind.Utc), r.TargetClassSessionId,
                courseName = r.TargetSession.Course.Name, r.TargetSession.StartsAt, r.TargetSession.EndsAt,
                teacher = r.TargetSession.Teacher.User.DisplayName, location = r.TargetSession.StudioLocation.Name, r.TargetSession.IsCancelled,
                canCancel = reason == null, cancelUnavailableReason = reason };
        }).ToArray() };
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
