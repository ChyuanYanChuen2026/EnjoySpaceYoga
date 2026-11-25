using System.Security.Claims;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Services;
using EnjoySpace.Web.Models.Entities;
using EnjoySpace.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace EnjoySpace.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Policy = "BackOffice"), AutoValidateAntiforgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class TrialDeskController(AppDbContext db, TrialBookingService service, TrialReviewSummaryService reviews) : Controller
{
    private long Actor => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    [EnjoySpace.Web.Services.BackOfficeQuery]
    [HttpGet] public async Task<IActionResult> Index(string review = "all", int page = 1) {
        if (review is not ("plans" or "bookings")) review = "all";
        var query = service.Requests.AsNoTracking();
        if (review == "plans") query = query.Where(x => reviews.PendingPlans.Any(p => p.Id == x.Id));
        if (review == "bookings") query = query.Where(x => reviews.PendingBookings.Any(b => b.TrialRequestId == x.Id));
        var model = new TrialDeskListModel { Review = review, TotalCount = await query.CountAsync() };
        model.Page = Math.Clamp(page, 1, model.PageCount);
        model.Items = await query.OrderByDescending(x => x.Status == "Pending" || x.Status == "Contacted"
                || db.TrialBookings.Any(b => b.TrialRequestId == x.Id && b.Status == "Pending"))
            .ThenByDescending(x => x.Id).Skip((model.Page - 1) * 20).Take(20).ToListAsync();
        var ids = model.Items.Select(x => x.Id).ToArray();
        model.PendingBookings = await reviews.PendingBookings.Where(x => ids.Contains(x.TrialRequestId))
            .GroupBy(x => x.TrialRequestId).Select(g => new { Id = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Id, x => x.Count);
        model.BookingReminders = await service.BookingRemindersAsync(model.Items);
        return View(model);
    }
    [EnjoySpace.Web.Services.BackOfficeQuery]
    [HttpGet] public async Task<IActionResult> Details(long id, string bookingStatus = "all") {
        var r = await service.Requests.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (r == null) return NotFound();
        ViewBag.Bookings = await db.TrialBookings.Include(x => x.ClassSession).ThenInclude(x => x.Course).Where(x => x.TrialRequestId == id).OrderBy(x => x.Id).AsNoTracking().ToListAsync();
        var quota = r.LessonPass?.SessionQuotaAtPurchase ?? r.QuotedQuota ?? r.LessonPlan.SessionQuota;
        ViewBag.ShowBookingFilters = quota >= 2;
        ViewBag.BookingStatus = quota >= 2 && bookingStatus is ("approved" or "cancelled") ? bookingStatus : "all";
        ViewBag.Referrers = await db.Students.Where(x => x.IsActive && x.User.IsActive && x.UserId != r.UserId).OrderBy(x => x.User.DisplayName)
            .Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(x.User.DisplayName, x.StudentId.ToString())).ToListAsync();
        ViewBag.Remaining = await service.RemainingAsync(r);
        return View(r);
    }
    private async Task<IActionResult> Change(long requestId, Func<Task> work) {
        try { if (!ModelState.IsValid) throw new InvalidOperationException("請確認填寫資料與版本。"); await work(); TempData["Success"] = "處理完成。"; }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException e) { TempData["Error"] = e.Message; }
        catch (DbUpdateException) { TempData["Error"] = "資料已更新，請核對後再操作。"; }
        catch (Microsoft.Data.SqlClient.SqlException) { TempData["Error"] = "暫時無法確認結果，請重新整理並核對紀錄。"; }
        return RedirectToAction(nameof(Details), new { id = requestId });
    }
    [HttpPost] public Task<IActionResult> Review(long id, byte[] rowVersion, bool approve, string? note, DateOnly from, long? referrerId)
        => Change(id, () => service.ReviewAsync(id, rowVersion, approve, note, from, referrerId, Actor));
    [HttpPost] public Task<IActionResult> ReviewBooking(long id, long requestId, byte[] rowVersion, bool approve, string? note)
        => Change(requestId, () => service.ReviewBookingAsync(id, rowVersion, approve, note, Actor));
    [HttpPost] public Task<IActionResult> CancelBooking(long id, long requestId, byte[] rowVersion, string reason)
        => Change(requestId, () => service.CancelAsync(id, rowVersion, reason, Actor, true));
    [HttpPost] public Task<IActionResult> Book(long id, byte[] rowVersion, Guid batchKey, int[] sessionIds)
        => Change(id, async () => { await service.BookAsync(id, rowVersion, batchKey, sessionIds, Actor, true); });
    [HttpGet] public async Task<IActionResult> Options(long id) {
        try { return Json(await service.OptionsAsync(id, Actor, true)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException e) { return Conflict(new { message = e.Message }); }
    }
}
