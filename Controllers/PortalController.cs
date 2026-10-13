using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Claims;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Controllers;

[ApiController, Route("api/portal"), AutoValidateAntiforgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class PortalController(AppDbContext db, IAntiforgery antiforgery, TimeProvider clock) : ControllerBase
{
    private long Actor => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private IQueryable<Course> PublicCourses => db.Courses.AsNoTracking().Where(x => x.IsActive && x.IsPubliclyVisible &&
        (x.Term.Status == "Open" || x.Term.Status == "Closed") && x.DefaultTeacher.IsActive && x.DefaultTeacher.User.IsActive && x.DefaultLocation.IsActive);
    private IQueryable<ClassSession> PublicSessions => db.ClassSessions.AsNoTracking().Where(x => PublicCourses.Select(c => c.CourseId).Contains(x.CourseId)
        && x.Teacher.IsActive && x.Teacher.User.IsActive && x.StudioLocation.IsActive);

    [HttpGet("csrf")]
    public IActionResult Csrf() => Ok(new { token = antiforgery.GetAndStoreTokens(HttpContext).RequestToken });

    [HttpPost("login"), EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginInput input)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.Account == input.Account.Trim() && x.IsActive);
        var hasher = new PasswordHasher<User>();
        var result = user is null ? PasswordVerificationResult.Failed : hasher.VerifyHashedPassword(user, user.PasswordHash, input.Password);
        var roles = user is null ? [] : await db.UserRoles.Where(x => x.UserId == user.UserId && x.Role.IsActive).Select(x => x.Role.RoleCode).ToListAsync();
        if (result == PasswordVerificationResult.Failed || roles.Contains("Admin") || !roles.Intersect(PortalAuthentication.Roles).Any())
            return Unauthorized(new { message = "帳號或密碼錯誤，或此帳號無官網使用權限。" });
        if ((roles.Contains("Teacher") && !await db.Teachers.AnyAsync(x => x.UserId == user!.UserId && x.IsActive)) ||
            (roles.Contains("Student") && !await db.Students.AnyAsync(x => x.UserId == user!.UserId && x.IsActive)))
            return Unauthorized(new { message = "此帳號的學生或老師資格已停用，請聯繫工作室。" });
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        { user!.PasswordHash = hasher.HashPassword(user, input.Password); await db.SaveChangesAsync(); }
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, user!.UserId.ToString()), new(ClaimTypes.Name, user.DisplayName) };
        claims.AddRange(roles.Intersect(PortalAuthentication.Roles).Select(x => new Claim(ClaimTypes.Role, x)));
        await HttpContext.SignInAsync(PortalAuthentication.Scheme, new ClaimsPrincipal(new ClaimsIdentity(claims, PortalAuthentication.Scheme)));
        return Ok(new { user.DisplayName, roles = roles.Intersect(PortalAuthentication.Roles) });
    }

    [HttpPost("register"), EnableRateLimiting("login")]
    public async Task<IActionResult> Register(RegisterInput input)
    {
        if (input.Password.Length < 10 || input.Password != input.ConfirmPassword) return BadRequest(new { message = "密碼須至少 10 字，且兩次輸入相同。" });
        var role = await db.Roles.SingleOrDefaultAsync(x => x.RoleCode == "TrialGuest" && x.IsActive);
        if (role is null) return Problem("體驗生註冊尚未啟用，請聯繫工作室。", statusCode: 503);
        var account = input.Account.Trim();
        if (await db.Users.AnyAsync(x => x.Account == account)) return Conflict(new { message = "此帳號已使用，請登入或改用其他帳號。" });
        var user = new User { Account = account, DisplayName = input.Name.Trim(), Email = input.Email.Trim(), Phone = input.Phone.Trim(), CreatedAtUtc = clock.GetUtcNow().UtcDateTime };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, input.Password);
        db.UserRoles.Add(new UserRole { User = user, Role = role, AssignedAtUtc = clock.GetUtcNow().UtcDateTime });
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { return Conflict(new { message = "帳號無法建立，請確認帳號是否已使用。" }); }
        return Ok(new { message = "帳號已建立，請登入。" });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout() { await HttpContext.SignOutAsync(PortalAuthentication.Scheme); return NoContent(); }

    [HttpGet("me"), Authorize(AuthenticationSchemes = PortalAuthentication.Scheme)]
    public async Task<IActionResult> Me() => Ok(await db.Users.Where(x => x.UserId == Actor).Select(x => new
    { x.DisplayName, x.Email, x.Phone, roles = db.UserRoles.Where(r => r.UserId == x.UserId && r.Role.IsActive).Select(r => r.Role.RoleCode).ToArray() }).SingleAsync());

    [HttpGet("catalog")]
    public async Task<IActionResult> Catalog() => Ok(new
    {
        courses = await PublicCourses.OrderBy(x => x.Name).Select(x => new { id = x.CourseId, x.Name, category = x.ClassCategory.Name,
            teacher = x.DefaultTeacher.User.DisplayName, location = x.DefaultLocation.Name, x.TermPrice, capacity = x.DefaultCapacity }).ToListAsync(),
        plans = await db.LessonPlans.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Price)
            .Select(x => new { id = x.LessonPlanId, name = x.PlanName, x.PlanType, x.AudienceType, x.Price, x.SessionQuota, x.ValidityUnit, x.ValidityValue }).ToListAsync(),
        locations = await db.StudioLocations.AsNoTracking().Where(x => x.IsActive).Select(x => new { id = x.StudioLocationId, x.Name, x.Address }).ToListAsync()
    });

    [HttpGet("sessions")]
    public async Task<IActionResult> Sessions(DateTimeOffset? from, DateTimeOffset? until, int? sessionId)
    {
        if (sessionId is <= 0) return BadRequest(new { message = "場次識別碼無效。" });
        var start = from ?? clock.GetUtcNow();
        var end = until ?? start.AddDays(60);
        if (end <= start || end - start > TimeSpan.FromDays(93)) return BadRequest(new { message = "查詢日期範圍須在 93 天內。" });
        var query = sessionId.HasValue ? PublicSessions.Where(x => x.Id == sessionId && x.StartsAt > clock.GetUtcNow())
            : PublicSessions.Where(x => x.StartsAt >= start && x.StartsAt < end);
        return Ok(await query.OrderBy(x => x.StartsAt).Select(x => new
        {
            x.Id, x.CourseId, name = x.Course.Name, category = x.Course.ClassCategory.Name, teacher = x.Teacher.User.DisplayName,
            location = x.StudioLocation.Name, address = x.StudioLocation.Address, x.StartsAt, x.EndsAt,
            capacity = x.CapacityOverride ?? x.Capacity,
            enrolled = db.ClassSessionParticipants.Count(p => p.ClassSessionId == x.Id && p.Status == "Active")
        }).ToListAsync());
    }

    [HttpGet("dashboard"), Authorize(AuthenticationSchemes = PortalAuthentication.Scheme)]
    public async Task<IActionResult> Dashboard()
    {
        var now = clock.GetUtcNow();
        var sessions = db.ClassSessions.AsNoTracking().Where(x => x.StartsAt >= now &&
            ((User.IsInRole("Teacher") && x.Teacher.UserId == Actor && x.Teacher.IsActive) ||
            db.ClassSessionParticipants.Any(p => p.ClassSessionId == x.Id && p.Student.UserId == Actor && p.Student.IsActive && p.Status == "Active")));
        return Ok(new
        {
            sessions = await sessions.OrderBy(x => x.StartsAt).Take(100).Select(x => new { x.Id, name = x.Course.Name, x.StartsAt, x.EndsAt, location = x.StudioLocation.Name }).ToListAsync(),
            requests = await db.PortalTrialRequests.AsNoTracking().Where(x => x.UserId == Actor).OrderByDescending(x => x.CreatedAtUtc).Take(100)
                .Select(x => new { x.Id, name = x.ClassSession.Course.Name, x.ClassSession.StartsAt, x.PlanName, x.QuotedPrice, x.Status, x.CreatedAtUtc }).ToListAsync()
        });
    }

    [HttpPost("trial-requests"), Authorize(AuthenticationSchemes = PortalAuthentication.Scheme, Roles = "Student,TrialGuest"), EnableRateLimiting("portal-trial")]
    public async Task<IActionResult> Apply(TrialInput input)
    {
        if (input.SubmissionKey == Guid.Empty) return BadRequest(new { message = "申請識別碼無效。" });
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var existing = await db.PortalTrialRequests.SingleOrDefaultAsync(x => x.SubmissionKey == input.SubmissionKey);
        if (existing != null) return existing.UserId == Actor ? Ok(new { existing.Id }) : Conflict();
        var session = await PublicSessions.SingleOrDefaultAsync(x => x.Id == input.SessionId && x.StartsAt > clock.GetUtcNow());
        var plan = await db.LessonPlans.SingleOrDefaultAsync(x => x.LessonPlanId == input.PlanId && x.IsActive && x.PlanType == "Trial" && x.AudienceType == "TrialGuest");
        if (session is null || plan is null) return BadRequest(new { message = "課程或方案已變更，請重新選擇。" });
        if (await db.ClassSessionParticipants.CountAsync(x => x.ClassSessionId == session.Id && x.Status == "Active") >= (session.CapacityOverride ?? session.Capacity))
            return Conflict(new { message = "此場次已額滿，請選擇其他場次。" });
        if (await db.PortalTrialRequests.AnyAsync(x => x.UserId == Actor && x.ClassSessionId == session.Id && (x.Status == "Pending" || x.Status == "Contacted")))
            return Conflict(new { message = "您已申請此場次，請至會員專區查看。" });
        var guest = new TrialGuest { Name = input.Name.Trim(), Phone = input.Phone.Trim(), Email = input.Email.Trim(), LineId = input.LineId?.Trim() ?? "",
            SourceType = "Direct", CreatedAtUtc = clock.GetUtcNow().UtcDateTime,
            Note = $"官網體驗申請；場次 #{session.Id}；方案 {plan.PlanName}；待聯繫。" };
        // Reuse only a guest already linked by a previous authenticated request, never by unverified email or phone.
        var guestId = await db.PortalTrialRequests.Where(x => x.UserId == Actor && x.TrialGuest.Status == "Active" && x.TrialGuest.ConvertedStudentId == null)
            .Select(x => (long?)x.TrialGuestId).FirstOrDefaultAsync();
        var request = new PortalTrialRequest { SubmissionKey = input.SubmissionKey, UserId = Actor, ClassSessionId = session.Id,
            LessonPlanId = plan.LessonPlanId, PlanName = plan.PlanName, QuotedPrice = plan.Price, Note = input.Note?.Trim(), CreatedAtUtc = clock.GetUtcNow().UtcDateTime };
        if (guestId.HasValue)
        {
            var ownedGuest = await db.TrialGuests.SingleAsync(x => x.TrialGuestId == guestId.Value);
            ownedGuest.Name = guest.Name; ownedGuest.Phone = guest.Phone; ownedGuest.Email = guest.Email; ownedGuest.LineId = guest.LineId;
            ownedGuest.UpdatedAtUtc = clock.GetUtcNow().UtcDateTime;
            request.TrialGuestId = ownedGuest.TrialGuestId;
        }
        else request.TrialGuest = guest;
        db.PortalTrialRequests.Add(request);
        await db.SaveChangesAsync(); await tx.CommitAsync();
        return Ok(new { request.Id, message = "體驗申請已送出，工作室將與您聯繫確認；此申請尚未保留名額。" });
    }
}

public class LoginInput
{
    [Required, StringLength(100)] public string Account { get; set; } = "";
    [Required, StringLength(200)] public string Password { get; set; } = "";
}
public class RegisterInput : LoginInput
{
    [Required, StringLength(100)] public string Name { get; set; } = "";
    [Required, EmailAddress, StringLength(200)] public string Email { get; set; } = "";
    [Required, Phone, StringLength(30)] public string Phone { get; set; } = "";
    [Required, MinLength(10), StringLength(200)] public string ConfirmPassword { get; set; } = "";
}
public class TrialInput
{
    public Guid SubmissionKey { get; set; }
    [Range(1, int.MaxValue)] public int SessionId { get; set; }
    [Range(1, long.MaxValue)] public long PlanId { get; set; }
    [Required, StringLength(100)] public string Name { get; set; } = "";
    [Required, Phone, StringLength(30)] public string Phone { get; set; } = "";
    [Required, EmailAddress, StringLength(200)] public string Email { get; set; } = "";
    [StringLength(100)] public string? LineId { get; set; }
    [StringLength(300)] public string? Note { get; set; }
}
