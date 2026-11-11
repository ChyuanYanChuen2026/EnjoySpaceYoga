using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace EnjoySpace.Web.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class AccountController(AppDbContext db, PasswordRecoveryService recovery, ILogger<AccountController> logger) : Controller
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
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(PasswordSession.ClaimType, PasswordSession.Version(user.PasswordHash))
        }.Concat(roles.Select(role => new Claim(ClaimTypes.Role, role))), CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        if (!roles.Contains("Admin")) return LocalRedirect("/TeacherDashboard");
        return LocalRedirect(Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl! : "/Admin/EnrollmentApplications");
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordModel());

    [HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("password-recovery")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (!recovery.IsConfigured)
        {
            Response.StatusCode = 503;
            ModelState.AddModelError("", "寄信服務尚未設定完成，請聯絡管理員。");
            return View(model);
        }
        try { await recovery.RequestAsync(model.Account, model.Email, HttpContext.RequestAborted); }
        catch (Exception ex) when (ex is System.Net.Mail.SmtpException or OperationCanceledException or InvalidOperationException)
        {
            // Do not log the destination, reset token or SMTP credentials, or reveal account existence.
            logger.LogWarning("Password recovery email could not be sent ({ErrorType}). Check SMTP configuration and availability.", ex.GetType().Name);
        }
        return RedirectToAction(nameof(ForgotPasswordSent));
    }

    [HttpGet]
    public IActionResult ForgotPasswordSent() => View();

    [HttpGet]
    public async Task<IActionResult> ResetPassword(string? token)
    {
        Response.Headers["Referrer-Policy"] = "no-referrer";
        if (!await recovery.IsValidAsync(token)) return View("ResetPasswordInvalid");
        return View(new ResetPasswordModel { Token = token! });
    }

    [HttpPost, ValidateAntiForgeryToken, EnableRateLimiting("password-recovery")]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
    {
        Response.Headers["Referrer-Policy"] = "no-referrer";
        if (!ModelState.IsValid) return View(model);
        if (!await recovery.ResetAsync(model.Token, model.Password)) return View("ResetPasswordInvalid");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(PortalAuthentication.Scheme);
        return RedirectToAction(nameof(ResetPasswordComplete));
    }

    [HttpGet]
    public IActionResult ResetPasswordComplete() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }
}

public class ForgotPasswordModel
{
    [Required(ErrorMessage = "請輸入帳號。"), StringLength(100)] public string Account { get; set; } = "";
    [Required(ErrorMessage = "請輸入登記的 Email。"), EmailAddress(ErrorMessage = "請輸入有效的 Email。"), StringLength(200)]
    public string Email { get; set; } = "";
}

public class ResetPasswordModel
{
    [Required, StringLength(4096)] public string Token { get; set; } = "";
    [Required(ErrorMessage = "請輸入新密碼。"), StringLength(200, MinimumLength = 10, ErrorMessage = "密碼須為 10～200 字。"), DataType(DataType.Password)]
    public string Password { get; set; } = "";
    [Required(ErrorMessage = "請再次輸入新密碼。"), Compare(nameof(Password), ErrorMessage = "兩次輸入的密碼不一致。"), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";
}

public class AdminLoginModel
{
    [Required(ErrorMessage = "請輸入帳號。"), StringLength(100, ErrorMessage = "帳號長度不可超過 100 字。")] public string Account { get; set; } = "";
    [Required(ErrorMessage = "請輸入密碼。"), DataType(DataType.Password)] public string Password { get; set; } = "";
    public string? ReturnUrl { get; set; }
}
