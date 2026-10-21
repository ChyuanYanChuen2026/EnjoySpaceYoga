using EnjoySpace.Web.Services;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Models.Enums;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace EnjoySpace.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class EnrollmentApplicationsController(AppDbContext db, EnrollmentWorkflowService workflow) : Controller
{
    public async Task<IActionResult> Index(bool all = false, string? sort = null)
    {
        var query = db.EnrollmentApplications.AsNoTracking().Include(x => x.Student).ThenInclude(x => x.User)
            .Include(x => x.Course).ThenInclude(x => x.Term).Include(x => x.Payment).AsQueryable();
        if (!all) query = query.Where(x => x.Status == EnrollmentApplicationStatus.Pending);
        ViewBag.All = all;
        return View(await EnjoySpace.Web.Models.AdminTableSorts.Applications.Apply(query.OrderByDescending(x => x.SubmittedAt).ThenBy(x => x.Id).Take(200), sort).ToListAsync());
    }
    public async Task<IActionResult> Details(long id)
    {
        var application = await db.EnrollmentApplications.AsNoTracking().Include(x => x.Student).ThenInclude(x => x.User)
            .Include(x => x.Course).ThenInclude(x => x.Term).Include(x => x.Payment).SingleOrDefaultAsync(x => x.Id == id);
        return application is null ? NotFound() : View(application);
    }
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new EnrollmentCreateModel();
        await PopulateAsync(model);
        return View(model);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EnrollmentCreateModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var preview = await workflow.PreviewAsync(model.StudentId!.Value, model.CourseId!.Value, ActorId);
                ModelState.Clear();
                return View("ConfirmCreate", preview);
            }
            catch (InvalidOperationException e) { ModelState.AddModelError("", e.Message); }
            catch (DbUpdateException) { ModelState.AddModelError("", "資料已變更或重複申請，請重新確認。"); }
            catch (SqlException e) when (e.Number == 1205) { ModelState.AddModelError("", "其他人正在處理相同課程，請重試。"); }
        }
        await PopulateAsync(model);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmCreate(EnrollmentConfirmationModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "報名確認資料不完整，請重新選擇學生與課程。";
            return RedirectToAction(nameof(Create));
        }
        try
        {
            var id = await workflow.CreateAsync(model.StudentId, model.CourseId, ActorId,
                model.ConfirmationToken, model.ConfirmDetails, model.ConfirmNoRefund);
            TempData["Success"] = "已完成報名前確認並建立申請，請再次核對收款後再審核。";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (InvalidOperationException e) { TempData["Error"] = e.Message; }
        catch (DbUpdateException) { TempData["Error"] = "資料已變更或已有重複申請，請重新確認。"; }
        catch (SqlException e) when (e.Number == 1205) { TempData["Error"] = "其他人正在處理相同課程，請重試。"; }
        return RedirectToAction(nameof(Create));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPayment(PaymentConfirmModel model) =>
        await ExecuteAsync(model.Id, () => workflow.ConfirmPaymentAsync(model.Id, model.RowVersion,
            model.ReceivedAmount!.Value, model.Method, model.PaidAtTaiwan!.Value, model.Note, ActorId, model.ConfirmStudentAgreement),
            "收款已確認，可以進行報名審核。");

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(EnrollmentReviewModel model) =>
        await ExecuteAsync(model.Id, () => workflow.ReviewAsync(model.Id, model.RowVersion,
            model.Approve, model.Note, ActorId, model.ConfirmFinalReview), model.Approve ? "報名已核准，正式名單已建立。" : "申請未獲核准；若已收款，請至付款管理處理退回款項。");

    private async Task<IActionResult> ExecuteAsync(long id, Func<Task> action, string success)
    {
        if (!ModelState.IsValid) TempData["Error"] = "資料不完整，請填寫必要欄位並重新操作。";
        else try { await action(); TempData["Success"] = success; }
            catch (InvalidOperationException e) { TempData["Error"] = e.Message; }
            catch (DbUpdateException) { TempData["Error"] = "資料已由其他人處理或發生衝突，請重新確認。"; }
            catch (SqlException e) when (e.Number == 1205) { TempData["Error"] = "同時有其他人操作，請重新確認後重試。"; }
        return RedirectToAction(nameof(Details), new { id });
    }
    private long ActorId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private async Task PopulateAsync(EnrollmentCreateModel model)
    {
        model.Students = await db.Students.Where(x => x.IsActive).OrderBy(x => x.User.DisplayName)
        .Select(x => new SelectListItem { Value = x.StudentId.ToString(), Text = x.User.DisplayName + " (" + x.User.Account + ")" }).ToListAsync();
        
        model.Courses = await db.Courses.Where(x =>x.IsActive && x.Term.Status == TermStatusOptions.Open && x.Term.ScheduleConfirmedAtUtc != null).OrderByDescending(x => x.Term.StartDate).ThenBy(x => x.Name)
        .Select(x => new SelectListItem { Value = x.CourseId.ToString(), Text = x.Term.Name + "｜" + x.Name + "｜$" + x.TermPrice}).ToListAsync();
    }
}

