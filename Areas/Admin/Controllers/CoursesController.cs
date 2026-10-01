using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Services;
using EnjoySpace.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnjoySpace.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
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

    private async Task<List<SelectListItem>> GetTeacherOptionsAsync()
    {
        var teachers = await _courseService.GetTeacherOptionsAsync();

        return teachers.Select(teacher => new SelectListItem
        {
            Value = teacher.TeacherId.ToString(),
            Text = teacher.User.DisplayName
        }).ToList();
    }

    private async Task<List<SelectListItem>> GetLocationOptionsAsync()
    {
        var locations = await _courseService.GetLocationOptionsAsync();
        return locations.Select(location => new SelectListItem
        {
            Value = location.StudioLocationId.ToString(),
            Text = location.Name
        }).ToList();
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new CourseCreateViewModel
        {
            TermOptions = await GetTermOptionsAsync(),
            ClassCategoryOptions = await GetClassCategoryOptionsAsync(),
            TeacherOptions = await GetTeacherOptionsAsync(),
            LocationOptions = await GetLocationOptionsAsync(),
        };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var course = await _courseService.GetByIdAsync(id);
        if (course is null)
        {
            return NotFound();
        }
        var model = new CourseEditViewModel
        {
            CourseId = course.CourseId,
            Name = course.Name,
            TermId = course.TermId,
            TermOptions = await GetTermOptionsAsync(),
            ClassCategoryId = course.ClassCategoryId,
            ClassCategoryOptions = await GetClassCategoryOptionsAsync(),
            DefaultTeacherId = course.DefaultTeacherId,
            TeacherOptions = await GetTeacherOptionsAsync(),
            DefaultLocationId = course.DefaultLocationId,
            LocationOptions = await GetLocationOptionsAsync(),
            RowVersion = course.RowVersion,
            DefaultCapacity = course.DefaultCapacity,
            TermPrice = course.TermPrice,
            IsPubliclyVisible = course.IsPubliclyVisible,
            AllowsFlexibleBooking = course.AllowsFlexibleBooking,
            AllowsMakeup = course.AllowsMakeup,
            IsActive = course.IsActive,
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseCreateViewModel model)
    {
        model.TermOptions = await GetTermOptionsAsync();
        model.ClassCategoryOptions = await GetClassCategoryOptionsAsync();
        model.TeacherOptions = await GetTeacherOptionsAsync();
        model.LocationOptions = await GetLocationOptionsAsync();
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
                 model.DefaultTeacherId!.Value,
                 model.DefaultLocationId!.Value,
                 model.TermPrice!.Value,
                 new CourseSettings(model.DefaultCapacity!.Value,
                     model.IsPubliclyVisible, model.AllowsFlexibleBooking, model.AllowsMakeup, model.IsActive));
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
    public async Task<IActionResult> Edit([FromRoute] long id, CourseEditViewModel model)
    {
        if (id != model.CourseId)
        {
            return BadRequest();
        }
        model.TermOptions = await GetTermOptionsAsync();
        model.ClassCategoryOptions = await GetClassCategoryOptionsAsync();
        model.TeacherOptions = await GetTeacherOptionsAsync();
        model.LocationOptions = await GetLocationOptionsAsync();
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
                 model.DefaultTeacherId!.Value,
                 model.DefaultLocationId!.Value,
                 model.TermPrice!.Value,
                 model.RowVersion,
                 new CourseSettings(model.DefaultCapacity!.Value,
                     model.IsPubliclyVisible, model.AllowsFlexibleBooking, model.AllowsMakeup, model.IsActive));
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
