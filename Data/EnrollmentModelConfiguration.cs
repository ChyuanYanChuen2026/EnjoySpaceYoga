using EnjoySpace.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Data;

internal static class EnrollmentModelConfiguration
{
    public static void Configure(ModelBuilder model)
    {
        var application = model.Entity<EnrollmentApplication>();
        var adjustment = model.Entity<EnrollmentAmountAdjustment>();
        adjustment.HasOne<EnrollmentApplication>().WithMany().HasForeignKey(x => x.EnrollmentApplicationId).OnDelete(DeleteBehavior.NoAction);
        adjustment.HasOne<User>().WithMany().HasForeignKey(x => x.AdjustedByUserId).OnDelete(DeleteBehavior.NoAction);
        adjustment.ToTable("EnrollmentAmountAdjustments", "dbo", t =>
            t.HasCheckConstraint("CK_EnrollmentAmountAdjustments_Amount", "[Amount] >= 0 AND [Amount] = FLOOR([Amount])"));
        application.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        application.Property(x => x.SubmittedAt).HasColumnName("AppliedAtUtc").HasColumnType("datetime2(0)")
            .HasConversion(x => x.UtcDateTime, x => new DateTimeOffset(DateTime.SpecifyKind(x, DateTimeKind.Utc)));
        application.Property(x => x.ReviewedAt).HasColumnName("ReviewedAtUtc").HasColumnType("datetime2(0)")
            .HasConversion(x => x.HasValue ? x.Value.UtcDateTime : (DateTime?)null,
                x => x.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(x.Value, DateTimeKind.Utc)) : (DateTimeOffset?)null);
        application.HasOne(x => x.Payment).WithMany().HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.NoAction);
        application.HasOne<User>().WithMany().HasForeignKey(x => x.ReviewedByUserId).OnDelete(DeleteBehavior.NoAction);
        application.HasOne<ClassSession>().WithMany().HasForeignKey(x => x.FirstSessionId).OnDelete(DeleteBehavior.NoAction);
        application.HasIndex(x => x.Status);
        application.HasIndex(x => x.PaymentId).IsUnique().HasFilter("[PaymentId] IS NOT NULL");
        application.ToTable("EnrollmentApplications", "dbo", t =>
        {
            t.HasCheckConstraint("CK_EnrollmentApplications_Status", "[Status] IN (N'Pending',N'Approved',N'Rejected',N'Cancelled')");
            t.HasCheckConstraint("CK_EnrollmentApplications_QuotedSessionCount", "[QuotedSessionCount] > 0");
            t.HasCheckConstraint("CK_EnrollmentApplications_QuotedAmount", "[QuotedAmount] >= 0");
        });
        var payment = model.Entity<Payment>();
        payment.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.NoAction);
        payment.HasOne(x => x.TrialGuest).WithMany().HasForeignKey(x => x.TrialGuestId).OnDelete(DeleteBehavior.NoAction);
        payment.HasOne(x => x.ConfirmedByUser).WithMany().HasForeignKey(x => x.ConfirmedByUserId).OnDelete(DeleteBehavior.NoAction);
        payment.HasIndex(x => x.Status);
        payment.Property(x => x.Status).IsConcurrencyToken();
        payment.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
        payment.ToTable("Payments", "dbo", t =>
        {
            t.HasCheckConstraint("CK_Payments_Amount", "[Amount] >= 0");
            t.HasCheckConstraint("CK_Payments_Payer", "([StudentId] IS NOT NULL AND [TrialGuestId] IS NULL) OR ([StudentId] IS NULL AND [TrialGuestId] IS NOT NULL)");
        });
        var guest = model.Entity<TrialGuest>();
        guest.HasOne<Student>().WithMany().HasForeignKey(x => x.ReferrerStudentId).OnDelete(DeleteBehavior.NoAction);
        guest.HasOne<Student>().WithMany().HasForeignKey(x => x.ConvertedStudentId).OnDelete(DeleteBehavior.NoAction);
        guest.HasOne<User>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.NoAction);
        guest.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
        guest.HasIndex(x => x.Phone);
        guest.HasIndex(x => x.Email);
        var enrollment = model.Entity<CourseEnrollment>();
        enrollment.HasOne(x => x.Course).WithMany().HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.NoAction);
        enrollment.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.NoAction);
        enrollment.HasOne(x => x.EnrollmentApplication).WithMany().HasForeignKey(x => x.EnrollmentApplicationId).OnDelete(DeleteBehavior.NoAction);
        enrollment.HasOne<User>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.NoAction);
        enrollment.HasAlternateKey(x => new { x.CourseId, x.StudentId }).HasName("UQ_CourseEnrollments_Course_Student");
        enrollment.HasAlternateKey(x => x.EnrollmentApplicationId).HasName("UQ_CourseEnrollments_Application");
        enrollment.Property(x => x.EnrolledAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
    }
}
