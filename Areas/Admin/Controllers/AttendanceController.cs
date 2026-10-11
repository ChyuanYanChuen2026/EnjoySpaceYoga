using System.Security.Claims;
using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class AttendanceController(AttendanceService attendance) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(int sessionId)
    {
        var roster = await attendance.GetRosterAsync(sessionId);
        return roster is null ? NotFound() : View(roster);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Record(AttendanceEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "點名資料無效，請重新選擇狀態、確認備註不超過 500 字，再送出。";
            return RedirectToAction(nameof(Index), new { sessionId = model.SessionId });
        }
        if (!long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var actorId)) return Forbid();
        try
        {
            await attendance.RecordAsync(model, actorId);
            TempData["Success"] = "出缺席紀錄已儲存。";
        }
        catch (InvalidOperationException e) { TempData["Error"] = e.Message; }
        catch (DbUpdateConcurrencyException) { TempData["Error"] = "點名紀錄已被其他人修改，請重新確認。"; }
        catch (DbUpdateException) { TempData["Error"] = "名單資料已變更，請重新確認後再操作。"; }
        catch (SqlException e) when (e.Number == 1205) { TempData["Error"] = "其他人正在更新名單，請重新確認後再操作。"; }
        return RedirectToAction(nameof(Index), new { sessionId = model.SessionId });
    }
}
