using EnjoySpace.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
namespace EnjoySpace.Web.Data;
internal static class StudentWorkflowsConfiguration
{
    public static void Configure(ModelBuilder m)
    {
        var qr = m.Entity<SessionQrCode>();
        qr.HasOne(x => x.ClassSession).WithOne().HasForeignKey<SessionQrCode>(x => x.ClassSessionId).OnDelete(DeleteBehavior.NoAction);
        qr.HasIndex(x => x.TokenHash).IsUnique();
        var log = m.Entity<CheckInEvent>();
        log.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
        log.HasOne(x => x.ClassSession).WithMany().HasForeignKey(x => x.ClassSessionId).OnDelete(DeleteBehavior.NoAction);
        log.HasOne<User>().WithMany().HasForeignKey(x => x.ResolvedByUserId).OnDelete(DeleteBehavior.NoAction);
        log.HasIndex(x => new { x.ClassSessionId, x.CreatedAt });
        var b = m.Entity<TrialBooking>();
        b.HasOne(x => x.TrialRequest).WithMany().HasForeignKey(x => x.TrialRequestId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.ClassSession).WithMany().HasForeignKey(x => x.ClassSessionId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne<User>().WithMany().HasForeignKey(x => x.ReviewedByUserId).OnDelete(DeleteBehavior.NoAction);
        b.HasIndex(x => new { x.TrialRequestId, x.ClassSessionId }).IsUnique().HasFilter("[Status] IN (N'Pending',N'Approved')");
        b.HasIndex(x => new { x.TrialRequestId, x.BatchKey, x.ClassSessionId }).IsUnique();
        b.ToTable("TrialBookings", "dbo", t => t.HasCheckConstraint("CK_TrialBookings_Status", "[Status] IN (N'Pending',N'Approved',N'Rejected',N'Cancelled')"));
        m.Entity<ClassSessionParticipant>().HasOne(x => x.TrialBooking).WithOne().HasForeignKey<ClassSessionParticipant>(x => x.TrialBookingId).OnDelete(DeleteBehavior.NoAction);
        m.Entity<ClassSessionParticipant>().HasIndex(x => x.TrialBookingId).IsUnique().HasFilter("[TrialBookingId] IS NOT NULL");
    }
}
