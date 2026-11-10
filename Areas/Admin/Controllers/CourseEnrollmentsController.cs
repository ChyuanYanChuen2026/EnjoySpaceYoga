using EnjoySpace.Web.Data;
using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Models;
using EnjoySpace.Web.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class CourseEnrollmentsController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? sort = null, int page = 1)
    {
        // Keep historical enrollments visible even when their student or course is inactive.
        var query = db.CourseEnrollments.AsNoTracking()
            .Include(x => x.Student).ThenInclude(x => x.User)
            .Include(x => x.Course).ThenInclude(x => x.Term);
        var model = new PagedRecords<CourseEnrollment> { TotalCount = await query.CountAsync() };
        model.Page = Math.Clamp(page, 1, model.PageCount);
        var normalizedSort = AdminTableSorts.Enrollments.Normalize(sort);
        ViewData["Sort"] = normalizedSort;
        model.Items = await AdminTableSorts.Enrollments.Apply(query, normalizedSort)
            .Skip((model.Page - 1) * 20).Take(20).ToListAsync();
        return View(model);
    }
}

