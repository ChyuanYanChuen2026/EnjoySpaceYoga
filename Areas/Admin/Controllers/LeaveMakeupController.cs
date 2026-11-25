using System.Security.Claims;
using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Policy = "BackOffice")]
public class LeaveMakeupController(LeaveMakeupService workflow) : Controller
{
    [HttpGet]
    [EnjoySpace.Web.Services.BackOfficeQuery]
    public async Task<IActionResult> Index(string? tab, int page = 1) => View(await workflow.GetListAsync(tab, page));

    [HttpGet]
    public async Task<IActionResult> Leave(long participantId, bool returnToSession = false)
    {
        ViewData["ReturnToSession"] = returnToSession;
        var model = await workflow.GetLeaveFormAsync(participantId);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Leave(LeaveCreateModel model, bool returnToSession = false)
    {
        ViewData["ReturnToSession"] = returnToSession;
        // Resolve the return destination from the participant, never from a submitted session ID.
        var returnSessionId = returnToSession ? (await workflow.GetLeaveFormAsync(model.ParticipantId))?.SessionId : null;
        if (ModelState.IsValid && await TryAsync(async () => { await workflow.CreateLeaveAsync(model, ActorId); }))
        {
            TempData["Success"] = "請假已登記，已建立一筆原學期日期範圍內的補課資格。";
            if (returnSessionId.HasValue)
                return RedirectToAction("Details", "ClassSessions", new { area = "Admin", id = returnSessionId.Value }, "attendance");
            return RedirectToAction(nameof(Index));
        }
        var fresh = await workflow.GetLeaveFormAsync(model.ParticipantId);
        if (fresh is null) return NotFound();
        model.StudentName = fresh.StudentName; model.CourseName = fresh.CourseName; model.StartsAt = fresh.StartsAt; model.SessionId = fresh.SessionId;
        // Keep the submitted version on validation failure; a stale form requires an explicit reload.
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Apply(long entitlementId)
    {
        try
        {
            var model = await workflow.GetMakeupFormAsync(entitlementId);
            return model is null ? NotFound() : View(model);
        }
        catch (InvalidOperationException e) { TempData["Error"] = e.Message; return RedirectToAction(nameof(Index), new { tab = "entitlements" }); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(MakeupCreateModel model)
    {
        if (ModelState.IsValid && await TryAsync(async () => { await workflow.CreateMakeupAsync(model, ActorId); }))
        {
            TempData["Success"] = "補課申請已建立，資格已保留；核准時才加入課堂名單。";
            return RedirectToAction(nameof(Index), new { tab = "requests" });
        }
        try
        {
            var fresh = await workflow.GetMakeupFormAsync(model.EntitlementId);
            if (fresh is null) return NotFound();
            model.StudentName = fresh.StudentName; model.SourceDescription = fresh.SourceDescription; model.Targets = fresh.Targets;
            model.PeriodStart = fresh.PeriodStart; model.PeriodEnd = fresh.PeriodEnd;
            return View(model);
        }
        catch (InvalidOperationException e) { TempData["Error"] = e.Message; return RedirectToAction(nameof(Index), new { tab = "entitlements" }); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> CancelLeave(LeaveMakeupActionModel model) => ChangeAsync(model, "leaves",
        () => workflow.CancelLeaveAsync(model, ActorId), "請假已取消，原課堂名單已恢復，補課資格已撤銷。");

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Review(LeaveMakeupActionModel model) => ChangeAsync(model, "requests",
        () => workflow.ReviewAsync(model, ActorId), model.Approve ? "補課已核准，學員已加入目標課堂名單。" : "申請已退回，補課資格已釋回。");

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> CancelMakeup(LeaveMakeupActionModel model) => ChangeAsync(model, "requests",
        () => workflow.CancelMakeupAsync(model, ActorId), "補課已取消，資格已釋回；已核准者的目標課堂名額也已釋出。");

    private long ActorId => long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    private async Task<IActionResult> ChangeAsync(LeaveMakeupActionModel model, string tab, Func<Task> action, string success)
    {
        if (!ModelState.IsValid) TempData["Error"] = "表單資料無效，請重新整理；備註最多 500 字。";
        else if (await TryAsync(action)) TempData["Success"] = success;
        return RedirectToAction(nameof(Index), new { tab });
    }
    private async Task<bool> TryAsync(Func<Task> action)
    {
        try { await action(); return true; }
        catch (InvalidOperationException e) { TempData["Error"] = e.Message; }
        catch (DbUpdateConcurrencyException) { TempData["Error"] = "資料已被其他人修改，請重新整理後核對。"; }
        catch (DbUpdateException) { TempData["Error"] = "資格或名單已變更，請重新整理後核對。"; }
        catch (SqlException e) when (e.Number == 1205) { TempData["Error"] = "其他人正在更新名額或資格，請重新整理後再試。"; }
        return false;
    }
}
