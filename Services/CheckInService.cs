using System.Data;
using System.Security.Cryptography;
using System.Text;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
namespace EnjoySpace.Web.Services;

public class CheckInFault(string code, string message, int? sessionId = null) : InvalidOperationException(message)
{
    public string Code { get; } = code;
    public int? SessionId { get; } = sessionId;
}

public class CheckInService(AppDbContext db, TimeProvider clock, IDataProtectionProvider protection, IServiceScopeFactory scopes, ILogger<CheckInService> logger)
{
    private readonly IDataProtector protector = protection.CreateProtector("EnjoySpace.SessionQr.v1");
    public async Task RequireStaffAsync(int sessionId, long actor)
    {
        var roles = await StaffAccess.RolesAsync(db, actor);
        if (!roles.Contains("Admin") && !(roles.Contains("Teacher") && await db.ClassSessions.AnyAsync(x => x.Id == sessionId && x.Teacher.UserId == actor && x.Teacher.IsActive)))
            throw new UnauthorizedAccessException();
    }
    public async Task<long> StudentAsync(long actor)
    {
        if (!await db.UserRoles.AnyAsync(x => x.UserId == actor && x.Role.RoleCode == "Student" && x.Role.IsActive && x.User.IsActive)) throw new UnauthorizedAccessException();
        return await db.Students.Where(x => x.UserId == actor && x.IsActive).Select(x => (long?)x.StudentId).SingleOrDefaultAsync() ?? throw new UnauthorizedAccessException();
    }
    private static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    public async Task<(string Token, DateTimeOffset OpensAt, DateTimeOffset ExpiresAt)> IssueAsync(int sessionId, long actor)
    {
        await RequireStaffAsync(sessionId, actor);
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var session = await db.ClassSessions.FromSqlInterpolated($"SELECT * FROM dbo.ClassSessions WITH (UPDLOCK,HOLDLOCK) WHERE Id={sessionId}").SingleAsync();
        Window(session, clock.GetUtcNow());
        var qr = await db.SessionQrCodes.SingleOrDefaultAsync(x => x.ClassSessionId == sessionId);
        if (qr == null || qr.OpensAt != session.StartsAt.AddMinutes(-30) || qr.ExpiresAt != session.EndsAt)
        {
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            if (qr == null) { qr = new SessionQrCode { ClassSessionId = sessionId }; db.SessionQrCodes.Add(qr); }
            qr.TokenHash = Hash(token); qr.ProtectedToken = protector.Protect(token);
            qr.OpensAt = session.StartsAt.AddMinutes(-30); qr.ExpiresAt = session.EndsAt;
            await db.SaveChangesAsync();
        }
        var result = (protector.Unprotect(qr.ProtectedToken), qr.OpensAt, qr.ExpiresAt);
        await tx.CommitAsync(); return result;
    }
    private static void Window(ClassSession session, DateTimeOffset now)
    {
        if (session.IsCancelled) throw new CheckInFault("SESSION_CANCELLED", "這堂課已停課，請聯繫工作室。", session.Id);
        if (now < session.StartsAt.AddMinutes(-30)) throw new CheckInFault("CHECK_IN_NOT_OPEN", "開課前 30 分鐘才開放打卡。", session.Id);
        if (now >= session.EndsAt) throw new CheckInFault("CHECK_IN_CLOSED", "這堂課已結束，請洽管理員確認出勤。", session.Id);
    }
    private IQueryable<ClassSessionParticipant> Roster(long student, int session) => db.ClassSessionParticipants
        .Include(x => x.Attendance).Include(x => x.CourseEnrollment).Include(x => x.ClassSession).ThenInclude(x => x.Course)
        .Include(x => x.ClassSession).ThenInclude(x => x.Teacher).ThenInclude(x => x.User)
        .Include(x => x.ClassSession).ThenInclude(x => x.StudioLocation)
        .Where(x => x.StudentId == student && x.ClassSessionId == session);
    private static void Active(ClassSessionParticipant p)
    {
        if (p.Status != "Active" || (p.CourseEnrollmentId != null && p.CourseEnrollment?.Status != "Active"))
            throw new CheckInFault("NOT_ON_ROSTER", "您已請假或不在這堂課的有效名單，請洽工作室。", p.ClassSessionId);
    }
    private object Receipt(ClassSessionParticipant p)
    {
        var now = clock.GetUtcNow();
        var can = p.Status == "Active" && !p.ClassSession.IsCancelled && (p.CourseEnrollmentId == null || p.CourseEnrollment?.Status == "Active")
            && p.Attendance.Status == "Scheduled" && p.Attendance.CheckInMethod != "Manual" && now >= p.ClassSession.StartsAt.AddMinutes(-30) && now < p.ClassSession.EndsAt;
        return new { sessionId = p.ClassSessionId, courseName = p.ClassSession.Course.Name, teacher = p.ClassSession.Teacher.User.DisplayName,
            location = p.ClassSession.StudioLocation.Name, p.ClassSession.StartsAt, p.ClassSession.EndsAt, status = p.Attendance.Status,
            checkedInAt = p.Attendance.CheckedInAtUtc == null ? (DateTimeOffset?)null : new DateTimeOffset(DateTime.SpecifyKind(p.Attendance.CheckedInAtUtc.Value, DateTimeKind.Utc)),
            method = p.Attendance.CheckInMethod, canCheckIn = can, expectedStatus = now > p.ClassSession.StartsAt.AddMinutes(10) ? "Late" : "Present", serverNow = now };
    }
    public async Task<object> ReceiptAsync(int sessionId, long actor)
    {
        var student = await StudentAsync(actor);
        var p = await Roster(student, sessionId).OrderByDescending(x => x.Status == "Active").ThenByDescending(x => x.SessionParticipantId).FirstOrDefaultAsync()
            ?? throw new CheckInFault("NOT_ON_ROSTER", "找不到您的課堂名單，請洽工作室。", sessionId);
        return Receipt(p);
    }
    public async Task<object> ScanAsync(string token, long actor, bool confirm)
    {
        var student = await StudentAsync(actor);
        int? sessionId = null;
        try
        {
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            if (string.IsNullOrWhiteSpace(token) || token.Length != 64) throw new CheckInFault("QR_INVALID", "QR Code 無效，請重新掃描教室的 QR Code。");
            var hash = Hash(token);
            var qr = await db.SessionQrCodes.Include(x => x.ClassSession).SingleOrDefaultAsync(x => x.TokenHash == hash)
                ?? throw new CheckInFault("QR_INVALID", "QR Code 無效或已更新，請重新掃描。");
            sessionId = qr.ClassSessionId;
            // Serialize scans with roster and classroom changes; rowversion also protects manual corrections.
            await db.ClassSessions.FromSqlInterpolated($"SELECT * FROM dbo.ClassSessions WITH (UPDLOCK,HOLDLOCK) WHERE Id={qr.ClassSessionId}").SingleAsync();
            if (qr.OpensAt != qr.ClassSession.StartsAt.AddMinutes(-30) || qr.ExpiresAt != qr.ClassSession.EndsAt)
                throw new CheckInFault("QR_EXPIRED", "課堂時間已調整，請重新掃描最新 QR Code。", sessionId);
            var p = await Roster(student, qr.ClassSessionId).OrderByDescending(x => x.Status == "Active").ThenByDescending(x => x.SessionParticipantId).FirstOrDefaultAsync()
                ?? throw new CheckInFault("NOT_ON_ROSTER", "您不在這堂課的名單，請確認登入帳號或洽工作室。", sessionId);
            Active(p);
            if (p.Attendance.CheckInMethod == "Manual") { await tx.CommitAsync(); return Receipt(p); }
            if (p.Attendance.Status is "Present" or "Late") { await tx.CommitAsync(); return Receipt(p); }
            Window(qr.ClassSession, clock.GetUtcNow());
            if (p.Attendance.Status != "Scheduled") throw new CheckInFault("ATTENDANCE_LOCKED", "出勤已由工作室登記，請聯繫管理員確認。", sessionId);
            if (confirm)
            {
                var now = clock.GetUtcNow();
                p.Attendance.Status = now > p.ClassSession.StartsAt.AddMinutes(10) ? "Late" : "Present";
                p.Attendance.CheckInMethod = "QR"; p.Attendance.CheckedInAtUtc = now.UtcDateTime;
                p.Attendance.CheckedInByUserId = actor; p.Attendance.UpdatedAtUtc = now.UtcDateTime;
                db.CheckInEvents.Add(new CheckInEvent { UserId = actor, ClassSessionId = sessionId, Code = "CHECKED_IN", Message = "學生確認打卡成功", AttendanceStatus = p.Attendance.Status, CreatedAt = now });
                var failures = await db.CheckInEvents.Where(x => x.UserId == actor && x.ClassSessionId == sessionId && x.IsError && x.ResolvedAt == null).ToListAsync();
                foreach (var failure in failures) { failure.ResolvedAt = now; failure.Resolution = "學生重試後成功打卡"; }
                await db.SaveChangesAsync();
            }
            var result = Receipt(p); await tx.CommitAsync(); return result;
        }
        catch (CheckInFault e) { await LogErrorAsync(actor, e.SessionId ?? sessionId, e.Code, e.Message); throw; }
        catch (Exception e) when (e is DbUpdateException or Microsoft.Data.SqlClient.SqlException)
        {
            logger.LogError(e, "Check-in persistence failure for session {SessionId}", sessionId);
            await LogErrorAsync(actor, sessionId, "CHECK_IN_ERROR", "打卡儲存結果待確認，請查詢打卡結果。");
            throw new CheckInFault("ATTENDANCE_CONFLICT", "打卡結果待確認，請先查詢結果；若持續失敗請洽管理員。", sessionId);
        }
    }
    public async Task LogErrorAsync(long actor, int? sessionId, string code, string message)
    {
        try {
            using var scope = scopes.CreateScope(); var audit = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            audit.CheckInEvents.Add(new CheckInEvent { UserId = actor, ClassSessionId = sessionId, Code = code, Message = message, IsError = true, CreatedAt = clock.GetUtcNow() });
            await audit.SaveChangesAsync();
        } catch (Exception e) { logger.LogError(e, "Could not persist check-in anomaly for user {UserId} and session {SessionId}", actor, sessionId); }
    }
}
