using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace EnjoySpace.Web.Controllers;

[ApiController, Route("api/portal/my-trials"), AutoValidateAntiforgeryToken, PortalFormValidation]
[Authorize(AuthenticationSchemes = PortalAuthentication.Scheme, Roles = "Student")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class PortalTrialsController(AppDbContext db, TrialBookingService service, TimeProvider clock) : ControllerBase
{
    private long Actor => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private async Task<IActionResult> Run(Func<Task<object>> work) {
        try { await service.StudentAsync(Actor); return Ok(await work()); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException e) { return Conflict(new { message = e.Message }); }
        catch (DbUpdateException) { return Conflict(new { message = "資料已更新，請重新整理紀錄後再操作。" }); }
        catch (Microsoft.Data.SqlClient.SqlException) { return StatusCode(503, new { message = "暫時無法確認送出結果，請重新整理紀錄，勿重複送出。" }); }
    }
    [HttpGet] public Task<IActionResult> Index() => Run(async () => {
        var parents = await service.Requests.AsNoTracking().Where(x => x.UserId == Actor).OrderByDescending(x => x.Id).ToArrayAsync();
        var results = new List<object>();
        foreach (var r in parents) {
            var issue = TrialBookingService.Unavailable(r, DateOnly.FromDateTime(clock.GetUtcNow().ToOffset(TimeSpan.FromHours(8)).DateTime));
            var referral = r.RequiresReferrerAtRequest ?? r.LessonPlan.RequiresReferrer;
            var bookings = await db.TrialBookings.AsNoTracking().Where(x => x.TrialRequestId == r.Id).OrderBy(x => x.ClassSession.StartsAt)
                .Select(x => new { x.Id, x.RowVersion, x.Status, x.ReviewNote, x.ClassSessionId, name = x.ClassSession.Course.Name,
                    x.ClassSession.StartsAt, x.ClassSession.EndsAt, teacher = x.ClassSession.Teacher.User.DisplayName, location = x.ClassSession.StudioLocation.Name,
                    attendance = db.ClassSessionParticipants.Where(p => p.TrialBookingId == x.Id).Select(p => p.Attendance.Status).FirstOrDefault(),
                    canCancel = x.Status == "Pending" || (x.Status == "Approved" && x.ClassSession.StartsAt > clock.GetUtcNow()
                        && db.ClassSessionParticipants.Any(p => p.TrialBookingId == x.Id && p.Attendance.Status == "Scheduled" && p.Attendance.CheckInMethod == null)) }).ToArrayAsync();
            results.Add(new { r.Id, r.RowVersion, r.PlanName, r.QuotedPrice, r.Status, r.ReviewNote, r.ReferrerName,
                quota = r.LessonPass?.SessionQuotaAtPurchase ?? r.QuotedQuota ?? r.LessonPlan.SessionQuota,
                remaining = await service.RemainingAsync(r), paymentStatus = r.LessonPass?.Payment.Status,
                validFromDate = r.LessonPass?.ValidFromDate, validUntilDate = r.LessonPass?.ValidUntilDate,
                canBook = issue == null && !referral, schedulingMessage = issue ?? (referral ? "請直接聯繫工作室，安排免費體驗上課。" : "可排課，請選擇希望上的場次。"), bookings });
        }
        return results;
    });
    [HttpGet("{id:long}/options")] public Task<IActionResult> Options(long id) => Run(() => service.OptionsAsync(id, Actor));
    [HttpPost("{id:long}/bookings")] public Task<IActionResult> Book(long id, TrialBookingInput input) => Run(async () => new { ids = await service.BookAsync(id, input.RowVersion, input.BatchKey, input.SessionIds, Actor), message = "預約已送出，工作室將逐堂審核；待審預約已占用堂數。" });
    [HttpPost("bookings/{id:long}/cancel")] public Task<IActionResult> Cancel(long id, TrialCancelInput input) => Run(async () => { await service.CancelAsync(id, input.RowVersion, input.Reason, Actor); return new { message = "預約已取消，堂數已釋放。" }; });
}
public class TrialBookingInput {
    [Required(ErrorMessage = "缺少資料版本，請重新整理頁面後再試。"), MinLength(8, ErrorMessage = "資料版本無效，請重新整理頁面後再試。"), MaxLength(8, ErrorMessage = "資料版本無效，請重新整理頁面後再試。")]
    public byte[] RowVersion { get; set; } = [];
    public Guid BatchKey { get; set; }
    [Required(ErrorMessage = "請選擇預約場次。"), MinLength(1, ErrorMessage = "請至少選擇一堂預約場次。"), MaxLength(20, ErrorMessage = "單次最多預約 20 堂。")]
    public int[] SessionIds { get; set; } = [];
}
public class TrialCancelInput {
    [Required(ErrorMessage = "缺少資料版本，請重新整理頁面後再試。"), MinLength(8, ErrorMessage = "資料版本無效，請重新整理頁面後再試。"), MaxLength(8, ErrorMessage = "資料版本無效，請重新整理頁面後再試。")]
    public byte[] RowVersion { get; set; } = [];
    [Required(ErrorMessage = "請填寫取消原因。"), StringLength(500, ErrorMessage = "取消原因最多 500 字。")]
    public string Reason { get; set; } = "";
}
