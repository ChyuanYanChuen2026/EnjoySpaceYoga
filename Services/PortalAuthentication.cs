using System.Security.Claims;
using EnjoySpace.Web.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Services;

public static class PortalAuthentication
{
    public const string Scheme = "Portal";
    public static readonly string[] Roles = ["Student", "Teacher", "TrialGuest"];

    public static void Configure(CookieAuthenticationOptions options)
    {
        options.Cookie.Name = "EnjoySpace.Portal";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = 401; return Task.CompletedTask; };
        options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = 403; return Task.CompletedTask; };
        options.Events.OnValidatePrincipal = async context =>
        {
            var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            if (!long.TryParse(context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
            { context.RejectPrincipal(); return; }
            var roles = await db.UserRoles.Where(x => x.UserId == id && x.User.IsActive && x.Role.IsActive)
                .Select(x => x.Role.RoleCode).ToListAsync();
            var claimed = context.Principal!.FindAll(ClaimTypes.Role).Select(x => x.Value).Order().ToArray();
            if (roles.Contains("Admin") || !roles.Intersect(Roles).Any() ||
                !claimed.SequenceEqual(roles.Intersect(Roles).Order())) context.RejectPrincipal();
        };
    }
}
