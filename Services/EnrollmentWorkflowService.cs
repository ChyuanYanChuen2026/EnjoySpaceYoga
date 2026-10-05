using System.Data;
using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using EnjoySpace.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Services;

public class EnrollmentWorkflowService(AppDbContext db)
{
    public async Task RequireAdminAsync(long actorId)
    {
        if (!await db.UserRoles.AnyAsync(x => x.UserId == actorId && x.User.IsActive && x.Role.IsActive && x.Role.RoleCode == "Admin"))
            throw new InvalidOperationException("請使用有效的管理員帳號登入。");
    }

    public async Task<long> CreateAsync(long studentId, long courseId, long actorId)
    {
        await RequireAdminAsync(actorId);

        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        if (!await db.Students.AnyAsync(x => x.StudentId == studentId && x.IsActive))
            throw new InvalidOperationException("找不到啟用中的學生。");

        var course = await db.Courses.Include(x => x.Term).SingleOrDefaultAsync(x => x.CourseId == courseId && x.IsActive) ?? throw new InvalidOperationException("找不到啟用中的課程。");
        if (course.Term.Status != TermStatusOptions.Open)
        {
            throw new InvalidOperationException("只有開放報名學期的課程可以建立報名。");
        }

        if (await db.EnrollmentApplications.AnyAsync(x => x.StudentId == studentId && x.CourseId == courseId && (x.Status == EnrollmentApplicationStatus.Pending || x.Status == EnrollmentApplicationStatus.Approved)) ||
            await db.CourseEnrollments.AnyAsync(x => x.StudentId == studentId && x.CourseId == courseId))
            throw new InvalidOperationException("這位學生已有此課程的申請或正式報名。");

        var count = await db.ClassSessions.CountAsync(x => x.CourseId == courseId);
        
        if (count is < 1 or > short.MaxValue)
            throw new InvalidOperationException("課程需先建立有效場次，才能計算整期報價。");
        
        // 本階段由後台代辦整期報名；報價固定保存，不接受瀏覽器傳入金額。
        var application = new EnrollmentApplication
        {
            StudentId = studentId, CourseId = courseId, ApplicationType = "FullTerm",
            QuotedSessionCount = (short)count, QuotedAmount = course.TermPrice,
            Payment = new Payment { StudentId = studentId, Amount = course.TermPrice, PaymentPurpose = "CourseEnrollment" }
        };
        
        db.EnrollmentApplications.Add(application);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return application.Id;
    }

    public async Task ConfirmPaymentAsync(long id, byte[] version, decimal receivedAmount,
        string method, DateTime paidAtTaiwan, string? note, long actorId)
    {
        await RequireAdminAsync(actorId);
        
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        
        var application = await LoadAsync(id, version);
        
        if (application.Status != EnrollmentApplicationStatus.Pending)
            throw new InvalidOperationException("此報名已完成審核，無法再確認付款。");
        
        var payment = application.Payment ?? throw new InvalidOperationException("申請尚未建立應收款。");
        
        if (payment.Status != "Pending") throw new InvalidOperationException("付款已處理，請重新整理。");
        
        if (method is not ("Cash" or "Transfer")) throw new InvalidOperationException("請選擇現金或轉帳。");
        
        if (receivedAmount != application.QuotedAmount || payment.Amount != application.QuotedAmount)
            throw new InvalidOperationException("實收金額必須等於申請報價；本階段不支援部分付款。");
        
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
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task ReviewAsync(long id, byte[] version, bool approve, string? note, long actorId)
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
            if (application.Payment is not { Status: "Confirmed" } payment ||
                payment.Amount != application.QuotedAmount || payment.StudentId != application.StudentId)
                throw new InvalidOperationException("需先確認付款，才能核准報名。");
            
            if (!application.Student.IsActive || !application.Course.IsActive)
                throw new InvalidOperationException("學生或課程已停用，無法核准。");
            
            if (await db.CourseEnrollments.AnyAsync(x => x.CourseId == application.CourseId && x.StudentId == application.StudentId))
                throw new InvalidOperationException("已有正式報名，不能重複核准。");
            
            var capacities = await db.ClassSessions.Where(x => x.CourseId == application.CourseId)
                .Select(x => x.CapacityOverride ?? x.Capacity).ToListAsync();
            
            if (capacities.Count == 0) throw new InvalidOperationException("課程沒有場次。");
            
            var capacity = Math.Min(application.Course.DefaultCapacity, capacities.Min());
            
            if (await db.CourseEnrollments.CountAsync(x => x.CourseId == application.CourseId && x.Status == "Active") >= capacity)
                throw new InvalidOperationException("課程名額已滿，無法核准；已收款可退回並列入待退款。");
            
            db.CourseEnrollments.Add(new CourseEnrollment
            {
                CourseId = application.CourseId, StudentId = application.StudentId,
                EnrollmentApplicationId = application.Id, CreatedByUserId = actorId
            });
            
            application.Status = EnrollmentApplicationStatus.Approved;
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
        var application = await db.EnrollmentApplications.Include(x => x.Payment).Include(x => x.Course)
            .Include(x => x.Student).SingleOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException("找不到報名申請。");
        if (version is null || !application.RowVersion.SequenceEqual(version))
            throw new InvalidOperationException("資料已被其他人修改，請重新整理後操作。");
        return application;
    }
}
