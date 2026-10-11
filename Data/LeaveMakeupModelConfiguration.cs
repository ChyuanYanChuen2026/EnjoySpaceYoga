using EnjoySpace.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Data;

internal static class LeaveMakeupModelConfiguration
{
    public static void Configure(ModelBuilder model)
    {
        var leave = model.Entity<LeaveRequest>();
        leave.HasOne(x => x.Participant).WithMany().HasForeignKey(x => x.SessionParticipantId).OnDelete(DeleteBehavior.NoAction);
        leave.HasOne<User>().WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.NoAction);
        leave.HasOne<User>().WithMany().HasForeignKey(x => x.CancelledByUserId).OnDelete(DeleteBehavior.NoAction);
        leave.HasIndex(x => x.SessionParticipantId).IsUnique().HasFilter("[Status] = N'Active'");
        leave.ToTable("LeaveRequests", "dbo", t => t.HasCheckConstraint("CK_LeaveRequests_Status", "[Status] IN (N'Active', N'Cancelled')"));
        var entitlement = model.Entity<MakeupEntitlement>();
        entitlement.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.NoAction);
        entitlement.HasOne(x => x.SourceTerm).WithMany().HasForeignKey(x => x.SourceTermId).OnDelete(DeleteBehavior.NoAction);
        entitlement.HasOne(x => x.SourceLeave).WithOne(x => x.Entitlement).HasForeignKey<MakeupEntitlement>(x => x.SourceLeaveRequestId).OnDelete(DeleteBehavior.NoAction);
        entitlement.ToTable("MakeupEntitlements", "dbo", t => t.HasCheckConstraint("CK_MakeupEntitlements_Status",
            "[Status] IN (N'Available', N'Reserved', N'Consumed', N'Revoked')"));
        var request = model.Entity<MakeupRequest>();
        request.HasOne(x => x.Entitlement).WithMany().HasForeignKey(x => x.MakeupEntitlementId).OnDelete(DeleteBehavior.NoAction);
        request.HasOne(x => x.TargetSession).WithMany().HasForeignKey(x => x.TargetClassSessionId).OnDelete(DeleteBehavior.NoAction);
        request.HasOne<User>().WithMany().HasForeignKey(x => x.RequestedByUserId).OnDelete(DeleteBehavior.NoAction);
        request.HasOne<User>().WithMany().HasForeignKey(x => x.ReviewedByUserId).OnDelete(DeleteBehavior.NoAction);
        request.HasOne<User>().WithMany().HasForeignKey(x => x.CancelledByUserId).OnDelete(DeleteBehavior.NoAction);
        // Reservation and consumption serialize on the entitlement row; retain status lookup for the invariant check.
        request.HasIndex(x => new { x.MakeupEntitlementId, x.Status });
        request.ToTable("MakeupRequests", "dbo", t => t.HasCheckConstraint("CK_MakeupRequests_Status",
            "[Status] IN (N'Pending', N'Approved', N'Rejected', N'Cancelled')"));
        var participant = model.Entity<ClassSessionParticipant>();
        participant.HasOne(x => x.MakeupRequest).WithOne().HasForeignKey<ClassSessionParticipant>(x => x.MakeupRequestId).OnDelete(DeleteBehavior.NoAction);
        participant.HasIndex(x => x.MakeupRequestId).IsUnique().HasFilter("[MakeupRequestId] IS NOT NULL");
    }
}
