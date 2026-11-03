using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Models.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Services;

public class TrialLessonService(AppDbContext db, IDataProtectionProvider protection, TimeProvider clock)
{
    private readonly ITimeLimitedDataProtector protector = protection.CreateProtector("EnjoySpace.LessonPurchase.v1").ToTimeLimitedDataProtector();
    private DateTime UtcNow => clock.GetUtcNow().UtcDateTime;
    public DateOnly Today => DateOnly.FromDateTime(clock.GetUtcNow().ToOffset(TimeSpan.FromHours(8)).DateTime);
    private async Task Admin(long actor)
    {
        if (!await db.UserRoles.AnyAsync(x => x.UserId == actor && x.User.IsActive && x.Role.IsActive && x.Role.RoleCode == "Admin"))
            throw new InvalidOperationException("請使用有效的管理員帳號登入。");
    }
    private static void Validate(object input)
    {
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(input, new ValidationContext(input), errors, true))
            throw new InvalidOperationException("欄位資料不完整或格式不正確，請檢查必填欄位、長度與數值。");
    }
    private static void Version(byte[] actual, byte[]? input)
    {
        if (input is null || !actual.SequenceEqual(input)) throw new InvalidOperationException("資料已變更，請重新載入並核對。");
    }

    public async Task<LessonPlanEditModel?> PlanAsync(long id) => await db.LessonPlans.AsNoTracking().Where(x => x.LessonPlanId == id)
        .Select(x => new LessonPlanEditModel { Id = x.LessonPlanId, RowVersion = x.RowVersion, PlanCode = x.PlanCode, PlanName = x.PlanName,
            PlanType = x.PlanType, SessionQuota = x.SessionQuota, Price = x.Price, ValidityUnit = x.ValidityUnit, ValidityValue = x.ValidityValue, IsActive = x.IsActive, RequiresReferrer = x.RequiresReferrer }).SingleOrDefaultAsync();
    public async Task<long> SavePlanAsync(LessonPlanEditModel input, long actor)
    {
        await Admin(actor); Validate(input);
        if (input.Id < 0 || input.PlanType is not ("Trial" or "Flexible")) throw new InvalidOperationException("請選擇有效的方案類型。");
        if (input.RequiresReferrer && input.PlanType != "Trial") throw new InvalidOperationException("介紹人條件僅適用於體驗方案。");
        if (input.ValidityUnit is not ("Days" or "Months" or "Unlimited") || (input.ValidityUnit != "Unlimited" && input.ValidityValue is not > 0))
            throw new InvalidOperationException("有限效期請填寫有效天數或月數。");
        if (decimal.Round(input.Price, 2) != input.Price) throw new InvalidOperationException("金額最多小數點後兩位。");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var code = input.PlanCode.Trim().ToUpperInvariant();
        if (await db.LessonPlans.AnyAsync(x => x.PlanCode == code && x.LessonPlanId != input.Id)) throw new InvalidOperationException("方案代碼已存在。");
        var plan = input.Id == 0 ? new LessonPlan { CreatedAtUtc = UtcNow } :
            await db.LessonPlans.SingleOrDefaultAsync(x => x.LessonPlanId == input.Id) ?? throw new InvalidOperationException("找不到方案。");
        if (input.Id > 0) Version(plan.RowVersion, input.RowVersion);
        plan.PlanCode = code; plan.PlanName = input.PlanName.Trim(); plan.PlanType = input.PlanType;
        plan.AudienceType = input.PlanType == "Trial" ? "TrialGuest" : "Student";
        plan.SessionQuota = input.SessionQuota; plan.Price = input.Price; plan.ValidityUnit = input.ValidityUnit;
        plan.ValidityValue = input.ValidityUnit == "Unlimited" ? null : input.ValidityValue; plan.IsActive = input.IsActive;
        plan.RequiresReferrer = input.RequiresReferrer;
        if (input.Id == 0) db.LessonPlans.Add(plan);
        await db.SaveChangesAsync(); await tx.CommitAsync(); return plan.LessonPlanId;
    }

    public async Task PopulatePurchaseAsync(LessonPurchaseModel input)
    {
        input.OwnerType = input.OwnerType == "Student" ? "Student" : "TrialGuest";
        input.Plans = (await db.LessonPlans.AsNoTracking().Where(x => x.IsActive && x.AudienceType == input.OwnerType).OrderBy(x => x.PlanName).ToListAsync())
            .Select(x => new SelectListItem($"{x.PlanName} · {x.SessionQuota} 堂 · ${x.Price:N0}", x.LessonPlanId.ToString())).ToList();
        input.Owners = input.OwnerType == "Student"
            ? await db.Students.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.User.DisplayName).Select(x => new SelectListItem(x.User.DisplayName, x.StudentId.ToString())).ToListAsync()
            : await db.TrialGuests.AsNoTracking().Where(x => x.Status == "Active" && x.ConvertedStudentId == null).OrderBy(x => x.Name).Select(x => new SelectListItem(x.Name + " · " + x.Phone, x.TrialGuestId.ToString())).ToListAsync();
    }
    private async Task<string> Owner(string type, long id)
    {
        if (type == "Student") return await db.Students.Where(x => x.StudentId == id && x.IsActive).Select(x => x.User.DisplayName).SingleOrDefaultAsync()
            ?? throw new InvalidOperationException("找不到有效的學生。");
        if (type == "TrialGuest") return await db.TrialGuests.Where(x => x.TrialGuestId == id && x.Status == "Active" && x.ConvertedStudentId == null).Select(x => x.Name).SingleOrDefaultAsync()
            ?? throw new InvalidOperationException("找不到有效的體驗生。");
        throw new InvalidOperationException("請選擇有效的購買對象。");
    }
    private async Task<(LessonPlan plan, LessonQuoteModel quote)> Quote(LessonPurchaseModel input)
    {
        Validate(input);
        var owner = await Owner(input.OwnerType, input.OwnerId);
        var plan = await db.LessonPlans.SingleOrDefaultAsync(x => x.LessonPlanId == input.PlanId && x.IsActive && x.AudienceType == input.OwnerType)
            ?? throw new InvalidOperationException("方案已停用或不適用此購買對象。");
        if (input.ValidFromDate < Today) throw new InvalidOperationException("生效日不得早於今天。");
        DateOnly? until;
        try
        {
            until = plan.ValidityUnit switch
            {
                "Days" => input.ValidFromDate.AddDays(plan.ValidityValue!.Value - 1),
                "Months" => input.ValidFromDate.AddMonths(plan.ValidityValue!.Value).AddDays(-1),
                _ => null
            };
        }
        catch (ArgumentOutOfRangeException) { throw new InvalidOperationException("生效日加上效期超出有效日期範圍。"); }
        return (plan, new LessonQuoteModel { OwnerName = owner, PlanName = plan.PlanName, Quota = plan.SessionQuota, Amount = plan.Price, From = input.ValidFromDate, Until = until });
    }
    private record Ticket(Guid Key, long Actor, long PlanId, string PlanVersion, string OwnerType, long OwnerId, string OwnerName, DateOnly From);
    public async Task<LessonQuoteModel> PreviewAsync(LessonPurchaseModel input, long actor)
    {
        await Admin(actor);
        var (plan, quote) = await Quote(input);
        quote.Token = protector.Protect(JsonSerializer.Serialize(new Ticket(Guid.NewGuid(), actor, plan.LessonPlanId, Convert.ToBase64String(plan.RowVersion), input.OwnerType, input.OwnerId, quote.OwnerName, input.ValidFromDate)), TimeSpan.FromMinutes(30));
        return quote;
    }
    public async Task<long> PurchaseAsync(string token, bool confirmDetails, long actor)
    {
        await Admin(actor);
        if (!confirmDetails) throw new InvalidOperationException("請先核對購買對象、金額、堂數及效期，並勾選確認。");
        Ticket ticket;
        try { ticket = JsonSerializer.Deserialize<Ticket>(protector.Unprotect(token)) ?? throw new JsonException(); }
        catch (Exception e) when (e is CryptographicException or JsonException or ArgumentException)
        { throw new InvalidOperationException("購買確認已失效，請重新核對方案。"); }
        if (ticket.Actor != actor) throw new InvalidOperationException("此確認頁不屬於目前的管理員。");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var existing = await db.LessonPasses.Where(x => x.PurchaseKey == ticket.Key).Select(x => (long?)x.LessonPassId).SingleOrDefaultAsync();
        if (existing.HasValue) return existing.Value;
        var (plan, quote) = await Quote(new LessonPurchaseModel { PlanId = ticket.PlanId, OwnerType = ticket.OwnerType, OwnerId = ticket.OwnerId, ValidFromDate = ticket.From });
        if (ticket.PlanVersion != Convert.ToBase64String(plan.RowVersion) || ticket.OwnerName != quote.OwnerName)
            throw new InvalidOperationException("方案或購買對象資料已變更，請重新核對。");
        var pass = new LessonPass { PurchaseKey = ticket.Key, LessonPlanId = plan.LessonPlanId, StudentId = ticket.OwnerType == "Student" ? ticket.OwnerId : null,
            TrialGuestId = ticket.OwnerType == "TrialGuest" ? ticket.OwnerId : null, PlanNameAtPurchase = plan.PlanName, PlanTypeAtPurchase = plan.PlanType,
            SessionQuotaAtPurchase = plan.SessionQuota, AmountAtPurchase = plan.Price, ValidFromDate = quote.From, ValidUntilDate = quote.Until,
            CreatedByUserId = actor, PurchasedAtUtc = UtcNow };
        pass.Payment = new Payment { StudentId = pass.StudentId, TrialGuestId = pass.TrialGuestId, PaymentPurpose = plan.PlanType == "Trial" ? "Trial" : "LessonPass", Amount = pass.AmountAtPurchase, CreatedAtUtc = UtcNow };
        db.LessonPasses.Add(pass); await db.SaveChangesAsync(); await tx.CommitAsync(); return pass.LessonPassId;
    }
    public Task<LessonPass?> PassAsync(long id) => db.LessonPasses.AsNoTracking().Include(x => x.Payment).Include(x => x.TrialGuest)
        .Include(x => x.Student).ThenInclude(x => x!.User).SingleOrDefaultAsync(x => x.LessonPassId == id);
    public async Task ConfirmPaymentAsync(LessonPaymentModel input, long actor)
    {
        await Admin(actor); Validate(input);
        if (!input.ConfirmDetails) throw new InvalidOperationException("請勾選確認已核對收款與方案內容。");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var pass = await db.LessonPasses.Include(x => x.Payment).SingleOrDefaultAsync(x => x.LessonPassId == input.Id)
            ?? throw new InvalidOperationException("找不到購買紀錄。");
        Version(pass.RowVersion, input.RowVersion);
        if (pass.Status != "Pending" || pass.Payment.Status != "Pending") throw new InvalidOperationException("此款項已處理，不能重複收款。");
        await Owner(pass.StudentId.HasValue ? "Student" : "TrialGuest", pass.StudentId ?? pass.TrialGuestId!.Value);
        if (pass.ValidUntilDate < Today) throw new InvalidOperationException("此方案效期已過，請取消待付款訂單後重新購買。");
        if (input.ReceivedAmount != pass.AmountAtPurchase || pass.Payment.Amount != pass.AmountAtPurchase) throw new InvalidOperationException("實收金額必須等於購買時的方案金額。");
        if (input.Method is not ("Cash" or "Transfer" or "LinePay")) throw new InvalidOperationException("請選擇有效的付款方式。");
        if (input.PaidAtTaiwan == default) throw new InvalidOperationException("請填寫實際付款時間。");
        if (input.PaidAtTaiwan > UtcNow.AddHours(8).AddMinutes(1) || input.PaidAtTaiwan < pass.PurchasedAtUtc.AddHours(8).AddMinutes(-1))
            throw new InvalidOperationException("付款時間須介於建立訂單與現在之間。");
        var paid = new DateTimeOffset(DateTime.SpecifyKind(input.PaidAtTaiwan, DateTimeKind.Unspecified), TimeSpan.FromHours(8)).UtcDateTime;
        pass.Payment.Status = "Confirmed"; pass.Payment.PaidAtUtc = paid; pass.Payment.PaymentMethod = input.Method;
        pass.Payment.ConfirmedByUserId = actor; pass.Payment.ConfirmedAtUtc = UtcNow; pass.Payment.Note = input.Note?.Trim();
        pass.Status = "Active";
        await db.SaveChangesAsync(); await tx.CommitAsync();
    }
    public async Task CancelAsync(long id, byte[] version, string? note, long actor)
    {
        await Admin(actor);
        if (string.IsNullOrWhiteSpace(note) || note.Length > 500) throw new InvalidOperationException("請填寫 1 至 500 字的取消原因。");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var pass = await db.LessonPasses.Include(x => x.Payment).SingleOrDefaultAsync(x => x.LessonPassId == id) ?? throw new InvalidOperationException("找不到購買紀錄。");
        Version(pass.RowVersion, version);
        if (pass.Status != "Pending" || pass.Payment.Status != "Pending") throw new InvalidOperationException("只能取消尚未收款的購買紀錄。");
        pass.Status = "Cancelled"; pass.Payment.Status = "Cancelled"; pass.Note = $"管理員 {actor} 於 {UtcNow:yyyy-MM-dd HH:mm:ss} UTC 取消：{note.Trim()}";
        if (pass.Note.Length > 500) throw new InvalidOperationException("取消原因過長，請縮短至 400 字內。");
        await db.SaveChangesAsync(); await tx.CommitAsync();
    }
    public static async Task<PagedRecords<T>> PageAsync<T>(IQueryable<T> query, int page)
    {
        var model = new PagedRecords<T> { TotalCount = await query.CountAsync() };
        model.Page = Math.Clamp(page, 1, model.PageCount);
        model.Items = await query.Skip((model.Page - 1) * 20).Take(20).ToListAsync(); return model;
    }
}
