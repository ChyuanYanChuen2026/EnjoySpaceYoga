using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EnjoySpace.Web.Services;

public sealed class PasswordRecoveryOptions
{
    public string PublicBaseUrl { get; set; } = "";
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string FromAddress { get; set; } = "";
    public string FromName { get; set; } = "Enjoy Space 米亞工作室";
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
}

public interface IPasswordResetEmailSender
{
    bool IsConfigured { get; }
    Task SendAsync(string email, string resetUrl, CancellationToken cancellationToken);
}

public sealed class SmtpPasswordResetEmailSender(IOptions<PasswordRecoveryOptions> options)
    : IPasswordResetEmailSender
{
    public bool IsConfigured => !string.IsNullOrWhiteSpace(options.Value.Host) &&
        MailAddress.TryCreate(options.Value.FromAddress, out _) && options.Value.Port is > 0 and <= 65535 &&
        options.Value.EnableSsl &&
        (string.IsNullOrEmpty(options.Value.UserName) || !string.IsNullOrEmpty(options.Value.Password));

    public async Task SendAsync(string email, string resetUrl, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var message = new MailMessage
        {
            From = new MailAddress(settings.FromAddress, settings.FromName),
            Subject = "Enjoy Space｜設定新密碼",
            Body = $"您好：\n\n請在 30 分鐘內開啟以下驗證連結，設定您的 Enjoy Space 密碼：\n{resetUrl}\n\n密碼設定成功後，連結即失效，請重新登入。若您未提出此申請，請忽略此信，原密碼不會變更。",
            BodyEncoding = Encoding.UTF8,
            SubjectEncoding = Encoding.UTF8
        };
        message.To.Add(new MailAddress(email));
        // STARTTLS is required. Credentials are supplied through secrets/environment configuration.
        using var client = new SmtpClient(settings.Host, settings.Port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = string.IsNullOrEmpty(settings.UserName) ? null : new NetworkCredential(settings.UserName, settings.Password)
        };
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(20));
        await client.SendMailAsync(message, timeout.Token);
    }
}

public static class PasswordSession
{
    public const string ClaimType = "enjoyspace:password-version";
    public static string Version(string hash) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(hash)));
    public static async Task<bool> IsCurrentAsync(AppDbContext db, ClaimsPrincipal? principal, long userId)
    {
        var hash = await db.Users.Where(x => x.UserId == userId && x.IsActive).Select(x => x.PasswordHash).SingleOrDefaultAsync();
        return hash is not null && principal?.FindFirstValue(ClaimType) == Version(hash);
    }
}

public sealed class PasswordRecoveryService(AppDbContext db, IDataProtectionProvider protection,
    IPasswordResetEmailSender emailSender, IOptions<PasswordRecoveryOptions> options,
    TimeProvider clock, IHostEnvironment environment)
{
    private readonly IDataProtector protector = protection.CreateProtector("EnjoySpace.PasswordRecovery.v1");
    private sealed record Ticket(long UserId, string Email, string PasswordVersion, string RowVersion, DateTimeOffset ExpiresAt);

    public bool IsConfigured => emailSender.IsConfigured && BaseUri() is not null;

    private Uri? BaseUri()
    {
        if (!Uri.TryCreate(options.Value.PublicBaseUrl, UriKind.Absolute, out var uri) ||
            !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment)) return null;
        return uri.Scheme == "https" || (environment.IsDevelopment() && uri.Scheme == "http" && uri.IsLoopback) ? uri : null;
    }

    private async Task<bool> EligibleAsync(User user) => user.IsActive && !string.IsNullOrWhiteSpace(user.Email) &&
        (await StaffAccess.RolesAsync(db, user.UserId)).Count > 0;

    public async Task RequestAsync(string account, string email, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured) throw new InvalidOperationException("Password recovery email is not configured.");
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Account == account.Trim(), cancellationToken);
        if (user is null || !string.Equals(user.Email?.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase) ||
            !await EligibleAsync(user)) return;
        var ticket = new Ticket(user.UserId, user.Email!, PasswordSession.Version(user.PasswordHash),
            Convert.ToBase64String(user.RowVersion), clock.GetUtcNow().AddMinutes(30));
        var token = protector.Protect(JsonSerializer.Serialize(ticket));
        var url = new Uri(BaseUri()!, "Account/ResetPassword").AbsoluteUri + "?token=" + Uri.EscapeDataString(token);
        await emailSender.SendAsync(user.Email!, url, cancellationToken);
    }

    private async Task<User?> FindAsync(string? token)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length > 4096) return null;
        Ticket? ticket;
        try { ticket = JsonSerializer.Deserialize<Ticket>(protector.Unprotect(token)); }
        catch (Exception ex) when (ex is CryptographicException or JsonException or FormatException) { return null; }
        if (ticket is null || ticket.ExpiresAt <= clock.GetUtcNow()) return null;
        var user = await db.Users.SingleOrDefaultAsync(x => x.UserId == ticket.UserId);
        if (user is null || ticket.Email != user.Email || ticket.PasswordVersion != PasswordSession.Version(user.PasswordHash) ||
            ticket.RowVersion != Convert.ToBase64String(user.RowVersion) || !await EligibleAsync(user)) return null;
        return user;
    }

    public async Task<bool> IsValidAsync(string? token) => await FindAsync(token) is not null;

    public async Task<bool> ResetAsync(string token, string password)
    {
        if (password.Length is < 10 or > 200) return false;
        var user = await FindAsync(token);
        if (user is null) return false;
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);
        user.UpdatedAtUtc = clock.GetUtcNow().UtcDateTime;
        // SQL rowversion makes simultaneous submissions single-use as well.
        try { await db.SaveChangesAsync(); return true; }
        catch (DbUpdateConcurrencyException) { db.ChangeTracker.Clear(); return false; }
    }
}
