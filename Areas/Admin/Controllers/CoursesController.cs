using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Services;
using EnjoySpace.Web.Serviece;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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

    private async Task<List<SelectListItem>> GetTermOptionsAsync()
    {
        var terms = await _courseService.GetTermOptionsAsync();
        return terms.Select(term => new SelectListItem
        {
            Value = term.TermId.ToString(),
            Text = $"{term.Name}（{term.StartDate:yyyy/MM/dd}～{term.EndDate:yyyy/MM/dd}）"
        }).ToList();
    }

    private async Task<List<SelectListItem>> GetClassCategoryOptionsAsync()
    {
        var categories = await _courseService.GetClassCategoryOptionsAsync();
        return categories.Select(category => new SelectListItem
        {
            Value = category.ClassCategoryId.ToString(),
            Text = category.Name
        }).ToList();
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new CourseCreateViewModel
        {
            TermOptions = await GetTermOptionsAsync(),
            ClassCategoryOptions = await GetClassCategoryOptionsAsync()
        };
        return View(model);
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
            TermId = course.TermId,
            TermOptions = await GetTermOptionsAsync(),
            ClassCategoryId = course.ClassCategoryId,
            ClassCategoryOptions = await GetClassCategoryOptionsAsync(),
            StartDate = course.StartDate,
            EndDate = course.EndDate,
            RowVersion = course.RowVersion
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseCreateViewModel model)
    {
        model.TermOptions = await GetTermOptionsAsync();
        model.ClassCategoryOptions = await GetClassCategoryOptionsAsync();
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        try
        {
            await _courseService.CreateAsync(
                model.Name,
                model.TermId!.Value,
                model.ClassCategoryId!.Value,
                model.StartDate!.Value,
                model.EndDate!.Value);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        TempData["SuccessMessage"] = "課程新增成功。";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromRoute] int id, CourseEditViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }
        model.TermOptions = await GetTermOptionsAsync();
        model.ClassCategoryOptions = await GetClassCategoryOptionsAsync();
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        CourseUpdateResult result;
        try
        {
            result = await _courseService.UpdateAsync(
                 id,
                 model.Name,
                 model.TermId!.Value,
                 model.ClassCategoryId!.Value,
                 model.StartDate!.Value,
                 model.EndDate!.Value,
                 model.RowVersion);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
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