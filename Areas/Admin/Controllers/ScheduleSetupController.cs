using System.Security.Claims;
using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models;
using EnjoySpace.Web.Models.Entities;
using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class ScheduleSetupController(AppDbContext db, CourseService courses,
    ClassSessionService sessions, TermScheduleService schedule) : Controller
{
    private long Actor => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private static DateTime? Taiwan(DateTime? utc) => utc?.AddHours(8);
    private static DateTime? Utc(DateTime? local) => local.HasValue
        ? new DateTimeOffset(DateTime.SpecifyKind(local.Value, DateTimeKind.Unspecified), TimeSpan.FromHours(8)).UtcDateTime : null;

    private async Task<Term> Draft(long id) => await db.Terms.AsNoTracking().SingleOrDefaultAsync(x => x.TermId == id && x.Status == "Draft")
        ?? throw new InvalidOperationException("找不到草稿學期，或學期已離開草稿。請返回學期列表確認。");

    private async Task Version(long termId, byte[]? version)
    {
        var term = await Draft(termId);
        if (version is null || version.Length != 8 || !term.RowVersion.SequenceEqual(version))
            throw new InvalidOperationException("課表已被其他操作更新，請重新載入此步驟後再試。");
    }

    private async Task<List<string>> Issues(long termId)
    {
        var items = await schedule.GetSummaryAsync(termId);
        var issues = new List<string>();
        if (!items.Any(x => x.IsActive)) issues.Add("至少需要一門啟用中的課程。");
        issues.AddRange(items.Where(x => x.IsActive && x.SessionCount is < 1 or > short.MaxValue)
            .Select(x => $"「{x.Name}」尚未完成有效場次設定。"));
        var term = await Draft(termId);
        var start = new DateTimeOffset(term.StartDate.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8));
        var end = new DateTimeOffset(term.EndDate.ToDateTime(TimeOnly.MaxValue), TimeSpan.FromHours(8));
        if (await db.ClassSessions.AnyAsync(x => x.Course.TermId == termId && (x.StartsAt < start || x.EndsAt > end)))
            issues.Add("有場次超出學期期間，請先修正。");
        if (await db.Courses.AnyAsync(x => x.TermId == termId && x.IsActive &&
            (!x.DefaultTeacher.IsActive || !x.DefaultLocation.IsActive || !x.ClassCategory.IsActive)) ||
            await db.ClassSessions.AnyAsync(x => x.Course.TermId == termId && x.Course.IsActive && (!x.Teacher.IsActive || !x.StudioLocation.IsActive)))
            issues.Add("有課程或場次使用已停用的分類、老師或教室，請先修正。");
        if (await db.ClassSessions.AnyAsync(x => x.Course.TermId == termId && x.Course.IsActive &&
            db.ClassSessions.Any(y => y.Id != x.Id && y.Course.IsActive && y.StartsAt < x.EndsAt && y.EndsAt > x.StartsAt &&
                (y.CourseId == x.CourseId || y.TeacherId == x.TeacherId || y.StudioLocationId == x.StudioLocationId))))
            issues.Add("有場次的課程、老師或教室時段重疊，請從場次管理核對並修正。");
        return issues;
    }

    private async Task<ScheduleSetupViewModel> Build(long termId, long courseId, int step)
    {
        var model = new ScheduleSetupViewModel { TermId = termId, CourseId = courseId, Step = step };
        if (termId == 0)
        {
            model.Step = 1;
            model.DraftTerms = await courses.GetDraftTermOptionsAsync();
            return model;
        }
        var term = await Draft(termId);
        model.RowVersion = term.RowVersion;
        model.Term = new TermCreateViewModel { Name = term.Name, StartDate = term.StartDate, EndDate = term.EndDate,
            RegistrationStartAtTaiwan = Taiwan(term.RegistrationStartAtUtc), RegistrationEndAtTaiwan = Taiwan(term.RegistrationEndAtUtc) };
        model.Courses = await schedule.GetSummaryAsync(termId);
        if (step == 2)
        {
            model.Course.TermId = termId;
            model.Course.ClassCategoryOptions = (await courses.GetClassCategoryOptionsAsync()).Select(x => new SelectListItem(x.Name, x.ClassCategoryId.ToString())).ToList();
            model.Course.TeacherOptions = (await courses.GetTeacherOptionsAsync()).Select(x => new SelectListItem(x.User.DisplayName, x.TeacherId.ToString())).ToList();
            model.Course.LocationOptions = (await courses.GetLocationOptionsAsync()).Select(x => new SelectListItem(x.Name, x.StudioLocationId.ToString())).ToList();
        }
        if (step == 3)
        {
            var course = await db.Courses.AsNoTracking().SingleOrDefaultAsync(x => x.CourseId == courseId && x.TermId == termId)
                ?? throw new InvalidOperationException("請先選擇此學期已儲存的課程，再設定場次。");
            model.CourseName = course.Name;
            model.Capacity = course.DefaultCapacity;
            model.Session = new ClassSessionCreateViewModel { CourseId = courseId, TeacherId = course.DefaultTeacherId, StudioLocationId = course.DefaultLocationId };
            model.Session.TeacherOptions = (await courses.GetTeacherOptionsAsync()).Select(x => new SelectListItem(x.User.DisplayName, x.TeacherId.ToString())).ToList();
            model.Session.LocationOptions = (await courses.GetLocationOptionsAsync()).Select(x => new SelectListItem(x.Name, x.StudioLocationId.ToString())).ToList();
        }
        if (step is 3 or 4)
            model.Sessions = await db.ClassSessions.AsNoTracking().Where(x => x.Course.TermId == termId && (step == 4 || x.CourseId == courseId))
                .OrderBy(x => x.StartsAt).ThenBy(x => x.Id).Select(x => new ClassSessionListItem { Id = x.Id, CourseName = x.Course.Name,
                    TeacherName = x.Teacher.User.DisplayName, LocationName = x.StudioLocation.Name, StartsAt = x.StartsAt, EndsAt = x.EndsAt,
                    EffectiveCapacity = x.CapacityOverride ?? x.Capacity }).ToListAsync();
        if (step == 4) model.Issues = await Issues(termId);
        return model;
    }

    [HttpGet]
    public async Task<IActionResult> Index(long termId = 0, long courseId = 0, int step = 0)
    {
        if (!ModelState.IsValid || termId < 0 || courseId < 0 || step is < 0 or > 4) return BadRequest();
        if (termId == 0 && (courseId != 0 || step > 1)) return BadRequest("請先設定學期。");
        try
        {
            if (termId != 0)
            {
                await Draft(termId);
                var pending = (await schedule.GetSummaryAsync(termId)).FirstOrDefault(x => x.IsActive && x.SessionCount == 0);
                if (step == 0) step = pending != null ? 3 : await db.Courses.AnyAsync(x => x.TermId == termId) ? 4 : 2;
                if (step == 2 && pending != null)
                {
                    TempData["Error"] = "請先完成上一門課的場次，再新增下一門課。";
                    return RedirectToAction(nameof(Index), new { termId, courseId = pending.CourseId, step = 3 });
                }
                if (step == 3 && courseId == 0) courseId = pending?.CourseId ?? 0;
                if (step == 4 && !await db.Courses.AnyAsync(x => x.TermId == termId))
                    return RedirectToAction(nameof(Index), new { termId, step = 2 });
            }
            var model = await Build(termId, courseId, step == 0 ? 1 : step);
            // Initialize only a new GET form; drafts and failed POST input keep their own values.
            if (termId == 0)
            {
                var taiwanNow = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8)).DateTime;
                var currentMinute = new DateTime(taiwanNow.Year, taiwanNow.Month, taiwanNow.Day, taiwanNow.Hour, taiwanNow.Minute, 0);
                model.Term = new TermCreateViewModel
                {
                    StartDate = DateOnly.FromDateTime(taiwanNow),
                    EndDate = DateOnly.FromDateTime(taiwanNow),
                    RegistrationStartAtTaiwan = currentMinute,
                    RegistrationEndAtTaiwan = currentMinute
                };
            }
            return View(model);
        }
        catch (InvalidOperationException e) { TempData["Error"] = e.Message; return RedirectToAction("Index", "Terms"); }
    }

    private async Task<bool> Attempt(Func<Task> action)
    {
        try { await action(); return true; }
        catch (ArgumentException e) { ModelState.AddModelError("", e.Message); }
        catch (InvalidOperationException e) { ModelState.AddModelError("", e.Message); }
        catch (DbUpdateException) { ModelState.AddModelError("", "資料已變更，本次未儲存。請重新載入此步驟。"); }
        catch (SqlException e) when (e.Number == 1205) { ModelState.AddModelError("", "其他人正在修改課表，本次未儲存。請重新載入後重試。"); }
        db.ChangeTracker.Clear();
        return false;
    }

    private async Task<IActionResult> Redisplay(long termId, long courseId, int step, object input, byte[]? rowVersion)
    {
        try
        {
            var model = await Build(termId, courseId, step);
            // Keep the submitted version so retrying a stale form cannot overwrite newer data.
            model.RowVersion = rowVersion ?? [];
            if (input is TermCreateViewModel term) model.Term = term;
            if (input is CourseCreateViewModel course)
            {
                course.ClassCategoryOptions = model.Course.ClassCategoryOptions; course.TeacherOptions = model.Course.TeacherOptions;
                course.LocationOptions = model.Course.LocationOptions; model.Course = course;
            }
            if (input is ClassSessionCreateViewModel session)
            {
                session.TeacherOptions = model.Session.TeacherOptions; session.LocationOptions = model.Session.LocationOptions; model.Session = session;
            }
            return View("Index", model);
        }
        catch (InvalidOperationException e) { TempData["Error"] = e.Message; return RedirectToAction("Index", "Terms"); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTerm(long termId, byte[]? rowVersion, [Bind(Prefix = "Term")] TermCreateViewModel input)
    {
        // A new term has no database version yet. Empty byte-array binding remains
        // unvalidated in MVC, so exclude this inapplicable field for creation only.
        if (termId == 0) ModelState.Remove(nameof(rowVersion));
        if (termId < 0) return BadRequest();
        if (ModelState.IsValid && await Attempt(async () =>
        {
            if (termId == 0)
            {
                var term = new Term { Name = input.Name.Trim(), StartDate = input.StartDate!.Value, EndDate = input.EndDate!.Value,
                    RegistrationStartAtUtc = Utc(input.RegistrationStartAtTaiwan), RegistrationEndAtUtc = Utc(input.RegistrationEndAtTaiwan),
                    Status = "Draft", CreatedAtUtc = DateTime.UtcNow };
                db.Terms.Add(term); await db.SaveChangesAsync(); termId = term.TermId;
            }
            else
            {
                await Version(termId, rowVersion);
                await schedule.UpdateAsync(new TermEditViewModel { TermId = termId, Name = input.Name,
                    StartDate = input.StartDate, EndDate = input.EndDate, RegistrationStartAtTaiwan = input.RegistrationStartAtTaiwan,
                    RegistrationEndAtTaiwan = input.RegistrationEndAtTaiwan, Status = "Draft", RowVersion = rowVersion! }, Actor);
            }
        }))
        {
            TempData["Success"] = "學期草稿已儲存。";
            return RedirectToAction(nameof(Index), new { termId, step = 2 });
        }
        return await Redisplay(termId, 0, 1, input, rowVersion);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCourse(long termId, byte[]? rowVersion, [Bind(Prefix = "Course")] CourseCreateViewModel input)
    {
        if (termId <= 0 || input.TermId != termId) return BadRequest();
        long courseId = 0;
        if (ModelState.IsValid && await Attempt(async () =>
        {
            await Version(termId, rowVersion);
            if ((await schedule.GetSummaryAsync(termId)).Any(x => x.IsActive && x.SessionCount == 0))
                throw new InvalidOperationException("請先完成上一門課的場次，再新增下一門課。");
            courseId = await courses.CreateAsync(input.Name, termId, input.ClassCategoryId!.Value, input.DefaultTeacherId!.Value,
                input.DefaultLocationId!.Value, input.TermPrice!.Value,
                new CourseSettings(input.DefaultCapacity!.Value, input.IsPubliclyVisible, input.AllowsFlexibleBooking, input.AllowsMakeup, input.IsActive), rowVersion);
        }))
        {
            TempData["Success"] = "課程草稿已儲存，請設定這門課的場次。";
            return RedirectToAction(nameof(Index), new { termId, courseId, step = input.IsActive ? 3 : 4 });
        }
        return await Redisplay(termId, 0, 2, input, rowVersion);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSession(long termId, long courseId, byte[]? rowVersion, [Bind(Prefix = "Session")] ClassSessionCreateViewModel input)
    {
        if (termId <= 0 || courseId <= 0 || courseId != input.CourseId) return BadRequest();
        if (ModelState.IsValid && await Attempt(async () =>
        {
            await Version(termId, rowVersion);
            var error = await sessions.CreateAsync(courseId, input.TeacherId, input.StudioLocationId, input.StartsAt!.Value, input.EndsAt!.Value, termId, rowVersion);
            if (error != null) throw new InvalidOperationException(error);
        }))
        {
            TempData["Success"] = "場次已儲存。可繼續新增場次，或確認本課場次已排完。";
            return RedirectToAction(nameof(Index), new { termId, courseId, step = 3 });
        }
        return await Redisplay(termId, courseId, 3, input, rowVersion);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> NextCourse(long termId, long courseId, byte[]? rowVersion, string next)
    {
        if (!ModelState.IsValid || next is not ("course" or "review")) return BadRequest();
        if (await Attempt(async () =>
        {
            await Version(termId, rowVersion);
            if (!await db.Courses.AnyAsync(x => x.CourseId == courseId && x.TermId == termId && x.IsActive &&
                db.ClassSessions.Any(s => s.CourseId == x.CourseId && !s.IsExtraSession)))
                throw new InvalidOperationException("請先為此學期的課程儲存至少一個場次，再進入下一步。");
        })) return RedirectToAction(nameof(Index), new { termId, step = next == "course" ? 2 : 4 });
        return await Redisplay(termId, courseId, 3, new object(), rowVersion);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(long termId, byte[]? rowVersion, bool confirmSchedule)
    {
        if (!ModelState.IsValid) return BadRequest();
        if (await Attempt(async () =>
        {
            await Version(termId, rowVersion);
            if (!confirmSchedule) throw new InvalidOperationException("請先核對完整堂數及課表，並勾選確認。");
            var issues = await Issues(termId);
            if (issues.Count > 0) throw new InvalidOperationException(string.Join(" ", issues));
        }))
        {
            TempData["Success"] = "新增設定檢核通過，學期仍為草稿。請在此頁確認並開放報名。";
            return RedirectToAction("Edit", "Terms", new { id = termId });
        }
        return await Redisplay(termId, 0, 4, new object(), rowVersion);
    }
}
