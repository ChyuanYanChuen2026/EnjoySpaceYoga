using EnjoySpace.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnjoySpace.Web.Data;

internal static class SessionOperationsModelConfiguration
{
    public static void Configure(ModelBuilder model)
    {
        model.Entity<ClassSession>().Property(x => x.IsCancelled).HasDefaultValue(false);
        model.Entity<ClassSession>().HasOne(x => x.CancelledByUser).WithMany()
            .HasForeignKey(x => x.CancelledByUserId).OnDelete(DeleteBehavior.NoAction);
        var change = model.Entity<SessionTeacherChange>();
        change.HasOne(x => x.ClassSession).WithMany().HasForeignKey(x => x.ClassSessionId).OnDelete(DeleteBehavior.NoAction);
        change.HasOne(x => x.PreviousTeacher).WithMany().HasForeignKey(x => x.PreviousTeacherId).OnDelete(DeleteBehavior.NoAction);
        change.HasOne(x => x.Teacher).WithMany().HasForeignKey(x => x.TeacherId).OnDelete(DeleteBehavior.NoAction);
        change.HasOne(x => x.ChangedByUser).WithMany().HasForeignKey(x => x.ChangedByUserId).OnDelete(DeleteBehavior.NoAction);
    }
}
