using EnjoySpace.Web.Areas.Admin.ViewModels;
using EnjoySpace.Web.Data;
using EnjoySpace.Web.Migrations;
using EnjoySpace.Web.Models.Entities;
using EnjoySpace.Web.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Data.SqlClient;

internal static class TrialLessonChecks
{
    public static async Task RunAsync(Func<AppDbContext> createDb, long admin, long student, Action<bool, string> assert)
    {
        var protection = new EphemeralDataProtectionProvider();
        TrialLessonService Service(AppDbContext db) => new(db, protection, TimeProvider.System);
        async Task<T> Read<T>(Func<AppDbContext, Task<T>> action) { await using var db = createDb(); return await action(db); }
        async Task Do(Func<AppDbContext, Task> action) { await using var db = createDb(); await action(db); }
        async Task Reject(Func<Task> action, string text, string label)
        {
            try { await action(); }
            catch (InvalidOperationException e) when (e.Message.Contains(text)) { assert(true, label); return; }
            throw new Exception("FAIL: " + label);
        }
        var guestInput = new TrialGuestEditModel { Name = "體驗測試甲", Phone = "0900000000", Email = "trial@example.test" };
        await Reject(() => Do(db => Service(db).SaveGuestAsync(guestInput, 0)), "管理員", "trial management requires admin");
        var guestId = await Read(db => Service(db).SaveGuestAsync(guestInput, admin));
        // Verify this migration against its own schema in a separate database. Other in-flight
        // features may add new foreign keys to plans without belonging to this migration.
        await using (var source = createDb())
        {
            var connection = new SqlConnectionStringBuilder(source.Database.GetConnectionString())
                { InitialCatalog = "EnjoySpace_TrialMigration_" + Guid.NewGuid().ToString("N") };
            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(connection.ConnectionString).Options;
            await using var schema = new TrialMigrationContext(options);
            var created = await schema.Database.EnsureCreatedAsync();
            if (!created) throw new Exception("Refusing to use an existing migration database.");
            try
            {
                schema.TrialGuests.Add(new TrialGuest { Name = "既有體驗生", Phone = "0900000000", SourceType = "Direct" });
                await schema.SaveChangesAsync(); schema.ChangeTracker.Clear();
                await using (var tx = await schema.Database.BeginTransactionAsync())
                {
                    var generator = schema.GetService<IMigrationsSqlGenerator>();
                    foreach (var command in generator.Generate(new AddTrialPlansAndPurchases().DownOperations)) await schema.Database.ExecuteSqlRawAsync(command.CommandText);
                    foreach (var command in generator.Generate(new AddTrialPlansAndPurchases().UpOperations)) await schema.Database.ExecuteSqlRawAsync(command.CommandText);
                    await tx.CommitAsync();
                }
                assert(await schema.TrialGuests.AnyAsync(x => x.Name == "既有體驗生" && x.RowVersion.Length == 8), "trial migration preserves existing guest and generates version");
            }
            finally { await schema.Database.EnsureDeletedAsync(); }
        }
        var edit = (await Read(db => Service(db).GuestAsync(guestId)))!;
        edit.SourceType = "Referral";
        await Reject(() => Do(db => Service(db).SaveGuestAsync(edit, admin)), "介紹", "referral requires existing student");
        edit.ReferrerStudentId = student;
        await Do(db => Service(db).SaveGuestAsync(edit, admin));
        await Reject(() => Do(db => Service(db).SaveGuestAsync(edit, admin)), "已變更", "stale guest edit rejected");
        var planInput = new LessonPlanEditModel { PlanCode = "TRIAL-CHECK", PlanName = "體驗方案測試", Price = 300, SessionQuota = 1, ValidityValue = 30 };
        var planId = await Read(db => Service(db).SavePlanAsync(planInput, admin));
        await Reject(() => Do(db => Service(db).SavePlanAsync(planInput, admin)), "已存在", "duplicate plan code rejected");
        var invalid = new LessonPlanEditModel { PlanCode = "INVALID", PlanName = "錯誤價格", Price = 1.234m };
        await Reject(() => Do(db => Service(db).SavePlanAsync(invalid, admin)), "兩位", "plan prices cannot silently round");
        invalid.Price = 100; invalid.SessionQuota = 0;
        await Reject(() => Do(db => Service(db).SavePlanAsync(invalid, admin)), "欄位", "zero session quota rejected");
        invalid.SessionQuota = 1; invalid.ValidityValue = null;
        await Reject(() => Do(db => Service(db).SavePlanAsync(invalid, admin)), "天數", "finite plans require validity value");
        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(8));
        var purchase = new LessonPurchaseModel { PlanId = planId, OwnerType = "TrialGuest", OwnerId = guestId, ValidFromDate = today };
        var preview = await Read(db => Service(db).PreviewAsync(purchase, admin));
        assert(preview.Until == today.AddDays(29) && preview.Amount == 300 && preview.Quota == 1, "quote computes inclusive validity and server price");
        await Reject(() => Do(db => Service(db).PurchaseAsync(preview.Token, false, admin)), "勾選", "purchase requires explicit confirmation");
        await Reject(() => Do(db => Service(db).PurchaseAsync("tampered", true, admin)), "失效", "purchase token tampering rejected");
        var expiredToken = protection.CreateProtector("EnjoySpace.LessonPurchase.v1").ToTimeLimitedDataProtector().Protect("{}", TimeSpan.FromMilliseconds(1));
        await Task.Delay(30);
        await Reject(() => Do(db => Service(db).PurchaseAsync(expiredToken, true, admin)), "失效", "expired purchase confirmation rejected");
        var passId = await Read(db => Service(db).PurchaseAsync(preview.Token, true, admin));
        assert(await Read(db => Service(db).PurchaseAsync(preview.Token, true, admin)) == passId, "replayed confirmation returns same purchase");
        assert(await Read(db => db.LessonPasses.CountAsync()) == 1 && await Read(db => db.Payments.CountAsync(x => x.TrialGuestId == guestId)) == 1, "duplicate confirmation cannot duplicate pass or invoice");
        assert(await Read(db => db.LessonPasses.AnyAsync(x => x.LessonPassId == passId && x.Status == "Pending" && x.Payment.Status == "Pending")), "unpaid purchase remains inactive");
        var stalePreview = await Read(db => Service(db).PreviewAsync(purchase, admin));
        var planEdit = (await Read(db => Service(db).PlanAsync(planId)))!;
        planEdit.Price = 450; planEdit.SessionQuota = 2; planEdit.ValidityValue = 60;
        await Do(db => Service(db).SavePlanAsync(planEdit, admin));
        await Reject(() => Do(db => Service(db).SavePlanAsync(planEdit, admin)), "已變更", "stale plan edit rejected");
        await Reject(() => Do(db => Service(db).PurchaseAsync(stalePreview.Token, true, admin)), "已變更", "changed plan invalidates unsubmitted quote");
        var original = (await Read(db => Service(db).PassAsync(passId)))!;
        assert(original.AmountAtPurchase == 300 && original.SessionQuotaAtPurchase == 1 && original.ValidUntilDate == today.AddDays(29), "plan edit cannot rewrite purchased terms");
        LessonPaymentModel Payment(LessonPass p, decimal? amount = null) => new() { Id = p.LessonPassId, RowVersion = p.RowVersion,
            ReceivedAmount = amount ?? p.AmountAtPurchase, Method = "Cash", PaidAtTaiwan = DateTime.UtcNow.AddHours(8), ConfirmDetails = true };
        await Reject(() => Do(db => Service(db).ConfirmPaymentAsync(Payment(original, 450), admin)), "實收", "payment uses original quote rather than current price");
        var noConsent = Payment(original); noConsent.ConfirmDetails = false;
        await Reject(() => Do(db => Service(db).ConfirmPaymentAsync(noConsent, admin)), "勾選", "collection requires confirmation");
        var badTime = Payment(original); badTime.PaidAtTaiwan = new DateTime(1, 1, 1, 1, 0, 0);
        await Reject(() => Do(db => Service(db).ConfirmPaymentAsync(badTime, admin)), "付款時間", "out of range payment date rejected without overflow");
        await Do(db => Service(db).ConfirmPaymentAsync(Payment(original), admin));
        assert(await Read(db => db.LessonPasses.AnyAsync(x => x.LessonPassId == passId && x.Status == "Active" && x.Payment.Status == "Confirmed" && x.Payment.ConfirmedByUserId == admin)), "confirmed payment activates pass atomically");
        await Reject(() => Do(db => Service(db).ConfirmPaymentAsync(Payment(original), admin)), "已變更", "repeated collection cannot overwrite receipt");
        var paid = (await Read(db => Service(db).PassAsync(passId)))!;
        await Reject(() => Do(db => Service(db).CancelAsync(passId, paid.RowVersion, "取消", admin)), "尚未收款", "paid pass cannot use pending order cancellation");
        preview = await Read(db => Service(db).PreviewAsync(purchase, admin));
        var cancelId = await Read(db => Service(db).PurchaseAsync(preview.Token, true, admin));
        var cancel = (await Read(db => Service(db).PassAsync(cancelId)))!;
        await Do(db => Service(db).CancelAsync(cancelId, cancel.RowVersion, "測試取消", admin));
        assert(await Read(db => db.LessonPasses.AnyAsync(x => x.LessonPassId == cancelId && x.Status == "Cancelled" && x.Payment.Status == "Cancelled")), "pending cancellation cancels both pass and receivable");
        await Reject(() => Do(db => Service(db).ConfirmPaymentAsync(Payment(cancel), admin)), "已變更", "cancelled order cannot be collected with stale form");
        purchase.OwnerType = "Student"; purchase.OwnerId = student;
        await Reject(() => Do(db => Service(db).PreviewAsync(purchase, admin)), "不適用", "students cannot buy trial guest plans");
        planInput.PlanCode = "FLEX-CHECK"; planInput.PlanType = "Flexible"; planInput.ValidityUnit = "Unlimited"; planInput.ValidityValue = null;
        var flexId = await Read(db => Service(db).SavePlanAsync(planInput, admin));
        purchase.PlanId = flexId;
        preview = await Read(db => Service(db).PreviewAsync(purchase, admin));
        assert(preview.Until is null, "unlimited plan has no expiry");
        var studentPass = await Read(db => Service(db).PurchaseAsync(preview.Token, true, admin));
        assert(await Read(db => db.LessonPasses.AnyAsync(x => x.LessonPassId == studentPass && x.StudentId == student && x.TrialGuestId == null && x.Payment.StudentId == student)), "student plan assigns correct payer and owner");
        var flexEdit = (await Read(db => Service(db).PlanAsync(flexId)))!; flexEdit.IsActive = false;
        await Do(db => Service(db).SavePlanAsync(flexEdit, admin));
        await Reject(() => Do(db => Service(db).PreviewAsync(purchase, admin)), "停用", "inactive plan cannot be purchased");
        purchase.PlanId = planId; purchase.OwnerType = "TrialGuest"; purchase.OwnerId = guestId;
        purchase.ValidFromDate = today.AddDays(-1);
        await Reject(() => Do(db => Service(db).PreviewAsync(purchase, admin)), "早於", "purchase cannot backdate start");
        purchase.ValidFromDate = DateOnly.MaxValue;
        await Reject(() => Do(db => Service(db).PreviewAsync(purchase, admin)), "日期範圍", "validity overflow rejected");
        purchase.ValidFromDate = today;
        preview = await Read(db => Service(db).PreviewAsync(purchase, admin));
        var expiredPassId = await Read(db => Service(db).PurchaseAsync(preview.Token, true, admin));
        var expiredPass = (await Read(db => Service(db).PassAsync(expiredPassId)))!;
        await Reject(() => Do(db => new TrialLessonService(db, protection, new AttendanceTestClock(DateTimeOffset.UtcNow.AddDays(70))).ConfirmPaymentAsync(Payment(expiredPass), admin)), "效期已過", "expired pending pass cannot activate");
        edit = (await Read(db => Service(db).GuestAsync(guestId)))!; edit.IsActive = false;
        await Do(db => Service(db).SaveGuestAsync(edit, admin));
        await Reject(() => Do(db => Service(db).PreviewAsync(purchase, admin)), "體驗生", "inactive guest cannot purchase");
    }
}

internal sealed class TrialMigrationContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        var included = new AddTrialPlansAndPurchases().TargetModel.GetEntityTypes().Select(x => x.Name).ToHashSet();
        foreach (var entity in builder.Model.GetEntityTypes().ToList())
            if (!included.Contains(entity.Name)) builder.Ignore(entity.Name);
    }
}
