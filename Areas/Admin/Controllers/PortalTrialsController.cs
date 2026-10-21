using EnjoySpace.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class PortalTrialsController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string? sort = null)
    {
        page = Math.Max(1, page);
        ViewBag.Page = page;
        ViewBag.Total = await db.PortalTrialRequests.CountAsync();
        return View(await EnjoySpace.Web.Models.AdminTableSorts.PortalTrials.Apply(db.PortalTrialRequests.AsNoTracking().Include(x => x.TrialGuest).Include(x => x.ClassSession).ThenInclude(x => x.Course)
            , sort).Skip((page - 1) * 30).Take(30).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long id, string status, string expectedStatus)
    {
        if (status is not ("Contacted" or "Cancelled")) return BadRequest();
        var changed = await db.PortalTrialRequests.Where(x => x.Id == id && x.Status == expectedStatus && x.Status != "Cancelled")
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, status));
        TempData["Success"] = changed == 1 ? "申請狀態已更新。" : "狀態已由其他人變更，請重新核對。";
        return RedirectToAction(nameof(Index));
    }
}
