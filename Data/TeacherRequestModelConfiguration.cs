using EnjoySpace.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Data;

internal static class TeacherRequestModelConfiguration
{
    public static void Configure(ModelBuilder builder)
    {
        var e = builder.Entity<TeacherRequest>();
        e.HasIndex(x => x.SubmissionKey).IsUnique();
        e.HasIndex(x => new { x.TeacherId, x.Status });
        e.HasIndex(x => x.CreatedSessionId).IsUnique().HasFilter("[CreatedSessionId] IS NOT NULL");
        e.HasIndex(x => x.TargetSessionId).IsUnique().HasFilter("[Kind] = N'Capacity' AND [Status] = N'Pending'");
        e.HasOne(x => x.Teacher).WithMany().HasForeignKey(x => x.TeacherId).OnDelete(DeleteBehavior.NoAction);
        e.HasOne(x => x.Course).WithMany().HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.NoAction);
        e.HasOne(x => x.StudioLocation).WithMany().HasForeignKey(x => x.StudioLocationId).OnDelete(DeleteBehavior.NoAction);
        e.HasOne(x => x.TargetSession).WithMany().HasForeignKey(x => x.TargetSessionId).OnDelete(DeleteBehavior.NoAction);
        e.HasOne(x => x.CreatedSession).WithMany().HasForeignKey(x => x.CreatedSessionId).OnDelete(DeleteBehavior.NoAction);
        e.HasOne<User>().WithMany().HasForeignKey(x => x.SubmittedByUserId).OnDelete(DeleteBehavior.NoAction);
        e.HasOne<User>().WithMany().HasForeignKey(x => x.ReviewedByUserId).OnDelete(DeleteBehavior.NoAction);
        e.Property(x => x.TargetVersion).HasMaxLength(8);
        e.ToTable(t =>
        {
            t.HasCheckConstraint("CK_TeacherRequests_Kind", "([Kind] = N'ExtraSession' AND [TargetSessionId] IS NULL) OR ([Kind] = N'Capacity' AND [TargetSessionId] IS NOT NULL AND [OriginalCapacity] IS NOT NULL AND [TargetVersion] IS NOT NULL)");
            t.HasCheckConstraint("CK_TeacherRequests_Status", "[Status] IN (N'Pending', N'Approved', N'Rejected')");
            t.HasCheckConstraint("CK_TeacherRequests_Capacity", "[RequestedCapacity] BETWEEN 1 AND 32767");
            t.HasCheckConstraint("CK_TeacherRequests_Time", "[EndsAt] > [StartsAt]");
        });
    }
}
