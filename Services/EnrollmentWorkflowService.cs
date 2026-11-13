using System.Data;
using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using EnjoySpace.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System.Text.Json;

namespace EnjoySpace.Web.Services;

public class EnrollmentWorkflowService(AppDbContext db, IDataProtectionProvider protection, TimeProvider? clock = null)
{
    private readonly TimeProvider clock = clock ?? TimeProvider.System;
    public const string NoRefundPolicy = "報名後不提供學生自行退課退款，請在報名前確認完整課表、堂數與金額。";
    private readonly ITimeLimitedDataProtector confirmationProtector = protection
        .CreateProtector("EnjoySpace.EnrollmentConfirmation.v1").ToTimeLimitedDataProtector();
    public async Task RequireAdminAsync(long actorId)
    {
        if (!await db.UserRoles.AnyAsync(x => x.UserId == actorId && x.User.IsActive && x.Role.IsActive && x.Role.RoleCode == "Admin"))
            throw new InvalidOperationException("請使用有效的管理員帳號登入。");
    }

    public async Task<long> RequireStudentAsync(long actorId)
    {
        if (!await db.UserRoles.AnyAsync(x => x.UserId == actorId && x.User.IsActive && x.Role.IsActive && x.Role.RoleCode == "Student"))
            throw new InvalidOperationException("請使用有效的學生帳號登入後報名。");
        return await db.Students.Where(x => x.UserId == actorId && x.IsActive).Select(x => (long?)x.StudentId).SingleOrDefaultAsync()
            ?? throw new InvalidOperationException("找不到有效的學生資料，請聯繫工作室。");
    }
    private async Task RequireActorAsync(long studentId, long actorId, bool studentAction)
    {
        if (studentAction)
        {
            if (await RequireStudentAsync(actorId) != studentId) throw new InvalidOperationException("只能為本人申請報名。");
        }
        else await RequireAdminAsync(actorId);
    }
    public async Task<EnrollmentPreviewModel> PreviewAsync(long studentId, long courseId, long actorId, bool studentAction = false)
    {
        await RequireActorAsync(studentId, actorId, studentAction);
        var preview = await LoadPreviewAsync(studentId, courseId, studentAction);
        preview.ConfirmationToken = confirmationProtector.Protect(
            JsonSerializer.Serialize(new ConfirmationTicket(actorId, Fingerprint(preview))), TimeSpan.FromMinutes(30));
        return preview;
    }

    public async Task<long> CreateAsync(long studentId, long courseId, long actorId,
        string confirmationToken, bool confirmDetails, bool confirmNoRefund, bool studentAction = false, string? requestedPaymentMethod = null)
    {
        await RequireActorAsync(studentId, actorId, studentAction);
        if (studentAction && requestedPaymentMethod is not ("Cash" or "Transfer"))
            throw new InvalidOperationException("請選擇現場現金或銀行轉帳。");
        if (!confirmDetails || !confirmNoRefund)
            throw new InvalidOperationException(studentAction ? "請先核對完整課表與金額，並勾選確認報名須知。" : "請先與學生核對完整報名內容，並確認不提供退課退款。");

        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        // Retrying after a lost response must not create a second application/payment.
        if (studentAction)
        {
            var existing = await db.EnrollmentApplications.Include(x => x.Payment).FirstOrDefaultAsync(x =>
                x.StudentId == studentId && x.CourseId == courseId &&
                (x.Status == EnrollmentApplicationStatus.Pending || x.Status == EnrollmentApplicationStatus.Approved));
            if (existing != null)
            {
                if (existing.Payment?.PaymentMethod != requestedPaymentMethod)
                    throw new InvalidOperationException("您已有此課程的報名申請，請至報名進度確認；變更付款方式請聯繫工作室。");
                return existing.Id;
            }
        }
        var preview = await LoadPreviewAsync(studentId, courseId, studentAction);
        ConfirmationTicket? ticket;
        try
        {
            ticket = JsonSerializer.Deserialize<ConfirmationTicket>(confirmationProtector.Unprotect(confirmationToken));
        }
        catch (Exception e) when (e is CryptographicException or JsonException or ArgumentException)
        {
            throw new InvalidOperationException("報名確認已失效，請重新選擇並核對完整課表。");
        }
        if (ticket is null || ticket.ActorId != actorId || ticket.Fingerprint != Fingerprint(preview))
            throw new InvalidOperationException("學生或課程資料已變更，請重新核對報名內容後再送出。");
        
        // 即使核對課表期間跨過截止時間，也不能建立新的申請或應收款。
        var registrationError = RegistrationWindow.UnavailableReason(
            preview.RegistrationStartAtUtc, preview.RegistrationEndAtUtc, clock.GetUtcNow());
        if (registrationError != null) throw new InvalidOperationException(registrationError);
        // Shared server quote; selecting a payment method never confirms payment.
        var application = new EnrollmentApplication
        {
            StudentId = studentId, 
            CourseId = courseId, 
            ApplicationType = "FullTerm",
            QuotedSessionCount = (short)preview.Sessions.Count,
            QuotedAmount = preview.Amount,
            PolicyConfirmedAtUtc = DateTime.UtcNow,
            PolicyConfirmedByUserId = actorId,
            ConfirmedPolicy = NoRefundPolicy,
            Payment = new Payment { 
                StudentId = studentId, 
                Amount = preview.Amount,
                PaymentPurpose = "CourseEnrollment",
                PaymentMethod = studentAction ? requestedPaymentMethod : null
            }
        };
        
        db.EnrollmentApplications.Add(application);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return application.Id;
    }

