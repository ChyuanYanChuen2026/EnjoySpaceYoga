using System.Security.Claims;
using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnjoySpace.Web.Controllers;

[Authorize(Roles = "Teacher")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class TeacherDashboardController(TeacherDashboardService service) : Controller
{
    private long ActorId => long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    public async Task<IActionResult> Index(DateOnly? month)
    {
        if (!ModelState.IsValid || month is { Year: < 1900 or > 9998 }) return BadRequest("請選擇有效月份。");
        try { return View(await service.GetAsync(ActorId, month)); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    public async Task<IActionResult> Details(int id)
    {
        try {
            var model = await service.DetailAsync(ActorId, id);
            return model is null ? NotFound() : View(model);
        }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
}
