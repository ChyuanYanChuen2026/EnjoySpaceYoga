using EnjoySpace.Web.Data;
using EnjoySpace.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");
builder.Services.AddAuthentication().AddCookie(PortalAuthentication.Scheme, PortalAuthentication.Configure);
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Events.OnRedirectToAccessDenied = async context =>
    {
        if (!context.HttpContext.User.IsInRole(StaffAccess.ReadOnlyRole))
        {
            context.Response.Redirect(context.RedirectUri);
            return;
        }
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync("目前帳號只有後台查詢權限，無法執行這項操作。");
    };
    options.Events.OnValidatePrincipal = async context =>
    {
        var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        if (!long.TryParse(context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
        { context.RejectPrincipal(); return; }
        if (!await PasswordSession.IsCurrentAsync(db, context.Principal, id))
        { context.RejectPrincipal(); return; }
        var roles = await StaffAccess.RolesAsync(db, id);
        if (roles.Count == 0) { context.RejectPrincipal(); return; }
        var identity = (ClaimsIdentity)context.Principal!.Identity!;
        foreach (var claim in identity.FindAll(ClaimTypes.Role).ToList()) identity.RemoveClaim(claim);
        identity.AddClaims(roles.Select(role => new Claim(ClaimTypes.Role, role)));
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BackOffice", policy => policy.RequireAuthenticatedUser()
        .RequireAssertion(context => context.User.IsInRole("Admin") || BackOfficeQueryAttribute.Allows(context)));
    options.AddPolicy("BackOfficeTeacher", policy => policy.RequireAuthenticatedUser()
        .RequireAssertion(context => context.User.IsInRole("Admin") || context.User.IsInRole("Teacher")
            || BackOfficeQueryAttribute.Allows(context)));
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("check-in", context => RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 200, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    options.AddPolicy("password-recovery", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    options.AddPolicy("portal-trial", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("找不到 DefaultConnection 連線字串。");

builder.Services.AddDbContext<AppDbContext>(options =>options.UseSqlServer(connectionString));
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<PortalCatalogService>();
builder.Services.AddScoped<AdminCatalogService>();
builder.Services.AddScoped<ClassSessionService>();
builder.Services.AddScoped<EnrollmentApplicationService>();
builder.Services.AddScoped<EnrollmentWorkflowService>();
builder.Services.AddScoped<TermScheduleService>();
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<CheckInService>();
builder.Services.AddScoped<TrialBookingService>();
builder.Services.AddScoped<TrialReviewSummaryService>();
builder.Services.AddScoped<LeaveMakeupService>();
builder.Services.AddScoped<TrialLessonService>();
builder.Services.AddScoped<TeacherRequestService>();
builder.Services.AddScoped<TeacherDashboardService>();
builder.Services.Configure<PasswordRecoveryOptions>(builder.Configuration.GetSection("PasswordRecovery"));
builder.Services.AddScoped<IPasswordResetEmailSender, SmtpPasswordResetEmailSender>();
builder.Services.AddScoped<PasswordRecoveryService>();

var app = builder.Build();
var resetArgument = Array.IndexOf(args, "--reset-password");
if (resetArgument >= 0)
{
    if (!app.Environment.IsDevelopment() || resetArgument + 1 >= args.Length)
        throw new InvalidOperationException("請在 Development 環境使用 --reset-password 帳號。");
    using var scope = app.Services.CreateScope();
    await AdminAccountSetup.ResetPasswordAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>(),  args[resetArgument + 1]);
    return;
}

var adminArgument = Array.IndexOf(args, "--create-admin");
var readOnlyArgument = Array.IndexOf(args, "--create-readonly");
if (adminArgument >= 0 || readOnlyArgument >= 0)
{
    var argument = adminArgument >= 0 ? adminArgument : readOnlyArgument;
    if (!app.Environment.IsDevelopment() || argument + 1 >= args.Length || (adminArgument >= 0 && readOnlyArgument >= 0))
        throw new InvalidOperationException("請在 Development 環境使用 --create-admin 帳號或 --create-readonly 帳號。");
    using var scope = app.Services.CreateScope();
    await AdminAccountSetup.CreateAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>(), args[argument + 1], readOnlyArgument >= 0);
    return;
}
var seedOnly = args.Contains("--seed-only", StringComparer.OrdinalIgnoreCase);
if (app.Environment.IsDevelopment() &&
    (seedOnly || builder.Configuration.GetValue<bool>("Seed:SyncAutumn2026Schedule")))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var result = await DbSeeder.SeedAsync(db);
    Console.WriteLine(
        $"2026 秋季課表同步完成：Mia 建立={result.MiaCreated}，Celia 建立={result.CeliaCreated}，" +
        $"課程新增={result.CoursesCreated}，課程更新={result.CoursesUpdated}，" +
        $"場次新增={result.SessionsCreated}，" +
        $"場次更新={result.SessionsUpdated}，場次移除={result.SessionsRemoved}。");
}
if (seedOnly)
{
    return;
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
// The portal has a separate cookie and cannot inherit an Admin MVC session.
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/portal"))
    {
        var result = await context.AuthenticateAsync(PortalAuthentication.Scheme);
        context.User = result.Principal ?? new ClaimsPrincipal(new ClaimsIdentity());
    }
    await next();
});

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
