using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Models.Entities;
using EnjoySpace.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class TermsController(AppDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var terms = await db.Terms
            .AsNoTracking()
            .OrderByDescending(t => t.StartDate)
            .ThenByDescending(t => t.TermId)
            .Select(t => new TermListItemViewModel
            {
                TermId = t.TermId,
                Name = t.Name,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                RegistrationStartAtUtc = t.RegistrationStartAtUtc,
                RegistrationEndAtUtc = t.RegistrationEndAtUtc,
                Status = t.Status
            })
            .ToListAsync();

        return View(terms);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new TermCreateViewModel());
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
            RowVersion = t.RowVersion
        }).SingleOrDefaultAsync();

        if (model is null) return NotFound();

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

        var term = await db.Terms.SingleOrDefaultAsync(t => t.TermId == id);

        if (term is null) return NotFound();

        if (!TermStatusOptions.CanChangeTo(term.Status, model.Status))
        {
            ModelState.AddModelError(nameof(model.Status), $"不允許從「{TermStatusOptions.GetText(term.Status)}」" + $"直接變更為「{TermStatusOptions.GetText(model.Status)}」。");
            return View(model);
        }

        // 使用開啟表單時的版本，避免覆蓋他人已儲存的修改。
        db.Entry(term).Property(t => t.RowVersion).OriginalValue = model.RowVersion;

        term.Name = model.Name;
        term.StartDate = model.StartDate!.Value;
        term.EndDate = model.EndDate!.Value;
        term.RegistrationStartAtUtc = ToUtc(model.RegistrationStartAtTaiwan);
        term.RegistrationEndAtUtc = ToUtc(model.RegistrationEndAtTaiwan);
        term.Status = model.Status;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            ModelState.AddModelError("", "資料已被其他操作修改或刪除，本次修改未儲存。" + "請返回列表，重新開啟編輯後再修改。");

            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }
}