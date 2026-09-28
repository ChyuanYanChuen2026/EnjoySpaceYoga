using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnjoySpace.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class DashboardController : Controller
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel
        {
            CourseCount = await _dashboardService.GetCourseCountAsync(),

            TodayClassSessionCount = await _dashboardService.GetTodayClassSessionCountAsync(),

            PendingEnrollmentCount = await _dashboardService.GetPendingEnrollmentCountAsync()
        };
        return View(model);
    }
}