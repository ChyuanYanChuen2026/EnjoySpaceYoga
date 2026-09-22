using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnjoySpace.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class ClassSessionsController : Controller
{
    private readonly ClassSessionService _classSessionService;

    public ClassSessionsController(ClassSessionService classSessionService)
    {
        _classSessionService = classSessionService;
    }

    public async Task<IActionResult> Index()
    {
        var sessions = await _classSessionService.GetAllAsync();
        return View(sessions);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var model = await _classSessionService.GetDetailAsync(id);

        if (model is null)
        {
            return NotFound();
        }
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Events( [FromQuery] DateTimeOffset? start,  [FromQuery] DateTimeOffset? end)
    {
        if (!ModelState.IsValid ||  !start.HasValue ||  !end.HasValue || end.Value <= start.Value)
        {
            return BadRequest("請提供有效的 start、end 日期範圍。");
        }

        var sessions = await _classSessionService .GetCalendarSessionsAsync(start.Value, end.Value);
        var events = sessions.Select(session => new
        {
            id = session.Id.ToString(),
            title = $"{session.CourseName}｜{session.TeacherName}",
            start = session.StartsAt.ToString("O"),
            end = session.EndsAt.ToString("O"),
            allDay = false,
            url = Url.Action( nameof(Details), "ClassSessions", new { area = "Admin", id = session.Id })
        });
        return Json(events);
    }
}