    public async Task AdjustAmountAsync(long id, byte[] version, decimal amount, string reason, long actorId)
    {
        await RequireAdminAsync(actorId);
        if (amount < 0 || amount > 99999999m || amount != decimal.Truncate(amount))
            throw new InvalidOperationException("自訂應收金額須為 0 至 99,999,999 的整數。");
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 500)
            throw new InvalidOperationException("請填寫 500 字內的金額調整原因。");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var application = await LoadAsync(id, version);
        var payment = application.Payment;
        if (application.Status != EnrollmentApplicationStatus.Pending || payment is null || payment.Status != "Pending"
            || payment.StudentId != application.StudentId)
            throw new InvalidOperationException("僅待審且尚未確認收款的報名可調整應收金額。");
        if (payment.Amount == amount) { await tx.CommitAsync(); return; }
        db.EnrollmentAmountAdjustments.Add(new EnrollmentAmountAdjustment
        {
            EnrollmentApplicationId = id, PreviousAmount = payment.Amount, Amount = amount,
            Reason = reason.Trim(), AdjustedByUserId = actorId, AdjustedAt = clock.GetUtcNow()
        });
        payment.Amount = amount;
        // Update the application version too: old payment/review forms must be rejected.
        db.Entry(application).Property(x => x.QuotedAmount).IsModified = true;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task ConfirmPaymentAsync(long id, byte[] version, decimal receivedAmount,
        string method, DateTime paidAtTaiwan, string? note, long actorId, bool confirmStudentAgreement)
    {
        await RequireAdminAsync(actorId);
        
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        
        var application = await LoadAsync(id, version);
        if (!confirmStudentAgreement)
            throw new InvalidOperationException("請先再次確認學生、完整課表、金額及不提供退課退款政策，再登記收款。");
        await ValidateApplicationScheduleAsync(application);
        
        if (application.Status != EnrollmentApplicationStatus.Pending)
            throw new InvalidOperationException("此報名已完成審核，無法再確認付款。");
        
        var payment = application.Payment ?? throw new InvalidOperationException("申請尚未建立應收款。");
        
        if (payment.Status != "Pending") throw new InvalidOperationException("付款已處理，請重新整理。");
        
        if (method is not ("Cash" or "Transfer" or "LinePay")) throw new InvalidOperationException("請選擇現金、轉帳或 Line Pay。");
        
        if (receivedAmount != payment.Amount || receivedAmount < 0)
            throw new InvalidOperationException("實收金額必須等於目前應收金額；請先調整應收並填寫原因，不支援部分付款。");
        
        var paidAtUtc = new DateTimeOffset(DateTime.SpecifyKind(paidAtTaiwan, DateTimeKind.Unspecified), TimeSpan.FromHours(8)).UtcDateTime;
        
        if (paidAtUtc > DateTime.UtcNow.AddMinutes(1) || paidAtUtc < application.SubmittedAt.UtcDateTime.AddMinutes(-1))
            throw new InvalidOperationException("付款時間須介於申請時間與現在之間。");
        
        if (note?.Length > 500) throw new InvalidOperationException("備註最多 500 字。");
        payment.PaymentMethod = method;
        payment.PaidAtUtc = paidAtUtc;
        payment.Status = "Confirmed";
        payment.ConfirmedByUserId = actorId;
        payment.ConfirmedAtUtc = DateTime.UtcNow;
        payment.Note = note?.Trim();
        // Also record explicit confirmation for applications created before this workflow existed.
        application.PolicyConfirmedAtUtc ??= DateTime.UtcNow;
        application.PolicyConfirmedByUserId ??= actorId;
        application.ConfirmedPolicy = NoRefundPolicy;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task ReviewAsync(long id, byte[] version, bool approve, string? note, long actorId, bool confirmFinalReview)
    {
        await RequireAdminAsync(actorId);
        
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        
        var application = await LoadAsync(id, version);
        
        if (application.Status != EnrollmentApplicationStatus.Pending)
            throw new InvalidOperationException("此報名已審核，請重新整理。");
        
        if (note?.Length > 500) throw new InvalidOperationException("審核備註最多 500 字。");
        
        if (!approve && string.IsNullOrWhiteSpace(note))
            throw new InvalidOperationException("退回報名時請填寫原因。");
        
        if (approve)
        {
            if (!confirmFinalReview)
                throw new InvalidOperationException("請完成最後核對，確認學生已了解報名後不提供退課退款。");
            await ValidateApplicationScheduleAsync(application);
            if (application.Payment is not { Status: "Confirmed" } payment ||
                payment.StudentId != application.StudentId)
                throw new InvalidOperationException("需先確認付款，才能核准報名。");
            
            if (!application.Student.IsActive || !application.Course.IsActive)
                throw new InvalidOperationException("學生或課程已停用，無法核准。");
            
            if (await db.CourseEnrollments.AnyAsync(x => x.CourseId == application.CourseId && x.StudentId == application.StudentId))
                throw new InvalidOperationException("已有正式報名，不能重複核准。");
            
            var capacities = await db.ClassSessions.Where(x => x.CourseId == application.CourseId && !x.IsExtraSession)
                .Select(x => x.CapacityOverride ?? x.Capacity).ToListAsync();
            
            if (capacities.Count == 0) throw new InvalidOperationException("課程沒有場次。");
            
            var capacity = Math.Min(application.Course.DefaultCapacity, capacities.Min());
            
            if (await db.CourseEnrollments.CountAsync(x => x.CourseId == application.CourseId && x.Status == "Active") >= capacity)
                throw new InvalidOperationException("課程名額已滿，無法核准；已收款可退回並列入待退款。");

            // Makeup participants occupy the same seats as regular enrollments.
            var sessionIds = await db.ClassSessions.Where(x => x.CourseId == application.CourseId && !x.IsExtraSession).OrderBy(x => x.Id).Select(x => x.Id).ToListAsync();
            foreach (var sessionId in sessionIds) await SessionCapacity.RequireSeatAsync(db, sessionId);
            
            var enrollment = new CourseEnrollment
            {
                CourseId = application.CourseId, StudentId = application.StudentId,
                EnrollmentApplicationId = application.Id, CreatedByUserId = actorId
            };
            db.CourseEnrollments.Add(enrollment);
            await AttendanceService.AddEnrollmentRosterAsync(db, enrollment, actorId);
            
            application.Status = EnrollmentApplicationStatus.Approved;
            application.PolicyConfirmedAtUtc ??= DateTime.UtcNow;
            application.PolicyConfirmedByUserId ??= actorId;
            application.ConfirmedPolicy = NoRefundPolicy;
        }
        else
        {
            application.Status = EnrollmentApplicationStatus.Rejected;
            if (application.Payment is { } payment)
                payment.Status = payment.Status == "Confirmed" ? "RefundPending" : "Cancelled";
        }
        application.ReviewedAt = DateTimeOffset.UtcNow;
        application.ReviewedByUserId = actorId;
        application.ReviewNote = note?.Trim();
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task RecordRefundAsync(long paymentId, string note, long actorId)
    {
        await RequireAdminAsync(actorId);
        if (string.IsNullOrWhiteSpace(note) || note.Length > 400)
            throw new InvalidOperationException("請填寫 400 字內的實際退款時間、方式與憑據。");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var payment = await db.Payments.SingleOrDefaultAsync(x => x.PaymentId == paymentId && x.Status == "RefundPending")
            ?? throw new InvalidOperationException("付款不在待退款狀態，請重新整理。");
        var refundNote = $"退款登記 UTC {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}，管理員 {actorId}：{note.Trim()}";
        var combinedNote = string.IsNullOrWhiteSpace(payment.Note) ? refundNote : payment.Note + "\n" + refundNote;
        if (combinedNote.Length > 500)
            throw new InvalidOperationException("退款說明加上原收款備註超過 500 字，請縮短退款說明；原收款紀錄將保留。");
        payment.Status = "Refunded";
        payment.Note = combinedNote;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    private async Task<EnrollmentApplication> LoadAsync(long id, byte[] version)
    {
        var application = await db.EnrollmentApplications.Include(x => x.Payment).Include(x => x.Course).ThenInclude(x => x.Term)
            .Include(x => x.Student).SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException("找不到報名申請。");
        if (version is null || !application.RowVersion.SequenceEqual(version))
            throw new InvalidOperationException("資料已被其他人修改，請重新整理後操作。");
        return application;
    }

    private async Task<EnrollmentPreviewModel> LoadPreviewAsync(long studentId, long courseId, bool studentAction = false)
    {
        var student = await db.Students.AsNoTracking().Include(x => x.User)
            .SingleOrDefaultAsync(x => x.StudentId == studentId && x.IsActive)
            ?? throw new InvalidOperationException(studentAction ? "您的學生資料目前無法使用，請聯繫工作室。" : "找不到啟用中的學生。");
        var course = await db.Courses.AsNoTracking().Include(x => x.Term)
            .SingleOrDefaultAsync(x => x.CourseId == courseId && x.IsActive)
            ?? throw new InvalidOperationException("找不到啟用中的課程。");
        if (course.Term.Status != TermStatusOptions.Open || course.Term.ScheduleConfirmedAtUtc is null)
            throw new InvalidOperationException(studentAction ? "這門課程尚未開放報名，請稍後再查看或聯繫工作室。" : "此學期尚未確認整期排課完成並開放報名。");
        var registrationError = RegistrationWindow.UnavailableReason(
            course.Term.RegistrationStartAtUtc, course.Term.RegistrationEndAtUtc, clock.GetUtcNow());
        if (registrationError != null) throw new InvalidOperationException(registrationError);
        if (studentAction)
        {
            if (!await new PortalCatalogService(db).Courses.AnyAsync(x => x.CourseId == courseId))
                throw new InvalidOperationException("此課程目前不開放官網報名，請聯繫工作室。");
            if (await db.ClassSessions.AnyAsync(x => x.CourseId == courseId && !x.IsExtraSession && x.StartsAt <= clock.GetUtcNow()))
                throw new InvalidOperationException("此班級已開課，中途報名請聯繫工作室辦理。");
            if (await db.ClassSessions.AnyAsync(x => x.CourseId == courseId && !x.IsExtraSession &&
                (x.IsCancelled || !x.Teacher.IsActive || !x.StudioLocation.IsActive)))
                throw new InvalidOperationException("整期課表尚有停課或未開放場次，請聯繫工作室確認。");
        }
        if (await db.EnrollmentApplications.AnyAsync(x => x.StudentId == studentId && x.CourseId == courseId
            && (x.Status == EnrollmentApplicationStatus.Pending || x.Status == EnrollmentApplicationStatus.Approved))
            || await db.CourseEnrollments.AnyAsync(x => x.StudentId == studentId && x.CourseId == courseId))
            throw new InvalidOperationException(studentAction ? "您已有這門課程的報名紀錄，請查看報名進度或我的課程。" : "這位學生已有此課程的申請或正式報名。");
        var sessions = await db.ClassSessions.AsNoTracking().Where(x => x.CourseId == courseId && !x.IsExtraSession)
            .OrderBy(x => x.StartsAt).ThenBy(x => x.Id).Select(x => new EnrollmentSessionPreview
            {
                Id = x.Id, StartsAt = x.StartsAt, EndsAt = x.EndsAt, TeacherName = x.Teacher.User.DisplayName,
                LocationName = x.StudioLocation.Name, Capacity = x.CapacityOverride ?? x.Capacity, RowVersion = x.RowVersion
            }).ToListAsync();
        if (sessions.Count is < 1 or > short.MaxValue)
            throw new InvalidOperationException("課程尚無有效的整期場次。");
        var capacity = Math.Min(course.DefaultCapacity, sessions.Min(x => x.Capacity));
        if (await db.CourseEnrollments.CountAsync(x => x.CourseId == courseId && x.Status == "Active") >= capacity)
            throw new InvalidOperationException(studentAction ? "這門課程已額滿，請選擇其他課程或聯繫工作室。" : "課程已額滿，不能建立報名與應收款。");
        if (studentAction)
            foreach (var session in sessions) await SessionCapacity.RequireSeatAsync(db, session.Id);
        return new EnrollmentPreviewModel
        {
            StudentId = studentId, CourseId = courseId, StudentName = student.User.DisplayName,
            StudentAccount = student.User.Account, TermName = course.Term.Name, CourseName = course.Name,
            RegistrationStartAtUtc = RegistrationWindow.AsUtc(course.Term.RegistrationStartAtUtc),
            RegistrationEndAtUtc = RegistrationWindow.AsUtc(course.Term.RegistrationEndAtUtc),
            ServerNowUtc = clock.GetUtcNow(),
            Amount = course.TermPrice, Sessions = sessions
        };
    }

    private async Task ValidateApplicationScheduleAsync(EnrollmentApplication application)
    {
        if (!application.Student.IsActive || !application.Course.IsActive)
            throw new InvalidOperationException("學生或課程已停用，請先核對申請。");
        if (application.Course.Term.ScheduleConfirmedAtUtc is null)
            throw new InvalidOperationException("請先在學期管理確認完整課表，再處理收款與核准。");
        var capacities = await db.ClassSessions.Where(x => x.CourseId == application.CourseId && !x.IsExtraSession)
            .Select(x => x.CapacityOverride ?? x.Capacity).ToListAsync();
        if (capacities.Count == 0 || capacities.Count != application.QuotedSessionCount)
            throw new InvalidOperationException("此申請的報價堂數與目前課表不同，請先人工核對，不能直接收款或核准。");
        var capacity = Math.Min(application.Course.DefaultCapacity, capacities.Min());
        if (await db.CourseEnrollments.CountAsync(x => x.CourseId == application.CourseId && x.Status == "Active") >= capacity)
            throw new InvalidOperationException("課程已額滿，不能繼續收款或核准。");
    }

    private static string Fingerprint(EnrollmentPreviewModel preview) => Convert.ToHexString(SHA256.HashData(
        JsonSerializer.SerializeToUtf8Bytes(new { preview.StudentId, preview.CourseId, preview.StudentName,
            preview.StudentAccount, preview.TermName, preview.CourseName, preview.Amount, preview.Sessions,
            preview.RegistrationStartAtUtc, preview.RegistrationEndAtUtc, Policy = NoRefundPolicy })));

    private sealed record ConfirmationTicket(long ActorId, string Fingerprint);
}
