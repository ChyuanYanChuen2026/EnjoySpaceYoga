using System.Security.Claims;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

internal static class PasswordRecoveryChecks
{
    public static async Task RunAsync(Func<AppDbContext> createDb, Action<bool, string> assert)
    {
        var protection = new EphemeralDataProtectionProvider();
        var sender = new CaptureEmail();
        var clock = new TestClock();
        var settings = Options.Create(new PasswordRecoveryOptions { PublicBaseUrl = "https://studio.example/" });
        var environment = new TestEnvironment();
        PasswordRecoveryService Service(AppDbContext db) => new(db, protection, sender, settings, clock, environment);
        long userId;
        await using (var db = createDb())
        {
            var role = await db.Roles.SingleAsync(x => x.RoleCode == "Teacher");
            var user = new User { Account = "password-reset-check", DisplayName = "密碼測試老師", Email = "teacher@example.test" };
            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, "Original-password_2026!");
            db.Teachers.Add(new Teacher { User = user });
            db.UserRoles.Add(new UserRole { User = user, RoleId = role.RoleId });
            await db.SaveChangesAsync();
            userId = user.UserId;
        }
        async Task<string> Request()
        {
            await using var db = createDb();
            await Service(db).RequestAsync("password-reset-check", "teacher@example.test");
            return Uri.UnescapeDataString(new Uri(sender.Url).Query["?token=".Length..]);
        }
        async Task<bool> Valid(string token) { await using var db = createDb(); return await Service(db).IsValidAsync(token); }
        await using (var db = createDb())
        {
            await Service(db).RequestAsync("missing-account", "teacher@example.test");
            await Service(db).RequestAsync("password-reset-check", "wrong@example.test");
            assert(sender.Count == 0, "password reset: unknown account and mismatched email send nothing");
            settings.Value.PublicBaseUrl = "http://untrusted.example/";
            assert(!Service(db).IsConfigured, "password reset: non-local HTTP link configuration rejected");
            settings.Value.PublicBaseUrl = "https://studio.example/";
        }
        var token = await Request();
        assert(sender.Count == 1 && sender.Email == "teacher@example.test" && sender.Url.StartsWith("https://studio.example/Account/ResetPassword?token="),
            "password reset: verification link sent to stored email with configured origin");
        assert(await Valid(token) && await Valid(token), "password reset: opening link does not consume it");
        assert(!await Valid(token + "tampered") && !await Valid("invalid"), "password reset: tampering and malformed tokens rejected");
        clock.Now = clock.Now.AddMinutes(30);
        assert(!await Valid(token), "password reset: link expires at 30 minutes");
        token = await Request();
        await using (var db = createDb())
            assert(!await Service(db).ResetAsync(token, "short"), "password reset: short password rejected");
        assert(await Valid(token), "password reset: validation failure preserves link");
        ClaimsPrincipal oldPrincipal;
        await using (var db = createDb())
        {
            var user = await db.Users.SingleAsync(x => x.UserId == userId);
            oldPrincipal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(PasswordSession.ClaimType, PasswordSession.Version(user.PasswordHash))], "test"));
            assert(await PasswordSession.IsCurrentAsync(db, oldPrincipal, userId), "password reset: current session accepted before reset");
        }
        await using (var db1 = createDb())
        await using (var db2 = createDb())
        {
            var first = Service(db1); var second = Service(db2);
            await first.IsValidAsync(token); await second.IsValidAsync(token);
            assert(await first.ResetAsync(token, "New-password_2026!"), "password reset: new password saved");
            assert(!await second.ResetAsync(token, "Concurrent-password_2026!"), "password reset: stale concurrent submission rejected");
        }
        assert(!await Valid(token), "password reset: used link cannot be replayed");
        await using (var db = createDb())
        {
            var user = await db.Users.SingleAsync(x => x.UserId == userId);
            var hasher = new PasswordHasher<User>();
            assert(hasher.VerifyHashedPassword(user, user.PasswordHash, "New-password_2026!") != PasswordVerificationResult.Failed &&
                hasher.VerifyHashedPassword(user, user.PasswordHash, "Original-password_2026!") == PasswordVerificationResult.Failed,
                "password reset: only new password authenticates");
            assert(!await PasswordSession.IsCurrentAsync(db, oldPrincipal, userId), "password reset: old MVC and portal session version rejected");
        }
        token = await Request();
        await using (var db = createDb())
        {
            var user = await db.Users.SingleAsync(x => x.UserId == userId);
            user.Email = "updated@example.test";
            await db.SaveChangesAsync();
        }
        assert(!await Valid(token), "password reset: email change invalidates existing link");
        await using (var db = createDb())
        {
            var user = await db.Users.SingleAsync(x => x.UserId == userId);
            user.Email = "teacher@example.test";
            await db.SaveChangesAsync();
        }
        token = await Request();
        await using (var db = createDb())
        {
            var teacher = await db.Teachers.Include(x => x.User).SingleAsync(x => x.UserId == userId);
            teacher.IsActive = false;
            await db.SaveChangesAsync();
        }
        assert(!await Valid(token), "password reset: disabled teacher cannot use outstanding link");
        var count = sender.Count;
        await using (var db = createDb()) await Service(db).RequestAsync("password-reset-check", "teacher@example.test");
        assert(sender.Count == count, "password reset: disabled teacher does not receive reset email");
        await using (var db = createDb())
        {
            var teacher = await db.Teachers.Include(x => x.User).SingleAsync(x => x.UserId == userId);
            teacher.IsActive = true; teacher.User.IsActive = false;
            await db.SaveChangesAsync();
        }
        await using (var db = createDb()) await Service(db).RequestAsync("password-reset-check", "teacher@example.test");
        assert(sender.Count == count, "password reset: disabled user cannot reactivate through password reset");
    }

    private sealed class CaptureEmail : IPasswordResetEmailSender
    {
        public bool IsConfigured => true;
        public int Count { get; private set; }
        public string Url { get; private set; } = "";
        public string Email { get; private set; } = "";
        public Task SendAsync(string email, string resetUrl, CancellationToken cancellationToken)
        { Count++; Url = resetUrl; Email = email; return Task.CompletedTask; }
    }
    private sealed class TestClock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 10, 17, 0, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => Now;
    }
    private sealed class TestEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "PasswordRecoveryChecks";
        public string ContentRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
