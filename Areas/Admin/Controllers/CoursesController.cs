using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Services;
using EnjoySpace.Web.Serviece;
using Microsoft.AspNetCore.Mvc;

namespace EnjoySpace.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class CoursesController : Controller
{
    private readonly CourseService _courseService;

    public CoursesController(CourseService courseService)
    {
        _courseService = courseService;
    }

    public async Task<IActionResult> Index()
    {
        var courses = await _courseService.GetAllAsync();
        return View(courses);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CourseCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        await _courseService.CreateAsync(model.Name, model.StartDate!.Value, model.EndDate!.Value);
        TempData["SuccessMessage"] = "課程新增成功。";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var course = await _courseService.GetByIdAsync(id);
        if (course is null)
        {
            return NotFound();
        }
        var model = new CourseEditViewModel
        {
            Id = course.Id,
            Name = course.Name,
            StartDate = course.StartDate,
            EndDate = course.EndDate,
            RowVersion = course.RowVersion
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromRoute] int id, CourseEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _courseService.UpdateAsync(id, model.Name,  model.StartDate!.Value, model.EndDate!.Value, model.RowVersion);

        if (result == CourseUpdateResult.NotFound)
        {
            return NotFound();
        }

        if (result == CourseUpdateResult.Conflict)
        {
            ModelState.AddModelError(string.Empty, "這筆課程已被其他操作修改或刪除，本次修改未儲存。" + "請先保留需要的輸入，再重新載入最新資料。");
            return View(model);
        }

        TempData["SuccessMessage"] = "課程修改成功。";
        return RedirectToAction(nameof(Index));
    }

}