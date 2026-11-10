using System.ComponentModel.DataAnnotations;
using System.Data;
using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models;
using EnjoySpace.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Services;

public class TeacherRequestService(AppDbContext db, TimeProvider clock)
{
    private async Task<bool> IsAdmin(long actor)
    {
        var roles = await StaffAccess.RolesAsync(db, actor);
        if (roles.Count == 0) throw new InvalidOperationException("請使用有效的老師或管理員帳號。");
        return roles.Contains("Admin");
    }
    private async Task TeacherAccess(long teacherId, long actor, bool admin)
    {
        if (!await db.Teachers.AnyAsync(x => x.TeacherId == teacherId && x.IsActive && (admin || (x.User.IsActive && x.UserId == actor))))
            throw new InvalidOperationException("只能代辦有效老師，或操作自己的授課申請。");
    }
    private static void Validate(object input)
    {
        if (!Validator.TryValidateObject(input, new ValidationContext(input), [], true))
            throw new InvalidOperationException("請檢查必填欄位、名額及備註長度。");
    }
    private IQueryable<TeacherRequest> Requests() => db.TeacherRequests.AsNoTracking().Include(x => x.Teacher).ThenInclude(x => x.User)
        .Include(x => x.Course).ThenInclude(x => x.Term).Include(x => x.StudioLocation);
    public async Task<PagedRecords<TeacherRequest>> ListAsync(long actor, int page, string? sort = null)
    {
        var admin = await IsAdmin(actor);
        return await TrialLessonService.PageAsync(AdminTableSorts.TeacherRequests.Apply(Requests().Where(x => admin || x.Teacher.UserId == actor), sort), page);
    }
    public async Task<TeacherRequest?> DetailAsync(long id, long actor)
    {
        var admin = await IsAdmin(actor);
        return await Requests().SingleOrDefaultAsync(x => x.Id == id && (admin || x.Teacher.UserId == actor));
    }
    public async Task PopulateAsync(TeacherRequestInput input, long actor)
    {
        var admin = await IsAdmin(actor);
        input.Teachers = await db.Teachers.Where(x => x.IsActive && (admin || (x.User.IsActive && x.UserId == actor)))
            .OrderBy(x => x.User.DisplayName).Select(x => new SelectListItem(x.User.DisplayName, x.TeacherId.ToString())).ToListAsync();
        if (!admin) input.TeacherId = await db.Teachers.Where(x => x.UserId == actor && x.IsActive).Select(x => x.TeacherId).SingleAsync();
        input.Courses = await db.Courses.Where(x => x.IsActive && x.DefaultTeacherId == input.TeacherId && x.Term.ScheduleConfirmedAtUtc != null && (x.Term.Status == "Open" || x.Term.Status == "Closed"))
            .OrderBy(x => x.Name).Select(x => new SelectListItem(x.Term.Name + " · " + x.Name, x.CourseId.ToString())).ToListAsync();
        input.Locations = await db.StudioLocations.Where(x => x.IsActive).OrderBy(x => x.Name).Select(x => new SelectListItem(x.Name, x.StudioLocationId.ToString())).ToListAsync();
        var now = clock.GetUtcNow();
        var sessions = await db.ClassSessions.Where(x => x.TeacherId == input.TeacherId && x.StartsAt > now && x.Course.IsActive
            && x.Course.Term.ScheduleConfirmedAtUtc != null && (x.Course.Term.Status == "Open" || x.Course.Term.Status == "Closed"))
            .OrderBy(x => x.StartsAt).Select(x => new { x.Id, x.Course.Name, x.StartsAt, Capacity = x.CapacityOverride ?? x.Capacity }).ToListAsync();
        input.Sessions = sessions.Select(x => new SelectListItem($"{x.StartsAt.ToOffset(TimeSpan.FromHours(8)):yyyy/MM/dd HH:mm} · {x.Name} · 目前 {x.Capacity} 人", x.Id.ToString())).ToList();
    }
    private async Task<Course> Course(long id)
    {
        var course = await db.Courses.Include(x => x.Term).SingleOrDefaultAsync(x => x.CourseId == id);
        if (course is null || !course.IsActive || course.Term.ScheduleConfirmedAtUtc is null || course.Term.Status is not ("Open" or "Closed"))
            throw new InvalidOperationException("僅接受已確認課表且開放或停止報名學期的有效課程。");
        return course;
    }
    private async Task CheckExtra(TeacherRequest request)
    {
        var course = await Course(request.CourseId);
        if (course.DefaultTeacherId != request.TeacherId) throw new InvalidOperationException("僅限課程的預設授課老師申請加開。");
        if (request.StartsAt <= clock.GetUtcNow() || request.EndsAt <= request.StartsAt)
            throw new InvalidOperationException("加開課須尚未開始，結束時間須晚於開始時間。");
        if (DateOnly.FromDateTime(request.StartsAt.ToOffset(TimeSpan.FromHours(8)).DateTime) < course.Term.StartDate
            || DateOnly.FromDateTime(request.EndsAt.ToOffset(TimeSpan.FromHours(8)).DateTime) > course.Term.EndDate)
            throw new InvalidOperationException("加開課必須在同一期的起訖日期內。");
        if (!await db.StudioLocations.AnyAsync(x => x.StudioLocationId == request.StudioLocationId && x.IsActive))
            throw new InvalidOperationException("教室不存在或已停用。");
        if (await db.ClassSessions.AnyAsync(x => x.StartsAt < request.EndsAt && x.EndsAt > request.StartsAt
            && (x.TeacherId == request.TeacherId || x.StudioLocationId == request.StudioLocationId)))
            throw new InvalidOperationException("老師或教室的時段與既有課堂衝突。");
    }
    private async Task<ClassSession> CapacitySession(int? id, long teacherId)
    {
        // Same lock order as seat assignment: lock the session before changing its capacity.
        var session = await db.ClassSessions.FromSqlInterpolated($"SELECT * FROM dbo.ClassSessions WITH (UPDLOCK, HOLDLOCK) WHERE Id = {id}").SingleOrDefaultAsync()
            ?? throw new InvalidOperationException("找不到申請課堂。");
        if (session.TeacherId != teacherId) throw new InvalidOperationException("只能申請自己授課場次的超收。");
        if (session.StartsAt <= clock.GetUtcNow()) throw new InvalidOperationException("已開始的課堂不能申請或核准超收。");
        await Course(session.CourseId);
        return session;
    }
    private static DateTimeOffset Taiwan(DateTime? value)
    {
        if (value is null) throw new InvalidOperationException("請填寫加開課的起訖時間。");
        try { return new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Unspecified), TimeSpan.FromHours(8)); }
        catch (ArgumentException) { throw new InvalidOperationException("日期超出有效範圍。"); }
    }
    public async Task<long> SubmitAsync(TeacherRequestInput input, long actor)
    {
        var admin = await IsAdmin(actor); Validate(input);
        await TeacherAccess(input.TeacherId, actor, admin);
        if (input.SubmissionKey == Guid.Empty || input.Kind is not ("ExtraSession" or "Capacity")) throw new InvalidOperationException("申請類型或表單無效。");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var existing = await db.TeacherRequests.SingleOrDefaultAsync(x => x.SubmissionKey == input.SubmissionKey);
        if (existing != null)
        {
            if (existing.SubmittedByUserId != actor) throw new InvalidOperationException("此表單不屬於目前帳號。");
            return existing.Id;
        }
        var request = new TeacherRequest { SubmissionKey = input.SubmissionKey, Kind = input.Kind, TeacherId = input.TeacherId,
            Reason = input.Reason.Trim(), RequestedCapacity = input.RequestedCapacity, SubmittedByUserId = actor, SubmittedAt = clock.GetUtcNow() };
        if (input.Kind == "ExtraSession")
        {
            request.CourseId = input.CourseId; request.StudioLocationId = input.StudioLocationId;
            request.StartsAt = Taiwan(input.StartsAtTaiwan); request.EndsAt = Taiwan(input.EndsAtTaiwan);
            await CheckExtra(request);
            if (await db.TeacherRequests.AnyAsync(x => x.Kind == "ExtraSession" && x.Status == "Pending" && x.StartsAt < request.EndsAt && x.EndsAt > request.StartsAt
                && (x.TeacherId == request.TeacherId || x.StudioLocationId == request.StudioLocationId)))
                throw new InvalidOperationException("此老師或教室的時段已有待審加開申請。");
        }
        else
        {
            var session = await CapacitySession(input.TargetSessionId, input.TeacherId);
            request.TargetSessionId = session.Id; request.CourseId = session.CourseId; request.StudioLocationId = session.StudioLocationId;
            request.StartsAt = session.StartsAt; request.EndsAt = session.EndsAt; request.TargetVersion = session.RowVersion;
            request.OriginalCapacity = session.CapacityOverride ?? session.Capacity;
            if (request.RequestedCapacity <= request.OriginalCapacity) throw new InvalidOperationException("新總名額必須大於目前名額。");
            if (await db.TeacherRequests.AnyAsync(x => x.TargetSessionId == session.Id && x.Status == "Pending"))
                throw new InvalidOperationException("此課堂已有待審超收申請。");
        }
        db.TeacherRequests.Add(request); await db.SaveChangesAsync(); await tx.CommitAsync(); return request.Id;
    }
    public async Task ReviewAsync(TeacherRequestReview input, long actor)
    {
        if (!await IsAdmin(actor)) throw new InvalidOperationException("僅管理員可以審核。");
        Validate(input);
        if (input.Decision is not ("Approve" or "Reject")) throw new InvalidOperationException("請選擇核准或退回。");
        if (!input.ConfirmDetails) throw new InvalidOperationException("請勾選已核對申請內容。");
        if (input.Decision == "Reject" && string.IsNullOrWhiteSpace(input.Note)) throw new InvalidOperationException("退回時必須填寫原因。");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var request = await db.TeacherRequests.SingleOrDefaultAsync(x => x.Id == input.Id) ?? throw new InvalidOperationException("找不到申請。");
        if (input.RowVersion is null || !request.RowVersion.SequenceEqual(input.RowVersion) || request.Status != "Pending")
            throw new InvalidOperationException("申請已變更或已審核，請重新載入。");
        if (input.Decision == "Approve")
        {
            await TeacherAccess(request.TeacherId, actor, true);
            if (request.Kind == "ExtraSession")
            {
                await CheckExtra(request);
                request.CreatedSession = new ClassSession { CourseId = request.CourseId, TeacherId = request.TeacherId,
                    StudioLocationId = request.StudioLocationId, StartsAt = request.StartsAt, EndsAt = request.EndsAt,
                    Capacity = request.RequestedCapacity, IsExtraSession = true };
            }
            else
            {
                var session = await CapacitySession(request.TargetSessionId, request.TeacherId);
                if (!session.RowVersion.SequenceEqual(request.TargetVersion ?? []) || request.RequestedCapacity <= (session.CapacityOverride ?? session.Capacity))
                    throw new InvalidOperationException("課堂資料或名額已變更，請退回後重新申請。");
                session.CapacityOverride = request.RequestedCapacity;
            }
            request.Status = "Approved";
        }
        else request.Status = "Rejected";
        request.ReviewedByUserId = actor; request.ReviewedAt = clock.GetUtcNow(); request.ReviewNote = input.Note?.Trim();
        await db.SaveChangesAsync(); await tx.CommitAsync();
    }
}
