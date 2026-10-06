using EnjoySpace.Web.Services;
using EnjoySpace.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace EnjoySpace.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
public class ClassSessionsController : Controller
{
    private readonly ClassSessionService _classSessionService;

    public ClassSessionsController(ClassSessionService classSessionService)
    {
        _classSessionService = classSessionService;
    }

    public async Task<IActionResult> Index([FromQuery] CatalogFilter filter, [FromServices] AdminCatalogService catalog)
    {
        var sessions = await catalog.GetSessionsAsync(filter);
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
    public async Task<IActionResult> Delete(int id)
    {
        var model = await _classSessionService.GetDetailAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, byte[] rowVersion)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "資料版本無效，請重新開啟刪除確認頁。";
            return RedirectToAction(nameof(Delete), new { id });
        }
        try
        {
            await _classSessionService.DeleteAsync(id, rowVersion);
            TempData["Success"] = "場次已刪除。";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException e) { TempData["Error"] = e.Message; }
        catch (DbUpdateException) { TempData["Error"] = "場次已變更或已有關聯紀錄，未刪除資料。"; }
        catch (SqlException e) when (e.Number == 1205) { TempData["Error"] = "其他人正在操作課表，請重新確認。"; }
        return RedirectToAction(nameof(Delete), new { id });
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

    private async Task PopulateOptionsAsync( ClassSessionCreateViewModel model)
    {
        var courses = await _classSessionService.GetCourseOptionsAsync();
        var teachers = await _classSessionService.GetTeacherOptionsAsync();
        var locations = await _classSessionService.GetLocationOptionsAsync();

        model.CourseOptions = courses.Select(course => new SelectListItem
        {
            Value = course.CourseId.ToString(),
            Text = $"{course.Name}（{course.Term.StartDate:yyyy/MM/dd}～{course.Term.EndDate:yyyy/MM/dd}）"
        }).ToList();

        model.TeacherOptions = teachers.Select(teacher => new SelectListItem
        {
            Value = teacher.TeacherId.ToString(),
            Text = teacher.User!.DisplayName
        }).ToList();

        model.LocationOptions = locations.Select(location => new SelectListItem
        {
            Value = location.StudioLocationId.ToString(),
            Text = location.Name
        }).ToList();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClassSessionCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model);

            return View(model);
        }

        string? errorMessage;
        try
        {
            errorMessage = await _classSessionService.CreateAsync(
            model.CourseId,
            model.TeacherId,
            model.StudioLocationId,
            model.StartsAt!.Value,
            model.EndsAt!.Value);
        }
        catch (InvalidOperationException e) { errorMessage = e.Message; }
        catch (DbUpdateException) { errorMessage = "課程或場次資料已變更，請重新確認。"; }
        catch (SqlException e) when (e.Number == 1205) { errorMessage = "其他人正在操作課表，請重試。"; }

        if (errorMessage is not null)
        {
            ModelState.AddModelError(string.Empty, errorMessage);

            await PopulateOptionsAsync(model);

            return View(model);
        }

        TempData["SuccessMessage"] = "場次新增成功。";

        return RedirectToAction(nameof(Index));
    }


}
