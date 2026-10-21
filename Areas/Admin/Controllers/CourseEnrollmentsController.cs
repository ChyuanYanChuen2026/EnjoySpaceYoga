using EnjoySpace.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class CourseEnrollmentsController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? sort = null) => View(await EnjoySpace.Web.Models.AdminTableSorts.Enrollments.Apply(db.CourseEnrollments.AsNoTracking()
        .Include(x => x.Student).ThenInclude(x => x.User).Include(x => x.Course).ThenInclude(x => x.Term)
        .OrderByDescending(x => x.EnrolledAtUtc).ThenBy(x => x.CourseEnrollmentId).Take(200), sort).ToListAsync());
}

