using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Policy = "BackOffice")]
public class LessonPlansController(AppDbContext db, TrialLessonService service) : TrialLessonControllerBase
{
    [EnjoySpace.Web.Services.BackOfficeQuery]
    [HttpGet] public async Task<IActionResult> Index(int page = 1, string? sort = null) => View(await TrialLessonService.PageAsync(EnjoySpace.Web.Models.AdminTableSorts.Plans.Apply(db.LessonPlans.AsNoTracking(), sort), page));
    [HttpGet] public async Task<IActionResult> Edit(long id = 0)
    {
        var model = id == 0 ? new LessonPlanEditModel() : await service.PlanAsync(id);
        return model is null ? NotFound() : View(model);
    }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Edit(LessonPlanEditModel model)
    {
        if (ModelState.IsValid && await Attempt(async () => { await service.SavePlanAsync(model, ActorId); }))
        { TempData["Success"] = "方案已儲存；既有購買紀錄維持原條件。"; return RedirectToAction(nameof(Index)); }
        return View(model);
    }
}
