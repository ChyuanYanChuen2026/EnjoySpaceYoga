using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnjoySpace.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using System.Security.Cryptography;

namespace EnjoySpace.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class TeachersController(AppDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var teachers = await db.Teachers
            .AsNoTracking()
            .OrderBy(t => t.TeacherNo)
            .ThenBy(t => t.TeacherId)
            .Select(t => new TeacherListItemViewModel
            {
                TeacherId = t.TeacherId,
                TeacherNo = t.TeacherNo,
                DisplayName = t.User.DisplayName,
                Phone = t.User.Phone,
                Email = t.User.Email,
                IsActive = t.IsActive,
                IsUserActive = t.User.IsActive
            })
            .ToListAsync();

        return View(teachers);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new TeacherCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TeacherCreateViewModel model)
    {
        if (!ModelState.IsValid)  return View(model);

        model.DisplayName = model.DisplayName.Trim();
        model.TeacherNo = Clean(model.TeacherNo);
        model.Phone = Clean(model.Phone);
        model.Email = Clean(model.Email);
        model.Note = Clean(model.Note);

        if (string.IsNullOrWhiteSpace(model.DisplayName))
        {
            ModelState.AddModelError(nameof(model.DisplayName), "請輸入姓名");
        }

        if (model.TeacherNo is not null && await db.Teachers.AnyAsync(t => t.TeacherNo == model.TeacherNo))
        {
            ModelState.AddModelError(
                nameof(model.TeacherNo),
                "此老師編號已使用，請改用其他編號");
        }

        var teacherRole = await db.Roles.SingleOrDefaultAsync(r => r.RoleCode == "Teacher" && r.IsActive);

        if (teacherRole is null)
        {
            ModelState.AddModelError("", "找不到有效的 Teacher 角色，請先確認角色設定");
        }

        if (!ModelState.IsValid)  return View(model);

        if (!long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminUserId))
        {
            return Challenge();
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            Account = $"teacher-{Guid.NewGuid():N}",
            DisplayName = model.DisplayName,
            Phone = model.Phone,
            Email = model.Email,
            IsActive = false,
            CreatedAtUtc = now
        };

        // 只保存隨機密碼的雜湊，帳號暫不開放登入。
        var randomPassword = Convert.ToBase64String( RandomNumberGenerator.GetBytes(32));

        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, randomPassword);

        var teacher = new Teacher
        {
            User = user,
            TeacherNo = model.TeacherNo,
            IsActive = model.IsActive,
            Note = model.Note,
            CreatedAtUtc = now
        };

        db.Teachers.Add(teacher);
        db.UserRoles.Add(new UserRole
        {
            User = user,
            RoleId = teacherRole!.RoleId,
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
            ModelState.AddModelError( "", "老師編號或帳號資料重複，請確認後重新送出");
            return View(model);
        }
        return RedirectToAction(nameof(Index));
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null  : value.Trim();
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        if (id <= 0)
            return NotFound();

        var model = await db.Teachers
            .AsNoTracking()
            .Where(t => t.TeacherId == id)
            .Select(t => new TeacherEditViewModel
            {
                TeacherId = t.TeacherId,
                TeacherNo = t.TeacherNo,
                DisplayName = t.User.DisplayName,
                Phone = t.User.Phone,
                Email = t.User.Email,
                IsActive = t.IsActive,
                Note = t.Note,
                RowVersion = t.User.RowVersion
            })
            .SingleOrDefaultAsync();

        if (model is null) return NotFound();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, TeacherEditViewModel model)
    {
        if (id != model.TeacherId)  return BadRequest();

        if (!ModelState.IsValid)   return View(model);

        model.DisplayName = model.DisplayName.Trim();
        model.TeacherNo = Clean(model.TeacherNo);
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

        if (model.TeacherNo is not null && await db.Teachers.AnyAsync(t =>  t.TeacherNo == model.TeacherNo &&  t.TeacherId != model.TeacherId))
        {
            ModelState.AddModelError(nameof(model.TeacherNo), "此老師編號已使用，請改用其他編號");
        }

        if (!ModelState.IsValid) return View(model);

        var teacher = await db.Teachers.Include(t => t.User).SingleOrDefaultAsync(t => t.TeacherId == id);

        if (teacher is null) return NotFound();

        var user = teacher.User;

        // 使用開啟表單時的版本檢查修改衝突。
        db.Entry(user).Property(u => u.RowVersion).OriginalValue = model.RowVersion;

        teacher.TeacherNo = model.TeacherNo;
        teacher.IsActive = model.IsActive;
        teacher.Note = model.Note;

        user.DisplayName = model.DisplayName;
        user.Phone = model.Phone;
        user.Email = model.Email;
        user.UpdatedAtUtc = DateTime.UtcNow;

        // 即使只修改 Teacher，也更新 User，觸發版本檢查。
        db.Entry(user).Property(u => u.UpdatedAtUtc).IsModified = true;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            ModelState.AddModelError("", "資料已被其他操作修改或刪除，本次修改未儲存。" + "請返回列表，重新開啟編輯後再修改。");
            return View(model);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627))
        {
            ModelState.AddModelError( "", "資料重複，請確認老師編號是否已被使用");
            return View(model);
        }
        return RedirectToAction(nameof(Index));
    }

}