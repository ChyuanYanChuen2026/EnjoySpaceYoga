using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnjoySpace.Web.ViewComponents;

public class TrialReviewNoticeViewComponent(TrialReviewSummaryService reviews) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(bool compact = false)
    {
        if (!UserClaimsPrincipal.IsInRole("Admin")) return Content("");
        return View(compact ? "Compact" : "Default", await reviews.GetAsync());
    }
}
