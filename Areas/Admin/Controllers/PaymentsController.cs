using System.Security.Claims;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace EnjoySpace.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class PaymentsController(AppDbContext db, EnrollmentWorkflowService workflow) : Controller
{
    public async Task<IActionResult> Index() => View(await db.EnrollmentApplications.AsNoTracking()
        .Include(x => x.Payment).Include(x => x.Student).ThenInclude(x => x.User).Include(x => x.Course).ThenInclude(x => x.Term)
        .Where(x => x.PaymentId != null).OrderByDescending(x => x.Payment!.CreatedAtUtc).Take(200).ToListAsync());

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

