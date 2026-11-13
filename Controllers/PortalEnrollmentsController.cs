using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Services;
using EnjoySpace.Web.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Controllers;

[ApiController, Route("api/portal/enrollments"), AutoValidateAntiforgeryToken]
[Authorize(AuthenticationSchemes = PortalAuthentication.Scheme, Roles = "Student")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class PortalEnrollmentsController(AppDbContext db, EnrollmentWorkflowService workflow) : ControllerBase
{
    private long Actor => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private async Task<IActionResult> Attempt(Func<Task<object>> action)
    {
        try { return Ok(await action()); }
        catch (InvalidOperationException e) { return Conflict(new { message = e.Message }); }
        catch (DbUpdateException) { return Conflict(new { message = "報名資料已更新，請先到報名進度確認是否已送出。" }); }
        catch (SqlException e) when (e.Number == 1205) { return Conflict(new { message = "報名資料正在更新，請先查詢報名進度後再試。" }); }
    }
    [HttpGet("preview/{courseId:long}")]
    public Task<IActionResult> Preview(long courseId) => Attempt(async () =>
    {
        var studentId = await workflow.RequireStudentAsync(Actor);
        var existing = await Current(studentId, courseId);
        if (existing != null) return new { existing };
        return new { details = await workflow.PreviewAsync(studentId, courseId, Actor, true), policy = EnrollmentWorkflowService.NoRefundPolicy };
    });
    [HttpGet("status/{courseId:long}")]
    public Task<IActionResult> Status(long courseId) => Attempt(async () => new { existing = await Current(await workflow.RequireStudentAsync(Actor), courseId) });

    private async Task<StudentEnrollmentState?> Current(long studentId, long courseId)
    {
        var enrolled = await db.CourseEnrollments.AsNoTracking().Where(x => x.StudentId == studentId && x.CourseId == courseId)
            .Select(x => x.Status).FirstOrDefaultAsync();
        if (enrolled == "Active") return new("Enrolled", "您已成功報名這門課程，可以查看課表與辦理請假。");
        if (enrolled != null) return new("Inactive", "您的這筆報名目前已停止上課安排，如需繼續上課，請聯繫工作室。");
        var application = await db.EnrollmentApplications.AsNoTracking().Where(x => x.StudentId == studentId && x.CourseId == courseId
            && (x.Status == EnrollmentApplicationStatus.Pending || x.Status == EnrollmentApplicationStatus.Approved))
            .OrderByDescending(x => x.Id).Select(x => new { payment = x.Payment == null ? null : x.Payment.Status }).FirstOrDefaultAsync();
        if (application == null) return null;
        return application.payment == "Confirmed"
            ? new("Paid", "已確認您的付款，工作室正在為您安排入班。")
            : new("Pending", "您已申請這門課程，請等候工作室確認。");
    }
    [HttpPost]
    public Task<IActionResult> Create(PortalEnrollmentInput input) => Attempt(async () => new
    {
        id = await workflow.CreateAsync(await workflow.RequireStudentAsync(Actor), input.CourseId, Actor,
            input.ConfirmationToken, input.ConfirmDetails, input.ConfirmPolicy, true, input.PaymentMethod)
    });
    [HttpGet]
    public Task<IActionResult> List() => Attempt(async () =>
    {
        var studentId = await workflow.RequireStudentAsync(Actor);
        return await db.EnrollmentApplications.AsNoTracking().Where(x => x.StudentId == studentId)
            .OrderByDescending(x => x.SubmittedAt).ThenByDescending(x => x.Id).Select(x => new
            {
                x.Id, x.CourseId, courseName = x.Course.Name, termName = x.Course.Term.Name,
                x.QuotedSessionCount, x.QuotedAmount, x.SubmittedAt, x.ReviewedAt, x.ReviewNote,
                amountDue = x.Payment == null ? x.QuotedAmount : x.Payment.Amount,
                status = x.Status.ToString(), paymentStatus = x.Payment == null ? null : x.Payment.Status,
                paymentMethod = x.Payment == null ? null : x.Payment.PaymentMethod
            }).ToListAsync();
    });
}
public record StudentEnrollmentState(string State, string Message);
public class PortalEnrollmentInput
{
    [Range(1, long.MaxValue)] public long CourseId { get; set; }
    [Required, StringLength(4096)] public string ConfirmationToken { get; set; } = "";
    [Required, RegularExpression("^(Cash|Transfer)$")] public string PaymentMethod { get; set; } = "";
    public bool ConfirmDetails { get; set; }
    public bool ConfirmPolicy { get; set; }
}
