using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class TrialGuestsController(AppDbContext db, TrialLessonService service) : TrialLessonControllerBase
{
    [HttpGet] public async Task<IActionResult> Index(int page = 1) => View(await TrialLessonService.PageAsync(db.TrialGuests.AsNoTracking().OrderByDescending(x => x.TrialGuestId), page));
    [HttpGet] public async Task<IActionResult> Edit(long id = 0)
    {
        var model = id == 0 ? new TrialGuestEditModel() : await service.GuestAsync(id);
        if (model is null) return NotFound();
        await service.PopulateGuestAsync(model); return View(model);
    }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Edit(TrialGuestEditModel model)
    {
        if (ModelState.IsValid && await Attempt(async () => { await service.SaveGuestAsync(model, ActorId); }))
        { TempData["Success"] = "體驗生資料已儲存。"; return RedirectToAction(nameof(Index)); }
        await service.PopulateGuestAsync(model); return View(model);
    }
}
