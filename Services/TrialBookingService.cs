using System.Data;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
namespace EnjoySpace.Web.Services;

public class TrialBookingService(AppDbContext db, TimeProvider clock)
{
    private DateOnly Today => DateOnly.FromDateTime(clock.GetUtcNow().ToOffset(TimeSpan.FromHours(8)).DateTime);
    public async Task AdminAsync(long actor)
    { if (!(await StaffAccess.RolesAsync(db, actor)).Contains("Admin")) throw new UnauthorizedAccessException(); }
    public async Task<long> StudentAsync(long actor)
    {
        return await db.Students.Where(x => x.UserId == actor && x.IsActive && x.User.IsActive && db.UserRoles.Any(r => r.UserId == actor && r.Role.IsActive && r.Role.RoleCode == "Student"))
            .Select(x => (long?)x.StudentId).SingleOrDefaultAsync() ?? throw new UnauthorizedAccessException();
    }
    private static void Version(byte[] actual, byte[] expected)
    { if (expected == null || !actual.SequenceEqual(expected)) throw new InvalidOperationException("資料已更新，請重新整理後再操作。"); }
    public IQueryable<PortalTrialRequest> Requests => db.PortalTrialRequests.Include(x => x.User).Include(x => x.TrialGuest).Include(x => x.LessonPlan)
        .Include(x => x.LessonPass).ThenInclude(x => x!.Payment).Include(x => x.ClassSession).ThenInclude(x => x.Course);
    private async Task<PortalTrialRequest> LockAsync(long id)
    {
        if (await db.PortalTrialRequests.FromSqlInterpolated($"SELECT * FROM dbo.PortalTrialRequests WITH (UPDLOCK,HOLDLOCK) WHERE Id={id}").SingleOrDefaultAsync() == null)
            throw new InvalidOperationException("找不到此體驗申請，請重新整理申請紀錄。");
        return await Requests.SingleAsync(x => x.Id == id);
    }
    public static string? Unavailable(PortalTrialRequest r, DateOnly today)
    {
        if (r.Status == "Rejected") return "申請已退回，請查看工作室回覆後重新申請。";
        if (r.Status == "Cancelled" || r.LessonPass?.Status == "Cancelled") return "此體驗方案已取消，若需要安排課程請聯繫工作室。";
        if (r.Status != "Approved") return "體驗方案尚未核准，請等候工作室審核。";
        if (r.LessonPass?.Status != "Active" || r.LessonPass.Payment.Status != "Confirmed") return "待工作室確認收款後即可排課。";
        if (r.LessonPass.ValidUntilDate < today) return "體驗方案已到期。";
        return null;
    }
    public async Task ReviewAsync(long id, byte[] version, bool approve, string? note, DateOnly from, long? referrerId, long actor)
    {
        await AdminAsync(actor);
        if (note?.Length > 500 || (!approve && string.IsNullOrWhiteSpace(note))) throw new InvalidOperationException("退回時請填寫原因，最多 500 字。");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var r = await LockAsync(id); Version(r.RowVersion, version);
        if (r.Status is not ("Pending" or "Contacted")) throw new InvalidOperationException("此申請已處理，請重新整理。");
        await StudentAsync(r.UserId);
        if (approve)
        {
            if (from < Today) throw new InvalidOperationException("方案生效日不可早於今天。");
            if (r.LessonPassId != null) throw new InvalidOperationException("此申請已有堂券，請確認既有資料，勿重複建立。");
            if (r.RequiresReferrerAtRequest ?? r.LessonPlan.RequiresReferrer)
            {
                if (!referrerId.HasValue || !await db.Students.AnyAsync(x => x.StudentId == referrerId && x.IsActive && x.UserId != r.UserId))
                    throw new InvalidOperationException("請核對介紹人並選擇有效的介紹學生，不可介紹自己。");
                r.TrialGuest.ReferrerStudentId = referrerId;
            }
            var unit = r.QuotedValidityUnit ?? r.LessonPlan.ValidityUnit;
            var value = r.QuotedValidityUnit == null ? r.LessonPlan.ValidityValue : r.QuotedValidityValue;
            var quota = r.QuotedQuota ?? r.LessonPlan.SessionQuota;
            if (quota <= 0) throw new InvalidOperationException("方案堂數無效。");
            DateOnly? until = unit switch { "Months" => from.AddMonths(value!.Value).AddDays(-1), "Days" => from.AddDays(value!.Value - 1), "Unlimited" => null, _ => throw new InvalidOperationException("請核對方案效期。") };
            var free = r.QuotedPrice == 0;
            r.LessonPass = new LessonPass {
                PurchaseKey = Guid.NewGuid(), LessonPlanId = r.LessonPlanId, TrialGuestId = r.TrialGuestId,
                PlanNameAtPurchase = r.PlanName, PlanTypeAtPurchase = "Trial", SessionQuotaAtPurchase = quota,
                AmountAtPurchase = r.QuotedPrice, ValidFromDate = from, ValidUntilDate = until, CreatedByUserId = actor,
                PurchasedAtUtc = clock.GetUtcNow().UtcDateTime, Status = free ? "Active" : "Pending",
                Payment = new Payment { TrialGuestId = r.TrialGuestId, PaymentPurpose = "Trial", Amount = r.QuotedPrice,
                    Status = free ? "Confirmed" : "Pending", CreatedAtUtc = clock.GetUtcNow().UtcDateTime,
                    ConfirmedByUserId = free ? actor : null, ConfirmedAtUtc = free ? clock.GetUtcNow().UtcDateTime : null,
                    Note = free ? "免費體驗核對通過，無實際收款。" : null }
            };
        }
        r.Status = approve ? "Approved" : "Rejected"; r.ReviewedByUserId = actor; r.ReviewedAtUtc = clock.GetUtcNow().UtcDateTime; r.ReviewNote = note?.Trim();
        await db.SaveChangesAsync(); await tx.CommitAsync();
    }
    private IQueryable<TrialBooking> Held(long request) => db.TrialBookings.Where(x => x.TrialRequestId == request && (x.Status == "Pending" || x.Status == "Approved"));
    public async Task<int> RemainingAsync(PortalTrialRequest r)
    {
        // Legacy approved roster entries without a child booking also consume the original pass.
        var old = r.LessonPassId == null ? 0 : await db.ClassSessionParticipants.CountAsync(x => x.LessonPassId == r.LessonPassId && x.TrialBookingId == null && x.Status == "Active");
        return Math.Max(0, (r.LessonPass?.SessionQuotaAtPurchase ?? 0) - await Held(r.Id).CountAsync() - old);
    }
    private IQueryable<ClassSession> Targets(PortalTrialRequest r, long student)
    {
        var pass = r.LessonPass!;
        var start = new DateTimeOffset(pass.ValidFromDate.ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8));
        DateTimeOffset end = pass.ValidUntilDate is { } date ? new(date.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8)) : DateTimeOffset.MaxValue;
        var now = clock.GetUtcNow();
        return new PortalCatalogService(db).Sessions.Where(s => !s.IsCancelled && s.StartsAt > now && s.StartsAt >= start && s.EndsAt <= end
            && !db.ClassSessionParticipants.Any(p => p.ClassSessionId == s.Id && p.StudentId == student && p.Status == "Active")
            && !db.TrialBookings.Any(b => b.TrialRequest.UserId == r.UserId && b.ClassSessionId == s.Id && (b.Status == "Pending" || b.Status == "Approved"))
            && db.ClassSessionParticipants.Count(p => p.ClassSessionId == s.Id && p.Status == "Active") < (s.CapacityOverride ?? s.Capacity));
    }
    public async Task<object> OptionsAsync(long id, long actor, bool admin = false)
    {
        if (admin) await AdminAsync(actor);
        var r = await Requests.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException("找不到此體驗申請，請重新整理申請紀錄。");
        if (!admin && r.UserId != actor) throw new UnauthorizedAccessException();
        if (!admin && (r.RequiresReferrerAtRequest ?? r.LessonPlan.RequiresReferrer)) throw new InvalidOperationException("免費體驗請直接聯繫工作室安排上課。");
        var student = await StudentAsync(r.UserId);
        var issue = Unavailable(r, Today); if (issue != null) throw new InvalidOperationException(issue);
        return new { r.Id, r.RowVersion, remaining = await RemainingAsync(r), r.LessonPass!.ValidFromDate, r.LessonPass.ValidUntilDate,
            sessions = await Targets(r, student).OrderBy(x => x.StartsAt).Select(x => new { x.Id, x.RowVersion, name = x.Course.Name, x.StartsAt, x.EndsAt, teacher = x.Teacher.User.DisplayName, location = x.StudioLocation.Name }).ToArrayAsync() };
    }
    public async Task<long[]> BookAsync(long id, byte[] version, Guid batch, int[] sessions, long actor, bool admin = false)
    {
        if (admin) await AdminAsync(actor);
        if (batch == Guid.Empty || sessions.Length == 0 || sessions.Length > 20) throw new InvalidOperationException("請選擇要預約的場次，單次最多 20 堂。");
        if (sessions.Distinct().Count() != sessions.Length) throw new InvalidOperationException("重複選擇同一堂課，請調整後再送出。");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var r = await LockAsync(id);
        if (!admin && r.UserId != actor) throw new UnauthorizedAccessException();
        if (!admin && (r.RequiresReferrerAtRequest ?? r.LessonPlan.RequiresReferrer)) throw new InvalidOperationException("免費體驗請直接聯繫工作室安排上課。");
        var student = await StudentAsync(r.UserId);
        // Lock the student across different parent applications to prevent duplicate bookings.
        await db.Students.FromSqlInterpolated($"SELECT * FROM dbo.Students WITH (UPDLOCK,HOLDLOCK) WHERE StudentId={student}").SingleAsync();
        var previous = await db.TrialBookings.Where(x => x.TrialRequestId == id && x.BatchKey == batch).OrderBy(x => x.Id).ToArrayAsync();
        if (previous.Length > 0) {
            if (!previous.Select(x => x.ClassSessionId).Order().SequenceEqual(sessions.Order())) throw new InvalidOperationException("此送出識別碼已使用，請重新整理紀錄。");
            await tx.CommitAsync(); return previous.Select(x => x.Id).ToArray();
        }
        Version(r.RowVersion, version);
        var issue = Unavailable(r, Today); if (issue != null) throw new InvalidOperationException(issue);
        if (sessions.Length > await RemainingAsync(r)) throw new InvalidOperationException("可預約堂數不足；待審預約也會占用堂數，請重新整理。");
        if (await Targets(r, student).CountAsync(x => sessions.Contains(x.Id)) != sessions.Length)
            throw new InvalidOperationException("有場次已預約、額滿、停課、超過效期或不再開放，請重新選擇。");
        var rows = sessions.Select(s => new TrialBooking { TrialRequestId = id, ClassSessionId = s, BatchKey = batch, CreatedAt = clock.GetUtcNow() }).ToArray();
        db.TrialBookings.AddRange(rows); await db.SaveChangesAsync(); await tx.CommitAsync(); return rows.Select(x => x.Id).ToArray();
    }
    public async Task ReviewBookingAsync(long id, byte[] version, bool approve, string? note, long actor)
    {
        await AdminAsync(actor);
        if (note?.Length > 500 || (!approve && string.IsNullOrWhiteSpace(note))) throw new InvalidOperationException("請填寫退回原因，最多 500 字。");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var booking = await db.TrialBookings.SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException("找不到此體驗預約，請重新整理預約紀錄。");
        var r = await LockAsync(booking.TrialRequestId); Version(booking.RowVersion, version);
        if (booking.Status != "Pending") throw new InvalidOperationException("這筆預約已處理，請重新整理。");
        if (approve)
        {
            var issue = Unavailable(r, Today); if (issue != null) throw new InvalidOperationException(issue);
            var student = await StudentAsync(r.UserId);
            var s = await db.ClassSessions.Include(x => x.Course).ThenInclude(x => x.Term).Include(x => x.Teacher).Include(x => x.StudioLocation).SingleAsync(x => x.Id == booking.ClassSessionId);
            var day = DateOnly.FromDateTime(s.StartsAt.ToOffset(TimeSpan.FromHours(8)).DateTime);
            if (s.IsCancelled || s.StartsAt <= clock.GetUtcNow() || !s.Course.IsActive || !s.Course.IsPubliclyVisible || !s.Teacher.IsActive || !s.StudioLocation.IsActive
                || s.Course.Term.Status is not ("Open" or "Closed") || day < r.LessonPass!.ValidFromDate || day > r.LessonPass.ValidUntilDate)
                throw new InvalidOperationException("場次已停課、開始或不符合方案效期，請退回讓學生重選。");
            if (r.LessonPass.ValidUntilDate is { } until && s.EndsAt > new DateTimeOffset(until.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.FromHours(8)))
                throw new InvalidOperationException("場次結束時間超過方案效期，請重新選擇。");
            await SessionCapacity.RequireSeatAsync(db, s.Id);
            if (await db.ClassSessionParticipants.AnyAsync(x => x.StudentId == student && x.ClassSessionId == s.Id && x.Status == "Active"))
                throw new InvalidOperationException("學生已在這堂課的名單，不能重複安排。");
            db.ClassSessionParticipants.Add(new ClassSessionParticipant { ClassSessionId = s.Id, StudentId = student,
                LessonPassId = r.LessonPassId, TrialRequestId = r.Id, TrialBookingId = booking.Id, ParticipationType = "Trial", AddedByUserId = actor, AddedAtUtc = clock.GetUtcNow().UtcDateTime });
        }
        booking.Status = approve ? "Approved" : "Rejected"; booking.ReviewedAt = clock.GetUtcNow(); booking.ReviewedByUserId = actor; booking.ReviewNote = note?.Trim();
        await db.SaveChangesAsync(); await tx.CommitAsync();
    }
    public async Task CancelAsync(long id, byte[] version, string reason, long actor, bool admin = false)
    {
        if (admin) await AdminAsync(actor);
        if (string.IsNullOrWhiteSpace(reason) || reason.Length > 500) throw new InvalidOperationException("請填寫取消原因，最多 500 字。");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var b = await db.TrialBookings.Include(x => x.ClassSession).SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException("找不到此體驗預約，請重新整理預約紀錄。");
        var r = await LockAsync(b.TrialRequestId);
        if (!admin && r.UserId != actor) throw new UnauthorizedAccessException();
        Version(b.RowVersion, version);
        if (b.Status is not ("Pending" or "Approved")) throw new InvalidOperationException("此預約已取消或退回。");
        var p = await db.ClassSessionParticipants.Include(x => x.Attendance).SingleOrDefaultAsync(x => x.TrialBookingId == b.Id);
        if (b.Status == "Approved" && (clock.GetUtcNow() >= b.ClassSession.StartsAt || p?.Attendance.Status != "Scheduled" || p.Attendance.CheckInMethod != null))
            throw new InvalidOperationException("已開課或已點名，無法取消這堂體驗。");
        if (p != null) { p.Status = "Cancelled"; p.CancellationReason = reason.Trim(); p.CancelledAtUtc = clock.GetUtcNow().UtcDateTime; }
        b.Status = "Cancelled"; b.ReviewNote = reason.Trim(); b.ReviewedByUserId = actor; b.ReviewedAt = clock.GetUtcNow();
        await db.SaveChangesAsync(); await tx.CommitAsync();
    }
}
