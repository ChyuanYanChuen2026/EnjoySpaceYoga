using EnjoySpace.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using System.Security.Cryptography;
using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class StudentsController(AppDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? sort = null)
    {
        var students = await EnjoySpace.Web.Models.AdminTableSorts.Students.Apply(db.Students.AsNoTracking(), sort)
            .Select(s => new StudentListItemViewModel
            {
                StudentId = s.StudentId,
                StudentNo = s.StudentNo,
                DisplayName = s.User.DisplayName,
                Phone = s.User.Phone,
                Email = s.User.Email,
                JoinedDate = s.JoinedDate,
                IsActive = s.IsActive,
                IsUserActive = s.User.IsActive
            })
            .ToListAsync();

        return View(students);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new StudentCreateViewModel());
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        model.DisplayName = model.DisplayName.Trim();
        model.StudentNo = Clean(model.StudentNo);
        model.Phone = Clean(model.Phone);
        model.Email = Clean(model.Email);
        model.Note = Clean(model.Note);

        if (string.IsNullOrWhiteSpace(model.DisplayName))
        {
            ModelState.AddModelError(nameof(model.DisplayName), "請輸入姓名");
        }

        if (model.StudentNo is not null && await db.Students.AnyAsync(s => s.StudentNo == model.StudentNo))
        {
            ModelState.AddModelError(nameof(model.StudentNo), "此學號已使用，請改用其他學號");
        }

        var studentRole = await db.Roles.SingleOrDefaultAsync(r => r.RoleCode == "Student" && r.IsActive);

        if (studentRole is null)
        {
            ModelState.AddModelError("", "找不到有效的 Student 角色，請先確認角色設定");
        }

        if (!ModelState.IsValid) return View(model);

        if (!long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminUserId))
        {
            return Challenge();
        }

        var now = DateTime.UtcNow;

        var user = new User
        {
            Account = $"student-{Guid.NewGuid():N}",
            DisplayName = model.DisplayName,
            Phone = model.Phone,
            Email = model.Email,
            IsActive = false,
            CreatedAtUtc = now
        };

        // 產生隨機密碼並只保存雜湊，帳號暫不開放登入。
        var randomPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, randomPassword);

        var student = new Student
        {
            User = user,
            StudentNo = model.StudentNo,
            JoinedDate = model.JoinedDate,
            IsActive = model.IsActive,
            Note = model.Note,
            CreatedAtUtc = now
        };

        db.Students.Add(student);

        db.UserRoles.Add(new UserRole
        {
            User = user,
            RoleId = studentRole!.RoleId,
            AssignedByUserId = adminUserId,
            AssignedAtUtc = now
        });

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627))
        {
            // 資料庫唯一限制也會攔住同時送出的重複資料。
            ModelState.AddModelError("", "學號或帳號資料重複，請確認後重新送出");
            return View(model);
        }
        return RedirectToAction(nameof(Index));
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        if (id <= 0) return NotFound();

        var model = await db.Students
            .AsNoTracking()
            .Where(s => s.StudentId == id)
            .Select(s => new StudentEditViewModel
            {
                StudentId = s.StudentId,
                StudentNo = s.StudentNo,
                DisplayName = s.User.DisplayName,
                Phone = s.User.Phone,
                Email = s.User.Email,
                JoinedDate = s.JoinedDate,
                IsActive = s.IsActive,
                Note = s.Note,
                RowVersion = s.User.RowVersion
            })
            .SingleOrDefaultAsync();

        if (model is null) return NotFound();

        return View(model);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, StudentEditViewModel model)
    {
        if (id != model.StudentId)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        model.DisplayName = model.DisplayName.Trim();
        model.StudentNo = Clean(model.StudentNo);
        model.Phone = Clean(model.Phone);
        model.Email = Clean(model.Email);
        model.Note = Clean(model.Note);

        if (string.IsNullOrWhiteSpace(model.DisplayName))
        {
            ModelState.AddModelError(nameof(model.DisplayName), "請輸入姓名");
        }

        if (model.RowVersion.Length != 8)
        {
            ModelState.AddModelError("", "資料版本不正確，請返回列表後重新開啟編輯");
        }

        if (model.StudentNo is not null &&
            await db.Students.AnyAsync(s =>
                s.StudentNo == model.StudentNo &&
                s.StudentId != model.StudentId))
        {
            ModelState.AddModelError(nameof(model.StudentNo), "此學號已使用，請改用其他學號");
        }

        if (!ModelState.IsValid) return View(model);
        var student = await db.Students
            .Include(s => s.User)
            .SingleOrDefaultAsync(s => s.StudentId == id);

        if (student is null) return NotFound();
        var user = student.User;

        // 使用開啟表單時的版本，檢查期間是否有人修改資料。
        db.Entry(user).Property(u => u.RowVersion).OriginalValue = model.RowVersion;

        student.StudentNo = model.StudentNo;
        student.JoinedDate = model.JoinedDate;
        student.IsActive = model.IsActive;
        student.Note = model.Note;

        user.DisplayName = model.DisplayName;
        user.Phone = model.Phone;
        user.Email = model.Email;
        user.UpdatedAtUtc = DateTime.UtcNow;

        // 即使只修改 Student，也更新 User，觸發版本檢查。
        db.Entry(user).Property(u => u.UpdatedAtUtc).IsModified = true;
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            ModelState.AddModelError(
                "",
                "資料已被其他操作修改或刪除，本次修改未儲存。" +
                "請返回列表，重新開啟編輯後再修改。");
            return View(model);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627))
        {
            ModelState.AddModelError("", "資料重複，請確認學號是否已被使用");
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }
}