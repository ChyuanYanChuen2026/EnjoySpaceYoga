using System.Security.Claims;
using Microsoft.Data.SqlClient;
using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Models.Entities;
using EnjoySpace.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnjoySpace.Web.Services;

namespace EnjoySpace.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class TermsController(AppDbContext db, TermScheduleService schedule) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] CatalogFilter filter, [FromServices] AdminCatalogService catalog)
    {
        var terms = await catalog.GetTermsAsync(filter);

        return View(terms);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return RedirectToAction("Index", "ScheduleSetup");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TermCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        model.Name = model.Name.Trim();

        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), "請輸入學期名稱");
            return View(model);
        }

        var term = new Term
        {
            Name = model.Name,
            StartDate = model.StartDate!.Value,
            EndDate = model.EndDate!.Value,
            RegistrationStartAtUtc = ToUtc(model.RegistrationStartAtTaiwan),
            RegistrationEndAtUtc = ToUtc(model.RegistrationEndAtTaiwan),
            Status = "Draft",
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Terms.Add(term);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private static DateTime? ToUtc(DateTime? taiwanTime)
    {
        if (!taiwanTime.HasValue) return null;
        var localTime = DateTime.SpecifyKind(taiwanTime.Value, DateTimeKind.Unspecified);
        return new DateTimeOffset(localTime, TimeSpan.FromHours(8)).UtcDateTime;
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        if (id <= 0) return NotFound();

        var model = await db.Terms.AsNoTracking().Where(t => t.TermId == id).Select(t => new TermEditViewModel
        {
            TermId = t.TermId,
            Name = t.Name,
            StartDate = t.StartDate,
            EndDate = t.EndDate,
            RegistrationStartAtTaiwan = ToTaiwanTime(t.RegistrationStartAtUtc),
            RegistrationEndAtTaiwan = ToTaiwanTime(t.RegistrationEndAtUtc),
            Status = t.Status,
            ScheduleConfirmedAtUtc = t.ScheduleConfirmedAtUtc,
            RowVersion = t.RowVersion
        }).SingleOrDefaultAsync();

        if (model is null) return NotFound();

        model.Schedule = await schedule.GetSummaryAsync(id);
        return View(model);
    }

    private static DateTime? ToTaiwanTime(DateTime? utcTime)
    {
        if (!utcTime.HasValue) return null;
        var utc = DateTime.SpecifyKind(utcTime.Value, DateTimeKind.Utc);
        return new DateTimeOffset(utc).ToOffset(TimeSpan.FromHours(8)).DateTime;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, TermEditViewModel model)
    {
        if (id != model.TermId) return BadRequest();
        model.Schedule = await schedule.GetSummaryAsync(id);
        model.ScheduleConfirmedAtUtc = await db.Terms.Where(x => x.TermId == id).Select(x => x.ScheduleConfirmedAtUtc).SingleOrDefaultAsync();

        if (!ModelState.IsValid) return View(model);

        model.Name = model.Name.Trim();

        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), "請輸入學期名稱");
        }

        if (!TermStatusOptions.IsValid(model.Status))
        {
            ModelState.AddModelError(nameof(model.Status), "請選擇有效的學期狀態");
        }

        if (model.RowVersion.Length != 8)
        {
            ModelState.AddModelError("", "資料版本不正確，請返回列表後重新開啟編輯");
        }

        if (!ModelState.IsValid) return View(model);

        try
        {
            await schedule.UpdateAsync(model, long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));
        }
        catch (InvalidOperationException e) { ModelState.AddModelError("", e.Message); }
        catch (DbUpdateException) { ModelState.AddModelError("", "資料已被其他操作修改，請重新開啟編輯頁。"); }
        catch (SqlException e) when (e.Number == 1205) { ModelState.AddModelError("", "其他人正在修改課表，請重新整理後重試。"); }
        if (!ModelState.IsValid) return View(model);
        TempData["Success"] = "學期已更新。";
        return RedirectToAction(nameof(Index));
    }
}
