using EnjoySpace.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Data;

internal static class LessonPlanModelConfiguration
{
    public static void Configure(ModelBuilder model)
    {
        var plan = model.Entity<LessonPlan>();
        plan.HasIndex(x => x.PlanCode).IsUnique();
        plan.ToTable("LessonPlans", "dbo", t =>
        {
            t.HasCheckConstraint("CK_LessonPlans_Quota", "[SessionQuota] > 0");
            t.HasCheckConstraint("CK_LessonPlans_Price", "[Price] >= 0");
            t.HasCheckConstraint("CK_LessonPlans_Audience", "([PlanType] = N'Trial' AND [AudienceType] = N'TrialGuest') OR ([PlanType] = N'Flexible' AND [AudienceType] = N'Student')");
            t.HasCheckConstraint("CK_LessonPlans_Validity", "([ValidityUnit] = N'Unlimited' AND [ValidityValue] IS NULL) OR ([ValidityUnit] = N'Days' AND [ValidityValue] IS NOT NULL AND [ValidityValue] > 0)");
        });
        var pass = model.Entity<LessonPass>();
        pass.HasOne(x => x.LessonPlan).WithMany().HasForeignKey(x => x.LessonPlanId).OnDelete(DeleteBehavior.NoAction);
        pass.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.NoAction);
        pass.HasOne(x => x.TrialGuest).WithMany().HasForeignKey(x => x.TrialGuestId).OnDelete(DeleteBehavior.NoAction);
        pass.HasOne(x => x.Payment).WithMany().HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.NoAction);
        pass.HasOne<User>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.NoAction);
        pass.HasIndex(x => x.PurchaseKey).IsUnique();
        pass.HasIndex(x => x.PaymentId).IsUnique();
        pass.ToTable("LessonPasses", "dbo", t =>
        {
            t.HasCheckConstraint("CK_LessonPasses_Owner", "([StudentId] IS NOT NULL AND [TrialGuestId] IS NULL) OR ([StudentId] IS NULL AND [TrialGuestId] IS NOT NULL)");
            t.HasCheckConstraint("CK_LessonPasses_Status", "[Status] IN (N'Pending', N'Active', N'Cancelled')");
            t.HasCheckConstraint("CK_LessonPasses_Quota", "[SessionQuotaAtPurchase] > 0");
            t.HasCheckConstraint("CK_LessonPasses_Amount", "[AmountAtPurchase] >= 0");
            t.HasCheckConstraint("CK_LessonPasses_DateRange", "[ValidUntilDate] IS NULL OR [ValidUntilDate] >= [ValidFromDate]");
        });
    }
}
