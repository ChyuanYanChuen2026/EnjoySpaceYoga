using EnjoySpace.Web.Areas.Admin.Controllers;
using EnjoySpace.Web.Models;
using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnjoySpace.Web.Controllers;

[Authorize(Roles = "Admin,Teacher")]
public class TeacherRequestsController(TeacherRequestService service) : TrialLessonControllerBase
{
    public async Task<IActionResult> Index(int page = 1, string? sort = null) => View(await service.ListAsync(ActorId, page, sort));
    [HttpGet] public async Task<IActionResult> Create(string kind = "ExtraSession", long teacherId = 0)
    {
        var model = new TeacherRequestInput { Kind = kind == "Capacity" ? "Capacity" : "ExtraSession", TeacherId = teacherId };
        await service.PopulateAsync(model, ActorId); return View(model);
    }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Create(TeacherRequestInput model)
    {
        long id = 0;
        if (ModelState.IsValid && await Attempt(async () => id = await service.SubmitAsync(model, ActorId)))
        { TempData["Success"] = "申請已送出，管理員核准後才生效。"; return RedirectToAction(nameof(Details), new { id }); }
        await service.PopulateAsync(model, ActorId); return View(model);
    }
    public async Task<IActionResult> Details(long id)
    {
        var request = await service.DetailAsync(id, ActorId);
        return request is null ? NotFound() : View(request);
    }
    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Review(TeacherRequestReview model)
    {
        if (!ModelState.IsValid) TempData["Error"] = "審核欄位無效，請重新載入並核對。";
        else if (await Attempt(() => service.ReviewAsync(model, ActorId))) TempData["Success"] = "審核完成。";
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }
}
