using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class LessonPassesController(AppDbContext db, TrialLessonService service) : TrialLessonControllerBase
{
    [HttpGet] public async Task<IActionResult> Index(int page = 1) => View(await TrialLessonService.PageAsync(db.LessonPasses.AsNoTracking()
        .Include(x => x.Payment).Include(x => x.TrialGuest).Include(x => x.Student).ThenInclude(x => x!.User).OrderByDescending(x => x.LessonPassId), page));
    [HttpGet] public async Task<IActionResult> Create(string ownerType = "TrialGuest", long ownerId = 0)
    {
        var model = new LessonPurchaseModel { OwnerType = ownerType, OwnerId = ownerId, ValidFromDate = service.Today };
        await service.PopulatePurchaseAsync(model); return View(model);
    }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Preview(LessonPurchaseModel model)
    {
        LessonQuoteModel? quote = null;
        if (ModelState.IsValid && await Attempt(async () => { quote = await service.PreviewAsync(model, ActorId); })) return View("Confirm", quote);
        await service.PopulatePurchaseAsync(model); return View("Create", model);
    }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Purchase(string token, bool confirmDetails)
    {
        long id = 0;
        if (ModelState.IsValid && await Attempt(async () => { id = await service.PurchaseAsync(token, confirmDetails, ActorId); }))
        { TempData["Success"] = "待付款訂單已建立，確認收款後才會啟用。"; return RedirectToAction(nameof(Details), new { id }); }
        if (!ModelState.IsValid && TempData["Error"] == null) TempData["Error"] = "確認資料無效，請重新選擇方案。";
        return RedirectToAction(nameof(Create));
    }
    [HttpGet] public async Task<IActionResult> Details(long id)
    {
        var model = await service.PassAsync(id);
        ViewData["PaidAtTaiwan"] = DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-ddTHH:mm");
        ViewData["Today"] = service.Today;
        return model is null ? NotFound() : View(model);
    }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> ConfirmPayment(LessonPaymentModel model)
    {
        if (!ModelState.IsValid) TempData["Error"] = "收款欄位無效，請檢查金額、時間與資料版本。";
        else if (await Attempt(() => service.ConfirmPaymentAsync(model, ActorId))) TempData["Success"] = "收款已確認，堂券已啟用。";
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Cancel(LeaveMakeupActionModel model)
    {
        if (!ModelState.IsValid) TempData["Error"] = "取消資料無效，請重新整理並填寫原因。";
        else if (await Attempt(() => service.CancelAsync(model.Id, model.RowVersion, model.Note, ActorId))) TempData["Success"] = "尚未收款的訂單已取消。";
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }
}
