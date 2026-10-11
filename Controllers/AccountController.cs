using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using EnjoySpace.Web.Services;

namespace EnjoySpace.Web.Controllers;

public class AccountController(AppDbContext db) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl) => View(new AdminLoginModel { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("login")]
    public async Task<IActionResult> Login(AdminLoginModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await db.Users.SingleOrDefaultAsync(x => x.Account == model.Account && x.IsActive);
        var hasher = new PasswordHasher<User>();
        var result = user is null ? PasswordVerificationResult.Failed :
            hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        var roles = user is null ? new List<string>() : await StaffAccess.RolesAsync(db, user.UserId);
        if (result == PasswordVerificationResult.Failed || roles.Count == 0)
        {
            ModelState.AddModelError("", "帳號或密碼錯誤，或沒有有效的管理員／老師權限。");
            return View(model);
        }
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user!.PasswordHash = hasher.HashPassword(user, model.Password);
            await db.SaveChangesAsync();
        }
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user!.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.DisplayName)
        }.Concat(roles.Select(role => new Claim(ClaimTypes.Role, role))), CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        return LocalRedirect(Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl! : roles.Contains("Admin") ? "/Admin/EnrollmentApplications" : "/TeacherRequests");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }
}

public class AdminLoginModel
{
    [Required, StringLength(100)] public string Account { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
    public string? ReturnUrl { get; set; }
}
