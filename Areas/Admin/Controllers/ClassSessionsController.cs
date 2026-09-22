using EnjoySpace.Web.Services;
using EnjoySpace.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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


    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new ClassSessionCreateViewModel();

        await PopulateOptionsAsync(model);

        return View(model);
    }

    private async Task PopulateOptionsAsync(
    ClassSessionCreateViewModel model)
    {
        var courses = await _classSessionService.GetCourseOptionsAsync();
        var teachers = await _classSessionService.GetTeacherOptionsAsync();
        var locations = await _classSessionService.GetLocationOptionsAsync();

        model.CourseOptions = courses.Select(course => new SelectListItem
        {
            Value = course.Id.ToString(),
            Text = $"{course.Name}（{course.StartDate:yyyy/MM/dd}～{course.EndDate:yyyy/MM/dd}）"
        }).ToList();

        model.TeacherOptions = teachers.Select(teacher => new SelectListItem
        {
            Value = teacher.Id.ToString(),
            Text = teacher.Name
        }).ToList();

        model.LocationOptions = locations.Select(location => new SelectListItem
        {
            Value = location.Id.ToString(),
            Text = location.Name
        }).ToList();
    }


}