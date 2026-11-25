using System.Security.Claims;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using EnjoySpace.Web.Areas.Admin.ViewModels;

namespace EnjoySpace.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Policy = "BackOffice")]
public class PaymentsController(AppDbContext db, EnrollmentWorkflowService workflow) : Controller
{
    [EnjoySpace.Web.Services.BackOfficeQuery]
    public async Task<IActionResult> Index(int page = 1, string? sort = null)
    {
        var result = new PaymentListModel { TotalCount = await db.Payments.CountAsync() };
        result.Page = Math.Clamp(page, 1, result.PageCount);
        var payments = await EnjoySpace.Web.Models.AdminTableSorts.Payments.Apply(db.Payments.AsNoTracking().Include(x => x.Student).ThenInclude(x => x!.User).Include(x => x.TrialGuest)
            , sort).Skip((result.Page - 1) * 20).Take(20).ToListAsync();
        var ids = payments.Select(x => x.PaymentId).ToList();
        var applications = await db.EnrollmentApplications.AsNoTracking().Where(x => x.PaymentId != null && ids.Contains(x.PaymentId.Value))
            .Select(x => new { PaymentId = x.PaymentId!.Value, x.Id, Description = x.Course.Term.Name + " · " + x.Course.Name }).ToDictionaryAsync(x => x.PaymentId);
        var passes = await db.LessonPasses.AsNoTracking().Where(x => ids.Contains(x.PaymentId))
            .Select(x => new { x.PaymentId, x.LessonPassId, x.PlanNameAtPurchase }).ToDictionaryAsync(x => x.PaymentId);
        result.Items = payments.Select(p => new PaymentListRow { Payment = p,
            ApplicationId = applications.GetValueOrDefault(p.PaymentId)?.Id,
            PassId = passes.GetValueOrDefault(p.PaymentId)?.LessonPassId,
            Description = applications.GetValueOrDefault(p.PaymentId)?.Description ?? passes.GetValueOrDefault(p.PaymentId)?.PlanNameAtPurchase ?? p.PaymentPurpose }).ToList();
        return View(result);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Refund(long paymentId, string note)
    {
        try
        {
            await workflow.RecordRefundAsync(paymentId, note, long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));
            TempData["Success"] = "已記錄人工退款完成。";
        }
        catch (InvalidOperationException e) { TempData["Error"] = e.Message; }
        catch (DbUpdateException) { TempData["Error"] = "資料已變更，請重新確認。"; }
        catch (SqlException e) when (e.Number == 1205) { TempData["Error"] = "其他人正在操作，請重試。"; }
        return RedirectToAction(nameof(Index));
    }
}

