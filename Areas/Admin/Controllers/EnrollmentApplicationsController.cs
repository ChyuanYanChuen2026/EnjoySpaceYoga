using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnjoySpace.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class EnrollmentApplicationsController : Controller
{
    private readonly EnrollmentApplicationService _applicationService;

    public EnrollmentApplicationsController(
        EnrollmentApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    public async Task<IActionResult> Index()
    {
        var applications = await _applicationService.GetPendingAsync();

        return View(applications);
    }
}