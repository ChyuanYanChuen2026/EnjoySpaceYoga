using System.Security.Claims;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Services;
using EnjoySpace.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
namespace EnjoySpace.Web.Controllers;

[Authorize(Roles = "Admin,Teacher"), AutoValidateAntiforgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class ClassroomController(AppDbContext db, CheckInService service, AttendanceService attendance, IConfiguration config, IWebHostEnvironment env, TimeProvider clock) : Controller
{
    private long Actor => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private async Task<IActionResult> Api(Func<Task<object>> work) {
        try { return Json(await work()); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException e) { return Conflict(new { message = e.Message }); }
        catch (DbUpdateException) { return Conflict(new { message = "資料已更新，請重新整理後再操作。" }); }
        catch (Microsoft.Data.SqlClient.SqlException) { return StatusCode(503, new { message = "暫時無法更新，請重新整理後確認。" }); }
    }
    [HttpGet] public async Task<IActionResult> Index(int sessionId) {
        try { await service.RequireStaffAsync(sessionId, Actor); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        var model = await attendance.GetRosterAsync(sessionId);
        return model == null ? NotFound() : View(model);
    }
    [HttpGet] public Task<IActionResult> Snapshot(int sessionId) => Api(async () => {
        await service.RequireStaffAsync(sessionId, Actor);
        var roster = await attendance.GetRosterAsync(sessionId);
        var ids = roster!.Participants.Select(x => x.ParticipantId).ToArray();
        var records = await db.AttendanceRecords.AsNoTracking().Where(x => ids.Contains(x.SessionParticipantId)).Select(x => new { x.SessionParticipantId, x.Status, x.CheckInMethod, x.CheckedInAtUtc, x.RowVersion, x.ManualReason }).ToArrayAsync();
        var issues = await db.CheckInEvents.AsNoTracking().Where(x => x.ClassSessionId == sessionId && x.IsError)
            .OrderByDescending(x => x.CreatedAt).Take(100).Select(x => new { x.Id, student = x.User.DisplayName, x.Code, x.Message, x.CreatedAt, x.ResolvedAt, x.Resolution }).ToArrayAsync();
        var session = await db.ClassSessions.AsNoTracking().SingleAsync(x => x.Id == sessionId);
        return new { records, issues, serverNow = clock.GetUtcNow(), canDisplayQr = !session.IsCancelled && clock.GetUtcNow() >= session.StartsAt.AddMinutes(-30) && clock.GetUtcNow() < session.EndsAt };
    });
    [HttpPost] public Task<IActionResult> Qr(int sessionId) => Api(async () => {
        var baseUrl = config["Portal:PublicBaseUrl"]?.TrimEnd('/');
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) || !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment)
            || (uri.Scheme != "https" && !(env.IsDevelopment() && uri.Scheme == "http")))
            throw new InvalidOperationException("請先設定 Portal:PublicBaseUrl 為手機可連線的前台網址，再開啟 QR Code。");
        var qr = await service.IssueAsync(sessionId, Actor);
        var url = baseUrl + "/check-in#code=" + Uri.EscapeDataString(qr.Token);
        using var data = QRCodeGenerator.GenerateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        using var png = new PngByteQRCode(data);
        return new { image = "data:image/png;base64," + Convert.ToBase64String(png.GetGraphic(8)), qr.OpensAt, qr.ExpiresAt,
            warning = uri.IsLoopback ? "此網址只供本機測試，其他手機無法連線。" : null };
    });
    [HttpPost] public Task<IActionResult> Record(AttendanceEditViewModel input) => Api(async () => {
        if (!ModelState.IsValid) throw new InvalidOperationException("請確認出勤資料、版本與更正原因。");
        await service.RequireStaffAsync(input.SessionId, Actor);
        await attendance.RecordAsync(input, Actor); return new { message = "出勤已更新。" };
    });
    [HttpGet] public async Task<IActionResult> Errors() {
        if (!User.IsInRole("Admin")) return Forbid();
        return View(await db.CheckInEvents.Include(x => x.User).AsNoTracking().Where(x => x.IsError).OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync());
    }
    [HttpPost] public Task<IActionResult> Resolve(long id, string reason) => Api(async () => {
        var issue = await db.CheckInEvents.SingleAsync(x => x.Id == id);
        if (issue.ClassSessionId.HasValue) await service.RequireStaffAsync(issue.ClassSessionId.Value, Actor);
        else if (!User.IsInRole("Admin")) throw new UnauthorizedAccessException();
        if (string.IsNullOrWhiteSpace(reason) || reason.Length > 500) throw new InvalidOperationException("請填寫處理說明，最多 500 字。");
        if (issue.ResolvedAt == null) { issue.ResolvedAt = clock.GetUtcNow(); issue.ResolvedByUserId = Actor; issue.Resolution = reason.Trim(); await db.SaveChangesAsync(); }
        return new { message = "已記錄處理結果。" };
    });
}
