using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Controllers;

[ApiController, Route("api/portal/learning"), AutoValidateAntiforgeryToken]
[Authorize(AuthenticationSchemes = PortalAuthentication.Scheme, Roles = "Student")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class PortalLearningController(AppDbContext db, EnrollmentWorkflowService enrollment, LeaveMakeupService leaves, TimeProvider clock) : ControllerBase
{
    private long Actor => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private async Task<IActionResult> Attempt(Func<long, Task<object>> action)
    {
        try { return Ok(await action(await enrollment.RequireStudentAsync(Actor))); }
        catch (UnauthorizedAccessException) { return NotFound(new { message = "找不到您的上課或請假紀錄，請重新整理。" }); }
        catch (InvalidOperationException e) { return Conflict(new { message = e.Message }); }
        catch (DbUpdateException) { return Conflict(new { message = "課堂資料已更新，請重新整理確認請假狀態後再操作。" }); }
        catch (SqlException e) when (e.Number == 1205) { return Conflict(new { message = "課堂資料正在更新，請重新整理確認請假狀態後再操作。" }); }
    }

    [HttpGet]
    public Task<IActionResult> List() => Attempt(async studentId =>
    {
        var courses = await db.CourseEnrollments.AsNoTracking().Where(x => x.StudentId == studentId && x.Status == "Active")
            .OrderByDescending(x => x.Course.Term.StartDate).ThenBy(x => x.CourseId)
            .Select(x => new { x.CourseEnrollmentId, x.CourseId, name = x.Course.Name, termName = x.Course.Term.Name }).ToListAsync();
        var participants = await db.ClassSessionParticipants.AsNoTracking().Where(x => x.StudentId == studentId && (x.ParticipationType == "Regular" || x.ParticipationType == "Makeup")
            && x.CourseEnrollment != null && x.CourseEnrollment.StudentId == studentId && x.CourseEnrollment.Status == "Active")
            .Include(x => x.CourseEnrollment).Include(x => x.Attendance)
            .Include(x => x.ClassSession).ThenInclude(x => x.Course)
            .Include(x => x.ClassSession).ThenInclude(x => x.Teacher).ThenInclude(x => x.User)
            .Include(x => x.ClassSession).ThenInclude(x => x.StudioLocation)
            .OrderBy(x => x.ClassSession.StartsAt).ThenBy(x => x.SessionParticipantId).ToListAsync();
        var ids = participants.Select(x => x.SessionParticipantId).ToArray();
        var latestLeaves = (await db.LeaveRequests.AsNoTracking().Where(x => ids.Contains(x.SessionParticipantId))
            .Include(x => x.Entitlement).OrderByDescending(x => x.LeaveRequestId).ToListAsync())
            .GroupBy(x => x.SessionParticipantId).ToDictionary(x => x.Key, x => x.First());
        var reserved = await db.MakeupRequests.Where(x => x.Entitlement.StudentId == studentId && (x.Status == "Pending" || x.Status == "Approved"))
            .Select(x => x.MakeupEntitlementId).ToListAsync();
        var sessionIds = participants.Select(x => x.ClassSessionId).Distinct().ToArray();
        var occupied = await db.ClassSessionParticipants.Where(x => sessionIds.Contains(x.ClassSessionId) && x.Status == "Active")
            .GroupBy(x => x.ClassSessionId).Select(x => new { id = x.Key, count = x.Count() }).ToDictionaryAsync(x => x.id, x => x.count);
        var now = clock.GetUtcNow();
        return new { serverNow = now, courses = courses.Select(c => new
        {
            c.CourseId, c.name, c.termName,
            sessions = participants.Where(p => p.CourseEnrollmentId == c.CourseEnrollmentId).Select(p =>
            {
                latestLeaves.TryGetValue(p.SessionParticipantId, out var leave);
                var full = occupied.GetValueOrDefault(p.ClassSessionId) >= (p.ClassSession.CapacityOverride ?? p.ClassSession.Capacity);
                var leaveReason = LeaveMakeupService.LeaveUnavailableReason(p, now);
                var cancelReason = LeaveMakeupService.CancelLeaveUnavailableReason(p, leave, now,
                    leave != null && reserved.Contains(leave.Entitlement.MakeupEntitlementId), full);
                return new
                {
                    id = p.SessionParticipantId, sessionId = p.ClassSessionId, p.RowVersion, p.ClassSession.StartsAt, p.ClassSession.EndsAt,
                    p.ParticipationType, p.MakeupRequestId, courseName = p.ClassSession.Course.Name,
                    teacher = p.ClassSession.Teacher.User.DisplayName, location = p.ClassSession.StudioLocation.Name,
                    p.ClassSession.IsCancelled, status = p.Status, attendanceStatus = p.Attendance?.Status,
                    canLeave = leaveReason == null, leaveUnavailableReason = leaveReason,
                    canCancelLeave = cancelReason == null, cancelLeaveUnavailableReason = cancelReason,
                    leave = leave == null ? null : new { id = leave.LeaveRequestId, leave.RowVersion, leave.Status, leave.Reason,
                        entitlementId = leave.Entitlement.MakeupEntitlementId, entitlementStatus = leave.Entitlement.Status }
                };
            }).ToArray()
        }).ToArray() };
    });

    [HttpPost("leaves")]
    public Task<IActionResult> Create(PortalLeaveInput input) => Attempt(async studentId =>
    {
        if (!await db.ClassSessionParticipants.AnyAsync(x => x.SessionParticipantId == input.ParticipantId && x.StudentId == studentId))
            throw new UnauthorizedAccessException();
        var id = await leaves.CreateLeaveAsync(new LeaveCreateModel { ParticipantId = input.ParticipantId, RowVersion = input.RowVersion, Reason = input.Reason }, Actor, true);
        return new { id, message = "請假已完成，已為您保留一堂補課資格。請到「我的補課」選擇場次並送出申請。" };
    });

    [HttpPost("leaves/{id:long}/cancel")]
    public Task<IActionResult> Cancel(long id, PortalLeaveCancelInput input) => Attempt(async studentId =>
    {
        if (!await db.LeaveRequests.AnyAsync(x => x.LeaveRequestId == id && x.Participant.StudentId == studentId))
            throw new UnauthorizedAccessException();
        await leaves.CancelLeaveAsync(new LeaveMakeupActionModel { Id = id, RowVersion = input.RowVersion }, Actor, true);
        return new { message = "請假已取消，您已恢復這堂課的上課安排；這次請假產生的補課資格已撤銷。" };
    });
}
public class PortalLeaveCancelInput
{
    [Required, MinLength(8), MaxLength(8)] public byte[] RowVersion { get; set; } = [];
}
public class PortalLeaveInput : PortalLeaveCancelInput
{
    [Range(1, long.MaxValue)] public long ParticipantId { get; set; }
    [Required(ErrorMessage = "請填寫請假原因。"), StringLength(500, ErrorMessage = "請假原因最多 500 字。")] public string Reason { get; set; } = "";
}
